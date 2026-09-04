using System.Runtime.InteropServices;

namespace AvaloniaVignettes.Hosts.Desktop;

internal static class MacOSDockIcon
{
    private const string ObjectiveCLibrary = "/usr/lib/libobjc.A.dylib";

    public static bool Apply(string applicationName)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return false;
        }

        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.icns");
        if (!File.Exists(iconPath))
        {
            return false;
        }

        var image = nint.Zero;
        try
        {
            var name = CreateNativeString(applicationName);
            var processInfo = Send(GetClass("NSProcessInfo"), GetSelector("processInfo"));
            Send(processInfo, GetSelector("setProcessName:"), name);

            var path = CreateNativeString(iconPath);
            image = Send(GetClass("NSImage"), GetSelector("alloc"));
            image = Send(image, GetSelector("initWithContentsOfFile:"), path);
            if (image == nint.Zero)
            {
                return false;
            }

            var application = Send(GetClass("NSApplication"), GetSelector("sharedApplication"));
            Send(application, GetSelector("setApplicationIconImage:"), image);
            return true;
        }
        finally
        {
            if (image != nint.Zero)
            {
                Send(image, GetSelector("release"));
            }
        }
    }

    private static nint CreateNativeString(string value) =>
        SendUtf8(GetClass("NSString"), GetSelector("stringWithUTF8String:"), value);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_getClass")]
    private static extern nint GetClass(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string className);

    [DllImport(ObjectiveCLibrary, EntryPoint = "sel_registerName")]
    private static extern nint GetSelector(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string selectorName);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern nint Send(nint receiver, nint selector);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern nint Send(nint receiver, nint selector, nint argument);

    [DllImport(ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern nint SendUtf8(
        nint receiver,
        nint selector,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string argument);
}
