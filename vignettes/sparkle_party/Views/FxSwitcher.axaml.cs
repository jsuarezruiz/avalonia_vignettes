using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SparkleParty.Views;

/// <summary>
/// The four effect buttons along the bottom. Port of <c>fx_switcher.dart</c>.
/// </summary>
public partial class FxSwitcher : UserControl
{
    public static readonly StyledProperty<int> SelectedProperty =
        AvaloniaProperty.Register<FxSwitcher, int>(nameof(Selected));

    private static readonly string[] Names = ["waterfall", "fireworks", "comet", "pinwheel"];

    private Image[] _icons = [];

    public FxSwitcher()
    {
        InitializeComponent();

        _icons = [Waterfall, Fireworks, Comet, Pinwheel];

        UpdateIcons();
    }

    /// <summary>
    /// Raised when one of the effects is picked.
    /// </summary>
    public event EventHandler<int>? Picked;

    /// <summary>
    /// Gets or sets which effect is showing.
    /// </summary>
    public int Selected
    {
        get => GetValue(SelectedProperty);
        set => SetValue(SelectedProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedProperty)
        {
            UpdateIcons();
        }
    }

    private static Bitmap Load(string name) =>
        new(AssetLoader.Open(new Uri($"avares://SparkleParty/Assets/Buttons/{name}.png")));

    private void UpdateIcons()
    {
        for (var i = 0; i < _icons.Length; i++)
        {
            _icons[i].Source = Load($"{Names[i]}-{(i == Selected ? "selected" : "idle")}");
        }
    }

    private void OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string tag } && int.TryParse(tag, out var index))
        {
            Picked?.Invoke(this, index);
        }
    }
}
