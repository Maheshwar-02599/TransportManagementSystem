using TransportationManagement.Models;
namespace TransportationManagement.Interfaces
{
    public interface ITripRepository
    {
        List<Trip> GetAllTrips();
        Trip? GetTripById(int tripId);
        List<Trip> GetTripsByDriverId(int driverId);
        void AddTrip(Trip trip);
        void UpdateTrip(Trip trip);
        void DeleteTrip(int tripId);
    }
}
