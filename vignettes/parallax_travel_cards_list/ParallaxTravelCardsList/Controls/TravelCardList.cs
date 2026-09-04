using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvaloniaVignettes.Shared.Animation;
using AvaloniaVignettes.Shared.Controls;
using ParallaxTravelCardsList.Models;

namespace ParallaxTravelCardsList.Controls;

/// <summary>
/// The paged strip of travel cards. Port of <c>travel_card_list.dart</c>'s <c>TravelCardListState</c>.
/// </summary>
/// <remarks>
/// On top of the paging <see cref="Carousel"/> provides, this tracks a normalised drag offset:
/// every scroll notification nudges it by <c>delta * 0.01</c>, and lifting the pointer springs it
/// back to zero on an elastic curve. That single value drives both the 3D tilt of the cards and the
/// parallax of the artwork inside them.
/// </remarks>
public sealed class TravelCardList : ProgressCarousel
{
    public static readonly StyledProperty<double> ScreenWidthProperty =
        AvaloniaProperty.Register<TravelCardList, double>(nameof(ScreenWidth));

    public static readonly StyledProperty<double> ScreenHeightProperty =
        AvaloniaProperty.Register<TravelCardList, double>(nameof(ScreenHeight));

    public static readonly DirectProperty<TravelCardList, double> NormalizedOffsetProperty =
        AvaloniaProperty.RegisterDirect<TravelCardList, double>(nameof(NormalizedOffset), o => o.NormalizedOffset);

    public static readonly DirectProperty<TravelCardList, double> CardWidthProperty =
        AvaloniaProperty.RegisterDirect<TravelCardList, double>(nameof(CardWidth), o => o.CardWidth);

    public static readonly DirectProperty<TravelCardList, double> CardContentHeightProperty =
        AvaloniaProperty.RegisterDirect<TravelCardList, double>(nameof(CardContentHeight), o => o.CardContentHeight);

    public static readonly DirectProperty<TravelCardList, double> RotationYProperty =
        AvaloniaProperty.RegisterDirect<TravelCardList, double>(nameof(RotationY), o => o.RotationY);

    public static readonly DirectProperty<TravelCardList, City?> SelectedCityProperty =
        AvaloniaProperty.RegisterDirect<TravelCardList, City?>(nameof(SelectedCity), o => o.SelectedCity);

    private const double MaxRotation = 20d;

    private const double ScrollFactor = 0.01d;

    private static readonly TimeSpan SettleDuration = TimeSpan.FromMilliseconds(1000);
    private static readonly ElasticOutEasing SettleEasing = new();

    private FrameTicker? _settleTicker;
    private double _settleFrom;
    private double _normalizedOffset;
    private double _rotationY;
    private double _cardWidth = 320d;
    private double _cardContentHeight = 350d;
    private City? _selectedCity;

    static TravelCardList()
    {
        ScreenWidthProperty.Changed.AddClassHandler<TravelCardList>((x, _) => x.UpdateMetrics());
        ScreenHeightProperty.Changed.AddClassHandler<TravelCardList>((x, _) => x.UpdateMetrics());
        SelectedItemProperty.Changed.AddClassHandler<TravelCardList>((x, e) =>
            x.SelectedCity = e.GetNewValue<object?>() as City);
    }

    public TravelCardList()
    {
        ScrollStarted += OnScrollStarted;
        ScrollUpdated += OnScrollUpdated;
        DragReleased += OnDragReleased;
    }

    /// <summary>
    /// Gets or sets the width of the screen, the equivalent of <c>MediaQuery.size.width</c>.
    /// </summary>
    public double ScreenWidth
    {
        get => GetValue(ScreenWidthProperty);
        set => SetValue(ScreenWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the screen, the equivalent of <c>MediaQuery.size.height</c>.
    /// </summary>
    public double ScreenHeight
    {
        get => GetValue(ScreenHeightProperty);
        set => SetValue(ScreenHeightProperty, value);
    }

    /// <summary>
    /// Gets the drag offset, from -1 to 1, that drives the card tilt and the artwork parallax.
    /// </summary>
    public double NormalizedOffset
    {
        get => _normalizedOffset;
        private set
        {
            SetAndRaise(NormalizedOffsetProperty, ref _normalizedOffset, value);
            RotationY = value * MaxRotation;
        }
    }

    /// <summary>
    /// Gets the tilt, in degrees, the cards are currently rendered at.
    /// </summary>
    public double RotationY
    {
        get => _rotationY;
        private set => SetAndRaise(RotationYProperty, ref _rotationY, value);
    }

    /// <summary>
    /// Gets the destination whose card is centred, the equivalent of <c>onCityChange</c>.
    /// </summary>
    public City? SelectedCity
    {
        get => _selectedCity;
        private set => SetAndRaise(SelectedCityProperty, ref _selectedCity, value);
    }

    /// <summary>
    /// Gets the width of a single card.
    /// </summary>
    public double CardWidth
    {
        get => _cardWidth;
        private set => SetAndRaise(CardWidthProperty, ref _cardWidth, value);
    }

    /// <summary>
    /// Gets the height passed to each card's renderer, which is the page height less the 50 pixels
    /// the Flutter original reserves.
    /// </summary>
    public double CardContentHeight
    {
        get => _cardContentHeight;
        private set => SetAndRaise(CardContentHeightProperty, ref _cardContentHeight, value);
    }

    // Recomputes the card metrics from the screen size, matching the Flutter build method:
    // the page is 48% of the screen height clamped to 300..400, and a card is 80% as wide as it
    // is tall.
    private void UpdateMetrics()
    {
        if (ScreenWidth <= 0d || ScreenHeight <= 0d)
        {
            return;
        }

        var pageHeight = Math.Clamp(ScreenHeight * 0.48d, 300d, 400d);
        var pageWidth = pageHeight * 0.8d;

        Height = pageHeight;
        CardWidth = pageWidth;

        // Match the synced source's renderer height, rather than the older showcase GIF.
        CardContentHeight = pageHeight - 50d;

        // PageController takes a fraction of the viewport rather than a pixel width.
        ViewportFraction = pageWidth / ScreenWidth;
    }

    /// <summary>
    /// Forces the list into a fixed drag position so a frame can be rendered deterministically.
    /// Used only by the screenshot harness.
    /// </summary>
    internal void SetCaptureState(double page, double normalizedOffset)
    {
        StopSettle();

        SelectedIndex = Math.Clamp(
            (int)Math.Round(page, MidpointRounding.AwayFromZero),
            0,
            Math.Max(0, ItemCount - 1));

        // The native carousel panel scrolls in logical page units. Its public IScrollable surface
        // lets the capture harness hold it between pages without reimplementing carousel layout.
        if (ItemsPanelRoot is IScrollable scrollable)
        {
            scrollable.Offset = scrollable.Offset.WithX(page);
        }

        ReportPage(page);
        NormalizedOffset = normalizedOffset;
    }

    private void OnScrollStarted(object? sender, PageScrollEventArgs e) => StopSettle();

    private void OnScrollUpdated(object? sender, PageScrollEventArgs e)
    {
        if (IsDragging)
        {
            NormalizedOffset = Math.Clamp(NormalizedOffset + (e.Delta * ScrollFactor), -1d, 1d);
        }
    }

    private void OnDragReleased(object? sender, PageScrollEventArgs e)
    {
        if (Math.Abs(NormalizedOffset) < double.Epsilon)
        {
            return;
        }

        _settleFrom = NormalizedOffset;
        _settleTicker ??= new FrameTicker(this, OnSettleTick);
        _settleTicker.Start();
    }

    private void OnSettleTick(TimeSpan elapsed)
    {
        var progress = Math.Clamp(elapsed / SettleDuration, 0d, 1d);

        // The tween runs from wherever the drag left the offset back to zero.
        NormalizedOffset = _settleFrom * (1d - SettleEasing.Ease(progress));

        if (progress >= 1d)
        {
            StopSettle();
        }
    }

    private void StopSettle() => _settleTicker?.Stop();
}
