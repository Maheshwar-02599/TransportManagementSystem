using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
	public class TripRepository : ITripRepository
	{
		private readonly ApplicationDbContext _context;
		public TripRepository(ApplicationDbContext context) { _context = context; }

		public List<Trip> GetAllTrips() => _context.Trips.Include(t => t.Vehicle).Include(t => t.Driver).ToList();

		public Trip? GetTripById(int tripId) => _context.Trips.Include(t => t.Vehicle).Include(t => t.Driver).FirstOrDefault(t => t.tripId == tripId);

		public List<Trip> GetTripsByDriverId(int driverId) => _context.Trips.Include(t => t.Vehicle).Where(t => t.driverId == driverId).ToList();

		public void AddTrip(Trip tripDetails)
		{
			// NEW CONSTRAINT: Prevent assigning an IN_SERVICE vehicle
			var targetVehicle = _context.Vehicles.AsNoTracking().FirstOrDefault(v => v.vehicleId == tripDetails.vehicleId);
			if (targetVehicle != null && targetVehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
			{
				throw new Exception("Constraint Failed: The selected vehicle is currently IN_SERVICE and cannot be dispatched.");
			}

			var driverOccupied = _context.Trips.Any(t => t.driverId == tripDetails.driverId && t.tripStatus != TripStatus.COMPLETED);
			if (driverOccupied)
			{
				throw new Exception("Driver already has an active trip!");
			}

			var vehicleOccupied = _context.Trips.Any(t => t.vehicleId == tripDetails.vehicleId && t.tripStatus != TripStatus.COMPLETED);
			if (vehicleOccupied)
			{
				throw new Exception("Vehicle is already in use for another active trip!");
			}

			_context.Trips.Add(tripDetails);
			_context.SaveChanges();
		}

		public void UpdateTrip(Trip tripDetails)
		{
			var currentTripRecord = _context.Trips.AsNoTracking().FirstOrDefault(t => t.tripId == tripDetails.tripId);
			if (currentTripRecord == null) throw new Exception("Trip not found");

			// NEW CONSTRAINT: Check IN_SERVICE status on update
			var targetVehicle = _context.Vehicles.AsNoTracking().FirstOrDefault(v => v.vehicleId == tripDetails.vehicleId);
			if (targetVehicle != null && targetVehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
			{
				throw new Exception("Constraint Failed: The selected vehicle is currently IN_SERVICE and cannot be dispatched.");
			}

			// Check ONLY if driver changed
			if (currentTripRecord.driverId != tripDetails.driverId)
			{
				var driverOccupied = _context.Trips.Any(t => t.driverId == tripDetails.driverId && t.tripId != tripDetails.tripId && t.tripStatus != TripStatus.COMPLETED);
				if (driverOccupied) throw new Exception("Driver already has an active trip!");
			}

			// Check ONLY if vehicle changed
			if (currentTripRecord.vehicleId != tripDetails.vehicleId)
			{
				var vehicleOccupied = _context.Trips.Any(t => t.vehicleId == tripDetails.vehicleId && t.tripId != tripDetails.tripId && t.tripStatus != TripStatus.COMPLETED);
				if (vehicleOccupied) throw new Exception("Vehicle is already in use for another active trip!");
			}

			_context.Trips.Update(tripDetails);
			_context.SaveChanges();
		}

		public void DeleteTrip(int tripId)
		{
			var t = _context.Trips.Find(tripId);
			if (t != null) { _context.Trips.Remove(t); _context.SaveChanges(); }
		}
	}
}