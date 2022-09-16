using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Humanizer;
using WFBot.Events;
using WFBot.Features.Commands;
using WFBot.Features.Common;
using WFBot.Features.Telemetry;
using WFBot.Features.Utils;
using WFBot.Orichalt;
using WFBot.TextCommandCore;
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

            var handler = new CommandsHandler(o, message);
            
            // TODO 优化task数量
            // TODO cancellation token
            return Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                var cancelSource = new CancellationTokenSource();
                AsyncContext.SetCancellationToken(cancelSource.Token);
                AsyncContext.SetOrichaltContext(o);
                var commandProcessTask = handler.ProcessCommandInput();
                var platforminfo = o.GetInfo();
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

                var result = "";
                if (handler.OutputStringBuilder.IsValueCreated)
                {
                    var s = handler.OutputStringBuilder.ToString().Trim();
                    result = s;
                    MiguelNetwork.Reply(o, s);
                }

                if (!commandProcessTask.Result.result.IsNullOrWhiteSpace())
                {
                    result = commandProcessTask.Result.result;
                }

                if (commandProcessTask.Result.matched)
                {
                    Interlocked.Increment(ref WFBotCore.InstanceCommandsProcessed);
#if !DEBUG
                    TelemetryClient.ReportCommand(new CommandReport(o.GetGroupIdentifier().AnonymizeString(),o.GetSenderIdentifier().AnonymizeString() ,o.PlainMessage, result, DateTime.Now, sw.Elapsed.TotalSeconds.ToString("F1")+"s", TelemetryClient.ClientID));
#endif
                    Trace.WriteLine($"命令 {platforminfo} 处理完成: {sw.Elapsed.Seconds:N1}s.");
                }


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
