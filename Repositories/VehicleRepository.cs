using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
	public class VehicleRepository : IVehicleRepository
	{
		private readonly ApplicationDbContext _context;
		public VehicleRepository(ApplicationDbContext context) { _context = context; }

		public async Task<List<Vehicle>> GetAllVehiclesAsync()
			=> await _context.Vehicles.ToListAsync();

		public async Task<Vehicle?> GetVehicleByIdAsync(int vehicleId)
			=> await _context.Vehicles.FindAsync(vehicleId);

		public async Task AddVehicleAsync(Vehicle vehicle)
		{
			await _context.Vehicles.AddAsync(vehicle);
			await _context.SaveChangesAsync();
		}

		// Entity Framework doesn't need an 'UpdateAsync', but saving is async!
		public async Task UpdateVehicleAsync(Vehicle vehicle)
		{
			_context.Vehicles.Update(vehicle);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteVehicleAsync(int vehicleId)
		{
			var v = await _context.Vehicles.FindAsync(vehicleId);
			if (v != null)
			{
				_context.Vehicles.Remove(v);
				await _context.SaveChangesAsync();
			}
		}
	}
}