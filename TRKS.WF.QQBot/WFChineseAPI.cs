using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Channels;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public static WFTranslator WFTranslator = new WFTranslator();
        public static WFChineseAPI WFChineseApi = new WFChineseAPI();
    }
    public class WFChineseAPI
    {
        private WFTranslator translator = WFResource.WFTranslator;

        public List<WFInvasion> GetInvasions()
        {
            var invasions = WebHelper.DownloadJson<List<WFInvasion>>("https://api.warframestat.us/pc/invasions");
            foreach (var invasion in invasions)
            {
                translator.TranslateInvasion(invasion);
                invasion.activation= GetRealTime(invasion.activation);
            }

            return invasions;
        }

        public List<WFAlert> GetAlerts()
        {
            try
            {
                var alerts = WebHelper.DownloadJson<List<WFAlert>>("https://api.warframestat.us/pc/alerts");
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

        public CetusCycle GetCetusCycle()
        {
            var cycle = WebHelper.DownloadJson<CetusCycle>("https://api.warframestat.us/pc/cetusCycle");
            cycle.Expiry = GetRealTime(cycle.Expiry);
            return cycle;
        }

        public VallisCycle GetVallisCycle()
        {
            var cycle = WebHelper.DownloadJson<VallisCycle>("https://api.warframestat.us/pc/vallisCycle");
            cycle.expiry = GetRealTime(cycle.expiry);
            return cycle;
        }
        /*
        public async Task<List<SyndicateMission>> GetSyndicateMissions()// 这里暂时不着急改
        {
            try
            {
                var mission = await client.GetSyndicateMissionsAsync(Platform.PC);
            }
            catch (TaskCanceledException)
            {
                // nah
            }
            catch (HttpRequestException)
            {
                // nah
            }
            catch (Exception e)
            {
                var qq = Config.Instance.QQ;
                Messenger.SendPrivate(qq, $"赏金获取报错:{e}");
            }

            return null;
        }
        */


        public Sortie GetSortie()
        {
            var sortie = WebHelper.DownloadJson<Sortie>("https://api.warframestat.us/pc/sortie");
            translator.TranslateSortie(sortie);
            return sortie;
        }

        public List<SyndicateMission> GetSyndicateMissions()
        {
            var missions = WebHelper.DownloadJson<List<SyndicateMission>>("https://api.warframestat.us/pc/syndicateMissions");
            translator.TranslateSyndicateMission(missions);
            return missions;
        }
        public VoidTrader GetVoidTrader()
        {
            var trader = WebHelper.DownloadJson<VoidTrader>("https://api.warframestat.us/pc/voidTrader");
            trader.activation = GetRealTime(trader.activation);
            trader.expiry = GetRealTime(trader.expiry);
            translator.TranslateVoidTrader(trader);
            return trader;
        }

        public List<Fissure> GetFissures()
        {
            var fissures = WebHelper.DownloadJson<List<Fissure>>("https://api.warframestat.us/pc/fissures");
            translator.TranslateFissures(fissures);
            return fissures;
        }

        public List<Event> GetEvents()
        {
            var events = WebHelper.DownloadJson<List<Event>>("https://api.warframestat.us/pc/events");
            translator.TranslateEvents(events);
            foreach (var @event in events)
            {
                @event.expiry = GetRealTime(@event.expiry);
            }

            return events;
        }

        private static DateTime GetRealTime(DateTime time)
        {
            return time + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        }
    }

    public class WFTranslator
    {
        private Dictionary<string /*type*/, Translator> dictTranslators = new Dictionary<string, Translator>();
        private Dictionary<string, Translator> searchwordTranslator = new Dictionary<string, Translator>();
        private Translator invasionTranslator = new Translator();
        private Translator alertTranslator = new Translator();
        private List<string> weapons = new List<string>();
        private WFApi translateApi = GetTranslateApi();

        public WFTranslator()
        {
            dictTranslators.Add("All", new Translator());
            dictTranslators.Add("WM", new Translator());

            foreach (var dict in translateApi.Dict)
            {
                var type = dict.Type;
                if (!dictTranslators.ContainsKey(type))
                {
                    dictTranslators.Add(type, new Translator()); 
                }               
                dictTranslators["All"].AddEntry(dict.En, dict.Zh);
                dictTranslators[type].AddEntry(dict.En, dict.Zh);
            }
            
            foreach (var sale in translateApi.Sale)
            {
                if (!searchwordTranslator.ContainsKey("Word"))
                {
                    searchwordTranslator.Add("Word", new Translator());
                }

                if (!searchwordTranslator.ContainsKey("Item"))
                {
                    searchwordTranslator.Add("Item", new Translator());
                }
                searchwordTranslator["Word"].AddEntry(sale.Zh.Format(), sale.Search);
                searchwordTranslator["Item"].AddEntry(sale.Search, sale.Zh);
            }

            foreach (var invasion in translateApi.Invasion)
            {
                invasionTranslator.AddEntry(invasion.En, invasion.Zh);
            }

            foreach (var alert in translateApi.Alert)
            {
                alertTranslator.AddEntry(alert.En, alert.Zh);
            }

            foreach (var riven in translateApi.Riven)
            {
                weapons.Add(riven.Name.Format());
            }

            foreach (var modifier in translateApi.Modifier)
            {
                if (!dictTranslators.ContainsKey("Modifier"))
                {
                    dictTranslators.Add("Modifier", new Translator());
                }
                dictTranslators["Modifier"].AddEntry(modifier.en, modifier.zh);
            }
        }

        private static WFApi GetTranslateApi()
        {
            var alerts = WebHelper.DownloadJson<Alert[]>(
                    "https://raw.githubusercontent.com/Richasy/WFA_Lexicon/master/WF_Alert.json");
            var dicts = WebHelper.DownloadJson<Dict[]>(
                    "https://raw.githubusercontent.com/Richasy/WFA_Lexicon/master/WF_Dict.json");
            var invasions = WebHelper.DownloadJson<Invasion[]>(
                    "https://raw.githubusercontent.com/Richasy/WFA_Lexicon/master/WF_Invasion.json");
            var sales = WebHelper.DownloadJson<Sale[]>(
                    "https://raw.githubusercontent.com/Richasy/WFA_Lexicon/master/WF_Sale.json");
            var riven = WebHelper.DownloadJson<Riven[]>(
                    "https://raw.githubusercontent.com/Richasy/WFA_Lexicon/master/WF_Riven.json");
            var relic = WebHelper.DownloadJson<Relic[]>(
                    "https://raw.githubusercontent.com/Richasy/WFA_Lexicon/master/WF_Relic.json");
            var modifier = WebHelper.DownloadJson<Modifier[]>(
                "https://raw.githubusercontent.com/Richasy/WFA_Lexicon/master/WF_Modifier.json");
            var translateApi = new WFApi
            {
                Alert = alerts, Dict = dicts, Invasion = invasions, Relic = relic, Riven = riven, Sale = sales, Modifier = modifier
            };
            return translateApi;
        }

        public List<string> GetSimilarItem(string word)
        {
            Fastenshtein.Levenshtein lev = new Fastenshtein.Levenshtein(word);
            var distancelist = new SortedSet<StringInfo>();
            foreach (var sale in translateApi.Sale)
            {
                var distance = lev.DistanceFrom(sale.Zh.Format());
                distancelist.Add(new StringInfo {LevDistance = distance, Name = sale.Zh});
            }

            return distancelist.Where(dis => dis.LevDistance != 0).Take(5).Select(info => info.Name).ToList();
        }

        public List<Relic> GetRelicInfo(string word)
        {
            return translateApi.Relic.Where(relic => relic.Name.Format().Contains(word)).ToList();
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

        private void TranslateReward(Defenderreward reward)
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

        private void TranslateReward(Attackerreward reward)
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
            var strings = node.Split('(');
            var nodeRegion = strings[1].Split(')')[0];
            return strings[0] + dictTranslators["Star"].Translate(nodeRegion);
        }

        public bool ContainsWeapon(string weapon)
        {
            return weapons.Contains(weapon);
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
                                job.rewardPool[i] = sb.ToString();
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
            foreach (var fissure in fissures)
            {
                fissure.node = TranslateNode(fissure.node);
                fissure.tier = dictTranslators["Word"].Translate(fissure.tier);
                fissure.missionType = dictTranslators["Mission"].Translate(fissure.missionType);
            }
        }

        public void TranslateVoidTrader(VoidTrader trader)
        {
            trader.location = TranslateNode(trader.location).Replace("Relay", "中继站");
            foreach (var inventory in trader.inventory)
            {
                inventory.item = dictTranslators["All"].Translate(inventory.item);
            }
            // ohhhhhhhhhhhhhhhhhhhhhhh奸商第一百次来带的东西真他妈劲爆啊啊啊啊啊啊啊啊啊啊啊 啊啊啊啊啊啊啊啊啊啊之后还带了活动电可我没囤多少呜呜呜呜呜呜穷了 哈哈哈哈哈哈老子开出一张绝路啊啊啊啊啊啊爽死了
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
    }
}
