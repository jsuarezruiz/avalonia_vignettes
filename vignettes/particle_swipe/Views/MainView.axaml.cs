using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ParticleSwipe.Controls;
using ParticleSwipe.Models;

namespace ParticleSwipe.Views;

/// <summary>
/// The inbox, with the particle layer over it. Port of <c>demo.dart</c>.
/// </summary>
/// <remarks>
/// The particles are thrown from where the row is on screen, so the position has to be read before
/// the row starts collapsing. A delete waits a moment before firing so the row is mostly closed by
/// the time its particles appear, which reads as the row bursting rather than as the burst pushing
/// it out of the way.
/// </remarks>
public partial class MainView : UserControl
{
    /// <summary>
    /// How long a delete waits before its particles are thrown.
    /// </summary>
    private static readonly TimeSpan ExplosionDelay = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Where a favourite burst is centred, measured from the row's top left corner.
    /// </summary>
    private static readonly Point FavoriteBurstOffset = new(60d, 46d);

    private readonly ObservableCollection<Email> _inbox = DemoData.CreateInbox();

    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        Messages.ItemsSource = _inbox;
        Messages.ContainerPrepared += (_, _) => UpdateRowShading();

        _inbox.CollectionChanged += OnInboxChanged;

        AddHandler(SwipeItem.SwipeActionEvent, OnSwipeAction);
        AddHandler(SwipeItem.RemovedEvent, OnRowRemoved);
    }

    private void OnInboxChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateRowShading();

    private void OnSwipeAction(object? sender, SwipeActionEventArgs e)
    {
        if (e.Source is not SwipeItem row || row.Email is not { } email)
        {
            return;
        }

        // Where the row sits in the particle layer's coordinates, which is what the burst is
        // positioned against.
        var origin = row.TranslatePoint(default, Particles) ?? default;

        if (e.Action == Controls.SwipeAction.Remove)
        {
            var width = row.Bounds.Width;

            DispatcherTimer.RunOnce(
                () => Particles.Field.LineExplosion(origin.X, origin.Y, width),
                ExplosionDelay);

            row.BeginRemove();
            return;
        }

        email.ToggleFavorite();

        if (email.IsFavorite)
        {
            Particles.Field.PointExplosion(
                origin.X + FavoriteBurstOffset.X,
                origin.Y + FavoriteBurstOffset.Y,
                count: 100);
        }
    }

    private void OnRowRemoved(object? sender, RoutedEventArgs e)
    {
        if (e.Source is SwipeItem { Email: { } email })
        {
            _inbox.Remove(email);
        }
    }

    /// <summary>
    /// Re-applies the alternating row fills. Deleting a row shifts every row below it, so the
    /// shading has to be handed out again rather than fixed when the row was created.
    /// </summary>
    private void UpdateRowShading()
    {
        for (var i = 0; i < Messages.ItemCount; i++)
        {
            if (FindRow(i) is { } row)
            {
                row.IsAlternate = i % 2 != 0;
            }
        }
    }

    private SwipeItem? FindRow(int index) =>
        Messages.ContainerFromIndex(index) is { } container
            ? container as SwipeItem ?? container.GetVisualDescendants().OfType<SwipeItem>().FirstOrDefault()
            : null;
}
