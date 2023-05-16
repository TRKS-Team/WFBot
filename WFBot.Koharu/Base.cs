using SkiaSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using Topten.RichTextKit;

namespace WFBot.Koharu;

public class Painters
{
    public static T Create<T>()
    {
        return Activator.CreateInstance<T>();
    }
}

public abstract class Painter<T>
{
    public abstract IDrawingCommand Draw(T data);
    public static Color BaseBackgroundColor = Color.FromArgb(42, 43, 48); // https://danbooru.donmai.us/posts/2931102
    protected virtual double? BottomInfoTagSize => null;
    public static TextOptions textOptions => CreateTextOptions();

    static TextOptions CreateTextOptions()
    {
        return new TextOptions(40,  Color.White, false, -1);
    }

    protected static VerticalLayoutBuilder VerticalLayout(Alignment defaultAlignment = Alignment.TopOrLeft, int? minHeight = null)
    {
        var layout = new VerticalLayoutBuilder();
        layout.DefaultAlignment(defaultAlignment);
        if (minHeight != null)
        {
            layout.MarginY(minHeight.Value);
        }

        return layout;
    }

    protected static HorizontalLayoutBuilder HorizontalLayout(Alignment defaultAlignment = Alignment.TopOrLeft, int? minWidth = null)
    {
        var layout = new HorizontalLayoutBuilder();
        layout.DefaultAlignment(defaultAlignment);
        if (minWidth != null)
        {
            layout.MarginX(minWidth.Value);
        }
        return layout;
    }

    protected static IDrawingCommand PlaceLeftAndRight(IDrawingCommand left, IDrawingCommand right, int? minWidth = null)
    {
        var layout = new HorizontalDockLayoutBuilder(minWidth + right.Size.Width, false);
        return layout.Draw(left).Alignment(Alignment.TopOrLeft).Draw(right).Alignment(Alignment.DownOrRight).Build();
    }

    protected static IDrawingCommand SimpleImageRendering(string text, int maxWidth = 1000)
    {
        return new MarginCommand(new TextCommand(text, textOptions with{ MaxWidth = maxWidth}), 30,30,30,30);
    }

    protected static Color SwitchLineColor(ref bool lcb)
    {
        lcb = !lcb;
        return lcb ? Color.FromArgb(23, 30, 33) : Color.FromArgb(16, 22, 25);
    }
    
    static ConcurrentDictionary<string, SKBitmap> Cache = new();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static SKBitmap GetResource(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WFBot.Koharu.Resources." + path + ".png");
        if (stream == null)
        {
            Console.WriteLine($"错误: {path} 为 null");
            //todo
            /*var image = new Image<Rgba32>(50, 50, new Rgba32(0, 0, 0));
            image.Mutate(x => x.Fill(new Color(new Rgba32(206, 64, 202)), new RectangleF(0, 0, 25, 25)));
            image.Mutate(x => x.Fill(new Color(new Rgba32(206, 64, 202)), new RectangleF(25, 25, 25, 25)));
            */
            return SKBitmap.FromImage(SKImage.Create(new SKImageInfo(1,1)));
        }
        Cache[path] = SKBitmap.Decode(stream);
        return Cache[path];
    }

}

public static class WFBotExtensions
{
    static Color BaseBackgroundColor = Color.FromArgb(42, 43, 48); // https://danbooru.donmai.us/posts/2931102
    public static IDrawingCommand ApplyWFBotInfoTag(this IDrawingCommand command, string wfbotCommand, bool isNotification = false, double? BottomInfoTagSize = null)
    {
        var v = command;
        var size = BottomInfoTagSize ?? 60.0 / 0.78;
        var text = "> WFBot_  "; // 好兄弟 虽然你可以改 但是不建议你改 至少保留一下原文吧
        TextCommand wfbotTextCommand;

        do
        {
            size *= 0.78;
            wfbotTextCommand = new TextCommand(text, Painter<object>.textOptions with { Size = (int)size });
        } while (wfbotTextCommand.Size.Width / (double)v.Size.Width > 0.42 && BottomInfoTagSize == null && size > 2);
        
        var bottomBarColor = isNotification ? Color.FromArgb(244, 67, 54) : Color.FromArgb(3, 169, 244);
        var commandInfoText = new TextCommand("   / " + wfbotCommand, Painter<object>.textOptions with{ Size = (int)size });

        return new VerticalLayoutBuilder()
            .Draw(command.ApplyBackground(BaseBackgroundColor))
            .Draw(new HorizontalDockLayoutBuilder(v.Size.Width, true)
                .Draw(commandInfoText).Alignment(Alignment.TopOrLeft)
                .Draw(wfbotTextCommand).Alignment(Alignment.DownOrRight)
                .Build().ApplyBackground(bottomBarColor)
                ).Build();
    }
}


public sealed class HorizontalLayoutBuilder : LayoutBuilder
{
    protected override ComplexDrawingCommand Layout { get; } = new HorizontalLayout();
}

public sealed class VerticalLayoutBuilder : LayoutBuilder
{
    protected override ComplexDrawingCommand Layout { get; } = new VerticalLayout();
}

public sealed class HorizontalDockLayoutBuilder : LayoutBuilder
{
    int _minWidth;
    bool _forceWidth;

    public HorizontalDockLayoutBuilder(int? minWidth = null, bool forceWidth = false)
    {
        _minWidth = minWidth ?? -1;
        _forceWidth = forceWidth;
        Layout = new HorizontalDockLayout(_minWidth, _forceWidth);
    }

    protected override ComplexDrawingCommand Layout { get; }
}




public abstract class LayoutBuilder
{
    protected IDrawingCommand? lastCommand = null;
    protected abstract ComplexDrawingCommand Layout { get; }
    public TextOptions Options { get; set; }
    Alignment defaultAlignment = Koharu.Alignment.TopOrLeft;
    public LayoutBuilder DefaultAlignment(Alignment alignment)
    {
        defaultAlignment = alignment;
        return this;
    }
    public LayoutBuilder Alignment(Alignment alignment)
    {
        Layout.Draw(lastCommand!, alignment);
        lastCommand = null;
        return this;
    }

    public LayoutBuilder Draw(IDrawingCommand command)
    {
        if (lastCommand != null)
        {
            Layout.Draw(lastCommand, defaultAlignment);
        }
        lastCommand = command;
        return this;
    }

    public LayoutBuilder DrawRange(params IDrawingCommand[] commands)
    {
        foreach (var drawingCommand in commands)
        {
            Draw(drawingCommand);
        }
        return this;
    }

    public ComplexDrawingCommand Build()
    {
        if (lastCommand != null)
        {
            Layout.Draw(lastCommand, defaultAlignment);
        }

        return Layout;
    }

    public ComplexDrawingCommand UnsafeGetLayout() => Layout;

    // public static implicit operator IDrawingCommand(LayoutBuilder builder)
    // {
    //     return builder.Build();
    // }
}

public static class LayoutExtensions
{
    
    public static LayoutBuilder Margin(this LayoutBuilder builder, int xy)
    {
        builder.Draw(new Margin(new Size(xy, xy)));
        return builder;
    }

    public static LayoutBuilder Margin10(this LayoutBuilder builder) => Margin(builder, 10);
    public static LayoutBuilder Margin100(this LayoutBuilder builder) => Margin(builder, 100);
    public static LayoutBuilder Margin20(this LayoutBuilder builder) => Margin(builder, 20);

    public static LayoutBuilder MarginX(this LayoutBuilder builder, int x)
    {
        builder.Draw(new Margin(new Size(x, 0)));
        return builder;
    }
    public static LayoutBuilder MarginY(this LayoutBuilder builder, int y)
    {
        builder.Draw(new Margin(new Size(0, y)));
        return builder;
    }

    public static LayoutBuilder BackgroundColor(this LayoutBuilder builder, Color color)
    {
        builder.Draw(new FillCommand(builder.UnsafeGetLayout(), color));
        return builder;
    }

    public static LayoutBuilder Text(this LayoutBuilder builder, string text, TextOptions? options = null)
    {
        builder.Draw(new TextCommand(text, options ?? builder.Options));
        return builder;
    }

    public static LayoutBuilder Image(this LayoutBuilder builder, SKBitmap image)
    {
        builder.Draw(new ImageCommand(image));
        return builder;
    }

    public static LayoutBuilder Rect(this LayoutBuilder builder, Size size, Color color)
    {
        builder.Draw(new FillCommandCore(size, color));
        return builder;
    }

    public static LayoutBuilder ImageResource(this LayoutBuilder builder, string res)
    {
        builder.Draw(Painter<object>.GetResource(res).AsCommand());
        return builder;
    }

    public static IDrawingCommand ApplyRoundedCorner(this IDrawingCommand command, Color color, float curvature,
        int margin = 0)
    {
        return new RoundedRectCommand(command, color, curvature, margin);
    }

    public static IDrawingCommand ApplyBackground(this IDrawingCommand command, Color color)
    {
        return new FillCommand(command, color);
    }

    public static IDrawingCommand ApplyMargin(this IDrawingCommand command, int margin)
    {
        return MarginCommand.Of(command, margin);
    }
}

public sealed class Margin : IDrawingContent
{
    public Size Size { get; }

    public Margin(Size size) => Size = size;

    // public static Margin Of(int xy) => new Margin(Size.Of(xy, xy));
    // public static Margin Of(int x, int y) => new Margin(Size.Of(x, y));
    // public static Margin OfX(int x) => new Margin(Size.Of(x, 0));
    // public static Margin OfY(int y) => new Margin(Size.Of(0, y));

    public void DrawCore(SKCanvas canvas, Point position) { }

}


public interface IDrawingCore
{
    void DrawCore(SKCanvas canvas, Point position);
}

public interface IDrawingContent : IDrawingCommand, IDrawingCore
{

}

public interface IDrawingCommand
{
    Size Size { get; }
    virtual DrawingPriority DrawingPriority => DrawingPriority.Foreground;
}

public record struct DrawingCommandWithAlignment(IDrawingCommand DrawingCommand, Alignment Alignment);
public record struct DrawingContentWithPosition(IDrawingContent DrawingCommand, Point Position);


public abstract class ComplexDrawingCommand : IDrawingCommand
{
    protected List<DrawingCommandWithAlignment> DrawingCommands { get; } = new ();
    protected int version = 0;

    public void Draw(IDrawingCommand command, Alignment alignment = Alignment.TopOrLeft)
    {
        DrawingCommands.Add(new(command, alignment));
        version++;
    }
    
    public abstract Size Size { get; }
    public abstract void AcquireAllDrawingCommands(List<DrawingContentWithPosition> commands, Point offset);
    public virtual DrawingPriority DrawingPriority => DrawingPriority.Foreground;

    protected static void ApplyCommand(List<DrawingContentWithPosition> commands, IDrawingCommand drawingCommand, Point position)
    {
        if (drawingCommand is ComplexDrawingCommand layout)
        {
            layout.AcquireAllDrawingCommands(commands, position);
        }
        else if (drawingCommand is IDrawingContent content and not Margin)
        {
            commands.Add(new DrawingContentWithPosition(content, position));
        }
    }
}

public sealed class DrawingCommandWrapper : ComplexDrawingCommand
{
    IDrawingCommand _command;

    public DrawingCommandWrapper(IDrawingCommand command)
    {
        _command = command;
        Size = _command.Size;
    }

    public override Size Size { get; }
    public override void AcquireAllDrawingCommands(List<DrawingContentWithPosition> commands, Point offset)
    {
        ApplyCommand(commands, _command, offset);
    }
}




public sealed class HorizontalDockLayout : ComplexDrawingCommand
{
    readonly int _minWidth;
    Size lastSize;
    int lastVersion = -1;
    bool _forceWidth;

    public HorizontalDockLayout(int minWidth = -1, bool forceWidth = false)
    {
        _minWidth = minWidth;
        _forceWidth = forceWidth;
    }

    public override Size Size
    {
        get
        {
            if (lastVersion != version)
            {
                lastSize = Size.Of(
                    _forceWidth ? _minWidth : Math.Max(_minWidth, DrawingCommands.Select(x => x.DrawingCommand.Size.Width).Max()),
                    DrawingCommands.Select(x => x.DrawingCommand.Size.Height).Max());
            }
            Debug.Assert(_minWidth != 0);
            return lastSize;
        }
    }

    public override void AcquireAllDrawingCommands(List<DrawingContentWithPosition> commands, Point offset)
    {
        var size = Size;
        var globalY = offset.Y;
        var maxWidth = size.Width;
        foreach (var (drawingCommand, alignment) in DrawingCommands)
        {
            var xOrigin = offset.X;
            var x = alignment switch
            {
                Alignment.TopOrLeft => xOrigin,
                Alignment.Center => xOrigin + (maxWidth - drawingCommand.Size.Width) / 2,
                Alignment.DownOrRight => xOrigin + (maxWidth - drawingCommand.Size.Width),
                _ => throw new ArgumentOutOfRangeException()
            };
            var position = Point.Of(x, globalY);
            ApplyCommand(commands, drawingCommand, position);
        }
    }

}


public sealed class VerticalLayout : ComplexDrawingCommand
{
    Size lastSize;
    int lastVersion = -1;

    public override Size Size
    {
        get
        {
            if (lastVersion != version)
            {
                lastVersion = version;
                var maxWidth = 0;
                var height = 0;
                foreach (var (drawingCommand, alignment) in DrawingCommands)
                {
                    height += drawingCommand.Size.Height;
                    var width = drawingCommand.Size.Width;
                    maxWidth = width > maxWidth ? width : maxWidth;
                }
                lastSize = Size.Of(maxWidth, height);
            }

            return lastSize;
        }
    }

    public override void AcquireAllDrawingCommands(List<DrawingContentWithPosition> commands, Point offset)
    {
        var size = Size;
        var globalY = offset.Y;
        var maxWidth = size.Width;
        foreach (var (drawingCommand, alignment) in DrawingCommands)
        {
            var xOrigin = offset.X;
            var x = alignment switch
            {
                Alignment.TopOrLeft => xOrigin,
                Alignment.Center => xOrigin + (maxWidth - drawingCommand.Size.Width) / 2,
                Alignment.DownOrRight => xOrigin + (maxWidth - drawingCommand.Size.Width),
                _ => throw new ArgumentOutOfRangeException()
            };
            var position = Point.Of(x, globalY);
            ApplyCommand(commands, drawingCommand, position);

            globalY += drawingCommand.Size.Height;
        }
    }
}


public sealed class HorizontalLayout : ComplexDrawingCommand
{
    Size lastSize;
    int lastVersion = -1;

    public override Size Size
    {
        get
        {
            if (lastVersion != version)
            {
                lastVersion = version;
                var maxHeight = 0;
                var width = 0;
                foreach (var (drawingCommand, alignment) in DrawingCommands)
                {
                    width += drawingCommand.Size.Width;
                    var height = drawingCommand.Size.Height;
                    maxHeight = height > maxHeight ? height : maxHeight;
                }
                lastSize = Size.Of(width, maxHeight);
            }

            return lastSize;
        }
    }

    public override void AcquireAllDrawingCommands(List<DrawingContentWithPosition> commands, Point offset)
    {
        var size = Size;
        var globalX = offset.X;
        var maxHeight = size.Height;
        foreach (var (drawingCommand, alignment) in DrawingCommands)
        {
            var yOrigin = offset.Y;
            var y = alignment switch
            {
                Alignment.TopOrLeft => yOrigin,
                Alignment.Center => yOrigin + (maxHeight - drawingCommand.Size.Height) / 2,
                Alignment.DownOrRight => yOrigin + (maxHeight - drawingCommand.Size.Height),
                _ => throw new ArgumentOutOfRangeException()
            };
            var position = Point.Of(globalX, y);
            ApplyCommand(commands, drawingCommand, position);

            globalX += drawingCommand.Size.Width;
        }
    }


}

public enum Alignment
{
    TopOrLeft, Center, DownOrRight
}

public enum DrawingPriority
{
    Background, Foreground
}

public record struct Size(int Width, int Height)
{
    public static Size Of(int x, int y) => new (x, y);
    public static Size Of(double x, int y) => new ((int)x, y);
    public static Size Of(double x, double y) => new ((int)x, (int)y);
    public static Size Of(int x, double y) => new ((int)x, (int)y);
}

public record struct Point(int X, int Y)
{
    public static Point Of(int x, int y) => new Point(x, y);
    public static implicit operator SKPoint(Point p) => new SKPoint(p.X, p.Y);
}



public static class SbCSharpUtils
{
    public static SKColor ToSkColor(this Color color) => new SKColor(color.R, color.G, color.B, color.A);
    public static ImageCommand AsCommand(this SKBitmap bitmap) => new ImageCommand(bitmap);

    public static void AcquireAllDrawingCommands(this IDrawingCommand content, List<DrawingContentWithPosition> list, Point? pos = null) =>
        new DrawingCommandWrapper(content).AcquireAllDrawingCommands(list, pos ?? Point.Of(0, 0));

    public static SKBitmap Resize(this SKBitmap bitmap, int resizedWidth, int resizedHeight)
    {
        using var surface2 = SKSurface.Create(new SKImageInfo(resizedWidth, resizedHeight, SKColorType.Rgba8888));
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.FilterQuality = SKFilterQuality.High;

        surface2.Canvas.DrawBitmap(bitmap, new SKRectI(0, 0, resizedWidth, resizedHeight),
            paint);
        surface2.Canvas.Flush();

        using var newImg = surface2.Snapshot();
        return SKBitmap.FromImage(newImg);
    }
}