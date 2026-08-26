using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using ParallaxTravelCardsHero.Views;

namespace ParallaxTravelCardsHero;

/// <summary>
/// Renders the vignette at a set of fixed points along the transition so its layout can be checked
/// against the Flutter reference without a human driving the pointer.
/// </summary>
/// <remarks>
/// Enabled with <c>--capture &lt;directory&gt;</c>. Each frame is named for how far into the
/// navigation it was taken, in milliseconds, for example <c>open0850.png</c>.
/// </remarks>
internal static class CaptureRunner
{
    // When to take each frame, in milliseconds from the start of the navigation. The samples
    // bunch up at both ends of the 1700 ms route, because that is where a hand-off between the
    // flying stand-in and a real control could show.
    private static readonly int[] Frames =
    [
        0, 30, 60, 120, 250, 420, 600, 850, 1100, 1360, 1530, 1640, 1690, 1720, 1760, 1820, 1950,
    ];


    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        // Let the window settle so the bindings that depend on its bounds have run.
        await Task.Delay(600);

        var view = FrameCapture.Find<MainView>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        // A full round trip and then a second opening: the second one is what would catch a flight
        // that starts from wherever the previous one finished.
        await SampleAsync(window, view, size, outputDirectory, "open1", view.BeginCapture);
        await SampleAsync(window, view, size, outputDirectory, "back", view.BackCapture);
        await SampleAsync(window, view, size, outputDirectory, "open2", view.BeginCapture);

    }

    // Runs one navigation and renders a frame at each sample point along it. The navigation plays
    // on its own clock, so the frames are taken from it as it goes rather than posed one at a time.
    private static async Task SampleAsync(
        Window window,
        MainView view,
        PixelSize size,
        string outputDirectory,
        string prefix,
        Action navigate)
    {
        var elapsed = 0;

        navigate();

        foreach (var at in Frames)
        {
            if (at > elapsed)
            {
                await Task.Delay(at - elapsed);
                elapsed = at;
            }

            window.UpdateLayout();

            var name = string.Format(CultureInfo.InvariantCulture, "{0}_{1:0000}", prefix, at);

            FrameCapture.Write(view, size, outputDirectory, name);
        }

        // Let the navigation finish before the next one starts.
        await Task.Delay(400);
    }
}
