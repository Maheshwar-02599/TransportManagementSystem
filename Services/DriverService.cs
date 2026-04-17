using System.Collections.Generic;
using System.Threading.Tasks;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
	public class DriverService
	{
		private readonly IDriverRepository _repo;
		public DriverService(IDriverRepository repo) { _repo = repo; }

		public async Task<List<Driver>> GetAllDriversAsync() => await _repo.GetAllDriversAsync();
		public async Task<Driver?> GetDriverDetailsAsync(int id) => await _repo.GetDriverByIdAsync(id);
		public async Task<Driver?> GetDriverByUserIdAsync(int userId) => await _repo.GetDriverByUserIdAsync(userId);
		public async Task AddDriverAsync(Driver d) => await _repo.AddDriverAsync(d);
		public async Task UpdateDriverAsync(Driver d) => await _repo.UpdateDriverAsync(d);
		public async Task DeleteDriverAsync(int id) => await _repo.DeleteDriverAsync(id);

		// Updated to use the async version of the TripRepo method
		public async Task<List<Trip>> GetAssignedTripsAsync(int driverId, ITripRepository tripRepo)
			=> await tripRepo.GetTripsByDriverIdAsync(driverId);
	}
} 