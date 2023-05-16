namespace WFBot.Koharu;


public record FissureData(List<FissureData.Fissure> Fissures, int Tier) : IDrawingData
{
    public class Fissure
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public string node { get; set; }
        public string missionType { get; set; }
        public string enemy { get; set; }
        public string tier { get; set; }
        public int tierNum { get; set; }
        public bool expired { get; set; }
        public string eta { get; set; }
        public bool isStorm { get; set; }
        public bool isHard { get; set; }
    }
}


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
                .Text($"{fissure.missionType} - {fissure.enemy}", textOptions with {Size = 43})
                .Text(
                    $"{fissure.tier}(T{fissure.tierNum}) {(fissure.isHard ? "钢铁裂缝" : fissure.isStorm ? "虚空风暴" : "普通裂缝")}",
                    textOptions with {Size = 43})
                .Text($"{fissure.node}", textOptions with {Size = 33})
                .Text($"{fissure.eta}", textOptions with {Size = 33, Color = Color.FromArgb(170, 170, 170)})
                .Margin10()
                .Build()).Build();
        return left;
    }


}