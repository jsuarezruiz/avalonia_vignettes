using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaVignettes.Shared.Capture;

namespace AvaloniaVignettes.Shared.Hosting;

public static class VignetteLifetime
{
    public static void Configure<TView>(
        IApplicationLifetime? lifetime,
        Func<Window> createWindow,
        Func<Window, string, Task> capture)
        where TView : Control, new()
    {
        switch (lifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                var window = createWindow();
                desktop.MainWindow = window;

                if (FrameCapture.GetOutputDirectory(desktop.Args ?? []) is { } outputDirectory)
                {
                    window.Opened += async (_, _) =>
                        await RunCaptureAsync(desktop, window, outputDirectory, capture);
                }

                break;

            case IActivityApplicationLifetime activity:
                activity.MainViewFactory = static () => new TView();
                break;

            case ISingleViewApplicationLifetime singleView:
                singleView.MainView = new TView();
                break;
        }
    }

    private static async Task RunCaptureAsync(
        IClassicDesktopStyleApplicationLifetime desktop,
        Window window,
        string outputDirectory,
        Func<Window, string, Task> capture)
    {
        var exitCode = 0;

        try
        {
            await capture(window, outputDirectory);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            exitCode = 1;
        }
        finally
        {
            Dispatcher.UIThread.Post(() => desktop.Shutdown(exitCode));
        }
    }
}
