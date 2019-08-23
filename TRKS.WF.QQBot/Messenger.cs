using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newbe.Mahua;
using Settings;
using TRKS.WF.QQBot.MahuaEvents;
using Timer = System.Timers.Timer;

namespace TRKS.WF.QQBot
{
    public static class MessengerHandlers
    {
        public static Action<string> DebugAlternateHandler;
        public static Action<string> MessageAlternateHandler;
    }

    public static class Messenger
    {
        public static Dictionary<string, int> GroupCallDic = new Dictionary<string, int>();
        public static Timer PrivateMessageTimer = new Timer(1000);
        public static Dictionary<string, string> PrivateMessageDictionary = new Dictionary<string, string>();

        static Messenger()
        {
            // 大家都知道你很蠢啦

            PrivateMessageTimer.Elapsed += (s, e) =>
            {
                if (MessengerHandlers.DebugAlternateHandler != null) return;
                lock (typeof(Messenger))
                {
                    try
                    {
                        foreach (var pair in PrivateMessageDictionary)
                        {
                            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
                            {
                                var api = robotSession.MahuaApi;
                                api.SendPrivateMessage(pair.Key, pair.Value);
                            }
                        }
                        PrivateMessageDictionary.Clear();
                    }
                    catch (Exception)
                    {
                    }
                }

            };
            PrivateMessageTimer.Start();
        }

        public static void IncreaseCallCounts(string group)
        {
            if (GroupCallDic.ContainsKey(group))
            {
                GroupCallDic[group]++;
            }
            else
            {
                GroupCallDic[group] = 1;
            }
            Task.Delay(TimeSpan.FromSeconds(60)).ContinueWith(task => GroupCallDic[group]--);

        }

        public static void SendDebugInfo(string content)
        {
            if (MessengerHandlers.DebugAlternateHandler != null)
            {
                MessengerHandlers.DebugAlternateHandler(content);
                return;
            }

            if (Config.Instance.QQ.IsNumber())
                SendPrivate(Config.Instance.QQ.ToHumanQQNumber(), content);
            Trace.WriteLine($"Debug message: {content}", "Message");
        }

        public static void SendPrivate(HumanQQNumber humanQQ, string content)
        {
            var qq = humanQQ.QQ;
            lock (typeof(Messenger))
            {
                if (PrivateMessageDictionary.ContainsKey(qq))
                {
                    PrivateMessageDictionary[qq] += "\r\n" + content;
                }
                else
                {
                    PrivateMessageDictionary[qq] = content;
                }
            }
        }

        private static Dictionary<string, string> previousMessageDic = new Dictionary<string, string>();
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SendGroup(GroupNumber g, string content)
        {
            if (MessengerHandlers.MessageAlternateHandler != null)
            {
                MessengerHandlers.MessageAlternateHandler(content);
                return;
            }

            var qq = g.QQ;
            if (previousMessageDic.ContainsKey(qq) && content == previousMessageDic[qq]) return;

            previousMessageDic[qq] = content;

            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var api = robotSession.MahuaApi;
                api.SendGroupMessage(qq, content);
            }
            //Thread.Sleep(1000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
        }

        public static void Broadcast(string content)
        {
            Task.Factory.StartNew(() =>
            {
                var count = 0;
                foreach (var group in Config.Instance.WFGroupList)
                {
                    if (count > 20 && content.StartsWith("机器人开始了自动更新")) return;

                    var sb = new StringBuilder();
                    sb.AppendLine(content);
                    if (count > 10) sb.AppendLine($"发送次序: {count}(与真实延迟了{7 * count}秒)");
                    sb.AppendLine($"如果想要获取更好的体验,请自行部署.");
                    SendGroup(group.ToGroupNumber(), sb.ToString().Trim());
                    count++;
                    Thread.Sleep(7000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
                }
            }, TaskCreationOptions.LongRunning);
        }

        public static void SendBotStatus(GroupNumber group)
        {
            var sb = new StringBuilder();
            var q1 = Task.Run(() => WebHelper.TryGet("https://warframestat.us"));
            var q2 = Task.Run(() => WebHelper.TryGet("https://api.warframe.market/v1/items/valkyr_prime_set/orders?include=item"));
            var q3 = Task.Run(() => WebHelper.TryGet("https://api.richasy.cn/wfa/rm/riven"));
            var q4 = Task.Run(() => WebHelper.TryGet("https://10o.io/kuvalog.json"));
            Task.WaitAll(q1, q2, q3, q4);

            var apistat = q1.Result;
            var wmstat = q2.Result;
            var wfastat = q3.Result;
            var kuvastat = q4.Result;
            if (apistat.IsOnline && wmstat.IsOnline && wfastat.IsOnline && kuvastat.IsOnline)
            {
                sb.AppendLine("机器人状态: 一切正常");
            }
            else
            {
                sb.AppendLine("机器人状态: 不正常");
            }

            if (InitEvent1.onlineBuild)
            {
                sb.AppendLine($"插件版本: {InitEvent1.localVersion}");
            }
            else
            {
                sb.AppendLine($"插件版本: 非官方");
            }
            sb.AppendLine($"    任务API: {apistat.Latency}ms [{(apistat.IsOnline ? "在线" : "离线")}]");
            sb.AppendLine($"    WarframeMarket: {wmstat.Latency}ms [{(wmstat.IsOnline ? "在线" : "离线")}]");
            sb.AppendLine($"    WFA紫卡市场: {wfastat.Latency}ms [{(wfastat.IsOnline ? "在线" : "离线")}]");
            sb.AppendLine($"    赤毒/仲裁API: {kuvastat.Latency}ms [{(kuvastat.IsOnline ? "在线" : "离线")}]");
            var commit = CommitsGetter.Get("https://api.github.com/repos/TRKS-Team/WFBot/commits")?.Format() ?? "GitHub Commit 获取异常, 可能是请求次数过多, 如果你是机器人主人, 解决方案请查看 FAQ.";
            sb.AppendLine(commit);
            sb.ToString().Trim().AddPlatformInfo().SendToGroup(group);
        }

        private static string Format(this CommitData[] commits)
        {
            var sb = new StringBuilder();
            sb.AppendLine("以下是 GitHub 的最后 3 条 Commit");
            foreach (var commit in commits.Take(3))
            {
                sb.AppendLine(
                    $"  {commit.commit.committer.date} {commit.commit.committer.name}: [{Regex.Replace(commit.commit.message, @"\r\n?|\n", "")}]");
            }

            return sb.ToString().Trim();
        }
        public static void SendHelpdoc(GroupNumber group)
        {
            SendGroup(@group, @"欢迎查看机器人唯一指定帮助文档
宣传贴地址: https://warframe.love/thread-230.htm
在线最新文档: https://github.com/TRKS-Team/WFBot/blob/master/README.md 
项目地址: https://github.com/TRKS-Team/WFBot
赞助(乞讨)地址: https://afdian.net/@TheRealKamisama
您的赞助会用来维持公用机器人, 也能推动我继续维护本插件.
本机器人为公益项目, 持续维护中.
如果你见到有人使用本插件盈利, 请在上方项目地址反馈.
如果你想给你的群也整个机器人, 请在上方项目地址了解");
            if (File.Exists("data/image/帮助文档.png"))
            {
                SendGroup(@group, @"[CQ:image,file=\帮助文档.png]");
            }
            else
            {
                SendGroup(@group, @"作者: TheRealKamisama
如果群里没有自动通知 请务必检查是否启用了通知功能
    /wiki [关键词] | 搜索 wiki 上的页面
    /午夜电波 | 每日每周即将过期的挑战
    /机器人状态 | 机器人目前的运行状态
    /警报 | 当前的所有警报
    /入侵 | 当前的所有入侵
    /突击 | 当前的所有突击
    /活动 | 当前的所有活动
    /虚空商人 | 奸商的状态
    /平原 | 地球平原 现在的时间 和 金星平原 现在的温度
    /查询 [物品名称] | 查询 WarframeMarket, 查询未开紫卡请输入: 手枪未开紫卡
    /紫卡 [武器名称] | 紫卡市场 数据来自 WFA 紫卡市场
    /地球赏金 | 地球平原的全部/单一赏金任务
    /金星赏金 | 金星平原的全部/单一赏金任务
    /裂隙 | 查询全部裂隙
    /遗物 [关键词] | (eg. 后纪 s3 前纪 B3) 所有与关键词有关的遗物
    /翻译 [关键词] |（eg. 致残突击 犀牛prime) 中 -> 英 / 英 -> 中 翻译
    /小小黑 目前小小黑的信息
*私聊*管理命令:
    /添加群 ******* 群号 | 启用 [群号] 对应的群的通知功能
    /删除群 ******* 群号 | 禁用 [群号] 对应的群的通知功能
    不启用通知功能新的任务将不会通知到群内
");
            }
        }

        /* 当麻理解不了下面的代码 */
        // 现在可以了
        public static void SendToGroup(this string content, GroupNumber qq)
        {
            SendGroup(qq, content);
        }

        public static void SendToPrivate(this string content, HumanQQNumber qq)
        {
            SendPrivate(qq, content);
        }

        public static GroupNumber ToGroupNumber(this string qq)
        {
            return new GroupNumber(qq);
        }

        public static HumanQQNumber ToHumanQQNumber(this string qq)
        {
            return new HumanQQNumber(qq);
        }
        private static List<GroupInfo> GetGroups()
        {
            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var mahuaApi = robotSession.MahuaApi;
                var groups = mahuaApi.GetGroupsWithModel().Model.ToList();
                return groups;
            }
        }

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
    }

    public class GroupNumber
    {
        public string QQ { get; }

        public GroupNumber(string qq)
        {
            QQ = qq;
        }

        public override string ToString()
        {
            return QQ;
        }
    }

    public class HumanQQNumber
    {
        public string QQ { get; }

        public HumanQQNumber(string qq)
        {
            QQ = qq;
        }

        public override string ToString()
        {
            return QQ;
        }
    }
}
