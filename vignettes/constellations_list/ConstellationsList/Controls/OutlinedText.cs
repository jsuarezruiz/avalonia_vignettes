using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace ConstellationsList.Controls;

/// <summary>
/// Draws a line of text either filled or as an outline.
/// </summary>
/// <remarks>
/// The original sets a <c>Paint</c> with <c>PaintingStyle.stroke</c> on its text style, which
/// Flutter honours directly. Avalonia's <see cref="TextBlock"/> only fills, so the text is turned
/// into geometry and either filled or stroked with a pen, which is what the paint style amounts to.
/// </remarks>
public sealed class OutlinedText : Control
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<OutlinedText, string?>(nameof(Text));

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextBlock.FontFamilyProperty.AddOwner<OutlinedText>();

    public static readonly StyledProperty<double> FontSizeProperty =
        TextBlock.FontSizeProperty.AddOwner<OutlinedText>();

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextBlock.ForegroundProperty.AddOwner<OutlinedText>();

    public static readonly StyledProperty<bool> IsOutlinedProperty =
        AvaloniaProperty.Register<OutlinedText, bool>(nameof(IsOutlined));

    public static readonly StyledProperty<double> LineHeightProperty =
        AvaloniaProperty.Register<OutlinedText, double>(nameof(LineHeight), double.NaN);

    private const double StrokeWidth = 1d;

    static OutlinedText()
    {
        AffectsRender<OutlinedText>(ForegroundProperty, IsOutlinedProperty);
        AffectsMeasure<OutlinedText>(TextProperty, FontFamilyProperty, FontSizeProperty, LineHeightProperty);
    }

    /// <summary>
    /// Gets or sets the text to draw.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the font.
    /// </summary>
    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the text.
    /// </summary>
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush the text is drawn in.
    /// </summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the text is stroked rather than filled.
    /// </summary>
    public bool IsOutlined
    {
        get => GetValue(IsOutlinedProperty);
        set => SetValue(IsOutlinedProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the line box; the font's own when not set.
    /// </summary>
    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Build() is not { } formatted || Foreground is not { } brush)
        {
            return;
        }

        // The line box may be taller than the glyphs, so the text is centred in whatever is left.
        var top = double.IsNaN(LineHeight) ? 0d : (LineHeight - formatted.Height) / 2d;

        // The glyphs only change with the text, so the geometry is kept from draw to draw.
        if (_geometry is null || _geometryTop != top)
        {
            _geometry = formatted.BuildGeometry(new Point(0d, top));
            _geometryTop = top;
        }

        if (_geometry is null)
        {
            return;
        }

        if (IsOutlined)
        {
            _stroke ??= new ImmutablePen(brush.ToImmutable(), StrokeWidth);

            context.DrawGeometry(null, _stroke, _geometry);
        }
        else
        {
            context.DrawGeometry(brush, null, _geometry);
        }
    }

    private FormattedText? _formatted;
    private Geometry? _geometry;
    private ImmutablePen? _stroke;
    private double _geometryTop = double.NaN;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // Anything that reshapes or recolours the glyphs drops what was kept.
        if (change.Property == TextProperty || change.Property == FontFamilyProperty ||
            change.Property == FontSizeProperty || change.Property == ForegroundProperty)
        {
            _formatted = null;
            _geometry = null;
            _stroke = null;
        }
        else if (change.Property == LineHeightProperty)
        {
            _geometry = null;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Build() is not { } formatted)
        {
            return default;
        }

        return new Size(formatted.WidthIncludingTrailingWhitespace, double.IsNaN(LineHeight) ? formatted.Height : LineHeight);
    }

    private FormattedText? Build()
    {
        if (Text is not { Length: > 0 } text || FontSize <= 0d)
        {
            return null;
        }

        return _formatted ??= new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily),
            FontSize,
            Foreground);
    }
}
