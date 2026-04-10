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
    }
}
