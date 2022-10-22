using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WFBot.Features.ImageRendering;
using WFBot.Features.Telemetry;
using WFBot.Features.Utils;
using WFBot.Orichalt;
using WFBot.TextCommandCore;
using WFBot.Utils;

namespace WFBot.Features.Commands
{
    public partial class CommandsHandler
    {
        string VersionText => WFBotCore.IsOfficial ? $"WFBot 官方 {(WFBotCore.Version)}" : $"WFBot 非官方 {(WFBotCore.Version)}";

        [MatchersIgnoreCase("wfbotversion", "wfbot-version")]
        string Version()
        {
            return VersionText;
        }

        [Matchers("help", "帮助", "功能")]
        void HelpDoc()
        {
            MiguelNetwork.Reply(O, WFFormatter.HelpCommandSegment1());
            if (AsyncContext.GetUseImageRendering())
            {
                SendImage(ImageRenderHelper.SimpleImageRendering(WFFormatter.HelpCommandSegment2()));            
            }
            else
            {
                MiguelNetwork.Reply(O, WFFormatter.HelpCommandSegment2());
            }
        }
        
        [DoNotMeasureTime]
        [AddPlatformInfo]
        [Matchers("状态", "status", "机器人状态", "机器人信息", "我需要机器人")]
        async Task<string> Status()
        {
            var sb = new StringBuilder();
            var q1 = WebHelper.TryGet("https://warframestat.us");
            var q2 = WebHelper.TryGet("https://api.warframe.market/v1/items/valkyr_prime_set/orders?include=item");
            var q3 = WebHelper.TryGet("https://wfbot.kraber.top:8888/Resources/");
            var commitTask = Task.Run(() =>
                CommitsGetter.Get("https://api.github.com/repos/TRKS-Team/WFBot/commits?per_page=5"));
            // var q3 = Task.Run(() => WebHelper.TryGet("https://api.richasy.cn/wfa/rm/riven"));
            // var q4 = Task.Run(() => WebHelper.TryGet("https://10o.io/kuvalog.json"));
            
            var apistat = await q1;
            var wmstat = await q2;
            var cdnstat = await q3;
            var commitResult = await commitTask;
            // var wfastat = await q3;
            // var kuvastat = await q4;
            
            if (AsyncContext.GetUseImageRendering())
            {
                SendImage(ImageRenderHelper.SimpleImageRendering(WFFormatter.FormatStatusCommand(apistat, wmstat, cdnstat, sb, commitResult)));
                OutputStringBuilder.Value.Clear();
                return "";
            }
            else
            {
                return WFFormatter.FormatStatusCommand(apistat, wmstat, cdnstat, sb, commitResult);
            }
        }

        
    }
}
