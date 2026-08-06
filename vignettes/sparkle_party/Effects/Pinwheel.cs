using Avalonia;

namespace SparkleParty.Effects;

/// <summary>
/// Arms of sparks turning about a point. Port of <c>pinwheel.dart</c>.
/// </summary>
public sealed class Pinwheel : ParticleField
{
    /// <summary>
    /// How many arms the wheel throws.
    /// </summary>
    private const int Arms = 11;

    private double _hue;
    private Point _point;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pinwheel"/> class.
    /// </summary>
    public Pinwheel(SpriteSheet sheet, Size size)
        : base(sheet, size)
    {
        _point = Center;
    }

    /// <inheritdoc />
    public override void Tick()
    {
        var target = TouchPoint ?? Center;
        var chase = TouchPoint is null ? 0.05d : 0.2d;

        _point += (target - _point) * chase;

        _hue += 10d;

        var adding = 200;

        for (var i = 0; i < Count; i++)
        {
            var particle = Particles[i];

            if (particle.Life == 0d && --adding > 0)
            {
                Activate(i, _point);
            }
            else if (particle.Life == 0d)
            {
                continue;
            }

            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;

            // The wheel's sparks die away rather than fading evenly, which tapers each arm.
            particle.Life *= 0.95d;
            particle.Life -= 1d;

            if (particle.Life <= 0d)
            {
                ResetParticle(i);
                continue;
            }

            var radius = particle.Life / 100d * 48d;

            SetQuad(i, particle.X - radius, particle.Y - radius, particle.X + radius, particle.Y + radius);

            if (particle.Animate)
            {
                SetFrame(i, ++particle.Frame % Sheet.Length);
            }
        }
    }

    private void Activate(int index, Point point)
    {
        var particle = Particles[index];
        var angle = (Rnd.Int(0, Arms) / (double)Arms * Math.PI * 2d)
            - (_hue * 0.007d)
            + Rnd.Double(0d, Math.PI / Arms / 2d);
        var speed = Rnd.Double(10d, 12d);

        particle.X = point.X;
        particle.Y = point.Y;
        particle.VelocityX = Math.Sin(angle) * speed;
        particle.VelocityY = Math.Cos(angle) * speed;

        particle.Animate = true;
        particle.Life = Rnd.Double(60d, 100d);
        particle.Frame = Rnd.Int(0, Sheet.Length);

        var hue = (_hue * 0.2d) + (Rnd.Ratio * 40d) + (angle / Math.PI * 180d);

        SetColour(index, FromHsl(hue, 1d, (Rnd.Bit(0.05d) * 0.6d) + 0.4d));
    }
}
