using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
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
        Directory.CreateDirectory(outputDirectory);

        // Let the window settle so the rows have been measured.
        await Task.Delay(700);

        var view = FrameCapture.Find<MainView>(window);
        var field = FrameCapture.Find<ParticleFieldView>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        FrameCapture.Write(view, size, outputDirectory, "rest");

        field.Field.PointExplosion(60d, 46d + SwipeItem.NominalHeight, count: 100);
        await CaptureSequenceAsync(view, size, outputDirectory, "favorite");

        field.Field.LineExplosion(0d, SwipeItem.NominalHeight * 3d, window.ClientSize.Width);
        await CaptureSequenceAsync(view, size, outputDirectory, "delete");

    }

    private static async Task CaptureSequenceAsync(Control view, PixelSize size, string outputDirectory, string name)
    {
        var elapsed = 0;

        foreach (var time in FrameTimes)
        {
            await Task.Delay(Math.Max(0, time - elapsed));
            elapsed = time;

            FrameCapture.Write(view, size, outputDirectory, $"{name}_{time.ToString("0000", CultureInfo.InvariantCulture)}");
        }
    }

}
