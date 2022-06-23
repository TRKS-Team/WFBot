using System;
using System.Threading;
using WFBot.Orichalt;

namespace WFBot.Utils
{
    public static class AsyncContext
    {
        internal static readonly AsyncLocal<Container<CancellationToken>> CurrentCancellationToken =
            new AsyncLocal<Container<CancellationToken>>();
        internal static readonly AsyncLocal<Container<OrichaltContext>> CurrentOrichaltContext =
            new AsyncLocal<Container<OrichaltContext>>();

        public static void SetOrichaltContext(OrichaltContext context)
        {
            CurrentOrichaltContext.Value = new Container<OrichaltContext>(context);
        }

        public static OrichaltContext GetOrichaltContext()
        {
            return CurrentOrichaltContext.Value?.Value ?? throw new Exception("Message Sender not found.");
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