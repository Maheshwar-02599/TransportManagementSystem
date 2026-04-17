using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
	public class TripRepository : ITripRepository
	{
		private readonly ApplicationDbContext _context;
		public TripRepository(ApplicationDbContext context) { _context = context; }

		public async Task<List<Trip>> GetAllTripsAsync()
			=> await _context.Trips.Include(t => t.Vehicle).Include(t => t.Driver).ToListAsync();

		public async Task<Trip?> GetTripByIdAsync(int tripId)
			=> await _context.Trips.Include(t => t.Vehicle).Include(t => t.Driver).FirstOrDefaultAsync(t => t.tripId == tripId);

		public async Task<List<Trip>> GetTripsByDriverIdAsync(int driverId)
			=> await _context.Trips.Include(t => t.Vehicle).Include(t => t.Driver).Where(t => t.driverId == driverId).ToListAsync();

		public async Task AddTripAsync(Trip tripDetails)
		{
			var targetVehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.vehicleId == tripDetails.vehicleId);
			if (targetVehicle != null && targetVehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
			{
				throw new Exception("Constraint Failed: The selected vehicle is currently IN_SERVICE and cannot be dispatched.");
			}

			var driverOccupied = await _context.Trips.AnyAsync(t => t.driverId == tripDetails.driverId && t.tripStatus != TripStatus.COMPLETED);
			if (driverOccupied) throw new Exception("Driver already has an active trip!");

			var vehicleOccupied = await _context.Trips.AnyAsync(t => t.vehicleId == tripDetails.vehicleId && t.tripStatus != TripStatus.COMPLETED);
			if (vehicleOccupied) throw new Exception("Vehicle is already in use for another active trip!");

			await _context.Trips.AddAsync(tripDetails);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateTripAsync(Trip tripDetails)
		{
			var currentTripRecord = await _context.Trips.AsNoTracking().FirstOrDefaultAsync(t => t.tripId == tripDetails.tripId);
			if (currentTripRecord == null) throw new Exception("Trip not found");

			var targetVehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.vehicleId == tripDetails.vehicleId);
			if (targetVehicle != null && targetVehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
			{
				throw new Exception("Constraint Failed: The selected vehicle is currently IN_SERVICE and cannot be dispatched.");
			}

			if (currentTripRecord.driverId != tripDetails.driverId)
			{
				var driverOccupied = await _context.Trips.AnyAsync(t => t.driverId == tripDetails.driverId && t.tripId != tripDetails.tripId && t.tripStatus != TripStatus.COMPLETED);
				if (driverOccupied) throw new Exception("Driver already has an active trip!");
			}

			if (currentTripRecord.vehicleId != tripDetails.vehicleId)
			{
				var vehicleOccupied = await _context.Trips.AnyAsync(t => t.vehicleId == tripDetails.vehicleId && t.tripId != tripDetails.tripId && t.tripStatus != TripStatus.COMPLETED);
				if (vehicleOccupied) throw new Exception("Vehicle is already in use for another active trip!");
			}

			_context.Trips.Update(tripDetails);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteTripAsync(int tripId)
		{
			var t = await _context.Trips.FindAsync(tripId);
			if (t != null) { _context.Trips.Remove(t); await _context.SaveChangesAsync(); }
		}
	}
}