using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WarframeAlertingPrime.SDK.Models.GameInfo;
using WFBot.Events;
using WFBot.Features.Common;
using WFBot.Features.ImageRendering;
using WFBot.Features.Utils;
using WFBot.Orichalt;
using WFBot.TextCommandCore;
using WFBot.Utils;
using static WFBot.Features.Utils.Messenger;

namespace WFBot.Features.Commands
{
    public partial class CommandsHandler : ICommandHandler<CommandsHandler>
    {
        public Action<Message> MessageSender { get; }
        public Action<RichMessages> RichMessageSender { get; set; }
        public Action<Message> ErrorMessageSender { get; }
        public string Message { get; }
        public OrichaltContext O { get; private set; }
        public readonly Lazy<StringBuilder> OutputStringBuilder = new Lazy<StringBuilder>(() => new StringBuilder());

        private static readonly WMSearcher _wmSearcher = new WMSearcher();
        private static readonly RMSearcher _rmSearcher = new RMSearcher();
        private static readonly WMASearcher _wmaSearcher = new WMASearcher();


        void SendImage(byte[] bytes)
        {
            RichMessageSender(new RichMessages() { new ImageMessage() { Content = bytes } });
        }

        void SendImageAndText(byte[] bytes, string s)
        {
            RichMessageSender(new RichMessages() { new ImageMessage() { Content = bytes }, new TextMessage() {Content = s} });
        }

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

        public CommandsHandler(OrichaltContext o, string message)
        {
            MessageSender = (msg) =>
            {
                MiguelNetwork.Reply(AsyncContext.GetOrichaltContext(), msg.Content);
            };
            RichMessageSender = (msg) =>
            {
                MiguelNetwork.Reply(AsyncContext.GetOrichaltContext(), msg);
            };
            Message = message;
            O = o;
            ErrorMessageSender = msg => SendDebugInfo(msg);
        }

    }
}
