using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WFBot.Features.Utils;
using WFBot.TextCommandCore;
using WFBot.Utils;

namespace WFBot.Features.Commands
{

    public partial class CommandsHandler
    {
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

            // 小屎山
            return _wmSearcher.SendWMInfo(
                word.ToLower()
                    .Replace(quickReply, "")
                    .Replace(buyer, "")
                    .Format(), QR, B);
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