using TollFeeCalculator.Config;
using TollFeeCalculator.Services;

namespace TollFeeCalculator;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            await RunTollCalculatorDemo();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static async Task RunTollCalculatorDemo()
    {
        using var httpClient = new HttpClient();
        // Add your API key here if needed
        // httpClient.DefaultRequestHeaders.Add("X-Api-Key", "your-api-key");

        var holidayService = new HolidayService(httpClient);
        var calculator = new TollCalculator(holidayService);

        // Create test vehicles
        var car = new Car();
        var motorbike = new Motorbike();

        // Create sample passages
        var passages = new List<DateTime>
        {
            // May 1st (Public Holiday)
            new(2013, 5, 1, 7, 0, 0),
            new(2025, 5, 2, 8, 0, 0),

            // May 15th
            new(2013, 5, 15, 7, 0, 0),   // 18 kr
            new(2013, 5, 15, 7, 30, 0),  // Within 60 min of previous
            new(2013, 5, 15, 15, 45, 0), // 18 kr
            new(2013, 5, 15, 16, 2, 0),  // Within 60 min of previous
            new(2013, 5, 15, 18, 0, 0),  // 8 kr

            // May 16th
            new(2013, 5, 16, 8, 0, 0),   // 13 kr
            new(2013, 5, 16, 14, 35, 0), // 8 kr
            new(2013, 5, 16, 16, 0, 0),  // 18 kr

            // July 1st (Toll-free month)
            new(2013, 7, 1, 8, 0, 0),
            new(2013, 7, 1, 16, 0, 0),
        };

        // Calculate and display results for car
        Console.WriteLine("=== Car Toll Fees ===");
        await DisplayTollFees(calculator, car, passages);

        // Calculate and display results for motorbike (should be free)
        Console.WriteLine("\n=== Motorbike Toll Fees (Should be Free) ===");
        await DisplayTollFees(calculator, motorbike, passages);
    }

    private static async Task DisplayTollFees(TollCalculator calculator, IVehicle vehicle, List<DateTime> passages)
    {
        var fees = await calculator.GetTollFees(vehicle, passages);

        foreach (var (date, totalFee) in fees.OrderBy(x => x.Key))
        {
            Console.WriteLine($"\nDate: {date:yyyy-MM-dd}");
            Console.WriteLine($"Total toll fee: {totalFee} kr");

            // Show passages that affect the fee
            var dayPassages = passages
                .Where(p => p.Date == date)
                .OrderBy(p => p)
                .ToList();

            if (dayPassages.Any())
            {
                var lastTime = dayPassages[0];
                var lastFee = await calculator.GetTollFee(vehicle, new List<DateTime> { lastTime });
                if (lastFee > 0)
                {
                    Console.WriteLine($"  Passage at {lastTime:HH:mm}: {lastFee} kr");
                }

                foreach (var passage in dayPassages.Skip(1))
                {
                    var timeDiff = passage - lastTime;
                    if (timeDiff.TotalMinutes > TollFeeConfig.SingleChargeIntervalMinutes)
                    {
                        var fee = await calculator.GetTollFee(vehicle, new List<DateTime> { passage });
                        if (fee > 0)
                        {
                            Console.WriteLine($"  Passage at {passage:HH:mm}: {fee} kr");
                            lastTime = passage;
                        }
                    }
                }
            }
        }
    }
}