using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	public class TripController : Controller
	{
		private readonly TripService _tripService;
		private readonly VehicleService _vehicleService;
		private readonly DriverService _driverService;

		// NEW: Fuel Service added for auto-logging
		private readonly FuelService _fuelService;

		public TripController(TripService tripService, VehicleService vehicleService, DriverService driverService, FuelService fuelService)
		{
			_tripService = tripService;
			_vehicleService = vehicleService;
			_driverService = driverService;
			_fuelService = fuelService;
		}

		// --- RBAC SECURITY LOGIC ---
		private bool CanView()
		{
			var r = HttpContext.Session.GetString("Role");
			return r == "Admin" || r == "FleetManager" || r == "Driver";
		}

		private bool CanEdit()
		{
			var r = HttpContext.Session.GetString("Role");
			return r == "FleetManager";
		}
		// ---------------------------

		private void LoadDropdowns()
		{
			var vehicles = _vehicleService.GetAllVehicles();
			var drivers = _driverService.GetAllDrivers();
			ViewBag.Vehicles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(vehicles, "vehicleId", "vehicleNumber");
			ViewBag.Drivers = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(drivers, "driverId", "name");
		}

		public IActionResult Index()
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			var allTrips = _tripService.GetAllTrips();
			var role = HttpContext.Session.GetString("Role");
			var currentUserEmail = HttpContext.Session.GetString("Username")?.ToLower().Trim();

			if (role == "Driver" && !string.IsNullOrEmpty(currentUserEmail))
			{
				// Filter trips to only show the ones assigned to this specific driver
				var emailPrefix = currentUserEmail.Split('@')[0];
				var driverTrips = allTrips.Where(t => t.Driver != null &&
								  t.Driver.name.ToLower().Trim().Contains(emailPrefix))
								  .ToList();

				return View(driverTrips);
			}

			// Admins and Fleet Managers see all trips
			return View(allTrips);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult StartTrip(int id)
		{
			var trip = _tripService.GetTripPlan(id);
			if (trip != null)
			{
				trip.tripStatus = TripStatus.IN_PROGRESS;
				_tripService.UpdateTripStatus(trip);
				TempData["Success"] = "Trip started! Drive safely.";
			}
			return RedirectToAction("Index");
		}

		// =================================================================
		// NEW: AUTO-FUEL CALCULATION LOGIC
		// =================================================================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult EndTrip(int id, int distanceTraveled)
		{
			var trip = _tripService.GetTripPlan(id);

			// Validate that the driver actually entered a distance
			if (trip != null && distanceTraveled > 0)
			{
				// 1. Mark trip as completed
				trip.tripStatus = TripStatus.COMPLETED;
				_tripService.UpdateTripStatus(trip);

				// 2. Dynamic Fuel Calculation (10 km/L mileage and Rs. 100 per Liter)
				decimal mileageKmPerLiter = 10.0m;
				decimal costPerLiter = 100.0m;

				decimal calculatedFuelQty = Math.Round((decimal)distanceTraveled / mileageKmPerLiter, 2);
				decimal calculatedFuelCost = Math.Round(calculatedFuelQty * costPerLiter, 2);

				// 3. Determine new Odometer reading based on past entries
				var vehicleFuelHistory = _fuelService.GetFuelConsumption(trip.vehicleId);
				int lastOdometer = vehicleFuelHistory.Any() ? vehicleFuelHistory.Max(f => f.odometerReading) : 0;
				int newOdometerReading = lastOdometer + distanceTraveled;

				// 4. Auto-generate the Fuel Entry
				var autoFuelEntry = new FuelEntry
				{
					vehicleId = trip.vehicleId,
					fuelQuantity = (decimal)calculatedFuelQty,
					fuelCost = (decimal)calculatedFuelCost,
					odometerReading = newOdometerReading,
					entryDate = DateTime.Now
				};

				_fuelService.AddFuelEntry(autoFuelEntry);

				TempData["Success"] = $"Trip completed! System auto-logged {calculatedFuelQty}L of fuel for {distanceTraveled}km traveled.";
			}
			else if (distanceTraveled <= 0)
			{
				TempData["Error"] = "Distance traveled must be greater than 0 to complete the trip.";
			}

			return RedirectToAction("Index");
		}
		// =================================================================

		[HttpGet]
		public IActionResult CreateTrip()
		{
			if (!CanEdit()) return RedirectToAction("Index");
			LoadDropdowns();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CreateTrip(Trip trip)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			ModelState.Remove("Vehicle");
			ModelState.Remove("Driver");

			var fleetVehicle = _vehicleService.GetVehicleDetails(trip.vehicleId);
			if (fleetVehicle != null && fleetVehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
			{
				ModelState.AddModelError("vehicleId", "DENIED: This vehicle is under maintenance.");
			}

			if (ModelState.IsValid)
			{
				try
				{
					_tripService.CreateTrip(trip);
					TempData["Success"] = "Trip created successfully.";
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, "System Error: " + ex.Message);
				}
			}
			LoadDropdowns();
			return View(trip);
		}

		[HttpGet]
		public IActionResult GetTripPlan(int id)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");
			var tripData = _tripService.GetTripPlan(id);
			if (tripData == null) return NotFound();
			return View(tripData);
		}

		[HttpGet]
		public IActionResult UpdateTripStatus(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			var tripData = _tripService.GetTripPlan(id);
			if (tripData == null) return NotFound();
			LoadDropdowns();
			return View(tripData);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateTripStatus(Trip routeModel)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			ModelState.Remove("Vehicle");
			ModelState.Remove("Driver");

			var selectedVehicle = _vehicleService.GetVehicleDetails(routeModel.vehicleId);
			if (selectedVehicle != null && selectedVehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
			{
				ModelState.AddModelError("vehicleId", "This vehicle is currently under service.");
			}

			if (ModelState.IsValid)
			{
				try
				{
					_tripService.UpdateTripStatus(routeModel);
					TempData["Success"] = "Trip updated successfully.";
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, "System Error: " + ex.Message);
				}
			}
			LoadDropdowns();
			return View(routeModel);
		}

		[HttpGet]
		public IActionResult Delete(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			var tripData = _tripService.GetTripPlan(id);
			if (tripData == null) return NotFound();
			return View(tripData);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			try
			{
				var tripData = _tripService.GetTripPlan(id);
				if (tripData != null && tripData.tripStatus == TripStatus.IN_PROGRESS)
				{
					TempData["Error"] = "Cannot delete a trip that is currently IN_PROGRESS.";
					return RedirectToAction("Index");
				}
				_tripService.DeleteTrip(id);
				TempData["Success"] = "Trip deleted.";
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Error deleting trip: " + ex.Message;
				return RedirectToAction("Index");
			}
		}
	}
}