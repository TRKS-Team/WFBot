using System.Diagnostics;
using System.Drawing;
using SkiaSharp;
using Topten.RichTextKit;
using static System.Net.Mime.MediaTypeNames;

namespace WFBot.Koharu;



public sealed class MarginCommand : ComplexDrawingCommand
{
    int _top;
    int _left;
    int _down;
    int _right;
    IDrawingCommand _drawingCommand;

    public MarginCommand(IDrawingCommand command, int top, int left, int down, int right)
    {
        _drawingCommand = command;
        _top = top;
        _left = left;
        _down = down;
        _right = right;
    }

    public override Size Size
    {
        get
        {
            var size = _drawingCommand.Size;
            size.Height += _top + _down;
            size.Width += _left + _right;
            return size;
        }
    }

    public static MarginCommand Of(IDrawingCommand command, int margin) =>
        new MarginCommand(command, margin, margin, margin, margin);
    

    public override void AcquireAllDrawingCommands(List<DrawingContentWithPosition> commands, Point offset)
    {
        offset.X += _left;
        offset.Y += _top;
        ApplyCommand(commands, _drawingCommand, offset);
    }
}

public sealed class RoundedRectCommand : ComplexDrawingCommand
{
    IDrawingCommand _drawingCommand;
    float _curvature;
    Color _color;
    int _margin;

    public RoundedRectCommand(IDrawingCommand command, Color color, float curvature, int margin = 0)
    {
        _color = color;
        _drawingCommand = command;
        _margin = margin;
        _curvature = curvature;
    }

    public override Size Size
    {
        get
        {
            var size = _drawingCommand.Size;
            size.Height += 2 * _margin;
            size.Width += 2 * _margin;
            return size;
        }
    }

    public override DrawingPriority DrawingPriority => DrawingPriority.Background;
    public override void AcquireAllDrawingCommands(List<DrawingContentWithPosition> commands, Point offset)
    {
        ApplyCommand(commands, new RoundedRectCommandCore(_drawingCommand.Size, _color, _curvature, _margin), offset);
        offset.X += _margin;
        offset.Y += _margin;
        ApplyCommand(commands, _drawingCommand, offset);
    }
}

public sealed class RoundedRectCommandCore : IDrawingContent
{
    float _curvature;

    public RoundedRectCommandCore(Size size, Color color, float curvature, int margin)
    {
        size.Width += margin * 2;
        size.Height += margin * 2;
        Size = size;
        Color = color;
        _curvature = curvature;
    }

    public Color Color { get; }
    public Size Size { get; }
    public void DrawCore(SKCanvas canvas, Point position)
    {
        canvas.DrawRoundRect(position.X, position.Y, Size.Width, Size.Height, _curvature, _curvature, new SKPaint() { Color = Color.ToSkColor() });
    }
}

public sealed class FillCommand : ComplexDrawingCommand
{
    IDrawingCommand _drawingCommand;
    Color _color;

    public FillCommand(IDrawingCommand command, Color color)
    {
        _color = color;
        _drawingCommand = command;
    }

    public override Size Size => _drawingCommand.Size;
    public override DrawingPriority DrawingPriority => DrawingPriority.Background;
    public override void AcquireAllDrawingCommands(List<DrawingContentWithPosition> commands, Point offset)
    {
        ApplyCommand(commands, new FillCommandCore(_drawingCommand.Size, _color), offset);
        ApplyCommand(commands, _drawingCommand, offset);
    }
}
public sealed class FillCommandCore : IDrawingContent
{
    public FillCommandCore(Size size, Color color)
    {
        Size = size;
        Color = color;
    }

    public Color Color { get; }
    public Size Size { get; }
    public void DrawCore(SKCanvas canvas, Point position)
    {
        canvas.DrawRect(position.X, position.Y, Size.Width, Size.Height, new SKPaint() { Color = Color.ToSkColor() });
    }
}

class MyFontMapper : FontMapper
{
    public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants)
    {
        return SKTypeface.FromFile("WFConfigs/font.ttf");
    }
}

public sealed class TextCommand : IDrawingContent
{
    public TextCommand(string text, TextOptions options)
    {
        textBlock = new TextBlock();
        textBlock.AddText(text, new Style()
        {
            TextColor = options.Color.ToSkColor(),
            FontSize = options.Size,
            FontWeight = options.Bold ? 400 : 700,
        });
        
        textBlock.MaxWidth = options.MaxWidth == -1 ? null : options.MaxWidth;
        textBlock.FontMapper = new MyFontMapper();
        size = new Size((int)textBlock.MeasuredWidth, (int)textBlock.MeasuredHeight);
        //Debug.Assert(size.Height != 0);
    }

    //static SKCanvas measurerCanvas = SKSurface.Create(new SKImageInfo(800, 700, SKColorType.Rgba8888)).Canvas;


    Size size;
    public Size Size => size;
    
    TextBlock textBlock;
    
    public void DrawCore(SKCanvas canvas, Point position)
    {
        textBlock.Paint(canvas, position, new TextPaintOptions(){Edging = SKFontEdging.Antialias});
    }
}

public class RichTextBuilder
{
    TextBlock textBlock = new TextBlock();
    TextOptions lastTextOptions;
    string? lastText = null;

    void Commit()
    {
        if (lastText != null)
        {
            textBlock.AddText(lastText, new Style()
            {
                TextColor = lastTextOptions.Color.ToSkColor(),
                FontSize = lastTextOptions.Size,
                FontWeight = lastTextOptions.Bold ? 400 : 700,
            });
            lastTextOptions = Painter<object>.textOptions;
        }
    }
    private RichTextBuilder() {}

    public static RichTextBuilder Create(int maxWidth = 1000)
    {
        var b = new RichTextBuilder();
        b.lastTextOptions = Painter<object>.textOptions;
        b.textBlock.MaxWidth = maxWidth;
        return b;
    }

    public RichTextBuilder Text(string text)
    {
        Commit();
        lastText = text;
        return this;
    }

    public RichTextBuilder Bold()
    {
        lastTextOptions.Bold = true;
        return this;
    }

    public RichTextBuilder Color(Color color)
    {
        lastTextOptions.Color = color;
        return this;
    }

    public RichTextBuilder Size(int size)
    {
        lastTextOptions.Size = size;
        return this;
    }
    
    public IDrawingCommand Build()
    {
        Commit();
        return new RichTextCommand(textBlock);
    }
}

public class RichTextCommand : IDrawingCommand
{
    TextBlock textBlock;

    public RichTextCommand(TextBlock textBlock)
    {
        this.textBlock = textBlock;

        textBlock.FontMapper = new MyFontMapper();
        size = new Size((int)textBlock.MeasuredWidth, (int)textBlock.MeasuredHeight);
    }
    Size size;
    public Size Size => size;

    public void DrawCore(SKCanvas canvas, Point position)
    {
        textBlock.Paint(canvas, position, new TextPaintOptions() { Edging = SKFontEdging.Antialias });
    }
}

public record struct TextOptions(int Size, Color Color, bool Bold, int MaxWidth);

public sealed class ImageCommand : IDrawingContent
{
    SKBitmap bitmap;

    public ImageCommand(SKBitmap bitmap)
    {
        this.bitmap = bitmap;
    }

    public Size Size => Size.Of(bitmap.Width, bitmap.Height);
    public void DrawCore(SKCanvas canvas, Point position)
    {
        canvas.DrawBitmap(bitmap, position);
    }
    
}