using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using TextCommandCore;
using WarframeAlertingPrime.SDK.Models.Core;
using WarframeAlertingPrime.SDK.Models.Others;
using WFBot.Features.Resource;
using WFBot.Features.Utils;
using WFBot.Utils;
using Order = WarframeAlertingPrime.SDK.Models.User.Order;

namespace WFBot.Features.Common
{
    public class RMSearcher
    {
        private Client wfaClient => WFResources.WFAApi.WfaClient;
        private bool isWFA => WFResources.WFAApi.isWFA;
        private WFTranslator translator => WFResources.WFTranslator;


        /*public string GetAccessToken()
        {
            var body = $"client_id={Config.Instance.ClientId}&client_secret={Config.Instance.ClientSecret}&grant_type=client_credentials";
            var header = new WebHeaderCollection
            {
                { "Content-Type", "application/x-www-form-urlencoded" }
            };
            var accessToken = WebHelper.UploadJson<AccessToken>("https://api.richasy.cn/connect/token", body, header).access_token;

            Config.Instance.Last_update = DateTime.Now;
            Config.Save();
            return accessToken;
        }

        public void UpdateAccessToken()
        {
            if (isWFA && DateTime.Now - Config.Instance.Last_update > TimeSpan.FromDays(7))
            {
                Config.Instance.AcessToken = GetAccessToken();
                Config.Save();
            }
        }*/

        /*public List<RivenInfo> GetRivenInfos(string weapon)
        {
            var header = new WebHeaderCollection();
            var count = Config.Instance.WFASearchCount;
            var platform = Config.Instance.Platform.GetSymbols().First();
            if (Config.Instance.Platform == Platform.NS)
            {
                platform = "ns";
            }
            header.Add("Authorization", $"Bearer {Config.Instance.AcessToken}");
            header.Add("Platform", platform);
            header.Add("Weapon", weapon.ToBase64());
            return WebHelper.DownloadJson<List<RivenInfo>>($"https://api.richasy.cn/wfa/rm/riven", header).Where(info => info.isSell == 1).Take(count).ToList(); // 操 云之幻好蠢 为什么不能在请求里限制是买还是卖
        }*/

        public async Task<List<Order>> GetRivenOrders(string weapon)
        {
            var option = new SearchRivenOrderOption { Category = "", IsVeiled = false, OrderType = "sell", Page = 1, PageSize = 20, Weapon = Uri.EscapeUriString(weapon) };
            var orders = (await wfaClient.QueryRivenOrdersAsync(option) ?? throw new CommandException("由于未知原因, 返回的数据为空.")).Items;
            translator.TranslateRivenOrders(orders);
            return orders;
        }

        public async Task<List<RivenData>> GetRivenDatas()
        {
            var info = await WebHelper.DownloadJsonAsync<List<RivenData>>(
                "http://n9e5v4d8.ssl.hwcdn.net/repos/weeklyRivensPC.json");
            info.ForEach(d => d.compatibility = d.compatibility.IsNullOrEmpty() ? "" : d.compatibility.Replace("<ARCHWING> ", "").Format());
            return info;
        }

        public async Task<string> SendRivenInfos(string weapon)
        {
            var sb = new StringBuilder();

            try
            {
                if (isWFA)
                {
                    var weaponinfo = translator.GetMatchedWeapon(weapon.Format());
                    if (weaponinfo.Any())
                    {
                        if (Config.Instance.NotifyBeforeResult)
                        {
                            AsyncContext.SendGroupMessage("好嘞, 等着, 着啥急啊, 这不帮你查呢.");
                        }
                        var orders = await GetRivenOrders(weaponinfo.First().name);
                        var data = (await GetRivenDatas()).Where(d => d.compatibility.Format() == weapon).ToList();
                        var msg = orders.Any() ? WFFormatter.ToString(orders.Take(Config.Instance.WFASearchCount).ToList(), data, weaponinfo.First()) : $"抱歉, 目前紫卡市场没有任何出售: {weapon} 紫卡的用户.".AddRemainCallCount();
                        sb.AppendLine(msg.AddPlatformInfo());
                    }
                    else
                    {
                        sb.AppendLine($"武器 {weapon} 不存在.");
                        var similarlist = translator.GetSimilarItem(weapon, "rm");
                        if (similarlist.Any())
                        {
                            sb.AppendLine("请问这下面有没有你要找的武器呢?（可尝试复制下面的名称来进行搜索)");
                            foreach (var item in similarlist)
                            {
                                sb.AppendLine($"    {item}");
                            }
                        }

                    }
                }
                else
                {
                    sb.AppendLine("本机器人没有 WFA 授权, 本功能无法使用, 请联系机器人管理员.");
                }
            }
            catch (WebException)
            {
                sb.AppendLine("经过我们的多次尝试, 依然无法访问紫卡市场. 如果你不能谅解, 有本事顺着网线来打我呀.");
            }
            return sb.ToString().Trim();
        }
    }
}
