using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Animation;
using TicketFold.Controls;
using TicketFold.Models;

namespace TicketFold.Views;

/// <summary>
/// The list of boarding passes. Port of <c>demo.dart</c>.
/// </summary>
/// <remarks>
/// Folding a ticket open also scrolls the list, so the ticket that just grew lands just below the
/// top of the screen instead of pushing everything under it out of view. The scroll waits a quarter
/// of a second first, which lets the fold get going before the list starts to move.
/// </remarks>
public partial class MainView : UserControl
{
    private static readonly TimeSpan ScrollDuration = TimeSpan.FromSeconds(1);

    private static readonly Easing ScrollEasing =
        new IntervalEasing(0.25d, 1d, FlutterEasings.EaseOutQuad);

    private readonly AnimationController _scroll;

    private double _scrollFrom;
    private double _scrollTo;

    public MainView()
    {
        InitializeComponent();

        Tickets.ItemsSource = DemoData.BoardingPasses;

        _scroll = new AnimationController(this, OnScrollProgressChanged)
        {
            Duration = ScrollDuration,
        };

        AddHandler(Button.ClickEvent, OnTicketClicked);
    }

    private void OnTicketClicked(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Ticket ticket || IndexOf(ticket) is var index && index < 0)
        {
            return;
        }

        var openBefore = CountOpenBefore(index);

        // Measured with the round numbers the original uses rather than the real fold heights, so
        // the list settles exactly where it does there.
        var offset = (Ticket.NominalOpenHeight * openBefore)
            + (Ticket.NominalClosedHeight * (index - openBefore))
            - (Ticket.NominalClosedHeight * 0.5d);

        _scrollFrom = Scroller.Offset.Y;
        _scrollTo = Math.Max(0d, offset);

        _scroll.SetValue(0d);
        _scroll.Forward();
    }

    private void OnScrollProgressChanged(double progress)
    {
        var eased = ScrollEasing.Ease(progress);

        Scroller.Offset = Scroller.Offset.WithY(_scrollFrom + ((_scrollTo - _scrollFrom) * eased));
    }

    private int CountOpenBefore(int index)
    {
        var count = 0;

        for (var i = 0; i < index; i++)
        {
            if (FindTicket(i) is { IsOpen: true })
            {
                count++;
            }
        }

        return count;
    }

    private int IndexOf(Ticket ticket)
    {
        for (var i = 0; i < Tickets.ItemCount; i++)
        {
            if (FindTicket(i) == ticket)
            {
                return i;
            }
        }

        return -1;
    }

    private Ticket? FindTicket(int index) =>
        Tickets.ContainerFromIndex(index) is { } container
            ? container as Ticket ?? container.GetVisualDescendants().OfType<Ticket>().FirstOrDefault()
            : null;
}
