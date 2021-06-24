using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using TextCommandCore;
using WFBot.Features.Resource;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Features.Commands
{

    public partial class CommandsHandler
    {
        private static WFChineseAPI api => WFResources.WFChineseApi;
        private static WFTranslator translator => WFResources.WFTranslator;

        [Matchers("金星赏金", "金星平原赏金", "福尔图娜赏金", "奥布山谷赏金")]
        [AddPlatformInfoAndAddRemainCallCountToTheCommandResultAndMakeTRKSHappyByDoingSoWhatSoEver]
        async Task FortunaMissions(int index = 0)
        {
            var missions = await api.GetSyndicateMissions();
            AppendLine(WFFormatter.ToString(missions.First(mission => mission.syndicate == "Solaris United"), index));
            AppendLine();
            Append($"您正在查看 福尔图娜 的全部赏金任务, 使用 /地球赏金 /火卫二赏金 来查询其他地区.");
        }

        [Matchers("地球赏金", "地球平原赏金", "希图斯赏金")]
        [AddPlatformInfoAndAddRemainCallCountToTheCommandResultAndMakeTRKSHappyByDoingSoWhatSoEver]
        async Task CetusMissions(int index = 0)
        {
            var missions = await api.GetSyndicateMissions();
            AppendLine(WFFormatter.ToString(missions.First(mission => mission.syndicate == "Ostrons"), index));
            AppendLine();
            Append("您正在查看 希图斯 的全部赏金任务, 使用 /金星赏金 /火卫二赏金 来查询其他地区.");
        }

        [Matchers("火卫赏金", "火卫二赏金", "火卫平原赏金", "火卫二平原赏金", "殁世幽都赏金", "魔胎之境赏金")]
        [AddPlatformInfoAndAddRemainCallCountToTheCommandResultAndMakeTRKSHappyByDoingSoWhatSoEver]
        async Task NecraliskMissions(int index = 0)
        {
            var missions = await api.GetSyndicateMissions();
            Append(WFFormatter.ToString(missions.First(mission => mission.syndicate == "Entrati"), index));
            AppendLine();
            AppendLine("您正在查看 殁世幽都 的全部赏金任务, 使用 /金星赏金 /地球赏金 来查询其他地区.");
        }

        [Matchers("翻译")]
        [CombineParams]
        [AddRemainCallCount]
        string Translate(string word)
        {
            return translator.GetTranslateResult(word.Format());
        }

        [Matchers("遗物")]
        [CombineParams]
        string RelicInfo(string word)
        {
            var relic = api.GetRelics(word.Format());
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

        [Matchers("平野", "夜灵平野", "平原", "夜灵平原", "金星平原", "奥布山谷", "金星平原温度", "平原温度", "平原时间")]
        [AddPlatformInfoAndAddRemainCallCountToTheCommandResultAndMakeTRKSHappyByDoingSoWhatSoEver]
        async Task Cycles()
        {
            var cetuscycle = api.GetCetusCycle();
            var valliscycle = api.GetVallisCycle();
            var earthcycle = api.GetEarthCycle();
            var cambioncycle = api.GetCambionCycle();
            AppendLine(WFFormatter.ToString(await cetuscycle));
            AppendLine(WFFormatter.ToString(await valliscycle));
            AppendLine(WFFormatter.ToString(await earthcycle));
            AppendLine(WFFormatter.ToString(await cambioncycle));
        }

        [Matchers("突击")]
        [AddPlatformInfoAndAddRemainCallCountToTheCommandResultAndMakeTRKSHappyByDoingSoWhatSoEver]
        async Task<string> Sortie()
        {
            return WFFormatter.ToString(await api.GetSortie());
        }

        [Matchers("奸商", "虚空商人", "商人")]
        [AddPlatformInfoAndAddRemainCallCountToTheCommandResultAndMakeTRKSHappyByDoingSoWhatSoEver]
        async Task<string> VoidTrader()
        {
            return WFFormatter.ToString(await api.GetVoidTrader());
        }

        [Matchers("活动", "事件")]
        [AddPlatformInfoAndAddRemainCallCountToTheCommandResultAndMakeTRKSHappyByDoingSoWhatSoEver]
        async Task<string> Events()
        {
            var events = await api.GetEvents();
            if (events.Any())
            {
                return WFFormatter.ToString(events);
            }
            else
            {
                return "目前游戏内没有任何活动 (尸鬼, 豺狼, 舰队).";
            }
        }

        [Matchers("裂隙", "裂缝")]
        [AddPlatformInfoAndAddRemainCallCountToTheCommandResultAndMakeTRKSHappyByDoingSoWhatSoEver]
        async Task<string> Fissures(int tier = 0)
        {
            // var fissures = _fissures.Where(fissure => fissure.active).ToList();
            var fissures = (await api.GetFissures()).Where(fissure => fissure.active).ToList();
            return WFFormatter.ToString(fissures, tier);
            // _fissures = api.GetFissures();
        }

        [Matchers("午夜电波", "电波", "每日任务", "每周任务", "每日任务", "每周挑战")]
        [AddPlatformInfoAndAddRemainCallCountToTheCommandResultAndMakeTRKSHappyByDoingSoWhatSoEver]
        async Task<string> NightWave()
        {
            return WFFormatter.ToString(await api.GetNightWave());
        }

        [Matchers("仲裁", "仲裁警报", "精英警报")]
        async Task Arbitration()
        {
            var ar = await api.GetArbitrationMission();
            // var mission = kuvas.First(k => k.missiontype == "EliteAlertMission" && k.start < DateTime.Now && DateTime.Now < k.end);
            AppendLine("以下是仲裁警报的信息: ");
            AppendLine(WFFormatter.ToString(ar));
        }

        [Matchers("赤毒", "赤毒虹吸器", "赤毒洪潮", "赤毒任务")]
        async Task Kuva()
        {
            var kuvas = await api.GetKuvaMissions();
            AppendLine("以下是所有赤毒任务: \n\n");
            // foreach (var kuva in kuvas.Where(k => k.missiontype.Contains("KuvaMission") && k.start < DateTime.Now && DateTime.Now < k.end))
            foreach (var kuva in kuvas)
            {
                AppendLine(WFFormatter.ToString(kuva));
                AppendLine();
            }
        }

        [Matchers("s船", "前哨战", "sentient", "异常", "异常事件", "sentient异常事件")]
        [AddPlatformInfoAndAddRemainCallCountToTheCommandResultAndMakeTRKSHappyByDoingSoWhatSoEver]
        async Task SentientOutpost()
        {
            var outpost = await api.GetSentientOutpost();
            AppendLine($"Sentient异常事件已发现:");
            AppendLine(WFFormatter.ToString(outpost));
        }

    }

}