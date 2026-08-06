using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaVignettes.Shared.Animation;
using SparkleParty.Effects;

namespace SparkleParty.Views;

/// <summary>
/// The party: a logo, four effects to pick between and a field of sparkles. Port of
/// <c>demo.dart</c>.
/// </summary>
/// <remarks>
/// Swapping effects fades the field out, builds the new one and fades it back in, so the change is
/// never seen. The caption fades away the first time the screen is touched and comes back with the
/// next effect, since each one is played differently.
/// </remarks>
public partial class MainView : UserControl
{
    /// <summary>
    /// Defines the <see cref="Instruction"/> property.
    /// </summary>
    public static readonly DirectProperty<MainView, string> InstructionProperty =
        AvaloniaProperty.RegisterDirect<MainView, string>(nameof(Instruction), o => o.Instruction);

    private static readonly string[] Instructions =
    [
        "TOUCH AND DRAG ON THE SCREEN",
        "TAP OR DRAG ON THE SCREEN",
        "DRAG ON THE SCREEN",
        "DRAG ON THE SCREEN",
    ];

    private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(350);
    private static readonly TimeSpan CaptionDuration = TimeSpan.FromMilliseconds(800);

    private readonly SpriteSheet _sheet = new("sparkleparty_spritesheet_2", 16, 64, 64);
    private readonly AnimationController _transition;
    private readonly AnimationController _caption;

    private string _instruction = Instructions[0];
    private int _index;
    private int _wanted;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        DataContext = this;

        _transition = new AnimationController(this, OnTransitionChanged) { Duration = TransitionDuration };
        _caption = new AnimationController(this, OnCaptionChanged) { Duration = CaptionDuration };

        Switcher.Picked += OnPicked;
    }

    /// <summary>
    /// Gets the line telling you how this effect is played.
    /// </summary>
    public string Instruction
    {
        get => _instruction;
        private set => SetAndRaise(InstructionProperty, ref _instruction, value);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var arranged = base.ArrangeOverride(finalSize);

        // The effects are built for the size they play in, so the first arrange starts them.
        if (Particles.Field is null && finalSize.Width > 0d && finalSize.Height > 0d)
        {
            Particles.Field = Build(_index, finalSize);
        }

        return arranged;
    }

    /// <summary>
    /// Switches to an effect, as picking its button does.
    /// </summary>
    public void Show(int index) => OnPicked(this, index);

    /// <summary>
    /// Puts the pointer somewhere, or takes it away.
    /// </summary>
    public void Touch(Point? point)
    {
        if (Particles.Field is { } field)
        {
            field.TouchPoint = point;
        }
    }

    private ParticleField Build(int index, Size size)
    {
        ParticleField field = index switch
        {
            1 => new Fireworks(_sheet, size),
            2 => new Comet(_sheet, size),
            3 => new Pinwheel(_sheet, size),
            _ => new Waterfall(_sheet, size),
        };

        field.Reset();

        return field;
    }

    private void OnPicked(object? sender, int index)
    {
        if (index == _index)
        {
            return;
        }

        _wanted = index;
        Switcher.Selected = index;

        _transition.SetValue(0d);
        _transition.Forward();
    }

    private void OnTransitionChanged(double progress)
    {
        Particles.Opacity = 1d - progress;

        // Once it is out of sight, swap the effect and bring it back.
        if (progress < 1d || _index == _wanted)
        {
            return;
        }

        _index = _wanted;
        Instruction = Instructions[_index];
        Particles.Field = Build(_index, Bounds.Size);

        _caption.Reverse();
        _transition.Reverse();
    }

    private void OnCaptionChanged(double progress) => Caption.Opacity = 1d - progress;

    private void OnTouched(object? sender, RoutedEventArgs e)
    {
        // The caption goes on the first touch and stays gone until the effect changes.
        if (!_caption.IsAnimating && _caption.Value < 1d)
        {
            _caption.Forward();
        }
    }
}
