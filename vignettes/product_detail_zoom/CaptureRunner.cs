using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using ProductDetailZoom.Views;

namespace ProductDetailZoom;

/// <summary>
/// Presses the zoom and grabs frames along the three second flight.
/// </summary>
internal static class CaptureRunner
{

    /// <summary>
    /// Renders every frame to <paramref name="outputDirectory"/> and shuts the app down.
    /// </summary>
    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        // The zoom button holds itself back for a second before fading in.
        await Task.Delay(1800);

        var view = FrameCapture.Find<MainView>(window);
        var product = FrameCapture.Find<ProductPage>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        FrameCapture.Write(view, size, outputDirectory, "1_product");

        // The whole real path: the press, its 300 of fade, then the three second route with the
        // speaker spinning through the black middle of it. The pulsing button is a template around
        // a plain one, and the press lives on the one inside.
        FrameCapture.Find<Button>(product.ZoomButton)
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        await Task.Delay(1000);
        FrameCapture.Write(view, size, outputDirectory, "2_leaving");

        await Task.Delay(800);
        FrameCapture.Write(view, size, outputDirectory, "3_spinning");

        await Task.Delay(1000);
        FrameCapture.Write(view, size, outputDirectory, "4_arriving");

        await Task.Delay(1200);
        FrameCapture.Write(view, size, outputDirectory, "5_detail");

        FrameCapture.Shutdown();
    }

}
