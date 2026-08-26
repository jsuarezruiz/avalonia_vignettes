using Avalonia;

namespace SparkleParty.Effects;

/// <summary>
/// Bursts that climb, spread and fade to white sparks. Port of <c>fireworks.dart</c>.
/// </summary>
/// <remarks>
/// A burst is set off by a tap, and by the effect itself every so often when nobody is tapping. The
/// cooldown is what stops a quick tap clearing the point before the tick has read it.
/// </remarks>
public sealed class Fireworks : ParticleField
{
    private double _hue = 120d;
    private int _cooldown;
    private int _nextAuto = 10;
    private Point? _pending;

    public Fireworks(SpriteSheet sheet, Size size)
        : base(sheet, size)
    {
    }

    public override Point? TouchPoint
    {
        get => _pending;
        set
        {
            if (value is not null && _cooldown <= 0)
            {
                _pending = value;
                _cooldown = 4;
            }
        }
    }

    public override void Tick()
    {
        var adding = 0;
        var wow = 0d;

        if (_pending is not null)
        {
            wow = Rnd.Double(0.4d, 1d);
            adding = (int)(wow * 600d);
            _hue = Rnd.Degrees();
        }

        for (var i = 0; i < Count; i++)
        {
            var particle = Particles[i];

            if (particle.Life == 0d && --adding > 0 && _pending is { } point)
            {
                Activate(i, point, wow);
            }
            else if (particle.Life == 0d)
            {
                continue;
            }

            particle.VelocityY += 0.2d;
            particle.VelocityY *= 0.95d;
            particle.Y += particle.VelocityY;

            particle.VelocityX *= 0.95d;
            particle.X += particle.VelocityX;

            particle.VelocityZ *= 0.98d;
            particle.Z += particle.VelocityZ;

            if (--particle.Life <= 0d)
            {
                ResetParticle(i);
                continue;
            }

            var radius = ((particle.Life / 100d * 0.7d) + 0.3d) * (8d + particle.Z);

            SetQuad(i, particle.X - radius, particle.Y - radius, particle.X + radius, particle.Y + radius);

            // The last of a spark's life is spent white and flickering.
            if (particle.Life < 30d && !particle.Animate)
            {
                particle.Animate = true;

                SetColour(i, SkiaSharp.SKColors.White);
            }

            if (particle.Animate)
            {
                SetFrame(i, Rnd.Int(0, Sheet.Length));
            }
        }

        _pending = null;

        if (--_cooldown < -_nextAuto)
        {
            TouchPoint = new Point(Rnd.Double(0.2d, 0.8d) * Width, Rnd.Double(0.2d, 0.6d) * Height);
            _nextAuto = Rnd.Int(10, 90);
        }
    }

    private void Activate(int index, Point point, double wow)
    {
        var particle = Particles[index];
        var maximum = wow * 18d;
        var angle = Rnd.Radians();
        var speed = Rnd.Double(0.5d, maximum);

        particle.X = point.X;
        particle.Y = point.Y;
        particle.VelocityX = Math.Sin(angle) * speed;
        particle.VelocityY = (Math.Cos(angle) * speed) - 5d;

        // Depth is faked: the slower sparks are read as nearer and drawn larger.
        particle.Z = 0d;
        particle.VelocityZ = Math.Cos(speed / maximum * Math.PI / 2d) * wow;

        particle.Animate = false;
        particle.Life = Rnd.Double(60d, 100d);

        SetColour(index, FromHsl(_hue + Rnd.Double(0d, 40d), 1d, 0.45d));
    }
}
