using System;
using System.Timers;
using GammaLibrary.Extensions;
using WarframeAlertingPrime.SDK.Models.Core;
using WarframeAlertingPrime.SDK.Models.Enums;
using WFBot.Windows;

namespace WFBot.Features.Utils
{
    public class WFAApi
    {
        public bool isWFA = !Config.Instance.ClientId.IsNullOrWhiteSpace() &&
                     !Config.Instance.ClientSecret.IsNullOrWhiteSpace();

        public Client wfaClient;

        private Timer timer = new Timer(TimeSpan.FromHours(2).TotalMilliseconds);
        public WFAApi()
        {
            // UpdateAccessToken();
            UpdateClient();
            timer.Elapsed += (s, e) => /*UpdateAccessToken();*/ UpdateClient();
            timer.Start();
        }

        public void UpdateClient()
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
                if (DateTime.Now - Config.Instance.Last_update > TimeSpan.FromDays(7))
                {
                    wfaClient = new Client(Config.Instance.ClientId, Config.Instance.ClientSecret, new[]
                    {
                        "wfa.basic", "wfa.riven.query", "wfa.user.read", "wfa.lib.query"

                    }, wfaPlatform);
                    wfaClient.InitAsync().Wait();
                    Config.Instance.Last_update = DateTime.Now;
                    Config.Instance.AcessToken = wfaClient.Token;
                    Config.Save();
                }
                else
                {
                    wfaClient = new Client(Config.Instance.AcessToken, wfaPlatform);
                }
            }

        }
    }
}
