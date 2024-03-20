using GammaLibrary.Extensions;
using SkiaSharp;
using System.Drawing;
using static WFBot.Koharu.InvasionData;

namespace WFBot.Koharu;


public class FissurePainter : Painter<FissureData>
{
    public override IDrawingCommand Draw(FissureData data)
    {
        var fissures = data.Fissures;
        var tier = data.Tier;
        if (!fissures.Any()) return SimpleImageRendering("目前没有裂隙");
        fissures = fissures.Where(x => tier == 0 || x.tierNum == tier).OrderBy(f => f.tierNum).ToList();
        var commands = fissures.Select(Single).ToArray();
        var max = commands.Max(x => x.Size.Width);
        var i = 0;
        var lineColorBool = true;
        foreach (ref var command in commands.AsSpan())
        {
            command = PlaceLeftAndRight(command,
                HorizontalLayout().Margin100().ImageResource($"Factions.{fissures[i++].enemy.ToLower()}").Build(), max)
                .ApplyBackground(SwitchLineColor(ref lineColorBool));
        }

        return VerticalLayout().DrawRange(commands).Build();
    }

    static IDrawingCommand Single(FissureData.Fissure fissure)
    {
        var left = HorizontalLayout(Alignment.Center)
            .ImageResource($"Fissures.{fissure.tierNum}").Margin20()
            .Draw(VerticalLayout()
                .Text($"{fissure.missionType} - {fissure.enemy}", textOptions with { Size = 43 })
                .Text(
                    $"{fissure.tier}(T{fissure.tierNum}) {(fissure.isHard ? "钢铁裂缝" : fissure.isStorm ? "虚空风暴" : "普通裂缝")}",
                    textOptions with { Size = 43 })
                .Text($"{fissure.node}", textOptions with { Size = 33 })
                .Text($"{fissure.eta}", textOptions with { Size = 33, Color = Color.FromArgb(170, 170, 170) })
                .Margin10()
                .Build()).Build();
        return left;
    }
}


public class InvasionPainter : Painter<InvasionData>
{
    public override IDrawingCommand Draw(InvasionData data)
    {
        var invasions = data.Invasions.Select(SingleInvasion).ToArray();
        var lineColorBool = true;
        foreach (ref var command in invasions.AsSpan())
        {
            command = command.ApplyBackground(SwitchLineColor(ref lineColorBool));
        }

        return VerticalLayout().DrawRange(invasions).Build();
    }


    public IDrawingCommand SingleInvasion(InvasionData.WFInvasion invasion)
    {
        var grineerColor = Color.FromArgb(227, 49, 62);
        var infestedColor = Color.FromArgb(106, 220, 141);
        var corpusColor = Color.FromArgb(96, 182, 229);
        var aColor = invasion.attackingFaction switch
        {
            "Corpus" => corpusColor,
            "Infested" => infestedColor,
            "Grineer" => grineerColor
        };
        var aPercent = invasion.completion / 100.0;
        var bColor = invasion.defendingFaction switch
        {
            "Corpus" => corpusColor,
            "Infested" => infestedColor,
            "Grineer" => grineerColor
        };

        var bPercent = 1 - aPercent;
        var percentageBarWidth = 560;
        var percentageBarHeight = 10;
        var percentageBar = HorizontalLayout().Rect(Size.Of(aPercent * percentageBarWidth, percentageBarHeight), aColor)
            .Rect(Size.Of(bPercent * percentageBarWidth, percentageBarHeight), bColor).Build();

        var factionA = $"{invasion.attackingFaction.ToUpper()} {aPercent * 100:F1}%";
        var factionB = $"{bPercent * 100:F1}% {invasion.defendingFaction.ToUpper()}";
        var rewardA = $"{(!invasion.vsInfestation ? $"{ToString(invasion.attackerReward)}" : "")}";
        var rewardB = $"{ToString(invasion.defenderReward)}";
        var attackerImage = GetResourceWithSize($"Factions.{invasion.attackingFaction.ToLower()}",30, 30);
        var defenderImage = GetResourceWithSize($"Factions.{invasion.defendingFaction.ToLower()}",30, 30);
        

        SKBitmap attackerReward1 = null;
        if (!invasion.vsInfestation)
        {
            attackerReward1 = GetInvasionReward(invasion.attackerReward.countedItems.FirstOrDefault()?.type);
        }
        IDrawingCommand attackerReward = invasion.vsInfestation
            ? new Margin(Size.Of(35, 35))
            : attackerReward1.AsCommand();
        var defenderRewardImage = GetInvasionReward(invasion.defenderReward.countedItems.First().type);
        var defenderReward = defenderRewardImage.AsCommand();

        var desc = Text($"{FlipNode(invasion.node)}", textOptions with { Size = 23, Bold = true });
        var infoTextOptions = textOptions with {Size = 18};
        var faction = PlaceLeftAndRight(HorizontalLayout().Image(attackerImage).Margin10().Text(factionA, infoTextOptions).Build(),
            HorizontalLayout().Text(factionB, infoTextOptions).Margin10().Image(defenderImage).Build(), percentageBarWidth, true);
        var reward = PlaceLeftAndRight(HorizontalLayout().Draw(attackerReward).Margin10().Text(rewardA, infoTextOptions).Build(),
            HorizontalLayout().Text(rewardB, infoTextOptions).Margin10().Draw(defenderReward).Build(), percentageBarWidth, true);

        var result = VerticalLayout().Draw(desc).Margin10().Draw(percentageBar).Margin10().Draw(faction).Margin10()
            .Draw(reward).Build().ApplyMargin(20);
        


        return result;
    }

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
    public static string FlipNode(string node)
    {
        return node.Split(' ').Reverse().Connect(" ");
    }

    public static SKBitmap GetInvasionReward(string name)
    {
        var trims = new List<string> { "蓝图", "枪管", "枪机", "枪托", "连接器", "刀刃", "握柄", "散热器" };
        name = trims.Aggregate(name, (current, trim) => current.Replace(trim, ""));
        name = name.Trim();
        return GetResourceWithSize($"InvasionRewards.{name}", 35, 35);
    }
}