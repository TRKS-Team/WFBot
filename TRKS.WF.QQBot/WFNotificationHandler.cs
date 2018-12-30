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
        private static readonly object InvasionLocker = new object();
        private static readonly object AlertLocker = new object();
        private static readonly object WFAlertLocker = new object();

        private readonly HashSet<string> sendedAlertsSet = new HashSet<string>();
        private readonly HashSet<string> sendedInvSet = new HashSet<string>();
        private bool _inited;
        public readonly Timer Timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        private readonly WFChineseAPI api = WFResource.WFChineseApi;

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

            foreach (var alert in alerts)
                sendedAlertsSet.Add(alert.Id);

            foreach (var inv in invs)
                sendedInvSet.Add(inv.id);

            Timer.Elapsed += (sender, eventArgs) =>
            {
                UpdateAlerts();
                UpdateInvasions();
                UpdateWFGroups();
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

        private void UpdateWFGroups()
        {
            var groups = GetGroups().Select(group => group.Group).ToList();
            foreach (var group in Config.Instance.WFGroupList.Where(group => !groups.Contains(group)))
            {
                Config.Instance.WFGroupList.Remove(group);
            }
            Config.Save();
        }

        private void UpdateInvasions()
        {
            lock (InvasionLocker)
            {
                try
                {
                    foreach (var inv in api.GetInvasions().Where(inv => !inv.completed && !sendedInvSet.Contains(inv.id)))
                    {   
                        // 不发已经完成的入侵 你学的好快啊
                        // 不发已经发过的入侵
                        var list = GetAllInvasionsCountedItems(inv);

                        if (Config.Instance.InvationRewardList.Any(item => list.Contains(item)))
                        {
                            var notifyText = $"指挥官, 太阳系陷入了一片混乱, 查看你的星图\r\n" +
                                             $"{WFFormatter.ToString(inv)}";

                            Messenger.Broadcast(notifyText);
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
        }

        private static List<string> GetAllInvasionsCountedItems(WFInvasion inv)
        {
            var list = new List<string>();
            foreach (var reward in inv.attackerReward.countedItems)
            {
                list.Add(reward.type);
            }

            foreach (var reward in inv.defenderReward.countedItems)
            {
                list.Add(reward.type);
            }

            return list;
        }

        public void SendAllInvasions(string group)
        {
            var invasions = api.GetInvasions();
            var sb = new StringBuilder();
            sb.AppendLine("指挥官, 下面是太阳系内所有的入侵任务.");

            foreach (var invasion in invasions.Where(invasion => !invasion.completed))
            {
                sb.AppendLine(WFFormatter.ToString(invasion));
                sb.AppendLine();
            }

            Messenger.SendGroup(group, sb.ToString().Trim());
        }

        private void UpdateAlerts()
        {
            lock (AlertLocker)
            {
                try
                {
                    var alerts = api.GetAlerts();
                    foreach (var alert in alerts.Where(alert => !sendedAlertsSet.Contains(alert.Id)))
                    {
                        SendWFAlert(alert);
                    }
                }
                catch (Exception e)
                {
                    Messenger.SendDebugInfo(e.ToString());
                }
            }
        }

        public void SendAllAlerts(string group)
        {
            var alerts = api.GetAlerts();
            var sb = new StringBuilder();

            sb.AppendLine("指挥官, 下面是太阳系内所有的警报任务, 供您挑选.");
            foreach (var alert in alerts)
            {
                sb.AppendLine(WFFormatter.ToString(alert));
                sb.AppendLine();
            }

            Messenger.SendGroup(group, sb.ToString().Trim());
        }

        private void SendWFAlert(WFAlert alert)
        {
            lock (WFAlertLocker)
            {
                var reward = alert.Mission.Reward;
                if (reward.Items.Any() || reward.CountedItems.Any())
                {
                    var result = "指挥官, Ordis拦截到了一条警报, 您要开始另一项光荣的打砸抢任务了吗?\r\n" +
                                 WFFormatter.ToString(alert) +
                                 "\r\n可使用: /help来查看机器人的更多说明.";
                    Messenger.Broadcast(result);
                    sendedAlertsSet.Add(alert.Id);
                }
            }
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

