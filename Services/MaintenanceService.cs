using System.Collections.Generic;
using System.Threading.Tasks;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
	public class MaintenanceService
	{
		private readonly IMaintenanceRepository _repo;
		public MaintenanceService(IMaintenanceRepository repo) { _repo = repo; }

		public async Task<List<MaintenanceRecord>> GetAllMaintenanceRecordsAsync() => await _repo.GetAllMaintenanceRecordsAsync();
		public async Task<MaintenanceRecord?> GetMaintenanceByIdAsync(int id) => await _repo.GetMaintenanceByIdAsync(id);
		public async Task<List<MaintenanceRecord>> GetMaintenanceHistoryAsync(int vehicleId) => await _repo.GetMaintenanceByVehicleIdAsync(vehicleId);

		public async Task ScheduleMaintenanceAsync(MaintenanceRecord r) => await _repo.AddMaintenanceAsync(r);
		public async Task UpdateServiceRecordAsync(MaintenanceRecord r) => await _repo.UpdateMaintenanceAsync(r);
		public async Task DeleteMaintenanceAsync(int id) => await _repo.DeleteMaintenanceAsync(id);
	}
}