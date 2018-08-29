using Newbe.Mahua;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

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
        public DateTime GetRealTime()
        {
            return Expiry + TimeSpan.FromHours(8);
        }
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
            InitWFNotification();
        }

        public Dictionary<string, string> MissionsDic = new Dictionary<string, string>();
        public Dictionary<string, string> InvDic = new Dictionary<string, string>();
        public HashSet<string> SendedAlertsSet = new HashSet<string>();
        private readonly bool inited;
        public WFApi WfApi = GetWfApi();
        public Timer timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        
        public void SendInvasions(Invasions invasions)
        {
            foreach (var inv in invasions.Property1)
            {
                if (!inv.completed)
                {
                    var typeSet = new HashSet<string>();
                    foreach (var reward in inv.attackerReward.countedItems)
                    {
                        typeSet.Add(reward.type);
                    }

                    foreach (var reward in inv.defenderReward.countedItems)
                    {
                        typeSet.Add(reward.type);
                    }

                    foreach (var item in Config.Instance.InvationRewardList)
                    {
                        if (typeSet.Contains(item))
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine("指挥官,太阳系陷入了一片混乱,查看你的星图");
                            sb.Append(InvDic[inv.id]);
                            foreach (var group in Config.Instance.WFGroupList)
                            {
                                Messenger.SendGroup(group, sb.ToString());
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void UpdateInvasions()
        {
            var inv = WebHelper.DownloadJson<Invasions>("https://api.warframestat.us/pc/invasions");
            UpdateInvasionsDic(inv, WfApi);
            SendInvasions(inv);
        }

        public void UpdateInvasionsDic(Invasions invasions, WFApi wfApi)
        {
            foreach (var inv in invasions.Property1)
            {
                if (!inv.completed)
                {
                    var attackeritemstring = "";
                    var defenderitemstring = "";
                    var completion = Math.Ceiling(inv.completion);
                    foreach (var api in wfApi.Invasion)
                    {
                        foreach (var item in inv.attackerReward.countedItems)
                        {
                            if (item.type == api.En)
                            {
                                attackeritemstring += $"{item.count}个{api.Zh}";
                            }
                        }

                        foreach (var item in inv.defenderReward.countedItems)
                        {
                            if (item.type == api.En)
                            {
                                defenderitemstring += $"{item.count}个{api.Zh}";
                            }
                        }
                    }

                    foreach (var api in wfApi.Dict.Where(api => api.Type == "Star").Where(api => inv.node.Contains(api.En)))
                    {
                        inv.node = inv.node.Replace(api.En, api.Zh);
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine($"地点:{inv.node}");
                    sb.AppendLine($"进攻方:{inv.attackingFaction}");
                    if (!inv.vsInfestation)
                    {
                        sb.AppendLine($"奖励:{attackeritemstring}");
                    }

                    sb.AppendLine($"进度:{completion}%");
                    sb.AppendLine($"防守方:{inv.defendingFaction}");
                    sb.AppendLine($"奖励:{defenderitemstring}");
                    sb.Append($"进度{100 - completion}%");

                    InvDic[inv.id] = sb.ToString();
                }
            }
        }

        public void SendAllInvasions(string group)
        {

        }

        public void InitWFNotification()
        {
            if (inited) return;
            var alerts = GetWFAlerts();

            foreach (var alert in alerts)
            {
                SendedAlertsSet.Add(alert.Id);
            }
            timer.Elapsed += (sender, eventArgs) =>
            {
                UpdateAlerts();
                UpdateInvasions();
            };
            timer.Start();
        }

        public void UpdateAlertDic(WFAlerts[] wfAlerts)
        {
            var wfApi = WfApi;
            foreach (var alert in wfAlerts)
            {
                var itemString = "";
                var mission = alert.Mission;
                var reward = mission.Reward;

                if (alert.RewardTypes[0] == "endo")
                {
                    Concat($"+{reward.ItemString.Replace("Endo", "内融核心")}");
                } 
                // 我就这么写 不服的发pr 能用就好(

                // translate item
                foreach (var api in wfApi.Alert)
                {
                    foreach (var item in reward.Items)
                    {
                        if (item == api.En)
                        {
                            Concat($"{api.Zh}");
                        }
                    }

                    foreach (var countedItem in reward.CountedItems)
                    {
                        if (countedItem.Type == api.En)
                        {
                            Concat($"{reward.CountedItems[0].Count}个{api.Zh}");
                        }
                    }
                }

                // translate mission
                foreach (var api in wfApi.Dict)
                {
                    if (mission.Type == api.En) mission.Type = api.Zh;

                    if (api.Type == "Star") mission.Node = mission.Node.Replace(api.En, api.Zh);
                }

                // build string
                var sb = new StringBuilder();
                sb.AppendLine($"{mission.Node} 等级{mission.MinEnemyLevel}-{mission.MaxEnemyLevel}");
                sb.AppendLine($"{mission.Type}-{mission.Faction}");
                sb.AppendLine($"奖励:{reward.Credits}{itemString}");
                sb.Append($"过期时间:{alert.GetRealTime()}");

                MissionsDic[alert.Id] = sb.ToString();

                void Concat(string content)
                {
                    itemString += $"+{content}"; // 其实更推荐使用 StringBuilder, 这么用不爽你可以回滚(
                }
            }
        }

        private static WFApi GetWfApi()
        {
            return WebHelper.DownloadJson<WFApi>("https://api.richasy.cn/api/lib/localdb/tables");
        }

        private static WFAlerts[] GetWFAlerts()
        {
            return WebHelper.DownloadJson<WFAlerts[]>("https://api.warframestat.us/pc/alerts");
        }

        public void UpdateAlerts()
        {
            try
            {
                var alerts = GetWFAlerts();
                foreach (var alert in alerts)
                {
                    if (!MissionsDic.ContainsKey(alert.Id))
                    {
                        UpdateAlertDic(alerts);
                        SendWFAlert(alerts);
                        // break;
                        // 疑问: break 的意义是啥?
                    }
                }
            }
            catch (WebException)
            {
                // 什么都不做
            }
            catch (Exception e)
            {
                const string qq = "1141946313"; // 这是我自己的qq号.
                Messenger.SendPrivate(qq, e.ToString());
                //TODO 写配置文件
            }

        }

        public void SendAllAlerts(string group)
        {
            var alerts = GetWFAlerts();
            UpdateAlerts();

            var sb = new StringBuilder();
            sb.AppendLine("指挥官,下面是太阳系内所有的警报任务,供您挑选.");
            foreach (var alert in alerts)
            {
                sb.AppendLine(MissionsDic[alert.Id]);
                sb.AppendLine();
            }

            // var path = Path.Combine("alert", Path.GetRandomFileName().Replace(".", "") + ".jpg"); // 我发现amanda会把这种带点的文件识别错误...
            // RenderAlert(result, path);
            // api.SendGroupMessage(group, $@"[QQ:pic={path.Replace(@"\\", @"\")}]");
            Messenger.SendGroup(group, sb.ToString());

        }

        public void SendWFAlert(WFAlerts[] alerts)
        {
            foreach (var alert in alerts)
            {
                var reward = alert.Mission.Reward;
                if ((reward.Items.Length > 0 || reward.CountedItems.Length > 0)
                    && (!SendedAlertsSet.Contains(alert.Id)))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($@"指挥官,Ordis拦截到了一条警报,您要开始另一项光荣的打砸抢任务了吗?");
                    sb.Append(MissionsDic[alert.Id]);

                    var result = sb.ToString();
                    foreach (var group in Config.Instance.WFGroupList)
                    {
                        Messenger.SendGroup(group, result);
                    }

                    SendedAlertsSet.Add(alert.Id);
                }
            }

            // api.SendGroupMessage(group, $@"[QQ:pic={path.Replace(@"\\", @"\")}]");// 图片渲染还是问题太多 文字好一点吧...

            // var path = Path.Combine("alert", Path.GetRandomFileName().Replace(".", "") + ".jpg"); // 我发现amanda会把这种带点的文件识别错误...
            // RenderAlert(result, path);
        }
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
    }
}
