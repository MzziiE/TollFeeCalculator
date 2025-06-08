using System;
using TollFeeCalculator.Config;
using TollFeeCalculator.Exceptions;

namespace TollFeeCalculator;

public class TollCalculator
{
    /// <summary>
    /// Calculate toll fees for multiple days
    /// </summary>
    /// <param name="vehicle">The vehicle making the passages</param>
    /// <param name="dates">Date and time of all passages across any number of days</param>
    /// <returns>Dictionary with dates as keys and total toll fees as values</returns>
    public Dictionary<DateTime, int> GetTollFees(IVehicle vehicle, DateTime[] dates)
    {
        if (dates == null || dates.Length == 0)
        {
            throw new InvalidPassageDatesException("Passage dates cannot be null or empty");
        }

        return dates
            .GroupBy(d => d.Date)
            .ToDictionary(
                group => group.Key,
                group => GetTollFee(vehicle, group.ToArray())
            );
    }

    /// <summary>
    /// Calculate the total toll fee for one day
    /// </summary>
    /// <param name="vehicle">The vehicle making the passages</param>
    /// <param name="dates">Date and time of all passages on one day</param>
    /// <returns>The total toll fee for that day</returns>
    /// <exception cref="InvalidPassageDatesException">Thrown when dates array is null or empty</exception>
    /// <exception cref="MultipleDaysException">Thrown when passages span multiple days</exception>
    public int GetTollFee(IVehicle vehicle, DateTime[] dates)
    {
        ValidatePassages(dates);

        if (IsTollFreeVehicle(vehicle))
        {
            return 0;
        }

        var sortedDates = dates.OrderBy(d => d).ToArray();
        DateTime intervalStart = sortedDates[0];
        int totalFee = 0;
        int tempFee = GetTollFee(intervalStart, vehicle);

        foreach (DateTime date in sortedDates.Skip(1))
        {
            int nextFee = GetTollFee(date, vehicle);
            TimeSpan timeDiff = date - intervalStart;

            if (timeDiff.TotalMinutes <= TollFeeConfig.SingleChargeIntervalMinutes)
            {
                // Within single charge interval, only charge the highest fee
                if (nextFee > tempFee)
                {
                    totalFee = totalFee - tempFee + nextFee;
                    tempFee = nextFee;
                }
            }
            else
            {
                // New interval starts
                totalFee += nextFee;
                intervalStart = date;
                tempFee = nextFee;
            }
        }

        // Add the fee for the first passage
        totalFee += tempFee;

        return Math.Min(totalFee, TollFeeConfig.MaxDailyFee);
    }

    private void ValidatePassages(DateTime[] dates)
    {
        if (dates == null || dates.Length == 0)
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

        string vehicleType = vehicle.GetVehicleType();
        return Enum.TryParse<TollFreeVehicles>(vehicleType, out var _);
    }

    public int GetTollFee(DateTime date, IVehicle vehicle)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle))
        {
            return 0;
        }

        foreach (var interval in TollFeeConfig.FeeIntervals)
        {
            if (interval.IsWithinInterval(date))
            {
                return interval.Fee;
            }
        }

        return 0; // Free passage outside of toll hours
    }

    private bool IsTollFreeDate(DateTime date)
    {
        // Free on weekends
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            return true;
        }

        // Free during toll-free months (e.g., July)
        if (TollFeeConfig.TollFreeMonths.Contains(date.Month))
        {
            return true;
        }

        // Free on holidays (only 2013 implemented)
        if (date.Year == 2013)
        {
            return TollFeeConfig.Holidays2013.Contains((date.Month, date.Day));
        }

        return false;
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