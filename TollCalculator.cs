using System.Data;
using TollFeeCalculator.Config;
using TollFeeCalculator.Exceptions;
using TollFeeCalculator.Extensions;
using TollFeeCalculator.Services;

namespace TollFeeCalculator;

public class TollCalculator
{
    private readonly IHolidayService _holidayService;

    public TollCalculator(IHolidayService holidayService)
    {
        _holidayService = holidayService;
    }

    /// <summary>
    /// Calculate toll fees for multiple days
    /// </summary>
    /// <param name="vehicle">The vehicle making the passages</param>
    /// <param name="dates">Date and time of all passages across any number of days</param>
    /// <returns>Dictionary with dates as keys and total toll fees as values</returns>
    public async Task<Dictionary<DateTime, int>> GetTollFees(IVehicle vehicle, List<DateTime> dates)
    {
        if (dates == null || !dates.Any())
        {
            throw new InvalidPassageDatesException("Passage dates cannot be null or empty");
        }

        var result = new Dictionary<DateTime, int>();

        foreach (var group in dates.GroupBy(d => d.Date))
        {
            // First check if this date is toll-free
            if (await IsTollFreeDate(group.Key))
            {
                result.Add(group.Key, 0);
                continue;
            }

            // If not toll-free, calculate the fee
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
        ValidatePassages(dates);

        if (IsTollFreeVehicle(vehicle))
        {
            return 0;
        }

        var sortedDates = dates.OrderBy(d => d).ToList();
        var totalFee = 0;
        var currentIntervalStart = sortedDates[0];
        var currentIntervalFee = await GetSinglePassageFee(currentIntervalStart, vehicle);

        foreach (var date in sortedDates.Skip(1))
        {
            var fee = await GetSinglePassageFee(date, vehicle);
            var timeDiff = date - currentIntervalStart;

            if (timeDiff.TotalMinutes > TollFeeConfig.SingleChargeIntervalMinutes)
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
        if (vehicle == null) return false;
        return Enum.TryParse<TollFreeVehicles>(vehicle.GetVehicleType(), out _);
    }

    private async Task<bool> IsTollFreeDate(DateTime date)
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return true;
        }

        if (TollFeeConfig.TollFreeMonths.Contains(date.Month))
        {
            return true;
        }

        var holidays = await _holidayService.GetHolidays(date.Year);
        return holidays.Any(h => h.Date == date.Date);
    }

    private enum TollFreeVehicles
    {
        Motorbike,
        Tractor,
        Emergency,
        Diplomat,
        Foreign,
        Military
    }
}