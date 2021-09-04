using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Mirai_CSharp;
using Mirai_CSharp.Extensions;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin;
using WFBot;
using WFBot.Connector;
using WFBot.Features.Utils;

namespace MiraiHTTPConnector
{
    public class Connector : WFBotConnectorBase
    {
        private MiraiHttpSession session;

        public override void Init()
        {
            var config = MiraiConfig.Instance;
            var qq = config.BotQQ;
            var host = config.Host;
            var port = config.Port;
            var authKey = config.AuthKey;
            if (Directory.Exists("WFBotImageCaches"))
            {
                Directory.Delete("WFBotImageCaches", true);
            }
            
            if (qq == default || host == default || port == default || authKey == default)
            {
                // todo 直接控制台写
                throw new InvalidOperationException("请在 MiraiConfig.json 内补全信息, 详情请查看文档.");
            }

            var options = new MiraiHttpSessionOptions(host, port, authKey); // 至少八位数
            session = new MiraiHttpSession();
            session.GroupMessageEvt += (sender, args) =>
            {
                var msg = args.Chain.GetPlain();
                ReportGroupMessage(args.Sender.Group.Id, args.Sender.Id, msg);
                return Task.FromResult(true);
            };

            session.FriendMessageEvt += (sender, args) =>
            {
                var msg = args.Chain.GetPlain();
                ReportFriendMessage(args.Sender.Id, msg);
                return Task.FromResult(true);
            };

            session.DisconnectedEvt += async (sender, exception) =>  
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Mirai 连接断开, 正在重连...");
                        await session.ConnectAsync(options, qq);
                        return true;
                    }
                    catch (Exception)
                    {
                        await Task.Delay(1000);
                    }
                }
            };
            session.ConnectAsync(options, qq).Wait();
        }

        public override void SendGroupMessage(GroupID groupID, string message)
        {
            var msgID = session.SendGroupMessageAsync(groupID, new PlainMessage(message)).Result;

            if (MiraiConfigInMain.Instance.AutoRevoke)
            {
                Task.Delay(MiraiConfigInMain.Instance.RevokeTimeInSeconds)
                    .ContinueWith((t) => { session.RevokeMessageAsync(msgID); });
            }
        }
        
        

        public override void SendPrivateMessage(UserID userID, string message)
        {
            session.SendFriendMessageAsync(userID, new PlainMessage(message)).Wait();
        }
    }
}
