using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WFBot.Features.Timers.Base
{
    abstract class WFBotTimer
    {
        private readonly Timer timer;
        public WFBotTimer(TimeSpan delay)
        {
            timer = new Timer(delay.TotalMilliseconds);
            timer.Elapsed += (sender, args) => Tick();
            timer.Start();
        }

        protected abstract void Tick();
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class CalledByTimerAttribute : Attribute
    {
    }
}
