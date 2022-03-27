using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
