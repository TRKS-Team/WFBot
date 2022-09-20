using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WFBot.Features.ImageRendering;
using WFBot.Features.Resource;
using WFBot.Features.Utils;
using WFBot.Orichalt;
using WFBot.TextCommandCore;
using WFBot.Utils;

namespace WFBot.Features.Commands
{

    public partial class CommandsHandler
    {
        static Dictionary<string, string> LastWMSearchs = new Dictionary<string, string>();
        // 三巨头
        [Matchers("查询", "wm")]
        [CombineParams]
        [DoNotMeasureTime]
        Task<string> WM(string word)
        {
            const string quickReply = " -qr";
            const string buyer = " -b";
            var QR = word.ToLower().Contains(quickReply);
            var B = word.ToLower().Contains(buyer);

            LastWMSearchs[AsyncContext.GetOrichaltContext().GetSenderIdentifier()] = word;
            // 小屎山
            return _wmSearcher.SendWMInfo(
                word.ToLower()
                    .Replace(quickReply, "")
                    .Replace(buyer, "")
                    .Format(), QR, B, SendImageAndText);
        }

        [MatchersIgnoreCase("qr")]
        [DoNotMeasureTime]
        Task<string> WMQR()
        {
            const string quickReply = " -qr";
            const string buyer = " -b";
            var sender = AsyncContext.GetOrichaltContext().GetSenderIdentifier();
            if (LastWMSearchs.ContainsKey(sender))
            {
                var lastWmSearch = LastWMSearchs[sender];
                LastWMSearchs.Remove(sender);
                var B = lastWmSearch.ToLower().Contains(buyer);
                return _wmSearcher.SendWMInfo(
                    lastWmSearch.ToLower()
                        .Replace(quickReply, "")
                        .Replace(buyer, "")
                        .Format(), true, B, SendImageAndText);
            }

            return null;
        }

        [MatchersIgnoreCase("buyer")]
        [DoNotMeasureTime]
        Task<string> WMB()
        {
            const string quickReply = " -qr";
            const string buyer = " -b";
            var sender = AsyncContext.GetOrichaltContext().GetSenderIdentifier();
            if (LastWMSearchs.ContainsKey(sender))
            {
                var lastWmSearch = LastWMSearchs[sender];
                LastWMSearchs.Remove(sender);
                var QR = lastWmSearch.ToLower().Contains(quickReply);
                return _wmSearcher.SendWMInfo(
                    lastWmSearch.ToLower()
                        .Replace(quickReply, "")
                        .Replace(buyer, "")
                        .Format(), QR, true, SendImageAndText);
            }

            return null;
        }

        [Matchers("紫卡")]
        [CombineParams]
        [DoNotMeasureTime]
        Task<string> Riven(string word)
        {
            word = word.Format();
            
            return SendRivenAuctions(word);
        }
        
        // 等有空了挪回去
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

                if (AsyncContext.GetUseImageRendering())
                {
                    var image = ImageRenderHelper.RivenAuction(auctions.Take(Config.Instance.WFASearchCount).ToList(), weapon);
                    SendImage(image);
                    return null;
                }
                else
                {
                    var msg = WFFormatter.ToString(auctions.Take(Config.Instance.WFASearchCount).ToList(), weapon).AddPlatformInfo();
                    sb.AppendLine(msg);

                }

            }
            else
            {
                var similarlist = translator.GetSimilarItem(name, "wma");
                WFFormatter.WeaponNotExists(name, sb, similarlist);
            }

            return sb.ToString().Trim();
        }
        /*
        [Matchers("WFA紫卡", "wfa紫卡")]
        [CombineParams]
        [DoNotMeasureTime]
        Task<string> WFARiven(string word)
        {
            word = word.Format();
            return _rmSearcher.SendRivenInfos(word);
        }*/
    }
}