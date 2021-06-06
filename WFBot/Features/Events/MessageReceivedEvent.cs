﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Humanizer;
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
        bool showedSlashTip = false;

        public void ProcessGroupMessage(GroupID groupId, UserID senderId, string message)
        {
            // 检查每分钟最大调用
            if (CheckCallPerMin(groupId)) return;

            // 处理以 '/' 开头的消息
            RunAutoReply(groupId, message);
            if (Config.Instance.IsSlashRequired && !message.StartsWith('/'))
            {
                if (!showedSlashTip)
                {
                    Trace.WriteLine("提示: 设置中要求命令必须以 / 开头. ");
                    showedSlashTip = true;
                }
                return;
            }
            message = message.TrimStart('/', '、', '／');

            var handler = new GroupMessageHandler(senderId, groupId, message);
            
            // TODO 优化task数量
            // TODO cancellation token
            Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                var cancelSource = new CancellationTokenSource();
                AsyncContext.SetCancellationToken(cancelSource.Token);
                var sender = new GroupMessageSender(groupId);
                AsyncContext.SetMessageSender(sender);
                var commandProcessTask = handler.ProcessCommandInput();

                using var locker = WFBotResourceLock.Create(
                    $"命令处理 #{Interlocked.Increment(ref commandCount)} 群[{groupId}] 用户[{senderId}] 内容[{message}]");
                await Task.WhenAny(commandProcessTask, Task.Delay(TimeSpan.FromSeconds(60)));
                
                if (!commandProcessTask.IsCompleted)
                {
                    cancelSource.Cancel();
                    await Task.Delay(10.Seconds());
                    if (!commandProcessTask.IsCompleted)
                    {
                        sender.SendMessage($"命令 [{message}] 处理超时.");
                    }
                    Trace.WriteLine($"命令 群[{groupId}] 用户[{senderId}] 内容[{message}] 处理超时.");
                    return;
                }
#if !DEBUG
                if (commandProcessTask.Result.matched)
                {
                    Trace.WriteLine($"命令 群 [{groupId}] 用户 [{senderId}] 内容 [{message}] 处理完成: {sw.Elapsed.Seconds:N1}s.");
                }
#endif

            });
        }

        void RunAutoReply(GroupID groupId, string message)
        {
            message = message.ToLowerInvariant();
            if (Config.Instance.CustomReplies.ContainsKey(message))
            {
                Config.Instance.CustomReplies[message].SendToGroup(groupId);
            }
        }

        private static bool CheckCallPerMin(GroupID groupId)
        {
            lock (GroupCallDic)
            {
                if (GroupCallDic.ContainsKey(groupId))
                {
                    if (GroupCallDic[groupId] > Config.Instance.CallperMinute && Config.Instance.CallperMinute != 0) return true;
                }
                else
                {
                    GroupCallDic[groupId] = 0;
                }

            }

            return false;
        }
    }

    public partial class GroupMessageHandler
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

        [Matchers("查询", "wm", "Wm", "WM", "wM")]
        [CombineParams]
        [DoNotMeasureTime]
        Task<string> WM(string word)
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
            return _wmSearcher.SendWMInfo(word.Replace(quickReply, "").Replace(quickReply.ToLower(), "").Replace(buyer, "").Replace(buyer.ToLower(), "").Format(), QR, B);
        }

        [Matchers("紫卡")]
        [CombineParams]
        [DoNotMeasureTime]
        Task<string> Riven(string word)
        {
            word = word.Format();
            return _rmSearcher.SendRivenInfos(word);
        }

        [Matchers("翻译", "trans", "TRANS", "Trans")]
        [CombineParams]
        string Translate(string word)
        {
            word = word.Format();
            return _wfStatus.SendTranslateResult(word);
        }

        /*[Matchers("wiki")]
        [CombineParams]
        string Wiki(string word = "wiki")
        {
            return _wikiSearcher.SendSearch(word).Replace("'", "%27");
            // 这简直就是官方吞mod最形象的解释
        }*/

        [Matchers("遗物", "核桃")]
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

        [Matchers("地球平原", "平野", "夜灵平野", "平原", "夜灵平原", "金星平原", "奥布山谷", "金星平原温度", "火卫二平原", "魔胎之境", "平原温度", "平原时间")]
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

        [Matchers("奸商", "虚空商人")]
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

        [Matchers("磨弓救救我", "磨弓帮帮我", "磨弓救救我！", "磨弓帮帮我！", "磨弓救救我!", "磨弓帮帮我!"/*"help", "帮助", "功能", "救命"*/)]
        void HelpDoc()
        {
            SendHelpdoc();
        }

        [DoNotMeasureTime]
        [Matchers("磨弓状态" /*"status", "状态", "机器人状态", "机器人信息", "我需要机器人"*/)]
        Task<string> Status()
        {
            return SendBotStatus();
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

        [Matchers("赤毒虹吸器", "赤毒洪潮", "赤毒任务")]
        Task<string> Kuva()
        {
            return _wfStatus.SendKuvaMissions();
        }
        
        [Matchers("s船状态","S船状态")]
        Task<string> SentientOutpost()
        {
            return _wfStatus.SendSentientOutpost();
        }

        
    }

    public partial class GroupMessageHandler : ICommandHandler<GroupMessageHandler>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; }
        public Action<Message> ErrorMessageSender { get; }

        public UserID Sender { get; }
        public string Message { get; }
        public GroupID Group { get; }
        GroupMessageSender MsgSender => new GroupMessageSender(Group);

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
            };
            Group = group;
            Message = message;

            ErrorMessageSender = msg => SendDebugInfo(msg);
        }

    }
}
