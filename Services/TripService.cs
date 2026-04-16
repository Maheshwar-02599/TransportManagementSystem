using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
    public class TripService
    {
        private readonly ITripRepository _repo;
        public TripService(ITripRepository repo) { _repo = repo; }

        public List<Trip> GetAllTrips() => _repo.GetAllTrips();
        public Trip? GetTripPlan(int id) => _repo.GetTripById(id);
        public List<Trip> GetAssignedTrips(int driverId) => _repo.GetTripsByDriverId(driverId);
        public void CreateTrip(Trip t) => _repo.AddTrip(t);
        public void UpdateTripStatus(Trip t) => _repo.UpdateTrip(t);
        public void DeleteTrip(int id) => _repo.DeleteTrip(id);
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