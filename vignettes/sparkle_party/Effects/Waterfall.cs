using Avalonia;

namespace SparkleParty.Effects;

/// <summary>
/// Sparkles pouring down the screen, which the pointer pushes aside. Port of <c>waterfall.dart</c>.
/// </summary>
public sealed class Waterfall : ParticleField
{
    private const double Reach = 100d;

    private double _hue;

    public Waterfall(SpriteSheet sheet, Size size)
        : base(sheet, size, Math.Min(size.Width, size.Height) > 600d ? 40000 : 20000)
    {
    }

    public override void Tick()
    {
        _hue -= 1d;

        for (var i = 0; i < Count; i++)
        {
            var particle = Particles[i];

            if (TouchPoint is { } touch)
            {
                var dy = touch.Y - particle.Y;
                var dx = touch.X - particle.X;

                if (dy < Reach && dx < Reach)
                {
                    var distance = Math.Sqrt((dx * dx) + (dy * dy));

                    if (distance < Reach)
                    {
                        var angle = Math.Atan2(dy, dx);
                        var push = (Reach - distance) / Reach * -1d;

                        particle.VelocityX += push * Math.Cos(angle);
                        particle.VelocityY += push * Math.Sin(angle);
                    }
                }
            }

            particle.VelocityY += 0.1d;
            particle.X += particle.VelocityX;
            particle.VelocityX *= 0.99d;
            particle.Y += particle.VelocityY;

            if (particle.Y > Height)
            {
                Activate(i);
            }

            var radius = 12d * particle.Scale;

            SetQuad(i, particle.X - radius, particle.Y - radius, particle.X + radius, particle.Y + radius);

            if (particle.Animate)
            {
                SetFrame(i, ++particle.Frame % Sheet.Length);
            }
        }
    }

    protected override Particle ResetParticle(int index)
    {
        var particle = base.ResetParticle(index);

        // Spread down the screen to begin with, so the fall starts already flowing.
        particle.Y = Rnd.Ratio * Height;

        return particle;
    }

    private void Activate(int index)
    {
        var particle = Particles[index];

        particle.X = Rnd.Ratio * Width;
        particle.Y = 0d;
        particle.VelocityX = Rnd.Double(-2d, 2d);
        particle.VelocityY = Rnd.Ratio * 5d;
        particle.Animate = Rnd.Bool(0.02d);
        particle.Scale = Rnd.Double(0.8d, 1.2d);

        // The hue drifts with time and across the screen, so the fall is a moving rainbow.
        var hue = _hue + (Rnd.Ratio * 40d) + (particle.X / Width * 90d);

        SetColour(index, FromHsl(hue, 1d, particle.Animate ? 1d : 0.4d));
    }
}
