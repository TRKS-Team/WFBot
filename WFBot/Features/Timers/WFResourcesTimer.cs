using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Humanizer;
using Manganese.Array;
using WFBot.Features.Resource;
using WFBot.Features.Timers.Base;

namespace WFBot.Features.Timers
{
    class WFResourcesTimer : WFBotTimer
    {
        // TEST
        public WFResourcesTimer() : base(1.Minutes())
        {
        }

        protected override void Tick()
        {
            var resources = WFResourcesManager.WFResourceDic.Values
                .SelectMany(v => v)
                .GroupBy(v => v.Category)
                .Select(v => v.First().IsGitHub ? v.Take(1) : v)
                .SelectMany(v => v);
            Task.WhenAll(resources.Select(r => r.Update()));
        }
    }
}
