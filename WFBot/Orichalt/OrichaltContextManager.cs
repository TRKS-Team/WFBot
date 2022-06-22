using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WFBot.Features.Utils;
using WFBot.Orichalt.OrichaltConnectors;

namespace WFBot.Orichalt
{
    public enum MessagePlatform
    {
        OneBot = 0,
        Kaiheila = 1,
        QQChannel = 2,
        MiraiHTTP = 3,
        Test = 4,
        Unknown = 5
    }

    public enum MessageScope
    {
        Public,
        Private
    }
    public class OrichaltContext : IDisposable
    {
        public OrichaltContext(string plainMessage, MessagePlatform platform, MessageScope scope)
        {
            PlainMessage = plainMessage;
            Platform = platform;
            Scope = scope;
            UUID = Guid.NewGuid().ToString();
            lifeTask = Task.Delay(TimeSpan.FromMinutes(10)).ContinueWith(t =>
            {
                Dispose();
            });
        }

        public string UUID { get; set; }
        public string PlainMessage { get; set; }
        public MessagePlatform Platform { get; set; }
        public MessageScope Scope { get; set; }
        private Task lifeTask;
        bool disposed = false;

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            MiguelNetwork.OrichaltContextManager.DisposeOrichaltContext(this);
        }

    }
    public class OrichaltContextManager
    {
        public Dictionary<string, OneBotContext> OneBotContexts = new Dictionary<string, OneBotContext>();

        public Dictionary<string, MiraiHTTPContext> MiraiHTTPContexts = new Dictionary<string, MiraiHTTPContext>();
        // 往下扩展各个平台

        public PlatformContextBase GetPlatformContext(OrichaltContext context)
        {
            return context.Platform switch
            {
                MessagePlatform.OneBot => OneBotContexts[context.UUID],
                _ => new PlatformContextBase()
            };
        }

        public OneBotContext GetOneBotContext(OrichaltContext o)
        {
            return OneBotContexts[o.UUID];
        }

        public MiraiHTTPContext GetMiraiHTTPContext(OrichaltContext o)
        {
            return MiraiHTTPContexts[o.UUID];
        }
        public OrichaltContext PutPlatformContext(OneBotContext context)
        {
            MessageScope scope;
            switch (context.Type)
            {
                case MessageType.Group:
                    scope = MessageScope.Public;
                    break;
                case MessageType.Private:
                    scope = MessageScope.Private;
                    break;
                default:
                    scope = MessageScope.Public;
                    break;
            }
            var o = new OrichaltContext(context.RawMessage, MessagePlatform.OneBot, scope);
            OneBotContexts[o.UUID] = context;
            return o;
        }
        public OrichaltContext PutPlatformContext(MiraiHTTPContext context)
        {
            MessageScope scope;
            switch (context.Type)
            {
                case MessageType.Group:
                    scope = MessageScope.Public;
                    break;
                case MessageType.Private:
                    scope = MessageScope.Private;
                    break;
                default:
                    scope = MessageScope.Public;
                    break;
            }
            var o = new OrichaltContext(context.RawMessage, MessagePlatform.MiraiHTTP, scope);
            MiraiHTTPContexts[o.UUID] = context;
            return o;
        }
        public OrichaltContext PutPlatformContext(KaiheilaContext context)
        {
            throw new NotImplementedException();
        }
        public OrichaltContext PutPlatformContext(QQChannelContext context)
        {
            throw new NotImplementedException();
        }
        public static OrichaltContext PutPlatformContext(TestContext context)
        {
            var o = new OrichaltContext(context.RawMessage, MessagePlatform.Test, MessageScope.Public);
            return o;
        }
        public void DisposeOrichaltContext(OrichaltContext context)
        {
            switch (context.Platform)
            {
                case MessagePlatform.OneBot:
                    OneBotContexts.Remove(context.UUID);
                    break;
                case MessagePlatform.MiraiHTTP:
                    MiraiHTTPContexts.Remove(context.UUID);
                    break;
            }
            // 往下扩展各个平台
        }

    }
}
