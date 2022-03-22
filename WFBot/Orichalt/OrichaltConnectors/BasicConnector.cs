namespace WFBot.Orichalt.OrichaltConnectors
{
    public class MessageContextBase
    {

    }
    public partial class Connectors
    {
        private MessagePlatform Platform;
        public Connectors(MessagePlatform platform)
        {
            Platform = platform;
        }
    }
}
