using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using WFBot.Features.Resource;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Features.Common
{
    class WMASearcher
    {
        private WFTranslator translator => WFResources.WFTranslator;
        private WMRAttribute[] attributes => WFResources.WMAuction.Attributes;
        private WMRRiven[] rivens => WFResources.WMAuction.Rivens;
        private static string platform => Config.Instance.Platform == Platform.NS ? "switch" : Config.Instance.Platform.GetSymbols().First();
        // 这是给WarframeMarketAuctions用的
        public async Task<List<RivenAuction>> GetRivenAuctions(string urlname)
        {
            var header = new List<KeyValuePair<string, string>>
                {new KeyValuePair<string, string>("Platform", platform)};
            var auctions = await WebHelper.DownloadJsonAsync<RivenAuctions>(
                $"https://api.warframe.market/v1/auctions/search?type=riven&weapon_url_name={urlname}&sort_by=price_desc", header);
            return auctions.Payload.Auctions;
        }
        public async Task<string> SendRivenAuctions(string name)
        {
            var sb = new StringBuilder();
            // 规范一下 武器的名字都按中文传递, 需要英文/更详细信息的时候会调用translate里的weaponinfo或rivens
            var matchedweapon = rivens.Where(r => r.ItemName == name).ToList();
            if (matchedweapon.Any())
            {
                var weapon = matchedweapon.First();
                var weaponinfo = translator.GetMatchedWeapon(weapon.ItemName).First();
                if (Config.Instance.NotifyBeforeResult)
                {
                    AsyncContext.SendGroupMessage("好嘞, 等着, 着啥急啊, 这不帮你查呢.");
                }

                var auctions = GetRivenAuctions(weapon.UrlName).Result;

                var msg = WFFormatter.ToString(auctions.Take(Config.Instance.WFASearchCount).ToList(), weaponinfo).AddPlatformInfo();
                sb.AppendLine(msg);
            }
            else
            {
                sb.AppendLine($"武器 {name} 不存在");
                var similarlist = translator.GetSimilarItem(name, "rm");
                if (similarlist.Any())
                {
                    sb.AppendLine("请问这下面有没有你要找的武器呢?（可尝试复制下面的名称来进行搜索)");
                    foreach (var item in similarlist)
                    {
                        sb.AppendLine($"    {item}");
                    }
                }
            }

            return sb.ToString().Trim();
        }

    }
}
