using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Humanizer;
using Humanizer.Localisation;
using Newbe.Mahua;

namespace TRKS.WF.QQBot
{

    public class WFStatus
    {
        private readonly WFChineseAPI api = WFResource.WFChineseApi;
        private readonly WFTranslator translator = WFResource.WFTranslator;

        public void SendCycles(string group)
        {
            var cetuscycle = api.GetCetusCycle();
            var valliscycle = api.GetVallisCycle();
            var msg = WFFormatter.ToString(cetuscycle) + Environment.NewLine + WFFormatter.ToString(valliscycle);
            
            Messenger.SendGroup(group, msg);
        }

        public void SendSortie(string group)
        {
            var sortie = api.GetSortie();
            var msg = WFFormatter.ToString(sortie);
            
            Messenger.SendGroup(group, msg);
        }

        public void SendVoidTrader(string group)
        {
            var trader = api.GetVoidTrader();
            var msg = WFFormatter.ToString(trader);

            Messenger.SendGroup(group, msg);
        }


        public void SendSyndicateMissions(string group, int index)
        {
            var missions = api.GetSyndicateMissions();
            var sb = new StringBuilder();
            sb.AppendLine(WFFormatter.ToString(missions.Where(mission => mission.syndicate == "Ostrons").ToList().First(), index - 1));
            sb.AppendLine(WFFormatter.ToString(missions.Where(mission => mission.syndicate == "Solaris United").ToList()[0], index - 1));
            sb.AppendLine();
            sb.Append($"您正在查看第 {index} 个赏金任务 可使用 /[关键词][空格][任务顺序] 来查询特定任务.");
            Messenger.SendGroup(group, sb.ToString());
        }

        public void SendFissures(string group, List<string> words)
        {
            var fissures = api.GetFissures();
            var result = new List<Fissure>();
            fissures = fissures.Where(fissure => fissure.active).ToList();
            if (words.Count > 1)
            {
                fissures = fissures.Where(fissure => fissure.missionType == words[0] || fissure.tier == words[0])
                    .ToList();
                words.RemoveAt(0);
                foreach (var word in words)
                {
                    result.Concat(fissures.Where(fissure => fissure.missionType == word || fissure.tier == word));
                }
            }
            else
            {
                foreach (var word in words)
                {
                    result = fissures.Where(fissure => fissure.missionType == word || fissure.tier == word).ToList();
                }
            }

            var msg = WFFormatter.ToString(result) + Environment.NewLine + $"你正在查看与{string.Join(" ", words)}有关的所有裂隙.";
            Messenger.SendGroup(group, msg);
        }

        public void SendRelicInfo(string group, string word)
        {
            var relics = translator.GetRelicInfo(word);
            var msg = WFFormatter.ToString(relics) + $"你正在查看与 {word} 有关的所有遗物.";
            Messenger.SendGroup(group, msg);
        }
    }
}
