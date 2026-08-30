using Avalonia;
using Avalonia.Markup.Xaml;
using AvaloniaVignettes.Shared.Hosting;
using Indie3D.Views;

namespace Indie3D;

/// <summary>
/// The application entry point. Port of <c>main.dart</c>'s <c>App</c>.
/// </summary>
public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        VignetteLifetime.Configure<MainView>(
            ApplicationLifetime,
            static () => new MainWindow(),
            CaptureRunner.RunAsync);

        base.OnFrameworkInitializationCompleted();
    }
}
