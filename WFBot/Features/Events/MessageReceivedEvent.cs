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
using WFBot.Orichalt;
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

        public Task ProcessGroupMessage(OrichaltContext o)
        {
            // 检查每分钟最大调用
            // if (CheckCallPerMin(groupId)) return Task.CompletedTask;

            // 处理以 '/' 开头的消息
            RunAutoReply(o);
            if (Config.Instance.IsSlashRequired && !o.PlainMessage.StartsWith('/'))
            {
                if (!showedSlashTip)
                {
                    Trace.WriteLine("提示: 设置中要求命令必须以 / 开头. ");
                    showedSlashTip = true;
                }
                return Task.CompletedTask;
            }
            var message = o.PlainMessage.TrimStart('/', '、', '／');

            var handler = new CommandsHandler(message);
            
            // TODO 优化task数量
            // TODO cancellation token
            return Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                var cancelSource = new CancellationTokenSource();
                AsyncContext.SetCancellationToken(cancelSource.Token);
                AsyncContext.SetOrichaltContext(o);
                var commandProcessTask = handler.ProcessCommandInput();
                var platforminfo = "";
                switch (o.Platform)
                {
                    case MessagePlatform.OneBot:
                        var context = MiguelNetwork.OrichaltContextManager.OneBotContexts[o.UUID];
                        platforminfo = $"平台[OneBot] 群[{context.Group}] 用户[{context.SenderID}] 内容[{context.RawMessage}]";
                        break;
                    case MessagePlatform.Kaiheila:
                        break;
                    case MessagePlatform.QQChannel:
                        break;
                }
                using var locker = WFBotResourceLock.Create($"命令处理 #{Interlocked.Increment(ref commandCount)} {platforminfo}");
                await Task.WhenAny(commandProcessTask, Task.Delay(TimeSpan.FromSeconds(60)));
                
                if (!commandProcessTask.IsCompleted)
                {
                    cancelSource.Cancel();
                    await Task.Delay(10.Seconds());
                    if (!commandProcessTask.IsCompleted)
                    {
                        MiguelNetwork.Reply(o, $"命令 [{message}] 处理超时.");
                    }
                    Trace.WriteLine($"命令 {platforminfo} 处理超时.");
                    return;
                }

                if (handler.OutputStringBuilder.IsValueCreated)
                {
                    MiguelNetwork.Reply(o, handler.OutputStringBuilder.ToString().Trim());
                }
#if !DEBUG
                if (commandProcessTask.Result.matched)
                {
                    Trace.WriteLine($"命令 {platforminfo} 处理完成: {sw.Elapsed.Seconds:N1}s.");
                }
#endif

            });
        }

        void RunAutoReply(OrichaltContext o)
        {
            var message = o.PlainMessage.ToLowerInvariant();
            if (Config.Instance.CustomReplies.ContainsKey(message))
            { 
                MiguelNetwork.Reply(o, Config.Instance.CustomReplies[message]);
            }
        }

    }


}
