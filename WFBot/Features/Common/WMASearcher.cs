using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using WFBot.Features.Resource;
using WFBot.Features.Utils;
using WFBot.Orichalt;
using WFBot.Utils;

namespace WFBot.Features.Common
{
    public class RivenAuctionOption
    {
        // TODO
    }
    class WMASearcher
    {
        private WFTranslator translator => WFResources.WFTranslator;
        private WMAAttribute[] attributes => WFResources.WMAuction.Attributes;
        private WeaponInfo[] weaponInfos => WFResources.Weaponinfos;
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
            // 规范一下 武器的名字都按中文传递 使用WFResources.WeaponInfos来获取在判断武器存在后所传递的对象
            var weapons = weaponInfos.Where(r => r.zhname.Format() == name).ToList();
            if (weapons.Any())
            {
                var weapon = weapons.First();
                if (Config.Instance.NotifyBeforeResult)
                {
                    MiguelNetwork.Reply(AsyncContext.GetOrichaltContext(), WFFormatter.Searching(weapon.zhname));
                }

                var auctions = await GetRivenAuctions(weapon.urlname);

                var msg = WFFormatter.ToString(auctions.Take(Config.Instance.WFASearchCount).ToList(), weapon).AddPlatformInfo();
                sb.AppendLine(msg);
            }
            else
            {
                var similarlist = translator.GetSimilarItem(name, "wma");
                WFFormatter.WeaponNotExists(name, sb, similarlist);
            }

            return sb.ToString().Trim();
        }

        
    }
}
