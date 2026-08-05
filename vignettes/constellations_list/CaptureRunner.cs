using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using ConstellationsList.Views;

namespace ConstellationsList;

/// <summary>
/// Grabs the list and then a constellation's detail page, so the star field and the flight can be
/// checked without a human scrolling or tapping.
/// </summary>
/// <remarks>Enabled with <c>--capture &lt;directory&gt;</c>.</remarks>
internal static class CaptureRunner
{

    /// <summary>
    /// Renders every frame to <paramref name="outputDirectory"/> and shuts the app down.
    /// </summary>
    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        await Task.Delay(900);

        var view = FrameCapture.Find<MainView>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        FrameCapture.Write(view, size, outputDirectory, "list");

        // Drive the field by hand to show what a fast scroll does to it.
        var stars = FrameCapture.Find<Controls.StarField>(window);

        stars.Speed = 8d;
        await Task.Delay(400);
        FrameCapture.Write(view, size, outputDirectory, "scrolling");

        stars.Speed = 0.2d;
        await Task.Delay(400);
        FrameCapture.Write(view, size, outputDirectory, "idle");

        // Open a constellation so the chart, the lettering and the flight are all exercised.
        var page = new Views.DetailPage
        {
            Constellation = Models.DemoData.Constellations[3],
            IsRedMode = true,
        };

        view.ShowDetailForCapture(page);
        page.Reveal(TimeSpan.FromMilliseconds(1500));

        var elapsed = 0;

        foreach (var time in (int[])[500, 1000, 1800])
        {
            await Task.Delay(time - elapsed);
            elapsed = time;

            FrameCapture.Write(view, size, outputDirectory, $"detail_{time:0000}");
        }

        FrameCapture.Shutdown();
    }

}
