using System.Threading;

namespace WFBot.Utils
{
    public static class AsyncContext
    {
        internal static readonly AsyncLocal<Container<CancellationToken>> CurrentCancellationToken =
            new AsyncLocal<Container<CancellationToken>>();

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