using TollFeeCalculator.Models;

namespace TollFeeCalculator.Config;

public class TollFeeConfig
{
    public static readonly TollFeeInterval[] FeeIntervals =
    [
        new TollFeeInterval(new TimeSpan(6, 0, 0), new TimeSpan(6, 29, 59), 8),
        new TollFeeInterval(new TimeSpan(6, 30, 0), new TimeSpan(6, 59, 59), 13),
        new TollFeeInterval(new TimeSpan(7, 0, 0), new TimeSpan(7, 59, 59), 18),
        new TollFeeInterval(new TimeSpan(8, 0, 0), new TimeSpan(8, 29, 59), 13),
        new TollFeeInterval(new TimeSpan(8, 30, 0), new TimeSpan(14, 59, 59), 8),
        new TollFeeInterval(new TimeSpan(15, 0, 0), new TimeSpan(15, 29, 59), 13),
        new TollFeeInterval(new TimeSpan(15, 30, 0), new TimeSpan(16, 59, 59), 18),
        new TollFeeInterval(new TimeSpan(17, 0, 0), new TimeSpan(17, 59, 59), 13),
        new TollFeeInterval(new TimeSpan(18, 0, 0), new TimeSpan(18, 29, 59), 8),
    ];

    public const int MaxDailyFee = 60;
    public const int SingleChargeIntervalMinutes = 60;

    public static readonly int[] TollFreeMonths = [7];  // July

    public HttpClient CreateClient()
    {
        
        var client = new HttpClient(new HttpClientHandler { }, true);
        client.BaseAddress = new Uri("https://api.dagsmart.se");
        return client;
    }

    public async Task<string> GetHolidays()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("/holidays?year=2025&weekends=false");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return string.Empty;
    }

    public static readonly (int month, int day)[] Holidays2013 = new[]
    {
        (1, 1),   // New Year's Day
        (3, 28),  // Maundy Thursday
        (3, 29),  // Good Friday
        (4, 1),   // Easter Monday
        (4, 30),  // King's Birthday
        (5, 1),   // Labour Day
        (5, 8),   // Victory in Europe Day
        (5, 9),   // Ascension Day
        (6, 5),   // National Day
        (6, 6),   // Sweden's National Day
        (6, 21),  // Midsummer Eve
        (11, 1),  // All Saints' Day
        (12, 24), // Christmas Eve
        (12, 25), // Christmas Day
        (12, 26), // Boxing Day
        (12, 31), // New Year's Eve
    };
} 