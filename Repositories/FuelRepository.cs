using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
    public class FuelRepository : IFuelRepository
    {
        private readonly ApplicationDbContext _context;
        public FuelRepository(ApplicationDbContext context) { _context = context; }

        public List<FuelEntry> GetAllFuelEntries() => _context.FuelEntries.Include(f => f.Vehicle).ToList();

        public FuelEntry? GetFuelEntryById(int fuelId) => _context.FuelEntries.Include(f => f.Vehicle).FirstOrDefault(f => f.fuelId == fuelId);

        public List<FuelEntry> GetFuelEntriesByVehicleId(int vehicleId) => _context.FuelEntries.Include(f => f.Vehicle).Where(f => f.vehicleId == vehicleId).ToList();

        public void AddFuelEntry(FuelEntry f) { _context.FuelEntries.Add(f); _context.SaveChanges(); }

        public void UpdateFuelEntry(FuelEntry f) { _context.FuelEntries.Update(f); _context.SaveChanges(); }

        public void DeleteFuelEntry(int fuelId)
        {
            var f = _context.FuelEntries.Find(fuelId);
            if (f != null) { _context.FuelEntries.Remove(f); _context.SaveChanges(); }
        }
    }
}
