using System.Collections.Generic;
using System.Threading.Tasks;
using TransportationManagement.Models;

namespace TransportationManagement.Interfaces
{
	public interface IMaintenanceRepository
	{
		Task<List<MaintenanceRecord>> GetAllMaintenanceRecordsAsync();
		Task<MaintenanceRecord?> GetMaintenanceByIdAsync(int maintenanceId);
		Task<List<MaintenanceRecord>> GetMaintenanceByVehicleIdAsync(int vehicleId);
		Task AddMaintenanceAsync(MaintenanceRecord record);
		Task UpdateMaintenanceAsync(MaintenanceRecord record);
		Task DeleteMaintenanceAsync(int maintenanceId);
	}
}