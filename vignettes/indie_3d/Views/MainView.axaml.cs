using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.Platform;
using AvaloniaVignettes.Shared.Animation;
using AvaloniaVignettes.Shared.Controls;
using Indie3D.Models;

namespace Indie3D.Views;

/// <summary>
/// The three artists, their shapes and the page they are swiped through. Port of <c>demo.dart</c>.
/// </summary>
/// <remarks>
/// One scene of shapes serves all three pages: each takes two layers of it, and scrolling swings the
/// camera sideways so the shapes appear to be a field the pages move across rather than scenery
/// belonging to any one of them.
/// </remarks>
public partial class MainView : UserControl
{
    /// <summary>
    /// The tallest a photograph is ever drawn, which is what they are decoded to.
    /// </summary>
    private const int ArtworkHeight = 820;

    /// <summary>
    /// How long a name takes to wipe in.
    /// </summary>
    private static readonly TimeSpan TitleDuration = TimeSpan.FromMilliseconds(400);

    /// <summary>
    /// How far the camera leans as the pages are dragged past it.
    /// </summary>
    private const double CameraSwing = 8d;

    /// <summary>
    /// How long the second name waits before following the first.
    /// </summary>
    private static readonly TimeSpan TitleStagger = TimeSpan.FromMilliseconds(200);

    private ShapeScene? _scene;
    private readonly FrameTicker _ticker;
    private readonly AnimationController[] _topTitles;
    private readonly AnimationController[] _bottomTitles;
    private readonly DispatcherTimer _stagger;

    private ArtistPage[] _pages = [];
    private TimeSpan _lastTick;
    private bool _hasTicked;
    private int _pageIndex;
    private double _lastPage;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        _pages = [Page0, Page1, Page2];

        foreach (var (page, i) in _pages.Select((page, i) => (page, i)))
        {
            page.Artwork = Load($"artist_{i + 1}", ArtworkHeight);
        }

        // Reading the models and settling them into a field takes a moment, which the original
        // spends showing that it is loading rather than on the frame.
        _ = LoadSceneAsync();

        _topTitles = [.. _pages.Select(page => Controller(value => page.Title.TopProgress = 1d - value))];
        _bottomTitles = [.. _pages.Select(page => Controller(value => page.Title.BottomProgress = 1d - value))];

        // The first page is already showing, so its name is already in.
        _topTitles[0].SetValue(1d);
        _bottomTitles[0].SetValue(1d);

        _stagger = new DispatcherTimer { Interval = TitleStagger };
        _stagger.Tick += OnStaggerTick;

        Pages.GetObservable(PageView.PageProperty).Subscribe(
            new AnonymousObserver<double>(OnPageMoved));

        _ticker = new FrameTicker(this, OnTick);

        // Tapped, not PointerReleased: the end of a swipe is not a tap.
        Tapped += OnTapped;

        UpdateIndicator();
    }

    private async Task LoadSceneAsync()
    {
        var scene = await Task.Run(() => new ShapeScene());

        scene.SetViewport(Bounds.Size);

        foreach (var page in _pages)
        {
            page.Scene = scene;
        }

        _scene = scene;
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // A ticker asks its top level for frames, so it can only start once there is one.
        _ticker.Start();
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        _scene?.SetViewport(finalSize);

        return base.ArrangeOverride(finalSize);
    }

    /// <summary>
    /// Loads a photograph at the size it is drawn at. The originals are 2048 square and are shown
    /// about a third of that, and every frame of drifting shapes recomposites them.
    /// </summary>
    private static Bitmap Load(string name, int height) => Bitmap.DecodeToHeight(
        AssetLoader.Open(new Uri($"avares://Indie3D/Assets/Images/{name}.png")),
        height);

    private AnimationController Controller(Action<double> onChanged) =>
        new(this, onChanged) { Duration = TitleDuration };

    private void OnTick(TimeSpan elapsed)
    {
        var step = _hasTicked ? Math.Max(0d, (elapsed - _lastTick).TotalSeconds) : 0d;

        _lastTick = elapsed;
        _hasTicked = true;

        if (_scene is null)
        {
            return;
        }

        _scene.SetViewport(Bounds.Size);
        _scene.Advance(Math.Min(step, 0.1d));

        // Only the pages on screen are worth redrawing: one at rest, two mid swipe.
        for (var i = 0; i < _pages.Length; i++)
        {
            if (Math.Abs(i - Pages.Page) < 1d)
            {
                _pages[i].Invalidate();
            }
        }
    }

    /// <summary>
    /// Follows the strip as it is dragged. Everything comes from where the pages are rather than
    /// from the drag itself, so a page changed in code swings the camera and wipes the name in the
    /// same way a swipe does.
    /// </summary>
    private void OnPageMoved(double page)
    {
        // The camera leans away as a page leaves and comes back as the next one lands, which is
        // what makes the shapes read as a field the pages travel across.
        var travelled = page - _pageIndex;
        var direction = Math.Sign(page - _lastPage);

        _lastPage = page;
        if (_scene is not null)
        {
            _scene.TargetCameraOffset =
                2d * Math.Abs(travelled) * CameraSwing * (direction == 0 ? 1 : direction);
        }

        // A name that has been revealed tracks the drag, so it wipes out as its page leaves and
        // back in if the drag turns round.
        var progress = ((1d - Math.Clamp(Math.Abs(travelled), 0d, 1d)) * 2d) - 1d;

        if (!_topTitles[_pageIndex].IsAnimating && _topTitles[_pageIndex].Value != 0d)
        {
            _topTitles[_pageIndex].SetValue(Math.Clamp(progress, 0d, 1d));
            _bottomTitles[_pageIndex].SetValue(Math.Clamp(progress, 0d, 1d));
        }

        var landed = (int)Math.Round(page);

        if (Math.Abs(page - landed) > 0.01d || landed == _pageIndex)
        {
            return;
        }

        _pageIndex = Math.Clamp(landed, 0, _pages.Length - 1);

        if (_scene is not null)
        {
            _scene.TargetCameraOffset = 0d;
        }

        UpdateIndicator();

        // The name of the page that has arrived wipes in, the second line a beat after the first.
        for (var i = 0; i < _pages.Length; i++)
        {
            if (i != _pageIndex)
            {
                _topTitles[i].SetValue(0d);
                _bottomTitles[i].SetValue(0d);
            }
        }

        _topTitles[_pageIndex].Forward();
        _stagger.Start();
    }

    private void OnStaggerTick(object? sender, EventArgs e)
    {
        _stagger.Stop();

        _bottomTitles[_pageIndex].Forward();
    }

    /// <summary>
    /// Shoves the shapes away from a point, as a tap does.
    /// </summary>
    public void Tap(Point position) => _scene?.Tap(position, Bounds.Size, _pageIndex);

    private void OnTapped(object? sender, TappedEventArgs e) => Tap(e.GetPosition(this));

    private void UpdateIndicator()
    {
        for (var i = 0; i < Indicator.Children.Count; i++)
        {
            Indicator.Children[i].Classes.Set("current", i == _pageIndex);
        }
    }
}
