using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ConstellationsList.Models;

namespace ConstellationsList.Controls;

/// <summary>
/// A constellation's name and what it depicts. Port of <c>constellation_title_card.dart</c>.
/// </summary>
/// <remarks>
/// Alternate entries are drawn in red outline rather than solid white, which is what gives the list
/// its rhythm as it scrolls. The same card is used in the detail view, so it can fly between the two.
/// </remarks>
public sealed class ConstellationTitleCard : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Constellation"/> property.
    /// </summary>
    public static readonly StyledProperty<Constellation?> ConstellationProperty =
        AvaloniaProperty.Register<ConstellationTitleCard, Constellation?>(nameof(Constellation));

    /// <summary>
    /// Defines the <see cref="IsRedMode"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRedModeProperty =
        AvaloniaProperty.Register<ConstellationTitleCard, bool>(nameof(IsRedMode));

    static ConstellationTitleCard() =>
        IsRedModeProperty.Changed.AddClassHandler<ConstellationTitleCard>((x, e) =>
            x.PseudoClasses.Set(":red", e.GetNewValue<bool>()));

    /// <summary>
    /// Gets or sets the constellation being named.
    /// </summary>
    public Constellation? Constellation
    {
        get => GetValue(ConstellationProperty);
        set => SetValue(ConstellationProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this entry is the red, outlined kind.
    /// </summary>
    public bool IsRedMode
    {
        get => GetValue(IsRedModeProperty);
        set => SetValue(IsRedModeProperty, value);
    }
}
