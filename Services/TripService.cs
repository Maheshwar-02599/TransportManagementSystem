using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
	public class TripService
	{
		private readonly ITripRepository _repo;
		public TripService(ITripRepository repo) { _repo = repo; }

		public async Task<List<Trip>> GetAllTripsAsync() => await _repo.GetAllTripsAsync();
		public async Task<Trip?> GetTripPlanAsync(int id) => await _repo.GetTripByIdAsync(id);
		public async Task<List<Trip>> GetAssignedTripsAsync(int driverId) => await _repo.GetTripsByDriverIdAsync(driverId);
		public async Task CreateTripAsync(Trip t) => await _repo.AddTripAsync(t);
		public async Task UpdateTripStatusAsync(Trip t) => await _repo.UpdateTripAsync(t);
		public async Task DeleteTripAsync(int id) => await _repo.DeleteTripAsync(id);
		public async Task<List<Trip>> GetTripsByDriverEmail(string email)
		{
			return await _repo.GetAllTripsEmail(email);
		}
	}
}