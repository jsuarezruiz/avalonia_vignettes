using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input.GestureRecognizers;

namespace AvaloniaVignettes.Shared.Controls;

/// <summary>
/// A native Avalonia <see cref="Carousel"/> with swiping enabled for touch, pen and mouse.
/// </summary>
/// <remarks>
/// Avalonia 12.1.0 creates its carousel's <see cref="SwipeGestureRecognizer"/> without setting
/// <see cref="SwipeGestureRecognizer.IsMouseEnabled"/>. Enable that option on the existing
/// recognizer; the native panel still owns gesture recognition, capture and page selection.
/// </remarks>
public class SwipeCarousel : Carousel
{
    static SwipeCarousel()
    {
        IsSwipeEnabledProperty.OverrideDefaultValue<SwipeCarousel>(true);
        FocusableProperty.OverrideDefaultValue<SwipeCarousel>(true);
    }

    public SwipeCarousel() => Loaded += (_, _) => EnableMouseSwiping();

    protected override Type StyleKeyOverride => typeof(Carousel);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        EnableMouseSwiping();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // These properties recreate Avalonia's recognizer, so reapply its mouse option afterwards.
        if (change.Property == IsSwipeEnabledProperty || change.Property == PageTransitionProperty
            || change.Property == ViewportFractionProperty || change.Property == WrapSelectionProperty)
        {
            EnableMouseSwiping();
        }
    }

    private void EnableMouseSwiping()
    {
        if (ItemsPanelRoot is not { } panel)
        {
            return;
        }

        foreach (var recognizer in panel.GestureRecognizers.OfType<SwipeGestureRecognizer>())
        {
            recognizer.IsMouseEnabled = true;
        }
    }
}
