using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using GammaLibrary.Extensions;
using WFBot.Events;
using WFBot.Features.Common;
using WFBot.Features.Other;
using WFBot.Features.Resource;
using WFBot.Utils;
using WFBot.Windows;

namespace WFBot.Features.Utils
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

    public class WFChineseAPI
    {
        private WFTranslator translator => WFResources.WFTranslator;
        private WFCD_All[] all => WFResources.WFCDAll;
        private WFContentApi content => WFResources.WFContent;

        private static string platform => Config.Instance.Platform.GetSymbols().First();
        private static readonly string WFstat = $"https://api.warframestat.us/{platform}";

        public async Task<List<WFInvasion>> GetInvasions()
        {
            var invasions = await WebHelper.DownloadJsonAsync<List<WFInvasion>>(WFstat + "/invasions");
            foreach (var invasion in invasions)
            {
                translator.TranslateInvasion(invasion);
                invasion.activation = GetRealTime(invasion.activation);
            }

            return invasions;
        }

        public async Task<List<WFAlert>> GetAlerts()
        {
            try
            {
                var alerts = await WebHelper.DownloadJsonAsync<List<WFAlert>>(WFstat + "/alerts");
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
        public async Task<List<Kuva>> GetKuvaMissions()
        {
            var kuvas = await WebHelper.DownloadJsonAsync<List<Kuva>>(WFstat + "/kuva");
            translator.TranslateKuvaMission(kuvas);
            return kuvas;
        }

        public async Task<Arbitration> GetArbitrationMission()
        {
            var ar = await WebHelper.DownloadJsonAsync<Arbitration>(WFstat + "/arbitration");
            translator.TranslateArbitrationMission(ar);
            return ar;
        }
        public async Task<WFNightWave> GetNightWave()
        {
            var wave = await WebHelper.DownloadJsonAsync<WFNightWave>(WFstat + "/nightwave");
            translator.TranslateNightWave(wave);
            return wave;
        }
        public async Task<CetusCycle> GetCetusCycle()
        {
            var cycle = await WebHelper.DownloadJsonAsync<CetusCycle>(WFstat + "/cetusCycle");
            cycle.Expiry = GetRealTime(cycle.Expiry);
            return cycle;
        }

        public async Task<VallisCycle> GetVallisCycle()
        {
            var cycle = await WebHelper.DownloadJsonAsync<VallisCycle>(WFstat + "/vallisCycle");
            cycle.expiry = GetRealTime(cycle.expiry);
            return cycle;
        }
        public async Task<EarthCycle> GetEarthCycle()
        {
            var cycle = await WebHelper.DownloadJsonAsync<EarthCycle>(WFstat + "/earthCycle");
            cycle.expiry = GetRealTime(cycle.expiry);
            return cycle;
        }

        public async Task<CambionCycle> GetCambionCycle()
        {
            var cycle = await WebHelper.DownloadJsonAsync<CambionCycle>(WFstat + "/cambionCycle");
            cycle.expiry = GetRealTime(cycle.expiry);
            return cycle;
        }


        public async Task<Sortie> GetSortie()
        {
            var sortie = await WebHelper.DownloadJsonAsync<Sortie>(WFstat + "/sortie");
            translator.TranslateSortie(sortie);
            return sortie;
        }

        public async Task<List<SyndicateMission>> GetSyndicateMissions()
        {
            var missions = await WebHelper.DownloadJsonAsync<List<SyndicateMission>>(WFstat + "/syndicateMissions");
            translator.TranslateSyndicateMission(missions);
            return missions;
        }
        public async Task<VoidTrader> GetVoidTrader()
        {
            var trader = await WebHelper.DownloadJsonAsync<VoidTrader>(WFstat + "/voidTrader");
            trader.activation = GetRealTime(trader.activation);
            trader.expiry = GetRealTime(trader.expiry);
            translator.TranslateVoidTrader(trader);
            return trader;
        }

        public async Task<List<Fissure>> GetFissures()
        {
            var fissures = await WebHelper.DownloadJsonAsync<List<Fissure>>(WFstat + "/fissures");
            translator.TranslateFissures(fissures);
            return fissures;
        }

        public async Task<List<Event>> GetEvents()
        {
            var events = await WebHelper.DownloadJsonAsync<List<Event>>(WFstat + "/events");
            translator.TranslateEvents(events);
            foreach (var @event in events)
            {
                @event.expiry = GetRealTime(@event.expiry);
            }

            return events;
        }

        public async Task<List<PersistentEnemie>> GetPersistentEnemies()
        {
            var enemies = await WebHelper.DownloadJsonAsync<List<PersistentEnemie>>(WFstat + "/persistentEnemies");
            translator.TranslatePersistentEnemies(enemies);
            return enemies;
        }

        public async Task<SentientOutpost> GetSentientOutpost()
        {
            var outpost = await WebHelper.DownloadJsonAsync<SentientOutpost>(WFstat + "/sentientOutposts");
            translator.TranslateSentientOutpost(outpost);
            return outpost;
        }

        public async Task<SentientAnomaly> GetSentientAnomaly()
        {
            var raw = await WebHelper.DownloadJsonAsync<RawSentientAnomaly>("https://semlar.com/anomaly.json");
            return translator.TranslateSentientAnomaly(raw);
        }

        public List<ExportRelicArcane> GetRelics(string word)
        {
            var items = content.ExportRelicArcanes.Where(ra =>
                ra.Name.Replace("遗物", "").Format().Contains(word.Format()));
            var result = new List<ExportRelicArcane>();
            foreach (var item in items)
            {
                if (result.All(r => r.Name != item.Name))
                {
                    result.Add(item);
                }
            }
            return result;

        }


        private static DateTime GetRealTime(DateTime time)
        {
            return time + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        }

    }


    public class WFTranslator
    {
        private Translator dictTranslator = new Translator(); // 翻译器之祖
        private Translator searchwordTranslator = new Translator(); // 将中文物品转换成wm的搜索地址
        private Translator wikiTranslator = new Translator(); // 我忘了是干嘛的 好像没用了
        private Translator nightwaveTranslator = new Translator(); // 午夜电波任务翻译
        private Translator relicrewardTranslator = new Translator();
        private Translator invasionTranslator = new Translator(); // 入侵物品翻译
        private List<Riven> weaponslist = new List<Riven>(); // 武器的list 用来wfa搜索
        // She is known as riven, riven of a thousand voice, the last known ahamkara.
        private List<string> weapons = new List<string>();// 所有武器的中文
        private WFApi translateApi => WFResources.WFTranslateData;


        public WFTranslator()
        {
            InitTranslators();
        }

        private void InitTranslators()
        {
            dictTranslator.Clear();
            foreach (var dict in translateApi.Dict)
            {

                dictTranslator.AddEntry(dict.En, dict.Zh);
                dictTranslator.AddEntry(dict.En, dict.Zh);
                searchwordTranslator.Clear();
            }

            foreach (var sale in translateApi.Sale)
            {
                searchwordTranslator.AddEntry(sale.zh.Format(), sale.code);
                relicrewardTranslator.AddEntry(sale.en.Format(), sale.zh);
            }
            relicrewardTranslator.AddEntry("Forma Blueprint".Format(), "福马 蓝图");
            // TODO 找一个更加全面的翻译对照表
            invasionTranslator.Clear();
            foreach (var invasion in translateApi.Invasion)
            {
                invasionTranslator.AddEntry(invasion.En, invasion.Zh);
            }

            translateApi.Riven.Select(r => r = new Riven
            {
                id = r.id,
                modulus = r.modulus,
                name = dictTranslator.Translate(r.name),
                rank = r
                    .rank,
                thumb = r.thumb,
                type = dictTranslator.Translate(r.type)
            });
            weapons.Clear();
            weaponslist.Clear();
            foreach (var riven in translateApi.Riven)
            {
                var zh = dictTranslator.Translate(riven.name);
                weapons.Add(zh);
                weaponslist.Add(new Riven { id = riven.id, modulus = riven.modulus, name = riven.name, rank = riven.rank, thumb = riven.thumb, type = riven.type, zhname = zh });
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
                return "并没有查询到任何翻译,请检查输入.";
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
                        if (sale.zh.StartsWith(str))
                        {
                            var distance = lev.DistanceFrom(sale.zh.Format());
                            distancelist.Add(new StringInfo { LevDistance = distance, Name = sale.zh });
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
        /*public List<Relic> GetRelicInfo(string word)
        {
            return translateApi.Relic.Where(relic => relic.Name.Format().Contains(word)).ToList();
        }*/
        private static DateTime GetRealTime(DateTime time)
        {
            return time + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        }

        public List<Riven> GetMatchedWeapon(string weapon)
        {
            return weaponslist.Where(w => w.zhname.Format() == weapon).ToList();
        }
        public string TranslateWeaponType(string type)
        {
            return dictTranslator.Translate(type);
        }
        public string TranslateRelicReward(string reward)
        {
            return relicrewardTranslator.Translate(reward.Format());
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
                kuva.type = dictTranslator.Translate(kuva.type);
            }
        }

        public DateTime ConvertUnixToDatetime(long unix)
        {
            var date = DateTimeOffset.FromUnixTimeSeconds(unix);
            return date.UtcDateTime + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        }

        public void TranslateSentientOutpost(SentientOutpost se)
        {
            if (se.mission != null)
            {
                se.mission.node = TranslateNode(se.mission.node);
            }
            se.activation = GetRealTime(se.activation);
            se.expiry = GetRealTime(se.expiry);
            //se.previous.expiry = GetRealTime(se.previous.expiry);
            //se.previous.activation = GetRealTime(se.activation);

        }
        // 打了最后一愿之后就再也没法直视Riven这个单词了
        public void TranslateRivenOrders(List<WarframeAlertingPrime.SDK.Models.User.Order> orders)
        {
            foreach (var order in orders)
            {
                foreach (var property in order.properties)
                {
                    property.name = dictTranslator.Translate(property.name);
                }
            }
        }
        public SentientAnomaly TranslateSentientAnomaly(RawSentientAnomaly anomaly)
        {
            return new SentientAnomaly { end = ConvertUnixToDatetime(anomaly.end), name = anomaly.name, projection = ConvertUnixToDatetime(anomaly.projection), start = ConvertUnixToDatetime(anomaly.start) };
        }
        public void TranslateArbitrationMission(Arbitration ar)
        {

            ar.activation = GetRealTime(ar.activation);
            ar.expiry = GetRealTime(ar.expiry);
            ar.node = TranslateNode(ar.node);
            // // trick
            // 不需要trick了
            ar.type = dictTranslator.Translate(ar.type);

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
                enemy.agentType = dictTranslator.Translate(enemy.agentType);
                enemy.lastDiscoveredAt = TranslateNode(enemy.lastDiscoveredAt);
                enemy.lastDiscoveredTime = GetRealTime(enemy.lastDiscoveredTime);
            }
        }
        public void TranslateEvents(List<Event> events)
        {
            foreach (var @event in events)
            {
                @event.description = dictTranslator.Translate(@event.description);
            }
        }

        public string TranslateSearchWord(string source)
        {
            return searchwordTranslator.Translate(source);
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
                t.type = dictTranslator.Translate(t.type);
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
                    result = strings[0] + dictTranslator.Translate(nodeRegion);
                }
                else
                {
                    return dictTranslator.Translate(node);
                }

            }

            return result;
        }

        public bool ContainsWeapon(string weapon)
        {
            return weapons.Contains(weapon);
        }


        public void TranslateAlert(WFAlert alert)
        {
            var mission = alert.Mission;
            mission.Node = TranslateNode(mission.Node);
            mission.Type = dictTranslator.Translate(mission.Type);
            TranslateReward(mission.Reward);

            void TranslateReward(Reward reward)
            {
                foreach (var item in reward.CountedItems)
                {
                    item.Type = dictTranslator.Translate(item.Type);
                }

                for (var i = 0; i < reward.Items.Length; i++)
                {
                    reward.Items[i] = dictTranslator.Translate(reward.Items[i]);
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

                                sb.Append(dictTranslator.Translate(item));
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
                variant.missionType = dictTranslator.Translate(variant.missionType);
                if (variant.modifier.Contains(":"))
                {
                    var strs = variant.modifier.Split(":".ToCharArray());
                    strs = strs.Select(s => dictTranslator.Translate(s.Trim())).ToArray();
                    variant.modifier = strs.Connect(": ");
                }
                variant.modifier = dictTranslator.Translate(variant.modifier);
            }
            sortie.boss = dictTranslator.Translate(sortie.boss);
        }

        public void TranslateFissures(List<Fissure> fissures)
        {
            fissures = fissures.OrderBy(fissure => fissure.tierNum).ToList();
            foreach (var fissure in fissures)
            {
                fissure.node = TranslateNode(fissure.node);
                fissure.tier = dictTranslator.Translate(fissure.tier);
                fissure.missionType = dictTranslator.Translate(fissure.missionType);
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
                inventory.item = dictTranslator.Translate(inventory.item);
            }
            // ohhhhhhhhhhhhhhhhhhhhhhh奸商第一百次来带的东西真他妈劲爆啊啊啊啊啊啊啊啊啊啊啊 啊啊啊啊啊啊啊啊啊啊之后还带了活动电可我没囤多少呜呜呜呜呜呜穷了 哈哈哈哈哈哈老子开出一张绝路啊啊啊啊啊啊爽死了 呜呜呜呜电男loki出库我没刷我穷死了 为啥带金首发DENMSL 爷爷退坑了D2真好玩

        }

        public void TranslateWMOrder(WMInfo info, string searchword)
        {

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

            foreach (var order in info.orders.Items)
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
            Debug.Assert(source != null);
            dic[source] = target;
        }

        public void Clear()
        {
            dic.Clear();
        }
    }
}
