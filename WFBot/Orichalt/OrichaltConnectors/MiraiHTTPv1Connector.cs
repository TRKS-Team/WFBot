using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using GammaLibrary;
using GammaLibrary.Extensions;
using Mirai_CSharp.Models;
using Mirai_CSharp;
using Mirai_CSharp.Extensions;
using WFBot.Features.Utils;

namespace WFBot.Orichalt.OrichaltConnectors
{
    public class MiraiHTTPV1Context : PlatformContextBase
    {
        public MiraiHTTPV1Context(GroupID @group, UserID senderId, string rawMessage, IMessageBase[] chain, MessageType type, DateTimeOffset time)
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
        public IMessageBase[] Chain { get; set; }
        public MessageType Type { get; set; }
        public DateTimeOffset Time { get; set; }
    }
    [Utils.Configuration("Miraiv1Config")]
    public class MiraiV1Config : Utils.Configuration<MiraiV1Config>
    {
        public string Host = "127.0.0.1";
        public ushort Port = 8080;
        public string AuthKey = "";
        public long BotQQ = default;

        public bool AutoRevoke = false;
        public int RevokeTimeInSeconds = 60;
    }

    public class MiraiHTTPV1Core
    {
        public event EventHandler<MiraiHTTPV1Context> MiraiHTTPMessageReceived;
        public MiraiHttpSession Mirai;
        public async Task Init()
        {
            if (config.BotQQ == default)
            {
                Console.WriteLine("请在Miraiv1Config.json里填入机器人的QQ号.");
                Console.WriteLine("按任意键继续");
                Console.ReadKey();
                WFBotCore.Instance.Shutdown();
            }

            if (config.AuthKey.IsNullOrWhiteSpace())
            {
                Console.WriteLine("请在Miraiv1Config.json将mirai控制台内生成的verifyKey填入AuthKey内.");
                Console.WriteLine("按任意键继续");
                Console.ReadKey();
                WFBotCore.Instance.Shutdown();
            }

            var options = new MiraiHttpSessionOptions(config.Host, config.Port, config.AuthKey);
            var mirai = new MiraiHttpSession();
            while (!mirai.Connected) await mirai.ConnectAsync(options, config.BotQQ);
            mirai.DisconnectedEvt += async (_, _) =>
            {
                while (true)
                {
                    try
                    {
                        await mirai.ConnectAsync(options, config.BotQQ);
                        return true;
                    }
                    catch (Exception)
                    {
                        await Task.Delay(1000);
                    }
                }
            };
            Mirai = mirai;

        }

        private MiraiV1Config config => MiraiV1Config.Instance;

        private void GroupMessageReceived(IGroupMessageEventArgs e)
        {
            var context = new MiraiHTTPV1Context(e.Sender.Group.Id, e.Sender.Id, e.Chain.GetPlain(), e.Chain,
                MessageType.Group, DateTimeOffset.Now);
            MiguelNetwork.MiraiHTTPV1Core.OnMiraiHttpV1Message(context);
        }

        private void FriendMessageReceived(IFriendMessageEventArgs e)
        {
            var context = new MiraiHTTPV1Context(new GroupID(), e.Sender.Id, e.Chain.GetPlain(), e.Chain,
                MessageType.Private, DateTimeOffset.Now);
            MiguelNetwork.MiraiHTTPV1Core.OnMiraiHttpV1Message(context);
        }

        protected virtual void OnMiraiHttpV1Message(MiraiHTTPV1Context e)
        {
            MiraiHTTPMessageReceived?.Invoke(this, e);
        }
    }

}
