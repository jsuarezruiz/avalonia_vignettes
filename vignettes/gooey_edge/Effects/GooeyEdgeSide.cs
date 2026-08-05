namespace GooeyEdge.Effects;

/// <summary>
/// Which edge of the control the gooey line runs along, and therefore which way it stretches.
/// Port of <c>side.dart</c>.
/// </summary>
public enum GooeyEdgeSide
{
    /// <summary>
    /// The line runs down the left edge and stretches to the right.
    /// </summary>
    Left,

    /// <summary>
    /// The line runs along the top edge and stretches downwards.
    /// </summary>
    Top,

    /// <summary>
    /// The line runs down the right edge and stretches to the left.
    /// </summary>
    Right,

    /// <summary>
    /// The line runs along the bottom edge and stretches upwards.
    /// </summary>
    Bottom,
}
