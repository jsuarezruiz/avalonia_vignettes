using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AvaloniaVignettes.Shared.Controls;

/// <summary>
/// Draws one frame of a sprite sheet, stretched to fill the control. Port of the shared Flutter
/// package's <c>Sprite</c> and <c>AnimatedSprite</c>.
/// </summary>
/// <remarks>
/// <see cref="Frame"/> is a <see cref="double"/> rather than an integer so an animation can be
/// pointed straight at it; it is rounded to the nearest frame when drawn, exactly as the original
/// does.
/// </remarks>
public sealed class Sprite : Control
{
    public static readonly StyledProperty<Bitmap?> SourceProperty =
        AvaloniaProperty.Register<Sprite, Bitmap?>(nameof(Source));

    public static readonly StyledProperty<int> FrameWidthProperty =
        AvaloniaProperty.Register<Sprite, int>(nameof(FrameWidth), 1);

    public static readonly StyledProperty<int> FrameHeightProperty =
        AvaloniaProperty.Register<Sprite, int>(nameof(FrameHeight), 1);

    public static readonly StyledProperty<double> FrameProperty =
        AvaloniaProperty.Register<Sprite, double>(nameof(Frame));

    static Sprite() =>
        AffectsRender<Sprite>(SourceProperty, FrameWidthProperty, FrameHeightProperty, FrameProperty);

    /// <summary>
    /// Gets or sets the sheet the frames are cut from.
    /// </summary>
    public Bitmap? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of one frame within the sheet.
    /// </summary>
    public int FrameWidth
    {
        get => GetValue(FrameWidthProperty);
        set => SetValue(FrameWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of one frame within the sheet.
    /// </summary>
    public int FrameHeight
    {
        get => GetValue(FrameHeightProperty);
        set => SetValue(FrameHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets which frame to draw, counting across the sheet then down.
    /// </summary>
    public double Frame
    {
        get => GetValue(FrameProperty);
        set => SetValue(FrameProperty, value);
    }

    /// <summary>
    /// Draws one frame of a sheet into <paramref name="destination"/>, for scenes that place the
    /// sprite themselves rather than hosting a <see cref="Sprite"/> control.
    /// </summary>
    /// <param name="context">The context to draw into.</param>
    /// <param name="sheet">The sheet the frames are cut from.</param>
    /// <param name="frameWidth">The width of one frame within the sheet.</param>
    /// <param name="frameHeight">The height of one frame within the sheet.</param>
    /// <param name="frame">Which frame to draw, counting across the sheet then down.</param>
    /// <param name="destination">Where to draw it, the frame being stretched to fill.</param>
    public static void DrawFrame(
        DrawingContext context,
        Bitmap sheet,
        int frameWidth,
        int frameHeight,
        double frame,
        Rect destination)
    {
        if (frameWidth <= 0 || frameHeight <= 0)
        {
            return;
        }

        var columns = (int)(sheet.Size.Width / frameWidth);

        if (columns <= 0)
        {
            return;
        }

        var index = (int)Math.Round(frame);
        var source = new Rect(
            index % columns * (double)frameWidth,
            index / columns * (double)frameHeight,
            frameWidth,
            frameHeight);

        context.DrawImage(sheet, source, destination);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Source is { } sheet)
        {
            DrawFrame(context, sheet, FrameWidth, FrameHeight, Frame, new Rect(Bounds.Size));
        }
    }
}
