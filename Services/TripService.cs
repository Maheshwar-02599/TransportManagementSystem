using System.Collections.Generic;
using System.Threading.Tasks;
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
		public List<Trip> GetTripsByDriverEmail(string email)
		{
			// We filter trips where the associated Driver's contact/email matches the logged in user
			// Or, more accurately, we find trips where the Driver record is linked to that User
			return _repo.GetAllTrips()
						.Where(t => t.Driver != null && t.Driver.contactNumber == email || t.Driver.name == email)
						.ToList();
		}
	}
}