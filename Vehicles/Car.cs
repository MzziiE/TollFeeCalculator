using TollFeeCalculator.Enums;

namespace TollFeeCalculator;

public class Car : IVehicle
{
    public string GetVehicleType()
    {
        return "Car";
    }
    public VehicleOwnership VehicleOwnership { get; set; } = VehicleOwnership.Private;
} 