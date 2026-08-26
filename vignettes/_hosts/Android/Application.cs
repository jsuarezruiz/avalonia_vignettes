using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace AvaloniaVignettes.Hosts.Android;

[Application(
    Description = "@string/app_description",
    Icon = "@mipmap/ic_launcher",
    RoundIcon = "@mipmap/ic_launcher_round",
    SupportsRtl = true)]
public sealed class VignetteApplication : AvaloniaAndroidApplication<VignetteApp>
{
    public VignetteApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).WithInterFont();
}
