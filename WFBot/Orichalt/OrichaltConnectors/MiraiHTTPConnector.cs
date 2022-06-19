using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public string Host = "localhost";
        public ushort Port = 8080;
        public string AuthKey = "123abc";

        public bool AutoRevoke = false;
        public int RevokeTimeInSeconds = 60;
        public long BotQQ = default;
    }

    public class MiraiHTTPCore
    {
        public event EventHandler<MiraiHTTPContext> MiraiHTTPMessageReceived;
        public IMiraiHttpSession session;
        public async Task Init()
        {
            config.BotQQ = 120019349;
            IServiceProvider services = new ServiceCollection()
                .AddMiraiBaseFramework()
                .AddHandler<WFBotPlugin>()
                .Services
                .AddDefaultMiraiHttpFramework()
                .ResolveParser<DynamicPlugin>()
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
            session = services.GetRequiredService<IMiraiHttpSession>();
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
                e.BlockRemainingHandlers = false;
                return Task.CompletedTask;
            }
        }
        [RegisterMiraiHttpParser(typeof(DefaultMappableMiraiHttpMessageParser<IFriendMessageEventArgs, FriendMessageEventArgs>))]
        public partial class WFBotPlugin : IMiraiHttpMessageHandler<IFriendMessageEventArgs>
        {
            public Task HandleMessageAsync(IMiraiHttpSession session, IFriendMessageEventArgs e)
            {

                var context = new MiraiHTTPContext(new GroupID(), e.Sender.Id, e.Chain.GetPlain(), e.Chain,
                    MessageType.Private, DateTimeOffset.Now);
                MiguelNetwork.MiraiHTTPCore.OnMiraiHttpMessage(context);
                e.BlockRemainingHandlers = false;
                return Task.CompletedTask;
            }
        }
        public partial class WFBotPlugin : IMiraiHttpMessageHandler<IDisconnectedEventArgs>
        {
            public async Task HandleMessageAsync(IMiraiHttpSession session, IDisconnectedEventArgs e)
            {
                while (!session.Connected)
                {
                    try
                    {
                        Trace.WriteLine("MiraiHTTP断开, 重连中···");
                        await session.ConnectAsync(e.LastConnectedQQNumber);
                        e.BlockRemainingHandlers = true;
                        Trace.WriteLine("MiraiHTTP已重连.");
                        break;
                    }
                    catch (ObjectDisposedException) // session 已被释放
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        Trace.WriteLine("MiraiHTTP重连失败, 1秒后重试···");
                        await Task.Delay(1000);
                    }
                }
            }
        }
        [RegisterMiraiHttpParser(typeof(DefaultMappableMiraiHttpMessageParser<IGroupMessageEventArgs, GroupMessageEventArgs>))]
        public partial class DynamicPlugin : IMiraiHttpMessageHandler<IGroupMessageEventArgs>
        {
            public Task HandleMessageAsync(IMiraiHttpSession session, IGroupMessageEventArgs message)
            {
                return Task.CompletedTask;
            }
        }
        [RegisterMiraiHttpParser(typeof(DefaultMappableMiraiHttpMessageParser<IFriendMessageEventArgs, FriendMessageEventArgs>))]
        public partial class DynamicPlugin : IMiraiHttpMessageHandler<IFriendMessageEventArgs>
        {
            public Task HandleMessageAsync(IMiraiHttpSession session, IFriendMessageEventArgs message)
            {
                return Task.CompletedTask;
            }
        }
    }

}
