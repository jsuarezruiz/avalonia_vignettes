using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;

namespace AvaloniaVignettes.Shared.Capture;

/// <summary>
/// The parts of <c>--capture</c> that do not vary: reading the directory off the command line,
/// preparing a session, sampling it on a timeline and writing frames.
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
    internal static string? GetOutputDirectory(string[] args)
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
    /// run only reports that a sequence was empty, which of them was missing is the useful part.
    /// </remarks>
    public static T Find<T>(Visual root)
        where T : Visual =>
        root.GetVisualDescendants().OfType<T>().FirstOrDefault()
        ?? throw new InvalidOperationException($"No {typeof(T).Name} was found in the visual tree.");

    /// <summary>
    /// Lets a window finish its first layout, then gathers the state every capture sequence needs.
    /// </summary>
    public static async Task<CaptureSession<TView>> StartAsync<TView>(
        Window window,
        string outputDirectory,
        int settleMilliseconds)
        where TView : Control
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentOutOfRangeException.ThrowIfNegative(settleMilliseconds);

        Directory.CreateDirectory(outputDirectory);

        if (settleMilliseconds > 0)
        {
            await Task.Delay(settleMilliseconds);
        }

        var view = Find<TView>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        return new CaptureSession<TView>(window, view, size, outputDirectory);
    }

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

}

/// <summary>
/// The window, view, dimensions and destination shared by one vignette capture sequence.
/// </summary>
public sealed class CaptureSession<TView>
    where TView : Control
{
    internal CaptureSession(Window window, TView view, PixelSize size, string outputDirectory)
    {
        Window = window;
        View = view;
        Size = size;
        OutputDirectory = outputDirectory;
    }

    public Window Window { get; }

    public TView View { get; }

    public PixelSize Size { get; }

    public string OutputDirectory { get; }

    /// <summary>
    /// Writes a named frame using this session's view, dimensions and destination.
    /// </summary>
    public void Write(string name) => FrameCapture.Write(View, Size, OutputDirectory, name);

    /// <summary>
    /// Captures frames at ascending millisecond offsets from the moment this method is called.
    /// </summary>
    public async Task SampleAsync(
        IEnumerable<int> frameTimes,
        Func<int, string> nameFrame,
        Action<int>? prepareFrame = null)
    {
        ArgumentNullException.ThrowIfNull(frameTimes);
        ArgumentNullException.ThrowIfNull(nameFrame);

        var times = frameTimes.ToArray();

        for (var index = 0; index < times.Length; index++)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(times[index]);

            if (index > 0 && times[index] < times[index - 1])
            {
                throw new ArgumentException("Frame times must be in ascending order.", nameof(frameTimes));
            }
        }

        var clock = Stopwatch.StartNew();

        foreach (var time in times)
        {
            var remaining = time - clock.ElapsedMilliseconds;

            if (remaining > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(remaining));
            }

            prepareFrame?.Invoke(time);
            Write(nameFrame(time));
        }
    }
}
