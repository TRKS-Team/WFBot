using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TextCommandCore;
using WFBot.Events;
using WFBot.Features.Common;
using WFBot.Features.Utils;
using WFBot.Utils;
using static WFBot.Features.Utils.Messenger;

namespace WFBot.Features.Commands
{
    public partial class CommandsHandler
    {
        
        
        void SendMessage(string msg) => MsgSender.SendMessage(msg);
    }

    public partial class CommandsHandler : ICommandHandler<CommandsHandler>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; }
        public Action<Message> ErrorMessageSender { get; }

        public UserID Sender { get; }
        public string Message { get; }
        public GroupID Group { get; }
        GroupMessageSender MsgSender => new GroupMessageSender(Group);
        public readonly Lazy<StringBuilder> OutputStringBuilder = new Lazy<StringBuilder>(() => new StringBuilder());
        string ICommandHandler<CommandsHandler>.Sender => Group;
        
        private static readonly WMSearcher _wmSearcher = new WMSearcher();
        private static readonly RMSearcher _rmSearcher = new RMSearcher();
        private static readonly WMASearcher _wmaSearcher = new WMASearcher();

        void Append(string s)
        {
            OutputStringBuilder.Value.Append(s);
        }

        void AppendLine(string s)
        {
            OutputStringBuilder.Value.AppendLine(s);
        }

        void AppendLine()
        {
            OutputStringBuilder.Value.AppendLine();
        }

        public CommandsHandler(UserID sender, GroupID group, string message)
        {
            Sender = sender;
            MessageSender = (id, msg) =>
            {
                SendGroup(id.ID, msg);
            };
            Group = group;
            Message = message;

            ErrorMessageSender = msg => SendDebugInfo(msg);
        }

    }
}
