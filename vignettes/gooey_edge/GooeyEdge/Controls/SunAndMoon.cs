using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;

namespace GooeyEdge.Controls;

/// <summary>
/// The sun and moon that swing past the corners as the pages change. Port of <c>sun_moon.dart</c>.
/// </summary>
/// <remarks>
/// All three bodies sit on one large circle, far enough out that only the one nearest a corner is
/// on screen. Changing page turns the whole circle a third of a turn and crossfades to the next
/// body, so a sun appears to set as a moon rises.
/// </remarks>
public sealed class SunAndMoon : Panel
{
    public static readonly StyledProperty<int> IndexProperty =
        AvaloniaProperty.Register<SunAndMoon, int>(nameof(Index));

    public static readonly StyledProperty<bool> IsDragCompletedProperty =
        AvaloniaProperty.Register<SunAndMoon, bool>(nameof(IsDragCompleted));

    private const string ImageRoot = "avares://GooeyEdge/Assets/Images";

    private const double RotationRadius = 300d;

    private const double BodySize = 60d;

    private static readonly TimeSpan RotationDuration = TimeSpan.FromMilliseconds(350);
    private static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(300);

    private static readonly (string Asset, double Degrees)[] Bodies =
    [
        ("Sun-Yellow", 240d),
        ("Sun-Red", 30d),
        ("Moon-Crescent", 180d),
    ];

    public static readonly StyledProperty<double> RotationAngleProperty =
        AvaloniaProperty.Register<SunAndMoon, double>(nameof(RotationAngle));

    private readonly RotateTransform _rotation = new();
    private readonly Image[] _images = new Image[Bodies.Length];

    private int _currentIndex;

    static SunAndMoon()
    {
        IndexProperty.Changed.AddClassHandler<SunAndMoon>((x, _) => x.UpdateForIndex());
        IsDragCompletedProperty.Changed.AddClassHandler<SunAndMoon>((x, _) => x.UpdateForIndex());
        RotationAngleProperty.Changed.AddClassHandler<SunAndMoon>((x, e) => x._rotation.Angle = e.GetNewValue<double>());
    }

    public SunAndMoon()
    {
        IsHitTestVisible = false;
        RenderTransform = _rotation;
        RenderTransformOrigin = RelativePoint.Center;

        // Transforms are not Visuals, so the turn is animated through a property on the control and
        // mirrored onto the transform.
        Transitions =
        [
            new DoubleTransition
            {
                Property = RotationAngleProperty,
                Duration = RotationDuration,
                Easing = new CubicEaseOut(),
            },
        ];

        for (var i = 0; i < Bodies.Length; i++)
        {
            var (asset, degrees) = Bodies[i];
            var radians = degrees / 180d * Math.PI;

            var image = new Image
            {
                Source = new Bitmap(AssetLoader.Open(new Uri($"{ImageRoot}/{asset}.png"))),
                Width = BodySize,
                Height = BodySize,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Opacity = i == 0 ? 1d : 0d,
                RenderTransform = new TranslateTransform(
                    RotationRadius * Math.Cos(radians),
                    RotationRadius * Math.Sin(radians)),
                Transitions =
                [
                    new DoubleTransition { Property = OpacityProperty, Duration = FadeDuration },
                ],
            };

            _images[i] = image;
            Children.Add(image);
        }
    }

    /// <summary>
    /// Gets or sets the page index the bodies should be positioned for.
    /// </summary>
    public int Index
    {
        get => GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the swipe that changed the index committed.
    /// </summary>
    public bool IsDragCompleted
    {
        get => GetValue(IsDragCompletedProperty);
        set => SetValue(IsDragCompletedProperty, value);
    }

    /// <summary>
    /// Gets or sets the turn of the whole sky, in degrees.
    /// </summary>
    public double RotationAngle
    {
        get => GetValue(RotationAngleProperty);
        set => SetValue(RotationAngleProperty, value);
    }

    // Turns the circle a third of a turn per page and crossfades to the body that belongs to it.
    // Only a committed swipe moves it, so a swipe that springs back leaves the sky alone.
    private void UpdateForIndex()
    {
        if (!IsDragCompleted || Index == _currentIndex)
        {
            return;
        }

        _currentIndex = Index;

        // The Flutter original tweens 1 -> 0 turns, so a rising index turns the sky backwards.
        SetCurrentValue(RotationAngleProperty, -(_currentIndex / 3d) * 360d);

        var visible = ((_currentIndex % 3) + 3) % 3;

        for (var i = 0; i < _images.Length; i++)
        {
            _images[i].Opacity = i == visible ? 1d : 0d;
        }
    }
}
