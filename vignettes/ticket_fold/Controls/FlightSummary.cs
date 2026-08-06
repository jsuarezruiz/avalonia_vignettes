using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;
using TicketFold.Models;

namespace TicketFold.Controls;

/// <summary>
/// Which way round a <see cref="FlightSummary"/> is printed.
/// </summary>
public enum SummaryPalette
{
    /// <summary>
    /// Dark ink on white, the face on the outside of a folded ticket.
    /// </summary>
    Light,

    /// <summary>
    /// White ink on the blue artwork, the face revealed when the ticket opens.
    /// </summary>
    Dark,
}

/// <summary>
/// The summary face of a boarding pass: airline, passenger, and the route from origin to
/// destination. Port of <c>flight_summary.dart</c>.
/// </summary>
/// <remarks>
/// The dark face flies its plane along the route each time the ticket is opened, taking far longer
/// than the fold itself so the motion carries on after the ticket has settled.
/// </remarks>
public sealed class FlightSummary : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="BoardingPass"/> property.
    /// </summary>
    public static readonly StyledProperty<BoardingPass?> BoardingPassProperty =
        AvaloniaProperty.Register<FlightSummary, BoardingPass?>(nameof(BoardingPass));

    /// <summary>
    /// Defines the <see cref="Palette"/> property.
    /// </summary>
    public static readonly StyledProperty<SummaryPalette> PaletteProperty =
        AvaloniaProperty.Register<FlightSummary, SummaryPalette>(nameof(Palette));

    /// <summary>
    /// Defines the <see cref="IsOpen"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<FlightSummary, bool>(nameof(IsOpen));

    /// <summary>
    /// Defines the <see cref="PassengerLabel"/> property.
    /// </summary>
    public static readonly DirectProperty<FlightSummary, string?> PassengerLabelProperty =
        AvaloniaProperty.RegisterDirect<FlightSummary, string?>(nameof(PassengerLabel), o => o.PassengerLabel);

    /// <summary>
    /// Defines the <see cref="BoardingLabel"/> property.
    /// </summary>
    public static readonly DirectProperty<FlightSummary, string?> BoardingLabelProperty =
        AvaloniaProperty.RegisterDirect<FlightSummary, string?>(nameof(BoardingLabel), o => o.BoardingLabel);

    /// <summary>
    /// Defines the <see cref="OriginCode"/> property.
    /// </summary>
    public static readonly DirectProperty<FlightSummary, string?> OriginCodeProperty =
        AvaloniaProperty.RegisterDirect<FlightSummary, string?>(nameof(OriginCode), o => o.OriginCode);

    /// <summary>
    /// Defines the <see cref="DestinationCode"/> property.
    /// </summary>
    public static readonly DirectProperty<FlightSummary, string?> DestinationCodeProperty =
        AvaloniaProperty.RegisterDirect<FlightSummary, string?>(nameof(DestinationCode), o => o.DestinationCode);

    /// <summary>
    /// Defines the <see cref="DurationLabel"/> property.
    /// </summary>
    public static readonly DirectProperty<FlightSummary, string?> DurationLabelProperty =
        AvaloniaProperty.RegisterDirect<FlightSummary, string?>(nameof(DurationLabel), o => o.DurationLabel);

    /// <summary>
    /// The height the plane is drawn at, and so the unit its slide is measured in.
    /// </summary>
    private const double PlaneHeight = 20d;

    /// <summary>
    /// The aspect ratio of the plane artwork, 56 by 53.
    /// </summary>
    private const double PlaneAspectRatio = 56d / 53d;

    /// <summary>
    /// Where the plane starts, in multiples of its own width.
    /// </summary>
    private const double PlaneSlideFrom = -2d;

    /// <summary>
    /// Where the plane finishes, in multiples of its own width.
    /// </summary>
    private const double PlaneSlideTo = 1d;

    private static readonly TimeSpan PlaneSlideDuration = TimeSpan.FromMilliseconds(1700);

    private readonly TranslateTransform _planeTransform = new();
    private readonly AnimationController _planeSlide;

    private string? _passengerLabel;
    private string? _boardingLabel;
    private string? _originCode;
    private string? _destinationCode;
    private string? _durationLabel;
    private Image? _plane;

    static FlightSummary()
    {
        BoardingPassProperty.Changed.AddClassHandler<FlightSummary>((x, _) => x.UpdateLabels());
        IsOpenProperty.Changed.AddClassHandler<FlightSummary>((x, e) => x.OnIsOpenChanged(e));
        PaletteProperty.Changed.AddClassHandler<FlightSummary>((x, _) => x.OnPlaneSlideChanged(x._planeSlide.Value));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlightSummary"/> class.
    /// </summary>
    public FlightSummary()
    {
        _planeSlide = new AnimationController(this, OnPlaneSlideChanged)
        {
            Duration = PlaneSlideDuration,
        };
    }

    /// <summary>
    /// Gets or sets the boarding pass being shown.
    /// </summary>
    public BoardingPass? BoardingPass
    {
        get => GetValue(BoardingPassProperty);
        set => SetValue(BoardingPassProperty, value);
    }

    /// <summary>
    /// Gets or sets which way round the face is printed.
    /// </summary>
    public SummaryPalette Palette
    {
        get => GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the ticket this face belongs to is open. Turning it
    /// on sends the plane off along its route again.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets the passenger's name, as printed.
    /// </summary>
    public string? PassengerLabel
    {
        get => _passengerLabel;
        private set => SetAndRaise(PassengerLabelProperty, ref _passengerLabel, value);
    }

    /// <summary>
    /// Gets the boarding time, as printed.
    /// </summary>
    public string? BoardingLabel
    {
        get => _boardingLabel;
        private set => SetAndRaise(BoardingLabelProperty, ref _boardingLabel, value);
    }

    /// <summary>
    /// Gets the origin airport code, as printed.
    /// </summary>
    public string? OriginCode
    {
        get => _originCode;
        private set => SetAndRaise(OriginCodeProperty, ref _originCode, value);
    }

    /// <summary>
    /// Gets the destination airport code, as printed.
    /// </summary>
    public string? DestinationCode
    {
        get => _destinationCode;
        private set => SetAndRaise(DestinationCodeProperty, ref _destinationCode, value);
    }

    /// <summary>
    /// Gets the flight duration, as printed.
    /// </summary>
    public string? DurationLabel
    {
        get => _durationLabel;
        private set => SetAndRaise(DurationLabelProperty, ref _durationLabel, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _plane = e.NameScope.Find<Image>("PART_Plane");

        if (_plane is not null)
        {
            _plane.RenderTransform = _planeTransform;
            OnPlaneSlideChanged(_planeSlide.Value);
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _planeSlide.Stop();
    }

    private void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!e.GetNewValue<bool>())
        {
            return;
        }

        // Opening always restarts the slide from the beginning, so a ticket opened twice sends the
        // plane across twice. Closing leaves it wherever it got to, out of sight behind the fold.
        _planeSlide.SetValue(0d);
        _planeSlide.Forward();
    }

    private void OnPlaneSlideChanged(double progress)
    {
        // Only the dark face flies its plane. On the light face the original draws the image
        // plainly, so it stays put on the middle of the route.
        if (Palette is not SummaryPalette.Dark)
        {
            _planeTransform.X = 0d;
            return;
        }

        var width = _plane?.Bounds.Width is > 0d ? _plane.Bounds.Width : PlaneHeight * PlaneAspectRatio;
        var offset = PlaneSlideFrom + ((PlaneSlideTo - PlaneSlideFrom) * FlutterEasings.EaseOutQuad.Ease(progress));

        _planeTransform.X = offset * width;
    }

    private void UpdateLabels()
    {
        if (BoardingPass is not { } pass)
        {
            PassengerLabel = BoardingLabel = OriginCode = DestinationCode = DurationLabel = null;
            return;
        }

        PassengerLabel = pass.PassengerName.ToUpperInvariant();

        // Flutter formats the boarding time through MaterialLocalizations, which is 'h:mm a' for the
        // en_US locale the vignette runs in.
        BoardingLabel = $"BOARDING {pass.BoardingTime.ToString("h:mm tt", CultureInfo.InvariantCulture)}";

        OriginCode = pass.Origin.Code.ToUpperInvariant();
        DestinationCode = pass.Destination.Code.ToUpperInvariant();
        DurationLabel = pass.Duration.ToString();
    }
}
