using Kook;
using Kook.WebSocket;

namespace WFBot.Orichalt.OrichaltConnectors
{
    [Utils.Configuration("KookConfig.json")]
    public class KookConfig : Utils.Configuration<KookConfig>
    {
        public string Token { get; set; }
    }
    public class KookContext : PlatformContextBase
    {
        public KookContext(SocketUser author, Kook.MessageType type, ISocketMessageChannel channel, string cleanContent, SocketGuild guild)
        {
            Author = author;
            Type = type;
            Channel = channel;
            CleanContent = cleanContent;
            Guild = guild;
        }

        public SocketUser Author { get; set; }
        public Kook.MessageType Type { get; set; }
        public ISocketMessageChannel Channel { get; set; }
        public string CleanContent { get; set; }
        public SocketGuild Guild { get; set; }
    }

    public class KookCore
    {
        public KookSocketClient KookClient;
        public string Token => KookConfig.Instance.Token;

        public KookCore()
        {
            KookClient = new KookSocketClient();
            KookClient.LoginAsync(TokenType.Bot, Token);
            KookClient.MessageReceived += KookClient_MessageReceived;
            KookClient.StartAsync().Wait();
        }

        public void SendToChannel(string msg, KookContext context)
        {
            var cb = new CardBuilder();
            var sb = new SectionModuleBuilder
            {
                Text = new PlainTextElementBuilder().WithContent(msg)
            };
            cb.AddModule(sb);
            context.Channel.SendCardAsync(cb.Build());
        }
        private Task KookClient_MessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new KookContext(message.Author, message.Type, message.Channel, message.CleanContent, message.Guild);
            return Task.CompletedTask;
        }
    }
}
