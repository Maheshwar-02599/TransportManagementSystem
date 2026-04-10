using TransportationManagement.Interfaces;
using TransportationManagement.Models;
namespace TransportationManagement.Services
{
    public class MaintenanceService
    {
        private readonly IMaintenanceRepository _repo;
        public MaintenanceService(IMaintenanceRepository repo) { _repo = repo; }
        public List<MaintenanceRecord> GetAllMaintenanceRecords() => _repo.GetAllMaintenanceRecords();
        public MaintenanceRecord? GetMaintenanceById(int id) => _repo.GetMaintenanceById(id);
        public List<MaintenanceRecord> GetMaintenanceHistory(int vehicleId) => _repo.GetMaintenanceByVehicleId(vehicleId);
        public void ScheduleMaintenance(MaintenanceRecord r) => _repo.AddMaintenance(r);
        public void UpdateServiceRecord(MaintenanceRecord r) => _repo.UpdateMaintenance(r);
        public void DeleteMaintenance(int id) => _repo.DeleteMaintenance(id);
    }
}
