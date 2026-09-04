using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaVignettes.Shared.Controls;
using ParallaxTravelCardsHero.Controls;
using ParallaxTravelCardsHero.Models;

namespace ParallaxTravelCardsHero.Views;

/// <summary>
/// Hosts the navigation and the shared element that crosses it. Stands in for Flutter's
/// <c>Navigator</c>, <c>WhitePageRoute</c> and <c>Hero</c> together.
/// </summary>
/// <remarks>
/// The route itself is an ordinary Avalonia navigation: a <see cref="NavigationPage"/> and an
/// <see cref="Avalonia.Animation.IPageTransition"/>. Only the card's flight has to be built by hand,
/// because a page transition is handed the two pages and nothing finer, so there is no way to hand a
/// control from one to the other. See <see cref="HeroFlight"/>.
/// </remarks>
public partial class MainView : UserControl
{
    private readonly City _city = DemoData.City;
    private readonly FadePageTransition _transition = new();

    public MainView()
    {
        InitializeComponent();

        DataContext = _city;
        FlightCard.City = _city;

        _transition.HeroFlight = Flight;
        Navigation.PageTransition = _transition;

        List.OpenCardButton.Click += OnCardClicked;
        SizeChanged += (_, e) =>
        {
            if (_detail is not null)
            {
                _detail.ScreenWidth = e.NewSize.Width;
                _detail.ScreenHeight = e.NewSize.Height;
            }
        };
    }

    private DetailPage? _detail;

    /// <summary>
    /// Opens the card, so <c>--capture</c> can sample frames as the navigation plays.
    /// </summary>
    internal void BeginCapture() => List.OpenCardButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

    /// <summary>
    /// Goes back, so <c>--capture</c> can sample the return the same way.
    /// </summary>
    internal void BackCapture() => OnBackRequested(this, EventArgs.Empty);

    private async void OnCardClicked(object? sender, RoutedEventArgs e)
    {
        if (Navigation.IsNavigating || Navigation.CanGoBack)
        {
            return;
        }

        _detail = new DetailPage
        {
            DataContext = _city,
            ScreenWidth = Bounds.Width,
            ScreenHeight = Bounds.Height,
        };

        _detail.BackRequested += OnBackRequested;
        _transition.Prepare(_detail);

        await Navigation.PushAsync(_detail);
    }

    private async void OnBackRequested(object? sender, EventArgs e)
    {
        if (Navigation.IsNavigating || !Navigation.CanGoBack)
        {
            return;
        }

        await Navigation.PopAsync();

        if (_detail is not null)
        {
            _detail.BackRequested -= OnBackRequested;
            _detail = null;
        }
    }
}
