using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TextCommandCore;
using WFBot.Features.Common;
using WFBot.Features.Other;
using WFBot.Features.Utils;
using WFBot.Utils;
using static WFBot.Features.Utils.Messenger;

namespace WFBot.Events
{
    /// <summary>
    /// 群消息接收事件
    /// </summary>
    public class MessageReceivedEvent
    {
        int commandCount;
        public void ProcessGroupMessage(GroupID groupId, UserID senderId, string message)
        {
            // 检查每分钟最大调用
            if (CheckCallPerMin(groupId)) return;

            // 处理以 '/' 开头的消息
            if (Config.Instance.IsSlashRequired && !message.StartsWith("/")) return;
            message = message.TrimStart('/');

            var handler = new GroupMessageHandler(senderId, groupId, message);
            var commandProcessTask
                = Task.Factory.StartNew(() => handler.ProcessCommandInput(), TaskCreationOptions.LongRunning);
            Task.Run(async () =>
            {
                using var locker = WFBotResourceLock.Create(
                    $"命令处理 #{Interlocked.Increment(ref commandCount)} 群'{groupId}' 用户'{senderId}' 内容'{message}'");
                await Task.WhenAny(commandProcessTask, Task.Delay(TimeSpan.FromSeconds(60)));
                if (!commandProcessTask.IsCompleted)
                {
                    SendGroup(groupId, $"命令 {message} 处理超时.");
                }
            });
        }

        private static bool CheckCallPerMin(GroupID groupId)
        {
            if (GroupCallDic.ContainsKey(groupId))
            {
                if (GroupCallDic[groupId] > Config.Instance.CallperMinute && Config.Instance.CallperMinute != 0) return true;
            }
            else
            {
                GroupCallDic[groupId] = 0;
            }

            return false;
        }
    }

    public partial class GroupMessageHandler
    {
        [Matchers("金星赏金", "金星平原赏金", "福尔图娜赏金", "奥布山谷赏金")]
        void FortunaMissions(int index = 0)
        {
            _wfStatus.SendFortunaMissions(Group, index);
        }

        [Matchers("地球赏金", "地球平原赏金", "希图斯赏金")]
        void CetusMissions(int index = 0)
        {
            _wfStatus.SendCetusMissions(Group, index);
        }

        [Matchers("查询", "wm")]
        [CombineParams]
        [DoNotMeasureTime]
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

        [Matchers("紫卡", "阿罕卡拉"/*彩蛋*/, "千语魅痕"/*彩蛋*/)]
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
            _wfStatus.SendTranslateResult(Group, word);
        }

        [Matchers("遗物")]
        [CombineParams]
        void RelicInfo(string word)
        {
            word = word.Format();
            _wfStatus.SendRelicInfo(Group, word);
        }

        [Matchers("警报")]
        void Alerts()
        {
            WFBotCore.Instance.NotificationHandler.SendAllAlerts(Group);
        }

        [Matchers("平野", "夜灵平野", "平原", "夜灵平原", "金星平原", "奥布山谷", "金星平原温度", "平原温度", "平原时间")]
        void Cycles()
        {
            _wfStatus.SendCycles(Group);
        }

        [Matchers("入侵")]
        void Invasions()
        {
            WFBotCore.Instance.NotificationHandler.SendAllInvasions(Group);
        }

        [Matchers("突击")]
        void Sortie()
        {
            _wfStatus.SendSortie(Group);
        }

        [Matchers("奸商", "虚空商人", "商人")]
        void VoidTrader()
        {
            _wfStatus.SendVoidTrader(Group);
        }

        [Matchers("活动", "事件")]
        void Events()
        {
            _wfStatus.SendEvent(Group);
        }


        [Matchers("裂隙", "裂缝")]
        void Fissures(int tier = 0)
        {
            _wfStatus.SendFissures(Group, tier);
        }

        [Matchers("小小黑", "追随者")]
        void AllPersistentEnemies()
        {
            WFBotCore.Instance.NotificationHandler.SendAllPersistentEnemies(Group);
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
            _wfStatus.SendNightWave(Group);
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
            _wfStatus.SendArbitrationMission(Group);
        }

        [Matchers("赤毒", "赤毒虹吸器", "赤毒洪潮", "赤毒任务")]
        void Kuva()
        {
            _wfStatus.SendKuvaMissions(Group);
        }
        /*
        [Matchers("s船", "前哨战", "sentient", "异常", "异常事件", "sentient异常事件")]
        void SentientOutpost()
        {
            _wfStatus.SendSentientOutpost(Group);
        }*/
    }

    public partial class GroupMessageHandler : ICommandHandler<GroupMessageHandler>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; }
        public Action<Message> ErrorMessageSender { get; }

        public UserID Sender { get; }
        public string Message { get; }
        public GroupID Group { get; }


        string ICommandHandler<GroupMessageHandler>.Sender => Group;

        private static readonly WFStatus _wfStatus = new WFStatus();
        private static readonly WMSearcher _wmSearcher = new WMSearcher();
        private static readonly RMSearcher _rmSearcher = new RMSearcher();
        private static readonly WikiSearcher _wikiSearcher = new WikiSearcher();

        public GroupMessageHandler(UserID sender, GroupID group, string message)
        {
            Sender = sender;
            MessageSender = (id, msg) =>
            {
                SendGroup(id.ID, msg);
                Trace.WriteLine($"Message Processed: Group [{Group}], QQ [{Sender}], Message Content [{message}], Result [{msg.Content}].", "Message");
            };
            Group = group;
            Message = message;

            ErrorMessageSender = msg => SendDebugInfo(msg);
        }

    }
}
