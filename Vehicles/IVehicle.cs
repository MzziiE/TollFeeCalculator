using TollFeeCalculator.Enums;

namespace TollFeeCalculator;

public interface IVehicle
{
    string GetVehicleType();
    VehicleOwnership VehicleOwnership { get; set; }
} 