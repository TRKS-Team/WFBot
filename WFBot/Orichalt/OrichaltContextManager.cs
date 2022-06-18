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
        OneBot,
        Kaiheila,
        QQChannel,
        MiraiHTTP,
        Test,
        Unknown
    }

    public enum MessageRange
    {
        Public,
        Private
    }
    public class OrichaltContext : IDisposable
    {
        public OrichaltContext(string plainMessage, MessagePlatform platform, MessageRange range)
        {
            PlainMessage = plainMessage;
            Platform = platform;
            Range = range;
            UUID = Guid.NewGuid().ToString();
            lifeTask = Task.Delay(TimeSpan.FromMinutes(10)).ContinueWith(t =>
            {
                Dispose();
            });
        }

        public string UUID { get; set; }
        public string PlainMessage { get; set; }
        public MessagePlatform Platform { get; set; }
        public MessageRange Range { get; set; }
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
        public OrichaltContext PutPlatformContext(OneBotContext context)
        {
            MessageRange range;
            switch (context.Type)
            {
                case MessageType.Group:
                    range = MessageRange.Public;
                    break;
                case MessageType.Private:
                    range = MessageRange.Private;
                    break;
                default:
                    range = MessageRange.Public;
                    break;
            }
            var o = new OrichaltContext(context.RawMessage, MessagePlatform.OneBot, range);
            OneBotContexts[o.UUID] = context;
            return o;
        }
        public OrichaltContext PutPlatformContext(MiraiHTTPContext context)
        {
            MessageRange range;
            switch (context.Type)
            {
                case MessageType.Group:
                    range = MessageRange.Public;
                    break;
                case MessageType.Private:
                    range = MessageRange.Private;
                    break;
                default:
                    range = MessageRange.Public;
                    break;
            }
            var o = new OrichaltContext(context.RawMessage, MessagePlatform.OneBot, range);
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
            var o = new OrichaltContext(context.RawMessage, MessagePlatform.Test, MessageRange.Public);
            return o;
        }
        public void DisposeOrichaltContext(OrichaltContext context)
        {
            OneBotContexts.Remove(context.UUID);
            // 往下扩展各个平台
        }

    }
}
