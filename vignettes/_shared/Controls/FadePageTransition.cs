using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;

using AvaloniaVignettes.Shared.Controls;

namespace AvaloniaVignettes.Shared.Controls;

/// <summary>
/// A navigation that empties the screen before refilling it: the page being left goes early, the one
/// being entered arrives late, and in between there is nothing but the background. It is the shape
/// several of the vignettes give a route so a shared element has the screen to itself while it
/// crosses — <c>white_page_route.dart</c> and <c>fade_color_page_route.dart</c> are both this.
/// </summary>
/// <remarks>
/// Those originals fade a coloured sheet in over the outgoing page rather than fading the page out.
/// Against a background of that colour the two are the same composite, and fading the page is what a
/// page transition is handed the means to do.
/// <para>
/// A pop is not this transition played backwards: the route's own clock reverses, so each end gets
/// its own pair of cues. Set <see cref="HeroFlight"/> to have a shared element cross on the same
/// clock.
/// </para>
/// </remarks>
public sealed class FadePageTransition : IPageTransition
{
    /// <summary>Gets or sets how long the navigation takes.</summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(1700);

    /// <summary>Gets or sets when the page being pushed away has finished going.</summary>
    public double PushFadeOutEnd { get; set; } = 0.2d;

    /// <summary>Gets or sets when the page being pushed in starts arriving.</summary>
    public double PushFadeInStart { get; set; } = 0.7d;

    /// <summary>Gets or sets when the page being popped has finished going.</summary>
    public double PopFadeOutEnd { get; set; } = 0.3d;

    /// <summary>Gets or sets when the page being returned to starts arriving.</summary>
    public double PopFadeInStart { get; set; } = 0.8d;

    /// <summary>Gets or sets the layer that carries a shared element across, if there is one.</summary>
    public HeroFlight? HeroFlight { get; set; }

    private Control? _hidden;

    /// <summary>
    /// Hides a page that is about to be pushed, until this transition takes charge of it.
    /// </summary>
    /// <remarks>
    /// A navigation adds the incoming page to the tree before it asks a transition to start, so
    /// without this the page is drawn whole for the frame in between — it appears complete, then
    /// vanishes behind the white as the transition finally begins.
    /// </remarks>
    public void Prepare(Control page)
    {
        _hidden = page;
        page.Opacity = 0d;
    }

    /// <inheritdoc />
    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        // Taking the fade over from Prepare, in that order: the page is only handed back once
        // whatever hosts it is holding it at nothing.
        if (to is not null)
        {
            to.Opacity = 0d;
        }

        if (_hidden is not null)
        {
            _hidden.Opacity = 1d;
            _hidden = null;
        }

        var running = new List<Task>(3);

        if (from is not null)
        {
            running.Add(Fade(1d, 0d, 0d, forward ? PushFadeOutEnd : PopFadeOutEnd).RunAsync(from, cancellationToken));
        }

        if (to is not null)
        {
            running.Add(Fade(0d, 1d, forward ? PushFadeInStart : PopFadeInStart, 1d).RunAsync(to, cancellationToken));
        }

        if (HeroFlight is { } flight)
        {
            running.Add(flight.RunAsync(from, to, forward, Duration, cancellationToken));
        }

        await Task.WhenAll(running);
    }

    /// <summary>
    /// Builds a fade that holds, runs over a window of the navigation, then holds again — the
    /// equivalent of wrapping an <c>Interval</c> around a <c>Tween</c>.
    /// </summary>
    private Avalonia.Animation.Animation Fade(double from, double to, double begin, double end)
    {
        var animation = new Avalonia.Animation.Animation { Duration = Duration, FillMode = FillMode.Forward };

        Hold(0d, from);

        if (begin > 0d)
        {
            Hold(begin, from);
        }

        Hold(end, to);

        if (end < 1d)
        {
            Hold(1d, to);
        }

        return animation;

        void Hold(double cue, double opacity) =>
            animation.Children.Add(new KeyFrame
            {
                Cue = new Cue(cue),
                Setters = { new Setter(Visual.OpacityProperty, opacity) },
            });
    }
}
