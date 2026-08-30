using System.Collections.ObjectModel;

namespace ParticleSwipe.Models;

/// <summary>
/// The inbox the vignette ships with. Port of <c>DemoData</c>.
/// </summary>
/// <remarks>
/// Seven messages repeated three times over, which is what gives the list enough length to scroll
/// while keeping the data in the source short.
/// </remarks>
public static class DemoData
{
    private const string Body =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum at viverra sem. " +
        "Suspendisse gravida magna in lorem vehicula…";

    /// <summary>
    /// Builds a fresh inbox. The list is mutable, because swiping deletes from it.
    /// </summary>
    public static ObservableCollection<Email> CreateInbox()
    {
        var inbox = new ObservableCollection<Email>();

        for (var repeat = 0; repeat < 3; repeat++)
        {
            inbox.Add(new Email("Jeffrey Evans", "Re: Workshop Preperation", Body));
            inbox.Add(new Email("Jordan Chow", "Reservation Confirmed for Brooklyn", Body, isRead: true));
            inbox.Add(new Email("Katherine Woodward", "Rough outline", Body));
            inbox.Add(new Email("Maddie Toohey", "Daily Recap for Tuesday, October 30", Body, isRead: true));
            inbox.Add(new Email("Tamia Clouthier", "Workshop Information", Body, isRead: true));
            inbox.Add(new Email("Daniel Song", "Possible Urgent Absence", Body));
            inbox.Add(new Email("Andrew Argue", "Vacation Request", Body, isRead: true));
        }

        return inbox;
    }
}
