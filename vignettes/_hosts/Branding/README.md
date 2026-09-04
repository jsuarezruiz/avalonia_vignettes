# Application branding

`avalonia-logo.png` is Avalonia's official application mark, sourced from the
[AvaloniaUI/Avalonia repository](https://github.com/AvaloniaUI/Avalonia/blob/main/build/Assets/Icon.png).

The Android and iOS launcher icons use the official mark in white over each
sample's own background color. The shared splash and launch-screen images use
the same mark so branding stays consistent across both mobile platforms.

Desktop projects use the same source artwork with platform-appropriate output:
PNG for Avalonia window/taskbar integration, ICO for Windows executables, and
ICNS for macOS bundles. Regenerate all desktop assets after changing the mobile
source icons with:

```shell
python3 -m pip install Pillow
python3 tools/generate-desktop-icons.py
```
