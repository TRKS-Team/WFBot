using System;
using System.Threading;

namespace WFBot.Utils
{
    public static class AsyncContext
    {
        internal static readonly AsyncLocal<Container<CancellationToken>> CurrentCancellationToken =
            new AsyncLocal<Container<CancellationToken>>();
        internal static readonly AsyncLocal<Container<GroupMessageSender>> CurrentMessageSender =
            new AsyncLocal<Container<GroupMessageSender>>();

        public static void SetMessageSender(GroupMessageSender sender)
        {
            CurrentMessageSender.Value = new Container<GroupMessageSender>(sender);
        }

        public static GroupMessageSender GetMessageSender()
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