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
        private string platform => Config.Instance.Platform.ToString();

        public void SendCycles(string group)
        {
            var cetuscycle = api.GetCetusCycle();
            var valliscycle = api.GetVallisCycle();
            var msg = $"{WFFormatter.ToString(cetuscycle)}\r\n" +
                      $"{WFFormatter.ToString(valliscycle)}" +
                      $"\r\n机器人目前运行的平台是: {platform}";

            Messenger.SendGroup(group, msg);
        }

        public void SendSortie(string group)
        {
            var sortie = api.GetSortie();
            var msg = WFFormatter.ToString(sortie);
            
            Messenger.SendGroup(group, msg + $"\r\n机器人目前运行的平台是: {platform}");
        }

        public void SendVoidTrader(string group)
        {
            var trader = api.GetVoidTrader();
            var msg = WFFormatter.ToString(trader);

            Messenger.SendGroup(group, msg + $"\r\n机器人目前运行的平台是: {platform}");
        }

        public void SendFortunaMissions(string group, int index)
        {
            var missions = api.GetSyndicateMissions();
            var sb = new StringBuilder();
            sb.AppendLine(WFFormatter.ToString(missions.First(mission => mission.syndicate == "Solaris United"), index));
            sb.AppendLine($"您正在查看 福尔图娜 的全部赏金任务,使用: /地球赏金 来查询希图斯的赏金任务.");
            sb.AppendLine($"机器人目前运行的平台是: {platform}");
            Messenger.SendGroup(group, sb.ToString());
        }

        public void SendCetusMissions(string group, int index)
        {
            var missions = api.GetSyndicateMissions();
            var sb = new StringBuilder();
            sb.AppendLine(WFFormatter.ToString(missions.First(mission => mission.syndicate == "Ostrons"), index));
            sb.AppendLine("您正在查看 希图斯 的全部赏金任务,使用: /金星赏金 来查询 福尔图娜 的赏金任务.");
            sb.AppendLine($"机器人目前运行的平台是: {platform}");
            Messenger.SendGroup(group, sb.ToString());
        }

        //public void SendFissures(string group, List<string> words)
        public void SendFissures(string group)
        {
            var fissures = api.GetFissures().Where(fissure => fissure.active).ToList();
            var msg = $"{WFFormatter.ToString(fissures)}\r\n机器人目前运行的平台是: {platform}";
            Messenger.SendGroup(group, msg);
        }

        public void SendRelicInfo(string group, string word)
        {
            var relics = translator.GetRelicInfo(word);
            var msg = $"{WFFormatter.ToString(relics)}\r\n" +
                      $"你正在查看与 {word} 有关的所有遗物." +
                      $"\r\n机器人目前运行的平台是: {platform}";

            Messenger.SendGroup(group, msg);
        }

        public void SendEvent(string group)
        {
            var events = api.GetEvents();
            if (events.Count > 0)
            {
                var msg = WFFormatter.ToString(events);
                Messenger.SendGroup(group, msg + $"\r\n机器人目前运行的平台是: {platform}");
            }
            else
            {
                Messenger.SendGroup(group, "目前游戏内没有任何活动(尸鬼,豺狼,舰队)." + $"\r\n机器人目前运行的平台是: {platform}");
            }
        }

        public void SendTranslateResult(string group, string str)
        {
            var msg = translator.GetTranslateResult(str);
            Messenger.SendGroup(group, msg);
        }
    }
}
