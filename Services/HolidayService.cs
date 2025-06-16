using PublicHoliday;
using System.Text.Json;
using TollFeeCalculator.Models;

namespace TollFeeCalculator.Services;

public class HolidayService : IHolidayService
{
    private static readonly Dictionary<int, List<HolidaysMapper>> _cache = new();
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _apiTimeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="HolidayService"/> class. apiTimeout is set to 5 seconds by default.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="apiTimeout"></param>
    public HolidayService(HttpClient httpClient, TimeSpan? apiTimeout = null)
    {
        _httpClient = httpClient;
        _apiTimeout = apiTimeout ?? TimeSpan.FromSeconds(5);
    }

    public async Task<List<HolidaysMapper>> GetHolidays(int year)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(year);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(year, DateTime.Now.Year);

        if (_cache.TryGetValue(year, out var cachedHolidays))
        {
            return cachedHolidays;
        }

        var holidays = await GetHolidaysWithFallback(year);

        _cache[year] = holidays;
        return holidays;
    }

    private async Task<List<HolidaysMapper>> GetHolidaysWithFallback(int year)
    {
        try
        {
            using var cts = new CancellationTokenSource(_apiTimeout);
            var apiHolidays = await GetHolidaysFromApi(year, cts.Token);
            if (apiHolidays.Any())
            {
                _cache[year] = apiHolidays;
                return apiHolidays;
            }

            var publicHolidays = new SwedenPublicHoliday()
                .PublicHolidayNames(year)
                .Select(d => new HolidaysMapper
                {
                    Date = d.Key,
                    Name = new Name { Sv = d.Value }
                }).ToList();

            if (publicHolidays.Any())
            {
                _cache[year] = publicHolidays;
                return publicHolidays;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PublicHoliday library failed: {ex.Message}");
        }

        // Strategy 2: Try API call (online, may fail)
        try
        {
            using var cts = new CancellationTokenSource(_apiTimeout);
            var apiHolidays = await GetHolidaysFromApi(year, cts.Token);
            if (apiHolidays.Any())
            {
                _cache[year] = apiHolidays;
                return apiHolidays;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"API call timed out after {_apiTimeout.TotalSeconds} seconds");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call failed: {ex.Message}");
        }

        // Strategy 3: Fallback to hardcoded holidays (always works)
        Console.WriteLine($"Using fallback holidays for year {year}");
        return GetFallbackHolidays(year);
    }

    private async Task<List<HolidaysMapper>> GetHolidaysFromApi(int year, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            $"https://api.dagsmart.se/holidays?year={year}&weekends=false",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Empty response from holidays API");
        }

        var holidays = JsonSerializer.Deserialize<List<HolidaysMapper>>(content);

        return holidays ?? throw new InvalidOperationException("Failed to deserialize holidays data");
    }

    private static List<HolidaysMapper> GetFallbackHolidays(int year)
    {
        return new List<HolidaysMapper>
        {
            new() { Date = new DateTime(year, 1, 1), Name = new Name { Sv = "Nyårsdagen" } },
            new() { Date = new DateTime(year, 1, 6), Name = new Name { Sv = "Trettondedag jul" } },
            new() { Date = new DateTime(year, 5, 1), Name = new Name { Sv = "Första maj" } },
            new() { Date = new DateTime(year, 6, 6), Name = new Name { Sv = "Sveriges nationaldag" } },
            new() { Date = new DateTime(year, 12, 24), Name = new Name { Sv = "Julafton" } },
            new() { Date = new DateTime(year, 12, 25), Name = new Name { Sv = "Juldagen" } },
            new() { Date = new DateTime(year, 12, 26), Name = new Name { Sv = "Annandag jul" } },
            new() { Date = new DateTime(year, 12, 31), Name = new Name { Sv = "Nyårsafton" } }
        };
    }
}