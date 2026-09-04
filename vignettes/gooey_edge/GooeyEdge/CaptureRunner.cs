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

        capture.Write(SwipeFractions[0].ToString("0.00", CultureInfo.InvariantCulture));
        carousel.Next();

        var previousDelay = TimeSpan.Zero;
        foreach (var fraction in SwipeFractions.Skip(1))
        {
            var delay = GooeyCarousel.TransitionDuration * fraction;
            await Task.Delay(delay - previousDelay);

            capture.Write(fraction.ToString("0.00", CultureInfo.InvariantCulture));
            previousDelay = delay;
        }

        await Task.Delay(700);
        capture.Write("1_settled");

        carousel.Next();
        await Task.Delay(1100);
        capture.Write("2_moon");

        carousel.Next();
        await Task.Delay(1100);
        capture.Write("3_wrapped");

        carousel.Previous();
        await Task.Delay(1100);
        capture.Write("4_back_to_moon");
    }

}
