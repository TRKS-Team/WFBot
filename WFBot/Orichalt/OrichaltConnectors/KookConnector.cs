using Kook;
using Kook.WebSocket;

namespace WFBot.Orichalt.OrichaltConnectors
{
    [Utils.Configuration("KookConfig")]
    public class KookConfig : Utils.Configuration<KookConfig>
    {
        public string Token { get; set; }
    }
    public class KookContext : PlatformContextBase
    {
        public KookContext(SocketUser author, MessageType type, ISocketMessageChannel channel, string cleanContent, SocketGuild guild, Orichalt.MessageScope scope)
        {
            Author = author;
            Type = type;
            Channel = channel;
            CleanContent = cleanContent;
            Guild = guild;
            Scope = scope;
        }

        public SocketUser Author { get; set; }
        public MessageType Type { get; set; }
        public ISocketMessageChannel Channel { get; set; }
        public string CleanContent { get; set; }
        public SocketGuild Guild { get; set; }
        public Orichalt.MessageScope Scope { get; set; }
    }

    public class KookCore
    {
        public KookSocketClient KookClient;
        public event EventHandler<KookContext> KookMessageReceived;
        public string Token => KookConfig.Instance.Token;

        public KookCore()
        {
            KookClient = new KookSocketClient();
            KookClient.LoginAsync(TokenType.Bot, Token);
            KookClient.MessageReceived += ChannelMessageReceived;
            KookClient.StartAsync().Wait();
        }

        private Task ChannelMessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new KookContext(message.Author, message.Type, message.Channel, message.CleanContent, message.Guild, Orichalt.MessageScope.Public);
            if (message.Author.IsBot != true)
            {
                OnKookMessage(context);
            }
            return Task.CompletedTask;
        }

        protected virtual void OnKookMessage(KookContext e)
        {
            KookMessageReceived?.Invoke(this, e);
        }
    }
}
