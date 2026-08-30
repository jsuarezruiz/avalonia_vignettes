using Avalonia.Interactivity;

namespace SpendingTracker.Controls;

/// <summary>
/// Says that the graph is being handled, or has stopped being handled. Port of
/// <c>interact_notification.dart</c>.
/// </summary>
/// <param name="routedEvent">The event being raised.</param>
/// <param name="ended">Whether the gesture has finished.</param>
public class InteractEventArgs(RoutedEvent routedEvent, bool ended) : RoutedEventArgs(routedEvent)
{
    /// <summary>
    /// Gets a value indicating whether the gesture has finished.
    /// </summary>
    public bool Ended { get; } = ended;
}
