﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Flurl.Http;
using GammaLibrary;
using GammaLibrary.Extensions;
using WFBot.Features.Utils;

namespace WFBot.Orichalt.OrichaltConnectors
{
    public class MiraiHTTPContext : PlatformContextBase
    {
        public MiraiHTTPContext(GroupID @group, UserID senderId, string rawMessage, MessageChain chain, MessageScope scope, DateTimeOffset time)
        {
            Group = @group;
            SenderID = senderId;
            RawMessage = rawMessage;
            Chain = chain;
            Scope = scope;
            Time = time;
        }

        public GroupID Group { get; set; }
        public UserID SenderID { get; set; }
        public string RawMessage { get; set; }
        public MessageChain Chain { get; set; }
        public MessageScope Scope { get; set; }
        public DateTimeOffset Time { get; set; }
    }
    [Utils.Configuration("Miraiv2Config")]
    public class MiraiConfig : Utils.Configuration<MiraiConfig>
    {
        public string Host = "127.0.0.1";
        public ushort Port = 8080;
        public string AuthKey = "";
        public long BotQQ = default;

        public bool AutoRevoke = false;
        public int RevokeTimeInSeconds = 60;
        public bool UseHttps = false;
    }

    public class MiraiHTTPCore
    {
        public event EventHandler<MiraiHTTPContext> MiraiHTTPMessageReceived;
        public MiraiBot Bot;
        public async Task Init()
        {
            if (config.BotQQ == default)
            {
                Trace.WriteLine("请在Miraiv2Config.json里填入机器人的QQ号.");
                Trace.WriteLine("注: 也可以使用WebUI来配置捏.");
                Trace.WriteLine("按任意键关闭");
                Console.ReadKey();
                WFBotCore.Instance.Shutdown();
            }

            if (config.AuthKey.IsNullOrWhiteSpace())
            {
                Trace.WriteLine("请在Miraiv2Config.json将mirai控制台内生成的verifyKey填入AuthKey内.");
                Trace.WriteLine("注: 也可以使用WebUI来配置捏.");
                Trace.WriteLine("按任意键关闭");
                Console.ReadKey();
                WFBotCore.Instance.Shutdown();
            }
            Bot = new MiraiBot
            {
                Address = $"{config.Host}:{config.Port}",
                QQ = config.BotQQ.ToString(),
                VerifyKey = config.AuthKey,
                //UseHttps = config.UseHttps
            };
            var i = true;
            while (true)
            {
                try
                {
                    Trace.WriteLine("尝试连接MiraiHTTPv2···");
                    await Bot.LaunchAsync();
                    break;
                }
                catch (FlurlHttpException e)
                {
                    Trace.WriteLine("MiraiHTTPv2连接失败, 1秒后重试···");
                    if (i)
                    {
                        Trace.WriteLine(e.ToString());
                    }
                    i = false;
                    await Task.Delay(1000);
                }
            }
            Trace.WriteLine("MiraiHTTPv2已连接.");
            Bot.DisconnectionHappened.Subscribe(async e =>
            {
                while (true)
                {
                    try
                    {
                        Trace.WriteLine("尝试连接MiraiHTTPv2···");
                        await Bot.LaunchAsync();
                        break;
                    }
                    catch (FlurlHttpException e1)
                    {
                        Trace.WriteLine("MiraiHTTPv2连接失败, 1秒后重试···");
                        await Task.Delay(1000);
                    }
                }
            });
            
            Bot.EventReceived.OfType<GroupMessageReceiver>().Subscribe(GroupMessageReceived);
            Bot.EventReceived.OfType<FriendMessageReceiver>().Subscribe(FriendMessageReceived);
        }

        private MiraiConfig config => MiraiConfig.Instance;

        private void GroupMessageReceived(GroupMessageReceiver e)
        {
            var context = new MiraiHTTPContext(e.Sender.Group.Id, e.Sender.Id, e.MessageChain.GetPlainMessage(), e.MessageChain,
                MessageScope.Group, DateTimeOffset.Now);
            MiguelNetwork.MiraiHTTPCore.OnMiraiHttpMessage(context);
        }

        private void FriendMessageReceived(FriendMessageReceiver e)
        {
            var context = new MiraiHTTPContext(new GroupID(), e.Sender.Id, e.MessageChain.GetPlainMessage(), e.MessageChain,
                MessageScope.Private, DateTimeOffset.Now);
            MiguelNetwork.MiraiHTTPCore.OnMiraiHttpMessage(context);
        }
        protected virtual void OnMiraiHttpMessage(MiraiHTTPContext e)
        {
            MiraiHTTPMessageReceived?.Invoke(this, e);
        }
    }

}
