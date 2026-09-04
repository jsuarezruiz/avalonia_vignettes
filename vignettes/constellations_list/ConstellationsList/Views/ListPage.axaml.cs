using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using ConstellationsList.Controls;
using ConstellationsList.Models;

namespace ConstellationsList.Views;

/// <summary>
/// The scrolling guide. Port of <c>constellation_list_view.dart</c> and
/// <c>constellation_list_renderer.dart</c>.
/// </summary>
/// <remarks>
/// Entries alternate side and colour, and each is nudged outwards past the edge it hangs off, so the
/// list reads as two ragged columns rather than one tidy one. The indents come from a seeded
/// generator so they are the same on every run, the original notes its designers wanted them stable.
/// </remarks>
public partial class ListPage : UserControl
{
    private const double EdgeNudge = 25d;

    private const double VerticalPadding = 24d;

    private double _previousOffset;

    public ListPage()
    {
        InitializeComponent();

        Build();

        Scroller.ScrollChanged += OnScrollChanged;
    }

    /// <summary>
    /// Raised as the list scrolls, carrying how far it moved since the last report.
    /// </summary>
    public event EventHandler<double>? Scrolled;

    /// <summary>
    /// Raised when an entry is tapped, carrying the card that was hit.
    /// </summary>
    public event EventHandler<ConstellationTitleCard>? EntryTapped;

    private void Build()
    {
        // Exact Random(1).nextInt(4) * 20 sequence from the original Dart list. System.Random(1)
        // produces a different layout even though both are deterministic.
        double[] indents = [20, 0, 60, 60, 60, 60, 20, 0, 20, 0, 20, 40, 60, 20, 0];

        for (var index = 0; index < DemoData.Constellations.Count; index++)
        {
            var isRedMode = index % 2 == 1;

            // The design asked for the first entry to be left alone and the rest to vary.
            var indent = indents[index % indents.Length];

            var card = new ConstellationTitleCard
            {
                Constellation = DemoData.Constellations[index],
                IsRedMode = isRedMode,
            };

            var row = new Button
            {
                Content = card,
                Padding = new Thickness(
                    isRedMode ? 0d : indent,
                    VerticalPadding,
                    isRedMode ? indent : 0d,
                    VerticalPadding),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Background = Brushes.Transparent,
                BorderThickness = default,
                Cursor = new Cursor(StandardCursorType.Hand),
                RenderTransform = new TranslateTransform(isRedMode ? EdgeNudge : -EdgeNudge, 0d),
            };

            row.Classes.Add("constellationRow");
            card.HorizontalAlignment = isRedMode ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            row.Click += (_, _) => EntryTapped?.Invoke(this, card);

            Entries.Items.Add(row);
        }
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var offset = Scroller.Offset.Y;

        Scrolled?.Invoke(this, offset - _previousOffset);

        _previousOffset = offset;
    }
}
