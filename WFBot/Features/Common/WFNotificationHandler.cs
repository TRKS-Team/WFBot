using System.Diagnostics;
using System.Text;
using HtmlAgilityPack;
using Manganese.Array;
using WFBot.Features.ImageRendering;
using WFBot.Features.Resource;
using WFBot.Features.Timers;
using WFBot.Features.Timers.Base;
using WFBot.Features.Utils;
using WFBot.Orichalt;
using WFBot.Utils;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace WFBot.Features.Common
{
    public class WFNotificationHandler
    {
        private static readonly object Locker = new object();
        private readonly HashSet<string> sendedAlertsSet = new HashSet<string>();
        private readonly HashSet<string> sendedInvSet = new HashSet<string>();
        private readonly HashSet<string> sendedFissureSet = new HashSet<string>();
        private readonly HashSet<DateTime> sendedStalkerSet = new HashSet<DateTime>();
        private readonly HashSet<WarframeUpdate> sendedUpdateSet = new HashSet<WarframeUpdate>();
        // 如果你把它改到5分钟以上 sentientoutpost会出错
        private WFChineseAPI api => WFResources.WFChineseApi;
        public List<WFAlert> AlertPool = new List<WFAlert>();
        public List<WFInvasion> InvasionPool = new List<WFInvasion>();
        public List<PersistentEnemie> StalkerPool = new List<PersistentEnemie>();
        volatile bool WFNotificationLoaded = false;

        public WFNotificationHandler()
        {
#pragma warning disable 4014
            InitWFNotificationAsync();
#pragma warning restore 4014
        }

        public void TestNotification()
        {
            sendedFissureSet.Clear();
            CheckHardVoidFissures(true).Wait();
        }
        private async Task InitWFNotificationAsync()
        {
            try
            {
                AsyncContext.SetCancellationToken(CancellationToken.None);
               /* var alerts = api.GetAlerts();
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
                    sendedUpdateSet.Add(update);*/
               var fissures = await api.GetFissures();
               foreach (var fissure in fissures) sendedFissureSet.Add(fissure.id);
               WFNotificationLoaded = true;
                Trace.WriteLine("WF 通知初始化完成.");
            }
            catch (Exception e)
            {
                Trace.WriteLine($"WF 通知初始化出错: {e}");
            }
        }

        [CalledByTimer(typeof(NotificationTimer))]
        public void Update()
        {
            if (!WFNotificationLoaded) return;
            
            lock (Locker)
            {
                Task.WaitAll(
                    /*UpdateAlertPool(),
                    UpdateInvasionPool(),
                    UpdatePersistentEnemiePool(),
                    CheckWarframeUpdates()*/
                    CheckHardVoidFissures()
                );
                
                // UpdateWFGroups(); 此处代码造成过一次数据丢失 暂时处理一下

                // CheckSentientOutpost();
                // 很不幸 S船已经被DE改的我不知道怎么写了
                // 无法与你继续互动
            }
        }

        public async Task<List<WarframeUpdate>> GetWarframeUpdates()
        {
            var result = new List<WarframeUpdate>();
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync("https://forums.warframe.com/forum/3-pc-update-notes/");
            var nodes = doc.DocumentNode.SelectNodes("/html/body/main/div/div/div/div[3]/div/ol/li/div/h4/span/a");
            foreach (var node in nodes)
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
                if (Config.Instance.EnableImageRendering)
                {
                    AsyncContext.SetCommandIdentifier("WFBot通知");
                    MiguelNetwork.Broadcast(new RichMessages()
                    {

                        new ImageMessage(){Content = ImageRenderHelper.SimpleImageRendering(msg) }
                    });
                }
                else
                {
                    MiguelNetwork.Broadcast(msg);
                }
                sendedUpdateSet.Add(updates.First());
            }
        }

        public async Task CheckHardVoidFissures(bool test = false)
        {
            var fissures = await api.GetFissures();
            var matches = fissures.Where(f => !sendedFissureSet.Contains(f.id)).Where(f => f.isHard && f.active)
                .Where(f => f.node.Contains("Mot") || f.node.Contains("Ani")).ToList();
            if (matches.Any())
            {
                AsyncContext.SetCommandIdentifier("WFBot通知");
                MiguelNetwork.Broadcast(new RichMessages
                {
                    new AtMessage {IsAll = true},
                    new TextMessage {Content = "(国际服)有新的钢铁虚空生存裂隙出现了!"},
                    new ImageMessage {Content = ImageRenderHelper.Fissures(matches.ToList(), 0)}
                });
                matches.ForEach(m => sendedFissureSet.Add(m.id));
            }
        }
        // public async Task SendSentientOutpost()
        // {
        //     var sb = new StringBuilder();
        //     var outpost = await api.GetSentientOutpost();
        //     sb.AppendLine("侦测到在途的Sentient异常事件: ");
        //     sb.AppendLine(WFFormatter.ToString(outpost));
        //     MiguelNetwork.Broadcast(sb.ToString().Trim());
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
            sb.AppendLine("我看到有小小黑冒头了? 干!");

            foreach (var enemy in enemies.Where(enemy => enemy.isDiscovered && !sendedStalkerSet.Contains(enemy.lastDiscoveredTime)))
            {
                sb.AppendLine(WFFormatter.ToString(enemy));
                sendedStalkerSet.Add(enemy.lastDiscoveredTime);
            }

            var result = sb.ToString().Trim();
            if (Config.Instance.EnableImageRendering)
            {
            AsyncContext.SetCommandIdentifier("WFBot通知");
                MiguelNetwork.Broadcast(new RichMessages()
                {

                    new ImageMessage(){Content = ImageRenderHelper.SimpleImageRendering(result) }
                });
            }
            else
            {
                MiguelNetwork.Broadcast(result);

            }
        }

        /*
        private void UpdateWFGroups()
        {
            var groups = GetGroups().Select(group => group.Group).ToList();
            Config.Instance.WFGroupList.RemoveAll(group => !groups.Contains(group));
            Config.Save();
        }
        */

        public async Task UpdatePersistentEnemiePool()
        {
            StalkerPool = await api.GetPersistentEnemies();
            CheckPersistentEnemies();
        }

        public async Task UpdateAlertPool()
        {
            AlertPool = await api.GetAlerts();
            CheckAlerts();
        }
        
        public async Task UpdateInvasionPool()
        {
            InvasionPool = await api.GetInvasions();
            CheckInvasions();
        }

        private void CheckInvasions()
        {
            try
            {
                if (Config.Instance.EnableImageRendering)
                {
                    var invs = new List<WFInvasion>();
                    foreach (var inv in InvasionPool.Where(inv => !inv.completed && !sendedInvSet.Contains(inv.id)))
                    {
                        // 不发已经完成的入侵 你学的好快啊
                        // 不发已经发过的入侵
                        var list = GetAllInvasionsCountedItems(inv).ToArray();

                        if (Config.Instance.InvationRewardList.Any(item => list.Contains(item)))
                        {
                            invs.Add(inv);
                            sendedInvSet.Add(inv.id);
                        }
                    }
                    AsyncContext.SetCommandIdentifier("WFBot通知");
                    if (invs.Any())
                    {
                        MiguelNetwork.Broadcast(new RichMessages() { new ImageMessage() { Content = ImageRenderHelper.Invasion(invs) } });
                    }

                }
                else
                {
                    foreach (var inv in InvasionPool.Where(inv => !inv.completed && !sendedInvSet.Contains(inv.id)))
                    {
                        // 不发已经完成的入侵 你学的好快啊
                        // 不发已经发过的入侵
                        var list = GetAllInvasionsCountedItems(inv).ToArray();

                        if (Config.Instance.InvationRewardList.Any(item => list.Contains(item)))
                        {
                            var notifyText = $"" +
                                             $"{WFFormatter.ToString(inv, true)}";

                            MiguelNetwork.Broadcast(notifyText.AddPlatformInfo());
                            sendedInvSet.Add(inv.id);
                        }
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
        
        private void SendWFAlert(WFAlert alert)
        {
            var result = "" +
                         WFFormatter.ToString(alert).AddHelpInfo().AddPlatformInfo();
            if (Config.Instance.EnableImageRendering)
            {
             AsyncContext.SetCommandIdentifier("WFBot通知");
                MiguelNetwork.Broadcast(new RichMessages()
                {
                    
                    new ImageMessage(){Content = ImageRenderHelper.SimpleImageRendering(result) }
                });
            }
            else
            {
                MiguelNetwork.Broadcast(result);

            }
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

