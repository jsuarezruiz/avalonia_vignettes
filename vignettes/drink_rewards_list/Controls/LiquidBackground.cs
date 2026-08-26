using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DrinkRewardsList.Effects;

namespace DrinkRewardsList.Controls;

/// <summary>
/// Draws the drink filling the card: two sloshing surfaces, one behind the other. Port of
/// <c>LiquidPainter</c>.
/// </summary>
/// <remarks>
/// Two surfaces rather than one is what sells the depth: they are randomised separately, drawn in
/// different browns and offset a few pixels, so the near one reads as liquid in front of the far
/// one rather than as a single flat shape.
/// <para>
/// Each surface is painted wider than the card and shifted sideways, so the curve ends never show.
/// </para>
/// </remarks>
public sealed class LiquidBackground : Control
{
    public static readonly StyledProperty<double> LevelProperty =
        AvaloniaProperty.Register<LiquidBackground, double>(nameof(Level));

    private const double WaveHeight = 100d;

    private const double NearSurfaceOffset = 5d;

    private static readonly IBrush FarBrush =
        new SolidColorBrush(Color.FromArgb(0x66, 0xC4, 0x8D, 0x3B));

    private static readonly IBrush NearBrush =
        new SolidColorBrush(Color.FromArgb(0x66, 0x9D, 0x7B, 0x32));

    static LiquidBackground() => AffectsRender<LiquidBackground>(LevelProperty);

    /// <summary>
    /// Gets the far surface, the one drawn behind.
    /// </summary>
    public LiquidSimulation FarSurface { get; } = new();

    /// <summary>
    /// Gets the near surface, the one drawn in front.
    /// </summary>
    public LiquidSimulation NearSurface { get; } = new();

    /// <summary>
    /// Gets or sets how far down the liquid sits, in pixels from where a full card would put it.
    /// </summary>
    public double Level
    {
        get => GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    /// <summary>
    /// Rolls fresh surfaces, one starting above the level and one below.
    /// </summary>
    public void Restart()
    {
        FarSurface.Start(flipY: true);
        NearSurface.Start(flipY: false);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        using (context.PushTransform(Matrix.CreateTranslation(0d, Level)))
        {
            DrawSurface(context, FarSurface, 0d, FarBrush);
            DrawSurface(context, NearSurface, NearSurfaceOffset, NearBrush);
        }
    }

    private void DrawSurface(DrawingContext context, LiquidSimulation surface, double offsetY, IBrush brush)
    {
        var size = Bounds.Size;

        if (size.Width <= 0d || surface.ControlPoints.Count == 0)
        {
            return;
        }

        var geometry = new StreamGeometry();

        using (var figure = geometry.Open())
        {
            // The body of the liquid is a box reaching well past both edges of the card; only its
            // top edge is drawn as curves.
            figure.BeginFigure(new Point(size.Width * 1.25d, 0d), isFilled: true);
            figure.LineTo(new Point(size.Width * 1.25d, size.Height));
            figure.LineTo(new Point(-size.Width * 0.25d, size.Height));
            figure.LineTo(new Point(-size.Width * 0.25d, 0d));

            for (var i = 0; i < LiquidSimulation.CurveCount; i++)
            {
                figure.QuadraticBezierTo(
                    Scale(surface.ControlPoints[i], size),
                    Scale(surface.EndPoints[i + 1], size));
            }

            figure.EndFigure(isClosed: true);
        }

        // The original shifts the canvas and then scales it, so the shift is scaled too. Avalonia
        // multiplies row vectors, which puts the two the other way round from Flutter's canvas calls.
        var transform = Matrix.CreateTranslation(surface.HorizontalOffset * size.Width, offsetY)
            * Matrix.CreateScale(surface.HorizontalScale, 1d);

        using (context.PushTransform(transform))
        {
            context.DrawGeometry(brush, null, geometry);
        }
    }

    private static Point Scale(Point point, Size size) => new(point.X * size.Width, WaveHeight * point.Y);
}
