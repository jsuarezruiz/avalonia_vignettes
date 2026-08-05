using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaVignettes.Shared.Controls;

/// <summary>
/// The grey-on-white skeletons the vignettes use as stand-in content. Port of the
/// <c>placeholder_*</c> widgets in the shared Flutter package.
/// </summary>
/// <remarks>
/// The bar widths are randomised so a list of them does not read as a repeating pattern. Flutter
/// re-rolls them on every build; these roll once per instance, which looks the same and does not
/// make the content twitch when something above it animates.
/// </remarks>
public abstract class PlaceholderBase : Control
{
    /// <summary>Defines the <see cref="Foreground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<PlaceholderBase, IBrush?>(
            nameof(Foreground), new SolidColorBrush(Color.FromRgb(0xF2, 0xF2, 0xF2)));

    /// <summary>Defines the <see cref="Background"/> property.</summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<PlaceholderBase, IBrush?>(nameof(Background), Brushes.White);

    /// <summary>Defines the <see cref="CornerRadius"/> property.</summary>
    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<PlaceholderBase, double>(nameof(CornerRadius), 4d);

    static PlaceholderBase() =>
        AffectsRender<PlaceholderBase>(ForegroundProperty, BackgroundProperty, CornerRadiusProperty);

    /// <summary>Gets or sets the brush the skeleton bars are drawn in.</summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>Gets or sets the card's fill.</summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>Gets or sets the card's corner radius.</summary>
    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>Draws the card behind the skeleton.</summary>
    protected void DrawCard(DrawingContext context)
    {
        if (Background is { } background)
        {
            context.DrawRectangle(background, null, new RoundedRect(new Rect(Bounds.Size), CornerRadius));
        }
    }

    /// <summary>Draws one skeleton bar.</summary>
    protected void DrawBar(DrawingContext context, double x, double y, double width, double height, double radius = 0d)
    {
        if (Foreground is not { } brush || width <= 0d)
        {
            return;
        }

        var rect = new Rect(x, y, width, height);

        context.DrawRectangle(brush, null, radius > 0d ? new RoundedRect(rect, radius) : new RoundedRect(rect));
    }
}

/// <summary>A tall card: an avatar, a title and four lines. Port of <c>PlaceholderCardTall</c>.</summary>
public sealed class PlaceholderCardTall : PlaceholderBase
{
    private const double Padding = 20d;
    private const double LineHeight = 14d;
    private const double AvatarSize = 45d;

    /// <summary>The tops of the four lines, measured from the card's padding.</summary>
    private static readonly double[] LineTops = [60d, 85d, 110d, 135d];

    private readonly double _titleWidth;
    private readonly double[] _lineInsets;

    /// <summary>Initializes a new instance of the <see cref="PlaceholderCardTall"/> class.</summary>
    public PlaceholderCardTall()
    {
        var random = Random.Shared;

        _titleWidth = 100d + random.Next(100);
        _lineInsets =
        [
            10d + random.Next(60),
            10d + random.Next(60),
            10d + random.Next(60),
            60d + random.Next(60),
        ];
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        DrawCard(context);

        var inner = Bounds.Width - (Padding * 2d);

        // The avatar and title sit at the top; the lines below are spaced 25 apart.
        DrawBar(context, Padding, Padding, AvatarSize, AvatarSize, AvatarSize / 2d);
        DrawBar(context, Padding + 65d, Padding + 10d, _titleWidth, LineHeight * 1.2d);

        for (var i = 0; i < _lineInsets.Length; i++)
        {
            DrawBar(context, Padding, Padding + LineTops[i], inner - _lineInsets[i], LineHeight);
        }
    }
}

/// <summary>A short card: two lines and a small square. Port of <c>PlaceholderCardShort</c>.</summary>
public sealed class PlaceholderCardShort : PlaceholderBase
{
    private const double Padding = 26d;
    private const double LineHeight = 16d;

    private readonly double _firstInset;
    private readonly double _secondInset;

    /// <summary>Initializes a new instance of the <see cref="PlaceholderCardShort"/> class.</summary>
    public PlaceholderCardShort()
    {
        var random = Random.Shared;

        _firstInset = 60d + random.Next(60);
        _secondInset = 80d + random.Next(120);

        Height = 90d;
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        DrawCard(context);

        var inner = Bounds.Width - (Padding * 2d);
        var innerHeight = Bounds.Height - (Padding * 2d);

        DrawBar(context, Padding, Padding, inner - _firstInset, LineHeight);
        DrawBar(context, Padding, Padding + LineHeight + 6d, inner - _secondInset, LineHeight);

        // The square is pinned to the right and centred on the card.
        DrawBar(context, Bounds.Width - Padding - 8d - 32d, Padding + ((innerHeight - 32d) / 2d), 32d, 32d, 4d);
    }
}

/// <summary>A framed mountain scene. Port of <c>PlaceholderImage</c>.</summary>
public sealed class PlaceholderImage : PlaceholderBase
{
    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        DrawCard(context);

        if (Foreground is not { } brush)
        {
            return;
        }

        // The scene is drawn at 60% of the card's width, from a baseline below the middle.
        var width = Bounds.Width * 0.6d;
        var origin = new Point(
            (Bounds.Width / 2d) - (width / 2d),
            (Bounds.Height / 2d) + (width * 0.7d / 2d));

        var geometry = new StreamGeometry();

        using (var figure = geometry.Open())
        {
            figure.BeginFigure(origin, isFilled: true);
            figure.LineTo(origin + new Vector(width * 0.4d, -width * 0.66d));
            figure.LineTo(origin + new Vector(width * 0.63d, -width * 0.29d));
            figure.LineTo(origin + new Vector(width * 0.74d, -width * 0.44d));
            figure.LineTo(origin + new Vector(width, 0d));
            figure.LineTo(origin);
            figure.EndFigure(isClosed: true);
        }

        // Stroked and filled both, which is what rounds off the peaks.
        var pen = new Pen(brush, 4d, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

        context.DrawGeometry(brush, pen, geometry);
        context.DrawEllipse(
            brush,
            null,
            origin + new Vector(width * 0.9d, -width * 0.7d),
            width * 0.1d,
            width * 0.1d);
    }
}

/// <summary>An image over a block of text. Port of <c>PlaceholderImageWithText</c>.</summary>
public sealed class PlaceholderImageWithText : PlaceholderBase
{
    /// <summary>The text block below the image is a fixed 94 tall.</summary>
    private const double TextBlockHeight = 94d;

    /// <summary>Initializes a new instance of the <see cref="PlaceholderImageWithText"/> class.</summary>
    public PlaceholderImageWithText() => CornerRadius = 0d;

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        DrawCard(context);

        var width = Bounds.Width;
        var imageHeight = Math.Max(0d, Bounds.Height - TextBlockHeight);

        DrawBar(context, 0d, 0d, width, imageHeight);

        var top = imageHeight;

        DrawBar(context, 10d, top + 12d, width - 10d - 70d, 16d);
        DrawBar(context, 10d, top + 40d, width - 10d - 30d, 10d);
        DrawBar(context, 10d, top + 56d, width - 20d, 10d);
        DrawBar(context, 10d, top + 72d, width - 10d - 60d, 10d);
    }
}
