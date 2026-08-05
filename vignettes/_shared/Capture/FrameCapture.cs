using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AvaloniaVignettes.Shared.Capture;

/// <summary>
/// The parts of <c>--capture</c> that do not vary: reading the directory off the command line,
/// writing one frame out, and closing the app once the last frame is written.
/// </summary>
/// <remarks>
/// Only the sequence of frames says anything about the vignette it belongs to, so that is all each
/// <c>CaptureRunner</c> keeps.
/// </remarks>
public static class FrameCapture
{
    /// <summary>
    /// Reads the capture directory out of the command line, if the switch is present.
    /// </summary>
    public static string? GetOutputDirectory(string[] args)
    {
        var index = Array.IndexOf(args, "--capture");
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    /// <summary>
    /// Finds the one <typeparamref name="T"/> under <paramref name="root"/>, and says which one was
    /// wanted when there is none.
    /// </summary>
    /// <remarks>
    /// A capture run drives controls it has to go looking for, and a bare <c>First</c> on an empty
    /// run only reports that a sequence was empty — which of them was missing is the useful part.
    /// </remarks>
    public static T Find<T>(Visual root)
        where T : Visual =>
        root.GetVisualDescendants().OfType<T>().FirstOrDefault()
        ?? throw new InvalidOperationException($"No {typeof(T).Name} was found in the visual tree.");

    /// <summary>
    /// Renders <paramref name="view"/> at <paramref name="size"/> and writes it to
    /// <paramref name="name"/>.png under <paramref name="outputDirectory"/>.
    /// </summary>
    public static void Write(Control view, PixelSize size, string outputDirectory, string name)
    {
        using var bitmap = new RenderTargetBitmap(size, new Vector(96d, 96d));
        bitmap.Render(view);

        var file = $"{name}.png";

        bitmap.Save(Path.Combine(outputDirectory, file), new PngBitmapEncoderOptions());
        Console.WriteLine($"captured {file}");
    }

    /// <summary>
    /// Closes the app, which is what ends the run once the last frame is written.
    /// </summary>
    public static void Shutdown() =>
        Dispatcher.UIThread.Post(() =>
            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown());
}
