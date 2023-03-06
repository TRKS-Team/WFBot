using GammaLibrary.Extensions;
using Humanizer;
using WFBot.Features.Timers.Base;
using WFBot.Orichalt;
using WFBot.Utils;

namespace WFBot.Features.Timers
{
    public class BotMarketHeartbeatTimer : WFBotTimer
    {
        public BotMarketHeartbeatTimer() : base(30.Minutes())
        {

        }

        protected override void Tick()
        {
            if (Config.Instance.Miguel_Platform == MessagePlatform.Kook && !Config.Instance.BotMarketUUID.IsNullOrEmpty())
            {
                WebHelper.DownloadStringAsync("http://bot.gekj.net/api/v1/online.bot",
                    new List<KeyValuePair<string, string>>
                    {
                        new("uuid", Config.Instance.BotMarketUUID)
                    }).Wait();
            }
        }
    }
}
