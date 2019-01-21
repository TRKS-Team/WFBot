using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Settings;

namespace TRKS.WF.QQBot
{
    public class RMSearcher
    {
        private Timer timer = new Timer(TimeSpan.FromHours(2).TotalMilliseconds);
        private WFTranslator translator = WFResource.WFTranslator;

        private bool isWFA = !string.IsNullOrEmpty(Config.Instance.ClientId) &&
                             !string.IsNullOrEmpty(Config.Instance.ClientSecret);

        private string platform = Config.Instance.Platform.ToString();
        public RMSearcher()
        {
            UpdateAccessToken();
            timer.Elapsed += (sender, eventArgs) =>
            {
                UpdateAccessToken();
            };
            timer.Start();
        }
        public string GetAccessToken()
        {
            var body = $"client_id={Config.Instance.ClientId}&client_secret={Config.Instance.ClientSecret}&grant_type=client_credentials";
            var header = new WebHeaderCollection
            {
                { "Content-Type", "application/x-www-form-urlencoded" }
            };
            var accesstoken = WebHelper.UploadJson<AccessToken>("https://api.richasy.cn/connect/token", body, header).access_token;
            Config.Instance.Last_update = DateTime.Now;
            Config.Save();
            return accesstoken;
            
        }

        public void UpdateAccessToken()
        {
            if (isWFA)
            {
                if (DateTime.Now - Config.Instance.Last_update > TimeSpan.FromDays(7))
                {
                    Config.Instance.AcessToken = GetAccessToken();
                    Config.Save();
                }
            }
        }

        public List<RivenInfo> GetRiveninfos(string weapon)
        {
            var header = new WebHeaderCollection();
            var count = 5;
            var platform = Config.Instance.Platform.GetSymbols().First();
            if (Config.Instance.Platform == Platform.NS)
            {
                platform = "ns";
            }
            header.Add("Authorization", $"Bearer {Config.Instance.AcessToken}");
            header.Add("Platform", platform);
            header.Add("Weapon", weapon.ToBase64());
            return WebHelper.DownloadJson<List<RivenInfo>>($"https://api.richasy.cn/wfa/rm/riven", header).Where(info => info.isSell == 1).Take(count).ToList(); // 操 云之幻好蠢 为什么不能在请求里限制是买还是卖
        }

        public void SendRiveninfos(string group, string weapon)
        {
            if (isWFA)
            {
                if (translator.ContainsWeapon(weapon))
                {
                    var info = GetRiveninfos(weapon);
                    var msg = "";
                    if (info.Count > 0)
                    {
                        msg = WFFormatter.ToString(info);
                    }
                    else
                    {
                        msg = $"抱歉, 目前紫卡市场没有任何出售: {weapon} 紫卡的用户.";
                    }
                    Messenger.SendGroup(group, msg + $"\r\n机器人目前运行的平台是: {platform}");
                }
                else
                {
                    Messenger.SendGroup(group, $"武器{weapon}不存在, 请检查格式(请注意: 悦音prime)");
                }
            }
            else
            {
                Messenger.SendGroup(group, "本机器人没有WFA授权,本功能无法使用,请联系机器人管理员.");
            }

            
        }
    } 
}
