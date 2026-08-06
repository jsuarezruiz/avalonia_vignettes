using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace ParallaxTravelCardsList.Controls;

/// <summary>
/// The hotel recommendations under the card strip. Port of <c>hotel_list.dart</c>.
/// </summary>
/// <remarks>
/// Swapping the hotels fades the rows back in over 700 ms, which is what tells the eye the list
/// belongs to the destination that just slid into view.
/// </remarks>
public sealed class HotelList : ItemsControl
{
    /// <summary>
    /// Defines the <see cref="ScreenHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ScreenHeightProperty =
        AvaloniaProperty.Register<HotelList, double>(nameof(ScreenHeight));

    /// <summary>
    /// Defines the <see cref="SectionHeight"/> property.
    /// </summary>
    public static readonly DirectProperty<HotelList, double> SectionHeightProperty =
        AvaloniaProperty.RegisterDirect<HotelList, double>(nameof(SectionHeight), o => o.SectionHeight);

    /// <summary>
    /// The section takes a quarter of the screen height, as in the Flutter original.
    /// </summary>
    private const double SectionHeightFactor = 0.25d;

    private static readonly Animation FadeIn = new()
    {
        Duration = TimeSpan.FromMilliseconds(700),
        FillMode = FillMode.Forward,
        Children =
        {
            new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(OpacityProperty, 0d) } },
            new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(OpacityProperty, 1d) } },
        },
    };

    private Control? _fadeHost;
    private CancellationTokenSource? _fadeCancellation;
    private double _sectionHeight;

    static HotelList()
    {
        ScreenHeightProperty.Changed.AddClassHandler<HotelList>((x, e) =>
            x.SectionHeight = e.GetNewValue<double>() * SectionHeightFactor);

        ItemsSourceProperty.Changed.AddClassHandler<HotelList>((x, _) => x.PlayFadeIn());
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
    /// Gets the height of the section, a quarter of <see cref="ScreenHeight"/>.
    /// </summary>
    public double SectionHeight
    {
        get => _sectionHeight;
        private set => SetAndRaise(SectionHeightProperty, ref _sectionHeight, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _fadeHost = e.NameScope.Find<Control>("PART_FadeHost");
        PlayFadeIn();
    }

    private void PlayFadeIn()
    {
        if (_fadeHost is null)
        {
            return;
        }

        _fadeCancellation?.Cancel();
        _fadeCancellation = new CancellationTokenSource();

        _ = FadeIn.RunAsync(_fadeHost, _fadeCancellation.Token);
    }
}
