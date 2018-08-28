using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Newbe.Mahua;

namespace TRKS.WF.QQBot
{
    [Configuration("WFConfig")]
    class Config : Configuration<Config>
    {
        public List<string> WFGroupList = new List<string>();

        public List<string> InvationRewardList = new List<string>();

        public string Code;
    }

    public class Invasions
    {
        public Class1[] Property1 { get; set; }
    }

    public class Class1
    {
        public string id { get; set; }
        public string node { get; set; }
        public string desc { get; set; }
        public Attackerreward attackerReward { get; set; }
        public string attackingFaction { get; set; }
        public Defenderreward defenderReward { get; set; }
        public string defendingFaction { get; set; }
        public bool vsInfestation { get; set; }
        public DateTime activation { get; set; }
        public int count { get; set; }
        public int requiredRuns { get; set; }
        public float completion { get; set; }
        public bool completed { get; set; }
        public string eta { get; set; }
        public string[] rewardTypes { get; set; }
    }

    public class Attackerreward
    {
        public object[] items { get; set; }
        public Counteditem[] countedItems { get; set; }
        public int credits { get; set; }
        public string asString { get; set; }
        public string itemString { get; set; }
        public string thumbnail { get; set; }
        public int color { get; set; }
    }

    public class Counteditem
    {
        public int count { get; set; }
        public string type { get; set; }
    }

    public class Defenderreward
    {
        public object[] items { get; set; }
        public Counteditem1[] countedItems { get; set; }
        public int credits { get; set; }
        public string asString { get; set; }
        public string itemString { get; set; }
        public string thumbnail { get; set; }
        public int color { get; set; }
    }

    public class Counteditem1
    {
        public int count { get; set; }
        public string type { get; set; }
    }

    public class WFApi
    {
        public Dict[] Dict { get; set; }
        public Sale[] Sale { get; set; }
        public Alert[] Alert { get; set; }
        public Invasion[] Invasion { get; set; }
        public Riven[] Riven { get; set; }
        public Statuscode[] StatusCode { get; set; }
        public Relic[] Relic { get; set; }
    }

    public class Dict
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Zh { get; set; }
        public string En { get; set; }
    }

    public class Sale
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Search { get; set; }
        public string Zh { get; set; }
        public string En { get; set; }
    }

    public class Alert
    {
        public int Id { get; set; }
        public string Zh { get; set; }
        public string En { get; set; }
    }

    public class Invasion
    {
        public int Id { get; set; }
        public string Zh { get; set; }
        public string En { get; set; }
    }

    public class Riven
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Level { get; set; }
        public float Ratio { get; set; }
    }

    public class Statuscode
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Zh { get; set; }
        public string En { get; set; }
    }

    public class Relic
    {
        public int Id { get; set; }
        public string Tier { get; set; }
        public string RelicName { get; set; }
        public string Rewards { get; set; }
        public string Name { get; set; }
    }

    public class WFAlert
    {
        public WFAlerts[] Property1 { get; set; }
    }

    public class WFAlerts // 某个好朋友让我改成大写，好习惯
    {
        public string Id { get; set; }
        public DateTime Activation { get; set; }
        public DateTime Expiry { get; set; }
        public Mission Mission { get; set; }
        public bool Expired { get; set; }
        public string Eta { get; set; }
        public string[] RewardTypes { get; set; }
    }

    public class Mission
    {
        public string Description { get; set; }
        public string Node { get; set; }
        public string Type { get; set; }
        public string Faction { get; set; }
        public Reward Reward { get; set; }
        public int MinEnemyLevel { get; set; }
        public int MaxEnemyLevel { get; set; }
        public bool Nightmare { get; set; }
        public bool ArchwingRequired { get; set; }
        public int MaxWaveNum { get; set; }
    }

    public class Reward
    {
        public string[] Items { get; set; }
        public CountedItem[] CountedItems { get; set; }
        public int Credits { get; set; }
        public string AsString { get; set; }
        public string ItemString { get; set; }
        public string Thumbnail { get; set; }
        public int Color { get; set; }
    }

    public class CountedItem
    {
        public int Count { get; set; }
        public string Type { get; set; }
    }
    class WFNotificationHandler
    {
        public WFNotificationHandler()
        {
            InitWFAlert();
        }

        public static Dictionary<string, string> MissionsDic = new Dictionary<string, string>();
        public static HashSet<string> SendedAlertsSet = new HashSet<string>();
        private static bool inited;
        public static WFApi WfApi = GetWfApi();
        public static System.Timers.Timer Timer = new System.Timers.Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);

        public void SendInvasions(Invasions invasions)
        {

        }

        public void UpdateInvasions()
        {
            var wc = new WebClient();
            var inva = wc.DownloadString("https://api.warframestat.us/pc/invasions").JsonDeserialize<Invasions>();
            UpdateInvasionsDic(inva);
            SendInvasions(inva);
        }

        public void UpdateInvasionsDic(Invasions invasions)
        {
            
        }

        public void SendAllInvasions(string group)
        {

        }
        public void InitWFAlert()
        {
            if (inited) return;
            var alerts = new WebClient().DownloadString("https://api.warframestat.us/pc/alerts")
                .JsonDeserialize<WFAlerts[]>();
            foreach (var alert in alerts)
            {
                SendedAlertsSet.Add(alert.Id);
            }
            Timer.Elapsed += (sender, eventArgs) => UpdateAlerts();
            Timer.Start();
        }
        public static WFApi GetWfApi()
        {
            var wc = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            return wc.DownloadString("https://api.richasy.cn/api/lib/localdb/tables").JsonDeserialize<WFApi>();

        }
        public void UpdateAlertDic(WFAlerts[] wfAlerts, WFApi wfApi)
        {
            foreach (var alert in wfAlerts)
            {
                var itemString = "";
                if (alert.RewardTypes[0] == "endo")
                {
                    itemString += $"+{alert.Mission.Reward.ItemString.Replace("Endo", "内融核心")}";
                }// 我就这么写 不服的发pr 能用就好(
                foreach (var api in wfApi.Alert)
                {
                    foreach (var item in alert.Mission.Reward.Items)
                    {
                        if (item == api.En)
                        {
                            itemString += $"+{api.Zh}";
                        }
                    }

                    foreach (var countedItem in alert.Mission.Reward.CountedItems)
                    {
                        if (countedItem.Type == api.En)
                        {
                            itemString += $"+{alert.Mission.Reward.CountedItems[0].Count}个{api.Zh}";
                        }
                    }                   
                }

                foreach (var api in wfApi.Dict)
                {
                    if (alert.Mission.Type == api.En)alert.Mission.Type = api.Zh;                   

                    if (api.Type == "Star")alert.Mission.Node = alert.Mission.Node.Replace(api.En, api.Zh);
                }
                var sb = new StringBuilder();
                sb.AppendLine($"{alert.Mission.Node} 等级{alert.Mission.MinEnemyLevel}-{alert.Mission.MaxEnemyLevel}");
                sb.AppendLine($"{alert.Mission.Type}-{alert.Mission.Faction}");
                sb.AppendLine($"奖励:{alert.Mission.Reward.Credits}{itemString}");
                sb.AppendLine($"过期时间:{alert.Expiry + TimeSpan.FromHours(8)}");
                MissionsDic[alert.Id] = sb.ToString();
            }
        }

        public void UpdateAlerts()
        {
            try
            {
                var alerts = new WebClient().DownloadString("https://api.warframestat.us/pc/alerts")
                    .JsonDeserialize<WFAlerts[]>();
                foreach (var alert in alerts)
                {
                    if (!MissionsDic.ContainsKey(alert.Id))
                    {
                        UpdateAlertDic(alerts, WfApi);
                        SendWFAlert(alerts);
                        break;
                    }
                }
            }
            catch (WebException)
            {
                // 什么都不做
            }
            catch (Exception e)
            {
                using (var robotSession = MahuaRobotManager.Instance.CreateSession())
                {
                    var api = robotSession.MahuaApi;
                    api.SendPrivateMessage("1141946313", e.ToString());// 这是我自己的qq号.
                }

            }

        }

        public void SendAllAlerts(string group)
        {
            var alerts = new WebClient().DownloadString("https://api.warframestat.us/pc/alerts")
                .JsonDeserialize<WFAlerts[]>();
            UpdateAlerts();
            var result = "指挥官,下面是太阳系内所有的警报任务,供您挑选.";
            foreach (var alert in alerts)
            {
                result += Environment.NewLine +Environment.NewLine + MissionsDic[alert.Id];
            }

            // var path = Path.Combine("alert", Path.GetRandomFileName().Replace(".", "") + ".jpg"); // 我发现amanda会把这种带点的文件识别错误...
            // RenderAlert(result, path);
            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var api = robotSession.MahuaApi;
                // api.SendGroupMessage(group, $@"[QQ:pic={path.Replace(@"\\", @"\")}]");
                api.SendGroupMessage(group, result);
            }
        }

        public void SendWFAlert(WFAlerts[] alerts)
        {

            foreach (var alert in alerts)
            {
                if (alert.Mission.Reward.Items.Length != 0 || alert.Mission.Reward.CountedItems.Length != 0)
                {
                    if (!SendedAlertsSet.Contains(alert.Id))
                    {
                        var result =
                            $@"指挥官,Ordis拦截到了一条警报,您要开始另一项光荣的打砸抢任务了吗?{Environment.NewLine}{MissionsDic[alert.Id]}";
                        // var path = Path.Combine("alert", Path.GetRandomFileName().Replace(".", "") + ".jpg"); // 我发现amanda会把这种带点的文件识别错误...
                        // RenderAlert(result, path);
                        foreach (var group in Config.Instance.WFGroupList)
                        {
                            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
                            {
                                var api = robotSession.MahuaApi;
                                // api.SendGroupMessage(group, $@"[QQ:pic={path.Replace(@"\\", @"\")}]");// 图片渲染还是问题太多 文字好一点吧...
                                api.SendGroupMessage(group, result);
                            }
                        }

                        SendedAlertsSet.Add(alert.Id);
                    }
                }
            }
        }
        public void RenderAlert(string content, string path)
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
    }
}
