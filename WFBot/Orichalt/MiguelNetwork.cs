using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using WFBot.Features.Utils;
using WFBot.Orichalt.OrichaltConnectors;

namespace WFBot.Orichalt
{
    public static class MiguelNetwork
    {
        private static MessagePlatform Platform;

        public static Connectors Connector;

        public static OrichaltContextManager OrichaltContextManager;

        private static bool Inited;

        public static event EventHandler<OrichaltContext> OrichaltMessageRecived;


        public static ConcurrentDictionary<GroupID, int> OneBotGroupCallDic = new ConcurrentDictionary<GroupID, int>();

        public static bool CheckCallPerMin(OrichaltContext o)
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    var context = OrichaltContextManager.GetOneBotContext(o);
                    lock (OneBotGroupCallDic)
                    {
                        if (OneBotGroupCallDic.ContainsKey(context.Group))
                        {
                            if (OneBotGroupCallDic[context.Group] > Config.Instance.CallperMinute && Config.Instance.CallperMinute != 0) return false;
                        }
                        else
                        {
                            OneBotGroupCallDic[context.Group] = 0;
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
                        var context = OrichaltContextManager.GetOneBotContext(o);
                        var group = context.Group;
                        if (OneBotGroupCallDic.ContainsKey(group))
                        {
                            OneBotGroupCallDic[group]++;
                        }
                        else
                        {
                            OneBotGroupCallDic[group] = 1;
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
            }

        }
        private static void OnOrichaltMessageRecived(OrichaltContext e)
        {
            OrichaltMessageRecived?.Invoke(null, e);
        }

        public static void InitMiguelNetwork(MessagePlatform platform)
        {
            Platform = platform;
            Connector = new Connectors(platform);
            OrichaltContextManager = new OrichaltContextManager();
            Connector.OneBotMessageReceived += Connector_OneBotMessageReceived;

            OrichaltMessageRecived += MiguelNetwork_OrichaltMessageRecived;
            Inited = true;
        }

        private static void MiguelNetwork_OrichaltMessageRecived(object sender, OrichaltContext e)
        {
            
        }

        private static void Connector_OneBotMessageReceived(object sender, OneBotContext e)
        {
            var o = OrichaltContextManager.PutPlatformContext(e);
            OnOrichaltMessageRecived(o);
        }





        public static void Reply(OrichaltContext o, string msg)
        // 响应通用命令应答
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    var context = OrichaltContextManager.OneBotContexts[o.UUID];
                    OneBotSendToGroup(context.Group, msg);
                    IncreaseCallCounts(o);
                    break;
                case MessagePlatform.Kaiheila:
                    break;
                case MessagePlatform.QQChannel:
                    break;
            }
        }
        public static void SendDebugInfo(string msg)
        // 发送通用Debug信息给管理者
        {
            switch (Platform)
            {
                case MessagePlatform.OneBot:
                    OneBotSendToPrivate(Config.Instance.QQ, msg);
                    break;
            }
        }

        public static void Broadcast(string content)
        // 广播通知到所有订阅消息的群体
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

        private static void OneBotSendToGroup(GroupID group, string msg)
        {
            Connector.OneBotClient.SendGroupMessageAsync(group, msg);
        }

        private static void OneBotSendToPrivate(UserID qq, string msg)
        {
            Connector.OneBotClient.SendPrivateMessageAsync(qq, msg);
        }

    }
}
