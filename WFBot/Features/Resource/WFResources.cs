using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Mono.Cecil.Cil;
using Newtonsoft.Json;
using WFBot.Features.Common;
using WFBot.Features.Utils;
using WFBot.Utils;
using JsonSerializer = System.Text.Json.JsonSerializer;

// ReSharper disable LocalizableElement

namespace WFBot.Features.Resource
{
    public static class WFResources
    {
        internal static bool ResourceLoadFailed = false;

        private static bool IsKraber = false;
        // 这里其实应该加锁
        internal static async Task InitWFResource()
        {
            if (WebHelper.TryGet("https://wfbot.kraber.top:8888/Resources/").Result.IsOnline)
            {
                IsKraber = true;
                Trace.WriteLine("已核实CDN链接正常, WFBot将会使用自建CDN下载资源.");
            }
            WFChineseApi = new WFChineseAPI();
            ThreadPool.SetMinThreads(64, 64);

            await Task.WhenAll(
                Task.Run(SetWFCDResources),
                Task.Run(SetWFContentApi),
                Task.Run(() => { WFAApi = new WFAApi(); }),
                Task.Run(async () => WMAuction = await GetWMAResources()),
                Task.Run(async () => WFTranslateData = await GetTranslateApi()),
                Task.Run(async () => WFBotTranslateData = await GetWFBotTranslateApi()),
                Task.Run(async () => RWildcardAndSlang = await GetWildcardAndSlang()),
                Task.Run(async () => WildCardSearcher = await WildCardSearcher.Create())
            );
            WFTranslator = new WFTranslator();
            Weaponinfos = GetWeaponInfos();
            
            if (ResourceLoadFailed) 
                throw new Exception("WFBot 资源初始化失败, 请查看上面的 log.");
            /*
            catch (Exception e)
            {
                Messenger.SendDebugInfo($"初始化出现问题, WFBot 无法运行: {e}");
                try
                {
                    Directory.Delete("WFCaches", true);
                }
                catch (Exception)
                {
                }
                MessageBox.Show($"初始化出现问题, WFBot 无法运行, 已经删除文件缓存, 请再试一次, 还是不行请检查 WFBotLogs: {e}", "WFBot", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            */
        }

        public static WFApi WFTranslateData { get; set; }
        public static WFBotApi WFBotTranslateData { get; set; }
        public static WFChineseAPI WFChineseApi { get; private set; }

        static WFTranslator _wfTranslator;
        static readonly object translatorLock = new object();
        static volatile int translatorVersion;
        public static WFTranslator WFTranslator
        {
            get
            {
                lock (translatorLock)
                {
                    if (WFResourceStatic.CategoryVersionDictionary[nameof(WFTranslator)] != translatorVersion)
                    {
                        Trace.WriteLine($"翻译器数据变动, 正在刷新翻译器..");
                        WFTranslator = new WFTranslator();
                        return WFTranslator;
                    }

                    return _wfTranslator;
                }
            }
            private set
            {
                lock (translatorLock)
                {
                    translatorVersion = WFResourceStatic.CategoryVersionDictionary[nameof(WFTranslator)];
                    _wfTranslator = value;
                }
            }
        }

        public static WFAApi WFAApi { get; private set; }

        public static WFCD_All[] WFCDAll => RWFCDAll.Value;
        public static WMAuction WMAuction { get; private set; }
        public static WFResource<WFCD_All[]> RWFCDAll { get; private set; }

        public static WFContentApi WFContent { get; private set; }

        public static WeaponInfo[] Weaponinfos { get; private set; }
        public static WildcardAndSlang WildcardAndSlang => RWildcardAndSlang?.Value;
        public static WFResource<WildcardAndSlang> RWildcardAndSlang { get; private set; }
        public static WildCardSearcher WildCardSearcher { get; private set; }
        private static WeaponInfo[] GetWeaponInfos()
        {
            var result = new List<WeaponInfo>();
            foreach (var urlname in WMAuction.ENRivens.Select(r => r.UrlName))
            {
                var zhriven = WMAuction.ZHRivens.First(r => r.UrlName == urlname);
                var enriven = WMAuction.ENRivens.First(r => r.UrlName == urlname);
                result.Add(new WeaponInfo { enname = enriven.ItemName, urlname = urlname, zhname = zhriven.ItemName });
            }

            return result.ToArray();
        }
        private static async Task<List<string>> GetWFOriginUrls(bool refresh = false)
        {
            const string source = "http://origin.warframe.com/origin/00000000/PublicExport/index_zh.txt.lzma";
            const string cdn =
                "http://wfbot.cdn.kraber.top/http://origin.warframe.com/origin/00000000/PublicExport/index_zh.txt.lzma";
            const string cycdn =
                "https://wfbot.cyan.cafe/api/WFOrigin"; // hk server

            // 似乎是Warframe服务器ban掉了阿里云的IP, 走一层cdn先
            var name = source.Split('/').Last();    
            var path = Path.Combine("WFCaches", name);
            var resultpath = Path.Combine("WFCaches", "index_zh.txt");
            List<string> result;
            if (File.Exists(resultpath) && !refresh)
            {
                result = (await File.ReadAllLinesAsync(resultpath)).ToList();
                return result;
            }
            var hc = new HttpClient();
            hc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");

            var response = await hc.GetAsync(source);
            if (!response.IsSuccessStatusCode)
            {
                response = await hc.GetAsync(cdn);
            }
            if (!response.IsSuccessStatusCode)
            {
                response = await hc.GetAsync(cycdn);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("从 CDN 获取 WFOrigin 均失败.");
            }
            
            // File.OpenWrite 的话第一次写入的如果比第二次长 copy过去就会导致有无效数据
            // 覆盖文件不能这么写
            await using (var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(stream);
            }
            LZMADecompress.DecompressFileLZMA(path, resultpath);

            result = (await File.ReadAllLinesAsync(resultpath)).ToList();
            return result;
        }

        private static async Task SetWFContentApi()
        {
            var result = new WFContentApi();
            const string source = "http://content.warframe.com/PublicExport/Manifest/";

            var resource = WFResource<ExportRelicArcane[]>.Create(
                resourceLoader: async s =>
                {
                    var r = await ResourceLoaders<ExportRelicArcaneZh>.JsonDotNetLoader(s);
                    return r.ExportRelicArcane;
                },
                fileName: "ExportRelicArcane_zh.json",
                requester: async _ =>
                {
                    //var count = 0;
                    List<string> urls;
                    try
                    {
                        urls = await GetWFOriginUrls();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("WFOriginUrls 获取失败, 下面是异常:");
                        Trace.WriteLine(e);
                        throw;
                    }
                    
                    var link = source + urls.First(u => u.Contains("ExportRelicArcane_zh.json"));
                    var hc = new HttpClient(new RetryHandler(new HttpClientHandler()));
                    var response = await hc.GetAsync(link);
                    if (!response.IsSuccessStatusCode)
                    {
                        urls = await GetWFOriginUrls(true);
                        response = await hc.GetAsync(link);
                    }

                    return await response.Content.ReadAsStreamAsync();

                });

            result.RExportRelicArcanes = resource;

            WFContent = result;
            await resource.WaitForInited();
        }
        public static string GetSHA(GitHubInfo info)
        {
            if (info.IsKraber)
            {
                return WebHelper.DownloadStringAsync(info.Kraber).Result;
            }
            var commits = CommitsGetter.Get($"https://api.github.com/repos/{info.Name}/commits");
            return commits?.FirstOrDefault()?.sha;
        }
        private static async Task<WMAuction> GetWMAResources()
        {
            var Auction = new WMAuction();
            var tasks = new List<Task>();
            var ZHHeader = new WebHeaderCollection
            {
                {"Language", "zh-hans"}
            };
            var ENHeader = new WebHeaderCollection
            {
                {"Language", "en"}
            };
            const string source = "https://api.warframe.market/v1";
            AddTask(ref Auction.RAttributes, $"{source}/riven/attributes", "WMA_Attributes.json", ZHHeader);
            AddTask(ref Auction.ZHRRivens, $"{source}/riven/items", "WMA_Rivens_Zh.json", ZHHeader);
            AddTask(ref Auction.ENRRivens, $"{source}/riven/items", "WMA_Rivens.En.json", ENHeader);
            // AddTask(ref Auction.RItems, $"{source}/items", "WMA_Items.json", ZHHeader);
            await Task.WhenAll(tasks.ToArray());
            //// 这几个网络资源每次请求时返回的String都不太一样但是实际上内容一样, 所以用JsonStringCompareUpdater
            // 我没时间写了, 就先不实现比较后刷新了
            void AddTask<T>(ref WFResource<T> obj, string url, string name, WebHeaderCollection header = default) where T : class
            {
                var resource = WFResource<T>.Create(url, fileName: name, category: nameof(WMASearcher), header: header, updater: WFResourceUpdaters<T>.JustUpdateDelegate);
                obj = resource;
                tasks.Add(resource.WaitForInited());
            }

            return Auction;
        }

        private static Task SetWFCDResources()
        {
            var header = new WebHeaderCollection
            {
                //{"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0"},
                {"Accept-Encoding", "br"}
            };

            var resource = WFResource<WFCD_All[]>.Create(url: "https://wfcd-all.kraber.top", nameof(WFCD_All),
                "All.json", 
                header, 
                ResourceLoaders<WFCD_All[]>.JsonDotNetLoader,
                null,
                WFResourceUpdaters<WFCD_All[]>.GitHubShaDelegate);
            if (WFResourcesManager.WFResourceGitHubInfos.All(i => i.Category != nameof(WFCD_All)))
            {
                var name = "WFCD/warframe-items";

                WFResourcesManager.AddInfo(name, "", nameof(WFCD_All), false);
            }


            RWFCDAll = resource;
            return resource.WaitForInited();
        }
        private static async Task<WFApi> GetTranslateApi()
        {
            var api = new WFApi();
            // var source = "https://raw.githubusercontent.com/Richasy/WFA_Lexicon/WFA5/";
            var source = IsKraber ? "https://wfbot.kraber.top:8888/Resources/Richasy/WFA_Lexicon@WFA5/" : "https://cdn.jsdelivr.net/gh/Richasy/WFA_Lexicon@WFA5/";
            var tasks = new List<Task>();

            AddTask(ref api.RDict, "WF_Dict.json");
            AddTask(ref api.RInvasion, "WF_Invasion.json");
            AddTask(ref api.RSale, "WF_Sale.json");
            AddTask(ref api.RRiven, "WF_Riven.json");
            AddTask(ref api.RAllriven, "WF_AllRiven.json");
            AddTask(ref api.RLib, "WF_Lib.json");
            AddTask(ref api.RNightWave, "WF_NightWave.json");
            if (WFResourcesManager.WFResourceGitHubInfos.All(i => i.Category != nameof(WFTranslator)))
            {
                var name = "Richasy/WFA_Lexicon";

                WFResourcesManager.AddInfo(name, "https://wfbot.kraber.top:8888/Resources/Richasy/WFA_Lexicon@WFA5/sha", nameof(WFTranslator), IsKraber);
            }
            await Task.WhenAll(tasks.ToArray());

            void AddTask<T>(ref WFResource<T> obj, string name) where T : class
            {
                var path = $"{source}{name}";
                var resource = WFResource<T>.Create(path, category: nameof(WFTranslator), null, null, null, null, WFResourceUpdaters<T>.GitHubShaDelegate);
                obj = resource;
                tasks.Add(resource.WaitForInited());
            }
            //Messenger.SendDebugInfo($"翻译 API 加载完成. 用时 {sw.Elapsed.TotalSeconds:N1}s.");
            // 嘿,如果你在看这个方法怎么用,让2019年3月14日23:59:23的trks来告诉你吧,这个api是本地缓存的api(在本地有的情况下),但是不久后将会被第三方线程操成最新的,我在这里浪费了好久,希望你不会.
            // 呃, 还好我当时写了这局注释 不然我可能之后会拉不出屎憋死 来自2020年2月20日15:34:49的trks
            // 天哪. 这api真是野蛮不堪 我当时为什么要这么写
            // 我的上帝, 我居然需要再次修改这个野蛮的api 简直不敢相信
            // 已经不会这么干了
            return api;

        }

        private static async Task<WFBotApi> GetWFBotTranslateApi()
        {
            var api = new WFBotApi();
            var tasks = new List<Task>();
            var source = IsKraber? "https://wfbot.kraber.top:8888/Resources/TRKS-Team/WFBot_Lexicon@master/" : "https://cdn.jsdelivr.net/gh/TRKS-Team/WFBot_Lexicon@master/";

            AddTask(ref api.RSale, "WFBot_Sale.json");

            if (WFResourcesManager.WFResourceGitHubInfos.All(i => i.Category != nameof(WFTranslator)))
            {
                var name = "TRKS-Team/WFBot_Lexicon";

                WFResourcesManager.AddInfo(name, "https://wfbot.kraber.top:8888/Resources/TRKS-Team/WFBot_Lexicon@master/sha", nameof(WFBotApi), IsKraber);
            }

            await Task.WhenAll(tasks.ToArray());

            void AddTask<T>(ref WFResource<T> obj, string name) where T : class
            {
                var path = $"{source}{name}";
                var resource = WFResource<T>.Create(path, category: nameof(WFBotApi), null, null, null, null, WFResourceUpdaters<T>.GitHubShaDelegate, WFResourceFinishers.UpdateTranslatorAndWildcardSearcher);
                obj = resource;
                tasks.Add(resource.WaitForInited());
            }

            return api;
        }
        private static async Task<WFResource<WildcardAndSlang>> GetWildcardAndSlang()
        {
            var resource =
                WFResource<WildcardAndSlang>.Create(
                     IsKraber?"https://wfbot.kraber.top:8888/Resources/TRKS-Team/WFBotSlang@latest/WF_Sale_Wildcard.json":"https://cdn.jsdelivr.net/gh/TRKS-Team/WFBotSlang@latest/WF_Sale_Wildcard.json",
                    nameof(WildcardAndSlang),
                    "WF_Sale_Wildcard.json",
                    resourceLoader: ResourceLoaders<WildcardAndSlang>.JsonDotNetLoader,
                    updater: WFResourceUpdaters<WildcardAndSlang>.GitHubShaDelegate,
                    finisher: WFResourceFinishers.UpdateTranslatorAndWildcardSearcher);
            if (WFResourcesManager.WFResourceGitHubInfos.All(i => i.Category != nameof(WildcardAndSlang)))
            {
                var name = "TRKS-Team/WFBotSlang";

                WFResourcesManager.AddInfo(name, "https://wfbot.kraber.top:8888/Resources/TRKS-Team/WFBotSlang@latest/sha", nameof(WildcardAndSlang), IsKraber);
            }

            await resource.WaitForInited();
            return resource;
        }
        private static async Task<bool> Updater(WFResource<WildcardAndSlang> resource)
        {
            try
            {
                // I already fixed it, and you are not coming back.

                var infos = GitHubInfos.Instance.Infos.Where(i => i.Category == resource.Category).ToList();

                if (!infos.Any()) return false;
                var info = infos.First();
                if (DateTime.Now - info.LastUpdated <= TimeSpan.FromMinutes(10)) return false;
                // 关于API的限制 有Token的话5000次/hr 无Token的话60次/hr 咱就不狠狠的造GitHub的服务器了
                var sha = GetSHA(info);
                if (sha == null) return false;
                if (info.SHA.IsNullOrEmpty())
                {
                    info.SHA = sha;
                    GitHubInfos.Save();
                    return false;
                }
                if (sha == info.SHA) return false;
                Messenger.SendDebugInfo($"发现{info.Category}有更新,正在更新···");
                await Task.WhenAll(WFResourcesManager.WFResourceDic[info.Category].Select(r => r.Reload(false)));
                WildCardSearcher.UpdateSearcher(); /*不用刷新翻译器*/

                GitHubInfos.Instance.Infos.Where(i => i.Category == info.Category).ForEach(i =>
                {
                    i.LastUpdated = DateTime.Now;
                    i.SHA = sha;
                });
                GitHubInfos.Save();

                return true;
            }
            catch (Exception)
            {
                Trace.WriteLine("用于刷新资源文件的GitHub Commits获取失败, 这可能和网络有关系, 可以尝试阅读https://github.com/TRKS-Team/WFBot/blob/universal/docs/token.md");
                return false;
            }
        }

        public static void UpdateWFTranslator()
        {
            Trace.WriteLine("正在刷新翻译器...");
            WFTranslator = new WFTranslator();
        }
        /*public static async Task<bool> UpdateLexion()
        {
            try
            {
                var commit = CommitsGetter.Get("https://api.github.com/repos/Richasy/WFA_Lexicon/commits");
                var sha = commit.First().sha;
                if (sha == Config.Instance.localsha) return true;
                Messenger.SendDebugInfo("发现辞典有更新,正在更新···");
                await UpdateTranslateApi();
                Config.Instance.localsha = sha;
                Config.Save();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // todo 这里需要改
        private static async Task UpdateTranslateApi()
        {
            WFTranslateData = await GetTranslateApi();
            WFTranslator = new WFTranslator();
        }*/

    }
}