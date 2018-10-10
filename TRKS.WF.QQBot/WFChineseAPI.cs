using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using WarframeNET;

namespace TRKS.WF.QQBot
{
    public class WFChineseAPI
    {
        private WarframeClient client;
        private WFTranslator translator;

        public WFChineseAPI()
        {
            client = new WarframeClient();
            translator = new WFTranslator();
        }

        public async Task<List<WarframeNET.Invasion>> GetInvasions()
        {
            var invasions = await client.GetInvasionsAsync(Platform.PC);
            foreach (var invasion in invasions)
            {
                translator.TranslateInvasion(invasion);
                invasion.StartTime = GetRealTime(invasion.StartTime);
            }

            return invasions;
        }

        public async Task<List<WarframeNET.Alert>> GetAlerts()
        {
            try
            {
                var alerts = await client.GetAlertsAsync(Platform.PC);
                foreach (var alert in alerts)
                {
                    translator.TranslateAlert(alert);
                    alert.StartTime = GetRealTime(alert.StartTime);
                    alert.EndTime = GetRealTime(alert.EndTime);
                }

                return alerts;
            }
            catch (HttpRequestException)
            {
                // 啥也不做
            }
            catch (Exception e)
            {
                var qq = Config.Instance.QQ;
                Messenger.SendPrivate(qq, $"警报获取报错:{Environment.NewLine}{e}");

            }
            return new List<WarframeNET.Alert>();
        }

        public CetusCycle GetCetusCycle()
        {
            var cycle = WebHelper.DownloadJson<CetusCycle>("https://api.warframestat.us/pc/cetusCycle");
            cycle.Expiry = GetRealTime(cycle.Expiry);
            return cycle;
        }

        public Sortie GetSortie()
        {
            var sortie = WebHelper.DownloadJson<Sortie>("https://api.warframestat.us/pc/sortie");
            translator.TranslateSortie(sortie);
            return sortie;
        }

        public VoidTrader GetVoidTrader()
        {
            var trader = WebHelper.DownloadJson<VoidTrader>("https://api.warframestat.us/pc/voidTrader");
            trader.activation = GetRealTime(trader.activation);
            trader.expiry = GetRealTime(trader.expiry);
            translator.TranslateVoidTrader(trader);
            return trader;
        }
        private static DateTime GetRealTime(DateTime time)
        {
            return time + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        }

    }


    public class WFTranslator
    {
        private WFApi translateApi = GetTranslateAPI();

        private Dictionary<string /*type*/, Translator> dictTranslators = new Dictionary<string, Translator>();
        private Translator searchwordTranslators = new Translator();
        private Translator invasionTranslator = new Translator();
        private Translator alertTranslator = new Translator();

        public WFTranslator()
        {
            foreach (var dict in translateApi.Dict)
            {
                var type = dict.Type;
                if (!dictTranslators.ContainsKey(type))
                {
                    dictTranslators.Add(type, new Translator()); 
                }

                if (!dictTranslators.ContainsKey("All"))
                {
                    dictTranslators.Add("All", new Translator());
                }
                dictTranslators["All"].AddEntry(dict.En, dict.Zh);
                dictTranslators[type].AddEntry(dict.En, dict.Zh);
            }

            foreach (var sale in translateApi.Sale)
            {
                 searchwordTranslators.AddEntry(sale.Zh.Replace(" ", "").ToLower(), sale.Search);
            }

            foreach (var invasion in translateApi.Invasion)
            {
                invasionTranslator.AddEntry(invasion.En, invasion.Zh);
            }

            foreach (var alert in translateApi.Alert)
            {
                alertTranslator.AddEntry(alert.En, alert.Zh);
            }
        }

        private static WFApi GetTranslateAPI()
        {
            return WebHelper.DownloadJson<WFApi>("https://api.richasy.cn/api/lib/localdb/tables");
        }

        public string TranslateSearchWord(string source)
        {
            return searchwordTranslators.Translate(source);
        }
        public void TranslateInvasion(WarframeNET.Invasion invasion)
        {
            TranslateReward(invasion.AttackerReward);
            TranslateReward(invasion.DefenderReward);
            invasion.Node = TranslateNode(invasion.Node);

            void TranslateReward(Reward reward)
            {
                foreach (var item in reward.CountedItems)
                {
                    item.Type = invasionTranslator.Translate(item.Type);
                }

                for (var i = 0; i < reward.Items.Count; i++)
                {
                    reward.Items[i] = alertTranslator.Translate(reward.Items[i]);
                }
            }
        }

        private string TranslateNode(string node)
        {
            var strings = node.Split('(');
            var nodeRegion = strings[1].Split(')')[0];
            return strings[0] + dictTranslators["Star"].Translate(nodeRegion);
        }

        public void TranslateAlert(WarframeNET.Alert alert)
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

                for (var i = 0; i < reward.Items.Count; i++)
                {
                    reward.Items[i] = alertTranslator.Translate(reward.Items[i]);
                }
            }

        }

        public void TranslateSortie(Sortie sortie)
        {
            foreach (var variant in sortie.variants)
            {
                variant.node = TranslateNode(variant.node).Replace("Plains of Eidolon", "夜灵平野"); // 这个不在翻译api里
                variant.missionType = dictTranslators["Mission"].Translate(variant.missionType);
                variant.modifier = TranslateModifier(variant.modifier);
            }
            sortie.boss = dictTranslators["Word"].Translate(sortie.boss);
        }


        public void TranslateVoidTrader(VoidTrader trader)
        {
            trader.location = TranslateNode(trader.location).Replace("Relay", "中继站");
            foreach (var inventory in trader.inventory)
            {
                inventory.item = dictTranslators["All"].Translate(inventory.item);
            }
            // 下次奸商来了我再写翻译所带物品 ohhhhhhhhhhhhhhhhhhhhhhh奸商第一百次来带的东西真他妈劲爆啊啊啊啊啊啊啊啊啊啊啊
        }

        public void TranslateWMOrder(WMInfo info)
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
        public string TranslateModifier(string modifier)
        {
            var result = "";
            switch (modifier)
            {
                case "Weapon Restriction: Assault Rifle Only":
                    result = "武器限定：突击步枪";
                    break;
                case "Weapon Restriction: Pistol Only":
                    result = "武器限定：手枪";
                    break;
                case "Weapon Restriction: Melee Only":
                    result = "武器限定：近战";
                    break;
                case "Weapon Restriction: Bow Only":
                    result = "武器限定：弓箭";
                    break;
                case "Weapon Restriction: Shotgun Only":
                    result = "武器限定：霰弹枪";
                    break;
                case "Weapon Restriction: Sniper Only":
                    result = "武器限定：狙击枪";
                    break;
                case "Enemy Elemental Enhancement: Corrosive":
                    result = "敌人元素强化：腐蚀";
                    break;
                case "Enemy Elemental Enhancement: Electricity":
                    result = "敌人元素强化：电击";
                    break;
                case "Enemy Elemental Enhancement: Blast":
                    result = "敌人元素强化：爆炸";
                    break;
                case "Enemy Elemental Enhancement: Heat":
                    result = "敌人元素强化：火焰";
                    break;
                case "Enemy Elemental Enhancement: Cold":
                    result = "敌人元素强化：冰冻";
                    break;
                case "Enemy Elemental Enhancement: Gas":
                    result = "敌人元素强化：毒气";
                    break;
                case "Enemy Elemental Enhancement: Magnetic":
                    result = "敌人元素强化：磁力";
                    break;
                case "Enemy Elemental Enhancement: Toxin":
                    result = "敌人元素强化：毒素";
                    break;
                case "Enemy Elemental Enhancement: Radiation":
                    result = "敌人元素强化：辐射";
                    break;
                case "Enemy Elemental Enhancement: Viral":
                    result = "敌人元素强化：病毒";
                    break;
                case "Enemy Physical Enhancement: Impact":
                    result = "敌人物理强化：冲击";
                    break;
                case "Enemy Physical Enhancement: Puncture":
                    result = "敌人物理强化：穿刺";
                    break;
                case "Enemy Physical Enhancement: Slash":
                    result = "敌人物理强化：切割";
                    break;
                case "Augmented Enemy Armor":
                    result = "敌人护甲强化";
                    break;
                case "Eximus Stronghold":
                    result = "卓越者大本营";
                    break;
                case "Energy Reduction":
                    result = "能量上限减少";
                    break;
                case "Enhanced Enemy Shields":
                    result = "敌人护盾强化";
                    break;
                case "Environmental Effect: Extreme Cold":
                    result = "环境改变：极寒";
                    break;
                case "Environmental Hazard: Fire":
                    result = "环境灾害：火灾";
                    break;
                case "Environmental Hazard: Dense Fog":
                    result = "环境灾害：浓雾";
                    break;
                case "Environmental Effect: Cryogenic Leakage":
                    result = "环境改变：冷却液泄露";
                    break;
                case "Environmental Hazard: Electromagnetic Anomalies":
                    result = "环境灾害：电磁异常";
                    break;
                case "Environmental Hazard: Radiation Pockets":
                    result = "环境灾害：辐射灾害";
                    break;
                default:
                    result = modifier;
                    break;
            }

            return result;
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
        private Dictionary<string, string> dic;

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
