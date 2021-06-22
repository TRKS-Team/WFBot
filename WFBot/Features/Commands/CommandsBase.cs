using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TextCommandCore;
using WFBot.Events;
using WFBot.Features.Common;
using WFBot.Features.Utils;
using WFBot.Utils;
using static WFBot.Features.Utils.Messenger;

namespace WFBot.Features.Commands
{
    public partial class CommandsHandler
    {
        [Matchers("金星赏金", "金星平原赏金", "福尔图娜赏金", "奥布山谷赏金")]
        Task<string> FortunaMissions(int index = 0)
        {
            return _wfStatus.SendFortunaMissions(index);
        }

        [Matchers("地球赏金", "地球平原赏金", "希图斯赏金")]
        Task<string> CetusMissions(int index = 0)
        {
            return _wfStatus.SendCetusMissions(index);
        }

        [Matchers("火卫赏金", "火卫二赏金", "火卫平原赏金", "火卫二平原赏金", "殁世幽都赏金", "魔胎之境赏金")]
        Task<string> NecraliskMissions(int index = 0)
        {
            return _wfStatus.SendNecraliskMissions(index);
        }


        [Matchers("查询", "wm")]
        [CombineParams]
        [DoNotMeasureTime]
        Task<string> WM(string word)
        {
            const string quickReply = " -qr";
            const string buyer = " -b";
            var QR = word.ToLower().Contains(quickReply);
            var B = word.ToLower().Contains(buyer);

            // 小屎山
            return _wmSearcher.SendWMInfo(
                word.ToLower().Replace(quickReply, "")
                    .Replace(buyer, "").Format(), QR, B);
        }

        [Matchers("紫卡")]
        [CombineParams]
        [DoNotMeasureTime]
        Task<string> Riven(string word)
        {
            word = word.Format();
            return _wmaSearcher.SendRivenAuctions(word);
        }

        [Matchers("WFA紫卡", "wfa紫卡")]
        [CombineParams]
        [DoNotMeasureTime]
        Task<string> WFARiven(string word)
        {
            word = word.Format();
            return _rmSearcher.SendRivenInfos(word);
        }
        [Matchers("翻译")]
        [CombineParams]
        string Translate(string word)
        {
            word = word.Format();
            return _wfStatus.SendTranslateResult(word);
        }
        
        [Matchers("遗物")]
        [CombineParams]
        string RelicInfo(string word)
        {
            word = word.Format();
            return _wfStatus.SendRelicInfo(word);
        }

        [Matchers("警报")]
        Task<string> Alerts()
        {
            return WFBotCore.Instance.NotificationHandler.SendAllAlerts();
        }

        [Matchers("平野", "夜灵平野", "平原", "夜灵平原", "金星平原", "奥布山谷", "金星平原温度", "平原温度", "平原时间")]
        Task<string> Cycles()
        {
            return _wfStatus.Cycles();
        }

        [Matchers("入侵")]
        Task<string> Invasions()
        {
            return WFBotCore.Instance.NotificationHandler.SendAllInvasions();
        }

        [Matchers("突击")]
        Task<string> Sortie()
        {
            return _wfStatus.SendSortie();
        }

        [Matchers("奸商", "虚空商人", "商人")]
        Task<string> VoidTrader()
        {
            return _wfStatus.SendVoidTrader();
        }

        [Matchers("活动", "事件")]
        Task<string> Events()
        {
            return _wfStatus.SendEvent();
        }


        [Matchers("裂隙", "裂缝")]
        Task<string> Fissures(int tier = 0)
        {
            return _wfStatus.SendFissures(tier);
        }

        [Matchers("小小黑", "追随者")]
        string AllPersistentEnemies()
        {
            return WFBotCore.Instance.NotificationHandler.SendAllPersistentEnemies();
        }



        [Matchers("午夜电波", "电波", "每日任务", "每周任务", "每日任务", "每周挑战")]
        Task<string> NightWave()
        {
            return _wfStatus.SendNightWave();
        }

        [Matchers("仲裁", "仲裁警报", "精英警报")]
        Task<string> Arbitration()
        {
            return _wfStatus.SendArbitrationMission();
        }

        [Matchers("赤毒", "赤毒虹吸器", "赤毒洪潮", "赤毒任务")]
        Task<string> Kuva()
        {
            return _wfStatus.SendKuvaMissions();
        }

        [Matchers("s船", "前哨战", "sentient", "异常", "异常事件", "sentient异常事件")]
        Task<string> SentientOutpost()
        {
            return _wfStatus.SendSentientOutpost();
        }

        void SendMessage(string msg) => MsgSender.SendMessage(msg);
    }

    public partial class CommandsHandler : ICommandHandler<CommandsHandler>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; }
        public Action<Message> ErrorMessageSender { get; }

        public UserID Sender { get; }
        public string Message { get; }
        public GroupID Group { get; }
        GroupMessageSender MsgSender => new GroupMessageSender(Group);

        string ICommandHandler<CommandsHandler>.Sender => Group;

        private static readonly WFStatus _wfStatus = new WFStatus();
        private static readonly WMSearcher _wmSearcher = new WMSearcher();
        private static readonly RMSearcher _rmSearcher = new RMSearcher();
        private static readonly WMASearcher _wmaSearcher = new WMASearcher();
        public CommandsHandler(UserID sender, GroupID group, string message)
        {
            Sender = sender;
            MessageSender = (id, msg) =>
            {
                SendGroup(id.ID, msg);
            };
            Group = group;
            Message = message;

            ErrorMessageSender = msg => SendDebugInfo(msg);
        }

    }
}
