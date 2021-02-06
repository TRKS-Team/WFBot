using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
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

        public async Task<string> Cycles()
        {
            var cetuscycle = api.GetCetusCycle();
            var valliscycle = api.GetVallisCycle();
            var earthcycle = api.GetEarthCycle();
            var cambioncycle = api.GetCambionCycle();
            var msg = $"{WFFormatter.ToString(await cetuscycle)}\n" +
                    $"{WFFormatter.ToString(await valliscycle)}\n" +
                    $"{WFFormatter.ToString(await earthcycle)}\n" +
                    $"{WFFormatter.ToString(await cambioncycle)}";

            return msg.AddPlatformInfo().AddRemainCallCount();
        }

        // public async Task<string> SendSentientOutpost()
        // {
        //     var outpost = await api.GetSentientOutpost();
        //     var msg = $"Sentient异常事件已发现:\n{WFFormatter.ToString(outpost)}";
        //
        //     return msg;
        // }
        public async Task<string> SendSortie()
        {
            var sortie = await api.GetSortie();
            var msg = WFFormatter.ToString(sortie);
            
            return msg.AddPlatformInfo().AddRemainCallCount();
        }

        public async Task<string> SendVoidTrader()
        {
            var trader = api.GetVoidTrader();
            var msg = WFFormatter.ToString(await trader);

            return msg.AddPlatformInfo().AddRemainCallCount();
        }

        public async Task<string> SendFortunaMissions(int index)
        {
            var missions = await api.GetSyndicateMissions();
            var sb = new StringBuilder();
            sb.AppendLine(WFFormatter.ToString(missions.First(mission => mission.syndicate == "Solaris United"), index));
            sb.AppendLine();
            sb.Append($"您正在查看 福尔图娜 的全部赏金任务, 使用: /地球赏金 来查询希图斯的赏金任务.");
            return sb.ToString().AddPlatformInfo().AddRemainCallCount();
        }

        public async Task<string> SendCetusMissions(int index)
        {
            var missions = await api.GetSyndicateMissions();
            var sb = new StringBuilder();
            sb.AppendLine(WFFormatter.ToString(missions.First(mission => mission.syndicate == "Ostrons"), index));
            sb.AppendLine();
            sb.Append("您正在查看 希图斯 的全部赏金任务, 使用: /金星赏金 来查询 福尔图娜 的赏金任务.");
            return sb.ToString().AddPlatformInfo().AddRemainCallCount();
        }

        //public void SendFissures(string group, List<string> words)
        public async Task<string> SendFissures(int tier)
        {
            // var fissures = _fissures.Where(fissure => fissure.active).ToList();
            var fissures = (await api.GetFissures()).Where(fissure => fissure.active).ToList();
            var msg = WFFormatter.ToString(fissures, tier);
            return msg.AddPlatformInfo().AddRemainCallCount();
            // _fissures = api.GetFissures();
        }

        public string SendRelicInfo(string word)
        {
            var relic = api.GetRelics(word);
            if (relic.IsEmpty())
            {
                return "没有找到符合条件的遗物.";
            }
            else
            {
                var msg = WFFormatter.ToString(relic);
                return msg;
            }
        }

        public async Task<string> SendEvent()
        {
            var events = await api.GetEvents();
            if (events.Any())
            {
                var msg = WFFormatter.ToString(events);
                return msg.AddPlatformInfo().AddRemainCallCount();
            }
            else
            {
                return "目前游戏内没有任何活动 (尸鬼, 豺狼, 舰队).".AddPlatformInfo().AddRemainCallCount();
            }
        }

        public async Task<string> SendNightWave()
        {
            var nightwave = await api.GetNightWave();
            var msg = WFFormatter.ToString(nightwave).AddPlatformInfo().AddRemainCallCount();
            return msg;
        }

        public string SendTranslateResult(string str)
        {
            var msg = translator.GetTranslateResult(str).AddRemainCallCount();
            return msg;
        }

        public async Task<string> SendKuvaMissions()
        {
            var sb = new StringBuilder();
            var kuvas = await api.GetKuvaMissions();
            sb.AppendLine("以下是所有赤毒任务: \n\n");
            // foreach (var kuva in kuvas.Where(k => k.missiontype.Contains("KuvaMission") && k.start < DateTime.Now && DateTime.Now < k.end))
            foreach (var kuva in kuvas)
            {
                sb.AppendLine(WFFormatter.ToString(kuva));
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        public async Task<string> SendArbitrationMission()
        {
            var sb = new StringBuilder();
            var ar = await api.GetArbitrationMission();
            // var mission = kuvas.First(k => k.missiontype == "EliteAlertMission" && k.start < DateTime.Now && DateTime.Now < k.end);
            sb.AppendLine("以下是仲裁警报的信息: ");
            sb.AppendLine(WFFormatter.ToString(ar));
            return sb.ToString().Trim();
        }
    }
}
