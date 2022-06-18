using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary;
using Microsoft.Extensions.DependencyInjection;
using Mirai.CSharp.Builders;
using Mirai.CSharp.Extensions;
using Mirai.CSharp.Handlers;
using Mirai.CSharp.HttpApi.Builder;
using Mirai.CSharp.HttpApi.Handlers;
using Mirai.CSharp.HttpApi.Models;
using Mirai.CSharp.HttpApi.Models.ChatMessages;
using Mirai.CSharp.HttpApi.Models.EventArgs;
using Mirai.CSharp.HttpApi.Options;
using Mirai.CSharp.HttpApi.Parsers;
using Mirai.CSharp.HttpApi.Parsers.Attributes;
using Mirai.CSharp.HttpApi.Session;
using Mirai.CSharp.Session;
using WFBot.Features.Utils;

namespace WFBot.Orichalt.OrichaltConnectors
{
    public class MiraiHTTPContext : PlatformContextBase
    {
        public MiraiHTTPContext(GroupID @group, UserID senderId, string rawMessage, IChatMessage[] chain, MessageType type, DateTimeOffset time)
        {
            Group = @group;
            SenderID = senderId;
            RawMessage = rawMessage;
            Chain = chain;
            Type = type;
            Time = time;
        }

        public GroupID Group { get; set; }
        public UserID SenderID { get; set; }
        public string RawMessage { get; set; }
        public IChatMessage[] Chain { get; set; }
        public MessageType Type { get; set; }
        public DateTimeOffset Time { get; set; }
    }
    [Configuration("MiraiConfig")]
    public class MiraiConfig : Configuration<MiraiConfig>
    {
        public string Host = "127.0.0.1";
        public ushort Port = 8080;
        public string AuthKey = "";

        public bool AutoRevoke = false;
        public int RevokeTimeInSeconds = 60;
        public long BotQQ = default;
    }

    public class MiraiHTTPCore
    {
        public event EventHandler<MiraiHTTPContext> MiraiHTTPMessageReceived;
        public async Task Init()
        {
            IServiceProvider services = new ServiceCollection()
                .AddMiraiBaseFramework()
                .AddHandler<WFBotPlugin>()
                .Services
                .AddDefaultMiraiHttpFramework()
                .AddClient<MiraiHttpSession>()
                .Services
                .Configure<MiraiHttpSessionOptions>(options =>
                {
                    options.Host = config.Host;
                    options.Port = config.Port;
                    options.AuthKey = config.AuthKey;
                })
                .AddLogging()
                .BuildServiceProvider();
            IMiraiHttpSession session = services.GetRequiredService<IMiraiHttpSession>();
            await session.ConnectAsync(config.BotQQ);
        }

        private MiraiConfig config => MiraiConfig.Instance;

        protected virtual void OnMiraiHttpMessage(MiraiHTTPContext e)
        {
            MiraiHTTPMessageReceived?.Invoke(this, e);
        }
        [RegisterMiraiHttpParser(typeof(DefaultMappableMiraiHttpMessageParser<IGroupMessageEventArgs, GroupMessageEventArgs>))]
        public partial class WFBotPlugin : IMiraiHttpMessageHandler<IGroupMessageEventArgs>
        {
            public Task HandleMessageAsync(IMiraiHttpSession session, IGroupMessageEventArgs e)
            {
                var context = new MiraiHTTPContext(e.Sender.Group.Id, e.Sender.Id, e.Chain.GetPlain(), e.Chain,
                    MessageType.Group, DateTimeOffset.Now);
                MiguelNetwork.MiraiHTTPCore.OnMiraiHttpMessage(context);
                return Task.CompletedTask;
            }
        }
        [RegisterMiraiHttpParser(typeof(DefaultMappableMiraiHttpMessageParser<IFriendMessageEventArgs, FriendMessageEventArgs>))]
        public partial class WFBotPlugin : IMiraiHttpMessageHandler<IFriendMessageEventArgs>
        {
            public Task HandleMessageAsync(IMiraiHttpSession session, IFriendMessageEventArgs e)
            {
                var context = new MiraiHTTPContext("", e.Sender.Id, e.Chain.GetPlain(), e.Chain,
                    MessageType.Private, DateTimeOffset.Now);
                MiguelNetwork.MiraiHTTPCore.OnMiraiHttpMessage(context);
                return Task.CompletedTask;
            }
        }
    }

}
