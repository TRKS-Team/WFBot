using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Mirai.Net.Data.Messages;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;
using Mirai_CSharp.Models;
using WFBot.Features.Utils;
using WFBot.Orichalt.OrichaltConnectors;

namespace WFBot.Orichalt
{
    public static class MiguelNetwork
    {
        private static MessagePlatform Platform;

        public static OneBotCore OneBotCore;

        public static MiraiHTTPCore MiraiHTTPCore;

        public static MiraiHTTPV1Core MiraiHTTPV1Core;
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
            }

            OrichaltMessageRecived += MiguelNetwork_OrichaltMessageRecived;
            Inited = true;
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
        public static void Reply(OrichaltContext o, string msg)
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    if (CheckCallPerMin(o))
                    {
                        var oneBotContext = OrichaltContextManager.GetOneBotContext(o);
                        OneBotSendToGroup(oneBotContext.Group, msg);
                        IncreaseCallCounts(o);
                    }
                    break;
                case MessagePlatform.Kaiheila:
                    break;
                case MessagePlatform.QQChannel:
                    break;
                case MessagePlatform.MiraiHTTP:
                    if (CheckCallPerMin(o))
                    {
                        var miraiHTTPContext = OrichaltContextManager.GetMiraiHTTPContext(o);
                        MiraiHTTPSendToGroup(miraiHTTPContext.Group, msg);
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
            switch (Platform)
            {
                case MessagePlatform.OneBot:
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
                            OneBotSendToGroup(group, sb.ToString().Trim());
                            count++;
                            Thread.Sleep(7000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
                        }
                    }, TaskCreationOptions.LongRunning);
                    break;
            }
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
        private static void OneBotSendToPrivate(UserID qq, string msg)
        {
            OneBotCore.OneBotClient.SendPrivateMessageAsync(qq, msg);
        }
        private static void MiraiHTTPSendToGroup(GroupID qq, string msg)
        {
            var builder = new MessageChainBuilder();
            builder.Plain(msg);
            MessageManager.SendGroupMessageAsync(qq, builder.Build());
        }

        private static void MiraiHTTPSendToPrivate(UserID qq, string msg)
        {
            var builder = new MessageChainBuilder();
            builder.Plain(msg);
            MessageManager.SendFriendMessageAsync(qq, builder.Build());
        }


        private static void MiraiHTTPV1SendToGroup(GroupID qq, string msg)
        {
            MiraiHTTPV1Core.Mirai.SendGroupMessageAsync(qq, new PlainMessage(msg));
        }
        private static void MiraiHTTPV1SendToPrivate(UserID qq, string msg)
        {
            MiraiHTTPV1Core.Mirai.SendFriendMessageAsync(qq, new PlainMessage(msg));
        }
    }
}
