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

            return $"[{mission.Node}] 等级 {mission.EnemyMinLevel}-{mission.EnemyMaxLevel}\r\n" +
                   $"-类型:     {mission.Type}-{mission.Faction}\r\n" +
                   $"-奖励:     {ToString(reward)}\r\n" +
                   $"-过期时间: {alert.EndTime}";
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
            sb.AppendLine($"距离 {nextTime} 还有 {time}");
            sb.Append($"在 {cycle}");
            return sb.ToString();
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
