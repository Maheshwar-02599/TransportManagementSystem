using TransportationManagement.Models;
namespace TransportationManagement.Interfaces
{
    public interface IVehicleRepository
    {
        List<Vehicle> GetAllVehicles();
        Vehicle? GetVehicleById(int vehicleId);
        void AddVehicle(Vehicle vehicle);
        void UpdateVehicle(Vehicle vehicle);
        void DeleteVehicle(int vehicleId);
    }
}
