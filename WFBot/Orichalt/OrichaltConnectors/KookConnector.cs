using Kook;
using Kook.WebSocket;
using System.Diagnostics;

namespace WFBot.Orichalt.OrichaltConnectors
{
    [Utils.Configuration("KookConfig.json")]
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

        }

        public async void Init()
        {
            Console.WriteLine("检测到Kook实例, 如果没有特殊需求, 建议使用公用机器人, 为了社区的良性发展, 请不要将您的机器人设为公用机器人.");
            KookClient = new KookSocketClient();
            await KookClient.LoginAsync(TokenType.Bot, Token);
            KookClient.MessageReceived += ChannelMessageReceived;
            try
            {
                await KookClient.StartAsync();
                Trace.WriteLine("尝试连接Kook···");
            }
            catch (Exception )
            {
                Trace.WriteLine("Kook连接失败, 1秒后重试···");
            }
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
