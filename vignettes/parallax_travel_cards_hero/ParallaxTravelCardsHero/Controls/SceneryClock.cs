namespace ParallaxTravelCardsHero.Controls;

/// <summary>
/// The clock the clouds and the leaves drift on, shared by every copy of the scenery on screen.
/// </summary>
/// <remarks>
/// The flight puts three sceneries in play: the card's, the details page's, and the copy flying
/// between them. Each would otherwise start its drift from scratch, so the clouds would jump the
/// moment one handed over to the next. The original works around the same problem by parking
/// its ticker state in a static map; because the widgets carry no keys they all collide on a single
/// entry, which is to say they all share one clock. This is that clock, said out loud.
/// <para>
/// Both hands advance by a fixed amount per rendered frame rather than per second, as the original's
/// raw <c>Ticker</c>s do, and both wrap rather than reverse.
/// </para>
/// </remarks>
internal static class SceneryClock
{
    private const double CloudSpeed = 0.0003d;

    private const double LeafSpeed = 0.001d;

    private static object? _driver;

    /// <summary>
    /// Gets how far the clouds have drifted, from 0 to 1. It starts half way across, the default
    /// <c>_Clouds</c> is built with, so one is always in shot when the card first opens.
    /// </summary>
    public static double CloudProgress { get; private set; } = 0.5d;

    /// <summary>
    /// Gets how far the leaves have travelled, from 0 to 1.
    /// </summary>
    public static double LeafProgress { get; private set; }

    /// <summary>
    /// Advances both hands, but only for the one caller elected to drive them. Every scenery ticks
    /// once per frame, and the clock must not run faster just because more of them are on screen.
    /// </summary>
    public static void Advance(object caller)
    {
        _driver ??= caller;

        if (!ReferenceEquals(_driver, caller))
        {
            return;
        }

        CloudProgress = CloudProgress <= 1d ? CloudProgress + CloudSpeed : 0d;
        LeafProgress = LeafProgress + LeafSpeed < 1d ? LeafProgress + LeafSpeed : 0d;
    }

    /// <summary>
    /// Gives up the driving seat, so the next scenery to tick takes it over.
    /// </summary>
    public static void Release(object caller)
    {
        if (ReferenceEquals(_driver, caller))
        {
            _driver = null;
        }
    }
}
