using System.Collections.Generic;
using System.Threading.Tasks;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
	public class FuelService
	{
		private readonly IFuelRepository _repo;
		public FuelService(IFuelRepository repo) { _repo = repo; }

		public async Task<List<FuelEntry>> GetAllFuelEntriesAsync() => await _repo.GetAllFuelEntriesAsync();
		public async Task<FuelEntry?> GetFuelEntryByIdAsync(int id) => await _repo.GetFuelEntryByIdAsync(id);
		public async Task<List<FuelEntry>> GetFuelConsumptionAsync(int vehicleId) => await _repo.GetFuelEntriesByVehicleIdAsync(vehicleId);
		public async Task<List<FuelEntry>> GenerateFuelReportAsync() => await _repo.GetAllFuelEntriesAsync();

		public async Task AddFuelEntryAsync(FuelEntry f) => await _repo.AddFuelEntryAsync(f);
		public async Task UpdateFuelEntryAsync(FuelEntry f) => await _repo.UpdateFuelEntryAsync(f);
		public async Task DeleteFuelEntryAsync(int id) => await _repo.DeleteFuelEntryAsync(id);
	}
}