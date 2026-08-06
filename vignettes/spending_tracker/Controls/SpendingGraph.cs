using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using AvaloniaVignettes.Shared.Animation;
using SpendingTracker.Models;

namespace SpendingTracker.Controls;

/// <summary>
/// The chart itself: two curves over a month grid, with the picked month called out. Port of
/// <c>spending_graph.dart</c> and the two painters behind it, <c>chart_painter.dart</c> and
/// <c>chart_background_painter.dart</c>.
/// </summary>
/// <remarks>
/// The original is a stack of a <c>CustomPaint</c> and half a dozen positioned <c>Text</c>s, all of
/// them placed from numbers the painters have already worked out. Splitting that into a template
/// would mean publishing every one of those numbers as a property and binding it back to a
/// <c>Canvas</c>, so the labels are drawn here alongside the curves they belong to. The colours stay
/// here too, which is where the original keeps them.
/// <para>
/// The domain is dragged sideways and the marker is placed by tapping. A pinch zooms in the
/// original; on the desktop the wheel does it, a notch at a time.
/// </para>
/// </remarks>
public sealed class SpendingGraph : Control
{
    /// <summary>
    /// Defines the <see cref="Chart"/> property.
    /// </summary>
    public static readonly StyledProperty<Chart?> ChartProperty =
        AvaloniaProperty.Register<SpendingGraph, Chart?>(nameof(Chart));

    /// <summary>
    /// Defines the <see cref="FontFamily"/> property.
    /// </summary>
    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<SpendingGraph>();

    /// <summary>
    /// Defines the <see cref="Interacted"/> event.
    /// </summary>
    public static readonly RoutedEvent<InteractEventArgs> InteractedEvent =
        RoutedEvent.Register<SpendingGraph, InteractEventArgs>(nameof(Interacted), RoutingStrategies.Bubble);

    /// <summary>
    /// The height the graph is laid out against, before the app scale.
    /// </summary>
    private const double DesignHeight = 160d;

    /// <summary>
    /// The height of the plot itself, before the app scale.
    /// </summary>
    private const double DesignPlotHeight = 150d;

    /// <summary>
    /// How far the first month sits in from the left edge.
    /// </summary>
    private const double Inset = 28d;

    /// <summary>
    /// How far a pointer travels before a click is taken for a drag.
    /// </summary>
    private const double DragSlop = 3d;

    private static readonly string[] MonthNames =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    private static readonly string[] AxisLabels = ["$8k", "$6k", "$4k", "$2k", "0"];

    /// <summary>
    /// The colours the curves are stroked with, two per series.
    /// </summary>
    private static readonly Color[] LineColours =
        [Color.Parse("#FF4A78ED"), Color.Parse("#FF5DB391"), Color.Parse("#FFA74CBA"), Color.Parse("#FFF287A6")];

    /// <summary>
    /// The colours the areas under the curves are filled with, two per series.
    /// </summary>
    private static readonly Color[] FillColours =
        [Color.Parse("#4C4AC3E5"), Color.Parse("#005290C7"), Color.Parse("#4CDEACD0"), Color.Parse("#00DEACD0")];

    private static readonly ImmutablePen GridPen = new(new ImmutableSolidColorBrush(Color.Parse("#FF3D4666")));

    private static readonly IBrush AxisBrush = new ImmutableSolidColorBrush(Color.Parse("#FFC4C8D9"));

    private static readonly IBrush SelectedMonthBrush = new ImmutableSolidColorBrush(Color.Parse("#FFC3C8D9"));

    private static readonly Color LabelBackColour = Color.Parse("#252B40");

    private static readonly Color LabelTextColour = Color.Parse("#DCE2F5");

    private readonly AnimationController _selectedFade;
    private readonly GlowSprite _glow = new();

    private Chart? _subscribed;
    private Size _gradientSize;
    private IPen[] _linePens = [];
    private IBrush[] _fillBrushes = [];

    private bool _dragging;
    private Point _pressedAt;
    private Point _lastAt;

    static SpendingGraph()
    {
        AffectsRender<SpendingGraph>(ChartProperty, FontFamilyProperty);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpendingGraph"/> class.
    /// </summary>
    public SpendingGraph()
    {
        _selectedFade = new AnimationController(this, _ => InvalidateVisual())
        {
            Duration = TimeSpan.FromMilliseconds(600),
        };

        // The marker is already up when the vignette opens.
        _selectedFade.SetValue(1d);
    }

    /// <summary>
    /// Raised when a gesture on the graph starts and again when it finishes.
    /// </summary>
    public event EventHandler<InteractEventArgs> Interacted
    {
        add => AddHandler(InteractedEvent, value);
        remove => RemoveHandler(InteractedEvent, value);
    }

    /// <summary>
    /// Gets or sets the window on to the data being plotted.
    /// </summary>
    public Chart? Chart
    {
        get => GetValue(ChartProperty);
        set => SetValue(ChartProperty, value);
    }

    /// <summary>
    /// Gets or sets the family the labels are lettered in.
    /// </summary>
    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Gets the face the labels are lettered in, which is Flutter's w200 for this family.
    /// </summary>
    private Typeface Light => new(FontFamily, FontStyle.Normal, FontWeight.Light);

    /// <summary>
    /// Gets the face the picked month is lettered in.
    /// </summary>
    private Typeface Bold => new(FontFamily, FontStyle.Normal, FontWeight.Bold);

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Chart is not { } chart || Bounds.Width <= 0d)
        {
            return;
        }

        var width = Bounds.Width;
        var height = DesignPlotHeight * AppScale.Of(this);

        EnsureGradients(width, height);

        // Transparent, but drawn: a control is only hit tested where it puts something, and the
        // original's CustomPaint answers for the whole of the plot whether it painted there or not.
        context.FillRectangle(Brushes.Transparent, new Rect(new Size(width, height)));

        PaintGrid(context, chart, width, height);
        PaintMonths(context, chart, width, height);

        for (var index = 0; index < chart.DataSets.Count; index++)
        {
            PaintSeries(context, chart, index, width, height);
        }

        PaintMarker(context, chart, width, height);
        PaintAxis(context, width, height);
        PaintValues(context, chart, width, height);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) =>
        new(
            double.IsInfinity(availableSize.Width) ? 0d : availableSize.Width,
            DesignHeight * AppScale.Of(this));

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ChartProperty)
        {
            Subscribe(change.GetNewValue<Chart?>());
        }
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _pressedAt = _lastAt = e.GetPosition(this);
        _dragging = false;

        e.Pointer.Capture(this);
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!Equals(e.Pointer.Captured, this))
        {
            return;
        }

        var position = e.GetPosition(this);

        if (!_dragging && Math.Abs(position.X - _pressedAt.X) > DragSlop)
        {
            _dragging = true;

            RaiseEvent(new InteractEventArgs(InteractedEvent, ended: false));
        }

        if (_dragging)
        {
            Drag(position.X - _lastAt.X);
        }

        _lastAt = position;
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!Equals(e.Pointer.Captured, this))
        {
            return;
        }

        e.Pointer.Capture(null);

        if (_dragging)
        {
            _dragging = false;

            RaiseEvent(new InteractEventArgs(InteractedEvent, ended: true));
        }
        else
        {
            Tap(e.GetPosition(this));
        }
    }

    /// <inheritdoc />
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (e.Delta.Y != 0d)
        {
            Zoom(Math.Pow(1.1d, -e.Delta.Y));

            e.Handled = true;
        }
    }

    private static double Lerp(double from, double to, double progress) =>
        (from * (1d - progress)) + (to * progress);

    /// <summary>
    /// Draws the month gridlines, which step a whole month at a time.
    /// </summary>
    private static void PaintGrid(DrawingContext context, Chart chart, double width, double height)
    {
        var span = chart.DomainEnd - chart.DomainStart;
        var first = chart.RoundedDomainStart;

        for (var i = first; i <= chart.RoundedDomainEnd; i++)
        {
            var x = ((i - first) / span * width) + Inset;

            // The half pixel is the original's: the line leans a little as it falls.
            context.DrawLine(GridPen, new Point(x + 0.5d, 0d), new Point(x, height));
        }
    }

    /// <summary>
    /// Ensures the gradients match the size they are being painted at.
    /// </summary>
    private void EnsureGradients(double width, double height)
    {
        var size = new Size(width, height);

        if (_gradientSize == size && _linePens.Length > 0)
        {
            return;
        }

        _gradientSize = size;
        _linePens = new IPen[LineColours.Length / 2];
        _fillBrushes = new IBrush[FillColours.Length / 2];

        for (var index = 0; index < _linePens.Length; index++)
        {
            // Both gradients are laid out over the plot rather than over the shape they paint, so
            // they are given absolute ends. A relative one would follow the filled area's bounds,
            // which move with the curve.
            _linePens[index] = new Pen(
                new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0d, 0d, RelativeUnit.Absolute),
                    EndPoint = new RelativePoint(width, 0d, RelativeUnit.Absolute),
                    GradientStops =
                    {
                        new GradientStop(LineColours[index * 2], 0d),
                        new GradientStop(LineColours[(index * 2) + 1], 1d),
                    },
                },
                4d);

            _fillBrushes[index] = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0d, 0d, RelativeUnit.Absolute),
                EndPoint = new RelativePoint(0d, height, RelativeUnit.Absolute),
                GradientStops =
                {
                    new GradientStop(FillColours[index * 2], 0.5d),
                    new GradientStop(FillColours[(index * 2) + 1], 1d),
                },
            };
        }
    }

    /// <summary>
    /// Draws the month names under the plot, in the strip the original clips them to.
    /// </summary>
    private void PaintMonths(DrawingContext context, Chart chart, double width, double height)
    {
        var start = chart.DomainStart;
        var span = chart.DomainEnd - start;

        using var clip = context.PushClip(new Rect(16d, height, Math.Max(0d, width - 32d), 24d));

        for (var i = chart.RoundedDomainStart; i <= chart.RoundedDomainEnd; i++)
        {
            var selected = i == chart.SelectedDataPoint;
            var text = new FormattedText(
                MonthNames[i % 12],
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                selected ? Bold : Light,
                12d,
                selected ? SelectedMonthBrush : AxisBrush);

            context.DrawText(text, new Point(((i - start) / span * width) + Inset - 10d, height + 10d));
        }
    }

    /// <summary>
    /// Draws one series: the curve, then the area beneath it.
    /// </summary>
    private void PaintSeries(DrawingContext context, Chart chart, int index, double width, double height)
    {
        var curve = new StreamGeometry();

        using (var sink = curve.Open())
        {
            Trace(sink, chart, index, width, height, close: false);
        }

        context.DrawGeometry(null, _linePens[index], curve);

        var area = new StreamGeometry();

        using (var sink = area.Open())
        {
            Trace(sink, chart, index, width, height, close: true);
        }

        context.DrawGeometry(_fillBrushes[index], null, area);
    }

    /// <summary>
    /// Traces one series into <paramref name="sink"/>, closing it down to the axis when asked.
    /// </summary>
    private void Trace(
        StreamGeometryContext sink,
        Chart chart,
        int index,
        double width,
        double height,
        bool close)
    {
        var start = chart.DomainStart;
        var end = chart.DomainEnd;
        var span = end - start;
        var values = chart.DataSets[index].Values;
        var first = (int)Math.Floor(start);
        var last = (int)Math.Ceiling(end);

        Point At(int month) => new(
            ((month - start) / span * width) + Inset,
            height - ((values[month] - chart.RangeStart) / chart.RangeEnd * height));

        // The curve is drawn from one month before the window where there is one, so its left edge
        // arrives at the right angle rather than starting flat.
        var previous = At(first > 0 ? first - 1 : 0);

        sink.BeginFigure(first > 0 ? previous : new Point(0d, previous.Y), isFilled: true);

        for (var month = first; month < last; month++)
        {
            previous = CurveTo(sink, previous, At(month));
        }

        if (last < (int)Math.Floor(chart.MaxDomain))
        {
            CurveTo(sink, previous, At(last));
        }
        else
        {
            // Nothing left to reach for, so the last month runs flat off the right edge.
            sink.LineTo(new Point(width, At(last - 1).Y));
        }

        if (close)
        {
            sink.LineTo(new Point(width, height));
            sink.LineTo(new Point(0d, height));
            sink.LineTo(new Point(0d, At(0).Y));
        }

        sink.EndFigure(close);

        static Point CurveTo(StreamGeometryContext sink, Point from, Point to)
        {
            // Both handles sit half way across, which keeps the curve flat where it meets a month.
            var middle = Lerp(from.X, to.X, 0.5d);

            sink.CubicBezierTo(new Point(middle, from.Y), new Point(middle, to.Y), to);

            return to;
        }
    }

    /// <summary>
    /// Draws the line and the two halos calling out the picked month.
    /// </summary>
    private void PaintMarker(DrawingContext context, Chart chart, double width, double height)
    {
        if (chart.SelectedDataPoint == -1)
        {
            return;
        }

        var fade = _selectedFade.Value;
        var brush = new ImmutableSolidColorBrush(Colors.White, fade);
        var x = ((chart.SelectedDataPoint - chart.DomainStart) / (chart.DomainEnd - chart.DomainStart) * width) + Inset;
        var highest = 0d;

        for (var index = 0; index < chart.DataSets.Count; index++)
        {
            highest = Math.Max(highest, chart.SelectedY(index));
        }

        context.DrawLine(new ImmutablePen(brush), new Point(x, height), new Point(x, height - (highest * height)));

        for (var index = 0; index < chart.DataSets.Count; index++)
        {
            var y = height - (chart.SelectedY(index) * height);

            using (context.PushRenderOptions(new RenderOptions
            {
                BitmapBlendingMode = BitmapBlendingMode.Plus,
                BitmapInterpolationMode = BitmapInterpolationMode.HighQuality,
            }))
            {
                context.DrawImage(_glow.For(fade), new Rect(x - 12d, y - 12d, 24d, 24d));
            }

            context.DrawEllipse(brush, null, new Point(x, y), 4.8d, 4.8d);
        }
    }

    /// <summary>
    /// Draws the two columns of y axis labels, one down each edge.
    /// </summary>
    private void PaintAxis(DrawingContext context, double width, double height)
    {
        var labels = Array.ConvertAll(
            AxisLabels,
            label => new FormattedText(
                label,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Light,
                12d,
                AxisBrush));

        // The column spreads the labels out with the first against the top and the last against the
        // bottom, which is what Flutter's spaceBetween does.
        var step = (height - labels[0].Height) / (labels.Length - 1);

        for (var index = 0; index < labels.Length; index++)
        {
            context.DrawText(labels[index], new Point(4d, index * step));
            context.DrawText(labels[index], new Point(width - 24d, index * step));
        }
    }

    /// <summary>
    /// Draws the two figures the picked month is worth.
    /// </summary>
    private void PaintValues(DrawingContext context, Chart chart, double width, double height)
    {
        if (chart.SelectedDataPoint == -1)
        {
            return;
        }

        var fade = _selectedFade.Value;
        var first = (height * (1d - chart.SelectedY(0))) + 10d;
        var second = (height * (1d - chart.SelectedY(1))) + 10d;

        // Two figures on the same level would sit on top of each other, so the lower one drops.
        if (Math.Abs(second - first) < 12d)
        {
            second += 24d;
        }

        var x = (width * chart.SelectedX()) + 8d;

        PaintValue(context, chart, 0, x, first, fade);
        PaintValue(context, chart, 1, x, second, fade);
    }

    private void PaintValue(DrawingContext context, Chart chart, int index, double x, double y, double fade)
    {
        var text = new FormattedText(
            Money.Format((int)Math.Round(
                chart.DataSets[index].Values[chart.SelectedDataPoint] * 1000d,
                MidpointRounding.AwayFromZero)),
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Light,
            10d,
            new ImmutableSolidColorBrush(LabelTextColour, fade))
        {
            TextAlignment = TextAlignment.Center,
            MaxTextWidth = 32d,
        };

        var background = new ImmutableSolidColorBrush(LabelBackColour, Math.Min(fade, 0.6d));

        context.FillRectangle(background, new Rect(x, y, 40d, text.Height + 8d));
        context.DrawText(text, new Point(x + 4d, y + 4d));
    }

    private void Subscribe(Chart? chart)
    {
        if (_subscribed is { } previous)
        {
            previous.PropertyChanged -= OnChartChanged;
        }

        _subscribed = chart;

        if (chart is not null)
        {
            chart.PropertyChanged += OnChartChanged;
        }
    }

    private void OnChartChanged(object? sender, PropertyChangedEventArgs e) => InvalidateVisual();

    private void Tap(Point position)
    {
        if (Chart is not { } chart)
        {
            return;
        }

        var offset = Lerp(chart.DomainStart, chart.DomainEnd, (position.X - Inset) / Bounds.Width);

        if (Models.Chart.Round(offset) == chart.SelectedDataPoint)
        {
            return;
        }

        chart.SelectedDataPoint = Models.Chart.Round(offset);

        _selectedFade.SetValue(0d);
        _selectedFade.Forward();
    }

    private void Drag(double delta)
    {
        if (Chart is not { } chart)
        {
            return;
        }

        var step = -delta / 200d * (chart.DomainEnd - chart.DomainStart);

        if (chart.DomainStart + step < 0d || chart.DomainEnd + step >= chart.MaxDomain)
        {
            return;
        }

        // Whichever end is moving into new ground goes first, so it is never clamped by the other.
        if (step < 0d)
        {
            chart.DomainStart += step;
            chart.DomainEnd += step;
        }
        else
        {
            chart.DomainEnd += step;
            chart.DomainStart += step;
        }
    }

    private void Zoom(double factor)
    {
        if (Chart is not { } chart)
        {
            return;
        }

        var span = chart.DomainEnd - chart.DomainStart;
        var step = (span * factor) - span;

        if (span + step < 13d)
        {
            if (chart.DomainEnd != chart.MaxDomain)
            {
                chart.DomainEnd += step;
            }
            else
            {
                chart.DomainStart -= step;
            }
        }

        _selectedFade.Reverse();

        RaiseEvent(new InteractEventArgs(InteractedEvent, ended: true));
    }
}
