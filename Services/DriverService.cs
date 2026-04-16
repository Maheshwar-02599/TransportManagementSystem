using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
    public class DriverService
    {
        private readonly IDriverRepository _driverRepo;
        private readonly ITripRepository _tripRepo; // Injected for security & decoupling

        public DriverService(IDriverRepository driverRepo, ITripRepository tripRepo)
        {
            _driverRepo = driverRepo;
            _tripRepo = tripRepo;
        }

        public List<Driver> GetAllDrivers() => _driverRepo.GetAllDrivers();
        public Driver? GetDriverDetails(int id) => _driverRepo.GetDriverById(id);
        public Driver? GetDriverByUserId(int userId) => _driverRepo.GetDriverByUserId(userId);
        public void AddDriver(Driver d) => _driverRepo.AddDriver(d);
        public void UpdateDriver(Driver d) => _driverRepo.UpdateDriver(d);
        public void DeleteDriver(int id) => _driverRepo.DeleteDriver(id);

        // Logic: The Controller no longer needs to pass the repository in
        public List<Trip> GetAssignedTrips(int driverId)
        {
            return _tripRepo.GetTripsByDriverId(driverId);
        }
    }
}