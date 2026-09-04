using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Animation;
using DrinkRewardsList.Controls;
using DrinkRewardsList.Models;

namespace DrinkRewardsList.Views;

/// <summary>
/// The rewards screen: a points header over a list of drinks, one of which can be open at a time.
/// Port of <c>demo.dart</c>.
/// </summary>
/// <remarks>
/// Everything in the header is sized from the header itself, which is a fifth of the screen. The
/// title, the star, the points and the caption are all fractions of that one number, so the whole
/// thing scales together.
/// </remarks>
public partial class MainView : UserControl
{
    public static readonly DirectProperty<MainView, HeaderMetrics> HeaderProperty =
        AvaloniaProperty.RegisterDirect<MainView, HeaderMetrics>(nameof(Header), o => o.Header);

    private const double HeaderFraction = 0.2d;

    private const double ListPaddingSize = 20d;

    private static readonly TimeSpan ScrollDuration = TimeSpan.FromMilliseconds(700);

    private readonly AnimationController _scroll;

    private HeaderMetrics _header = new(0d);
    private double _scrollFrom;
    private double _scrollTo;
    private Drink? _selected;

    public MainView()
    {
        InitializeComponent();

        DataContext = this;
        Drinks.ItemsSource = DemoData.Drinks;

        _scroll = new AnimationController(this, OnScrollProgressChanged) { Duration = ScrollDuration };

        AddHandler(Button.ClickEvent, OnCardClicked);
    }

    /// <summary>
    /// Gets how many points the customer has earned.
    /// </summary>
    public int EarnedPoints => DemoData.EarnedPoints;

    /// <summary>
    /// Gets every size in the header, all derived from its height.
    /// </summary>
    public HeaderMetrics Header
    {
        get => _header;
        private set => SetAndRaise(HeaderProperty, ref _header, value);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Header = new HeaderMetrics(finalSize.Height * HeaderFraction);

        return base.ArrangeOverride(finalSize);
    }

    private void OnCardClicked(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not DrinkCard card || card.Drink is not { } drink)
        {
            return;
        }

        // ToggleButton has already applied the user's choice by the time Click bubbles here.
        _selected = card.IsChecked == true ? drink : null;

        foreach (var other in this.GetVisualDescendants().OfType<DrinkCard>())
        {
            other.IsChecked = other.Drink is { } d && Equals(d, _selected);
        }

        if (_selected is null)
        {
            return;
        }

        // Stops a little short of the top, so the card that opened does not sit flush against the
        // header.
        var index = DemoData.IndexOf(drink);
        var pitch = DrinkCard.NominalHeightClosed + ListPaddingSize;

        _scrollFrom = Scroller.Offset.Y;
        _scrollTo = Math.Max(0d, (index * pitch) - (DrinkCard.NominalHeightClosed * 0.35d));

        _scroll.SetValue(0d);
        _scroll.Forward();
    }

    private void OnScrollProgressChanged(double progress)
    {
        var eased = FlutterEasings.EaseOutQuad.Ease(progress);

        Scroller.Offset = Scroller.Offset.WithY(_scrollFrom + ((_scrollTo - _scrollFrom) * eased));
    }
}
