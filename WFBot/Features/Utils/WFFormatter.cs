using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;
using Humanizer.Localisation;
using WarframeAlertingPrime.SDK.Models.Enums;
using WarframeAlertingPrime.SDK.Models.User;
using WFBot.Features.Common;
using WFBot.Windows;

namespace WFBot.Features.Utils
{
    public static class WFFormatter
    {
        private static WFTranslator translator => WFResource.WFTranslator;

        public static string Format(this CommitData[] commits)
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

        public static string ToString(SentientOutpost outpost)
        {
            var sb = new StringBuilder();
            if (outpost.active)
            {
                // TODO 交给大哥维护
                // 回↑ 这个功能好像可以删了
                sb.Append("此功能维护中");
                //var expiry =  outpost.expiry > DateTime.Now + TimeSpan.FromHours(1) ? outpost.previous.expiry : outpost.expiry; 
                // 这行大概是因为api是wip 以后可以直接删掉换成outpost.expiry
                // 但愿吧
                //var time = (expiry - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Minute, " ");
                //sb.AppendLine($"    [{outpost.mission.node}]-{outpost.mission.faction} {time}后过期");
            }
            else
            {
                var time = (outpost.activation - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Minute, " ");
                sb.AppendLine("目前没有Sentient异常");
                sb.AppendLine($"下一个异常在{outpost.activation}({time}后)");
            }
            return sb.ToString().Trim();
        }
        // 他妈了个蛋 我琢磨了好久如何将两个api同步 结果第二个api就直接整合了第一个api的数据?
        // 干你娘
        /*
        public static string ToString(SentientAnomaly anomaly)
        {
            var sb = new StringBuilder();
            var time = (anomaly.projection - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Minute, " ");
            sb.AppendLine($"下一个异常在{anomaly.projection}({time}后)");

            return sb.ToString().Trim();
        }*/
        [Pure]
        public static string ToString(Kuva kuva)
        {
            var sb = new StringBuilder();
            var time = (kuva.expiry - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Minute, " ");
            sb.AppendLine($"[{kuva.node}] {time} 后过期");
            sb.AppendLine($"-类型:    {kuva.type}-{kuva.enemy}");
            return sb.ToString().Trim();
        }
        public static string ToString(Arbitration ar)
        {
            var sb = new StringBuilder();
            var time = (ar.expiry - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Minute, " ");
            sb.AppendLine($"[{ar.node}] {time} 后过期");
            sb.AppendLine($"-类型:    {ar.type}-{ar.enemy}");
            return sb.ToString().Trim();
        }
        [Pure]
        public static string ToString(WFNightWave nightwave)
        {
            var sb = new StringBuilder();
            sb.AppendLine("以下是午夜电波挑战: ");
            sb.AppendLine();
            var onedayleft =
                nightwave.activeChallenges.Where(challenge => challenge.expiry - DateTime.Now < TimeSpan.FromDays(1));
            var elsechallenges = nightwave.activeChallenges.ToList();

            elsechallenges.RemoveAll(challenge => challenge.expiry - DateTime.Now < TimeSpan.FromDays(1));
            var challenges = elsechallenges;
            if (onedayleft.Any())
            {
                sb.AppendLine("一天内将会过期: ");
                sb.AppendLine("    " + ToString(onedayleft.ToList(), true));
            }

            challenges = elsechallenges.Where(challenge => challenge.isDaily).ToList();
            if (challenges.Any())
            {
                sb.AppendLine($"每日挑战({challenges.First().reputation}): ");
                sb.AppendLine("    " + ToString(challenges, false));
            }

            challenges = elsechallenges.Where(challenge => !challenge.isDaily && !challenge.isElite).ToList();
            if (challenges.Any())
            {
                sb.AppendLine($"每周挑战({challenges.First().reputation}): ");
                sb.AppendLine("    " + ToString(challenges, false));
            }

            challenges = elsechallenges.Where(challenge => challenge.isElite).ToList();
            if (challenges.Any())
            {
                sb.AppendLine($"精英每周挑战({challenges.First().reputation}): ");
                sb.AppendLine("    " + ToString(challenges, false));
            }
            // 不要尝试去读这个
            // 你会发现我真是个傻逼    
            // 其实 也有点大智若愚的感觉
            return sb.ToString().Trim();
        }

        public static string ToString(List<Activechallenge> challenges, bool withreputation)
        {
            var sb = new StringBuilder();
            foreach (var challenge in challenges.OrderBy(c => c.desc))
            {
                sb.AppendLine(withreputation ? $"    [{challenge.desc}]({challenge.reputation}) " : $"    [{challenge.desc}] ");
            }

            return sb.ToString().Trim();
        }
        [Pure]
        public static string ToString(List<Event> events)
        {
            var sb = new StringBuilder();
            sb.AppendLine("以下是游戏内所有的活动:");

            foreach (var @event in events)
            {
                var time = (@event.expiry - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Minute, " ");
                sb.AppendLine($"[{@event.description}]");
                sb.AppendLine($"- 剩余点数: {@event.health}");
                sb.AppendLine($"- 结束时间: {time} 后");
                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }
        [Pure]
        public static string ToString(PersistentEnemie enemy)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{enemy.agentType}]");
            if (enemy.isDiscovered)
            {
                sb.AppendLine($"- 出现在: {enemy.lastDiscoveredAt}");
            }
            sb.AppendLine($"- 剩余点数: {enemy.healthPercent:P}");
            return sb.ToString().Trim();

        }
        [Pure]
        public static string ToString(WFAlert alert)
        {
            var mission = alert.Mission;
            var reward = mission.Reward;
            var time = (alert.Expiry - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Minute, " ");

            return $"[{mission.Node}] 等级{mission.MinEnemyLevel}~{mission.MaxEnemyLevel}:\r\n" +
                   $"- 类型:     {mission.Type} - {mission.Faction}\r\n" +
                   $"- 奖励:     {ToString(reward)}\r\n" +
                   //$"-过期时间: {alert.Expiry}({time} 后)" +
                   $"- 过期时间: {time} 后";
        }
        [Pure]
        public static string ToString(List<Relic> relics)
        {
            var sb = new StringBuilder();
            foreach (var relic in relics)
            {
                var rewards = relic.Rewards.Split(' ').Select(reward => $"[{reward.Replace("_", " ")}]");
                var rewardstring = string.Join("", rewards);
                sb.AppendLine($"- {relic.Name}");
                sb.AppendLine($"> {rewardstring}");
                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }
        [Pure]
        public static string ToString(List<WarframeAlertingPrime.SDK.Models.User.Order> infos, List<RivenData> datas, Riven riven)
        {
            var weapon = infos.First().weapon;
            var sb = new StringBuilder();
            var weaponinfo = WFResource.WFTranslateData.Riven.First(d => d.name == weapon);
            sb.AppendLine($"下面是 {riven.zhname} 紫卡的基本信息(来自DE)");
            sb.AppendLine($"类型: {translator.TranslateWeaponType(weaponinfo.type)} 倾向: {weaponinfo.rank}星 倍率: {Math.Round(weaponinfo.modulus, 2)}");
            var rerolled = datas.Where(d => !d.rerolled);
            if (rerolled.Any())
            {
                sb.AppendLine($"0洗均价: {rerolled.First().avg}白金");
            }

            rerolled = datas.Where(d => d.rerolled);
            if (rerolled.Any())
            {
                sb.AppendLine($"全部均价: {rerolled.First().avg}白金");
            }
            sb.AppendLine($"下面是 {riven.zhname} 紫卡的 {infos.Count} 条卖家信息(来自WFA紫卡市场)");
            foreach (var info in infos)
            {
                sb.Append($"[{info.account.gameName}]  ");
                switch (info.account.status)
                {
                    case UserStatus.Offline:
                        sb.AppendLine("离线");
                        break;
                    case UserStatus.Online:
                        sb.AppendLine("在线");
                        break;
                    case UserStatus.InGame:
                        sb.AppendLine("游戏中");
                        break;
                }

                sb.AppendLine($"- 价格: {info.platinum}白鸡 ({info.reset}洗)");
                sb.Append($"  属性: ");
                foreach (var property in info.properties)
                {
                    var number = "";
                    switch (property.displayType)
                    {
                        case DisplayType.Percentage:
                            /*number = property.value.ToString("P");
                            break;*/
                        case DisplayType.Number:
                            number = property.value.ToString(CultureInfo.InvariantCulture) + "%";
                            break;
                    }

                    sb.Append($"{property.name}{(property.isDeduction ? "-" : "+")}{number}|");
                }

                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }
        [Pure]
        public static string ToString(List<Fissure> fissures)
        {
            var sb = new StringBuilder();
            foreach (var fissure in fissures.OrderBy(f => f.tier))
            {
                sb.AppendLine($"[{fissure.node}]");
                sb.AppendLine($"类型:    {fissure.missionType}-{fissure.enemy}");
                sb.AppendLine($"纪元:    {fissure.tier}(T{fissure.tierNum})");
                sb.AppendLine($"{fissure.eta} 后过期");
                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }
        [Pure]
        public static string ToString(SyndicateMission mission, int index)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"集团: {mission.syndicate}");
            sb.AppendLine();
            var count = 0;
            if (index >= 1 && index <= 5)
            {
                mission.jobs = new[] { mission.jobs[index - 1] };
            }
            foreach (var job in mission.jobs)
            {
                count++;
                if (index >= 1 && index <= 5)
                {
                    sb.AppendLine($"> 赏金{index}等级: {job.enemyLevels[0]} - {job.enemyLevels[1]}");
                }
                else
                {
                    sb.AppendLine($"> 赏金{count}等级: {job.enemyLevels[0]} - {job.enemyLevels[1]}");
                }

                sb.AppendLine("- 奖励:");
                foreach (var reward in job.rewardPool)
                {
                    sb.Append($"[{reward}]");
                }

                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }
        [Pure]
        public static string ToString(WFInvasion inv)
        {
            var sb = new StringBuilder();
            var completion = Math.Floor(inv.completion);

            sb.AppendLine($"地点: [{inv.node}]");

            sb.AppendLine($"> 进攻方: {inv.attackingFaction}");
            if (!inv.vsInfestation)
                sb.AppendLine($"奖励: {ToString(inv.attackerReward)}");
            sb.AppendLine($"进度: {completion}%");
            // sb.AppendLine();

            sb.AppendLine($"> 防守方: {inv.defendingFaction}");
            sb.AppendLine($"奖励: {ToString(inv.defenderReward)}");
            sb.Append($"进度 {100 - completion}%");
            return sb.ToString();
        }
        [Pure]
        public static string ToString(CetusCycle cycle)
        {
            var time = (cycle.Expiry - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"),
                TimeUnit.Hour, TimeUnit.Second, " ");
            var status = cycle.IsDay ? "白天" : "夜晚";
            var nextTime = !cycle.IsDay ? "白天" : "夜晚";

            var sb = new StringBuilder();
            sb.AppendLine($"现在地球平原的时间是: {status}");
            //sb.AppendLine($"将在 {cycle.Expiry} 变为 {nextTime}");
            sb.Append($"距离 {nextTime} 还有 {time}");

            return sb.ToString();
        }
        public static string ToString(EarthCycle cycle)
        {
            var time = (cycle.expiry - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"),
                TimeUnit.Hour, TimeUnit.Second, " ");
            var status = cycle.isDay ? "白天" : "夜晚";
            var nextTime = !cycle.isDay ? "白天" : "夜晚";

            var sb = new StringBuilder();
            sb.AppendLine($"现在地球的时间是: {status}");
            //sb.AppendLine($"将在 {cycle.Expiry} 变为 {nextTime}");
            sb.Append($"距离 {nextTime} 还有 {time}");

            return sb.ToString();
        }
        [Pure]
        public static string ToString(VallisCycle cycle)
        {
            var time = (cycle.expiry - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"),
                TimeUnit.Hour, TimeUnit.Second, " ");
            var temp = cycle.isWarm ? "温暖" : "寒冷";
            var nextTemp = !cycle.isWarm ? "温暖" : "寒冷";
            var sb = new StringBuilder();
            sb.AppendLine($"现在金星平原的温度是: {temp}");
            //sb.AppendLine($"将在{cycle.expiry} 变为 {nextTemp}");
            sb.Append($"距离 {nextTemp} 还有 {time}");

            return sb.ToString();
        }

        [Pure]
        public static string ToString(CambionCycle cycle)
        {
            var time = (cycle.expiry - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"),
                TimeUnit.Hour, TimeUnit.Second, " ");
            var status = cycle.active.FirstCharToUpper();
            var nextstatus = "";
            switch (status)
            {
                case "Fass":
                    nextstatus = "Vome";
                    break;
                case "Vome":
                    nextstatus = "Fass";
                    break;
            }
            var sb = new StringBuilder();
            sb.AppendLine($"现在火卫二平原的状态是: {status}");
            sb.Append($"距离 {nextstatus} 还有 {time}");

            return sb.ToString();
        }
        [Pure]
        public static string ToString(Sortie sortie)
        {
            var sb = new StringBuilder();
            sb.AppendLine("指挥官, 下面是今天的突击任务.");
            sb.AppendLine($"> 阵营: {sortie.faction}");
            sb.AppendLine($"> 头头: {sortie.boss}");
            sb.AppendLine();
            foreach (var variant in sortie.variants)
            {
                sb.AppendLine($"[{variant.node}]");
                sb.AppendLine($"- 类型:{variant.missionType}");
                sb.AppendLine($"- 状态:{variant.modifier}");
            }

            return sb.ToString().Trim();
        }
        [Pure]
        public static string ToString(VoidTrader trader)
        {
            var sb = new StringBuilder();
            if (trader.active)
            {
                var time = (DateTime.Now - trader.expiry).Humanize(int.MaxValue,
                    CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Second, " ");
                sb.AppendLine($"虚空商人已抵达: {trader.location}");
                sb.AppendLine($"携带商品:");
                foreach (var inventory in trader.inventory)
                {
                    sb.AppendLine($"         [{inventory.item}] {inventory.ducats}金币 + {inventory.credits}现金");
                }
                //sb.Append($"结束时间:{trader.expiry}({time} 后)");
                sb.Append($"结束时间: {time} 后");
            }
            else
            {
                var time = (DateTime.Now - trader.activation).Humanize(int.MaxValue,
                    CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Day, TimeUnit.Second, " ");
                //sb.Append($"虚空商人将在{trader.activation}({time} 后)抵达{trader.location}");
                sb.Append($"虚空商人将在 {time} 后 抵达{trader.location}");
            }

            return sb.ToString().Trim();
        }
        [Pure]
        public static string ToString(WMInfo info, bool withQR, bool isbuyer)
        {
            var sb = new StringBuilder();
            var itemItemsInSet = info.include.item.items_in_set;
            // var item = itemItemsInSet.Where(i => i.zh.item_name != i.en.item_name).ToList().Last();
            sb.AppendLine($"下面是物品: {info.sale.zh} 按价格{(isbuyer ? "从大到小": "从小到大")}的{info.payload.orders.Length}条 {(isbuyer ? "买家" : "卖家")} 信息");
            sb.AppendLine();
            foreach (var order in info.payload.orders)
            {
                sb.AppendLine($"{order.order_type} {order.platinum} 白鸡 [{order.user.ingame_name}] {order.user.status} ");
                if (withQR)
                {
                    sb.AppendLine(
                        $"- 快捷回复: /w {order.user.ingame_name} Hi! I want to {(isbuyer ? "sell" : "buy")}: {info.sale.en} for {order.platinum} platinum. (warframe.market)");
                }

            }
            // 以后不好看了再说
            return sb.ToString().Trim();
        }
        [Pure]
        public static string ToString(WMInfoEx info, bool withQR, bool isbuyer)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"下面是物品: {info.sale.zh} 按价格{(isbuyer ? "从大到小" : "从小到大")}的{info.orders.Items.Count}条{(isbuyer ? "买家" : "卖家")}信息");
            sb.AppendLine();

            foreach (var order in info.orders.Items)
            {
                sb.AppendLine($"{order.order_type} {order.platinum} 白鸡 [{order.user.ingame_name}] {order.user.status}");
                if (withQR)
                {
                    sb.AppendLine(
                        $"- 快捷回复: /w {order.user.ingame_name} Hi! I want to {(isbuyer ? "sell" : "buy")}: {info.sale.en} for {order.platinum} platinum. (warframe.market)");
                }
            }
            // 这已经很难看了好吧
            return sb.ToString().Trim();
        }
        [Pure]
        public static string ToString(RewardInfo reward)
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
        [Pure]
        public static string ToString(Reward reward)
        {
            var rewards = new List<string>();
            if (reward.Credits > 0)
            {
                rewards.Add($"{reward.Credits} cr");
            }

            foreach (var item in reward.Items)
            {
                rewards.Add(Regex.Replace(item, "(\\d+)(X)", "$1x"));
            }


            foreach (var item in reward.CountedItems)
            {
                rewards.Add($"{item.Count}x{item.Type}");
            }

            return string.Join(" + ", rewards);
        }
    }
}
