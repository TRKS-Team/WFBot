using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using WFBot.Orichalt.OrichaltConnectors;

namespace WFBot.Orichalt
{
    public enum MessagePlatform
    {
        OneBot,
        Kaiheila,
        QQChannel
    }
    public class OrichaltContext
    {
        public OrichaltContext(string plainCommand, MessagePlatform platform)
        {
            PlainCommand = plainCommand;
            Platform = platform;
            UUID = Guid.NewGuid().ToString();
            lifeTimer = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
            lifeTimer.Elapsed += (sender, args) =>
            {
                Dispose();
            };
            lifeTimer.Start();
        }

        public string UUID { get; set; }
        public string PlainCommand { get; set; }
        public MessagePlatform Platform { get; set; }
        private Timer lifeTimer;

        public void Dispose()
        {
            OrichaltContextManager.OneBotContexts.Remove(UUID);
        }

    }
    public static class OrichaltContextManager
    {
        public static Dictionary<string, OneBotContext> OneBotContexts = new Dictionary<string, OneBotContext>();

        public static MessageContextBase GetPlatformContext(OrichaltContext context)
        {
            return context.Platform switch
            {
                MessagePlatform.OneBot => OneBotContexts[context.UUID],
                _ => new MessageContextBase()
            };
        }

    }
}
