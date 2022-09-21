using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chaldene.Utils.Scaffolds;
using GammaLibrary.Extensions;
using Kook;
using Mirai_CSharp.Models;
using Sisters.WudiLib;
using WFBot.Features.ImageRendering;
using WFBot.Features.Utils;
using WFBot.Orichalt.OrichaltConnectors;
using ImageMessage = WFBot.Features.ImageRendering.ImageMessage;

namespace WFBot.Orichalt
{
    public static class MiguelNetwork
    {
        private static MessagePlatform Platform;

        public static OneBotCore OneBotCore;
        public static MiraiHTTPCore MiraiHTTPCore;
        public static MiraiHTTPV1Core MiraiHTTPV1Core;
        public static KookCore KookCore;

        public static OrichaltContextManager OrichaltContextManager;

        private static bool Inited;

        public static event EventHandler<OrichaltContext> OrichaltMessageRecived;


        public static ConcurrentDictionary<GroupID, int> OneBotGroupCallDic = new ConcurrentDictionary<GroupID, int>();
        public static ConcurrentDictionary<GroupID, int> MiraiHTTPGroupCallDic = new ConcurrentDictionary<GroupID, int>();
        public static ConcurrentDictionary<GroupID, int> MiraiHTTPV1GroupCallDic = new ConcurrentDictionary<GroupID, int>();


        public static bool CheckCallPerMin(OrichaltContext o)
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    var oneBotContext = OrichaltContextManager.GetOneBotContext(o);
                    lock (OneBotGroupCallDic)
                    {
                        if (OneBotGroupCallDic.ContainsKey(oneBotContext.Group))
                        {
                            if (OneBotGroupCallDic[oneBotContext.Group] > Config.Instance.CallperMinute - 1 && Config.Instance.CallperMinute != 0) return false;
                        }
                        else
                        {
                            OneBotGroupCallDic[oneBotContext.Group] = 0;
                        }

                    }

                    return true;
                case MessagePlatform.MiraiHTTP:
                    var miraiHTTPContext = OrichaltContextManager.GetMiraiHTTPContext(o);
                    lock (OneBotGroupCallDic)
                    {
                        if (MiraiHTTPGroupCallDic.ContainsKey(miraiHTTPContext.Group))
                        {
                            if (MiraiHTTPGroupCallDic[miraiHTTPContext.Group] > Config.Instance.CallperMinute - 1 && Config.Instance.CallperMinute != 0) return false;
                        }
                        else
                        {
                            MiraiHTTPGroupCallDic[miraiHTTPContext.Group] = 0;
                        }
                    }
                    return true;
                case MessagePlatform.MiraiHTTPV1:
                    var miraiHTTPContext1 = OrichaltContextManager.GetMiraiHTTPV1Context(o);
                    lock (OneBotGroupCallDic)
                    {
                        if (MiraiHTTPV1GroupCallDic.ContainsKey(miraiHTTPContext1.Group))
                        {
                            if (MiraiHTTPV1GroupCallDic[miraiHTTPContext1.Group] > Config.Instance.CallperMinute - 1 && Config.Instance.CallperMinute != 0) return false;
                        }
                        else
                        {
                            MiraiHTTPV1GroupCallDic[miraiHTTPContext1.Group] = 0;
                        }
                    }

                    return true;
                default:
                    return true;
            }

        }
        public static void IncreaseCallCounts(OrichaltContext o)
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    lock (OneBotGroupCallDic)
                    {
                        var oneBotContext = OrichaltContextManager.GetOneBotContext(o);
                        if (OneBotGroupCallDic.ContainsKey(oneBotContext.Group))
                        {
                            OneBotGroupCallDic[oneBotContext.Group]++;
                        }
                        else
                        {
                            OneBotGroupCallDic[oneBotContext.Group] = 1;
                        }
                    }

                    Task.Delay(TimeSpan.FromSeconds(60)).ContinueWith(task =>
                    {
                        lock (OneBotGroupCallDic)
                        {                
                            var context = OrichaltContextManager.GetOneBotContext(o);
                            var group = context.Group;
                            OneBotGroupCallDic[group]--;
                        }
                    });
                    break;
                case MessagePlatform.MiraiHTTP:
                    lock (MiraiHTTPGroupCallDic)
                    {
                        var miraiHTTPContext = OrichaltContextManager.GetMiraiHTTPContext(o);
                        if (MiraiHTTPGroupCallDic.ContainsKey(miraiHTTPContext.Group))
                        {
                            MiraiHTTPGroupCallDic[miraiHTTPContext.Group]++;
                        }
                        else
                        {
                            MiraiHTTPGroupCallDic[miraiHTTPContext.Group] = 1;
                        }
                    }
                    Task.Delay(TimeSpan.FromSeconds(60)).ContinueWith(task =>
                    {
                        lock (MiraiHTTPGroupCallDic)
                        {                
                            var context = OrichaltContextManager.GetMiraiHTTPContext(o);
                            var group = context.Group;
                            MiraiHTTPGroupCallDic[group]--;
                        }
                    });
                    break;
                case MessagePlatform.MiraiHTTPV1:
                    lock (MiraiHTTPV1GroupCallDic)
                    {
                        var miraiHTTPContext = OrichaltContextManager.GetMiraiHTTPV1Context(o);
                        if (MiraiHTTPV1GroupCallDic.ContainsKey(miraiHTTPContext.Group))
                        {
                            MiraiHTTPV1GroupCallDic[miraiHTTPContext.Group]++;
                        }
                        else
                        {
                            MiraiHTTPV1GroupCallDic[miraiHTTPContext.Group] = 1;
                        }
                    }
                    Task.Delay(TimeSpan.FromSeconds(60)).ContinueWith(task =>
                    {
                        lock (MiraiHTTPV1GroupCallDic)
                        {
                            var context = OrichaltContextManager.GetMiraiHTTPV1Context(o);
                            var group = context.Group;
                            MiraiHTTPV1GroupCallDic[group]--;
                        }
                    });
                    break;
            }

        }
        private static void OnOrichaltMessageRecived(OrichaltContext e)
        {
            OrichaltMessageRecived?.Invoke(null, e);
        }

        public static void InitMiguelNetwork(MessagePlatform platform)
        {
            if (Inited)
            {
                return;
            }
            Platform = platform;
            OrichaltContextManager = new OrichaltContextManager();
            switch (Platform)
            {
                case MessagePlatform.OneBot:
                    OneBotCore = new OneBotCore();
                    OneBotCore.OneBotMessageReceived += OneBotMessageReceived;
                    OneBotCore.Init().Wait();
                    break;
                case MessagePlatform.MiraiHTTP:
                    MiraiHTTPCore = new MiraiHTTPCore();
                    MiraiHTTPCore.MiraiHTTPMessageReceived += MiraiHTTPMessageReceived;
                    MiraiHTTPCore.Init().Wait();
                    break;
                case MessagePlatform.MiraiHTTPV1:
                    MiraiHTTPV1Core = new MiraiHTTPV1Core();
                    MiraiHTTPV1Core.MiraiHTTPMessageReceived += MiraiHTTPV1MessageReceived;
                    MiraiHTTPV1Core.Init().Wait();
                    break;
                case MessagePlatform.Kook:
                    KookCore = new KookCore();
                    KookCore.KookMessageReceived += KookMessageReceived;
                    break;
                case MessagePlatform.QQChannel:
                    break;
                case MessagePlatform.Test:
                    break;
                case MessagePlatform.Unknown:
                    break;
            }

            OrichaltMessageRecived += MiguelNetwork_OrichaltMessageRecived;
            Inited = true;
        }

        private static void KookMessageReceived(object sender, KookContext e)
        {
            var o = OrichaltContextManager.PutPlatformContext(e);
            OnOrichaltMessageRecived(o);
        }

        private static void MiguelNetwork_OrichaltMessageRecived(object sender, OrichaltContext e)
        {
            switch (e.Scope)
            {
                case MessageScope.Public:
                    _ = WFBotCore.Instance.OnGroupMessage(e);
                    break;
                case MessageScope.Private:
                    WFBotCore.Instance.OnFriendMessage(e);
                    break;
            }
        }

        private static void OneBotMessageReceived(object sender, OneBotContext e)
        {
            var o = OrichaltContextManager.PutPlatformContext(e);
            OnOrichaltMessageRecived(o);
        }

        private static void MiraiHTTPMessageReceived(object sender, MiraiHTTPContext e)
        {
            var o = OrichaltContextManager.PutPlatformContext(e);
            OnOrichaltMessageRecived(o);
        }

        static void MiraiHTTPV1MessageReceived(object sender, MiraiHTTPV1Context e)
        {
            var o = OrichaltContextManager.PutPlatformContext(e);
            OnOrichaltMessageRecived(o);
        }

        public static async Task ProcessTestInput(string input)
        {
            var o = OrichaltContextManager.PutPlatformContext(new TestContext { RawMessage = input });
            await WFBotCore.Instance.OnGroupMessage(o);
        }

        /// <summary>
        /// 响应通用命令应答
        /// </summary>
        /// <param name="o">OrichaltContext</param>
        /// <param name="msg">消息内容</param>
        public static void Reply(OrichaltContext o, RichMessages msg)
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    if (CheckCallPerMin(o))
                    {
                        var oneBotContext = OrichaltContextManager.GetOneBotContext(o);
                        _ = OneBotSendToGroupWithAutoRevoke(oneBotContext.Group, msg);
                        IncreaseCallCounts(o);
                    }
                    break;
                case MessagePlatform.Kook:
                    break;
                case MessagePlatform.QQChannel:
                    break;
                case MessagePlatform.MiraiHTTP:
                    if (CheckCallPerMin(o))
                    {
                        var miraiHTTPContext = OrichaltContextManager.GetMiraiHTTPContext(o);
                        MiraiHTTPSendToGroupWithAutoRevoke(miraiHTTPContext.Group, msg);
                        IncreaseCallCounts(o);
                    }
                    break;
                case MessagePlatform.MiraiHTTPV1:
                    break;


                case MessagePlatform.Test:
                    const string resultPath = "TestResult.log";
                    Trace.WriteLine(msg.OfType<TextMessage>().Select(x => x.Content).Connect());

                    if (File.Exists(resultPath) && File.ReadLines(resultPath).Last() == "Done.") // 哈哈 Trick.
                    {
                        File.Delete(resultPath);
                    }
                    File.AppendAllText(resultPath, msg + Environment.NewLine);
                    break;
            }
        }

        /// <summary>
        /// 响应通用命令应答
        /// </summary>
        /// <param name="o">OrichaltContext</param>
        /// <param name="msg">消息内容</param>
        public static void Reply(OrichaltContext o, string msg)
        {
            if (msg.IsNullOrWhiteSpace()) return;
            
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    if (CheckCallPerMin(o))
                    {
                        var oneBotContext = OrichaltContextManager.GetOneBotContext(o); 
                        _ = OneBotSendToGroupWithAutoRevoke(oneBotContext.Group, msg);
                        IncreaseCallCounts(o);
                    }
                    break;
                case MessagePlatform.Kook:
                    var kookcontext = OrichaltContextManager.GetKookContext(o);
                    ReplyKookChannelUser(msg, kookcontext);
                    break;
                case MessagePlatform.QQChannel:
                    break;
                case MessagePlatform.MiraiHTTP:
                    if (CheckCallPerMin(o))
                    {
                        var miraiHTTPContext = OrichaltContextManager.GetMiraiHTTPContext(o);
                        MiraiHTTPSendToGroupWithAutoRevoke(miraiHTTPContext.Group, msg);
                        IncreaseCallCounts(o);
                    }
                    break;
                case MessagePlatform.MiraiHTTPV1:
                    if (CheckCallPerMin(o))
                    {
                        var miraiHTTPContext = OrichaltContextManager.GetMiraiHTTPV1Context(o);
                        MiraiHTTPV1SendToGroup(miraiHTTPContext.Group, msg);
                        IncreaseCallCounts(o);
                    }
                    break;


                case MessagePlatform.Test:
                    const string resultPath = "TestResult.log";
                    Trace.WriteLine(msg);
                    if (File.Exists(resultPath) && File.ReadLines(resultPath).Last() == "Done.") // 哈哈 Trick.
                    {
                        File.Delete(resultPath);
                    }
                    File.AppendAllText(resultPath, msg + Environment.NewLine);
                    break;
            }
        }
        /// <summary>
        /// 响应私聊命令应答
        /// </summary>
        /// <param name="o">OrichaltContext</param>
        /// <param name="msg">消息内容</param>
        public static void PrivateReply(OrichaltContext o, string msg)
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    var onebotcontext = OrichaltContextManager.GetOneBotContext(o);
                    OneBotSendToPrivate(onebotcontext.SenderID, msg);
                    break;
                case MessagePlatform.MiraiHTTP:
                    var miraihttpcontext = OrichaltContextManager.GetMiraiHTTPContext(o);
                    MiraiHTTPSendToPrivate(miraihttpcontext.SenderID, msg);
                    break;
                case MessagePlatform.MiraiHTTPV1:
                    var miraihttpcontext1 = OrichaltContextManager.GetMiraiHTTPV1Context(o);
                    MiraiHTTPV1SendToPrivate(miraihttpcontext1.SenderID, msg);
                    break;
                case MessagePlatform.Kook:
                    break;

            }
        }
        /// <summary>
        /// 发送通用Debug信息给管理者
        /// </summary>
        /// <param name="msg">消息内容</param>
        public static void SendDebugInfo(string msg)
        {
            if (Config.Instance.QQ.IsNullOrWhiteSpace())
            {
                return;
            }
            if (!Inited)
            {
                Console.WriteLine("正在等待 Miguel Network 初始化完成...");
                SpinWait.SpinUntil(() => Inited);
            }
            switch (Platform)
            {
                case MessagePlatform.OneBot:
                    OneBotSendToPrivate(Config.Instance.QQ, msg);
                    break;
                case MessagePlatform.MiraiHTTP:
                    MiraiHTTPSendToPrivate(Config.Instance.QQ, msg);
                    break;
                case MessagePlatform.MiraiHTTPV1:
                    MiraiHTTPV1SendToPrivate(Config.Instance.QQ, msg);
                    break;
            }
        }

        /// <summary>
        /// 广播通知到所有订阅消息的群体
        /// </summary>
        /// <param name="content">消息内容</param>
        public static void Broadcast(string content)
        {
            if (!Inited)
            {
                Trace.WriteLine("由于 Miguel Network 未初始化完成, 广播无法发送.");
                return;
            }
            Task.Factory.StartNew(() =>
            {
                var count = 0;
                foreach (var group in Config.Instance.WFGroupList)
                {
                    var sb = new StringBuilder();
                    sb.Append("[WFBot通知] ");
                    sb.AppendLine(content);
                    if (count > 10) sb.AppendLine($"发送次序: {count}(与真实延迟了{7 * count}秒)");
                    // sb.AppendLine($"如果想要获取更好的体验,请自行部署.");
                    switch (Platform)
                    {
                        case MessagePlatform.OneBot:
                            OneBotSendToGroup(group, sb.ToString().Trim());
                            break;
                        case MessagePlatform.MiraiHTTP:
                            MiraiHTTPSendToGroup(group, sb.ToString().Trim());
                            break;
                        case MessagePlatform.Kook:
                            break;
                        case MessagePlatform.QQChannel:
                            break;
                        case MessagePlatform.Test:
                            break;
                        case MessagePlatform.Unknown:
                            break;
                        case MessagePlatform.MiraiHTTPV1:
                            MiraiHTTPV1SendToGroup(group, sb.ToString().Trim());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    count++;
                    Thread.Sleep(7000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
                }
            }, TaskCreationOptions.LongRunning);

        }

        //
        // 以下的方法不应在本类外调用, 通用功能应该调用通用接口
        //
        static readonly Dictionary<GroupID, string> previousMessageDic = new Dictionary<GroupID, string>();
        private static void OneBotSendToGroup(GroupID group, string msg)
        {
            var qq = group.ID;
            // 避免重复发送同一条消息
            if (previousMessageDic.ContainsKey(qq) && msg == previousMessageDic[qq]) return;
            previousMessageDic[qq] = msg;
            OneBotCore.OneBotClient.SendGroupMessageAsync(group, msg);
        }        
        private static async Task OneBotSendToGroupWithAutoRevoke(GroupID group, string msg)
        {
            var qq = group.ID;
            // 避免重复发送同一条消息
            if (previousMessageDic.ContainsKey(qq) && msg == previousMessageDic[qq]) return;
            previousMessageDic[qq] = msg;
            var response = await OneBotCore.OneBotClient.SendGroupMessageAsync(group, msg);
            if (OneBotConfig.Instance.AutoRevoke)
            {
                await Task.Delay(TimeSpan.FromSeconds(OneBotConfig.Instance.RevokeTimeInSeconds))
                   .ContinueWith(t => { OneBotCore.OneBotClient.RecallMessageAsync(response); });
            }
        }
        private static async Task OneBotSendToGroupWithAutoRevoke(GroupID group, RichMessages msg)
        {
            var response = await OneBotCore.OneBotClient.SendGroupMessageAsync(group, msg.Select(x => x switch{ImageMessage image => SendingMessage.ByteArrayImage(image.Content), TextMessage t=> new SendingMessage(t.Content) }).Aggregate((a, b) => a + b));
            if (OneBotConfig.Instance.AutoRevoke)
            {
                await Task.Delay(TimeSpan.FromSeconds(OneBotConfig.Instance.RevokeTimeInSeconds))
                    .ContinueWith(t => { OneBotCore.OneBotClient.RecallMessageAsync(response); });
            }
        }

        private static void OneBotSendToPrivate(UserID qq, string msg)
        {
            OneBotCore.OneBotClient.SendPrivateMessageAsync(qq, msg);
        }
        private static void MiraiHTTPSendToGroup(GroupID qq, string msg)
        {
            var builder = new MessageChainBuilder();
            builder.Plain(msg);
            MiraiHTTPCore.Bot.SendGroupMessageAsync(qq.ID, builder.Build());
        }
        private static void MiraiHTTPSendToGroupWithAutoRevoke(GroupID qq, string msg)
        {
            var builder = new MessageChainBuilder();
            builder.Plain(msg);
            if (MiraiConfig.Instance.AutoRevoke)
            {
                var message = MiraiHTTPCore.Bot.SendGroupMessageAsync(qq.ID, builder.Build()).Result;
                Task.Delay(TimeSpan.FromSeconds(MiraiConfig.Instance.RevokeTimeInSeconds)).ContinueWith(t =>
                {
                    MiraiHTTPCore.Bot.RecallAsync(message, qq.ID.ToString());
                });

                return;
            }
            MiraiHTTPCore.Bot.SendGroupMessageAsync(qq.ID, builder.Build());
        }

        private static void MiraiHTTPSendToGroupWithAutoRevoke(GroupID qq, RichMessages msg)
        {
            var builder = new MessageChainBuilder();
            
            foreach (var message in msg)
            {
                switch (message)
                {
                    case ImageMessage image:
                        builder.ImageFromId(MiraiHTTPCore.Bot.UploadImageAsync(new MemoryStream(image.Content)).Result
                            .ImageId);
                        break;
                    case TextMessage text:
                        builder.Plain(text.Content);
                        break;
                }
            }
            if (MiraiConfig.Instance.AutoRevoke)
            {
                var message = MiraiHTTPCore.Bot.SendGroupMessageAsync(qq.ID, builder.Build()).Result;
                Task.Delay(TimeSpan.FromSeconds(MiraiConfig.Instance.RevokeTimeInSeconds)).ContinueWith(t =>
                {
                    MiraiHTTPCore.Bot.RecallAsync(message, qq.ID.ToString());
                });

                return;
            }
            MiraiHTTPCore.Bot.SendGroupMessageAsync(qq.ID, builder.Build());
        }

        private static void MiraiHTTPV1SendToGroup(GroupID qq, string msg)
        {
            MiraiHTTPV1Core.Mirai.SendGroupMessageAsync(qq, new PlainMessage(msg));
        }

        private static void MiraiHTTPSendToPrivate(UserID qq, string msg)
        {
            var builder = new MessageChainBuilder();
            builder.Plain(msg);
            MiraiHTTPCore.Bot.SendFriendMessageAsync(qq.ID, builder.Build());
        }

        private static void MiraiHTTPV1SendToPrivate(UserID qq, string msg)
        {
            MiraiHTTPV1Core.Mirai.SendFriendMessageAsync(qq, new PlainMessage(msg));
        }


        public static void ReplyKookChannelUser(string msg, KookContext context)
        {
            var cb = new CardBuilder();
            var sb = new SectionModuleBuilder
            {
                Text = new PlainTextElementBuilder().WithContent(msg)
            };
            cb.AddModule(sb);
            context.Channel.SendCardAsync(cb.Build(), ephemeralUser: context.Author);
        }
    }
}
