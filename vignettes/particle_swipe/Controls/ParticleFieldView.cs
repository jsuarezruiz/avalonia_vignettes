using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;
using ParticleSwipe.Effects;

namespace ParticleSwipe.Controls;

/// <summary>
/// Draws a <see cref="Effects.ParticleField"/> over whatever is behind it. Port of
/// <c>particle_field_painter.dart</c>.
/// </summary>
/// <remarks>
/// The layer sits above the list and takes no pointer input, so bursts carry on over rows that are
/// still being scrolled or swiped. Particles are positioned in this control's coordinates rather
/// than the list's, which is why a burst stays where it was thrown even if the list scrolls
/// underneath it. The original does the same, with its painter filling the demo's stack.
/// </remarks>
public sealed class ParticleFieldView : Control
{
    private const string SpriteUri = "avares://ParticleSwipe/Assets/Images/circle_spritesheet.png";

    private const int SpriteFrames = 15;

    private const int SpriteFrameSize = 10;

    private readonly SpriteSheet _sprites = new(new Uri(SpriteUri), SpriteFrames, SpriteFrameSize, SpriteFrameSize);

    private FrameTicker? _ticker;

    public ParticleFieldView() => IsHitTestVisible = false;

    /// <summary>
    /// Gets the field being drawn.
    /// </summary>
    public ParticleField Field { get; } = new();

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        foreach (var particle in Field.Particles)
        {
            if (_sprites.GetFrame(GetFrameIndex(particle.Life)) is not { } frame)
            {
                continue;
            }

            // The sprite is anchored at its top left and scaled by what is left of the particle, so
            // a dying particle shrinks towards the point it was last drawn at.
            var size = SpriteFrameSize * particle.Life;

            context.DrawImage(
                _sprites.GetTinted(particle.Color),
                frame,
                new Rect(particle.X, particle.Y, size, size));
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _ticker ??= new FrameTicker(this, OnTick);
        _ticker.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _ticker?.Stop();
    }

    // Picks the sprite frame for a particle. The sheet is run through twice over a particle's life
    // and backwards, so the ring the frames draw closes in as the particle dies.
    private static int GetFrameIndex(double life) =>
        (int)Math.Floor(SpriteFrames * life * 2d % SpriteFrames);

    private void OnTick(TimeSpan elapsed)
    {
        if (Field.Tick(elapsed))
        {
            InvalidateVisual();
        }
    }
}
