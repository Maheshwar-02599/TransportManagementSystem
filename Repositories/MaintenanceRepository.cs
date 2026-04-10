using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
    public class MaintenanceRepository : IMaintenanceRepository
    {
        private readonly ApplicationDbContext _context;
        public MaintenanceRepository(ApplicationDbContext context) { _context = context; }

        public List<MaintenanceRecord> GetAllMaintenanceRecords() => _context.MaintenanceRecords.Include(m => m.Vehicle).ToList();

        public MaintenanceRecord? GetMaintenanceById(int maintenanceId) => _context.MaintenanceRecords.Include(m => m.Vehicle).FirstOrDefault(m => m.maintenanceId == maintenanceId);

        public List<MaintenanceRecord> GetMaintenanceByVehicleId(int vehicleId) => _context.MaintenanceRecords.Include(m => m.Vehicle).Where(m => m.vehicleId == vehicleId).ToList();

        public void AddMaintenance(MaintenanceRecord record) { _context.MaintenanceRecords.Add(record); _context.SaveChanges(); }

        public void UpdateMaintenance(MaintenanceRecord record) { _context.MaintenanceRecords.Update(record); _context.SaveChanges(); }

        public void DeleteMaintenance(int maintenanceId)
        {
            var m = _context.MaintenanceRecords.Find(maintenanceId);
            if (m != null) { _context.MaintenanceRecords.Remove(m); _context.SaveChanges(); }
        }
    }
}
