using System.Globalization;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using ParticleSwipe.Controls;
using ParticleSwipe.Views;

namespace ParticleSwipe;

/// <summary>
/// Renders both swipe effects part-way through so they can be checked against the Flutter reference
/// without a human dragging anything.
/// </summary>
/// <remarks>
/// Enabled with <c>--capture &lt;directory&gt;</c>. Each burst is fired directly at the field and
/// frames are grabbed as it travels, each named for the milliseconds elapsed since it began.
/// </remarks>
internal static class CaptureRunner
{
    private static readonly int[] FrameTimes = [60, 160, 320, 520, 760, 1000];


    public static async Task RunAsync(Window window, string outputDirectory)
    {
        // Let the window settle so the rows have been measured.
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 700);
        var field = FrameCapture.Find<ParticleFieldView>(window);
        var rows = capture.View.GetVisualDescendants().OfType<SwipeItem>().ToArray();
        if (rows.Length < 2 || rows.Where((row, index) => row.IsAlternate != (index % 2 != 0)).Any())
            throw new InvalidOperationException("Inbox row shading must alternate after templates are realized.");

        capture.Write("rest");

        field.Field.PointExplosion(60d, 46d + SwipeItem.NominalHeight, count: 100);
        await capture.SampleAsync(
            FrameTimes,
            time => $"favorite_{time.ToString("0000", CultureInfo.InvariantCulture)}");

        field.Field.LineExplosion(0d, SwipeItem.NominalHeight * 3d, window.ClientSize.Width);
        await capture.SampleAsync(
            FrameTimes,
            time => $"delete_{time.ToString("0000", CultureInfo.InvariantCulture)}");

    }

}
