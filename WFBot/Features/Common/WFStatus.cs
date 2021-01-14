using System.Linq;
using System.Text;
using WFBot.Features.Resource;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Features.Common
{

    public class WFStatus
    {
        private static WFChineseAPI api => WFResources.WFChineseApi;
        private static WFTranslator translator => WFResources.WFTranslator;
        private string platform => Config.Instance.Platform.ToString();
        // private List<Fissure> _fissures = api.GetFissures();

        public void SendCycles(GroupID group)
        {
            var cetuscycle = api.GetCetusCycle();
            var valliscycle = api.GetVallisCycle();
            var earthcycle = api.GetEarthCycle();
            var cambioncycle = api.GetCambionCycle();
            var msg = $"{WFFormatter.ToString(cetuscycle)}\r\n" +
                    $"{WFFormatter.ToString(valliscycle)}\r\n" +
                    $"{WFFormatter.ToString(earthcycle)}\r\n" +
                    $"{WFFormatter.ToString(cambioncycle)}";

            Messenger.SendGroup(group, msg.AddPlatformInfo().AddRemainCallCount(group));
        }

        public void SendSentientOutpost(GroupID group)
        {
  
            var outpost = api.GetSentientOutpost();
            var msg = $"Sentient异常事件已发现:\r\n{WFFormatter.ToString(outpost)}";

            Messenger.SendGroup(group, msg);
        }
        public void SendSortie(GroupID group)
        {
            var sortie = api.GetSortie();
            var msg = WFFormatter.ToString(sortie);
            
            Messenger.SendGroup(group, msg.AddPlatformInfo().AddRemainCallCount(group));
        }

        public void SendVoidTrader(GroupID group)
        {
            var trader = api.GetVoidTrader();
            var msg = WFFormatter.ToString(trader);

            Messenger.SendGroup(group, msg.AddPlatformInfo().AddRemainCallCount(group));
        }

        public void SendFortunaMissions(GroupID group, int index)
        {
            var missions = api.GetSyndicateMissions();
            var sb = new StringBuilder();
            sb.AppendLine(WFFormatter.ToString(missions.First(mission => mission.syndicate == "Solaris United"), index));
            sb.AppendLine();
            sb.Append($"您正在查看 福尔图娜 的全部赏金任务, 使用: /地球赏金 来查询希图斯的赏金任务.");
            Messenger.SendGroup(group, sb.ToString().AddPlatformInfo().AddRemainCallCount(group));
        }

        public void SendCetusMissions(GroupID group, int index)
        {
            var missions = api.GetSyndicateMissions();
            var sb = new StringBuilder();
            sb.AppendLine(WFFormatter.ToString(missions.First(mission => mission.syndicate == "Ostrons"), index));
            sb.AppendLine();
            sb.Append("您正在查看 希图斯 的全部赏金任务, 使用: /金星赏金 来查询 福尔图娜 的赏金任务.");
            Messenger.SendGroup(group, sb.ToString().AddPlatformInfo().AddRemainCallCount(group));
        }

        //public void SendFissures(string group, List<string> words)
        public void SendFissures(GroupID group, int tier)
        {
            // var fissures = _fissures.Where(fissure => fissure.active).ToList();
            var fissures = api.GetFissures().Where(fissure => fissure.active).ToList();
            var msg = WFFormatter.ToString(fissures, tier);
            Messenger.SendGroup(group, msg.AddPlatformInfo().AddRemainCallCount(group));
            // _fissures = api.GetFissures();
        }

        public void SendRelicInfo(GroupID group, string word)
        {
            var relic = api.GetRelics(word);
            var msg = WFFormatter.ToString(relic);
            Messenger.SendGroup(group, msg);
        }

        public void SendEvent(GroupID group)
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

        public void SendNightWave(GroupID group)
        {
            var nightwave = api.GetNightWave();
            var msg = WFFormatter.ToString(nightwave).AddPlatformInfo().AddRemainCallCount(group);
            Messenger.SendGroup(group, msg);
        }

        public void SendTranslateResult(GroupID group, string str)
        {
            var msg = translator.GetTranslateResult(str).AddRemainCallCount(group);
            Messenger.SendGroup(group, msg);
        }

        public void SendKuvaMissions(GroupID group)
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

        public void SendArbitrationMission(GroupID group)
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
