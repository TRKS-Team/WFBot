using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TextCommandCore;
using WFBot.Features.Utils;
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

        [Matchers("help", "帮助", "功能", "救命")]
        void HelpDoc()
        {
            // 为了社区的良性发展, 请不要随意修改.
            AsyncContext.SendGroupMessage($@"欢迎查看机器人唯一指定帮助文档
{VersionText}
在线最新文档: https://github.com/TRKS-Team/WFBot/blob/universal/README.md
项目地址: https://github.com/TRKS-Team/WFBot
赞助(乞讨)地址: https://afdian.net/@TheRealKamisama
您的赞助会成为我们维护本项目的动力.
本机器人为公益项目, 间断维护中.
如果你想给你的群也整个机器人, 请在上方项目地址了解");
            AsyncContext.SendGroupMessage(@"作者: TheRealKamisama
参数说明: <>为必填参数, []为选填参数, {}为附加选填参数, ()为补充说明
如果群里没有自动通知 请务必检查是否启用了通知功能
    /s船 | 当前的Sentient异常事件
    /遗物 <关键词> | 查询遗物的内容
    /wiki [关键词] | 搜索 wiki 上的页面
    /午夜电波 | 每日每周即将过期的挑战
    /机器人状态 | 机器人目前的运行状态
    /警报 | 所有警报
    /入侵 | 所有入侵
    /突击 | 所有突击
    /活动 | 所有活动
    /虚空商人 | 奸商的状态
    /平原 | 地球&金星&火卫二平原的时间循环
    /查询 <物品名称> {-qr} {-b} | 查询 WarframeMarket, 查询未开紫卡请输入: 手枪未开紫卡
    /紫卡 <武器名称> | 紫卡市场 数据来自 WM 紫卡市场
    /WFA紫卡 <武器名称> | 紫卡市场 数据来自 WFA 紫卡市场
    /地球赏金 [1-5]| 地球平原的全部/单一赏金任务
    /金星赏金 [1-5]| 金星平原的全部/单一赏金任务
    /火卫赏金 [1-5]| 火卫二平原的全部/单一赏金任务
    /裂隙 [1-5]| 查询全部/单一种类裂隙
    /翻译 <关键词> |（eg. 致残突击 犀牛prime) 中 -> 英 / 英 -> 中 翻译
    /小小黑 小小黑的信息
*私聊*管理命令:
    /添加群 ******* 群号 | 启用 [群号] 对应的群的通知功能
    /删除群 ******* 群号 | 禁用 [群号] 对应的群的通知功能
    没有启用通知的群不会收到机器人的任务提醒
");
        }
        
        [DoNotMeasureTime]
        [AddPlatformInfo]
        [Matchers("status", "状态", "机器人状态", "机器人信息", "我需要机器人")]
        async Task<string> Status()
        {
            var sb = new StringBuilder();
            var q1 = WebHelper.TryGet("https://warframestat.us");
            var q2 = WebHelper.TryGet("https://api.warframe.market/v1/items/valkyr_prime_set/orders?include=item");
            var commitTask = Task.Run(() =>
                CommitsGetter.Get("https://api.github.com/repos/TRKS-Team/WFBot/commits?per_page=5"));
            // var q3 = Task.Run(() => WebHelper.TryGet("https://api.richasy.cn/wfa/rm/riven"));
            // var q4 = Task.Run(() => WebHelper.TryGet("https://10o.io/kuvalog.json"));
            
            var apistat = await q1;
            var wmstat = await q2;
            // var wfastat = await q3;
            // var kuvastat = await q4;
            if (apistat.IsOnline && wmstat.IsOnline /*&& wfastat.IsOnline && kuvastat.IsOnline*/)
            {
                sb.AppendLine("机器人状态: 一切正常");
            }
            else
            {
                sb.AppendLine("机器人状态: 不正常");
            }

            sb.AppendLine($"WFBot 版本: {VersionText}");

            sb.AppendLine($"    任务API:  {(apistat.IsOnline ? $"{apistat.Latency}ms [在线]" : "[离线]")}");
            sb.AppendLine($"    WarframeMarket: {(wmstat.IsOnline ? $"{wmstat.Latency}ms [在线]" : "[离线]")}]");
            // sb.AppendLine($"    WFA紫卡市场: {wfastat.Latency}ms [{(wfastat.IsOnline ? "在线" : "离线")}]");
            // sb.AppendLine($"    赤毒/仲裁API: {kuvastat.Latency}ms [{(kuvastat.IsOnline ? "在线" : "离线")}]");
            var commit = (await commitTask)?.Format() ?? "GitHub Commit 获取异常, 可能是请求次数过多, 如果你是机器人主人, 解决方案请查看 FAQ.";
            sb.AppendLine(commit);
            return sb.ToString();
        }
    }
}
