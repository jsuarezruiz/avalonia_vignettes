using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaVignettes.Shared.Animation;

namespace DarkInkTransition.Controls;

/// <summary>
/// Reveals one child over another by bleeding ink across the screen. Port of
/// <c>transition_container.dart</c> and the shared <c>WidgetMask</c>.
/// </summary>
/// <remarks>
/// The ink is hand-drawn: a sheet of 40 frames, of which the run uses 34, played over the last 70%
/// of the transition after holding on the first frame for the opening 30%. The incoming child is
/// painted through that frame as an alpha mask, so the ink does not cover the old page; it cuts a
/// hole in the new one that grows.
/// <para>
/// The original composites this with <c>saveLayer</c> and <c>BlendMode.dstIn</c>. Avalonia says the
/// same thing with an opacity mask, and because the mask is a bitmap frame rather than a live
/// widget, an <see cref="ImageBrush"/> pointed at a source rectangle does it without a second
/// render pass.
/// </para>
/// </remarks>
public sealed class InkTransition : Decorator
{
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<InkTransition, double>(nameof(Progress));

    /// <summary>
    /// How long the reveal takes.
    /// </summary>
    public static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(1500);

    private const int Columns = 10;

    private const int FrameWidth = 360;

    private const int FrameHeight = 720;

    private static readonly TweenSequence Frames = new((0d, 0d, 30d), (0d, 34d, 70d));

    private static readonly Bitmap Sheet =
        new(AssetLoader.Open(new Uri("avares://DarkInkTransition/Assets/Images/ink_mask.png")));

    // One brush per frame, each over a cropped view of the sheet. Pointing a single brush at the
    // whole sheet and moving its source rectangle looks like the obvious way to do this, but the
    // rectangle is not honoured well enough to trust as a mask: the sheet ends up averaged across
    // the page, and an averaged mask is a half-transparent reveal rather than a clean one. Cropping
    // the frame leaves nothing to interpret.
    private static readonly Dictionary<int, ImageBrush> Masks = [];

    private ImageBrush? _mask;

    static InkTransition() =>
        ProgressProperty.Changed.AddClassHandler<InkTransition>((x, _) => x.Refresh());

    public InkTransition() => Refresh();

    /// <summary>
    /// Gets or sets how far through the reveal this is, from 0 to 1.
    /// </summary>
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    private void Refresh()
    {
        var frame = (int)Math.Round(Frames.Evaluate(Progress));

        if (!Masks.TryGetValue(frame, out var mask))
        {
            var source = new PixelRect(
                frame % Columns * FrameWidth,
                frame / Columns * FrameHeight,
                FrameWidth,
                FrameHeight);

            // Rendered out rather than referenced: DrawImage honours a source rectangle exactly,
            // where a brush's does not, and the result is a plain bitmap the brush cannot misread.
            var cut = new RenderTargetBitmap(new PixelSize(FrameWidth, FrameHeight), new Vector(96d, 96d));

            using (var context = cut.CreateDrawingContext())
            {
                context.DrawImage(
                    Sheet,
                    new Rect(source.X, source.Y, FrameWidth, FrameHeight),
                    new Rect(0d, 0d, FrameWidth, FrameHeight));
            }

            mask = new ImageBrush(cut) { Stretch = Stretch.Fill };
            Masks[frame] = mask;
        }

        if (!ReferenceEquals(_mask, mask))
        {
            _mask = mask;
            OpacityMask = mask;
        }
    }
}
