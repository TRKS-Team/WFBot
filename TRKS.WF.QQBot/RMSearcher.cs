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
        public Timer timer = new Timer(TimeSpan.FromHours(2).TotalMilliseconds);
        public WFTranslator translator = new WFTranslator();
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
            if (!string.IsNullOrEmpty(Config.Instance.ClientId) && !string.IsNullOrEmpty(Config.Instance.ClientSecret))
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
            header.Add("Authorization", $"Bearer {Config.Instance.AcessToken}");
            header.Add("Platform", "pc");
            header.Add("Weapon", weapon.ToBase64());
            return WebHelper.DownloadJson<List<RivenInfo>>($"https://api.richasy.cn/wfa/rm/riven?Count={count}", header);
        }

        public void SendRiveninfos(string group, string weapon)
        {
            if (translator.ContainsWeapon(weapon))
            {
                var info = GetRiveninfos(weapon);
                var msg = WFFormatter.ToString(info);
                Messenger.SendGroup(group, msg);
            }
            else
            {
                Messenger.SendGroup(group, $"武器{weapon}不存在,请检查格式(悦音prime)");
            }
        }
    } 
}
