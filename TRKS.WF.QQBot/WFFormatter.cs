using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Humanizer;
using Humanizer.Localisation;

namespace TRKS.WF.QQBot
{
    public static class WFFormatter
    {
        public static string ToString(WarframeNET.Alert alert)
        {
            var mission = alert.Mission;
            var reward = mission.Reward;
            var time = (alert.EndTime - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Second, " ");

            return $"[{mission.Node}] 等级 {mission.EnemyMinLevel}-{mission.EnemyMaxLevel}\r\n" +
                   $"-类型:     {mission.Type}-{mission.Faction}\r\n" +
                   $"-奖励:     {ToString(reward)}\r\n" +
                   $"-过期时间: {alert.EndTime}({time} 后)";
        }

        public static string ToString(WarframeNET.Invasion inv)
        {
            var sb = new StringBuilder();
            var completion = Math.Floor(inv.Completion);

            sb.AppendLine($"地点: {inv.Node}");

            sb.AppendLine($">进攻方: {inv.AttackingFaction}");
            if (!inv.IsVsInfestation)
                sb.AppendLine($"奖励: {ToString(inv.AttackerReward)}");
            sb.AppendLine($"进度: {completion}%");

            sb.AppendLine($">防守方: {inv.DefendingFaction}");
            sb.AppendLine($"奖励: {ToString(inv.DefenderReward)}");
            sb.Append($"进度 {100 - completion}%");
            return sb.ToString();
        }

        public static string ToString(CetusCycle cycle)
        {
            var time = (cycle.Expiry - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"),
                TimeUnit.Hour, TimeUnit.Millisecond, " ");
            var status = cycle.IsDay ? "白天" : "夜晚";
            var nextTime = !cycle.IsDay ? "白天" : "夜晚";

            var sb = new StringBuilder();
            sb.AppendLine($"现在平原的时间是: {status}");
            sb.AppendLine($"将在 {cycle.Expiry} 变为 {nextTime}");
            sb.Append($"距离 {nextTime} 还有 {time}");
            return sb.ToString();
        }

        public static string ToString(Sortie sortie)
        {
            var sb = new StringBuilder();
            sb.AppendLine("指挥官,下面是今天的突击任务.");
            foreach (var variant in sortie.variants)
            {
                sb.AppendLine($"[{variant.node}]");
                sb.AppendLine($"-类型:{variant.missionType}");
                sb.AppendLine($"-状态:{variant.modifier}");
            }

            return sb.ToString().Trim();
        }

        public static string ToString(VoidTrader trader)
        {
            var sb = new StringBuilder();
            if (trader.active)
            {
                var time = (DateTime.Now - trader.expiry).Humanize(int.MaxValue,
                    CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Second, " ");
                sb.AppendLine($"虚空商人已抵达:{trader.location}");
                sb.AppendLine($"携带商品:");
                sb.AppendLine($"     (这里还没写完...)");
                sb.Append($"结束时间:{trader.expiry}({time} 后)");
            }
            else
            {
                var time = (DateTime.Now - trader.activation).Humanize(int.MaxValue,
                    CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Second, " ");
                sb.Append($"虚空商人将在{trader.activation}({time} 后)抵达{trader.location}");
            }

            return sb.ToString().Trim();
        }

        public static string ToString(WarframeNET.Reward reward)
        {
            var rewards = new List<string>();
            if (reward.Credits > 0)
            {
                rewards.Add($"{reward.Credits} cr");
            }

            foreach (var item in reward.Items)
            {
                rewards.Add(item);
            }

            foreach (var item in reward.CountedItems)
            {
                rewards.Add($"{item.Count}x{item.Type}");
            }

            return string.Join(" + ", rewards);
            // string.Join 就是把一堆字符串连接起来
        }
    }
}
