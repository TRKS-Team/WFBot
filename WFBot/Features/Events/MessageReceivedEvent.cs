using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using TextCommandCore;
using WFBot.Events;
using WFBot.Features.Commands;
using WFBot.Features.Common;
using WFBot.Features.Utils;
using WFBot.Utils;
using static WFBot.Features.Utils.Messenger;

namespace WFBot.Features.Events
{
    /// <summary>
    /// 群消息接收事件
    /// </summary>
    public class MessageReceivedEvent
    {
        int commandCount;
        bool showedSlashTip = false;

        public Task ProcessGroupMessage(GroupID groupId, UserID senderId, string message)
        {
            // 检查每分钟最大调用
            if (CheckCallPerMin(groupId)) return Task.CompletedTask;

            // 处理以 '/' 开头的消息
            RunAutoReply(groupId, message);
            if (Config.Instance.IsSlashRequired && !message.StartsWith('/'))
            {
                if (!showedSlashTip)
                {
                    Trace.WriteLine("提示: 设置中要求命令必须以 / 开头. ");
                    showedSlashTip = true;
                }
                return Task.CompletedTask;
            }
            message = message.TrimStart('/', '、', '／');

            var handler = new CommandsHandler(senderId, groupId, message);
            
            // TODO 优化task数量
            // TODO cancellation token
            return Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                var cancelSource = new CancellationTokenSource();
                AsyncContext.SetCancellationToken(cancelSource.Token);
                IGroupMessageSender sender;
                if (WFBotCore.UseTestConnector)
                {
                    sender = AsyncContext.GetMessageSender();
                }
                else
                {                 
                    sender = new GroupMessageSender(groupId);
                    AsyncContext.SetMessageSender(sender);
                    
                }
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

                if (handler.OutputStringBuilder.IsValueCreated)
                {
                    sender.SendMessage(handler.OutputStringBuilder.ToString().Trim());
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


}
