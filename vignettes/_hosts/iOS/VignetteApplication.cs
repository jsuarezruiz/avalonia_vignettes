using Avalonia;
using Avalonia.iOS;
using Foundation;

namespace AvaloniaVignettes.Hosts.iOS;

[Register("AppDelegate")]
public sealed class VignetteApplication : AvaloniaAppDelegate<VignetteApp>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).WithInterFont();
}
