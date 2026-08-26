using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using SparkleParty.Views;

namespace SparkleParty;

/// <summary>
/// Plays each effect in turn and grabs frames, with and without a touch point.
/// </summary>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        await Task.Delay(900);

        var view = FrameCapture.Find<MainView>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);
        var names = (string[])["waterfall", "fireworks", "comet", "pinwheel"];

        for (var effect = 0; effect < names.Length; effect++)
        {
            view.Show(effect);

            await Task.Delay(1200);

            FrameCapture.Write(view, size, outputDirectory, $"{effect + 1}_{names[effect]}");

            // And again with the screen being touched, which is what each effect is played with.
            view.Touch(new Point(size.Width / 2d, size.Height * 0.45d));

            await Task.Delay(900);

            FrameCapture.Write(view, size, outputDirectory, $"{effect + 1}_{names[effect]}_touched");

            view.Touch(null);
        }

    }

}
