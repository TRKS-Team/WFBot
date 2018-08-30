using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var alerts = await client.GetAlertsAsync(Platform.PC);
            foreach (var alert in alerts)
            {
                translator.TranslateAlert(alert);
                alert.StartTime = GetRealTime(alert.StartTime);
                alert.EndTime = GetRealTime(alert.EndTime);
            }

            return alerts;
        }

        public CetusCycle GetCetusCycle()
        {
            var cycle = WebHelper.DownloadJson<CetusCycle>("https://api.warframestat.us/pc/cetusCycle");
            cycle.Expiry = GetRealTime(cycle.Expiry);
            return cycle;
        }

        private static DateTime GetRealTime(DateTime time)
        {
            return time + TimeSpan.FromHours(8); // TODO 这里需要改
        }
    }


    public class WFTranslator
    {
        private WFApi translateApi = GetTranslateAPI();

        private Dictionary<string /*type*/, Translator> dictTranslators = new Dictionary<string, Translator>();
        private Translator invasionTranslator = new Translator();
        private Translator alertTranslator = new Translator();

        public WFTranslator()
        {
            foreach (var dict in translateApi.Dict)
            {
                var type = dict.Type;
                if (!dictTranslators.ContainsKey(type))
                    dictTranslators.Add(type, new Translator());
                dictTranslators[type].AddEntry(dict.En, dict.Zh);
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
            return dic[source];
        }

        public void AddEntry(string source, string target)
        {
            dic[source] = target;
        }
    }
}
