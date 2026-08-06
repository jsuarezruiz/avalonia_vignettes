using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaVignettes.Shared.Animation;
using AvaloniaVignettes.Shared.Controls;

namespace ProductDetailZoom.Views;

/// <summary>
/// Holds both pages and runs the zoom between them. Stands in for Flutter's <c>Navigator</c>,
/// <c>FadeColorPageRoute</c> and <c>Hero</c> together.
/// </summary>
/// <remarks>
/// The route is three seconds long and spends the middle of it showing nothing but black, which is
/// what gives the speaker room to spin: the page being left is gone by 20% and the one being entered
/// does not arrive until 80%.
/// </remarks>
public partial class MainView : UserControl
{
    /// <summary>
    /// How long the zoom takes, from <c>FadeColorPageRoute</c>.
    /// </summary>
    private static readonly TimeSpan RouteDuration = TimeSpan.FromSeconds(3);

    /// <summary>
    /// How long the copy takes to slide aside, from <c>_transitionAnimController</c>.
    /// </summary>
    private static readonly TimeSpan CopySlideDuration = TimeSpan.FromMilliseconds(1200);

    /// <summary>
    /// How long to let the button fade before the route starts.
    /// </summary>
    private static readonly TimeSpan PressDelay = TimeSpan.FromMilliseconds(300);

    private readonly FadePageTransition _transition = new()
    {
        Duration = RouteDuration,
        PushFadeOutEnd = 0.2d,
        PushFadeInStart = 0.8d,
        PopFadeOutEnd = 0.2d,
        PopFadeInStart = 0.8d,
    };

    private DetailPage? _detail;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        FlightContent.SpriteSheet = ProductAssets.SpriteSheet;

        _transition.HeroFlight = Flight;
        Navigation.PageTransition = _transition;

        Product.ZoomRequested += OnZoomRequested;

        // The button is held back for a second so the speaker is the first thing to be looked at.
        FadeIn(Product.ZoomButton, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(350));
    }

    /// <summary>
    /// Fades a control in after a delay, as <c>DelayedFadeIn</c> does.
    /// </summary>
    private static void FadeIn(Visual target, TimeSpan delay, TimeSpan duration) =>
        DispatcherTimer.RunOnce(
            () => _ = new Animation
            {
                Duration = duration,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(OpacityProperty, 0d) } },
                    new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(OpacityProperty, 1d) } },
                },
            }.RunAsync(target),
            delay);

    private async void OnZoomRequested(object? sender, EventArgs e)
    {
        if (Navigation.IsNavigating || Navigation.CanGoBack)
        {
            return;
        }

        // The button goes first, then the copy slides aside, and only then does the route start,
        // so by the time the screen goes black there is nothing left on it to fade.
        Product.ZoomButton.Opacity = 0d;

        _ = SlideCopyAside();

        await Task.Delay(PressDelay);

        _detail = new DetailPage();
        _detail.ShrinkRequested += OnShrinkRequested;

        _transition.Prepare(_detail);

        // Brought back straight away, behind the hero that is now covering it, so it is already
        // there when the page returns rather than appearing once the return has finished.
        Product.ZoomButton.Opacity = 1d;

        // DelayedFadeIn runs from the moment the page is built, which is now, not from the moment
        // the route finishes, which is three seconds away.
        FadeIn(_detail.ShrinkButton, TimeSpan.FromMilliseconds(1530), TimeSpan.FromMilliseconds(170));

        await Navigation.PushAsync(_detail);
    }

    private async void OnShrinkRequested(object? sender, EventArgs e)
    {
        if (Navigation.IsNavigating || !Navigation.CanGoBack)
        {
            return;
        }

        await Navigation.PopAsync();

        if (_detail is not null)
        {
            _detail.ShrinkRequested -= OnShrinkRequested;
            _detail = null;
        }

        Product.CopySlide = 0d;
    }

    /// <summary>
    /// Nudges the copy a tenth of its width to the left over the second half of its run, which is
    /// just enough movement to read as the page giving way.
    /// </summary>
    private Task SlideCopyAside()
    {
        var animation = new Animation
        {
            Duration = CopySlideDuration,
            Easing = new IntervalEasing(0.5d, 1d, FlutterEasings.EaseOut),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(ProductPage.CopySlideProperty, 0d) },
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(ProductPage.CopySlideProperty, -0.1d) },
                },
            },
        };

        return animation.RunAsync(Product);
    }
}
