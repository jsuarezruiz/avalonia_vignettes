using Avalonia.Media;

namespace ParticleSwipe.Effects;

/// <summary>
/// A single particle. Port of <c>particle.dart</c>.
/// </summary>
/// <remarks>
/// <c>Life</c> does three jobs at once: it counts down to the particle's removal, it scales the
/// sprite, and it picks the sprite frame. That is why particles shrink and change shape as they
/// fade rather than simply going transparent.
/// </remarks>
public sealed class Particle
{
    /// <summary>
    /// Gets or sets the horizontal position, in pixels.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the vertical position, in pixels.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the horizontal velocity, in pixels per frame at 60fps.
    /// </summary>
    public double VelocityX { get; set; }

    /// <summary>
    /// Gets or sets the vertical velocity, in pixels per frame at 60fps.
    /// </summary>
    public double VelocityY { get; set; }

    /// <summary>
    /// Gets or sets what is left of the particle, from 1 down to 0.
    /// </summary>
    public double Life { get; set; } = 1d;

    /// <summary>
    /// Gets or sets the colour, whose alpha carries the particle's weight in the burst.
    /// </summary>
    public Color Color { get; set; }
}

/// <summary>
/// The particles thrown out by a swipe, and the physics that carries them. Port of
/// <c>particle_field.dart</c>; this holds the data only, <c>ParticleFieldView</c> draws it.
/// </summary>
public sealed class ParticleField
{
    /// <summary>
    /// The colour of a delete burst.
    /// </summary>
    public static readonly Color DeleteColor = Color.FromRgb(0xCB, 0x4A, 0x65);

    /// <summary>
    /// The colour of a favourite burst.
    /// </summary>
    public static readonly Color FavoriteColor = Color.FromRgb(0x54, 0xD8, 0xE6);

    /// <summary>
    /// Downward pull applied every frame.
    /// </summary>
    private const double Gravity = 0.05d;

    /// <summary>
    /// How much life a particle loses every frame.
    /// </summary>
    private const double LifeDecay = 0.01d;

    /// <summary>
    /// Alternate particles are drawn faintly, which is what gives a burst its depth.
    /// </summary>
    private const double StrongAlpha = 0.8d;

    /// <summary>
    /// The alpha of the faint particles.
    /// </summary>
    private const double FaintAlpha = 0.3d;

    private readonly List<Particle> _particles = [];
    private readonly Random _random = new();

    private TimeSpan _lastTick;
    private bool _hasLastTick;

    /// <summary>
    /// Gets the live particles.
    /// </summary>
    public IReadOnlyList<Particle> Particles => _particles;

    /// <summary>
    /// Throws a line of particles out sideways, the effect a deleted row leaves behind. The
    /// particles are spread evenly along the row's top edge and all drift left.
    /// </summary>
    /// <param name="x">The left edge of the line.</param>
    /// <param name="y">The height of the line.</param>
    /// <param name="width">How far the line runs.</param>
    /// <param name="count">How many particles to throw.</param>
    public void LineExplosion(double x, double y, double width, int count = 150)
    {
        for (var i = 0; i < count; i++)
        {
            _particles.Add(new Particle
            {
                X = x + ((double)i / count * width),
                Y = y,
                VelocityX = (_random.NextDouble() * 5d) - 5d,
                VelocityY = (_random.NextDouble() * 3d) - 2.5d,
                Life = (_random.NextDouble() * 0.5d) + 0.5d,
                Color = WithAlpha(DeleteColor, i),
            });
        }
    }

    /// <summary>
    /// Throws particles outwards from a point like a firework, the effect of starring a message.
    /// They start on a ring rather than at the centre, so the burst opens as a ring instead of
    /// swelling out of a single dot.
    /// </summary>
    /// <param name="x">The centre of the burst.</param>
    /// <param name="y">The centre of the burst.</param>
    /// <param name="count">How many particles to throw.</param>
    public void PointExplosion(double x, double y, int count = 55)
    {
        const double StartRadius = 18d;

        for (var i = 0; i < count; i++)
        {
            var rotation = (double)i / count * Math.PI * 2d;
            var (sin, cos) = Math.SinCos(rotation);
            var speed = (_random.NextDouble() * 2d) + 0.5d;

            _particles.Add(new Particle
            {
                X = x + (StartRadius * cos),
                Y = y + (StartRadius * sin),
                VelocityX = cos * speed,
                VelocityY = sin * speed,
                Life = (_random.NextDouble() * 0.5d) + 0.5d,
                Color = WithAlpha(FavoriteColor, i),
            });
        }
    }

    /// <summary>
    /// Advances every particle and drops the ones that have burnt out. <paramref name="elapsed"/> is
    /// the ticker's running total, not a delta.
    /// </summary>
    /// <returns><see langword="true"/> when something moved and the field needs redrawing.</returns>
    public bool Tick(TimeSpan elapsed)
    {
        // Time is counted in 60ths of a second so the velocities read as per-frame values, capped at
        // 1.5 frames so a stalled frame cannot fling everything off screen, and floored at zero so a
        // restarted clock cannot run the field backwards.
        var t = _hasLastTick
            ? Math.Clamp((elapsed - _lastTick).TotalSeconds * 60d, 0d, 1.5d)
            : 0d;

        _lastTick = elapsed;
        _hasLastTick = true;

        if (_particles.Count == 0)
        {
            return false;
        }

        for (var i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];

            particle.VelocityY += Gravity * t;
            particle.X += particle.VelocityX * t;
            particle.Y += particle.VelocityY * t;
            particle.Life -= LifeDecay * t;

            if (particle.Life <= 0d)
            {
                _particles.RemoveAt(i);
            }
        }

        return true;
    }

    private static Color WithAlpha(Color color, int index)
    {
        var alpha = index % 2 == 0 ? StrongAlpha : FaintAlpha;

        return new Color((byte)Math.Round(alpha * 255d), color.R, color.G, color.B);
    }
}
