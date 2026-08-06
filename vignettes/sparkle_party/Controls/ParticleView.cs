using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using AvaloniaVignettes.Shared.Animation;
using SkiaSharp;
using SparkleParty.Effects;

namespace SparkleParty.Controls;

/// <summary>
/// Runs an effect and draws its particles. Port of <c>fx_renderer.dart</c> and
/// <c>particle_fx_painter.dart</c>.
/// </summary>
/// <remarks>
/// Twenty thousand particles is far too many to draw one at a time, and the original does not: it
/// builds two triangles per particle into flat buffers and hands the lot to Skia as a single
/// <c>drawVertices</c>. Avalonia has no wrapper for that call, but it does lease out the Skia canvas
/// underneath, so the port is the same one call.
/// <para>
/// The sheet arrives as the paint's shader and the particles' colours are blended into it with
/// <see cref="SKBlendMode.DstIn"/>, which keeps the sparkle's shape and takes the effect's hue.
/// </para>
/// </remarks>
public sealed class ParticleView : Control
{
    /// <summary>
    /// Defines the <see cref="Field"/> property.
    /// </summary>
    public static readonly StyledProperty<ParticleField?> FieldProperty =
        AvaloniaProperty.Register<ParticleView, ParticleField?>(nameof(Field));

    /// <summary>
    /// Defines the <see cref="Touched"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> TouchedEvent =
        RoutedEvent.Register<ParticleView, RoutedEventArgs>(
            nameof(Touched),
            RoutingStrategies.Bubble);

    private readonly FrameTicker _ticker;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleView"/> class.
    /// </summary>
    public ParticleView()
    {
        _ticker = new FrameTicker(this, _ => Advance());

        ClipToBounds = true;
    }

    /// <summary>
    /// Raised the first time the field is touched, which is what dismisses the caption.
    /// </summary>
    public event EventHandler<RoutedEventArgs> Touched
    {
        add => AddHandler(TouchedEvent, value);
        remove => RemoveHandler(TouchedEvent, value);
    }

    /// <summary>
    /// Gets or sets the effect being played.
    /// </summary>
    public ParticleField? Field
    {
        get => GetValue(FieldProperty);
        set => SetValue(FieldProperty, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Transparent, but drawn: a control with nothing under the pointer is not hit tested, and
        // the original asks for the same thing with HitTestBehavior.opaque.
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));

        if (Field is { } field)
        {
            context.Custom(new ParticleDrawOperation(new Rect(Bounds.Size), field));
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // A ticker asks its top level for frames, so it can only start once there is one.
        _ticker.Start();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _ticker.Stop();
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        SetTouchPoint(e.GetPosition(this));
        e.Pointer.Capture(this);
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (Equals(e.Pointer.Captured, this))
        {
            SetTouchPoint(e.GetPosition(this));
        }
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        SetTouchPoint(null);
        e.Pointer.Capture(null);
    }

    private void SetTouchPoint(Point? point)
    {
        if (Field is { } field)
        {
            field.TouchPoint = point;
        }

        RaiseEvent(new RoutedEventArgs(TouchedEvent));
    }

    private void Advance()
    {
        Field?.Tick();

        InvalidateVisual();
    }

    /// <summary>
    /// Hands the effect's buffers to Skia, which is the whole of the drawing.
    /// </summary>
    private sealed class ParticleDrawOperation(Rect bounds, ParticleField field) : ICustomDrawOperation
    {
        /// <inheritdoc />
        public Rect Bounds { get; } = bounds;

        /// <inheritdoc />
        public bool HitTest(Point p) => Bounds.Contains(p);

        /// <inheritdoc />
        public bool Equals(ICustomDrawOperation? other) => false;

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        public void Render(ImmediateDrawingContext context)
        {
            if (context.TryGetFeature<ISkiaSharpApiLeaseFeature>() is not { } feature)
            {
                return;
            }

            using var lease = feature.Lease();
            using var vertices = SKVertices.CreateCopy(
                SKVertexMode.Triangles,
                field.Positions,
                field.TexCoords,
                field.Colours);
            using var paint = new SKPaint
            {
                Shader = SKShader.CreateImage(field.Sheet.Image),
                IsAntialias = true,
            };

            lease.SkCanvas.DrawVertices(vertices, SKBlendMode.DstIn, paint);
        }
    }
}
