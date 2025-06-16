using Microsoft.Extensions.DependencyInjection;
using TollFeeCalculator.Extensions;
using TollFeeCalculator.Config;
using TollFeeCalculator.Vehicles;

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
        // Set up dependency injection
        var services = new ServiceCollection();

        // Register services for toll calculator
        services.Initialize();

        // Build service provider
        using var serviceProvider = services.BuildServiceProvider();

        // Get the toll calculator service
        var calculator = serviceProvider.GetRequiredService<ITollCalculator>();

        // Create test vehicles
        var vehicles = new List<IVehicle>
        {
            new Car(),
            new Motorbike(),
            new Bus(),
        };

        // Create sample passages
        var passages = new List<DateTime>
        {
            // May 1st (Public Holiday)
            new(2013, 5, 1, 7, 0, 0),

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
        foreach (var vehicle in vehicles)
        {
            Console.WriteLine($"\n=== {vehicle.GetVehicleType()} Toll Fees (Should be Free) ===");
            await DisplayTollFees(calculator, vehicle, passages);
        }
    }

    private static async Task DisplayTollFees(ITollCalculator calculator, IVehicle vehicle, List<DateTime> passages)
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

            if (dayPassages.Any() && totalFee > 0)
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
            else if (totalFee == 0)
            {
                Console.WriteLine($"  Vehicle type '{vehicle.GetVehicleType()}' is toll-free");
            }
        }
    }
}