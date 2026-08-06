namespace PlantForms.Models;

/// <summary>
/// The countries the store ships to and their subdivisions. Port of <c>CountryData</c>.
/// </summary>
/// <remarks>
/// The original's state list merges Illinois with Indiana and Montana with Nebraska, which is a
/// typo rather than a rule, so both are split here.
/// </remarks>
public static class CountryData
{
    private static readonly string[] Countries = ["Canada", "France", "United States", "Japan"];

    private static readonly string[] CanadaProvinces =
    [
        "Alberta", "British Columbia", "Manitoba", "New Brunswick", "Newfoundland and Labrador",
        "Northwest Territories", "Nova Scotia", "Nunavut", "Ontario", "Prince Edward Island",
        "Quebec", "Saskatchewan", "Yukon",
    ];

    private static readonly string[] JapanPrefectures =
    [
        "Hokkaido", "Aomori", "Iwate", "Miyagi", "Akita", "Yamagata", "Fukushima", "Ibaraki",
        "Tochigi", "Gunma", "Saitama", "Chiba", "Tokyo", "Kanagawa", "Niigata", "Toyama",
        "Ishikawa", "Fukui", "Yamanashi", "Nagano", "Gifu", "Shizuoka", "Aichi", "Mie", "Shiga",
        "Kyoto", "Osaka", "Hyogo", "Nara", "Wakayama", "Tottori", "Shimane", "Okayama",
        "Hiroshima", "Yamaguchi", "Tokushima", "Kagawa", "Ehime", "Kochi", "Fukuoka", "Miyazaki",
        "Nagasaki", "Kumamoto", "Kagoshima", "Saga", "Oita", "Okinawa",
    ];

    private static readonly string[] UnitedStates =
    [
        "Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut",
        "Delaware", "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa",
        "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan",
        "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire",
        "New Jersey", "New Mexico", "New York", "North Carolina", "North Dakota", "Ohio",
        "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina", "South Dakota",
        "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia",
        "Wisconsin", "Wyoming",
    ];

    /// <summary>
    /// Gets the countries, in the order the picker lists them.
    /// </summary>
    public static IReadOnlyList<string> GetCountries() => Countries;

    /// <summary>
    /// Gets what a country calls its subdivisions, or empty if it has none.
    /// </summary>
    public static string GetSubdivisionTitle(string? country) => country switch
    {
        "Canada" => "Province",
        "Japan" => "Prefecture",
        "United States" => "State",
        _ => string.Empty,
    };

    /// <summary>
    /// Gets the subdivisions listed under the given title.
    /// </summary>
    public static IReadOnlyList<string> GetSubdivisions(string subdivision) => subdivision switch
    {
        "Province" => CanadaProvinces,
        "Prefecture" => JapanPrefectures,
        "State" => UnitedStates,
        _ => [],
    };
}
