using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
	public class FuelRepository : IFuelRepository
	{
		private readonly ApplicationDbContext _context;
		public FuelRepository(ApplicationDbContext context) { _context = context; }

		public async Task<List<FuelEntry>> GetAllFuelEntriesAsync()
			=> await _context.FuelEntries.Include(f => f.Vehicle).ToListAsync();

		public async Task<FuelEntry?> GetFuelEntryByIdAsync(int fuelId)
			=> await _context.FuelEntries.Include(f => f.Vehicle).FirstOrDefaultAsync(f => f.fuelId == fuelId);

		public async Task<List<FuelEntry>> GetFuelEntriesByVehicleIdAsync(int vehicleId)
			=> await _context.FuelEntries.Include(f => f.Vehicle).Where(f => f.vehicleId == vehicleId).ToListAsync();

		public async Task AddFuelEntryAsync(FuelEntry f)
		{
			await _context.FuelEntries.AddAsync(f);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateFuelEntryAsync(FuelEntry f)
		{
			_context.FuelEntries.Update(f);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteFuelEntryAsync(int fuelId)
		{
			var f = await _context.FuelEntries.FindAsync(fuelId);
			if (f != null) { _context.FuelEntries.Remove(f); await _context.SaveChangesAsync(); }
		}
	}
}