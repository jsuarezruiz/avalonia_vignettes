using System.Globalization;
using Avalonia.Controls;
using AvaloniaVignettes.Shared.Capture;
using GooeyEdge.Controls;
using GooeyEdge.Views;

namespace GooeyEdge;

/// <summary>
/// Renders the vignette part-way through a page change so the edge can be checked against the
/// Flutter reference without a human driving the pointer.
/// </summary>
/// <remarks>
/// Enabled with <c>--capture &lt;directory&gt;</c>. A page change is started and frames are grabbed
/// as the edge travels, each named for the milliseconds elapsed since it began.
/// </remarks>
internal static class CaptureRunner
{
    private static readonly double[] SwipeFractions = [0d, 0.2d, 0.45d, 0.7d, 0.95d];


    public static async Task RunAsync(Window window, string outputDirectory)
    {
        // Let the window settle so the bindings that depend on its bounds have run.
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 700);
        var carousel = FrameCapture.Find<GooeyCarousel>(window);

        foreach (var fraction in SwipeFractions)
        {
            await carousel.RunCaptureSwipeAsync(fraction);
            await Task.Delay(60);

            capture.Write(fraction.ToString("0.00", CultureInfo.InvariantCulture));
        }

    }

}
