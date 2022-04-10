using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
                    SendToGroup(context.Group, msg);
                    break;
                case MessagePlatform.Kaiheila:
                    break;
                case MessagePlatform.QQChannel:
                    break;
            }
        }

        public static void SendToGroup(GroupID group, string msg)
        {
            Connector.OneBotClient.SendGroupMessageAsync(group, msg);
        }
    }
}
