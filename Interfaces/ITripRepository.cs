using System.Collections.Generic;
using System.Threading.Tasks;
using TransportationManagement.Models;

namespace TransportationManagement.Interfaces
{
	public interface ITripRepository
	{
		Task<List<Trip>> GetAllTripsAsync();
		Task<Trip?> GetTripByIdAsync(int tripId);
		Task<List<Trip>> GetTripsByDriverIdAsync(int driverId);
		Task AddTripAsync(Trip trip);
		Task UpdateTripAsync(Trip trip);
		Task DeleteTripAsync(int tripId);
	}
}