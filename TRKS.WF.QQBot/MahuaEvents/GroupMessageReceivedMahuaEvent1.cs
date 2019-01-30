using System;
using System.Web;
using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using Settings;
using TextCommandCore;
using Number = System.Numerics.BigInteger;
using static TRKS.WF.QQBot.Messenger;

namespace TRKS.WF.QQBot.MahuaEvents
{
    /// <summary>
    /// 群消息接收事件
    /// </summary>
    public class GroupMessageReceivedMahuaEvent1
        : IGroupMessageReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;
        private static readonly object Locker = new object();
        internal static readonly WFNotificationHandler _WfNotificationHandler = new WFNotificationHandler();

        public GroupMessageReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessGroupMessage(GroupMessageReceivedContext context)
        {
            lock (Locker)
            {
                if (HotUpdateInfo.PreviousVersion) return;
                if (GroupCallDic.ContainsKey(context.FromGroup))
                {
                    if (GroupCallDic[context.FromGroup] > Config.Instance.CallperMinute && Config.Instance.CallperMinute != 0) return;
                }
                else
                {
                    GroupCallDic[context.FromGroup] = 0;
                }

                var message = HttpUtility.HtmlDecode(context.Message).ToLower();
                if (!message.StartsWith("/") && Config.Instance.IsSlashRequired) return;

                message = message.StartsWith("/") ? message.Substring(1) : message;

                var handler = new GroupMessageHandler(context.FromQq, context.FromGroup);
                handler.ProcessCommandInput(context.FromGroup, message);
            }
        }
    }

    public partial class GroupMessageHandler
    {
        [Matchers("金星赏金", "金星平原赏金", "福尔图娜赏金", "奥布山谷赏金")]
        void FortunaMissions(int index = 0)
        {
            _WFStatus.SendFortunaMissions(Group, index);
        }

        [Matchers("地球赏金", "地球平原赏金", "希图斯赏金")]
        void CetusMissions(int index = 0)
        {
            _WFStatus.SendCetusMissions(Group, index);
        }

        [Matchers("查询")]
        [CombineParams]
        void WM(string word)
        {
            word = word.Format();
            _wmSearcher.SendWMInfo(word, Group);
        }

        [Matchers("紫卡")]
        [CombineParams]
        void Riven(string word)
        {
            word = word.Format();
            _rmSearcher.SendRiveninfos(Group, word);
        }

        [Matchers("翻译")]
        [CombineParams]
        void Translate(string word)
        {
            word = word.Format();
            _WFStatus.SendTranslateResult(Group, word);
        }

        [Matchers("遗物")]
        [CombineParams]
        void RelicInfo(string word)
        {
            word = word.Format();
            _WFStatus.SendRelicInfo(Group, word);
        }

        [Matchers("警报")]
        void Alerts()
        {
            WfNotificationHandler.SendAllAlerts(Group);
        }

        [Matchers("平野", "夜灵平野", "平原", "夜灵平原", "金星平原", "奥布山谷", "金星平原温度", "平原温度", "平原时间")]
        void Cycles()
        {
            _WFStatus.SendCycles(Group);
        }

        [Matchers("入侵")]
        void Invasions()
        {
            WfNotificationHandler.SendAllInvasions(Group);
        }

        [Matchers("突击")]
        void Sortie()
        {
            _WFStatus.SendSortie(Group);
        }

        [Matchers("奸商", "虚空商人", "商人")]
        void VoidTrader()
        {
            _WFStatus.SendVoidTrader(Group);
        }

        [Matchers("活动", "事件")]
        void Events()
        {
            _WFStatus.SendEvent(Group);
        }


        [Matchers("裂隙", "裂缝")]
        void Fissures()
        {
            _WFStatus.SendFissures(Group);
        }

        [Matchers("小小黑", "追随者", "焦虑", "怨恨", "躁狂", "苦难", "折磨", "暴力")]
        void AllPersistentEnemies()
        {
            WfNotificationHandler.SendAllPersistentEnemies(Group);
        }

        [Matchers("help", "帮助", "功能", "救命")]
        void HelpDoc()
        {
            SendHelpdoc(Group);
        }
    }

    public partial class GroupMessageHandler : ICommandHandlerCollection<GroupMessageHandler>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; } = (id, msg) => SendGroup(id, msg);
        public Action<Message> ErrorMessageSender { get; } = msg => SendDebugInfo(msg);
        public string Sender { get; }
        public string Group { get; }

        internal static WFNotificationHandler WfNotificationHandler =>
            GroupMessageReceivedMahuaEvent1._WfNotificationHandler;

        private static readonly WFStatus _WFStatus = new WFStatus();
        private static readonly WMSearcher _wmSearcher = new WMSearcher();
        private static readonly RMSearcher _rmSearcher = new RMSearcher();

        public GroupMessageHandler(string sender, string group)
        {
            Sender = sender;
            Group = group;
        }

    }
}
