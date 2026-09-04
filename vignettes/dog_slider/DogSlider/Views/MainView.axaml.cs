using Avalonia;
using Avalonia.Controls;

namespace DogSlider.Views;

/// <summary>
/// The product page the slider sits on. Port of <c>demo.dart</c>.
/// </summary>
/// <remarks>
/// The slider reports a fraction; everything else on the page is derived from it: the count, and
/// the running total at six dollars a ball.
/// </remarks>
public partial class MainView : UserControl
{
    public static readonly StyledProperty<double> SliderValueProperty =
        AvaloniaProperty.Register<MainView, double>(nameof(SliderValue));

    public static readonly DirectProperty<MainView, int> TreatCountProperty =
        AvaloniaProperty.RegisterDirect<MainView, int>(nameof(TreatCount), o => o.TreatCount);

    public static readonly DirectProperty<MainView, string> TotalLabelProperty =
        AvaloniaProperty.RegisterDirect<MainView, string>(nameof(TotalLabel), o => o.TotalLabel);

    private const int MaxTreats = 10;

    private const int PricePerTreat = 6;

    private int _treatCount;
    private string _totalLabel = "$0 CAD";

    static MainView() =>
        SliderValueProperty.Changed.AddClassHandler<MainView>((x, _) => x.UpdateTotals());

    public MainView()
    {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>
    /// Gets or sets how far along the slider is, from 0 to 1.
    /// </summary>
    public double SliderValue
    {
        get => GetValue(SliderValueProperty);
        set => SetValue(SliderValueProperty, value);
    }

    /// <summary>
    /// Gets how many balls that comes to.
    /// </summary>
    public int TreatCount
    {
        get => _treatCount;
        private set => SetAndRaise(TreatCountProperty, ref _treatCount, value);
    }

    /// <summary>
    /// Gets the running total, as the cart bar prints it.
    /// </summary>
    public string TotalLabel
    {
        get => _totalLabel;
        private set => SetAndRaise(TotalLabelProperty, ref _totalLabel, value);
    }

    private void UpdateTotals()
    {
        TreatCount = (int)Math.Round(SliderValue * MaxTreats, MidpointRounding.AwayFromZero);
        TotalLabel = $"${TreatCount * PricePerTreat} CAD";
    }
}
