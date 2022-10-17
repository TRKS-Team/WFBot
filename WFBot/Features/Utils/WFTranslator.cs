using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using WFBot.Features.Common;
using WFBot.Features.Resource;

namespace WFBot.Features.Utils
{
    public class WFTranslator
    {
        private Translator dictTranslator = new Translator(); // 翻译器之祖
        private Translator searchwordTranslator = new Translator(); // 将中文物品转换成wm的搜索地址
        private Translator urlnameTranslator = new Translator();  // 将WMR的urlname转换成人类可读
        private Translator wikiTranslator = new Translator(); // 我忘了是干嘛的 好像没用了
        private Translator nightwaveTranslator = new Translator(); // 午夜电波任务翻译
        private Translator relicrewardTranslator = new Translator();
        private Translator invasionTranslator = new Translator(); // 入侵物品翻译
        private List<Riven> weaponslist = new List<Riven>(); // 武器的list 用来wfa搜索
        // She is known as riven, riven of a thousand voice, the last known ahamkara.
        private List<string> weapons = new List<string>();// 所有武器的中文
        private WFApi translateApi => WFResources.WFTranslateData;
        private WFBotApi wfbotApi => WFResources.WFBotTranslateData;
        private WMAAttribute[] attributes => WFResources.WMAuction.Attributes;
        private WMARiven[] rivens => WFResources.WMAuction.ZHRivens;
        private WeaponInfo[] weaponInfos => WFResources.Weaponinfos;


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
                searchwordTranslator.Clear();
            }

            foreach (var sale in wfbotApi.Sale)
            {
                searchwordTranslator.AddEntry(sale.zh.Format(), sale.code);
                relicrewardTranslator.AddEntry(sale.en.Format(), sale.zh);
            }
            relicrewardTranslator.AddEntry("Forma Blueprint".Format(), "福马 蓝图");

            invasionTranslator.Clear();
            foreach (var invasion in translateApi.Invasion)
            {
                invasionTranslator.AddEntry(invasion.En, invasion.Zh);
            }

            weapons.Clear();
            weaponslist.Clear();
            foreach (var riven in translateApi.Riven)
            {
                var zh = dictTranslator.Translate(riven.name).Result;
                weapons.Add(zh);
                weaponslist.Add(new Riven { id = riven.id, modulus = riven.modulus, name = riven.name, rank = riven.rank, thumb = riven.thumb, type = riven.type, zhname = zh });
            }
            nightwaveTranslator.Clear();
            foreach (var wave in translateApi.NightWave)
            {
                nightwaveTranslator.AddEntry(wave.en.Format(), wave.zh);
            }
            urlnameTranslator.Clear();
            foreach (var attribute in attributes)
            {
                urlnameTranslator.AddEntry(attribute.UrlName, attribute.Effect);
            }
        }

        static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(IEnumerable<TValue> enumerable, Func<TValue, TKey> selector)
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var item in enumerable)
            {
                result[selector(item)] = item;
            }

            return result;
        }
        /*void ApplySlang(Translator translator)
        {
            var sales = ToDictionary(translateApi.Sale, s => s.zh);
            foreach (var slangItem in SlangManager.AllSlang)
            {
                if (!sales.TryGetValue(slangItem.Source, out var sale))
                {
                    Console.WriteLine($"黑话: {slangItem.Source} 未能在 API 中找到对应条目.");
                }

                foreach (var s in slangItem.Slang)
                {
                    translator.AddEntry(s.Format(), sale.code);
                }
            }
        }*/

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
                    foreach (var sale in wfbotApi.Sale)
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
                case "wma":
                    foreach (var info in weaponInfos.Select(w => w.zhname))
                    {
                        if (info.StartsWith(str))
                        {
                            var distance = lev.DistanceFrom(info.Format());
                            distancelist.Add(new StringInfo {LevDistance = distance, Name = info});
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

        public string GetAttributeEffect(string urlname)
        {
            return urlnameTranslator.Translate(urlname).Result;
        }

        public string GetRivenUrlName(string weapon)
        {
            return rivens.First(r => r.ItemName.Format() == weapon).UrlName;
        }
        public List<Riven> GetMatchedWeapon(string weapon)
        {
            return weaponslist.Where(w => w.zhname.Format() == weapon).ToList();
        }
        public string TranslateWeaponType(string type)
        {
            return dictTranslator.Translate(type).Result;
        }
        public string TranslateRelicReward(string reward)
        {
            return relicrewardTranslator.Translate(reward.Format()).Result;
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
                kuva.type = dictTranslator.Translate(kuva.type).Result;
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

        public void TranslateArchonHunt(ArchonHunt ah)
        {
            ah.expiry = GetRealTime(ah.expiry);
            ah.activation = GetRealTime(ah.activation);
            
            ah.boss = dictTranslator.Translate(ah.boss).Result;

            foreach (var mission in ah.missions)
            {
                mission.node = TranslateNode(mission.node);
                mission.type = dictTranslator.Translate(mission.type).Result;
            }

        }
        // 打了最后一愿之后就再也没法直视Riven这个单词了
        public void TranslateRivenOrders(List<WarframeAlertingPrime.SDK.Models.User.Order> orders)
        {
            foreach (var order in orders)
            {
                foreach (var property in order.properties)
                {
                    property.name = dictTranslator.Translate(property.name).Result;
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
            ar.type = dictTranslator.Translate(ar.type).Result;

        }
        public void TranslateNightWave(WFNightWave nightwave)
        {
            foreach (var challenge in nightwave.activeChallenges)
            {
                var translate = nightwaveTranslator.Translate(challenge.desc.Format().Replace(",", ""));
                challenge.desc = translate.IsTranslated ? translate.Result : challenge.desc;
                challenge.title = nightwaveTranslator.Translate(challenge.title.Format()).Result;
                challenge.expiry = GetRealTime(challenge.expiry);
            }
        }
        public void TranslatePersistentEnemies(List<PersistentEnemie> enemies)
        {
            foreach (var enemy in enemies)
            {
                enemy.agentType = dictTranslator.Translate(enemy.agentType).Result;
                enemy.lastDiscoveredAt = TranslateNode(enemy.lastDiscoveredAt);
                enemy.lastDiscoveredTime = GetRealTime(enemy.lastDiscoveredTime);
            }
        }
        public void TranslateEvents(List<Event> events)
        {
            foreach (var @event in events)
            {
                @event.description = dictTranslator.Translate(@event.description).Result;
            }
        }

        public TranslateResult TranslateSearchWord(string source)
        {
            return searchwordTranslator.Translate(source);
        }

        public void TranslateInvasion(WFInvasion invasion)
        {
            TranslateReward(invasion.attackerReward);
            TranslateReward(invasion.defenderReward);
            
            invasion.node = TranslateNode(invasion.node);

            invasion.desc = dictTranslator.Translate(invasion.desc).Result;
        }

        private void TranslateReward(RewardInfo reward)
        {
            foreach (var item in reward.countedItems)
            {
                item.type = invasionTranslator.Translate(item.type).Result;
            }

            foreach (var t in reward.countedItems)
            {
                t.type = dictTranslator.Translate(t.type).Result;
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
                    return dictTranslator.Translate(node).Result;
                }

            }

            return result;
        }


        public void TranslateAlert(WFAlert alert)
        {
            var mission = alert.Mission;
            mission.Node = TranslateNode(mission.Node);
            mission.Type = dictTranslator.Translate(mission.Type).Result;
            TranslateReward(mission.Reward);

            void TranslateReward(Reward reward)
            {
                foreach (var item in reward.CountedItems)
                {
                    item.Type = dictTranslator.Translate(item.Type).Result;
                }

                for (var i = 0; i < reward.Items.Length; i++)
                {
                    reward.Items[i] = dictTranslator.Translate(reward.Items[i]).Result;
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
                    if (mission.syndicate == "Ostrons" || mission.syndicate == "Solaris United" || mission.syndicate == "Entrati")
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
                variant.missionType = dictTranslator.Translate(variant.missionType).Result;
                if (variant.modifier.Contains(":"))
                {
                    var strs = variant.modifier.Split(":".ToCharArray());
                    strs = strs.Select(s => dictTranslator.Translate(s.Trim()).Result).ToArray();
                    variant.modifier = strs.Connect(": ");
                }
                variant.modifier = dictTranslator.Translate(variant.modifier).Result;
            }
            sortie.boss = dictTranslator.Translate(sortie.boss).Result;
        }

        public void TranslateFissures(List<Fissure> fissures)
        {
            fissures = fissures.OrderBy(fissure => fissure.tierNum).ToList();
            foreach (var fissure in fissures)
            {
                fissure.node = TranslateNode(fissure.node);
                fissure.tier = dictTranslator.Translate(fissure.tier).Result;
                fissure.missionType = dictTranslator.Translate(fissure.missionType).Result;
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
                inventory.item = dictTranslator.Translate(inventory.item).Result;
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
                        order.user.status = "游戏中";
                        break;
                    case "online":
                        order.user.status = "在线";
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
                        order.user.status = "游戏中";
                        break;
                    case "online":
                        order.user.status = "在线";
                        break;
                    case "offline":
                        order.user.status = "离线";
                        break;
                }
            }

        }


    }
}