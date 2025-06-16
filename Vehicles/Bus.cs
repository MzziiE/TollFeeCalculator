using TollFeeCalculator.Enums;

namespace TollFeeCalculator.Vehicles;

public class Bus : IVehicle
{
    public string GetVehicleType()
    {
        return "Bus";
    }
    public VehicleOwnership VehicleOwnership { get; set; } = VehicleOwnership.Private;
}