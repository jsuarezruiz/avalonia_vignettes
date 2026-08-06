using Avalonia;
using SkiaSharp;

namespace SparkleParty.Effects;

/// <summary>
/// One particle of an effect. Port of <c>Particle</c> and the extra fields its subclasses add,
/// gathered into one type because four effects do not earn a hierarchy.
/// </summary>
public sealed class Particle
{
    /// <summary>
    /// Gets or sets how far across the field the particle is.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets how far down the field the particle is.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets how fast it is moving across the field.
    /// </summary>
    public double VelocityX { get; set; }

    /// <summary>
    /// Gets or sets how fast it is moving down the field.
    /// </summary>
    public double VelocityY { get; set; }

    /// <summary>
    /// Gets or sets how much longer it lives. Zero means it is spent and free to reuse.
    /// </summary>
    public double Life { get; set; }

    /// <summary>
    /// Gets or sets which frame of the sheet it is drawn with.
    /// </summary>
    public int Frame { get; set; }

    /// <summary>
    /// Gets or sets whether it runs through the sheet's frames.
    /// </summary>
    public bool Animate { get; set; }

    /// <summary>
    /// Gets or sets how big it is drawn, where the effect varies that.
    /// </summary>
    public double Scale { get; set; } = 1d;

    /// <summary>
    /// Gets or sets its distance towards the viewer, for the fireworks' fake depth.
    /// </summary>
    public double Z { get; set; }

    /// <summary>
    /// Gets or sets how fast that distance is changing.
    /// </summary>
    public double VelocityZ { get; set; }
}

/// <summary>
/// A field of particles, drawn as two textured triangles each. Port of <c>particle_fx.dart</c>.
/// </summary>
/// <remarks>
/// The buffers are laid out as the original's are, six vertices to a particle, and handed to Skia in
/// one call. Nothing here draws: an effect fills the buffers each tick and
/// <see cref="Controls.ParticleView"/> hands them over.
/// </remarks>
public abstract class ParticleField
{
    /// <summary>
    /// How many vertices a particle takes: two triangles.
    /// </summary>
    private const int VerticesPerParticle = 6;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleField"/> class.
    /// </summary>
    /// <param name="sheet">The sheet the particles are drawn from.</param>
    /// <param name="size">The area the effect plays in.</param>
    /// <param name="count">How many particles it has.</param>
    protected ParticleField(SpriteSheet sheet, Size size, int count = 10000)
    {
        Sheet = sheet;
        Width = size.Width;
        Height = size.Height;
        Count = count;
        Center = new Point(Width / 2d, Height / 2d);

        Positions = new SKPoint[count * VerticesPerParticle];
        TexCoords = new SKPoint[count * VerticesPerParticle];
        Colours = new SKColor[count * VerticesPerParticle];
        Particles = new Particle[count];

        for (var i = 0; i < count; i++)
        {
            Particles[i] = new Particle();
        }
    }

    /// <summary>
    /// Gets the vertex positions, six to a particle.
    /// </summary>
    public SKPoint[] Positions { get; }

    /// <summary>
    /// Gets the texture coordinates, six to a particle.
    /// </summary>
    public SKPoint[] TexCoords { get; }

    /// <summary>
    /// Gets the vertex colours, six to a particle.
    /// </summary>
    public SKColor[] Colours { get; }

    /// <summary>
    /// Gets or sets where the pointer is, or null when it is not down.
    /// </summary>
    public virtual Point? TouchPoint { get; set; }

    /// <summary>
    /// Gets the middle of the field, which the effects gather round.
    /// </summary>
    protected Point Center { get; set; }

    /// <summary>
    /// Gets the sheet the particles are drawn from.
    /// </summary>
    public SpriteSheet Sheet { get; }

    /// <summary>
    /// Gets how wide the field is.
    /// </summary>
    protected double Width { get; }

    /// <summary>
    /// Gets how tall the field is.
    /// </summary>
    protected double Height { get; }

    /// <summary>
    /// Gets how many particles the field has.
    /// </summary>
    protected int Count { get; }

    /// <summary>
    /// Gets the particles.
    /// </summary>
    protected Particle[] Particles { get; }

    /// <summary>
    /// Moves every particle on by one frame.
    /// </summary>
    public abstract void Tick();

    /// <summary>
    /// Starts the field off, with every particle spent and hidden.
    /// </summary>
    public void Reset()
    {
        for (var i = 0; i < Count; i++)
        {
            ResetParticle(i);
        }
    }

    /// <summary>
    /// Spends a particle and hides it.
    /// </summary>
    protected virtual Particle ResetParticle(int index)
    {
        var particle = Particles[index];

        particle.X = particle.Y = particle.VelocityX = particle.VelocityY = particle.Life = 0d;
        particle.Z = particle.VelocityZ = 0d;
        particle.Frame = Sheet.Length / 2;
        particle.Animate = false;

        SetColour(index, default);
        SetQuad(index, 0d, 0d, 0d, 0d);
        SetFrame(index, particle.Frame);

        return particle;
    }

    /// <summary>
    /// Places a particle's two triangles.
    /// </summary>
    protected void SetQuad(int index, double left, double top, double right, double bottom)
    {
        var i = index * VerticesPerParticle;
        var l = (float)left;
        var t = (float)top;
        var r = (float)right;
        var b = (float)bottom;

        Positions[i + 0] = new SKPoint(l, t);
        Positions[i + 1] = new SKPoint(r, t);
        Positions[i + 2] = new SKPoint(l, b);
        Positions[i + 3] = new SKPoint(r, t);
        Positions[i + 4] = new SKPoint(r, b);
        Positions[i + 5] = new SKPoint(l, b);
    }

    /// <summary>
    /// Colours a particle. The sheet gives the shape; this gives the hue.
    /// </summary>
    protected void SetColour(int index, SKColor colour)
    {
        var i = index * VerticesPerParticle;

        for (var corner = 0; corner < VerticesPerParticle; corner++)
        {
            Colours[i + corner] = colour;
        }
    }

    /// <summary>
    /// Points a particle at a frame of the sheet.
    /// </summary>
    protected void SetFrame(int index, int frame)
    {
        var i = index * VerticesPerParticle;
        var rect = Sheet.Frame(frame);

        TexCoords[i + 0] = new SKPoint(rect.Left, rect.Top);
        TexCoords[i + 1] = new SKPoint(rect.Right, rect.Top);
        TexCoords[i + 2] = new SKPoint(rect.Left, rect.Bottom);
        TexCoords[i + 3] = new SKPoint(rect.Right, rect.Top);
        TexCoords[i + 4] = new SKPoint(rect.Right, rect.Bottom);
        TexCoords[i + 5] = new SKPoint(rect.Left, rect.Bottom);
    }

    /// <summary>
    /// Builds a colour the way the effects ask for one, in hue and lightness.
    /// </summary>
    protected static SKColor FromHsl(double hue, double saturation, double lightness)
    {
        var colour = new Avalonia.Media.HslColor(1d, ((hue % 360d) + 360d) % 360d, saturation, lightness).ToRgb();

        return new SKColor(colour.R, colour.G, colour.B, colour.A);
    }
}
