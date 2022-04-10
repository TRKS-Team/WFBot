using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using WFBot.Features.Utils;
using WFBot.Orichalt.OrichaltConnectors;

namespace WFBot.Orichalt
{
    public enum MessagePlatform
    {
        OneBot,
        Kaiheila,
        QQChannel
    }

    public enum MessageRange
    {
        Public,
        Private
    }
    public class OrichaltContext
    {
        public OrichaltContext(string plainMessage, MessagePlatform platform, MessageRange range)
        {
            PlainMessage = plainMessage;
            Platform = platform;
            Range = range
            UUID = Guid.NewGuid().ToString();
            lifeTimer = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
            lifeTimer.Elapsed += (sender, args) =>
            {
                Dispose();
            };
            lifeTimer.Start();
        }

        public string UUID { get; set; }
        public string PlainMessage { get; set; }
        public MessagePlatform Platform { get; set; }
        public MessageRange Range { get; set; }
        private Timer lifeTimer;

        public void Dispose()
        {
            MiguelNetwork.OrichaltContextManager.DisposeOrichaltContext(this);
        }

    }
    public class OrichaltContextManager
    {
        public Dictionary<string, OneBotContext> OneBotContexts = new Dictionary<string, OneBotContext>();
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
        public OrichaltContext PutPlatformContext(KaiheilaContext context)
        {
            throw new NotImplementedException();
        }
        public OrichaltContext PutPlatformContext(QQChannelContext context)
        {
            throw new NotImplementedException();
        }

        public void DisposeOrichaltContext(OrichaltContext context)
        {
            OneBotContexts.Remove(context.UUID);
            // 往下扩展各个平台
        }

    }
}
