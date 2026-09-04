using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
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
                ApplyDesktopIcon(window);
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

    private static void ApplyDesktopIcon(Window window)
    {
        var assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name
            ?? window.GetType().Assembly.GetName().Name;
        var iconUri = new Uri($"avares://{assemblyName}/Assets/AppIcon.png");
        using var iconStream = AssetLoader.Open(iconUri);
        window.Icon = new WindowIcon(iconStream);
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
