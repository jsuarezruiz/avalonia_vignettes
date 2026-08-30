using Avalonia.Controls;
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

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 900);
        var view = capture.View;

        capture.Write("list");

        // Drive the field by hand to show what a fast scroll does to it.
        var stars = FrameCapture.Find<Controls.StarField>(window);

        stars.Speed = 8d;
        await Task.Delay(400);
        capture.Write("scrolling");

        stars.Speed = 0.2d;
        await Task.Delay(400);
        capture.Write("idle");

        // Open a constellation so the chart, the lettering and the flight are all exercised.
        var page = new Views.DetailPage
        {
            Constellation = Models.DemoData.Constellations[3],
            IsRedMode = true,
        };

        view.ShowDetailForCapture(page);
        page.Reveal(TimeSpan.FromMilliseconds(1500));

        await capture.SampleAsync((int[])[500, 1000, 1800], time => $"detail_{time:0000}");

    }

}
