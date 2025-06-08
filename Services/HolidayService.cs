using System.Text.Json;
using TollFeeCalculator.Models;

namespace TollFeeCalculator.Services;

public class HolidayService : IHolidayService
{
    private static readonly Dictionary<int, List<HolidaysMapper>> _cache = new();
    private readonly HttpClient _client;

    public HolidayService(HttpClient client)
    {
        _client = client;
    }

    public async Task<List<HolidaysMapper>> GetHolidays(int year)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(year);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(year, DateTime.Now.Year);

        if (_cache.TryGetValue(year, out var cachedHolidays))
        {
            return cachedHolidays;
        }

        try
        {
            var response = await _client.GetAsync($"https://api.dagsmart.se/holidays?year={year}&weekends=false");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var holidays = JsonSerializer.Deserialize<List<HolidaysMapper>>(content);

            if (holidays != null)
            {
                _cache[year] = holidays;
                return holidays;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching holidays: {ex.Message}");
            // Return hardcoded Swedish holidays for the requested year
            return GetSwedishHolidays(year);
        }

        return GetSwedishHolidays(year);
    }

    private static List<HolidaysMapper> GetSwedishHolidays(int year) =>
        new List<HolidaysMapper>
        {
            new() { Date = new DateTime(year, 1, 1) },
            new() { Date = new DateTime(year, 1, 6) },
            new() { Date = new DateTime(year, 5, 1) },
            new() { Date = new DateTime(year, 6, 6) },
            new() { Date = new DateTime(year, 12, 24) },
            new() { Date = new DateTime(year, 12, 25) },
            new() { Date = new DateTime(year, 12, 26) },
            new() { Date = new DateTime(year, 12, 31) }
        };
}