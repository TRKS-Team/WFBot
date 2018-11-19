using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Humanizer;
using Humanizer.Localisation;

namespace TRKS.WF.QQBot
{
    public static class WFFormatter
    {
        public static string ToString(WFAlert alert)
        {
            var mission = alert.Mission;
            var reward = mission.Reward;
            var time = (alert.Expiry - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Second, " ");

            return $"[{mission.Node}] 等级 {mission.MinEnemyLevel}-{mission.MaxEnemyLevel}\r\n" +
                   $"-类型:     {mission.Type}-{mission.Faction}\r\n" +
                   $"-奖励:     {ToString(reward)}\r\n" +
                   $"-过期时间: {alert.Expiry}({time} 后)";
        }
        public static string ToString(List<Fissure> fissures)
        {
            var sb = new StringBuilder();
            foreach (var fissure in fissures)
            {
                sb.AppendLine($"{fissure.tier}({fissure.tierNum}) {fissure.missionType}");
                sb.AppendLine($"{fissure.expiry}");
                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }
        public static string ToString(SyndicateMission mission, int index)
        {
            var sb = new StringBuilder();
            sb.Append($"集团:{mission.syndicate} 赏金任务 {index + 1} ");
            sb.AppendLine($"等级:{mission.jobs[index].enemyLevels[0]} - {mission.jobs[index].enemyLevels[1]}");
            sb.AppendLine("奖励:");
            foreach (var reward in mission.jobs[index].rewardPool)
            {
                sb.AppendLine($"    {reward}");
            }

            return sb.ToString().Trim();
        }
        public static string ToString(WFInvasion inv)
        {
            var sb = new StringBuilder();
            var completion = Math.Floor(inv.completion);

            sb.AppendLine($"地点: {inv.node}");

            sb.AppendLine($">进攻方: {inv.attackingFaction}");
            if (!inv.vsInfestation)
                sb.AppendLine($"奖励: {ToString(inv.attackerReward)}");
            sb.AppendLine($"进度: {completion}%");

            sb.AppendLine($">防守方: {inv.defendingFaction}");
            sb.AppendLine($"奖励: {ToString(inv.defenderReward)}");
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
            sb.AppendLine($">阵营: {sortie.faction}");
            sb.AppendLine($">首长: {sortie.boss}");
            sb.AppendLine("");
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
                foreach (var inventory in trader.inventory)
                {
                    sb.AppendLine($"         [{inventory.item}]");
                    sb.AppendLine($"         {inventory.ducats}金币 + {inventory.credits}现金");
                }
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

        public static string ToString(WMInfo info)
        {
            var sb = new StringBuilder();
            var itemItemsInSet = info.include.item.items_in_set;
            sb.AppendLine($"下面是物品: {itemItemsInSet.Where(item => item.zh.item_name != item.en.item_name).ToList().Last().zh.item_name} 按价格从小到大的{info.payload.orders.Length}条信息");
            sb.AppendLine();
            foreach (var order in info.payload.orders)
            {
                sb.AppendLine($"[{order.user.ingame_name}]   {order.user.status}");
                sb.AppendLine($"{order.order_type}  {order.platinum}白鸡");
            }
            // 以后不好看了再说
            return sb.ToString().Trim();
        }

        public static string ToString(Defenderreward reward)
        {
            var rewards = new List<string>();
            if (reward.credits > 0)
            {
                rewards.Add($"{reward.credits} cr");
            }

            foreach (var item in reward.countedItems)
            {
                rewards.Add($"{item.count}x{item.type}");
            }

            return string.Join(" + ", rewards);
        }
        public static string ToString(Attackerreward reward)
        {
            var rewards = new List<string>();
            if (reward.credits > 0)
            {
                rewards.Add($"{reward.credits} cr");
            }

            foreach (var item in reward.countedItems)
            {
                rewards.Add($"{item.count}x{item.type}");
            }

            return string.Join(" + ", rewards);
        }
        public static string ToString(Reward reward)
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
        }
    }
}
