using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using DarkInkTransition.Controls;

namespace DarkInkTransition.Views;

/// <summary>
/// The demo: an article, a bar, three controls, and a tap anywhere to change the scheme. Port of
/// <c>demo.dart</c>.
/// </summary>
/// <remarks>
/// The scheme is not animated, two complete copies of the article exist, and the new one is painted
/// over the old through the ink. When the reveal finishes the new copy becomes the background and
/// the old one is dropped, so only one is left standing between transitions.
/// </remarks>
public partial class MainView : UserControl
{
    private ArticlePage _background = new();
    private InkTransition? _reveal;
    private bool _isDark;
    private bool _isRevealing;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        Pages.Children.Add(_background);
        Bar.ToggleRequested += (_, _) => Toggle();
        Tapped += (_, _) => Toggle();
    }

    /// <summary>
    /// Changes the scheme, as a tap anywhere does. The capture harness drives it.
    /// </summary>
    internal async void Toggle()
    {
        if (_isRevealing)
        {
            return;
        }

        _isDark = !_isDark;

        Bar.IsDark = _isDark;
        Controls.IsDark = _isDark;

        var foreground = new ArticlePage { IsDark = _isDark };

        // The incoming copy starts where the outgoing one is, so the reveal does not also jump the
        // reader back to the top of the article.
        foreground.Scroll.Offset = _background.Scroll.Offset;

        _reveal = new InkTransition { Child = foreground };

        Pages.Children.Add(_reveal);
        _isRevealing = true;

        await new Animation
        {
            Duration = InkTransition.Duration,
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(InkTransition.ProgressProperty, 0d) } },
                new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(InkTransition.ProgressProperty, 1d) } },
            },
        }.RunAsync(_reveal);

        // The revealed copy becomes the page, and the one it covered is dropped.
        Pages.Children.Clear();
        _reveal.Child = null;
        Pages.Children.Add(foreground);

        _background = foreground;
        _reveal = null;
        _isRevealing = false;
    }
}
