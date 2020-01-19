using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Channels;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using GammaLibrary.Extensions;
using Settings;

namespace TRKS.WF.QQBot
{
    internal class StringInfo : IComparable<StringInfo>, IComparable
    {
        public string Name { get; set; }
        public int LevDistance { get; set; }

        public int CompareTo(StringInfo other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return LevDistance.CompareTo(other.LevDistance);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is StringInfo other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(StringInfo)}");
        }

        public static bool operator <(StringInfo left, StringInfo right)
        {
            return Comparer<StringInfo>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(StringInfo left, StringInfo right)
        {
            return Comparer<StringInfo>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(StringInfo left, StringInfo right)
        {
            return Comparer<StringInfo>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(StringInfo left, StringInfo right)
        {
            return Comparer<StringInfo>.Default.Compare(left, right) >= 0;
        }
    }
    public static class WFResource
    {
        public static WFApi WFApi { get; private set; } = GetTranslateApi();
        public static WFChineseAPI WFChineseApi { get; } = new WFChineseAPI();
        public static WFTranslator WFTranslator { get; private set; } = new WFTranslator();

        private static WFApi GetTranslateApi()
        {
            var api = new WFApi();
            Messenger.SendDebugInfo("正在加载翻译 API.");
            var source = "https://raw.githubusercontent.com/Richasy/WFA_Lexicon/master/";
            var sw = Stopwatch.StartNew();
            Task.WaitAll(
                Downloader.GetCacheOrDownload<Alert[]>($"{source}WF_Alert.json", alerts => api.Alert = alerts),
                Downloader.GetCacheOrDownload<Dict[]>($"{source}WF_Dict.json", dicts => api.Dict = dicts),
                Downloader.GetCacheOrDownload<Invasion[]>($"{source}WF_Invasion.json", invs => api.Invasion = invs),
                Downloader.GetCacheOrDownload<Sale[]>($"{source}WF_Sale.json", sales => api.Sale = sales),
                Downloader.GetCacheOrDownload<Riven[]>($"{source}WF_Riven.json", rivens => api.Riven = rivens),
                Downloader.GetCacheOrDownload<Relic[]>($"{source}WF_Relic.json", relics => api.Relic = relics),
                Downloader.GetCacheOrDownload<Modifier[]>($"{source}WF_Modifier.json", modifiers => api.Modifier = modifiers),
                Downloader.GetCacheOrDownload<NightWave[]>($"{source}WF_NightWave.json", nightwave => api.NightWave = nightwave)
            );

            Messenger.SendDebugInfo($"翻译 API 加载完成. 用时 {sw.Elapsed.TotalSeconds:N1}s.");
            // 嘿,如果你在看这个方法怎么用,让2019年3月14日23:59:23的trks来告诉你吧,这个api是本地缓存的api(在本地有的情况下),但是不久后将会被第三方线程操成最新的,我在这里浪费了好久,希望你不会.
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
            WFApi = GetTranslateApi();
            WFTranslator = new WFTranslator();
        }
    }

    public class WFChineseAPI
    {
        private WFTranslator translator => WFResource.WFTranslator;
        private string platform => Config.Instance.Platform.GetSymbols().First();

        public List<WFInvasion> GetInvasions()
        {
            var invasions = WebHelper.DownloadJson<List<WFInvasion>>($"https://api.warframestat.us/{platform}/invasions");
            foreach (var invasion in invasions)
            {
                translator.TranslateInvasion(invasion);
                invasion.activation = GetRealTime(invasion.activation);
            }

            return invasions;
        }

        public List<WFAlert> GetAlerts()
        {
            try
            {
                var alerts = WebHelper.DownloadJson<List<WFAlert>>($"https://api.warframestat.us/{platform}/alerts");
                foreach (var alert in alerts)
                {
                    translator.TranslateAlert(alert);
                    alert.Activation = GetRealTime(alert.Activation);
                    alert.Expiry = GetRealTime(alert.Expiry);
                }

                return alerts;
            }
            catch (HttpRequestException)
            {
                // 啥也不做
            }
            catch (WebException)
            {
                // 啥也不做
            }
            catch (Exception e)
            {
                Messenger.SendDebugInfo($"警报获取报错: \r\n" +
                                        $"{e}");

            }
            return new List<WFAlert>();
        }

        public List<Kuva> GetKuvaMissions()
        {
            var kuvas = WebHelper.DownloadJson<List<Kuva>>("https://api.warframestat.us/pc/kuva");
            translator.TranslateKuvaMission(kuvas);
            return kuvas;
        }
        public Arbitration GetArbitrationMission()
        {
            var ar = WebHelper.DownloadJson<Arbitration>("https://api.warframestat.us/pc/arbitration");
            translator.TranslateArbitrationMission(ar);
            return ar;
        }
        public WFNightWave GetNightWave()
        {
            var wave = WebHelper.DownloadJson<WFNightWave>($"https://api.warframestat.us/{platform}/nightwave");
            translator.TranslateNightWave(wave);
            return wave;
        }
        public CetusCycle GetCetusCycle()
        {
            var cycle = WebHelper.DownloadJson<CetusCycle>($"https://api.warframestat.us/{platform}/cetusCycle");
            cycle.Expiry = GetRealTime(cycle.Expiry);
            return cycle;
        }

        public VallisCycle GetVallisCycle()
        {
            var cycle = WebHelper.DownloadJson<VallisCycle>($"https://api.warframestat.us/{platform}/vallisCycle");
            cycle.expiry = GetRealTime(cycle.expiry);
            return cycle;
        }
        public EarthCycle GetEarthCycle()
        {
            var cycle = WebHelper.DownloadJson<EarthCycle>($"https://api.warframestat.us/{platform}/earthCycle");
            cycle.expiry = GetRealTime(cycle.expiry);
            return cycle;
        }



        public Sortie GetSortie()
        {
            var sortie = WebHelper.DownloadJson<Sortie>($"https://api.warframestat.us/{platform}/sortie");
            translator.TranslateSortie(sortie);
            return sortie;
        }

        public List<SyndicateMission> GetSyndicateMissions()
        {
            var missions = WebHelper.DownloadJson<List<SyndicateMission>>($"https://api.warframestat.us/{platform}/syndicateMissions");
            translator.TranslateSyndicateMission(missions);
            return missions;
        }
        public VoidTrader GetVoidTrader()
        {
            var trader = WebHelper.DownloadJson<VoidTrader>($"https://api.warframestat.us/{platform}/voidTrader");
            trader.activation = GetRealTime(trader.activation);
            trader.expiry = GetRealTime(trader.expiry);
            translator.TranslateVoidTrader(trader);
            return trader;
        }

        public List<Fissure> GetFissures()
        {
            var fissures = WebHelper.DownloadJson<List<Fissure>>($"https://api.warframestat.us/{platform}/fissures");
            translator.TranslateFissures(fissures);
            return fissures;
        }

        public List<Event> GetEvents()
        {
            var events = WebHelper.DownloadJson<List<Event>>($"https://api.warframestat.us/{platform}/events");
            translator.TranslateEvents(events);
            foreach (var @event in events)
            {
                @event.expiry = GetRealTime(@event.expiry);
            }

            return events;
        }

        public List<PersistentEnemie> GetPersistentEnemies()
        {
            var enemies = WebHelper.DownloadJson<List<PersistentEnemie>>("https://api.warframestat.us/pc/persistentEnemies");
            translator.TranslatePersistentEnemies(enemies);
            return enemies;
        }

        private static DateTime GetRealTime(DateTime time)
        {
            return time + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        }
    }


    public class WFTranslator
    {
        private ConcurrentDictionary<string /*type*/, Translator> dictTranslators = new ConcurrentDictionary<string, Translator>();
        private ConcurrentDictionary<string, Translator> searchwordTranslator = new ConcurrentDictionary<string, Translator>();
        private Translator wikiTranslator = new Translator();
        private Translator nightwaveTranslator = new Translator();
        private Translator invasionTranslator = new Translator();
        private Translator alertTranslator = new Translator();
        private List<string> weapons = new List<string>();
        private List<string> wikiwords = new List<string>();
        private WFApi translateApi => WFResource.WFApi;


        public WFTranslator()
        {
            InitTranslators();
        }

        private void InitTranslators()
        {
            dictTranslators.Clear();
            dictTranslators.TryAdd("All", new Translator());
            dictTranslators.TryAdd("WM", new Translator());
            foreach (var dict in translateApi.Dict)
            {
                var type = dict.Type;
                if (!dictTranslators.ContainsKey(type))
                {

                    dictTranslators.TryAdd(type, new Translator());
                }
                dictTranslators["All"].AddEntry(dict.En, dict.Zh);
                switch (type)
                {
                    case "Weapon":
                    case "Star":
                        dictTranslators[type].AddEntry(dict.En.Format(), dict.Zh);
                        break;
                    default:
                        dictTranslators[type].AddEntry(dict.En, dict.Zh);
                        break;

                }

            }
            searchwordTranslator.Clear();
            foreach (var sale in translateApi.Sale)
            {
                if (!searchwordTranslator.ContainsKey("Word"))
                {
                    searchwordTranslator.TryAdd("Word", new Translator());
                }

                if (!searchwordTranslator.ContainsKey("Item"))
                {
                    searchwordTranslator.TryAdd("Item", new Translator());
                }
                searchwordTranslator["Word"].AddEntry(sale.Zh.Format(), sale.Search);
                searchwordTranslator["Item"].AddEntry(sale.Search, sale.Zh);
            }
            invasionTranslator.Clear();
            foreach (var invasion in translateApi.Invasion)
            {
                invasionTranslator.AddEntry(invasion.En, invasion.Zh);
            }
            alertTranslator.Clear();
            foreach (var alert in translateApi.Alert)
            {
                alertTranslator.AddEntry(alert.En, alert.Zh);
            }
            weapons.Clear();
            foreach (var riven in translateApi.Riven)
            {
                weapons.Add(riven.Name.Format());
            }

            foreach (var modifier in translateApi.Modifier)
            {
                if (!dictTranslators.ContainsKey("Modifier"))
                {
                    dictTranslators.TryAdd("Modifier", new Translator());
                }
                dictTranslators["Modifier"].AddEntry(modifier.en, modifier.zh);
            }
            nightwaveTranslator.Clear();
            foreach (var wave in translateApi.NightWave)
            {
                nightwaveTranslator.AddEntry(wave.en.Format(), wave.zh);
            }
        }

        public string GetTranslateResult(string str)
        {
            if (str.IsNullOrEmpty())
            {
                return "关键词为空啊.";
            }


            /*var formatedDict = translateApi.Dict.Select(dict => new Dict
                    {En = dict.En.Format(), Id = dict.Id, Type = dict.Type, Zh = dict.Zh}).ToList();*/
            var zhResults = translateApi.Dict.Where(dict => dict.Zh.Format() == str).ToList();
            var enResults = translateApi.Dict.Where(dict => dict.En.Format() == str).ToList();
            if (!zhResults.Any() && !enResults.Any())
            {
                return "并没有查询到任何翻译,请检查源名.";
            }

            var sb = new StringBuilder();

            if (zhResults.Any())
            {
                sb.AppendLine("下面是中文 => 英文的结果:");
                foreach (var zhResult in zhResults)
                {
                    sb.AppendLine($"{zhResult.Zh} |=>| {zhResult.En}");
                }
            }
            else
            {
                sb.AppendLine("下面是 英文 => 中文的结果:");
                foreach (var enResult in enResults)
                {
                    sb.AppendLine($"{enResult.En} |=>| {enResult.Zh}");
                }
            }

            return sb.ToString().Trim();
        }
        public List<string> GetSimilarItem(string word, string mode)
        {
            var lev = new Fastenshtein.Levenshtein(word.Format());
            var distancelist = new SortedSet<StringInfo>();
            var str = word.Substring(0, 1);
            switch (mode)
            {
                case "wm":
                    foreach (var sale in translateApi.Sale)
                    {
                        if (sale.Zh.StartsWith(str))
                        {
                            var distance = lev.DistanceFrom(sale.Zh.Format());
                            distancelist.Add(new StringInfo { LevDistance = distance, Name = sale.Zh });
                        }
                    }
                    break;
                case "rm":
                    foreach (var weapon in weapons)
                    {
                        if (weapon.StartsWith(str))
                        {
                            var distance = lev.DistanceFrom(weapon.Format());
                            distancelist.Add(new StringInfo { LevDistance = distance, Name = weapon });
                        }
                    }
                    break;
                /*                
                case "wiki":
                    foreach (var wiki in wikiwords)
                    {
                        if (wiki.StartsWith(str) || Regex.IsMatch(word, @"[a-z]"))
                        {
                            var distance = lev.DistanceFrom(wiki.Format());
                            distancelist.Add(new StringInfo { LevDistance = distance, Name = wiki });
                        }
                    }                  
                    break;
                    */
                //木大
            }


            return distancelist.Where(dis => dis.LevDistance != 0).Take(5).Select(info => info.Name).ToList();
        }

        public List<Relic> GetRelicInfo(string word)
        {
            return translateApi.Relic.Where(relic => relic.Name.Format().Contains(word)).ToList();
        }
        private static DateTime GetRealTime(DateTime time)
        {
            return time + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        }

        public string TranslateWeapon(string weapon)
        {
            return dictTranslators["Weapon"].Translate(weapon);
        }
        public string TranslateWeaponType(string type)
        {
            return dictTranslators["Word"].Translate(type);
        }
        public void TranslateKuvaMission(List<Kuva> kuvas)
        {
            foreach (var kuva in kuvas)
            {
                kuva.activation = GetRealTime(kuva.activation);
                kuva.expiry = GetRealTime(kuva.expiry);
                kuva.name = TranslateNode(kuva.name);
                // // trick
                // 同↓
                kuva.type = dictTranslators["Mission"].Translate(kuva.type);
            }
        }
        public void TranslateArbitrationMission(Arbitration ar)
        {

                ar.activation = GetRealTime(ar.activation);
                ar.expiry = GetRealTime(ar.expiry);
                ar.node = TranslateNode(ar.node);
                // // trick
                // 不需要trick了
                ar.type = dictTranslators["Mission"].Translate(ar.type);
            
        }
        public void TranslateNightWave(WFNightWave nightwave)
        {
            foreach (var challenge in nightwave.activeChallenges)
            {
                challenge.desc = nightwaveTranslator.Translate(challenge.desc.Format().Replace(",", ""));
                challenge.title = nightwaveTranslator.Translate(challenge.title.Format());
                challenge.expiry = GetRealTime(challenge.expiry);
            }
        }
        public void TranslatePersistentEnemies(List<PersistentEnemie> enemies)
        {
            foreach (var enemy in enemies)
            {
                enemy.agentType = dictTranslators["Word"].Translate(enemy.agentType);
                enemy.lastDiscoveredAt = TranslateNode(enemy.lastDiscoveredAt);
                enemy.lastDiscoveredTime = GetRealTime(enemy.lastDiscoveredTime);
            }
        }
        public void TranslateEvents(List<Event> events)
        {
            foreach (var @event in events)
            {
                @event.description = dictTranslators["All"].Translate(@event.description);
            }
        }

        public string TranslateSearchWord(string source)
        {
            return searchwordTranslator["Word"].Translate(source);
        }

        public void TranslateInvasion(WFInvasion invasion)
        {
            TranslateReward(invasion.attackerReward);
            TranslateReward(invasion.defenderReward);
            invasion.node = TranslateNode(invasion.node);
        }

        private void TranslateReward(RewardInfo reward)
        {
            foreach (var item in reward.countedItems)
            {
                item.type = invasionTranslator.Translate(item.type);
            }

            foreach (var t in reward.countedItems)
            {
                t.type = alertTranslator.Translate(t.type);
            }
        }

        private string TranslateNode(string node)
        {
            var result = "";
            if (!node.IsNullOrEmpty())
            {
                var strings = node.Split('(');
                if (strings.Length >= 2)
                {
                    var nodeRegion = strings[1].Split(')')[0];
                    result = strings[0] + dictTranslators["Star"].Translate(nodeRegion.Format());
                }
                else
                {
                    return dictTranslators["Star"].Translate(node.Format());
                }

            }

            return result;
        }

        public bool ContainsWeapon(string weapon)
        {
            return weapons.Contains(weapon);
        }

        public bool ContainsWikiword(string word)
        {
            return wikiwords.Contains(word);
        }

        public void TranslateAlert(WFAlert alert)
        {
            var mission = alert.Mission;
            mission.Node = TranslateNode(mission.Node);
            mission.Type = dictTranslators["Mission"].Translate(mission.Type);
            TranslateReward(mission.Reward);

            void TranslateReward(Reward reward)
            {
                foreach (var item in reward.CountedItems)
                {
                    item.Type = alertTranslator.Translate(item.Type);
                }

                for (var i = 0; i < reward.Items.Length; i++)
                {
                    reward.Items[i] = alertTranslator.Translate(reward.Items[i]);
                }
            }

        }

        public void TranslateSyndicateMission(List<SyndicateMission> missions)
        {
            foreach (var mission in missions)
            {
                if (mission.jobs.Length == 0)
                {
                    if (mission.nodes.Length != 0)
                    {
                        for (var i = 0; i < mission.nodes.Length; i++)
                        {
                            mission.nodes[i] = TranslateNode(mission.nodes[i]);
                        }
                    }
                }
                else
                {
                    if (mission.syndicate == "Ostrons" || mission.syndicate == "Solaris United")
                    {
                        foreach (var job in mission.jobs)
                        {
                            for (int i = 0; i < job.rewardPool.Length; i++)
                            {
                                var reward = job.rewardPool[i];
                                var item = reward;
                                var count = "";
                                if (!reward.Contains("Relic"))
                                {
                                    item = Regex.Replace(reward, @"\d", "").Replace("X", "").Replace(",", "").Replace("BP", "Blueprint").Trim();
                                    count = Regex.Replace(reward, @"[^\d]*", "");
                                }
                                else
                                {
                                    item = item.Replace("Relic", "").Replace("Lith", "古纪").Replace("Meso", "前纪").Replace("Neo", "中纪").Replace("Axi", "后纪");// 这是暴力写法 我懒了 真的
                                }

                                var sb = new StringBuilder();
                                if (count.Length != 0)
                                {
                                    sb.Append($"{count}X");
                                }

                                sb.Append(dictTranslators["All"].Translate(item));
                                job.rewardPool[i] = Regex.Replace(sb.ToString(), "(\\d+)(X)", "$1x");
                            }
                        }
                    }
                }
            }
        }

        public void TranslateSortie(Sortie sortie)
        {
            foreach (var variant in sortie.variants)
            {
                variant.node = TranslateNode(variant.node).Replace("Plains of Eidolon", "夜灵平野"); // 这个不在翻译api里
                variant.missionType = dictTranslators["Mission"].Translate(variant.missionType);
                variant.modifier = dictTranslators["Modifier"].Translate(variant.modifier);
            }
            sortie.boss = dictTranslators["Word"].Translate(sortie.boss);
        }

        public void TranslateFissures(List<Fissure> fissures)
        {
            fissures = fissures.OrderBy(fissure => fissure.tierNum).ToList();
            foreach (var fissure in fissures)
            {
                fissure.node = TranslateNode(fissure.node);
                fissure.tier = dictTranslators["Word"].Translate(fissure.tier);
                fissure.missionType = dictTranslators["Mission"].Translate(fissure.missionType);
                fissure.expiry = GetRealTime(fissure.expiry);
                var delay = fissure.expiry - DateTime.Now;
                if (delay.Ticks > 0)
                {
                    Task.Delay(delay).ContinueWith(a => fissure.active = false);
                }
            }
        }


        public void TranslateVoidTrader(VoidTrader trader)
        {
            trader.location = TranslateNode(trader.location).Replace("Relay", "中继站");
            foreach (var inventory in trader.inventory)
            {
                inventory.item = dictTranslators["All"].Translate(inventory.item);
            }
            // ohhhhhhhhhhhhhhhhhhhhhhh奸商第一百次来带的东西真他妈劲爆啊啊啊啊啊啊啊啊啊啊啊 啊啊啊啊啊啊啊啊啊啊之后还带了活动电可我没囤多少呜呜呜呜呜呜穷了 哈哈哈哈哈哈老子开出一张绝路啊啊啊啊啊啊爽死了 呜呜呜呜电男loki出库我没刷我穷死了
        }

        public void TranslateWMOrder(WMInfo info, string searchword)
        {
            foreach (var iteminset in info.include.item.items_in_set.Where(word => word.url_name == searchword))
            {
                iteminset.zh.item_name = searchwordTranslator["Item"].Translate(searchword);
            }

            foreach (var order in info.payload.orders)
            {
                switch (order.order_type)
                {
                    case "buy":
                        order.order_type = "收购";
                        break;
                    case "sell":
                        order.order_type = "出售";
                        break;
                }

                switch (order.user.status)
                {
                    case "ingame":
                        order.user.status = "游戏内在线";
                        break;
                    case "online":
                        order.user.status = "WM在线";
                        break;
                    case "offline":
                        order.user.status = "离线";
                        break;
                }
            }

        }

        public void TranslateWMOrderEx(WMInfoEx info, string searchword)
        {
            info.info.enName = info.orders.First().itemName;
            info.info.zhName = searchwordTranslator["Item"].Translate(searchword);

            foreach (var order in info.orders)
            {
                switch (order.order_Type)
                {
                    case "buy":
                        order.order_Type = "收购";
                        break;
                    case "sell":
                        order.order_Type = "出售";
                        break;
                }

                switch (order.status)
                {
                    case "ingame":
                        order.status = "游戏内在线";
                        break;
                    case "online":
                        order.status = "WM在线";
                        break;
                    case "offline":
                        order.status = "离线";
                        break;
                }
            }

        }


    }

    /*
    public class ObjectType
    {
        public string Type;

        public ObjectType(string type)
        {
            Type = type;
        }
    }
    */
    public class Translator
    {
        private readonly Dictionary<string, string> dic;

        public Translator()
        {
            dic = new Dictionary<string, string>();
        }

        public Translator(Dictionary<string, string> dictionary)
        {
            dic = dictionary;
        }

        public string Translate(string source)
        {
            return dic.ContainsKey(source) ? dic[source] : source;
        }

        public void AddEntry(string source, string target)
        {
            dic[source] = target;
        }

        public void Clear()
        {
            dic.Clear();
        }
    }
}
