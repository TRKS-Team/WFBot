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
        private static WFChineseAPI api => WFResource.WFChineseApi;
        private static WFTranslator translator => WFResource.WFTranslator;
        private string platform => Config.Instance.Platform.ToString();
        // private List<Fissure> _fissures = api.GetFissures();

        public void SendCycles(GroupNumber group)
        {
            var cetuscycle = api.GetCetusCycle();
            var valliscycle = api.GetVallisCycle();
            var earthcycle = api.GetEarthCycle();
            var msg = $"{WFFormatter.ToString(cetuscycle)}\r\n" +
                      $"{WFFormatter.ToString(valliscycle)}\r\n" +
                      $"{WFFormatter.ToString(earthcycle)}";

            Messenger.SendGroup(group, msg.AddPlatformInfo().AddRemainCallCount(group));
        }

        public void SendSortie(GroupNumber group)
        {
            var sortie = api.GetSortie();
            var msg = WFFormatter.ToString(sortie);
            
            Messenger.SendGroup(group, msg.AddPlatformInfo().AddRemainCallCount(group));
        }

        public void SendVoidTrader(GroupNumber group)
        {
            var trader = api.GetVoidTrader();
            var msg = WFFormatter.ToString(trader);

            Messenger.SendGroup(group, msg.AddPlatformInfo().AddRemainCallCount(group));
        }

        public void SendFortunaMissions(GroupNumber group, int index)
        {
            var missions = api.GetSyndicateMissions();
            var sb = new StringBuilder();
            sb.AppendLine(WFFormatter.ToString(missions.First(mission => mission.syndicate == "Solaris United"), index));
            sb.AppendLine();
            sb.Append($"您正在查看 福尔图娜 的全部赏金任务, 使用: /地球赏金 来查询希图斯的赏金任务.");
            Messenger.SendGroup(group, sb.ToString().AddPlatformInfo().AddRemainCallCount(group));
        }

        public void SendCetusMissions(GroupNumber group, int index)
        {
            var missions = api.GetSyndicateMissions();
            var sb = new StringBuilder();
            sb.AppendLine(WFFormatter.ToString(missions.First(mission => mission.syndicate == "Ostrons"), index));
            sb.AppendLine();
            sb.Append("您正在查看 希图斯 的全部赏金任务, 使用: /金星赏金 来查询 福尔图娜 的赏金任务.");
            Messenger.SendGroup(group, sb.ToString().AddPlatformInfo().AddRemainCallCount(group));
        }

        //public void SendFissures(string group, List<string> words)
        public void SendFissures(GroupNumber group)
        {
            // var fissures = _fissures.Where(fissure => fissure.active).ToList();
            var fissures = api.GetFissures().Where(fissure => fissure.active).ToList();
            var msg = WFFormatter.ToString(fissures);
            Messenger.SendGroup(group, msg.AddPlatformInfo().AddRemainCallCount(group));
            // _fissures = api.GetFissures();
        }

        public void SendRelicInfo(GroupNumber group, string word)
        {
            var relics = translator.GetRelicInfo(word);
            var msg = $"{WFFormatter.ToString(relics)}\n\n" +
                      $"你正在查看与 {word} 有关的所有遗物.";

            Messenger.SendGroup(group, msg.AddPlatformInfo().AddRemainCallCount(group));
        }

        public void SendEvent(GroupNumber group)
        {
            var events = api.GetEvents();
            if (events.Any())
            {
                var msg = WFFormatter.ToString(events);
                Messenger.SendGroup(group, msg.AddPlatformInfo().AddRemainCallCount(group));
            }
            else
            {
                Messenger.SendGroup(group, "目前游戏内没有任何活动 (尸鬼, 豺狼, 舰队).".AddPlatformInfo().AddRemainCallCount(group));
            }
        }

        public void SendNightWave(GroupNumber group)
        {
            var nightwave = api.GetNightWave();
            var msg = WFFormatter.ToString(nightwave).AddPlatformInfo().AddRemainCallCount(group);
            Messenger.SendGroup(group, msg);
        }

        public void SendTranslateResult(GroupNumber group, string str)
        {
            var msg = translator.GetTranslateResult(str).AddRemainCallCount(group);
            Messenger.SendGroup(group, msg);
        }

        public void SendKuvaMissions(GroupNumber group)
        {
            var sb = new StringBuilder();
            var kuvas = api.GetKuvaMissions();
            sb.AppendLine("以下是所有赤毒任务: \n\n");
            // foreach (var kuva in kuvas.Where(k => k.missiontype.Contains("KuvaMission") && k.start < DateTime.Now && DateTime.Now < k.end))
            foreach (var kuva in kuvas)
            {
                sb.AppendLine(WFFormatter.ToString(kuva));
                sb.AppendLine();
            }
            Messenger.SendGroup(group, sb.ToString().Trim());
        }

        public void SendArbitrationMission(GroupNumber group)
        {
            var sb = new StringBuilder();
            var ar = api.GetArbitrationMission();
            // var mission = kuvas.First(k => k.missiontype == "EliteAlertMission" && k.start < DateTime.Now && DateTime.Now < k.end);
            sb.AppendLine("以下是仲裁警报的信息: ");
            sb.AppendLine(WFFormatter.ToString(ar));
            Messenger.SendGroup(group, sb.ToString().Trim());

        }
    }
}
