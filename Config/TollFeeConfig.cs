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
}