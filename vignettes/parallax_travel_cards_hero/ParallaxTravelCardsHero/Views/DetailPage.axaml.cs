using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaVignettes.Shared.Controls;
using ParallaxTravelCardsHero.Controls;

namespace ParallaxTravelCardsHero.Views;

/// <summary>
/// The page behind the card: the scene it opened into, a write-up, and the experiences on offer.
/// Port of <c>city_details_page.dart</c>.
/// </summary>
public partial class DetailPage : ContentPage, IHeroPage
{
    public static readonly StyledProperty<double> ScreenWidthProperty =
        AvaloniaProperty.Register<DetailPage, double>(nameof(ScreenWidth));

    public static readonly StyledProperty<double> ScreenHeightProperty =
        AvaloniaProperty.Register<DetailPage, double>(nameof(ScreenHeight));

    public static readonly DirectProperty<DetailPage, double> ExperiencesHeightProperty =
        AvaloniaProperty.RegisterDirect<DetailPage, double>(nameof(ExperiencesHeight), o => o.ExperiencesHeight);

    public static readonly DirectProperty<DetailPage, double> ExperienceWidthProperty =
        AvaloniaProperty.RegisterDirect<DetailPage, double>(nameof(ExperienceWidth), o => o.ExperienceWidth);

    private const double ExperiencesHeightFactor = 0.15d;

    private const double ExperienceWidthFactor = 0.3d;

    private double _experiencesHeight;
    private double _experienceWidth;

    public DetailPage() => InitializeComponent();

    static DetailPage()
    {
        ScreenWidthProperty.Changed.AddClassHandler<DetailPage>((x, e) =>
            x.ExperienceWidth = e.GetNewValue<double>() * ExperienceWidthFactor);

        ScreenHeightProperty.Changed.AddClassHandler<DetailPage>((x, e) =>
            x.ExperiencesHeight = e.GetNewValue<double>() * ExperiencesHeightFactor);
    }

    /// <summary>
    /// Raised when the back button is pressed.
    /// </summary>
    public event EventHandler? BackRequested;

    /// <summary>
    /// Gets or sets the screen width, the equivalent of <c>MediaQuery.size.width</c>.
    /// </summary>
    public double ScreenWidth
    {
        get => GetValue(ScreenWidthProperty);
        set => SetValue(ScreenWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the screen height, the equivalent of <c>MediaQuery.size.height</c>.
    /// </summary>
    public double ScreenHeight
    {
        get => GetValue(ScreenHeightProperty);
        set => SetValue(ScreenHeightProperty, value);
    }

    /// <summary>
    /// Gets the height of the row of experience cards.
    /// </summary>
    public double ExperiencesHeight
    {
        get => _experiencesHeight;
        private set => SetAndRaise(ExperiencesHeightProperty, ref _experiencesHeight, value);
    }

    /// <summary>
    /// Gets the width of one experience card, before its margin.
    /// </summary>
    public double ExperienceWidth
    {
        get => _experienceWidth;
        private set => SetAndRaise(ExperienceWidthProperty, ref _experienceWidth, value);
    }

    /// <summary>
    /// Gets the open scene, which is the end of the flight that lives on this page.
    /// </summary>
    public Control Hero => Scene;

    private void OnBackClick(object? sender, RoutedEventArgs e) => BackRequested?.Invoke(this, EventArgs.Empty);
}
