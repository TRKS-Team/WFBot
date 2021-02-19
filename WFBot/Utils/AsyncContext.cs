using System;
using System.Threading;

namespace WFBot.Utils
{
    public static class AsyncContext
    {
        internal static readonly AsyncLocal<Container<CancellationToken>> CurrentCancellationToken =
            new AsyncLocal<Container<CancellationToken>>();
        internal static readonly AsyncLocal<Container<IGroupMessageSender>> CurrentMessageSender =
            new AsyncLocal<Container<IGroupMessageSender>>();

        public static void SetMessageSender(IGroupMessageSender sender)
        {
            CurrentMessageSender.Value = new Container<IGroupMessageSender>(sender);
        }

        public static IGroupMessageSender GetMessageSender()
        {
            return CurrentMessageSender.Value?.Value ?? throw new Exception("Message Sender not found.");
        }

        public static void SendGroupMessage(string msg)
        {
            GetMessageSender().SendMessage(msg);
        }

        public static void SetCancellationToken(CancellationToken token)
        {
            CurrentCancellationToken.Value = new Container<CancellationToken>(token);
        }

        public static CancellationToken GetCancellationToken()
        {
            return CurrentCancellationToken.Value?.Value ?? CancellationToken.None;
        }
    }
}