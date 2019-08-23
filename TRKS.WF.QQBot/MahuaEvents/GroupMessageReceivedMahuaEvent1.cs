using System.Threading.Tasks;
using System;
using System.Diagnostics;
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
        public static readonly WFNotificationHandler _WfNotificationHandler = new WFNotificationHandler();

        public GroupMessageReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessGroupMessage(GroupMessageReceivedContext context)
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

            Task.Factory.StartNew(() =>
            {
                var message = HttpUtility.HtmlDecode(context.Message)?.ToLower();
                if (!message.StartsWith("/") && Config.Instance.IsSlashRequired) return;

                message = message.StartsWith("/") ? message.Substring(1) : message;

                var handler = new GroupMessageHandler(context.FromQq.ToHumanQQNumber(), context.FromGroup.ToGroupNumber(), message);
                var (matched, result) = handler.ProcessCommandInput();
                if (matched)
                {
                    IncreaseCallCounts(context.FromGroup);
                }

            }, TaskCreationOptions.LongRunning);
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
            const string quickReply = " -QR";
            const string buyer = " -B";
            var QR = false;
            var B = false;
            if (word.Contains(quickReply) || word.Contains(quickReply.ToLower()))
            {
                QR = true;
            }

            if (word.Contains(buyer) || word.Contains(buyer.ToLower()))
            {
                B = true;
            }
            // 小屎山
            _wmSearcher.SendWMInfo(word.Replace(quickReply, "").Replace(quickReply.ToLower(), "").Replace(buyer, "").Replace(buyer.ToLower(), "").Format(), Group, QR, B);
        } 

        [Matchers("紫卡")]
        [CombineParams]
        void Riven(string word)
        {
            word = word.Format();
            _rmSearcher.SendRivenInfos(Group, word);
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

        [Matchers("小小黑", "追随者")]
        void AllPersistentEnemies()
        {
            WfNotificationHandler.SendAllPersistentEnemies(Group);
        }

        [Matchers("help", "帮助", "功能", "救命")]
        void HelpDoc()
        {
            SendHelpdoc(Group);
        }

        [DoNotMeasureTime]
        [Matchers("status", "状态", "机器人状态", "机器人信息", "我需要机器人")]
        void Status()
        {
            SendBotStatus(Group);
        }

        [Matchers("午夜电波", "电波", "每日任务", "每周任务", "每日任务", "每周挑战")]
        void NightWave()
        {
            _WFStatus.SendNightWave(Group);
        }

        [Matchers("wiki")]
        [CombineParams]
        string Wiki(string word = "wiki")
        {
            return _wikiSearcher.SendSearch(word).Replace("'", "%27");
            // 这简直就是官方吞mod最形象的解释
        }

        [Matchers("仲裁", "仲裁警报", "精英警报")]
        void Arbitration()
        {
            _WFStatus.SendArbitrationMission(Group);
        }

        [Matchers("赤毒", "赤毒虹吸器", "赤毒洪潮", "赤毒任务")]
        void Kuva()
        {
            _WFStatus.SendKuvaMissions(Group);
        }
    }

    public partial class GroupMessageHandler : ICommandHandler<GroupMessageHandler>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; }
        public Action<Message> ErrorMessageSender { get; }

        public HumanQQNumber Sender { get; }
        public string Message { get; }
        public GroupNumber Group { get; }

        internal static WFNotificationHandler WfNotificationHandler =>
            GroupMessageReceivedMahuaEvent1._WfNotificationHandler;

        string ICommandHandler<GroupMessageHandler>.Sender => Group.QQ;

        private static readonly WFStatus _WFStatus = new WFStatus();
        private static readonly WMSearcher _wmSearcher = new WMSearcher();
        private static readonly RMSearcher _rmSearcher = new RMSearcher();
        private static readonly WikiSearcher _wikiSearcher = new WikiSearcher();

        public GroupMessageHandler(HumanQQNumber sender, GroupNumber group, string message)
        {
            _ = InitEvent1.localVersion;
            Sender = sender;
            MessageSender = (id, msg) =>
            {
                SendGroup(id.ID.ToGroupNumber(), msg);
                Trace.WriteLine($"Message Processed: Group [{Group}], QQ [{Sender}], Message Content [{message}], Result [{msg.Content}].", "Message");

            };
            Group = group;
            Message = message;

            ErrorMessageSender = msg => SendDebugInfo(msg);
        }

    }
}
