using System;

namespace WFBot.Orichalt.OrichaltConnectors
{
    public class PlatformContextBase
    {

    }
    public partial class Connectors
    {
        private MessagePlatform Platform;
        public Connectors(MessagePlatform platform)
        {
            Platform = platform;
            switch (Platform)
            {
                case MessagePlatform.OneBot:
                    InitOneBot();
                    break;
                case MessagePlatform.Kaiheila:
                    throw new NotImplementedException();
                    break;
                case MessagePlatform.QQChannel:
                    throw new NotImplementedException();
                    break;
            }
        }
    }
}
