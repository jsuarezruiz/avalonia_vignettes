using AvaloniaVignettes.Shared.Models;

namespace AvaloniaVignettes.Tests;

public sealed class HotelTests
{
    [Theory]
    [InlineData(-1d, 0)]
    [InlineData(3.8d, 3)]
    [InlineData(7d, 5)]
    public void StarsUseWholeRatingWithinDocumentedRange(double rating, int expectedCount)
    {
        var hotel = new Hotel("Hotel", rating, Reviews: 10, Price: 100);

        Assert.Equal(expectedCount, hotel.Stars.Count);
    }
}
