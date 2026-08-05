using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaVignettes.Shared.Capture;
using FluidNavBar.Views;

namespace FluidNavBar;

/// <summary>
/// The application entry point. Port of <c>main.dart</c>'s <c>App</c>.
/// </summary>
public partial class App : Application
{
    /// <inheritdoc />
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            desktop.MainWindow = window;

            if (FrameCapture.GetOutputDirectory(desktop.Args ?? []) is { } outputDirectory)
            {
                window.Opened += (_, _) => _ = CaptureRunner.RunAsync(window, outputDirectory);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
