
/*

 
 
 
 

                public static byte[] Fissures(List<Fissure> fissures, int tier)
                {
                    if (!fissures.Any())
                    {
                        return SimpleImageRendering("目前没有裂隙.");
                    }
                    fissures = fissures.Where(x => tier == 0 || x.tierNum == tier).OrderBy(f => f.tierNum).ToList();
                    var images = fissures.AsParallel().AsOrdered().Select(x => SingleFissure(x)).ToArray();
                    var max = images.Max(x => x.Width);
                    Parallel.For(0, fissures.Count, i =>
                    {
                        images[i] = StackImageXCentered(images[i], new Image<Rgba32>(max - images[i].Width + 10, 1),
                            Margin100,
                            GetResource($"Factions.{fissures[i].enemy.ToLower()}"));
                    });


                    var lineColorBool = true;
                    images = images.ForEach(i => i.SetBackgroundColor(SwitchLineColor(ref lineColorBool))).ToArray();
                    var image = StackImageY(images);
                    return Finish(image);
                    // 我还没想到怎么给Margin上色
                }

                public static Image<Rgba32> SingleFissure(Fissure fissure)
                {
                    return StackImageXCentered(GetResource($"Fissures.{fissure.tierNum}"), Margin20,
                        StackImageY(
                            RenderText($"{fissure.missionType} - {fissure.enemy}", options: CreateTextOptions(43, true)),
                            RenderText($"{fissure.tier}(T{fissure.tierNum}) {(fissure.isHard ? "钢铁裂缝" : fissure.isStorm ? "虚空风暴" : "普通裂缝")}", CreateTextOptions(33), Color.White),
                            RenderText($"{fissure.node}", CreateTextOptions(33)),
                            RenderText($"{fissure.eta}", CreateTextOptions(33), new Color(new Rgba32(170,170,170))), Margin10));
                }
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 */