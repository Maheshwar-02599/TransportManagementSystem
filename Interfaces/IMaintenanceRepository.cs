using TransportationManagement.Models;
namespace TransportationManagement.Interfaces
{
    public interface IMaintenanceRepository
    {
        List<MaintenanceRecord> GetAllMaintenanceRecords();
        MaintenanceRecord? GetMaintenanceById(int maintenanceId);
        List<MaintenanceRecord> GetMaintenanceByVehicleId(int vehicleId);
        void AddMaintenance(MaintenanceRecord record);
        void UpdateMaintenance(MaintenanceRecord record);
        void DeleteMaintenance(int maintenanceId);
    }
}
