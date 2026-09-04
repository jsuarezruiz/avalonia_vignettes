using Avalonia;
using Avalonia.Controls;
using AvaloniaVignettes.Shared.Capture;
using Indie3D.Views;

namespace Indie3D;

/// <summary>
/// Lets the shapes drift and swipes through the artists, grabbing frames as it goes.
/// </summary>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 900);
        var view = capture.View;

        // Measured from TextPainter with the original Staatliches asset. These catch regressions
        // to per-character geometry, which loses kerning and the final letter-spacing advance.
        foreach (var spec in new[] { ("MILES", 57.6d, 6d, 144.8544d), ("MILLER", 96d, 8d, 282.528d) })
        {
            var typeface = new Avalonia.Media.Typeface(Indie3D.Models.Fonts.Display, weight: Avalonia.Media.FontWeight.Bold);
            using var layout = new Avalonia.Media.TextFormatting.TextLayout(spec.Item1, typeface, spec.Item2, Avalonia.Media.Brushes.White, letterSpacing: spec.Item3);
            if (Math.Abs(layout.Width - spec.Item4) > 0.01d)
                throw new InvalidOperationException($"Title width differs from Flutter for {spec.Item1}: {layout.Width}");
        }

        await capture.SampleAsync((int[])[0, 600], time => $"1_drift_{time:0000}");

        // Each page brings its own model, colour and artist.
        var pages = FrameCapture.Find<AvaloniaVignettes.Shared.Controls.ProgressCarousel>(view);

        foreach (var page in (int[])[1, 2])
        {
            pages.SelectedIndex = page;

            // Mid flight: the name of the page being left is wiping out, the next has yet to arrive.
            await Task.Delay(120);
            capture.Write($"2_page_{page}_wiping");

            await Task.Delay(800);
            capture.Write($"2_page_{page}");
        }

        // A tap shoves the shapes of the page away from it.
        view.Tap(new Point(capture.Size.Width / 2d, capture.Size.Height / 2d));

        await Task.Delay(500);
        capture.Write("3_tapped");

    }

}
