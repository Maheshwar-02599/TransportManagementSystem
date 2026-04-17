using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
	public class DriverRepository : IDriverRepository
	{
		private readonly ApplicationDbContext _context;
		public DriverRepository(ApplicationDbContext context) { _context = context; }

		public async Task<List<Driver>> GetAllDriversAsync()
			=> await _context.Drivers.ToListAsync();

		public async Task<Driver?> GetDriverByIdAsync(int driverId)
			=> await _context.Drivers.FindAsync(driverId);

		public async Task<Driver?> GetDriverByUserIdAsync(int userId)
			=> await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

		public async Task AddDriverAsync(Driver driver)
		{
			await _context.Drivers.AddAsync(driver);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateDriverAsync(Driver driver)
		{
			_context.Drivers.Update(driver);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteDriverAsync(int driverId)
		{
			var d = await _context.Drivers.FindAsync(driverId);
			if (d != null)
			{
				_context.Drivers.Remove(d);
				await _context.SaveChangesAsync();
			}
		}
	}
}