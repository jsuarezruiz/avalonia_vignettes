using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;

namespace DogSlider.Controls;

/// <summary>
/// The dog, drawn and rigged in Avalonia rather than played back from a file.
/// </summary>
/// <remarks>
/// The original ships the dog as a Flare (Rive 1) document and plays its "walk" and "sit-front"
/// timelines. There is no runtime that reads that format from Avalonia. The current Rive control
/// for Avalonia plays Rive 2's <c>.riv</c>, which is a different format, so the character is
/// rebuilt here instead: the same parts the Flare file names (body, shade, patches, ears, legs,
/// leash and tag) drawn as vector shapes and posed from code.
/// <para>
/// Everything is laid out on a fixed design grid and scaled to fit, so the rig can be reasoned about
/// in whole numbers. Two values drive it: a looping walk phase, and a blend from standing to
/// sitting. Legs swing in diagonal pairs off the phase, the body bobs at twice that rate, and the
/// ear and tail trail behind, enough to read as a walk at the size it is drawn.
/// </para>
/// </remarks>
public sealed class DogView : Control
{
    /// <summary>
    /// Defines the <see cref="IsWalking"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsWalkingProperty =
        AvaloniaProperty.Register<DogView, bool>(nameof(IsWalking));

    /// <summary>
    /// Defines the <see cref="IsFlipped"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFlippedProperty =
        AvaloniaProperty.Register<DogView, bool>(nameof(IsFlipped));

    /// <summary>
    /// The grid the dog is drawn on, nose to tail.
    /// </summary>
    private const double DesignWidth = 100d;

    /// <summary>
    /// The grid the dog is drawn on, ears to paws.
    /// </summary>
    private const double DesignHeight = 56d;

    /// <summary>
    /// How long one full stride takes.
    /// </summary>
    private const double StrideSeconds = 0.42d;

    /// <summary>
    /// How long the dog takes to fold into a sit.
    /// </summary>
    private const double SitSeconds = 0.35d;

    /// <summary>
    /// How far a leg swings, in degrees.
    /// </summary>
    private const double LegSwing = 24d;

    /// <summary>
    /// Where the legs hang from.
    /// </summary>
    private const double HipY = 36d;

    /// <summary>
    /// How long a leg is. Short and stubby, as the original draws them.
    /// </summary>
    private const double LegLength = 19d;

    private static readonly IBrush CoatBrush = new SolidColorBrush(Color.FromRgb(0xE7, 0xCB, 0xB9));
    private static readonly IBrush ShadeBrush = new SolidColorBrush(Color.FromRgb(0xD8, 0xB8, 0xA3));
    private static readonly IBrush PatchBrush = new SolidColorBrush(Color.FromRgb(0xE8, 0x73, 0x40));
    private static readonly IBrush EarBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x31, 0x33));
    private static readonly IBrush CollarBrush = new SolidColorBrush(Color.FromRgb(0x2C, 0xB5, 0xB5));
    private static readonly IBrush TagBrush = new SolidColorBrush(Color.FromRgb(0xF1, 0xA3, 0x5D));

    private FrameTicker? _ticker;
    private TimeSpan _lastTick;
    private bool _hasLastTick;
    private double _walkPhase;
    private double _sitAmount = 1d;

    static DogView() =>
        AffectsRender<DogView>(IsWalkingProperty, IsFlippedProperty);

    /// <summary>
    /// Gets or sets a value indicating whether the dog is on the move.
    /// </summary>
    public bool IsWalking
    {
        get => GetValue(IsWalkingProperty);
        set => SetValue(IsWalkingProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the dog faces left.
    /// </summary>
    public bool IsFlipped
    {
        get => GetValue(IsFlippedProperty);
        set => SetValue(IsFlippedProperty, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var size = Bounds.Size;

        if (size.Width <= 0d || size.Height <= 0d)
        {
            return;
        }

        // Fit the design grid into the control, keeping the dog on the floor.
        var scale = Math.Min(size.Width / DesignWidth, size.Height / DesignHeight);
        var offsetX = (size.Width - (DesignWidth * scale)) / 2d;
        var offsetY = size.Height - (DesignHeight * scale);

        var fit = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offsetX, offsetY);

        // Facing is a mirror about the middle of the grid, so the dog turns on the spot.
        if (IsFlipped)
        {
            fit = Matrix.CreateTranslation(-DesignWidth / 2d, 0d)
                * Matrix.CreateScale(-1d, 1d)
                * Matrix.CreateTranslation(DesignWidth / 2d, 0d)
                * fit;
        }

        using (context.PushTransform(fit))
        {
            DrawDog(context);
        }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(DesignWidth, DesignHeight);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _ticker ??= new FrameTicker(this, OnTick);
        _ticker.Start();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _ticker?.Stop();
    }

    private void OnTick(TimeSpan elapsed)
    {
        var dt = _hasLastTick ? Math.Clamp((elapsed - _lastTick).TotalSeconds, 0d, 0.1d) : 0d;

        _lastTick = elapsed;
        _hasLastTick = true;

        var sitTarget = IsWalking ? 0d : 1d;
        var sit = Math.Clamp(_sitAmount + (Math.Sign(sitTarget - _sitAmount) * dt / SitSeconds), 0d, 1d);

        if (IsWalking)
        {
            _walkPhase = (_walkPhase + (dt / StrideSeconds)) % 1d;
        }

        _sitAmount = sit;

        InvalidateVisual();
    }

    private void DrawDog(DrawingContext context)
    {
        // Smoothstep keeps the fold into a sit from starting and stopping abruptly.
        var sit = _sitAmount * _sitAmount * (3d - (2d * _sitAmount));
        var swing = IsWalking ? LegSwing : 0d;
        var phase = _walkPhase * Math.PI * 2d;

        // The body bobs twice per stride, once for each pair of legs.
        var bob = IsWalking ? Math.Abs(Math.Sin(phase * 2d)) * 1.2d : 0d;

        // Sitting folds the hindquarters under rather than stretching the body down to the floor:
        // the barrel keeps its shape, a thigh swings out beneath it, and the whole dog tips back
        // onto it. The tip is taken about the shoulder, so the front legs stay planted.
        var tilt = sit * -6d;
        var backLeg = LegLength * (1d - sit);

        // The far pair are drawn first and in shadow, which is what gives the dog any depth at all.
        DrawLeg(context, 63d, Math.Sin(phase + Math.PI) * swing, LegLength, ShadeBrush);

        using (context.PushTransform(RotateAbout(tilt, new Point(68d, HipY))))
        {
            DrawLeg(context, 31d, Math.Sin(phase) * swing, backLeg, ShadeBrush);
            DrawTail(context, phase, sit);
            DrawHaunch(context, sit);
            DrawBody(context, bob);
            DrawHead(context, phase, bob);
            DrawLeg(context, 38d, Math.Sin(phase + Math.PI) * swing, backLeg, CoatBrush);
        }

        DrawLeg(context, 70d, Math.Sin(phase) * swing, LegLength, CoatBrush);
    }

    /// <summary>
    /// The thigh the dog settles onto. It is hidden inside the barrel while standing and swings out
    /// below it as the dog sits, which is what keeps the silhouette from simply swelling.
    /// </summary>
    private static void DrawHaunch(DrawingContext context, double sit)
    {
        context.DrawEllipse(
            CoatBrush,
            null,
            new Point(30d, 34d + (sit * 6d)),
            10.5d,
            6d + (sit * 5d));
    }

    private static void DrawBody(DrawingContext context, double bob)
    {
        // A soft bean rather than a box: heavier at the rump, tapering into the shoulders.
        var body = Path(
            (18d, 32d - bob),
            [
                ((18d, 25d - bob), (26d, 22d - bob), (39d, 22d - bob)),
                ((53d, 22d - bob), (67d, 23d - bob), (71d, 26d - bob)),
                ((74d, 30d - bob), (74d, 36d - bob), (69d, 38d - bob)),
                ((61d, 41d - bob), (28d, 41d - bob), (22d, 38d - bob)),
                ((19d, 36d - bob), (18d, 34d - bob), (18d, 32d - bob)),
            ]);

        context.DrawGeometry(CoatBrush, null, body);

        // Rust markings: a big one over the rump and a smaller one mid-back.
        // "Body Shade" in the Flare file: a darker underside, which stops the barrel reading flat.
        // A crescent: the outward curve follows the belly, the return runs higher so the shape has
        // area. Both bulging the same way collapses it to a sliver.
        context.DrawGeometry(ShadeBrush, null, Path(
            (25d, 34d - bob),
            [
                ((34d, 40.5d - bob), (58d, 40.5d - bob), (69d, 34d - bob)),
                ((58d, 37.5d - bob), (34d, 37.5d - bob), (25d, 34d - bob)),
            ]));

        context.DrawEllipse(PatchBrush, null, new Point(34d, 28d - bob), 8d, 5.5d);
        context.DrawEllipse(PatchBrush, null, new Point(55d, 27d - bob), 5.5d, 4.5d);
    }

    private static void DrawHead(DrawingContext context, double phase, double bob)
    {
        // The head sits low, level with the body, rather than perched above it.
        var centre = new Point(80d, 28d - bob);

        // Muzzle first, so the skull overlaps it.
        context.DrawGeometry(CoatBrush, null, Path(
            (centre.X + 2d, centre.Y - 3d),
            [
                ((centre.X + 12d, centre.Y - 4d), (centre.X + 19d, centre.Y), (centre.X + 19d, centre.Y + 4d)),
                ((centre.X + 19d, centre.Y + 8d), (centre.X + 11d, centre.Y + 10d), (centre.X + 2d, centre.Y + 9d)),
            ]));

        context.DrawEllipse(EarBrush, null, new Point(centre.X + 17.5d, centre.Y + 3.5d), 2.6d, 2.3d);

        // Skull over the top of both, then the markings on the face.
        context.DrawEllipse(CoatBrush, null, centre, 10.5d, 10d);
        context.DrawEllipse(PatchBrush, null, new Point(centre.X + 3d, centre.Y - 4d), 6.5d, 6d);
        context.DrawEllipse(EarBrush, null, new Point(centre.X + 7d, centre.Y - 1d), 1.7d, 1.7d);

        // The ear caps the back of the skull and hangs past it, trailing the stride. It sits over
        // the head rather than behind it, which is what makes it read as a floppy ear.
        using (context.PushTransform(RotateAbout(Math.Sin(phase - 0.6d) * 6d, new Point(centre.X - 6d, centre.Y - 8d))))
        {
            context.DrawGeometry(EarBrush, null, Path(
                (centre.X - 13d, centre.Y - 4d),
                [
                    ((centre.X - 13d, centre.Y - 12d), (centre.X - 5d, centre.Y - 14d), (centre.X - 1d, centre.Y - 9d)),
                    ((centre.X + 2d, centre.Y - 4d), (centre.X, centre.Y + 6d), (centre.X - 5d, centre.Y + 8d)),
                    ((centre.X - 11d, centre.Y + 10d), (centre.X - 14d, centre.Y + 3d), (centre.X - 13d, centre.Y - 4d)),
                ]));
        }

        // Collar and tag, at the join between head and body.
        context.DrawRectangle(
            CollarBrush,
            null,
            new RoundedRect(new Rect(centre.X - 14d, centre.Y - 5d, 5.5d, 17d), 2d));

        context.DrawEllipse(TagBrush, null, new Point(centre.X - 11d, centre.Y + 13d), 2.8d, 2.8d);
    }

    private static void DrawTail(DrawingContext context, double phase, double sit)
    {
        // The tail wags across the stride, and drops when the dog sits.
        var wag = (Math.Sin(phase * 2d) * 14d) - (sit * 24d);

        using (context.PushTransform(RotateAbout(wag - 34d, new Point(21d, 28d))))
        {
            // Tapered and curved, thick where it leaves the rump and tipped over at the end.
            context.DrawGeometry(CoatBrush, null, Path(
                (18.5d, 28d),
                [
                    ((17.5d, 21d), (18d, 15d), (21.5d, 11.5d)),
                    ((23d, 10d), (25d, 11.5d), (24d, 13.5d)),
                    ((22d, 17d), (22.5d, 22d), (23.5d, 28d)),
                ]));
        }
    }

    /// <summary>
    /// Draws a leg as two segments with a knee between them. The knee tucks as the leg swings
    /// through and straightens as it takes weight, which lifts the paw clear of the ground instead
    /// of dragging it: a straight rod on a hinge reads as a pendulum, not a step.
    /// </summary>
    private static void DrawLeg(DrawingContext context, double x, double angle, double length, IBrush brush)
    {
        if (length <= 0d)
        {
            return;
        }

        var hip = new Point(x, HipY);
        var thigh = length * 0.55d;
        var shin = length - thigh;

        // A negative angle is the leg reaching forward, which is the half of the stride that bends.
        var knee = Math.Max(0d, -angle) * 0.9d;

        using (context.PushTransform(RotateAbout(angle, hip)))
        {
            context.DrawRectangle(
                brush,
                null,
                new RoundedRect(new Rect(hip.X - 3.25d, hip.Y, 6.5d, thigh + 2d), 3.25d));

            var joint = new Point(hip.X, hip.Y + thigh);

            using (context.PushTransform(RotateAbout(knee, joint)))
            {
                context.DrawRectangle(
                    brush,
                    null,
                    new RoundedRect(new Rect(joint.X - 3d, joint.Y, 6d, shin), 3d));

                context.DrawEllipse(brush, null, new Point(joint.X + 0.8d, joint.Y + shin), 3.8d, 2.6d);
            }
        }
    }

    /// <summary>
    /// Builds a closed shape from a start point and a run of cubic segments.
    /// </summary>
    private static StreamGeometry Path(
        (double X, double Y) start,
        ((double X, double Y) C1, (double X, double Y) C2, (double X, double Y) End)[] curves)
    {
        var geometry = new StreamGeometry();

        using var figure = geometry.Open();

        figure.BeginFigure(new Point(start.X, start.Y), isFilled: true);

        foreach (var (c1, c2, end) in curves)
        {
            figure.CubicBezierTo(new Point(c1.X, c1.Y), new Point(c2.X, c2.Y), new Point(end.X, end.Y));
        }

        figure.EndFigure(isClosed: true);

        return geometry;
    }

    private static Matrix RotateAbout(double degrees, Point origin) =>
        Matrix.CreateTranslation(-origin.X, -origin.Y)
        * Matrix.CreateRotation(degrees * Math.PI / 180d)
        * Matrix.CreateTranslation(origin.X, origin.Y);
}
