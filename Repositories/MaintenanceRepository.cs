using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
	public class MaintenanceRepository : IMaintenanceRepository
	{
		private readonly ApplicationDbContext _context;
		public MaintenanceRepository(ApplicationDbContext context) { _context = context; }

		public async Task<List<MaintenanceRecord>> GetAllMaintenanceRecordsAsync()
			=> await _context.MaintenanceRecords.Include(m => m.Vehicle).ToListAsync();

		public async Task<MaintenanceRecord?> GetMaintenanceByIdAsync(int maintenanceId)
			=> await _context.MaintenanceRecords.Include(m => m.Vehicle).FirstOrDefaultAsync(m => m.maintenanceId == maintenanceId);

		public async Task<List<MaintenanceRecord>> GetMaintenanceByVehicleIdAsync(int vehicleId)
			=> await _context.MaintenanceRecords.Include(m => m.Vehicle).Where(m => m.vehicleId == vehicleId).ToListAsync();

		public async Task AddMaintenanceAsync(MaintenanceRecord record)
		{
			await _context.MaintenanceRecords.AddAsync(record);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateMaintenanceAsync(MaintenanceRecord record)
		{
			_context.MaintenanceRecords.Update(record);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteMaintenanceAsync(int maintenanceId)
		{
			var m = await _context.MaintenanceRecords.FindAsync(maintenanceId);
			if (m != null) { _context.MaintenanceRecords.Remove(m); await _context.SaveChangesAsync(); }
		}
	}
}