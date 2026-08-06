using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;
using AvaloniaVignettes.Shared.Media;

namespace ProductDetailZoom.Controls;

/// <summary>
/// Everything that arrives over the zoomed speaker: three callouts, and the headline that swings in
/// beneath them. Port of <c>product_details_transition.dart</c>.
/// </summary>
/// <remarks>
/// It is a pure function of <see cref="AnimationValue"/>, which is what lets the same control serve
/// as the settled detail page and as the overlay carried along by the flight. The callouts are held
/// back until the last third, the zoom is essentially over before the first line starts drawing,
/// and each then runs on its own slice of what remains.
/// </remarks>
public sealed class ProductDetailsOverlay : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="AnimationValue"/> property.
    /// </summary>
    public static readonly StyledProperty<double> AnimationValueProperty =
        AvaloniaProperty.Register<ProductDetailsOverlay, double>(nameof(AnimationValue), 1d);

    /// <summary>
    /// Defines the <see cref="FirstProgress"/> property.
    /// </summary>
    public static readonly DirectProperty<ProductDetailsOverlay, double> FirstProgressProperty =
        AvaloniaProperty.RegisterDirect<ProductDetailsOverlay, double>(nameof(FirstProgress), o => o.FirstProgress);

    /// <summary>
    /// Defines the <see cref="SecondProgress"/> property.
    /// </summary>
    public static readonly DirectProperty<ProductDetailsOverlay, double> SecondProgressProperty =
        AvaloniaProperty.RegisterDirect<ProductDetailsOverlay, double>(nameof(SecondProgress), o => o.SecondProgress);

    /// <summary>
    /// Defines the <see cref="ThirdProgress"/> property.
    /// </summary>
    public static readonly DirectProperty<ProductDetailsOverlay, double> ThirdProgressProperty =
        AvaloniaProperty.RegisterDirect<ProductDetailsOverlay, double>(nameof(ThirdProgress), o => o.ThirdProgress);

    /// <summary>
    /// Defines the <see cref="HeadlineOpacity"/> property.
    /// </summary>
    public static readonly DirectProperty<ProductDetailsOverlay, double> HeadlineOpacityProperty =
        AvaloniaProperty.RegisterDirect<ProductDetailsOverlay, double>(nameof(HeadlineOpacity), o => o.HeadlineOpacity);

    /// <summary>
    /// Defines the <see cref="HeadlineTransform"/> property.
    /// </summary>
    public static readonly DirectProperty<ProductDetailsOverlay, ITransform?> HeadlineTransformProperty =
        AvaloniaProperty.RegisterDirect<ProductDetailsOverlay, ITransform?>(
            nameof(HeadlineTransform), o => o.HeadlineTransform);

    private static readonly Easing Main = new IntervalEasing(0d, 0.8d, FlutterEasings.EaseOut);
    private static readonly Easing Linear = new IntervalEasing(0d, 0.8d);
    private static readonly Easing HeadlineFade = new IntervalEasing(0.2d, 1d);
    private static readonly Easing Callouts = new IntervalEasing(0.65d, 1d);
    private static readonly Easing FirstCallout = new IntervalEasing(0d, 0.85d);
    private static readonly Easing SecondCallout = new IntervalEasing(0.35d, 0.95d);
    private static readonly Easing ThirdCallout = new IntervalEasing(0.45d, 1d);

    private readonly TransformGroup _headline = new()
    {
        Children = { new ScaleTransform(), new TranslateTransform(), new MatrixTransform(Matrix.Identity) },
    };

    private Size _frame;
    private double _firstProgress;
    private double _secondProgress;
    private double _thirdProgress;
    private double _headlineOpacity;
    private ITransform? _headlineTransform;

    static ProductDetailsOverlay() =>
        AnimationValueProperty.Changed.AddClassHandler<ProductDetailsOverlay>((x, _) => x.Refresh());

    /// <summary>
    /// Gets or sets how far through the arrival this is, from 0 to 1.
    /// </summary>
    public double AnimationValue
    {
        get => GetValue(AnimationValueProperty);
        set => SetValue(AnimationValueProperty, value);
    }

    /// <summary>
    /// Gets how far along the topmost callout is.
    /// </summary>
    public double FirstProgress
    {
        get => _firstProgress;
        private set => SetAndRaise(FirstProgressProperty, ref _firstProgress, value);
    }

    /// <summary>
    /// Gets how far along the middle callout is.
    /// </summary>
    public double SecondProgress
    {
        get => _secondProgress;
        private set => SetAndRaise(SecondProgressProperty, ref _secondProgress, value);
    }

    /// <summary>
    /// Gets how far along the lowest callout is.
    /// </summary>
    public double ThirdProgress
    {
        get => _thirdProgress;
        private set => SetAndRaise(ThirdProgressProperty, ref _thirdProgress, value);
    }

    /// <summary>
    /// Gets the headline's opacity.
    /// </summary>
    public double HeadlineOpacity
    {
        get => _headlineOpacity;
        private set => SetAndRaise(HeadlineOpacityProperty, ref _headlineOpacity, value);
    }

    /// <summary>
    /// Gets the headline's combined scale, slide and swing.
    /// </summary>
    public ITransform? HeadlineTransform
    {
        get => _headlineTransform;
        private set => SetAndRaise(HeadlineTransformProperty, ref _headlineTransform, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductDetailsOverlay"/> class.
    /// </summary>
    public ProductDetailsOverlay()
    {
        _headlineTransform = _headline;
        Refresh();
    }

    private void Refresh()
    {
        var value = Math.Clamp(AnimationValue, 0d, 1d);
        var main = Main.Ease(value);

        var callouts = Callouts.Ease(value);

        FirstProgress = FirstCallout.Ease(callouts);
        SecondProgress = SecondCallout.Ease(callouts);
        ThirdProgress = ThirdCallout.Ease(callouts);

        HeadlineOpacity = HeadlineFade.Ease(main);

        var scale = 0.6d + (0.4d * main);

        // The slide is in fractions of the headline's own box, as a Flutter SlideTransition is.
        ((ScaleTransform)_headline.Children[0]).ScaleX = scale;
        ((ScaleTransform)_headline.Children[0]).ScaleY = scale;

        var translate = (TranslateTransform)_headline.Children[1];

        // The slide is in fractions of the headline's own box, and that box is the whole frame,
        // because the original's Stack expands its non-positioned children over the 300 x 500 the
        // headline asks for.
        translate.X = Lerp(0.6d, 0.1d, main) * _frame.Width;
        translate.Y = Lerp(0.7d, 0.95d, main) * _frame.Height;

        // A shallow swing towards the viewer, on the raw slice rather than the eased one.
        ((MatrixTransform)_headline.Children[2]).Matrix =
            Rotation3D.AroundY(Lerp(-0.09d, 0d, Linear.Ease(value)) * 180d / Math.PI, perspective: 0.01d);

        HeadlineTransform = _headline;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        // Taken from the size being arranged into, not from Bounds: Bounds still holds the previous
        // pass's size at this point, so reading it would leave the headline sliding by nothing.
        _frame = finalSize;

        var result = base.ArrangeOverride(finalSize);

        Refresh();

        return result;
    }

    private static double Lerp(double from, double to, double progress) => from + ((to - from) * progress);
}
