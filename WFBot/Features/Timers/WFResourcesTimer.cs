using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Humanizer;
using WFBot.Features.Resource;
using WFBot.Features.Timers.Base;

namespace WFBot.Features.Timers
{
    class WFResourcesTimer : WFBotTimer
    {
        // TEST
        public WFResourcesTimer() : base(10.Minutes())
        {
        }

        protected override void Tick()
        {
            var resources = new List<IWFResource>();
            resources = WFResourcesManager.WFResourceDic.Values.Aggregate(resources, (current, value) => current.Concat(value).ToList()); 
            Task.WhenAll(resources.Select(r => r.Update()));
        }
    }
}
