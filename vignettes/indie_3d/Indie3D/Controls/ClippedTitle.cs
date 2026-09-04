using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Indie3D.Models;

namespace Indie3D.Controls;

/// <summary>
/// The artist's name in the top right, wiped into view a line at a time. Port of
/// <c>_buildClippedText</c> and the <c>BlendMask</c> over it.
/// </summary>
/// <remarks>
/// The names knock the artwork out rather than sitting on it, which is a difference blend, and
/// Avalonia blends bitmaps only. So the glyphs become a clip and white arrives through them.
/// </remarks>
public sealed class ClippedTitle : Control
{
    public static readonly StyledProperty<string> TopTitleProperty =
        AvaloniaProperty.Register<ClippedTitle, string>(nameof(TopTitle), string.Empty);

    public static readonly StyledProperty<string> BottomTitleProperty =
        AvaloniaProperty.Register<ClippedTitle, string>(nameof(BottomTitle), string.Empty);

    public static readonly StyledProperty<double> TopProgressProperty =
        AvaloniaProperty.Register<ClippedTitle, double>(nameof(TopProgress), 1d);

    public static readonly StyledProperty<double> BottomProgressProperty =
        AvaloniaProperty.Register<ClippedTitle, double>(nameof(BottomProgress), 1d);

    public static readonly StyledProperty<double> BottomScaleProperty =
        AvaloniaProperty.Register<ClippedTitle, double>(nameof(BottomScale), 1d);

    private const double TopSize = 72d;
    private const double BottomSize = 120d;

    private const double TopLineFactor = 1d;
    private const double BottomLineFactor = 0.9d;

    private const double TopSpacing = 6d;
    private const double BottomSpacing = 8d;

    private const double BottomWipeOffset = -10d;

    private const double NaturalLine = 1.25d;

    private readonly (string Text, double Size, double Spacing, double Y, double Width)[] _cacheKeys = new (string, double, double, double, double)[2];
    private readonly Geometry?[] _cached = new Geometry?[2];

    static ClippedTitle() => AffectsRender<ClippedTitle>(
        TopTitleProperty,
        BottomTitleProperty,
        TopProgressProperty,
        BottomProgressProperty,
        BottomScaleProperty);

    /// <summary>
    /// Gets or sets the artist's first name.
    /// </summary>
    public string TopTitle
    {
        get => GetValue(TopTitleProperty);
        set => SetValue(TopTitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the artist's last name.
    /// </summary>
    public string BottomTitle
    {
        get => GetValue(BottomTitleProperty);
        set => SetValue(BottomTitleProperty, value);
    }

    /// <summary>
    /// Gets or sets how much of the top line is still wiped away, from 1 to 0.
    /// </summary>
    public double TopProgress
    {
        get => GetValue(TopProgressProperty);
        set => SetValue(TopProgressProperty, value);
    }

    /// <summary>
    /// Gets or sets how much of the bottom line is still wiped away, from 1 to 0.
    /// </summary>
    public double BottomProgress
    {
        get => GetValue(BottomProgressProperty);
        set => SetValue(BottomProgressProperty, value);
    }

    /// <summary>
    /// Gets or sets the extra scaling the bottom line carries on some pages.
    /// </summary>
    public double BottomScale
    {
        get => GetValue(BottomScaleProperty);
        set => SetValue(BottomScaleProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Bounds.Width <= 0d)
        {
            return;
        }

        // Landscape gets larger type; the phone the vignette was drawn for gets 0.8.
        var scale = Bounds.Width > Bounds.Height ? 1.15d : 0.8d;

        using var blend = context.PushRenderOptions(
            new RenderOptions { BitmapBlendingMode = BitmapBlendingMode.Difference });

        var top = DrawLine(context, 0, TopTitle, TopSize * scale, TopSpacing, TopLineFactor, 0d, TopProgress, 0d);

        DrawLine(
            context,
            1,
            BottomTitle,
            BottomSize * scale * BottomScale,
            BottomSpacing,
            BottomLineFactor,
            top,
            BottomProgress,
            BottomWipeOffset);
    }

    private double DrawLine(
        DrawingContext context,
        int line,
        string text,
        double size,
        double spacing,
        double lineFactor,
        double y,
        double progress,
        double wipeOffset)
    {
        var unroundedBox = size * lineFactor;
        var box = Math.Round(unroundedBox, MidpointRounding.AwayFromZero);

        if (text.Length == 0)
        {
            return box;
        }

        // The original inherits Material's even leading distribution: split the removed line
        // height equally above/below the baseline, then apply Flutter's pixel-rounded line box.
        var lineOffset = (unroundedBox - size * NaturalLine) / 2d + box - unroundedBox;
        var glyphs = Glyphs(line, text, size, spacing, y + lineOffset);

        using var wipe = context.PushClip(new Rect(
            0d,
            y + (progress * size) + wipeOffset,
            Bounds.Width,
            Math.Max(0d, Bounds.Height - y)));

        using var clip = context.PushGeometryClip(glyphs);

        context.DrawImage(Swatches.White, new Rect(Bounds.Size));

        return box;
    }

    private Geometry Glyphs(int line, string text, double size, double spacing, double y)
    {
        var key = (text, size, spacing, y, Bounds.Width);

        if (_cacheKeys[line] != key || _cached[line] is null)
        {
            _cached[line] = BuildGlyphs(text, size, spacing, y);
            _cacheKeys[line] = key;
        }

        return _cached[line]!;
    }

    // Use the native text shaper so letter spacing and kerning match the original paragraph.
    private Geometry BuildGlyphs(string text, double size, double spacing, double y)
    {
        var typeface = new Typeface(Fonts.Display, weight: FontWeight.Bold);
        using var layout = new TextLayout(text, typeface, size, Brushes.White, letterSpacing: spacing);
        var group = new GeometryGroup { FillRule = FillRule.NonZero };
        var x = Bounds.Width - layout.Width;
        foreach (var run in layout.TextLines[0].TextRuns.OfType<ShapedTextRun>())
        {
            var glyphs = run.GlyphRun.BuildGeometry();
            glyphs.Transform = new TranslateTransform(x, y);
            group.Children.Add(glyphs);
            x += run.Size.Width;
        }

        return group;
    }
}
