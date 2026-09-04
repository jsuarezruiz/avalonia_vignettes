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

        var sky = FrameCapture.Find<SunAndMoon>(window);
        carousel.Next();
        await Task.Delay(1100);
        capture.Write("2_moon");
        Console.WriteLine($"Gooey moon: selected={carousel.SelectedIndex}, visualIndex={sky.Index}, complete={sky.IsDragCompleted}, angle={sky.RotationAngle}, bounds={sky.Bounds}");
        AssertSky(carousel, sky, 2, -240d);

        carousel.Next();
        await Task.Delay(1100);
        capture.Write("3_wrapped");
        AssertSky(carousel, sky, 3, -360d);

        carousel.Previous();
        await Task.Delay(1100);
        capture.Write("4_back_to_moon");
        AssertSky(carousel, sky, 2, -240d);
    }

    private static void AssertSky(GooeyCarousel carousel, SunAndMoon sky, int logicalIndex, double angle)
    {
        if (sky.Index != logicalIndex || carousel.SelectedIndex != logicalIndex % 3 ||
            !sky.IsDragCompleted || Math.Abs(sky.RotationAngle - angle) > 0.01d)
            throw new InvalidOperationException($"Sky did not settle correctly: index={sky.Index}, angle={sky.RotationAngle}.");
    }

}
