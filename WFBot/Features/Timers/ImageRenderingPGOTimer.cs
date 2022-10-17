using Humanizer;
using WFBot.Features.ImageRendering;
using WFBot.Features.Timers.Base;

namespace WFBot.Features.Timers
{
    class ImageRenderingPGOTimer : WFBotTimer
    {
        public ImageRenderingPGOTimer() : base(60.Seconds())
        {
        }

        protected override void Tick()
        {
            ImageRenderingPGO.Tick();
        }
    }
}
