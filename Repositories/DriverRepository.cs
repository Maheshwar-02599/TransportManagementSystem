using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly ApplicationDbContext _context;
        public DriverRepository(ApplicationDbContext context) { _context = context; }

        public List<Driver> GetAllDrivers() => _context.Drivers.ToList();

        public Driver? GetDriverById(int driverId) => _context.Drivers.Find(driverId);

        public void AddDriver(Driver driver) { _context.Drivers.Add(driver); _context.SaveChanges(); }

        public void UpdateDriver(Driver driver) { _context.Drivers.Update(driver); _context.SaveChanges(); }

        public void DeleteDriver(int driverId)
        {
            var d = _context.Drivers.Find(driverId);
            if (d != null) { _context.Drivers.Remove(d); _context.SaveChanges(); }
        }
    }
}
