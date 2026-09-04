using System.IO.Compression;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using AvaloniaVignettes.Shared.Animation;

namespace DogSlider.Controls;

/// <summary>
/// Draws the original Flare character as native Avalonia vector geometry. The original runtime
/// exports its deformed paths at 60 Hz; no hand-drawn replacement or bitmap scaling is involved.
/// </summary>
public sealed class DogView : Control
{
    public static readonly StyledProperty<bool> IsWalkingProperty =
        AvaloniaProperty.Register<DogView, bool>(nameof(IsWalking));

    public static readonly StyledProperty<bool> IsFlippedProperty =
        AvaloniaProperty.Register<DogView, bool>(nameof(IsFlipped));

    private static readonly Artwork Original = LoadArtwork();
    private Pose[][] _frames = Original.Sitting;
    private int _frame = Original.Sitting.Length - 1;
    private bool _playing;
    private TimeSpan? _started;
    private FrameTicker? _ticker;

    static DogView()
    {
        AffectsRender<DogView>(IsFlippedProperty);
        IsWalkingProperty.Changed.AddClassHandler<DogView>((view, _) => view.Play());
    }

    public bool IsWalking
    {
        get => GetValue(IsWalkingProperty);
        set => SetValue(IsWalkingProperty, value);
    }

    public bool IsFlipped
    {
        get => GetValue(IsFlippedProperty);
        set => SetValue(IsFlippedProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Bounds.Width <= 0d || Bounds.Height <= 0d) return;

        // FlareActor's BoxFit.fitWidth / Alignment.bottomCenter, including artboard padding.
        var scale = Bounds.Width / Original.Width;
        var transform = new Matrix(IsFlipped ? -scale : scale, 0d, 0d, scale,
            IsFlipped ? Bounds.Width : 0d, Bounds.Height - Original.Height * scale);
        using (context.PushTransform(transform))
        {
            foreach (var pose in _frames[_frame]) DrawPose(context, pose, 0);
        }
    }

    protected override Size MeasureOverride(Size availableSize) => new(100d, 100d);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _started = null;
        _ticker ??= new FrameTicker(this, OnTick);
        _ticker.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _ticker?.Stop();
    }

    private void Play()
    {
        _frames = IsWalking ? Original.Walking : Original.Sitting;
        _frame = 0;
        _started = null;
        _playing = true;
        InvalidateVisual();
    }

    private void OnTick(TimeSpan elapsed)
    {
        if (!_playing) return;
        _started ??= elapsed;
        var frame = (int)((elapsed - _started.Value).TotalSeconds * Original.Fps);
        if (IsWalking)
        {
            frame %= _frames.Length;
        }
        else if (frame >= _frames.Length - 1)
        {
            frame = _frames.Length - 1;
            _playing = false;
        }
        if (frame == _frame) return;
        _frame = frame;
        InvalidateVisual();
    }

    private static void DrawPose(DrawingContext context, Pose pose, int clipIndex)
    {
        if (clipIndex < pose.Clips.Length)
        {
            using (context.PushGeometryClip(pose.Clips[clipIndex]))
                DrawPose(context, pose, clipIndex + 1);
            return;
        }
        foreach (var fill in pose.Fills) context.DrawGeometry(fill, null, pose.Path);
        foreach (var stroke in pose.Strokes) context.DrawGeometry(null, stroke, pose.Path);
    }

    private static Artwork LoadArtwork()
    {
        using var asset = AssetLoader.Open(new Uri("avares://DogSlider/Assets/Animation/dog-poses.json.gz"));
        using var gzip = new GZipStream(asset, CompressionMode.Decompress);
        using var document = JsonDocument.Parse(gzip);
        var root = document.RootElement;
        var animations = root.GetProperty("animations");
        return new Artwork(root.GetProperty("width").GetDouble(), root.GetProperty("height").GetDouble(),
            root.GetProperty("fps").GetDouble(), ReadFrames(animations.GetProperty("walk")),
            ReadFrames(animations.GetProperty("sit-front")));
    }

    private static Pose[][] ReadFrames(JsonElement animation) =>
        animation.GetProperty("frames").EnumerateArray().Select(frame =>
            frame.EnumerateArray().Select(shape => new Pose(
                ReadPath(shape),
                shape.GetProperty("clips").EnumerateArray().Select(ReadPath).ToArray(),
                shape.GetProperty("fills").EnumerateArray()
                    .Select(fill => (IBrush)new SolidColorBrush(Color.Parse(fill.GetString()!))).ToArray(),
                shape.GetProperty("strokes").EnumerateArray().Select(stroke => (IPen)new Pen(
                    new SolidColorBrush(Color.Parse(stroke.GetProperty("color").GetString()!)),
                    stroke.GetProperty("width").GetDouble(),
                    lineCap: stroke.GetProperty("cap").GetString() switch
                    {
                        "round" => PenLineCap.Round, "square" => PenLineCap.Square, _ => PenLineCap.Flat,
                    },
                    lineJoin: stroke.GetProperty("join").GetString() switch
                    {
                        "round" => PenLineJoin.Round, "bevel" => PenLineJoin.Bevel, _ => PenLineJoin.Miter,
                    })).ToArray())).ToArray()).ToArray();

    private static Geometry ReadPath(JsonElement element)
    {
        var fillRule = element.GetProperty("evenOdd").GetBoolean() ? "F0 " : "F1 ";
        return StreamGeometry.Parse(fillRule + element.GetProperty("path").GetString());
    }

    private sealed record Pose(Geometry Path, Geometry[] Clips, IBrush[] Fills, IPen[] Strokes);
    private sealed record Artwork(double Width, double Height, double Fps, Pose[][] Walking, Pose[][] Sitting);
}
