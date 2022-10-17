using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;
using GammaLibrary.Extensions;
using Humanizer;
using Humanizer.Localisation;
using PininSharp.Utils;
using SixLabors.Fonts;
using SixLabors.Fonts.Unicode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WFBot.Features.Resource;
using WFBot.Features.Utils;
using WFBot.Orichalt;
using WFBot.Utils;
using ColorX = System.Drawing.Color;

namespace WFBot.Features.ImageRendering
{
    public static class ImageRenderHelper
    {
        static ConcurrentDictionary<string, Image<Rgba32>> Cache = new ();
        static TextOptions defaultOptions = CreateTextOptions(40);
        public static TextOptions CreateTextOptions(int size = 40, bool bold = false)
        {
            var collection = new FontCollection();
            var family = collection.Add("WFConfigs/font.ttf");
            var font = family.CreateFont(size, bold ? FontStyle.Bold : FontStyle.Regular);

            return new TextOptions(font);
        }

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

        public static byte[] Invasion(IEnumerable<WFInvasion> invasions)
        {
            if (!invasions.Any())
            {
                return SimpleImageRendering("额, 没有任何入侵.");
            }
            var images = invasions.AsParallel().AsOrdered().Select(x => SingleInvasion(x)).ToArray();
            var lineColorBool = true;
            foreach (var image in images)
            {
                image.SetBackgroundColor(SwitchLineColor(ref lineColorBool));
            }

            return Finish(StackImageY(images), predefinedSize: 50);
        }

        public static string FlipNode(string node)
        {
            return node.Split(' ').Reverse().Connect(" ");
        }
        public static byte[] InvasionNotification(WFInvasion invasion)
        {
            AsyncContext.SetCommandIdentifier("WFBot通知");
            return Finish(StackImageY(SingleInvasion(invasion)), predefinedSize: 50);
        }

        public static Image<Rgba32> GetInvasionReward(string name)
        {
            var trims = new List<string> { "蓝图", "枪管", "枪机", "枪托", "连接器", "刀刃", "握柄", "散热器" };
            name = trims.Aggregate(name, (current, trim) => current.Replace(trim, ""));
            name = name.Trim();
            return GetResource($"InvasionRewards.{name}");
        }
        public static Image<Rgba32> SingleInvasion(WFInvasion invasion)
        {
            var grineerColor = ColorX.FromArgb(227, 49, 62);
            var infestedColor = ColorX.FromArgb(106, 220, 141);
            var corpusColor = ColorX.FromArgb(96, 182, 229);
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
            var percentageImage = new Image<Rgba32>(560, 10);
            var breakPoint = (int)(aPercent * percentageImage.Width);
            percentageImage.Mutate(x => x.Fill(new Rgba32(aColor.R, aColor.G, aColor.B),  new RectangleF(0,0, breakPoint, percentageImage.Height)));
            percentageImage.Mutate(x => x.Fill(new Rgba32(bColor.R, bColor.G, bColor.B), new RectangleF(breakPoint, 0, percentageImage.Width - breakPoint, percentageImage.Height)));
            var factionA = $"{invasion.attackingFaction.ToUpper()} {aPercent*100:F1}%";
            var factionB = $"{bPercent*100:F1}% {invasion.defendingFaction.ToUpper()}";
            var rewardA = $"{(!invasion.vsInfestation ? $"{WFFormatter.ToString(invasion.attackerReward)}" : "")}";
            var rewardB = $"{WFFormatter.ToString(invasion.defenderReward)}";
            var textOptions = CreateTextOptions(18);
            var factionImage = new Image<Rgba32>(560, 40);
            var rewardImage = new Image<Rgba32>(560, 40);
            using var attackerImage = GetResource($"Factions.{invasion.attackingFaction.ToLower()}").Clone().Resize(30, 30);
            using var defenderImage = GetResource($"Factions.{invasion.defendingFaction.ToLower()}").Clone().Resize(30, 30);
            using var attackerReward = invasion.vsInfestation
                ? new Image<Rgba32>(35, 35)
                : GetInvasionReward(invasion.attackerReward.countedItems.First().type).Clone().Resize(35, 35);
            using var defenderReward = GetInvasionReward(invasion.defenderReward.countedItems.First().type).Clone().Resize(35, 35);
            var desc = RenderText($"{FlipNode(invasion.node)}", CreateTextOptions(23));
            factionImage.Mutate(x => x.DrawImage(attackerImage, new Point(0,0), new GraphicsOptions()));
            rewardImage.Mutate(x => x.DrawImage(attackerReward, new Point(0,0), new GraphicsOptions()));
            factionImage.Mutate(x => x.DrawImage(defenderImage, new Point(factionImage.Width - defenderImage.Width,0), new GraphicsOptions()));
            rewardImage.Mutate(x => x.DrawImage(defenderReward, new Point(rewardImage.Width - defenderReward.Width, 0), new GraphicsOptions()));
            textOptions.Origin = new Vector2(attackerImage.Width + 10, 0);
            factionImage.Mutate(x => x.DrawText(textOptions, factionA, Color.White));
            rewardImage.Mutate(x => x.DrawText(textOptions, rewardA, Color.White));
            textOptions.HorizontalAlignment = HorizontalAlignment.Right;
            textOptions.TextAlignment = TextAlignment.End;
            textOptions.Origin = new Vector2(factionImage.Width - defenderImage.Width - 10, 0);
            factionImage.Mutate(x => x.DrawText(textOptions, factionB, Color.White));
            rewardImage.Mutate(x => x.DrawText(textOptions, rewardB, Color.White));
            return StackImageX(Margin20, StackImageY(Margin10, desc, StackImageY(Margin10, percentageImage, Margin10, factionImage, rewardImage, Margin20)), Margin20);
        }
        static Image<Rgba32> Sell = RenderRectangle(40, 40, new Rgba32(63, 35, 59))
            .OverlayTextCentered("卖", new Rgba32(203, 74, 158), 30)
            .ApplyRoundedCorners(10);

        static Image<Rgba32> Buy = RenderRectangle(40, 40, new Rgba32(25, 62, 53))
            .OverlayTextCentered("买", new Rgba32(32, 158, 112), 30)
            .ApplyRoundedCorners(10);

        static Image<Rgba32> PlatinumIcon = GetResource("WarframeMarket.PlatinumSimple").Resize(50, 50);

        static Image<Rgba32> Cubes = GetResource("WarframeMarket.Cubes").Resize(45, 45);

        public static Rgba32 SwitchLineColor(ref bool lcb)
        {
            lcb = !lcb;
            return lcb ? new Rgba32(23, 30, 33) : new Rgba32(16, 22, 25);
        }
        public static async Task<byte[]> WMInfo(WMInfo info, bool isbuyer, bool quickReply)
        {
            var sb = new StringBuilder();
            /*foreach (var order in info.payload.orders)
            {
                var type = isbuyer ? Buy : Sell;
                var name = RenderText(order.user.ingame_name, CreateTextOptions(), new Rgba32(60, 135, 156));
                var plat = RenderText(order.platinum.ToString(CultureInfo.InvariantCulture), CreateTextOptions(),
                    new Rgba32(203, 74, 141));
                var quantity = RenderText(order.quantity.ToString(), CreateTextOptions(), new Rgba32(115, 120, 120));
                var platicon = GetResource("WarframeMarket.PlatinumSimple").Resize(50, 50);

            }*/
            using var profiler = new ImageRenderProfiler();
            int nameMax = 0;
            int statusMax = 0;
            int platMax = 0;
            int quantityMax = 0;
            var nameOptions = CreateTextOptions(35);
            var statusOptions = CreateTextOptions(30);
            var platOptions = CreateTextOptions(30);
            var quantityOptions = CreateTextOptions(30);
            

            var lines = new List<Image<Rgba32>>();
            profiler.Segment("计算渲染头");

            string assetUrl = Config.Instance.UseWFBotProxy ? $"https://wfbot.cyan.cafe/api/WFBotProxy/{Config.Instance.WFBotProxyToken}*https://warframe.market/static/assets/" : "https://warframe.market/static/assets/";

            Image<Rgba32> avatar = null;
            Task.WaitAll(
                Task.Run(() =>
                    nameMax = MeasureTextsMaxWidth(info.payload.orders.Select(o => o.user.ingame_name).ToArray(),
                        nameOptions)),
                Task.Run(() =>
                    statusMax = MeasureTextsMaxWidth(info.payload.orders.Select(o => o.user.status).ToArray(),
                        statusOptions)),
                Task.Run(() =>
                    platMax = MeasureTextsMaxWidth(
                        info.payload.orders.Select(o => ((int) o.platinum).ToString()).ToArray(), platOptions)),
                Task.Run(() =>
                    quantityMax = MeasureTextsMaxWidth(info.payload.orders.Select(o => o.quantity.ToString()).ToArray(),
                        quantityOptions)),
                Task.Run(() =>
                {
                    var item = info.include.item.items_in_set.First(i => i.url_name == info.sale.code);
                    var avatar_url = assetUrl + (info.include.item.items_in_set.Length == 1 ? item.thumb : item.set_root ? item.thumb : item.sub_icon);
                    avatar = new Image<Rgba32>(128, 128).OverlayImageCentered(WebHelper.LoadImageFromWeb(avatar_url).Result);
                    var radius = (Math.Max(avatar.Width, avatar.Height) - 8) / 2;
                    avatar = avatar.ApplyCircleWithBoarder(radius, 4, new Rgba32(60, 135, 156));
                }));

            profiler.Segment("渲染avatar");

            var tags = info.sale.zh.Split(' ');
            var tagOptions = CreateTextOptions(50);
            var widths = tags.Select(t => (int)TextMeasurer.Measure(t, tagOptions).Width).ToArray();
            var texts = new List<Image<Rgba32>>();
            var currentLength = 0;
            var currentText = "";
            for (int i = 0; i < tags.Length; i++)
            {
                if (currentLength >= 300 || i == tags.Length-1)
                {
                    if (i == tags.Length-1)
                    {
                        currentText += currentText.IsNullOrEmpty() ? tags[i] : " " + tags[i];
                        currentLength += widths[i];
                    }
                    texts.Add(RenderText(currentText, tagOptions));
                    currentLength = default;
                    currentText = default;
                }
                currentText += currentText.IsNullOrEmpty() ? tags[i] : " " + tags[i];
                currentLength += widths[i];
            }
            var itemName = StackImageYCentered(texts.ToArray());
            lines.Add(Margin20);
            lines.Add(StackImageXCentered(Margin20, avatar, Margin40, itemName));
            lines.Add(Margin20);
            profiler.Segment("渲染头部");

            var lineColorBool = true;

            var headerOption = CreateTextOptions(20);
            var header = StackImageXCentered(
                Margin30,
                new Image<Rgba32>(40, 40),
                Margin30,
                RenderText("游戏名", headerOption, new Rgba32(145, 149, 150), nameMax, true),
                Margin30,
                RenderText("状态", headerOption, new Rgba32(145, 149, 150), statusMax, true),
                Margin30,
                RenderText("单价", headerOption, new Rgba32(145, 149, 150), platMax, true),
                Margin30,
                new Image<Rgba32>(50, 50),
                RenderText("数量", headerOption, new Rgba32(145, 149, 150), quantityMax+10+Cubes.Width, true),
                Margin30
            );
            // // 是不是很好奇为什么两侧要加空格? 当然是为了把下划线延长出来捏

            header = header.SetBackgroundColor(SwitchLineColor(ref lineColorBool));

            lines.Add(header);
            profiler.Segment("渲染表头");

            var nl = new Dictionary<Order, Image<Rgba32>>();
            Parallel.ForEach(info.payload.orders, order =>
            {
                var wmsingle = WMInfoSingle(isbuyer ? Buy : Sell,
                    new TextWithParams(order.user.ingame_name, nameMax, nameOptions),
                    new TextWithParams(order.user.status, statusMax, statusOptions),
                    new TextWithParams(((int) order.platinum).ToString(), platMax, platOptions),
                    new TextWithParams(order.quantity.ToString(), quantityMax, quantityOptions));
                wmsingle = wmsingle.SetBackgroundColor(SwitchLineColor(ref lineColorBool));
                lock (nl)
                {
                    nl.Add(order, wmsingle);
                }
            });
            
            foreach (var order in info.payload.orders)
            {
                lines.Add(nl[order]);
            }
            profiler.Segment("渲染表内容");

            var len = lines.Last().Width;
            var margin = len / 13.0;
            var image = RenderRectangle((int)(len - 2 * margin), 8, new Rgba32(255,255,255)).ApplyRoundedCorners(7);
            lines.Add(Margin20);
            lines.Add(StackImageX(new Image<Rgba32>((int)margin,1),image));
            lines.Add(Margin20);
            profiler.Segment("渲染底部");
            var result = StackImageY(lines.ToArray());
            profiler.Segment("图片拼合");
            return Finish(result, 60);
             

            // return Finish(StackImageY(info.payload.orders.AsParallel().AsOrdered().Select(order => WMInfoSingle(isbuyer ? Buy : Sell, new TextWithParams(order.user.ingame_name, nameMax, nameOptions), new TextWithParams(order.user.status, statusMax, statusOptions), new TextWithParams(((int)order.platinum).ToString(), platMax, platOptions), new TextWithParams(order.quantity.ToString(), quantityMax, quantityOptions))).ToArray()));


            // 写的一拖十

            /*var color1 = isbuyer ? new Color(new Rgba32(244, 67, 54)) : new Color(new Rgba32(100, 221, 23));
            var c1 = StackImageX(RenderText($"  物品: {info.sale.zh} 按价格{(isbuyer ? "从大到小" : "从小到大")} {info.payload.orders.Length}个 "), RenderText((isbuyer ? "买家  " : "卖家  "), color: color1));

            var c2s = new List<Image<Rgba32>>();
            foreach (var order in info.payload.orders)
            {
                var status = order.user.status switch
                {
                    "游戏中" => new Color(new Rgba32(89, 164, 231)),
                    "在线" => Color.White,
                    "离线" => new Color(new Rgba32(170, 170, 170))
                };
                c2s.Add(StackImageX(RenderText($"  {order.order_type} {order.platinum} 白鸡 [{order.user.ingame_name}]  "), RenderText($"{order.user.status}  ",color: color2)));
            }
            // 以后不好看了再说
            var c2 = StackImageY(c2s.ToArray());
            var sb2= new StringBuilder();
            if (!quickReply)
            {
                sb2.AppendLine($"直接发送 qr 来获得快捷回复.");
            }

            if (!isbuyer)
            {
                sb2.AppendLine($"直接发送 buyer 来查询买家.");
            }

            sb2.AppendLine();
            
            Thread.MemoryBarrier();
            return Finish(StackImageY(Margin20,c1, Margin10, c2, StackImageX(Margin20, RenderText("\n"+sb2.ToString().AddPlatformInfo().AddRemainCallCount())),Margin20));*/

        }

        public static int MeasureTextsMaxWidth(string[] texts, TextOptions options)
        {
            return texts.Select(t => (int)TextMeasurer.Measure(t, options).Width).Max();
        }
        public class TextWithParams
        {
            public TextWithParams(string text, int maxWidth, TextOptions options = null)
            {
                Text = text;
                MaxWidth = maxWidth;
                Options = options ?? CreateTextOptions();
            }

            public string Text { get; set; }
            public int MaxWidth{ get; set; }
            public TextOptions Options { get; set; }
        }
        public static Image<Rgba32> WMInfoSingle(Image<Rgba32> type, TextWithParams name, TextWithParams status, TextWithParams plat, TextWithParams quantity)
        {
            
            var statusColor = status.Text switch
            {
                "游戏中" => ColorX.FromArgb(157,119,233),
                "在线" => ColorX.FromArgb(0, 161, 0),
                "离线" => ColorX.FromArgb(209, 0, 0)
            };
            
            return StackImageXCentered(
                Margin30
                ,type,
                Margin30,
                RenderText(name, ColorX.FromArgb(79, 177, 205)),
                Margin30,
                RenderText(status, statusColor),
                Margin30,
                RenderText(plat, ColorX.FromArgb(235, 82, 182)),
                PlatinumIcon,
                Margin30,
                RenderText(quantity, ColorX.FromArgb(157, 180, 187)),
                Margin10,
                Cubes,
                Margin30);

        }


        public static byte[] SimpleImageRendering(string s,TextOptions options = null, int maxLength = -1)
        {
            var option = options ?? CreateTextOptions();
            if (maxLength != -1) option.WrappingLength = maxLength;
            
            return Finish(StackImageY(Margin30, StackImageX(Margin30, RenderText(s, option), Margin30), Margin30));
        }


        public static byte[] RivenAuction(List<RivenAuction> rivens, WeaponInfo weapon)
        {
            var baseOption = CreateTextOptions(30);
            
            var baseMsg = StackImageX(Margin20, RenderText(WFFormatter.GetRivenInfoString(weapon), baseOption));
            var s = rivens.AsParallel().AsOrdered().Select(x => SingleRiven(x, weapon)).ToArray();
            var images = new List<Image<Rgba32>>();
            for (int i = 0; i < s.Length / 3; i++)
            {
                images.Add(StackImageX(s[i*3],Margin10, s[i*3+1],Margin10 ,s[i*3 + 2]));
                images.Add(Margin20);
            }

            if (s.Length % 3 == 1)
            {
                images.Add(s[s.Length - 1]);
            }
            if (s.Length % 2 == 1)
            {
                images.Add(StackImageX(s[s.Length - 2],Margin10, s[s.Length - 1]));
            }

            var r = StackImageYCentered(images.ToArray());
            return Finish(StackImageYCentered(Margin10,baseMsg, Margin20,r, Margin20, RenderText("紫卡渲染目前还在优化, 下个版本会渲染为真实紫卡.") ,Margin20));
        }
        private static WFTranslator translator => WFResources.WFTranslator;

        public static Image<Rgba32> SingleRiven(RivenAuction auction, WeaponInfo weapon)
        {
            var option = CreateTextOptions(23);
            option.TextAlignment = TextAlignment.Center;
            
            string polarity;
            switch (auction.Item.Polarity)
            {
                case "madurai":
                    polarity = "γ";
                    break;
                case "vazarin":
                    polarity = "▽";
                    break;
                case "naramon":
                    polarity = "-";
                    break;
                case "zenurik":
                    polarity = "=";
                    break;
                default:
                    polarity = "";
                    break;
            }

            string ownerstatus;
            Color color;
            switch (auction.Owner.Status)
            {
                case "ingame":
                    ownerstatus = "游戏中";
                    color = new Color(new Rgba32(89, 164, 231));
                    break;
                case "online":
                    ownerstatus = "在线";
                    color = Color.White;
                    break;
                case "offline":
                    color = new Color(new Rgba32(170, 170, 170));
                    ownerstatus = "离线";
                    break;
                default:
                    ownerstatus = "";
                    color = Color.White;
                    break;
            }

            var price = auction.BuyoutPrice ?? auction.StartingPrice;

            var titleOption = CreateTextOptions(28);
            titleOption.WrappingLength = 400;
            titleOption.TextAlignment = TextAlignment.Center;
            var title = RenderText($"[{auction.Owner.IngameName} {ownerstatus}]\n{price}白金", titleOption, color);
            var c0 = new Image<Rgba32>(400, 100);
            var r = RenderText($"{auction.Item.MasteryLevel} {polarity}", option);
            c0.Mutate(x => x.DrawImage(r, new Point(c0.Width-r.Width - 10, 10), new GraphicsOptions()));
            var c1 = RenderText(new TextWithParams($"<{weapon.zhname} {auction.Item.Name}>", 400, option), ColorX.White, dontUseMaxLength: true);
            var t = new Image<Rgba32>(380, (int)TextMeasurer.Measure("鸡", option).Height);
            var rOptions = CreateTextOptions(23);
            t.Mutate(x => x.DrawText(rOptions, $"{auction.Item.ModRank}级", Color.White));
            rOptions.HorizontalAlignment = HorizontalAlignment.Right;
            rOptions.Origin = new Vector2(t.Width, 0);
            t.Mutate(x => x.DrawText(rOptions, $"{auction.Item.ReRolls}洗", Color.White));
          

            var sb = new StringBuilder();

            foreach (var attribute in auction.Item.Attributes)
            {
                sb.AppendLine($"{(attribute.Positive ? "+" : ""/*fun fact, 后面这个数据带正负*/)}{attribute.Value}% {translator.GetAttributeEffect(attribute.UrlName)}");
            }
            
            var c3 = RenderText(new TextWithParams(sb.ToString(), 400, option), ColorX.White, dontUseMaxLength: true);
            var i = StackImageYCentered(c0,c1, c3, t);
            var rect = RenderRectangle(i.Width + 20, i.Height + 20, Color.FromRgb(50,50,50));
            rect.ApplyRoundedCorners(7);
            rect.Mutate(x => x.DrawImage(i, new Point(10,10), new GraphicsOptions()));
            
            return StackImageYCentered(title, rect);
        }
        
        public static byte[] Finish(Image<Rgba32> image, int ?predefinedSize = null)
        {
            var profiler = new ImageRenderProfiler();
            var text = "> WFBot_  "; // 好兄弟 虽然你可以改 但是不建议你改 至少保留一下原文吧
            TextOptions options;
            var size = predefinedSize ?? 60.0 / 0.78;
            FontRectangle measure;
            
            do
            {
                size *= 0.78;
                options = CreateTextOptions((int)size);
                measure = TextMeasurer.Measure(text, options);
            } while (measure.Width / (double)image.Width > 0.42 && predefinedSize == null);
            profiler.Segment("计算底部渲染的文字大小");

            options.HorizontalAlignment = HorizontalAlignment.Right;
            options.VerticalAlignment = VerticalAlignment.Top;
            var textHeight = (int)measure.Height;

            var width = image.Width;
            var height = image.Height;
            options.Origin = new Vector2(width, height);
            var imageResult = new Image<Rgba32>(width, height + textHeight, new Rgba32(42,43,48)); // 我不会说我是从色图 https://danbooru.donmai.us/posts/2931102 取的颜色
            profiler.Segment("初始化完成渲染");
            imageResult.Mutate(x => x.DrawImage(image, new Point(0,0), new GraphicsOptions(){Antialias = false}));
            profiler.Segment("渲染原图");
            imageResult.Mutate(x => x.Fill(new DrawingOptions(), new Color(new Rgba32(100, 181, 246)), new RectangleF(0, height, width, textHeight)));
            imageResult.Mutate(x => x.DrawText(options,text,new Color(new Rgba32(255,255,255))));
            options.HorizontalAlignment = HorizontalAlignment.Left;
            options.Origin = new Vector2(0, height);
            imageResult.Mutate(x => x.DrawText(options, "   / " + AsyncContext.GetCommandIdentifier(), new Color(new Rgba32(255, 255, 255))));
            profiler.Segment("渲染底部标签");

            var ms = new MemoryStream();
            imageResult.Save(ms, new PngEncoder() {CompressionLevel = PngCompressionLevel.BestSpeed});
            image.Dispose();
            imageResult.Dispose();
            try
            {
                if (Random.Shared.NextDouble() < 0.015)
                {
                    MiguelNetwork.Reply(AsyncContext.GetOrichaltContext(), "提示: 图片渲染目前还在测试阶段, 可以在 <https://github.com/TRKS-Team/WFBot/issues/161> 反馈.\n在命令前或者命令后加入 * 可以对这个命令关闭图片渲染.");
                }
            }
            catch (Exception e)
            {
                
            }
            profiler.Segment("完成渲染");
            return ms.ToArray();
        }

        public static Image<Rgba32> GetResource(string path)
        {

            if (!Cache.ContainsKey(path))
            {
                var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("WFBot.Resources." + path + ".png");
                Cache[path] = Image.Load<Rgba32>(stream, new PngDecoder());
            }
            return Cache[path];
        }

        static Image<Rgba32> Margin10 = new Image<Rgba32>(10, 10, new Rgba32(0, 0, 0, 0));

        static Image<Rgba32> Margin20 = new Image<Rgba32>(20, 20, new Rgba32(0, 0, 0, 0));

        static Image<Rgba32> Margin30 = new Image<Rgba32>(30, 30, new Rgba32(0, 0, 0, 0));

        static Image<Rgba32> Margin40 = new Image<Rgba32>(40, 40, new Rgba32(0, 0, 0, 0));

        static Image<Rgba32> Margin100 = new Image<Rgba32>(100, 100, new Rgba32(0, 0, 0, 0));

        // 新加静态资源之后请加入StaticResources到内
        static Image<Rgba32>[] StaticResources = new Image<Rgba32>[] {Margin10, Margin20, Margin30, Margin40, Margin100, Buy, Sell, PlatinumIcon, Cubes};
        static Image<Rgba32>[] Margins = new Image<Rgba32>[] {Margin10, Margin20, Margin30, Margin40, Margin100};

        public static Image<Rgba32> StackImageX(params Image<Rgba32>[] images)
        {
            var width = images.Sum(x => x.Width);
            var height = images.Max(x => x.Height);
            var x = 0;
            var image = new Image<Rgba32>(width, height, new Rgba32(0, 0, 0, 0));
            foreach (var i in images)
            {
                if (!Margins.Contains(i))
                    image.Mutate(m => m.DrawImage(i, new Point(x, 0), new GraphicsOptions()));
                if (!StaticResources.Contains(i) && !Cache.Any(c => c.Value == i)) i.Dispose();
                x += i.Width;
            }

            return image;
        }
        public static Image<Rgba32> StackImageXCentered(params Image<Rgba32>[] images)
        {
            var width = images.Sum(x => x.Width);
            var height = images.Max(x => x.Height);
            var x = 0;
            var image = new Image<Rgba32>(width, height, new Rgba32(0, 0, 0, 0));
            foreach (var i in images)
            {
                if (!Margins.Contains(i))
                    image.Mutate(m => m.DrawImage(i, new Point(x, height / 2 - i.Height / 2), new GraphicsOptions()));
                if (!StaticResources.Contains(i) && !Cache.Any(c => c.Value == i)) i.Dispose();
                x += i.Width;
            }

            return image;
        }
        public static Image<Rgba32> StackImageYCentered(params Image<Rgba32>[] images)
        {
            var width = images.Max(x => x.Width);
            var height = images.Sum(x => x.Height);
            var y = 0;
            var image = new Image<Rgba32>(width, height, new Rgba32(0, 0, 0, 0));
            foreach (var i in images)
            {
                if (!Margins.Contains(i))
                    image.Mutate(m => m.DrawImage(i, new Point(width / 2 - i.Width / 2, y), new GraphicsOptions()));
                if (!StaticResources.Contains(i) && !Cache.Any(c => c.Value == i)) i.Dispose();
                y += i.Height;
            }

            return image;
        }
        public static Image<Rgba32> StackImageY(params Image<Rgba32>[] images)
        {
            var width = images.Max(x => x.Width);
            var height = images.Sum(x => x.Height);
            var y = 0;
            var image = new Image<Rgba32>(width, height, new Rgba32(0, 0, 0, 0));
            foreach (var i in images)
            {
                if (!Margins.Contains(i))
                    image.Mutate(m => m.DrawImage(i, new Point(0, y), new GraphicsOptions()));
                if (!StaticResources.Contains(i) && !Cache.Any(c => c.Value == i)) i.Dispose();
                y += i.Height;
            }

            return image;
        }
        public static Image<Rgba32> SetBackgroundColor(this Image<Rgba32> image, Color color)
        {
            image.Mutate(x => x.BackgroundColor(color));
            return image;
        }
        public static Image<Rgba32> OverlayImageCentered(this Image<Rgba32> background, Image<Rgba32> image)
        {
            var width = Math.Max(background.Width, image.Width);
            var height = Math.Max(background.Height, image.Height);
            var isbigger = background.Width >= image.Width && background.Height >= image.Height;
            var result = new Image<Rgba32>(width, height, new Rgba32(0, 0, 0, 0));

            result.Mutate(m => m.DrawImage(background, isbigger
                ? new Point(0, 0)
                : new Point((width - background.Width) / 2,
                    (height - background.Height) / 2), new GraphicsOptions()));
            result.Mutate(m => m.DrawImage(image,
                isbigger
                    ? new Point((width - image.Width) / 2, (height - image.Height) / 2)
                    : new Point(0, 0), new GraphicsOptions()));
            return result;
        }

        public static Image<Rgba32> OverlayTextCentered(this Image<Rgba32> background, string text, Color? color = null, int size = 40)
        {
            color ??= Color.White;
            var options = CreateTextOptions(size);
            options.HorizontalAlignment = HorizontalAlignment.Center;
            options.VerticalAlignment = VerticalAlignment.Center;
            options.Origin = new Vector2(background.Width / 2, background.Height / 2);
            background.Mutate(x => x.DrawText(options, text, color.Value));
            return background;
        }
        public static Image<Rgba32> RenderRectangle(int width, int height, Color? color = null)
        {
            color ??= Color.White;
            var image = new Image<Rgba32>(width, height, color.Value);
            return image;
        }
        public static Image<Rgba32> RenderText(string s, TextOptions options = null, Color? color = null, int minWidth = -1, bool underline = false)
        {
            options ??= defaultOptions;
            color ??= Color.White;
            // 就不用jieba了
            if (underline)
            {
                options.TextRuns = new List<TextRun> { new() {Start = 0, End = s.GetGraphemeCount(),TextDecorations = TextDecorations.Underline } };
            }
            if (s.IsNullOrWhiteSpace()) return new Image<Rgba32>(1, 1);
            // List<string> lines = new();
            //
            // var start = 0;
            // var end = 0;
            //
            //
            // while (end != s.Length)
            // {
            //     end++;
            //     if (TextMeasurer.Measure(s[start..end], options).Width > capLength)
            //     {
            //         var lastChar = s[end - 1];
            //         if (char.IsLetterOrDigit(lastChar))
            //         {
            //             var i = end;
            //             while (true)
            //             {
            //                 i--;
            //                 if (i <= start + 1)
            //                 {
            //                     end -= 1;
            //                     break;
            //                 }
            //
            //                 if (!char.IsLetterOrDigit(s[i]))
            //                 {
            //                     end = i + 1;
            //                 }
            //             }
            //         }
            //         else
            //         {
            //             end -= 1;
            //         }
            //
            //         lines.Add(s[start..end]);
            //         start = end;
            //     }
            // }
            //
            // if (start != s.Length - 1)
            // {
            //     lines.Add(s[start..end]);
            // }


            var measure = TextMeasurer.Measure(s, options);
            var image = new Image<Rgba32>(minWidth == -1 ? (int)measure.Width : Math.Max(minWidth, (int)measure.Width), (int) measure.Height, new Rgba32(0, 0, 0, 0));
            image.Mutate(x => x.DrawText(options, s, color.Value));
            return image;
        }

        public static Image<Rgba32> RenderText(TextWithParams t, ColorX color = default, bool underline = false, bool dontUseMaxLength = false)
        {
            return RenderText(t,new Rgba32(color.R, color.G, color.B),underline,dontUseMaxLength);
        }


        public static Image<Rgba32> RenderText(TextWithParams t, Color? color = null, bool underline = false, bool dontUseMaxLength = false)
        {
            color ??= Color.White;
            if (underline)
            {
                t.Options.TextRuns = new List<TextRun> { new() {Start = 0, End = t.Text.GetGraphemeCount(), TextDecorations = TextDecorations.Underline } };
            }
            if (t.Text.IsNullOrWhiteSpace()) return new Image<Rgba32>(1, 1);
            var measure = TextMeasurer.Measure(t.Text, t.Options);
            var image = new Image<Rgba32>(dontUseMaxLength ? (int)measure.Width : t.MaxWidth == (int)measure.Width ? (int)measure.Width : Math.Max(t.MaxWidth, (int)measure.Width), (int) measure.Height, new Rgba32(0, 0, 0, 0));
            image.Mutate(x => x.DrawText(t.Options, t.Text, color.Value));
            return image;
        }
        private static Image<Rgba32> Resize(this Image<Rgba32> image, int width, int height)
        {
            image.Mutate(x => x.Resize(width, height));
            return image;
        }
        private static Image<Rgba32> ApplyCircle(this Image<Rgba32> image, float circleRadius)
        {
            image.Mutate(x => x.ApplyCircle(circleRadius));
            return image;
        }
        private static Image<Rgba32> ApplyCircleWithBoarder(this Image<Rgba32> image, float circleRadius, float boarderWidth, Color boarderColor)
        {
            image.Mutate(x => x.ApplyCircleWithBoarder(circleRadius, boarderWidth, boarderColor));
            return image;
        }
        public static IImageProcessingContext ApplyCircle(this IImageProcessingContext ctx, float circleRadius)
        {
            Size size = ctx.GetCurrentSize();
            IPathCollection corners = BuildCircleCorners(size.Width, size.Height, circleRadius);

            ctx.SetGraphicsOptions(new GraphicsOptions()
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // enforces that any part of this shape that has color is punched out of the background
            });

            foreach (var c in corners)
            {
                ctx = ctx.Fill(Color.Red, c);
            }
            return ctx;
        }
        public static IImageProcessingContext ApplyCircleWithBoarder(this IImageProcessingContext ctx, float circleRadius, float boarderWidth, Color boarderColor)
        {
            Size size = ctx.GetCurrentSize();
            IPathCollection corners = BuildCircleCorners(size.Width, size.Height, circleRadius);

            ctx.SetGraphicsOptions(new GraphicsOptions
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // enforces that any part of this shape that has color is punched out of the background
            });

            foreach (var c in corners)
            {
                ctx = ctx.Fill(Color.Red, c);
            }

            var boarder = new EllipsePolygon(size.Width / 2, size.Height / 2, circleRadius+boarderWidth).Clip(new EllipsePolygon(size.Width / 2, size.Height / 2, circleRadius));
            ctx.SetGraphicsOptions(new GraphicsOptions());
            ctx = ctx.Fill(boarderColor, boarder);
            return ctx;
        }
        public static IPathCollection BuildCircleCorners(int imageWidith, int imageHeight, float circleRadius)
        {
            var rect = new RectangularPolygon(0, 0, imageWidith, imageHeight);

            IPath corners = rect.Clip(new EllipsePolygon(imageWidith/2, imageHeight/2, circleRadius));

            return new PathCollection(corners);

        }
        private static Image<Rgba32> ApplyRoundedCorners(this Image<Rgba32> image, float cornerRadius)
        {
            image.Mutate(x => x.ApplyRoundedCorners(cornerRadius));
            return image;
        }
        // This method can be seen as an inline implementation of an `IImageProcessor`:
        // (The combination of `IImageOperations.Apply()` + this could be replaced with an `IImageProcessor`)
        private static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext ctx, float cornerRadius)
        {
            Size size = ctx.GetCurrentSize();
            IPathCollection corners = BuildCorners(size.Width, size.Height, cornerRadius);

            ctx.SetGraphicsOptions(new GraphicsOptions
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // enforces that any part of this shape that has color is punched out of the background
            });
            
            // mutating in here as we already have a cloned original
            // use any color (not Transparent), so the corners will be clipped
            foreach (var c in corners)
            {
                ctx = ctx.Fill(Color.Red, c);
            }
            return ctx;
        }

        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            // first create a square
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            IPath cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            // corner is now a corner shape positions top left
            //lets make 3 more positioned correctly, we can do that by translating the original around the center of the image

            float rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
            float bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

            // move it across the width of the image - the width of the shape
            IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }

        public static byte[] Cycles(CetusCycle cetuscycle, VallisCycle valliscycle, EarthCycle earthcycle, CambionCycle cambioncycle)
        {
            var c1 = RenderText("地球平原 "+ (cetuscycle.IsDay ? "[白天]" :"[夜晚]"), minWidth: 120);
            var c1i = GetResource($"Weathers.{(cetuscycle.IsDay ? "sun" : "night")}");
            var c1ti = (cetuscycle.Expiry - DateTime.Now);
            var c1t = RenderText(c1ti.Hours > 0 ? c1ti.ToString("h\\h\\ m\\m\\ s\\s"): c1ti.ToString("m\\m\\ s\\s"));
            var e1 = StackImageYCentered(c1, Margin20, c1i, Margin20, c1t);

            var c2 = RenderText("地球 " + (earthcycle.isDay ? "[白天]" : "[夜晚]"), minWidth: 120);
            var c2i = GetResource($"Weathers.{(earthcycle.isDay ? "sun" : "night")}");
            var c2ti = (earthcycle.expiry - DateTime.Now);
            var c2t = RenderText(c2ti.Hours > 0 ? c2ti.ToString("h\\h\\ m\\m\\ s\\s") : c2ti.ToString("m\\m\\ s\\s"));
            var e2 = StackImageYCentered(c2, Margin20, c2i, Margin20, c2t);

            var c3 = RenderText("金星平原 " + (valliscycle.isWarm ? "[温暖]" : "[夜晚]"), minWidth: 120);
            var c3i = GetResource($"Weathers.{(valliscycle.isWarm ? "warm" : "cold")}");
            var c3ti = (valliscycle.expiry - DateTime.Now);
            var c3t = RenderText(c3ti.Hours > 0 ? c3ti.ToString("h\\h\\ m\\m\\ s\\s") : c3ti.ToString("m\\m\\ s\\s"));
            var e3 = StackImageYCentered(c3, Margin20, c3i, Margin20, c3t);

            var c4 = RenderText("火卫二平原 " + (cambioncycle.active.FirstCharToUpper() == "Fass" ? "[Fass]" : "[Vome]"), minWidth: 120);
            var c4i = GetResource($"Weathers.{(cambioncycle.active.FirstCharToUpper() == "Fass" ?"sun" : "night")}");
            var c4ti = (cambioncycle.expiry - DateTime.Now);
            var c4t = RenderText(c4ti.Hours > 0 ? c4ti.ToString("h\\h\\ m\\m\\ s\\s") : c4ti.ToString("m\\m\\ s\\s"));
            var e4 = StackImageYCentered(c4, Margin20, c4i, Margin20, c4t);



            return Finish(StackImageY(Margin40, StackImageXCentered(Margin30, e1, Margin30, e2, Margin30, e3, Margin30, e4, Margin30),Margin40));
        }
    }
}
