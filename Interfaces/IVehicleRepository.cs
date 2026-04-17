using TransportationManagement.Models;

namespace TransportationManagement.Interfaces
{
	public interface IVehicleRepository
	{
		Task<List<Vehicle>> GetAllVehiclesAsync();
		Task<Vehicle?> GetVehicleByIdAsync(int vehicleId);
		Task AddVehicleAsync(Vehicle vehicle);
		Task UpdateVehicleAsync(Vehicle vehicle);
		Task DeleteVehicleAsync(int vehicleId);
	}
}