using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace FluidNavBar.Controls;

/// <summary>
/// An icon drawn as a stroke that fills in from one end. Port of <c>fluid_icon.dart</c>.
/// </summary>
/// <remarks>
/// The whole outline is drawn once in grey, then the same outline is drawn again in black, cut
/// short at <see cref="FillAmount"/>. There is no masking or clipping involved: the black line
/// simply grows along the path, which is why it reads as ink running into the shape.
/// </remarks>
public sealed class FluidIcon : Control
{
    public static readonly StyledProperty<FluidIconData?> DataProperty =
        AvaloniaProperty.Register<FluidIcon, FluidIconData?>(nameof(Data));

    public static readonly StyledProperty<double> FillAmountProperty =
        AvaloniaProperty.Register<FluidIcon, double>(nameof(FillAmount));

    public static readonly StyledProperty<double> ScaleYProperty =
        AvaloniaProperty.Register<FluidIcon, double>(nameof(ScaleY), 1d);

    public static readonly StyledProperty<IBrush?> IdleBrushProperty =
        AvaloniaProperty.Register<FluidIcon, IBrush?>(nameof(IdleBrush), Brushes.Gray);

    public static readonly StyledProperty<IBrush?> ActiveBrushProperty =
        AvaloniaProperty.Register<FluidIcon, IBrush?>(nameof(ActiveBrush), Brushes.Black);

    private const double DataScale = 0.9d;

    private const double StrokeWidth = 2.4d;

    static FluidIcon() =>
        AffectsRender<FluidIcon>(DataProperty, FillAmountProperty, ScaleYProperty, IdleBrushProperty, ActiveBrushProperty);

    /// <summary>
    /// Gets or sets the icon to draw.
    /// </summary>
    public FluidIconData? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>
    /// Gets or sets how much of the outline is inked in, from 0 to 1.
    /// </summary>
    public double FillAmount
    {
        get => GetValue(FillAmountProperty);
        set => SetValue(FillAmountProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical squash applied about the icon's middle.
    /// </summary>
    public double ScaleY
    {
        get => GetValue(ScaleYProperty);
        set => SetValue(ScaleYProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush the un-inked outline is drawn in.
    /// </summary>
    public IBrush? IdleBrush
    {
        get => GetValue(IdleBrushProperty);
        set => SetValue(IdleBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush the inked part is drawn in.
    /// </summary>
    public IBrush? ActiveBrush
    {
        get => GetValue(ActiveBrushProperty);
        set => SetValue(ActiveBrushProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Data is not { } data)
        {
            return;
        }

        var size = Bounds.Size;

        // Squashed about the middle of the box, then centred and taken down to its drawn size. The
        // stroke rides along with the transform, so a squashed icon has a squashed line too.
        var transform = Matrix.CreateScale(DataScale, DataScale)
                        * Matrix.CreateTranslation(size.Width / 2d, 0d)
                        * Matrix.CreateScale(1d, ScaleY)
                        * Matrix.CreateTranslation(0d, size.Height / 2d);

        using var _ = context.PushTransform(transform);

        context.DrawGeometry(null, Pen(IdleBrush), data.Build(1d));

        if (FillAmount > 0d)
        {
            context.DrawGeometry(null, Pen(ActiveBrush), data.Build(FillAmount));
        }
    }

    private static Pen Pen(IBrush? brush) =>
        new(brush, StrokeWidth, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
}
