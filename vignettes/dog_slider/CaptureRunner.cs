using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using DogSlider.Controls;
using DogSlider.Views;

namespace DogSlider;

/// <summary>
/// Drives the slider across its range and grabs frames as the dog gives chase, so the walk and the
/// physics can be checked without a human dragging anything.
/// </summary>
/// <remarks>Enabled with <c>--capture &lt;directory&gt;</c>.</remarks>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        // Long enough for the dog's start delay to pass.
        await Task.Delay(1200);

        var view = FrameCapture.Find<MainView>(window);
        var slider = FrameCapture.Find<Controls.DogSlider>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        FrameCapture.Write(view, size, outputDirectory, "rest");

        // Send the ball down the line and watch the dog run after it. Half way keeps the dog
        // clear of the window edge so the settled pose can be seen whole.
        slider.Value = 0.55d;

        var elapsed = 0;

        foreach (var time in (int[])[150, 400, 700, 1000, 1400, 2000])
        {
            await Task.Delay(time - elapsed);
            elapsed = time;

            FrameCapture.Write(view, size, outputDirectory, $"chase_{time.ToString("0000", CultureInfo.InvariantCulture)}");
        }

        // Let the dog actually reach the ball and fold into a sit.
        await Task.Delay(2500);
        FrameCapture.Write(view, size, outputDirectory, "sitting");

        // Back to zero: the dog turns round and leaves.
        slider.Value = 0d;
        elapsed = 0;

        foreach (var time in (int[])[300, 700, 1100, 1600])
        {
            await Task.Delay(time - elapsed);
            elapsed = time;

            FrameCapture.Write(view, size, outputDirectory, $"leave_{time.ToString("0000", CultureInfo.InvariantCulture)}");
        }

    }

}
