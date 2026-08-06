using Avalonia;

namespace SparkleParty.Effects;

/// <summary>
/// A trail that chases the pointer and falls away behind it. Port of <c>comet.dart</c>.
/// </summary>
/// <remarks>
/// The head lags the pointer rather than following it exactly, and how fast the pointer is moving
/// decides how much it throws off, which is what gives the trail its weight.
/// </remarks>
public sealed class Comet : ParticleField
{
    private double _hue;
    private Point? _previous;
    private double _power;
    private Point _point;

    /// <summary>
    /// Initializes a new instance of the <see cref="Comet"/> class.
    /// </summary>
    public Comet(SpriteSheet sheet, Size size)
        : base(sheet, size)
    {
        Center = new Point(Width * 0.5d, 30d);
        _point = new Point(Width * 0.5d, Height + 50d);
    }

    /// <inheritdoc />
    public override void Tick()
    {
        var target = TouchPoint ?? Center;
        var chase = TouchPoint is null ? 0.05d : 0.2d;

        _point += (target - _point) * chase;

        _hue += 10d;

        var power = UpdatePower();
        var adding = (int)(180d * power);

        for (var i = 0; i < Count; i++)
        {
            var particle = Particles[i];

            if (particle.Life == 0d && --adding > 0)
            {
                Activate(i, _point, power);
            }
            else if (particle.Life == 0d)
            {
                continue;
            }

            particle.VelocityY += 0.25d;
            particle.VelocityY *= 0.99d;
            particle.X += particle.VelocityX;
            particle.VelocityX *= 0.99d;
            particle.Y += particle.VelocityY;

            particle.Life -= 1.2d;

            if (particle.Life <= 0d)
            {
                ResetParticle(i);
                continue;
            }

            var radius = particle.Life / 100d * 24d;

            SetQuad(i, particle.X - radius, particle.Y - radius, particle.X + radius, particle.Y + radius);
            SetFrame(i, ++particle.Frame % Sheet.Length);
        }

        _previous = TouchPoint;
    }

    private void Activate(int index, Point point, double power)
    {
        var particle = Particles[index];
        var angle = Rnd.Radians();
        var speed = Rnd.Double(3d, 10d) * power;

        particle.X = point.X;
        particle.Y = point.Y;
        particle.VelocityX = Math.Sin(angle) * speed;
        particle.VelocityY = Math.Cos(angle) * speed;

        particle.Animate = true;
        particle.Frame = Rnd.Int(0, Sheet.Length);
        particle.Life = Rnd.Double(50d, 100d) * (0.5d + (0.5d * power));

        var hue = (_hue * 0.5d) + (Rnd.Ratio * 40d) + (angle / Math.PI * 30d);

        SetColour(index, FromHsl(hue, 1d, Rnd.Bool(0.1d) ? 1d : 0.4d));
    }

    /// <summary>
    /// How hard the comet is throwing off sparks, which follows the pointer's speed.
    /// </summary>
    private double UpdatePower()
    {
        var power = 0.25d;

        if (TouchPoint is { } touch && _previous is { } previous)
        {
            var travelled = touch - previous;

            power = Math.Min(1d, (Math.Sqrt((travelled.X * travelled.X) + (travelled.Y * travelled.Y)) / 20d) + power);
        }

        _power += (power - _power) * 0.05d;

        return _power;
    }
}
