using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
                    .Format(), QR, B);
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
                        .Format(), true, B);
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
                        .Format(), QR, true);
            }

            return null;
        }

        [Matchers("紫卡")]
        [CombineParams]
        [DoNotMeasureTime]
        Task<string> Riven(string word)
        {
            word = word.Format();
            return _wmaSearcher.SendRivenAuctions(word);
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