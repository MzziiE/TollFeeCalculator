namespace TollFeeCalculator.Models;

public class TollFeeInterval
{
    public TimeSpan StartTime { get; }
    public TimeSpan EndTime { get; }
    public int Fee { get; }

    public TollFeeInterval(TimeSpan startTime, TimeSpan endTime, int fee)
    {
        if (startTime > endTime)
        {
            throw new ArgumentException("Start time must be before end time");
        }

        StartTime = startTime;
        EndTime = endTime;
        Fee = fee;
    }

    public bool IsWithinInterval(DateTime date)
    {
        TimeSpan time = date.TimeOfDay;
        return time >= StartTime && time <= EndTime;
    }
} 