using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace PlantForms.Controls;

/// <summary>
/// The button at the foot of a form, which fills up as the form is filled in. Port of
/// <c>submit_button.dart</c>.
/// </summary>
/// <remarks>
/// The fill is not animated in the original either: it is handed a stopped animation, so it steps
/// to each new share as a field passes.
/// </remarks>
public sealed class SubmitButton : Button
{
    public static readonly StyledProperty<double> CompletionProperty =
        AvaloniaProperty.Register<SubmitButton, double>(nameof(Completion), 1d);

    public static readonly StyledProperty<IBrush?> FillBrushProperty =
        AvaloniaProperty.Register<SubmitButton, IBrush?>(nameof(FillBrush));

    public static readonly StyledProperty<bool> IsErrorVisibleProperty =
        AvaloniaProperty.Register<SubmitButton, bool>(nameof(IsErrorVisible));

    private const double BarHeight = 48d;

    static SubmitButton() => AffectsRender<SubmitButton>(CompletionProperty, FillBrushProperty);

    /// <summary>
    /// Gets or sets how much of the form is filled in, from 0 to 1.
    /// </summary>
    public double Completion
    {
        get => GetValue(CompletionProperty);
        set => SetValue(CompletionProperty, value);
    }

    /// <summary>
    /// Gets or sets the colour that fills the button as the form is completed.
    /// </summary>
    public IBrush? FillBrush
    {
        get => GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to say that the form is unfinished.
    /// </summary>
    public bool IsErrorVisible
    {
        get => GetValue(IsErrorVisibleProperty);
        set => SetValue(IsErrorVisibleProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var radius = CornerRadius.TopLeft;
        var bar = new Rect(0d, 0d, Bounds.Width, BarHeight);

        if (Background is { } background)
        {
            context.DrawRectangle(background, null, new RoundedRect(bar, radius));
        }

        var completion = double.IsNaN(Completion) ? 0d : Math.Clamp(Completion, 0d, 1d);

        if (FillBrush is { } fill && completion > 0d)
        {
            var filled = new Rect(0d, 0d, Bounds.Width * completion, BarHeight);

            context.DrawRectangle(fill, null, new RoundedRect(filled, radius));
        }
    }
}
