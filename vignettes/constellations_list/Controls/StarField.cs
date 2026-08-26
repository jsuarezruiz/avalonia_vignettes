using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaVignettes.Shared.Animation;

namespace ConstellationsList.Controls;

/// <summary>
/// A field of stars flying past the viewer, drawn in perspective. Port of
/// <c>star_field.dart</c> and <c>star_field_painter.dart</c>.
/// </summary>
/// <remarks>
/// Each star holds a position in a boxy 3D space and is divided by its own depth to land on screen,
/// so the near ones sweep outwards fast while the distant ones barely move. Depth also sets how big
/// a star is drawn, which is what turns a flat scatter of dots into something with distance in it.
/// <para>
/// <see cref="Speed"/> is how far the field advances each frame, and everything else is driven from
/// it: the list hands over its scroll velocity, and tapping through to a constellation runs a
/// sequence that pulls the field back before throwing it forward. Negative speeds run the field
/// backwards, which is how the pull-back reads.
/// </para>
/// </remarks>
public sealed class StarField : Control
{
    public static readonly StyledProperty<double> SpeedProperty =
        AvaloniaProperty.Register<StarField, double>(nameof(Speed), 0.2d);

    public static readonly StyledProperty<int> StarCountProperty =
        AvaloniaProperty.Register<StarField, int>(nameof(StarCount), 400);

    private const double MaxZ = 500d;

    private const double MinZ = 1d;

    private const double Spread = 75d;

    private const double GlowingShare = 0.1d;

    private static readonly Color GlowColor = Color.FromRgb(0xD4, 0xA1, 0xFF);

    private readonly Random _random = new();
    private readonly List<Star> _stars = [];

    private FrameTicker? _ticker;
    private Bitmap? _glow;

    static StarField() => AffectsRender<StarField>(SpeedProperty);

    /// <summary>
    /// Gets or sets how far the field advances each frame.
    /// </summary>
    public double Speed
    {
        get => GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    /// <summary>
    /// Gets or sets how many stars there are.
    /// </summary>
    public int StarCount
    {
        get => GetValue(StarCountProperty);
        set => SetValue(StarCountProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var size = Bounds.Size;

        if (size.Width <= 0d || size.Height <= 0d)
        {
            return;
        }

        context.FillRectangle(Brushes.Black, new Rect(size));

        // The field is centred, so a star's own coordinates are measured from the middle of the view.
        using var _ = context.PushTransform(Matrix.CreateTranslation(size.Width / 2d, size.Height / 2d));

        foreach (var star in _stars)
        {
            // Depth both shrinks the star and pushes it towards the centre.
            var scale = 0.1d + (star.Size * (1d - (star.Z / size.Width)));
            var position = new Point(star.X / star.Z * size.Width, star.Y / star.Z * size.Height);

            if (scale <= 0d)
            {
                continue;
            }

            context.DrawEllipse(star.Brush, null, position, scale, scale);

            if (_glow is { } glow && star.IsGlowing)
            {
                // The halo breathes on two out-of-step waves, so no two glows pulse together.
                var time = Environment.TickCount64 / 200d;
                var width = (scale * 6d) + (2d * Math.Sin(time * 0.5d));
                var height = (scale * 6d) + (2d * Math.Cos(time * 0.75d));

                context.DrawImage(
                    glow,
                    new Rect(0d, 0d, glow.Size.Width, glow.Size.Height),
                    new Rect(position.X - (width / 2d), position.Y - (height / 2d), width, height));
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _glow ??= new Bitmap(AssetLoader.Open(new Uri("avares://ConstellationsList/Assets/Images/glow.png")));

        if (_stars.Count == 0)
        {
            for (var i = 0; i < StarCount; i++)
            {
                _stars.Add(Randomize(new Star(), randomDepth: true));
            }
        }

        _ticker ??= new FrameTicker(this, _ => Advance());
        _ticker.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _ticker?.Stop();
    }

    // Moves every star towards the viewer by Speed. A star that arrives is thrown
    // back to the far end with fresh values; one pushed out the back simply wraps to the front,
    // which is what lets a negative speed run the field in reverse without emptying it.
    private void Advance()
    {
        foreach (var star in _stars)
        {
            star.Z -= Speed;

            if (star.Z < MinZ)
            {
                Randomize(star, randomDepth: false);
            }
            else if (star.Z > MaxZ)
            {
                star.Z = MinZ;
            }
        }

        InvalidateVisual();
    }

    private Star Randomize(Star star, bool randomDepth)
    {
        star.X = (-1d + (_random.NextDouble() * 2d)) * Spread;
        star.Y = (-1d + (_random.NextDouble() * 2d)) * Spread;
        star.Z = randomDepth ? _random.NextDouble() * MaxZ : MaxZ;

        star.IsGlowing = _random.NextDouble() < GlowingShare;

        if (star.IsGlowing)
        {
            star.Brush = new SolidColorBrush(GlowColor);
            star.Size = 2d + (_random.NextDouble() * 2d);
        }
        else
        {
            star.Brush = Brushes.White;
            star.Size = 0.5d + (_random.NextDouble() * 2d);
        }

        return star;
    }

    private sealed class Star
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public double Size { get; set; } = 1d;

        public bool IsGlowing { get; set; }

        public IBrush Brush { get; set; } = Brushes.White;
    }
}
