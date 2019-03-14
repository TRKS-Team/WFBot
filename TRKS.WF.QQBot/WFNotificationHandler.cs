using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newbe.Mahua;
using Timer = System.Timers.Timer;
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace TRKS.WF.QQBot
{
    public class WFNotificationHandler
    {
        private static readonly object Locker = new object();
        private readonly HashSet<string> sendedAlertsSet = new HashSet<string>();
        private readonly HashSet<string> sendedInvSet = new HashSet<string>();
        private readonly HashSet<DateTime> sendedStalkerSet = new HashSet<DateTime>();
        private bool _inited;
        public readonly Timer Timer = new Timer(TimeSpan.FromMinutes(3).TotalMilliseconds);
        private WFChineseAPI api => WFResource.WFChineseApi;
        private string platform => Config.Instance.Platform.ToString();
        private List<WFAlert> AlertPool = new List<WFAlert>();
        private List<WFInvasion> InvasionPool = new List<WFInvasion>();
        private List<PersistentEnemie> StalkerPool = new List<PersistentEnemie>();

        public WFNotificationHandler()
        {
            InitWFNotification();
        }

        private void InitWFNotification()
        {
            if (_inited) return;
            _inited = true;

            var alerts = api.GetAlerts();
            var invs = api.GetInvasions();
            var enemies = api.GetPersistentEnemies();

            foreach (var alert in alerts)
                sendedAlertsSet.Add(alert.Id);
            foreach (var inv in invs)
                sendedInvSet.Add(inv.id);
            foreach (var enemy in enemies)
                sendedStalkerSet.Add(enemy.lastDiscoveredTime);

            Timer.Elapsed += (sender, eventArgs) =>
            {
                if (HotUpdateInfo.PreviousVersion) return;

                lock (Locker)
                {
                    UpdateAlertPool();
                    UpdateInvasionPool();
                    // UpdateWFGroups(); 此处代码造成过一次数据丢失 暂时处理一下
                    UpdatePersistentEnemiePool();
                }
            };
            Timer.Start();
        }

        private List<GroupInfo> GetGroups()
        {
            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var mahuaApi = robotSession.MahuaApi;
                var groups = mahuaApi.GetGroupsWithModel().Model.ToList();
                return groups;
            }
        }

        private void CheckPersistentEnemies()
        {
            var enemies = StalkerPool;
            if (!enemies.Any(enemy => enemy.isDiscovered && !sendedStalkerSet.Contains(enemy.lastDiscoveredTime))) return;

            var sb = new StringBuilder();
            sb.AppendLine("我看到有小小黑冒头了? 干!");

            foreach (var enemy in enemies.Where(enemy => enemy.isDiscovered && !sendedStalkerSet.Contains(enemy.lastDiscoveredTime)))
            {
                sb.AppendLine(WFFormatter.ToString(enemy));
                sendedStalkerSet.Add(enemy.lastDiscoveredTime);
            }

            Messenger.Broadcast(sb.ToString().Trim());
        }

        private void UpdateWFGroups()
        {
            var groups = GetGroups().Select(group => group.Group).ToList();
            Config.Instance.WFGroupList.RemoveAll(group => !groups.Contains(group));
            Config.Save();
        }

        private void UpdatePersistentEnemiePool()
        {
            StalkerPool = api.GetPersistentEnemies();
            CheckPersistentEnemies();
        }
        private void UpdateAlertPool()
        {
            AlertPool = api.GetAlerts();
            CheckAlerts();
        }

        private void UpdateInvasionPool()
        {
            InvasionPool = api.GetInvasions();
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
                        var notifyText = $"指挥官, 太阳系陷入了一片混乱, 查看你的星图\r\n" +
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

        public void SendAllPersistentEnemies(string group)
        {
            var enemies = StalkerPool;
            var sb = new StringBuilder();
            sb.AppendLine("下面是全太阳系内的小小黑, 快去锤爆?");
            foreach (var enemy in enemies)
            {
                sb.AppendLine(WFFormatter.ToString(enemy));
            }
            Messenger.SendGroup(group, sb.ToString().Trim());
        }
        public void SendAllInvasions(string group)
        {
            UpdateInvasionPool();
            var invasions = InvasionPool;
            var sb = new StringBuilder();
            sb.AppendLine("指挥官, 下面是太阳系内所有的入侵任务.");

            foreach (var invasion in invasions.Where(invasion => !invasion.completed))
            {
                sb.AppendLine(WFFormatter.ToString(invasion));
                sb.AppendLine();
            }

            Messenger.SendGroup(group, sb.ToString().Trim().AddPlatformInfo());
        }

        private void CheckAlerts()
        {
            try
            {
                var alerts = AlertPool.Where(alert => !sendedAlertsSet.Contains(alert.Id));
                // 后人不要尝试重构下面这坨代码 她很好用 但是你别想着去重构
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

        public void SendAllAlerts(string group)
        {
            UpdateAlertPool();
            var alerts = AlertPool;
            var sb = new StringBuilder();

            sb.AppendLine("指挥官, 下面是太阳系内所有的警报任务, 供您挑选.");
            foreach (var alert in alerts)
            {
                sb.AppendLine(WFFormatter.ToString(alert));
                sb.AppendLine();
            }

            Messenger.SendGroup(group, sb.ToString().Trim().AddPlatformInfo());
        }

        private void SendWFAlert(WFAlert alert)
        {
            var result = "指挥官, Ordis拦截到了一条警报, 您要开始另一项光荣的打砸抢任务了吗?\r\n" +
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

