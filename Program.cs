using TollFeeCalculator.Config;
using TollFeeCalculator.Exceptions;

namespace TollFeeCalculator;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            await RunTollCalculatorDemo();
        }
        catch (TollCalculatorException ex)
        {
            Console.WriteLine($"Toll calculation error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static async Task RunTollCalculatorDemo()
    {
        // Create instances of vehicles
        var car = new Car();
        var motorbike = new Motorbike();

        var holidays = await new TollFeeConfig().GetHolidays();

        // Create passages across multiple days
        var passages = new DateTime[]
        {
            // Day 1 - May 15
            new DateTime(2013, 5, 15, 7, 0, 0),   // 7:00 AM - Peak hour (18 kr)
            new DateTime(2013, 5, 15, 7, 30, 0),  // 7:30 AM - Within same hour (highest fee applies)
            new DateTime(2013, 5, 15, 15, 45, 0), // 3:45 PM - Another peak hour (18 kr)
            new DateTime(2013, 5, 15, 16, 2, 0),  // 4:02 PM - Within 60 min of previous (highest fee applies)
            new DateTime(2013, 5, 15, 18, 0, 0),  // 6:00 PM - Off-peak (8 kr)

            // Day 2 - May 16
            new DateTime(2013, 5, 16, 8, 0, 0),   // 8:00 AM - Peak hour (13 kr)
            new DateTime(2013, 5, 16, 14, 35, 0), // 2:35 PM - Off-peak (8 kr)
            new DateTime(2013, 5, 16, 16, 0, 0),  // 4:00 PM - Peak hour (18 kr)

            // Day 3 - July 1 (Free - July is toll-free)
            new DateTime(2013, 7, 1, 8, 0, 0),
            new DateTime(2013, 7, 1, 16, 0, 0),
        };

        var calculator = new TollCalculator();

        // Calculate tolls for multiple days
        Console.WriteLine("=== Multi-day Car Passages ===");
        var carTollFees = calculator.GetTollFees(car, passages);

        foreach (var (date, fee) in carTollFees.OrderBy(x => x.Key))
        {
            Console.WriteLine($"\nDate: {date:yyyy-MM-dd}");
            Console.WriteLine($"Total toll fee: {fee} kr");

            // Show individual passage fees for this day
            var dayPassages = passages.Where(p => p.Date == date);
            foreach (var passage in dayPassages)
            {
                var passageFee = calculator.GetTollFee(passage, car);
                Console.WriteLine($"  Passage at {passage:HH:mm}: {passageFee} kr");
            }
        }

        // Calculate tolls for motorbike (should be free)
        Console.WriteLine("\n=== Multi-day Motorbike Passages (Should be Free) ===");
        var bikeTollFees = calculator.GetTollFees(motorbike, passages);

        foreach (var (date, fee) in bikeTollFees.OrderBy(x => x.Key))
        {
            Console.WriteLine($"Date: {date:yyyy-MM-dd}, Total toll fee: {fee} kr");
        }
    }
}