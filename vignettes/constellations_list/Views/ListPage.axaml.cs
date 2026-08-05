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
    /// <summary>
    /// How far an entry is nudged past the edge it aligns to.
    /// </summary>
    private const double EdgeNudge = 25d;

    /// <summary>
    /// The gap above and below each entry.
    /// </summary>
    private const double VerticalPadding = 24d;

    private double _previousOffset;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListPage"/> class.
    /// </summary>
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
        // Seeded so the indents are identical on every run. The sequence differs from Dart's, since
        // the two runtimes' generators do not agree, but the intent, stable and varied, is kept.
        var random = new Random(1);

        for (var index = 0; index < DemoData.Constellations.Count; index++)
        {
            var isRedMode = index % 2 == 1;

            // The design asked for the first entry to be left alone and the rest to vary.
            var indent = index == 0 ? 20d : random.Next(4) * 20d;

            var card = new ConstellationTitleCard
            {
                Constellation = DemoData.Constellations[index],
                IsRedMode = isRedMode,
            };

            var row = new Border
            {
                Child = card,
                // The indent carries the nudge with it. Without that the transform shifts each row
                // outside the scroll presenter's clip, which crops the outermost 25px: the leading
                // letter of a left-hand entry, the trailing one of a right-hand entry.
                Padding = new Thickness(
                    isRedMode ? 0d : indent + EdgeNudge,
                    VerticalPadding,
                    isRedMode ? indent + EdgeNudge : 0d,
                    VerticalPadding),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand),
                RenderTransform = new TranslateTransform(isRedMode ? EdgeNudge : -EdgeNudge, 0d),
            };

            card.HorizontalAlignment = isRedMode ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            row.Tapped += (_, _) => EntryTapped?.Invoke(this, card);

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
