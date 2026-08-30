using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using AvaloniaVignettes.Shared.Capture;
using BasketballPullToRefresh.Views;

namespace BasketballPullToRefresh;

/// <summary>
/// Pulls the list down and lets go, grabbing frames as the ball is thrown, so the scene and the
/// score roll can be checked without a human dragging anything.
/// </summary>
/// <remarks>
/// The pull raises the same gesture the touch and mouse recognisers raise, on the presenter they
/// raise it on, so the container's threshold and state machine are exercised rather than skipped.
/// <para>
/// These frames cannot show the list moving: the container pushes it by composition offset, and an
/// offscreen render walks the control tree, not the compositor's. Only a recording shows that.
/// </para>
/// </remarks>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 600);
        var view = capture.View;
        var presenter = FrameCapture.Find<ScrollContentPresenter>(view);
        var gesture = 1;

        capture.Write("rest");

        // Pull down in stages: the hoop shrinks, and the caption changes once the pull counts.
        foreach (var distance in (double[])[60d, 120d, 200d])
        {
            presenter.RaiseEvent(
                new PullGestureEventArgs(gesture, new Vector(0d, distance), PullDirection.TopToBottom));

            await Task.Delay(200);

            capture.Write($"pull_{Name(distance)}");
        }

        // Let go past the threshold: the ball is thrown.
        presenter.RaiseEvent(new PullGestureEndedEventArgs(gesture, PullDirection.TopToBottom));

        await capture.SampleAsync(
            (int[])[200, 500, 800, 1200, 1600, 2000, 2400, 2900],
            time => $"throw_{Name(time)}");

    }

    private static string Name(double value) =>
        ((int)value).ToString("0000", CultureInfo.InvariantCulture);

}
