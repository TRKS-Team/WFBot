using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var options = new MiraiHttpSessionOptions("127.0.0.1", 8080, "********"); // 至少八位数
            session = new MiraiHttpSession();
            session.GroupMessageEvt += (sender, args) =>
            {
                var msg = args.Chain.GetPlain();
                ReportGroupMessage(args.Sender.Group.Id.ToString(), args.Sender.Id.ToString(), msg);
                return Task.FromResult(true);
            };

            var qq = MiraiConfig.Instance.BotQQ;

            if (qq == default)
            {
                MiraiConfig.Save();
                throw new Exception("请在 MiraiConfig.json 内填写机器人的 QQ 号");
            }
            
            session.ConnectAsync(options, qq).Wait();
        }

        public override void SendGroupMessage(GroupID groupID, string message)
        {
            session.SendGroupMessageAsync(groupID, new IMessageBase[]{ new PlainMessage(message) }).Wait();
        }

        public override void SendPrivateMessage(UserID userID, string message)
        {
            session.SendFriendMessageAsync(userID, new IMessageBase[]{ new PlainMessage(message) }).Wait();
        }
    }
}
