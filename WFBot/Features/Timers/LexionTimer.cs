using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Humanizer;
using WFBot.Features.Resource;
using WFBot.Features.Timers.Base;
using WFBot.Features.Utils;

namespace WFBot.Features.Timers
{
    class LexionTimer : WFBotTimer
    {
        public LexionTimer() : base((Config.Instance.GitHubOAuthKey.IsNullOrWhiteSpace() ? 600 : 60).Seconds())
        {
            
        }

        protected override async void Tick()
        {
            try
            {
                if (Config.Instance.UpdateLexion)
                {
                    await WFResources.UpdateLexion();
                }
            }
            catch (Exception)
            {
                //Messenger.SendDebugInfo(exception.ToString());

            }
        }
    }
}
