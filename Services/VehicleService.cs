using System.Collections.Generic;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
	public class VehicleService
	{
		private readonly IVehicleRepository _repo;
		public VehicleService(IVehicleRepository repo) { _repo = repo; }

		public async Task<List<Vehicle>> GetAllVehiclesAsync() => await _repo.GetAllVehiclesAsync();

		public async Task<Vehicle?> GetVehicleDetailsAsync(int id) => await _repo.GetVehicleByIdAsync(id);

		public async Task AddVehicleAsync(Vehicle v) => await _repo.AddVehicleAsync(v);

		public async Task UpdateVehicleAsync(Vehicle v) => await _repo.UpdateVehicleAsync(v);

		public async Task DeleteVehicleAsync(int id) => await _repo.DeleteVehicleAsync(id);
		public List<Vehicle> ListAllVehicles() => _repo.GetAllVehicles();
	}
}