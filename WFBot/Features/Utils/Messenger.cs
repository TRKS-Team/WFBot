using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using WFBot.Connector;
using WFBot.MahuaEvents;
using WFBot.Utils;
using Timer = System.Timers.Timer;

namespace WFBot.Features.Utils
{

    public static class Messenger
    {
        public static ConcurrentDictionary<string, int> GroupCallDic = new ConcurrentDictionary<string, int>();

        static Messenger()
        {
            // 大家都知道你很蠢啦

        }

        public static void IncreaseCallCounts(string group)
        {
            lock (GroupCallDic)
            {
                if (GroupCallDic.ContainsKey(group))
                {
                    GroupCallDic[group]++;
                }
                else
                {
                    GroupCallDic[group] = 1;
                }
            }

            Task.Delay(TimeSpan.FromSeconds(60)).ContinueWith(task =>
            {
                lock (GroupCallDic)
                {
                    GroupCallDic[group]--;
                }
            });
        }

        public static void SendDebugInfo(string content)
        {
            if (Config.Instance.QQ.IsNumber())
                SendPrivate(Config.Instance.QQ, content);
            Trace.WriteLine($"{content}", "Message");
        }

        public static void SendPrivate(UserID humanQQ, string content)
        {
            ConnectorManager.Connector.SendPrivateMessage(humanQQ, content);
            // todo
        }

        static readonly Dictionary<GroupID, string> previousMessageDic = new Dictionary<GroupID, string>();
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SendGroup(GroupID g, string content)
        {
            var qq = g.ID;
            // 避免重复发送同一条消息
            if (previousMessageDic.ContainsKey(qq) && content == previousMessageDic[qq]) return;
            previousMessageDic[qq] = content;

            ConnectorManager.Connector.SendGroupMessage(g, content);

            IncreaseCallCounts(g);
            //Thread.Sleep(1000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
        }

        public static void Broadcast(string content)
        {
            Task.Factory.StartNew(() =>
            {
                var count = 0;
                foreach (var group in Config.Instance.WFGroupList)
                {
                    if (count > 20 && content.StartsWith("开始自动更新")) return;

                    var sb = new StringBuilder();
                    sb.AppendLine(content);
                    if (count > 10) sb.AppendLine($"发送次序: {count}(与真实延迟了{7 * count}秒)");
                    sb.AppendLine($"如果想要获取更好的体验,请自行部署.");
                    sb.ToString().Trim().SendToGroup(group);
                    count++;
                    Thread.Sleep(7000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
                }
            }, TaskCreationOptions.LongRunning);
        }

        public static async Task<string> SendBotStatus()
        {
            var sb = new StringBuilder();
            var q1 = Task.Run(async () => await WebHelper.TryGet("https://warframestat.us"));
            var q2 = Task.Run(async () => await WebHelper.TryGet("https://api.warframe.market/v1/items/valkyr_prime_set/orders?include=item"));
            var commitTask = Task.Run(() =>
                CommitsGetter.Get("https://api.github.com/repos/TRKS-Team/WFBot/commits?per_page=5"));
            // var q3 = Task.Run(() => WebHelper.TryGet("https://api.richasy.cn/wfa/rm/riven"));
            // var q4 = Task.Run(() => WebHelper.TryGet("https://10o.io/kuvalog.json"));
            Task.WaitAll(q1, q2/*, q3, q4*/);

            var apistat = await q1;
            var wmstat = await q2;
            // var wfastat = await q3;
            // var kuvastat = await q4;
            if (apistat.IsOnline && wmstat.IsOnline /*&& wfastat.IsOnline && kuvastat.IsOnline*/)
            {
                sb.AppendLine("磨弓状态: 正常");
            }
            else
            {
                sb.AppendLine("磨弓状态: 坏掉了");
            }

            sb.AppendLine($"WFBot 版本: {VersionString}");

            sb.AppendLine($"    任务API:  {(apistat.IsOnline ? $"{apistat.Latency}ms [在线]" : "[离线]")}");
            sb.AppendLine($"    WarframeMarket: {(wmstat.IsOnline ? $"{wmstat.Latency}ms [在线]" : "[离线]")}]");
            // sb.AppendLine($"    WFA紫卡市场: {wfastat.Latency}ms [{(wfastat.IsOnline ? "在线" : "离线")}]");
            // sb.AppendLine($"    赤毒/仲裁API: {kuvastat.Latency}ms [{(kuvastat.IsOnline ? "在线" : "离线")}]");
            var commit = (await commitTask)?.Format() ?? "GitHub Commit 获取异常, 可能是请求次数过多";
            sb.AppendLine(commit);
            return sb.ToString().Trim().AddPlatformInfo();
        }

        private static string VersionString => WFBotCore.IsOfficial ? $"WFBot 官方 {(WFBotCore.Version)}" : $"WFBot 非官方 {(WFBotCore.Version)}";

        public static void SendHelpdoc()
        {
            var group = AsyncContext.GetMessageSender();

            // 为了社区的良性发展, 请不要随意修改.
            group.SendMessage($@"欢迎查看磨弓使用说明！
源项目地址: https://github.com/TRKS-Team/WFBot
");
            group.SendMessage(@"原作者: TheRealKamisama
参数说明: <>为必填参数, []为选填参数, {}为附加选填参数, ()为补充说明
    /突击 | 今日突击
    /警报 | 所有警报
    /仲裁 | 仲裁任务
    /入侵 | 所有入侵
    /裂隙 | 查询裂隙
    /活动 | 活动和事件，比如热美亚
    /电波 | 每日每周即将过期的挑战
    /平原 | 地球&金星&火卫二平原的时间循环
    /赤毒任务 | 赤毒虹吸器和红潮
    /s船状态 | 当前的Sentient异常事件
    /小小黑 小小黑的信息
    /奸商 | 虚空商人的状态
    /地球赏金 [1-5]| 地球平原的全部/单一赏金任务
    /金星赏金 [1-5]| 金星平原的全部/单一赏金任务
    /遗物 <关键词> | 查询遗物的内容
    /翻译 <关键词> |（eg. 致残突击 犀牛prime) 中 -> 英 / 英 -> 中 翻译
    /wm <物品名称> {-qr} {-b} | 查询 WarframeMarket, 查询未开紫卡请输入: 近战未开紫卡
    /紫卡 <武器名称> | 紫卡市场 数据来自 WFA 紫卡市场(暂时修复中)
");
            
        }

        /* 当麻理解不了下面的代码 */
        // 现在可以了
        public static void SendToGroup(this string content, GroupID qq)
        {
            SendGroup(qq, content);
        }

        public static void SendTo(this string content, GroupMessageSender sender)
        {
            sender.SendMessage(content);
        }

        public static void SendToPrivate(this string content, UserID qq)
        {
            SendPrivate(qq, content);
        }


        /*
        public static void SuperBroadcast(string content)
        {
            var groups = GetGroups().Select(g => g.Group);
            Task.Factory.StartNew(() =>
            {
                foreach (var group in groups)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(content);
                    SendGroup(group.ToGroupNumber(), sb.ToString().Trim());
                    Thread.Sleep(7000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
                }
            }, TaskCreationOptions.LongRunning);
        }
        */
    }

    public struct GroupID
    {
        public long ID { get; }

        public GroupID(long id)
        {
            ID = id;
        }

        public static implicit operator long(GroupID id)
        {
            return id.ID;
        }
        

        public static implicit operator string(GroupID id)
        {
            return id.ToString();
        }

        public static implicit operator GroupID(long id)
        {
            return new GroupID((uint) id);
        }

        public static implicit operator GroupID(string id)
        {
            return new GroupID(id.ToUInt());
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }

    public struct UserID
    {
        public long ID { get; }

        public UserID(long id)
        {
            ID = id;
        }

        public static implicit operator long(UserID id)
        {
            return id.ID;
        }
        
        public static implicit operator string(UserID id)
        {
            return id.ToString();
        }

        public static implicit operator UserID(long id)
        {
            return new UserID((uint)id);
        }

        public static implicit operator UserID(string id)
        {
            return new UserID(id.ToUInt());
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
