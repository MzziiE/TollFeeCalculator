using TollFeeCalculator.Models;

namespace TollFeeCalculator.Services;

public interface IHolidayService
{
    Task<List<HolidaysMapper>> GetHolidays(int year);
}