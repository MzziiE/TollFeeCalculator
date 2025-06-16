using TollFeeCalculator.Enums;

namespace TollFeeCalculator;

public class Motorbike : IVehicle
{
    public string GetVehicleType()
    {
        return "Motorbike";
    }
    public VehicleOwnership VehicleOwnership { get; set; } = VehicleOwnership.Private;
} 