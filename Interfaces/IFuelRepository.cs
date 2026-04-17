using System.Collections.Generic;
using System.Threading.Tasks;
using TransportationManagement.Models;

namespace TransportationManagement.Interfaces
{
	public interface IFuelRepository
	{
		Task<List<FuelEntry>> GetAllFuelEntriesAsync();
		Task<FuelEntry?> GetFuelEntryByIdAsync(int fuelId);
		Task<List<FuelEntry>> GetFuelEntriesByVehicleIdAsync(int vehicleId);
		Task AddFuelEntryAsync(FuelEntry fuelEntry);
		Task UpdateFuelEntryAsync(FuelEntry fuelEntry);
		Task DeleteFuelEntryAsync(int fuelId);
	}
}