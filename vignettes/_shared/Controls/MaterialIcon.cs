using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace AvaloniaVignettes.Shared.Controls;

/// <summary>
/// Draws one of Material's icon outlines at a given size, the way Flutter's <c>Icon</c> does.
/// </summary>
/// <remarks>
/// The outlines are authored on a 24 unit grid and most of them do not fill it: the menu bars, for
/// instance, occupy only the middle 14 units, and the three dots of the overflow icon barely four.
/// A <see cref="Avalonia.Controls.Shapes.Path"/> with a stretch scales the ink to fill its box,
/// which changes both how big the glyph is and where it sits, so icons come out oversized and
/// misaligned against text they are supposed to be centred with. This scales the grid instead of
/// the ink, leaving each glyph the size and position it was drawn at.
/// </remarks>
public sealed class MaterialIcon : Control
{
    /// <summary>Defines the <see cref="Data"/> property.</summary>
    public static readonly StyledProperty<Geometry?> DataProperty =
        AvaloniaProperty.Register<MaterialIcon, Geometry?>(nameof(Data));

    /// <summary>Defines the <see cref="IconSize"/> property.</summary>
    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<MaterialIcon, double>(nameof(IconSize), DesignGrid);

    /// <summary>Defines the <see cref="Foreground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<MaterialIcon>();

    /// <summary>The grid Material's icon outlines are drawn on.</summary>
    private const double DesignGrid = 24d;

    static MaterialIcon()
    {
        AffectsRender<MaterialIcon>(DataProperty, ForegroundProperty, IconSizeProperty);
        AffectsMeasure<MaterialIcon>(IconSizeProperty);
    }

    /// <summary>Gets or sets the outline, as authored on Material's 24 unit grid.</summary>
    public Geometry? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the icon's box, the equivalent of Flutter's <c>Icon.size</c>. The
    /// glyph itself is usually smaller, because it is not drawn edge to edge.
    /// </summary>
    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>Gets or sets the brush the outline is filled with.</summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Data is not { } data || Foreground is not { } brush)
        {
            return;
        }

        var scale = IconSize / DesignGrid;

        using (context.PushTransform(Matrix.CreateScale(scale, scale)))
        {
            context.DrawGeometry(brush, null, data);
        }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(IconSize, IconSize);
}
