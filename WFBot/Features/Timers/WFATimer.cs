using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Humanizer;
using WFBot.Features.Resource;
using WFBot.Features.Timers.Base;

namespace WFBot.Features.Timers
{
    class WFATimer : WFBotTimer
    {
        public WFATimer() : base(3.Hours())
        {
        }
        protected override void Tick()
        {
            try
            {
                WFResources.WFAApi.UpdateClient();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }
    }
}
