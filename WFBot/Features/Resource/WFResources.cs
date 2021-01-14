using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Features.Resource
{
    public static class WFResources
    {

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void InitWFResource()
        {
            WFTranslateData = GetTranslateApi();
            WFChineseApi = new WFChineseAPI();
            WFAApi = new WFAApi();
            WFTranslator = new WFTranslator();
            Task.WhenAll(
                SetWFContentApi(),
                SetWFCDResources());

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

        public static WFApi WFTranslateData { get; private set; }
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
        public static WFResource<WFCD_All[]> RWFCDAll { get; private set; }

        public static WFContentApi WFContent { get; private set; }

        private static List<string> GetWFOriginUrls()
        {
            // const string source = "http://origin.warframe.com/origin/00000000/PublicExport/index_zh.txt.lzma";
            const string source =
                "https://weathered-lake-14e8.therealkamisama.workers.dev/http://origin.warframe.com/origin/00000000/PublicExport/index_zh.txt.lzma";
            // 似乎是Warframe服务器ban掉了阿里云的IP, 走一层cdn先
            var name = source.Split('/').Last();

            var wc = new WebClient();
            var header = new WebHeaderCollection
            {
                {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0"}
            };
            wc.Headers = header;
            var path = Path.Combine("WFCaches", name);
            var resultpath = Path.Combine("WFCaches", "index_zh.txt");

            wc.DownloadFile(source, path);
            LZMADecompress.DecompressFileLZMA(path, resultpath);

            var result = File.ReadAllLines(resultpath).ToList();
            return result;
        }

        private static Task SetWFContentApi()
        {
            var result = new WFContentApi();
            var urls = GetWFOriginUrls();
            const string source = "http://content.warframe.com/PublicExport/Manifest/";

            var resource = new WFResource<ExportRelicArcane[]>(source + urls.First(u => u.Contains("ExportRelicArcane_zh.json")),resourceLoader: s => s.JsonDeserialize<ExportRelicArcaneZh>().ExportRelicArcane);
            var task = Task.Run(() =>
            {
                resource.Reload(true);
            });

            result.RExportRelicArcanes = resource;

            WFContent = result;
            return task;
        }

        private static Task SetWFCDResources()
        {
            var header = new WebHeaderCollection
            {
                {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0"}
            };

            var resource = new WFResource<WFCD_All[]>("https://api.warframestat.us/items", header: header);
            var task = Task.Run(() =>
            {
                resource.Reload(true);
            });

            RWFCDAll = resource;
            return task;
        }

        private static WFApi GetTranslateApi()
        {
            var api = new WFApi();
            // var source = "https://raw.githubusercontent.com/Richasy/WFA_Lexicon/WFA5/";
            var source = "https://cdn.jsdelivr.net/gh/Richasy/WFA_Lexicon@WFA5/";
            var tasks = new List<Task>();

            AddTask(ref api.RDict, "WF_Dict.json");
            AddTask(ref api.RInvasion, "WF_Invasion.json");
            AddTask(ref api.RSale, "WF_Sale.json");
            AddTask(ref api.RRiven, "WF_Riven.json");
            AddTask(ref api.RAllriven, "WF_AllRiven.json");
            AddTask(ref api.RLib, "WF_Lib.json");
            AddTask(ref api.RNightWave, "WF_NightWave.json");

            Task.WaitAll(tasks.ToArray());

            void AddTask<T>(ref WFResource<T> obj, string name) where T : class
            {
                var path = $"{source}{name}";

                var resource = new WFResource<T>(path, category: nameof(WFTranslator));
                var task = Task.Factory.StartNew(() =>
                {
                    resource.Reload(true);
                }, TaskCreationOptions.LongRunning);

                obj = resource;
                tasks.Add(task);
            }
            //Messenger.SendDebugInfo($"翻译 API 加载完成. 用时 {sw.Elapsed.TotalSeconds:N1}s.");
            // 嘿,如果你在看这个方法怎么用,让2019年3月14日23:59:23的trks来告诉你吧,这个api是本地缓存的api(在本地有的情况下),但是不久后将会被第三方线程操成最新的,我在这里浪费了好久,希望你不会.
            // 呃, 还好我当时写了这局注释 不然我可能之后会拉不出屎憋死 来自2020年2月20日15:34:49的trks
            // 天哪. 这api真是野蛮不堪 我当时为什么要这么写
            // 我的上帝, 我居然需要再次修改这个野蛮的api 简直不敢相信
            return api;

        }

        public static bool UpdateLexion()
        {
            try
            {
                var commit = CommitsGetter.Get("https://api.github.com/repos/Richasy/WFA_Lexicon/commits");
                var sha = commit.First().sha;
                if (sha == Config.Instance.localsha) return true;
                Messenger.SendDebugInfo("发现辞典有更新,正在更新···");
                UpdateTranslateApi();
                Config.Instance.localsha = sha;
                Config.Save();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void UpdateTranslateApi()
        {
            WFTranslateData = GetTranslateApi();
            WFTranslator = new WFTranslator();
        }
    }
}