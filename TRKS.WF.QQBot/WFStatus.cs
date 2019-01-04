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
            var msg = $"{WFFormatter.ToString(cetuscycle)}\r\n" +
                      $"{WFFormatter.ToString(valliscycle)}";
            
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

        public void SendFortunaMissions(string group)
        {
            var missions = api.GetSyndicateMissions();
            var sb = new StringBuilder();
            sb.AppendLine(WFFormatter.ToString(missions.Where(mission => mission.syndicate == "Solaris United").ToList().First()));
            sb.AppendLine($"您正在查看 福尔图娜 的全部赏金任务,使用: 地球赏金 来查询希图斯的赏金任务.");
            Messenger.SendGroup(group, sb.ToString());
        }

        public void SendCetusMissions(string group)
        {
            var missions = api.GetSyndicateMissions();
            var sb = new StringBuilder();
            sb.AppendLine(WFFormatter.ToString(missions.Where(mission => mission.syndicate == "Ostrons").ToList().First()));
            sb.AppendLine("您正在查看 希图斯 的全部赏金任务,使用: 金星赏金 来查询 福尔图娜 的赏金任务.");
            Messenger.SendGroup(group, sb.ToString());
        }

        //public void SendFissures(string group, List<string> words)
        public void SendFissures(string group)
        {
            var fissures = api.GetFissures();
            var result = new List<Fissure>();
            fissures = fissures.Where(fissure => fissure.active).ToList();
            /*if (words.Count > 1)
            {
                fissures = fissures.Where(fissure => fissure.missionType == words[0] || fissure.tier == words[0])
                    .ToList();
                words.RemoveAt(0);
                foreach (var word in words)
                {
                    result = result.Concat(fissures.Where(fissure => fissure.missionType == word || fissure.tier == word)).ToList();
                }
            }
            else
            {
                foreach (var word in words)
                {
                    result = fissures.Where(fissure => fissure.missionType == word || fissure.tier == word).ToList();
                }
            }*/
            result.AddRange(fissures);
            var msg = $"{WFFormatter.ToString(result)}\r\n" + "目前机器人正在调查一次发送全部裂隙的玩家反响,麻烦各大群的管理员来和我说一下你的看法.";
                //$你正在查看与{string.Join(" ", words)}有关的所有裂隙.";
            Messenger.SendGroup(group, msg);
        }

        public void SendRelicInfo(string group, string word)
        {
            var relics = translator.GetRelicInfo(word);
            var msg = $"{WFFormatter.ToString(relics)}\r\n" +
                      $"你正在查看与 {word} 有关的所有遗物.";

            Messenger.SendGroup(group, msg);
        }

        public void SendEvent(string group)
        {
            var events = api.GetEvents();
            var msg = WFFormatter.ToString(events);

            Messenger.SendGroup(group, msg);
        }
    }
}
