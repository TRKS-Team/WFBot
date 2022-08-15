using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace WFBot.Features.Timers.Base
{
    abstract class WFBotTimer
    {
        internal readonly Timer timer;
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
        public CalledByTimerAttribute(Type type)
        {
        }
    }
}
