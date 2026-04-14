using TransportationManagement.Interfaces;
using TransportationManagement.Models;
namespace TransportationManagement.Services
{
    public class DriverService
    {
        private readonly IDriverRepository _repo;
        public DriverService(IDriverRepository repo) { _repo = repo; }
        public List<Driver> GetAllDrivers() => _repo.GetAllDrivers();
        public Driver? GetDriverDetails(int id) => _repo.GetDriverById(id);
        public Driver? GetDriverByUserId(int userId) => _repo.GetDriverByUserId(userId);
        public void AddDriver(Driver d) => _repo.AddDriver(d);
        public void UpdateDriver(Driver d) => _repo.UpdateDriver(d);
        public void DeleteDriver(int id) => _repo.DeleteDriver(id);
        public List<Trip> GetAssignedTrips(int driverId, ITripRepository tripRepo) => tripRepo.GetTripsByDriverId(driverId);
    }
}
