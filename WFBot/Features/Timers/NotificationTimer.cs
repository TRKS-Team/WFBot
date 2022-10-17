using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using WFBot.Features.Timers.Base;

namespace WFBot.Features.Timers
{
    class NotificationTimer : WFBotTimer
    {
        public NotificationTimer() : base(1.Minutes())
        {
        }

        protected override void Tick()
        {
            WFBotCore.Instance.NotificationHandler.Update();
        }
    }
}
