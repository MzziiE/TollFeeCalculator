namespace TollFeeCalculator;

public interface ITollCalculator
{
    /// <summary>
    /// Calculate toll fees for multiple days
    /// </summary>
    /// <param name="vehicle">The vehicle making the passages</param>
    /// <param name="dates">Date and time of all passages across any number of days</param>
    /// <returns>Dictionary with dates as keys and total toll fees as values</returns>
    Task<Dictionary<DateTime, int>> GetTollFees(IVehicle vehicle, List<DateTime> dates);

    /// <summary>
    /// Calculate the total toll fee for one day
    /// </summary>
    /// <param name="vehicle">The vehicle making the passages</param>
    /// <param name="dates">Date and time of all passages on one day</param>
    /// <returns>The total toll fee for that day</returns>
    Task<int> GetTollFee(IVehicle vehicle, List<DateTime> dates);
}