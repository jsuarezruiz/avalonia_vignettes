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
    /// <summary>
    /// Defines the <see cref="SliderValue"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SliderValueProperty =
        AvaloniaProperty.Register<MainView, double>(nameof(SliderValue));

    /// <summary>
    /// Defines the <see cref="TreatCount"/> property.
    /// </summary>
    public static readonly DirectProperty<MainView, int> TreatCountProperty =
        AvaloniaProperty.RegisterDirect<MainView, int>(nameof(TreatCount), o => o.TreatCount);

    /// <summary>
    /// Defines the <see cref="TotalLabel"/> property.
    /// </summary>
    public static readonly DirectProperty<MainView, string> TotalLabelProperty =
        AvaloniaProperty.RegisterDirect<MainView, string>(nameof(TotalLabel), o => o.TotalLabel);

    /// <summary>
    /// The most balls the slider can ask for.
    /// </summary>
    private const int MaxTreats = 10;

    /// <summary>
    /// What one ball costs.
    /// </summary>
    private const int PricePerTreat = 6;

    private int _treatCount;
    private string _totalLabel = "$0 CAD";

    static MainView() =>
        SliderValueProperty.Changed.AddClassHandler<MainView>((x, _) => x.UpdateTotals());

    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
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
        TreatCount = (int)Math.Round(SliderValue * MaxTreats);
        TotalLabel = $"${TreatCount * PricePerTreat} CAD";
    }
}
