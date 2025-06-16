using System.Data;
using TollFeeCalculator.Config;
using TollFeeCalculator.Exceptions;
using TollFeeCalculator.Services;

namespace TollFeeCalculator;

public class TollCalculator : ITollCalculator
{
    private readonly IHolidayService _holidayService;
    private readonly Dictionary<DateTime, bool> _tollFreeDateCache = new();

    public TollCalculator(IHolidayService holidayService)
    {
        _holidayService = holidayService ?? throw new ArgumentNullException(nameof(holidayService));
    }

    /// <summary>
    /// Calculate toll fees for multiple days
    /// </summary>
    /// <param name="vehicle">The vehicle making the passages</param>
    /// <param name="dates">Date and time of all passages across any number of days</param>
    /// <returns>Dictionary with dates as keys and total toll fees as values</returns>
    public async Task<Dictionary<DateTime, int>> GetTollFees(IVehicle vehicle, List<DateTime> dates)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        if (dates == null || !dates.Any())
        {
            throw new InvalidPassageDatesException("Passage dates cannot be null or empty");
        }

        var result = new Dictionary<DateTime, int>();

        // Pre-check all unique dates for toll-free status to minimize API calls
        var uniqueDates = dates.Select(d => d.Date).Distinct().ToList();
        var tollFreeDates = new HashSet<DateTime>();

        foreach (var date in uniqueDates)
        {
            if (await IsTollFreeDate(date))
            {
                tollFreeDates.Add(date);
            }
        }

        foreach (var group in dates.GroupBy(d => d.Date))
        {
            if (tollFreeDates.Contains(group.Key))
            {
                result.Add(group.Key, 0);
                continue;
            }

            var dailyFee = await GetTollFee(vehicle, group.ToList());
            result.Add(group.Key, dailyFee);
        }

        return result;
    }

    /// <summary>
    /// Calculate the total toll fee for one day
    /// </summary>
    /// <param name="vehicle">The vehicle making the passages</param>
    /// <param name="dates">Date and time of all passages on one day</param>
    /// <returns>The total toll fee for that day</returns>
    /// <exception cref="InvalidPassageDatesException">Thrown when dates array is null or empty</exception>
    /// <exception cref="MultipleDaysException">Thrown when passages span multiple days</exception>
    public async Task<int> GetTollFee(IVehicle vehicle, List<DateTime> dates)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ValidatePassages(dates);

        if (IsTollFreeVehicle(vehicle))
        {
            return 0;
        }

        // Early exit if only one passage
        if (dates.Count == 1)
        {
            return await GetSinglePassageFee(dates[0], vehicle);
        }

        var sortedDates = dates.OrderBy(d => d).ToList();
        var totalFee = 0;
        var currentIntervalStart = sortedDates[0];
        var currentIntervalFee = await GetSinglePassageFee(currentIntervalStart, vehicle);

        foreach (var date in sortedDates.Skip(1))
        {
            var fee = await GetSinglePassageFee(date, vehicle);
            var timeDiff = date - currentIntervalStart;

            if (timeDiff.TotalMinutes >= TollFeeConfig.SingleChargeIntervalMinutes)
            {
                totalFee += currentIntervalFee;
                currentIntervalStart = date;
                currentIntervalFee = fee;
            }
            else
            {
                currentIntervalFee = Math.Max(currentIntervalFee, fee);
            }
        }

        totalFee += currentIntervalFee;
        return Math.Min(totalFee, TollFeeConfig.MaxDailyFee);
    }

    private async Task<int> GetSinglePassageFee(DateTime date, IVehicle vehicle)
    {
        if (await IsTollFreeDate(date) || IsTollFreeVehicle(vehicle))
        {
            return 0;
        }

        return TollFeeConfig.FeeIntervals
            .FirstOrDefault(interval => interval.IsWithinInterval(date))
            ?.Fee ?? 0;
    }

    private void ValidatePassages(List<DateTime> dates)
    {
        if (dates == null || !dates.Any())
        {
            throw new InvalidPassageDatesException("Passage dates cannot be null or empty");
        }

        var distinctDays = dates.Select(d => d.Date).Distinct();
        if (distinctDays.Count() > 1)
        {
            throw new MultipleDaysException("All passages must be on the same day");
        }
    }

    private bool IsTollFreeVehicle(IVehicle vehicle)
    {
        if (vehicle == null)
        {
            return false;
        }
        return Enum.TryParse<TollFreeVehicles>(vehicle.GetVehicleType(), out _);
    }

    private async Task<bool> IsTollFreeDate(DateTime date)
    {
        var dateOnly = date.Date;

        // Check cache first
        if (_tollFreeDateCache.TryGetValue(dateOnly, out var cachedResult))
        {
            return cachedResult;
        }

        var isTollFree = await CheckIfTollFreeDate(dateOnly);

        // Cache the result
        _tollFreeDateCache[dateOnly] = isTollFree;
        return isTollFree;
    }

    private async Task<bool> CheckIfTollFreeDate(DateTime date)
    {
        // Weekends are always toll-free
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return true;
        }

        // Toll-free months (e.g., July)
        if (TollFeeConfig.TollFreeMonths.Contains(date.Month))
        {
            return true;
        }

        // Check holidays
        try
        {
            var holidays = await _holidayService.GetHolidays(date.Year);
            return holidays.Any(h => h.Date.Date == date.Date);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking holidays for {date:yyyy-MM-dd}: {ex.Message}");
            // If we can't check holidays, assume it's not toll-free (safer default)
            return false;
        }
    }

    private enum TollFreeVehicles
    {
        Motorbike,
        Tractor,
        Emergency,
        Diplomat,
        Foreign,
        Military,
        Bus
    }
}