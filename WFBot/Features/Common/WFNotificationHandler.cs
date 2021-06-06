using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using GammaLibrary.Extensions;
using HtmlAgilityPack;
using WFBot.Events;
using WFBot.Features.Resource;
using WFBot.Features.Timers;
using WFBot.Features.Timers.Base;
using WFBot.Features.Utils;
using WFBot.Utils;
using Timer = System.Timers.Timer;
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace WFBot.Features.Other
{
    public class WFNotificationHandler
    {
        private static readonly object Locker = new object();
        private readonly HashSet<string> sendedAlertsSet = new HashSet<string>();
        private readonly HashSet<string> sendedInvSet = new HashSet<string>();
        private readonly HashSet<DateTime> sendedStalkerSet = new HashSet<DateTime>();
        private readonly HashSet<WarframeUpdate> sendedUpdateSet = new HashSet<WarframeUpdate>();
        // 如果你把它改到5分钟以上 sentientoutpost会出错
        private WFChineseAPI api => WFResources.WFChineseApi;
        private string platform => Config.Instance.Platform.ToString();
        private List<WFAlert> AlertPool = new List<WFAlert>();
        private List<WFInvasion> InvasionPool = new List<WFInvasion>();
        private List<PersistentEnemie> StalkerPool = new List<PersistentEnemie>();
        volatile bool WFNotificationLoaded = false;

        public WFNotificationHandler()
        {
#pragma warning disable 4014
            InitWFNotificationAsync();
#pragma warning restore 4014
        }

        private async Task InitWFNotificationAsync()
        {
            AsyncContext.SetCancellationToken(CancellationToken.None);
            var alerts = api.GetAlerts();
            var invs = api.GetInvasions();
            var enemies = api.GetPersistentEnemies();
            var updates = GetWarframeUpdates();

            foreach (var alert in await alerts)
                sendedAlertsSet.Add(alert.Id);
            foreach (var inv in await invs)
                sendedInvSet.Add(inv.id);
            foreach (var enemy in await enemies)
                sendedStalkerSet.Add(enemy.lastDiscoveredTime);
            foreach (var update in await updates)
                sendedUpdateSet.Add(update);
            WFNotificationLoaded = true;
            Trace.WriteLine("Warframe通知初始化完成！");
        }

        [CalledByTimer(typeof(NotificationTimer))]
        public void Update()
        {
            if (!WFNotificationLoaded) return;
            
            lock (Locker)
            {
                Task.WaitAll(
                    UpdateAlertPool(),
                    UpdateInvasionPool(),
                    UpdatePersistentEnemiePool(),
                    CheckWarframeUpdates()
                );
                
                // UpdateWFGroups(); 此处代码造成过一次数据丢失 暂时处理一下

                // CheckSentientOutpost();
                // 很不幸 S船已经被DE改的我不知道怎么写了
                // 无法与你继续互动
            }
        }

        public class WarframeUpdate
        {
            protected bool Equals(WarframeUpdate other)
            {
                return title == other.title && url == other.url;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((WarframeUpdate) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(title, url);
            }

            public string title { get; set; }
            public string url { get; set; }
        }
        public async Task<List<WarframeUpdate>> GetWarframeUpdates()
        {
            var result = new List<WarframeUpdate>();
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync("https://forums.warframe.com/forum/3-pc-update-notes/");
            foreach (var node in doc.DocumentNode.SelectNodes("/html/body/main/div/div/div/div[3]/div/ol/li/div/h4/span/a"))
            {
                result.Add(new WarframeUpdate {title = node.InnerText.Trim(), url = node.GetAttributeValue("href", "")});
            }

            return result;
        }
        public async Task CheckWarframeUpdates()
        {
            var updates = await GetWarframeUpdates();
            if (!sendedUpdateSet.Contains(updates.First()))
            {
                var msg = WFFormatter.ToString(updates.First());
                Messenger.Broadcast(msg);
                sendedUpdateSet.Add(updates.First());
            }
        }   
        // public async Task SendSentientOutpost()
        // {
        //     var sb = new StringBuilder();
        //     var outpost = await api.GetSentientOutpost();
        //     sb.AppendLine("侦测到在途的Sentient异常事件: ");
        //     sb.AppendLine(WFFormatter.ToString(outpost));
        //     Messenger.Broadcast(sb.ToString().Trim());
        // }
        
        // public async Task CheckSentientOutpost()
        // {
        //     var outpost = await api.GetSentientOutpost();
        //     // 这api妥妥的是个wip你信不信
        //     if (outpost.active && DateTime.Now - Config.Instance.SendSentientOutpostTime >= TimeSpan.FromMinutes(30))
        //     {
        //         await SendSentientOutpost();
        //         Config.Instance.SendSentientOutpostTime = DateTime.Now;
        //         Config.Save();
        //     }
        // }

        private void CheckPersistentEnemies()
        {
            var enemies = StalkerPool;
            if (!enemies.Any(enemy => enemy.isDiscovered && !sendedStalkerSet.Contains(enemy.lastDiscoveredTime))) return;

            var sb = new StringBuilder();
            sb.AppendLine("有小小黑出现了!");

            foreach (var enemy in enemies.Where(enemy => enemy.isDiscovered && !sendedStalkerSet.Contains(enemy.lastDiscoveredTime)))
            {
                sb.AppendLine(WFFormatter.ToString(enemy));
                sendedStalkerSet.Add(enemy.lastDiscoveredTime);
            }

            Messenger.Broadcast(sb.ToString().Trim());
        }

        /*
        private void UpdateWFGroups()
        {
            var groups = GetGroups().Select(group => group.Group).ToList();
            Config.Instance.WFGroupList.RemoveAll(group => !groups.Contains(group));
            Config.Save();
        }
        */

        private async Task UpdatePersistentEnemiePool()
        {
            StalkerPool = await api.GetPersistentEnemies();
            CheckPersistentEnemies();
        }

        private async Task UpdateAlertPool()
        {
            AlertPool = await api.GetAlerts();
            CheckAlerts();
        }

        private async Task UpdateInvasionPool()
        {
            InvasionPool = await api.GetInvasions();
            CheckInvasions();
        }

        private void CheckInvasions()
        {
            try
            {
                foreach (var inv in InvasionPool.Where(inv => !inv.completed && !sendedInvSet.Contains(inv.id)))
                {
                    // 不发已经完成的入侵 你学的好快啊
                    // 不发已经发过的入侵
                    var list = GetAllInvasionsCountedItems(inv).ToArray();

                    if (Config.Instance.InvationRewardList.Any(item => list.Contains(item)))
                    {
                        var notifyText = $"指挥官, 太阳系陷入了一片混乱。请查看你的星图！\r\n" +
                                         $"{WFFormatter.ToString(inv)}";

                        Messenger.Broadcast(notifyText.AddPlatformInfo());
                        sendedInvSet.Add(inv.id);
                    }
                }
            }
            catch (Exception e)
            {
                // 问题有点大 我慌一下
                Messenger.SendDebugInfo(e.ToString());
            }

        }

        private static IEnumerable<string> GetAllInvasionsCountedItems(WFInvasion inv)
        {
            foreach (var reward in inv.attackerReward.countedItems)
            {
                yield return reward.type;
            }

            foreach (var reward in inv.defenderReward.countedItems)
            {
                yield return reward.type;
            }
        }

        public string SendAllPersistentEnemies()
        {
            var enemies = StalkerPool;
            if (!enemies.Any())
            {
                return "目前没有小小黑出现";
            }

            var sb = new StringBuilder();
            sb.AppendLine("下面是全太阳系内的小小黑：");
            foreach (var enemy in enemies)
            {
                sb.AppendLine(WFFormatter.ToString(enemy));
            }
            return sb.ToString().Trim();
        }

        public async Task<string> SendAllInvasions()
        {
            try
            {
                await UpdateInvasionPool();
            }
            catch (OperationCanceledException)
            {
                return "操作超时";
            }
            var invasions = InvasionPool;
            var sb = new StringBuilder();
            sb.AppendLine("指挥官, 下面是太阳系内所有的入侵任务：");
            sb.AppendLine();

            foreach (var invasion in invasions.Where(invasion => !invasion.completed))
            {
                sb.AppendLine(WFFormatter.ToString(invasion));
                sb.AppendLine();
            }

            return sb.ToString().Trim().AddPlatformInfo();
        }

        private void CheckAlerts()
        {
            try
            {
                var alerts = AlertPool.Where(alert => !sendedAlertsSet.Contains(alert.Id));
                // 后人不要尝试重构下面这坨代码 她很好用 但是你别想着去重构
                // 操他妈 这玩意刚写好警报就没了
                var result =
                    Config.Instance.IsAlertRequiredRareItem
                        ? alerts.Where(a =>
                            (a.RewardTypes.Any(rewardtype =>
                                 rewardtype == "kavatGene" || rewardtype == "helmet" || rewardtype == "nitain" ||
                                 rewardtype == "other") || a.Mission.Reward.Items.Any()) &&
                            a.RewardTypes.Any(rewardtype => rewardtype != "endo"))
                        : alerts.Where(a => a.Mission.Reward.Items.Any() || a.Mission.Reward.CountedItems.Any());
                foreach (var alert in result)
                {
                    SendWFAlert(alert);
                }

            }
            catch (Exception e)
            {
                Messenger.SendDebugInfo(e.ToString());
            }
        }

        public async Task<string> SendAllAlerts()
        {
            try
            {
                await UpdateAlertPool();
            }
            catch (OperationCanceledException)
            {
                return "操作超时";
            }
            var alerts = AlertPool;
            var sb = new StringBuilder();

            sb.AppendLine("指挥官, 下面是太阳系内所有的警报任务：");
            foreach (var alert in alerts)
            {
                sb.AppendLine(WFFormatter.ToString(alert));
                sb.AppendLine();
            }

            return sb.ToString().Trim().AddPlatformInfo();
        }

        private void SendWFAlert(WFAlert alert)
        {
            var result = "指挥官, 刚刚拦截到了一条警报！\r\n" +
                         WFFormatter.ToString(alert).AddHelpInfo().AddPlatformInfo();
            Messenger.Broadcast(result);
            sendedAlertsSet.Add(alert.Id);
        }

        /* 以下是废弃的代码和注释
        // var path = Path.Combine("alert", Path.GetRandomFileName().Replace(".", "") + ".jpg"); // 我发现amanda会把这种带点的文件识别错误...
        // RenderAlert(result, path);
        // api.SendGroupMessage(group, $@"[QQ:pic={path.Replace(@"\\", @"\")}]");
        // api.SendGroupMessage(group, $@"[QQ:pic={path.Replace(@"\\", @"\")}]");// 图片渲染还是问题太多 文字好一点吧...
        // var path = Path.Combine("alert", Path.GetRandomFileName().Replace(".", "") + ".jpg"); // 我发现amanda会把这种带点的文件识别错误...
        // RenderAlert(result, path);

        public void RenderAlert(string content, string path)// 废弃了呀...
        {
            var strs = content.Split(Environment.NewLine.ToCharArray());
            var height = 60;
            var width = 60;
            var font = new Font("Microsoft YaHei", 16);// 雅黑还挺好看的
            var size = TextRenderer.MeasureText(strs[0], font);
            width += GetlongestWidth(strs, font);
            height += size.Height * strs.Length;
            height += 10 * (strs.Length - 1);
            var bitmap = new Bitmap(width, height);
            var graphics = Graphics.FromImage(bitmap);
            var p = new Point(30, 30);
            graphics.Clear(Color.Gray);
            foreach (var str in strs)
            {
                TextRenderer.DrawText(graphics, str, font, p, Color.Lavender);
                p.Y += TextRenderer.MeasureText(str, font).Height + 10;
            }

            bitmap.Save(path);
        }

        public int GetlongestWidth(string[] strs, Font font)
        {
            var width = new List<int>();
            foreach (var str in strs)
            {
                var size = TextRenderer.MeasureText(str, font);
                width.Add(size.Width);
            }

            return width.Max();
        }

        */
    }
}

