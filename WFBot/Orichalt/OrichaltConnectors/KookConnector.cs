using Kook;
using Kook.WebSocket;
using System.Diagnostics;

namespace WFBot.Orichalt.OrichaltConnectors
{
    [Utils.Configuration("KookConfig")]
    public class KookConfig : Utils.Configuration<KookConfig>
    {
        public string Token { get; set; }
        public ulong AdminID { get; set; }
        public Dictionary<ulong, ulong> NotificationChannelDict { get; set; } = new();
        public Dictionary<ulong, ulong> BotChannelDict { get; set; } = new();
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
            KookClient.DirectMessageReceived += DirectMessageReceived;
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

        private Task DirectMessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new KookContext(message.Author, message.Type, message.Channel, message.CleanContent,
                message.Guild, Orichalt.MessageScope.Private);
            if (message.Author.IsBot != true)
            {
                OnKookMessage(context);
            }
            return Task.CompletedTask;
        }

        private Task ChannelMessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new KookContext(message.Author, message.Type, message.Channel, message.CleanContent,
                message.Guild, Orichalt.MessageScope.Public);
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
        // 
        // 这一段方法只能在加了[{Platform}Only]的标签的消息处理中使用
        //

        public bool CheckKookModerator(KookContext context)
        {
            if (context.Author is SocketGuildUser guildUser)
            {
                return guildUser.Roles.Any(r => r.Name == "管理员");
            }

            return false;
        }
        public void SetKookNotifyChannel(OrichaltContext o)
        {
            var context = MiguelNetwork.OrichaltContextManager.GetKookContext(o);
            if (!CheckKookModerator(context))
            {
                MiguelNetwork.Reply(o, "你不是管理员.");
                return;
            }
            var guildId = context.Guild.Id;
            var channelId = context.Channel.Id;
            KookConfig.Instance.NotificationChannelDict[guildId] = channelId;
            KookConfig.Save();
            MiguelNetwork.Reply(o, "已设置通知接收频道.");
        }

        public void RemoveKookNotifyChannel(OrichaltContext o)
        {
            var context = MiguelNetwork.OrichaltContextManager.GetKookContext(o);
            if (!CheckKookModerator(context))
            {
                MiguelNetwork.Reply(o, "你不是管理员.");
                return;
            }
            var guildId = context.Guild.Id;
            var channelId = context.Channel.Id;
            if (KookConfig.Instance.NotificationChannelDict[guildId] == channelId)
            {
                KookConfig.Instance.NotificationChannelDict[guildId] = default;
                KookConfig.Save();
                MiguelNetwork.Reply(o, "已取消通知接收频道.");
                return;
            }
            MiguelNetwork.Reply(o, "本频道不是通知接收频道.");
        }

        public void SetKookBotChannel(OrichaltContext o)
        {
            var context = MiguelNetwork.OrichaltContextManager.GetKookContext(o);
            if (!CheckKookModerator(context))
            {
                MiguelNetwork.Reply(o, "你不是管理员.");
                return;
            }
            var guildId = context.Guild.Id;
            var channelId = context.Channel.Id;
            KookConfig.Instance.BotChannelDict[guildId] = channelId;
            KookConfig.Save();
            MiguelNetwork.Reply(o, "已设置机器人调用频道.");
        }

        public void RemoveKookBotChannel(OrichaltContext o)
        {
            var context = MiguelNetwork.OrichaltContextManager.GetKookContext(o);
            if (!CheckKookModerator(context))
            {
                MiguelNetwork.Reply(o, "你不是管理员.");
                return;
            }
            var guildId = context.Guild.Id;
            var channelId = context.Channel.Id;
            if (KookConfig.Instance.BotChannelDict[guildId] == channelId)
            {
                KookConfig.Instance.BotChannelDict[guildId] = default;
                KookConfig.Save();
                MiguelNetwork.Reply(o, "已取消机器人调用频道.");
                return;
            }
            MiguelNetwork.Reply(o, "本频道不是机器人调用频道.");
        }
    }
}
