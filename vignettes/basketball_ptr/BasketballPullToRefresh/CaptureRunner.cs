using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using AvaloniaVignettes.Shared.Capture;
using BasketballPullToRefresh.Views;

namespace BasketballPullToRefresh;

/// <summary>
/// Smoothly pulls the list down and lets go, recording the opening, the ball throw and the score
/// roll without a human dragging anything.
/// </summary>
/// <remarks>
/// The pull raises the same gesture the touch and mouse recognisers raise, on the presenter they
/// raise it on, so the container's threshold and state machine are exercised rather than skipped.
/// </remarks>
internal static class CaptureRunner
{
    private static readonly TimeSpan PullDuration = TimeSpan.FromMilliseconds(760);

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 900);
        var view = capture.View;
        var presenter = FrameCapture.Find<ScrollContentPresenter>(view);
        var gesture = 1;

        capture.Write("rest");

        await PullAsync(
            presenter,
            gesture,
            view.Metrics.Extent * Controls.BasketballHoop.MaxPull,
            PullDuration);
        capture.Write("pull_pending");

        // Let go past the threshold: the ball is thrown.
        presenter.RaiseEvent(new PullGestureEndedEventArgs(gesture, PullDirection.TopToBottom));

        await capture.SampleAsync(
            (int[])[200, 500, 800, 1200, 1600, 2000, 2400, 2900, 3400, 3970],
            time => $"throw_{Name(time)}");

    }

    private static async Task PullAsync(
        InputElement presenter,
        int gesture,
        double distance,
        TimeSpan duration)
    {
        const int frameMilliseconds = 35;

        var frames = Math.Max(1, (int)Math.Ceiling(duration.TotalMilliseconds / frameMilliseconds));

        for (var frame = 1; frame <= frames; frame++)
        {
            var progress = frame / (double)frames;
            var eased = 1d - Math.Pow(1d - progress, 2d);

            presenter.RaiseEvent(
                new PullGestureEventArgs(
                    gesture,
                    new Vector(0d, distance * eased),
                    PullDirection.TopToBottom));

            await Task.Delay(frameMilliseconds);
        }
    }

    private static string Name(double value) =>
        ((int)value).ToString("0000", CultureInfo.InvariantCulture);

}
