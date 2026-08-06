using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PlantForms.Controls;

/// <summary>
/// One page of the form: a white card along the bottom of the screen, with the scene showing above
/// it. Port of <c>form_page.dart</c>.
/// </summary>
[TemplatePart(PartRoot, typeof(Grid))]
[TemplatePart(PartBackArea, typeof(Control))]
public class FormPage : ContentControl
{
    private const string PartRoot = "PART_Root";
    private const string PartBackArea = "PART_BackArea";

    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<FormPage, string>(nameof(Title), string.Empty);

    /// <summary>
    /// Defines the <see cref="Proportion"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ProportionProperty =
        AvaloniaProperty.Register<FormPage, double>(nameof(Proportion), 0.85d);

    /// <summary>
    /// Defines the <see cref="BackRequested"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> BackRequestedEvent =
        RoutedEvent.Register<FormPage, RoutedEventArgs>(nameof(BackRequested), RoutingStrategies.Bubble);

    /// <summary>
    /// Defines the <see cref="NextRequested"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> NextRequestedEvent =
        RoutedEvent.Register<FormPage, RoutedEventArgs>(nameof(NextRequested), RoutingStrategies.Bubble);

    private Grid? _root;

    /// <summary>
    /// Raised when the page is done with and the next one should come up.
    /// </summary>
    public event EventHandler<RoutedEventArgs> NextRequested
    {
        add => AddHandler(NextRequestedEvent, value);
        remove => RemoveHandler(NextRequestedEvent, value);
    }

    /// <summary>
    /// Raised when the strip of scene above the card is tapped.
    /// </summary>
    public event EventHandler<RoutedEventArgs> BackRequested
    {
        add => AddHandler(BackRequestedEvent, value);
        remove => RemoveHandler(BackRequestedEvent, value);
    }

    /// <summary>
    /// Gets or sets the heading printed at the top of the card.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the share of the screen the card takes.
    /// </summary>
    public double Proportion
    {
        get => GetValue(ProportionProperty);
        set => SetValue(ProportionProperty, value);
    }

    /// <summary>
    /// Asks for the next page.
    /// </summary>
    protected void RequestNext() => RaiseEvent(new RoutedEventArgs(NextRequestedEvent));

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _root = e.NameScope.Find<Grid>(PartRoot);

        if (e.NameScope.Find<Control>(PartBackArea) is { } backArea)
        {
            backArea.Tapped += OnBackAreaTapped;
        }

        ApplyProportion();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ProportionProperty)
        {
            ApplyProportion();
        }
    }

    private void OnBackAreaTapped(object? sender, TappedEventArgs e) =>
        RaiseEvent(new RoutedEventArgs(BackRequestedEvent));

    private void ApplyProportion()
    {
        if (_root is not { RowDefinitions.Count: 2 })
        {
            return;
        }

        _root.RowDefinitions[0].Height = new GridLength(1d - Proportion, GridUnitType.Star);
        _root.RowDefinitions[1].Height = new GridLength(Proportion, GridUnitType.Star);
    }
}
