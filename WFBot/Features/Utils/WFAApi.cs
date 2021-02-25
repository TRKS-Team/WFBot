using System;
using System.Runtime.CompilerServices;
using System.Timers;
using GammaLibrary.Extensions;
using WarframeAlertingPrime.SDK.Models.Core;
using WarframeAlertingPrime.SDK.Models.Enums;
using WFBot.Features.Timers;
using WFBot.Features.Timers.Base;
using WFBot.Windows;

namespace WFBot.Features.Utils
{
    public class WFAApi
    {
        public bool isWFA => !Config.Instance.ClientId.IsNullOrWhiteSpace() &&
                     !Config.Instance.ClientSecret.IsNullOrWhiteSpace();

        readonly object wfaClientLock = new object();
        public Client WfaClient
        {
            get
            {
                lock (wfaClientLock)
                {
                    return wfaClient;
                }
            }
            // ReSharper disable once InconsistentlySynchronizedField
            // I know what i'm doing
            private set => wfaClient = value;
        }
        
        Client wfaClient;

        public WFAApi()
        {
            // UpdateAccessToken();
            UpdateClient();
        }

        [CalledByTimer(typeof(WFATimer))]
        public void UpdateClient()
        {
            lock (wfaClientLock)
            {
                PlatformType wfaPlatform;
                switch (Config.Instance.Platform)
                {
                    case Platform.PC:
                        wfaPlatform = PlatformType.PC;
                        break;
                    case Platform.NS:
                        wfaPlatform = PlatformType.Switch;
                        break;
                    case Platform.PS4:
                        wfaPlatform = PlatformType.PS4;
                        break;
                    case Platform.XBOX:
                        wfaPlatform = PlatformType.Xbox;
                        break;
                    default:
                        wfaPlatform = PlatformType.PC;
                        break;
                }

                if (isWFA) // 今后所有用到client的地方都要判断一次
                {
                    if (DateTime.Now - Config.Instance.Last_update > TimeSpan.FromDays(7) || Config.Instance.AcessToken.IsNullOrEmpty())
                    {
                        WfaClient = new Client(Config.Instance.ClientId, Config.Instance.ClientSecret, new[]
                        {
                            "wfa.basic", "wfa.riven.query", "wfa.user.read", "wfa.lib.query"

                        }, wfaPlatform);
                        WfaClient.InitAsync().Wait();
                        Config.Instance.Last_update = DateTime.Now;
                        Config.Instance.AcessToken = WfaClient.Token;
                        Config.Save();
                    }
                    else
                    {
                        WfaClient = new Client(Config.Instance.AcessToken, wfaPlatform);
                    }
                }
            }

        }
    }
}
