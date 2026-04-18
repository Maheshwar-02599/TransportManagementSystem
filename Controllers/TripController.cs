using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	public class TripController : Controller
	{
		private readonly TripService _tripService;
		private readonly VehicleService _vehicleService;
		private readonly DriverService _driverService;
		private readonly FuelService _fuelService;

		public TripController(TripService tripService, VehicleService vehicleService,
							  DriverService driverService, FuelService fuelService)
		{
			_tripService = tripService;
			_vehicleService = vehicleService;
			_driverService = driverService;
			_fuelService = fuelService;
		}

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

		// --- FIXED: Filter out Disabled/In-Service Vehicles and Drivers ---
		private async Task LoadDropdowns()
		{
			var allVehicles = await _vehicleService.GetAllVehiclesAsync();
			var allDrivers = await _driverService.GetAllDriversAsync();

			// Only show vehicles that are explicitly ACTIVE (ignores IN_SERVICE and RETIRED/Disabled)
			var availableVehicles = allVehicles.Where(v => v.vehiclestatus == VehicleStatus.ACTIVE).ToList();

			// Only show drivers that are explicitly AVAILABLE (ignores INACTIVE/Disabled)
			var availableDrivers = allDrivers.Where(d => d.status == DriverStatus.AVAILABLE).ToList();

			ViewBag.Vehicles = new SelectList(availableVehicles, "vehicleId", "vehicleNumber");
			ViewBag.Drivers = new SelectList(availableDrivers, "driverId", "name");
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			var allTrips = await _tripService.GetAllTripsAsync();
			var role = HttpContext.Session.GetString("Role");

			if (role == "Driver")
			{
				int? currentUserId = HttpContext.Session.GetInt32("UserId");

				if (currentUserId.HasValue)
				{
					var currentDriver = await _driverService.GetDriverByUserIdAsync(currentUserId.Value);

					if (currentDriver != null)
					{
						var driverTrips = allTrips.Where(t => t.driverId == currentDriver.driverId).ToList();
						return View(driverTrips);
					}
					else
					{
						return View(new List<Trip>());
					}
				}
			}

			return View(allTrips);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> StartTrip(int id)
		{
			var trip = await _tripService.GetTripPlanAsync(id);
			if (trip != null)
			{
				trip.tripStatus = TripStatus.IN_PROGRESS;
				await _tripService.UpdateTripStatusAsync(trip);
				TempData["Success"] = "Trip started! Drive safely.";
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EndTrip(int id, int distanceTraveled)
		{
			var trip = await _tripService.GetTripPlanAsync(id);

			if (trip != null && distanceTraveled > 0)
			{
				trip.tripStatus = TripStatus.COMPLETED;
				await _tripService.UpdateTripStatusAsync(trip);

				decimal mileageKmPerLiter = 10.0m;
				decimal costPerLiter = 100.0m;

				decimal calculatedFuelQty = Math.Round((decimal)distanceTraveled / mileageKmPerLiter, 2);
				decimal calculatedFuelCost = Math.Round(calculatedFuelQty * costPerLiter, 2);

				var vehicleFuelHistory = await _fuelService.GetFuelConsumptionAsync(trip.vehicleId);
				int lastOdometer = vehicleFuelHistory.Any() ? vehicleFuelHistory.Max(f => f.odometerReading) : 0;
				int newOdometerReading = lastOdometer + distanceTraveled;

				var autoFuelEntry = new FuelEntry
				{
					vehicleId = trip.vehicleId,
					fuelQuantity = (decimal)calculatedFuelQty,
					fuelCost = (decimal)calculatedFuelCost,
					odometerReading = newOdometerReading,
					entryDate = DateTime.Now
				};

				await _fuelService.AddFuelEntryAsync(autoFuelEntry);

				TempData["Success"] = $"Trip completed! System auto-logged {calculatedFuelQty}L of fuel for {distanceTraveled}km traveled.";
			}
			else if (distanceTraveled <= 0)
			{
				TempData["Error"] = "Distance traveled must be greater than 0 to complete the trip.";
			}

			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<IActionResult> CreateTrip()
		{
			if (!CanEdit()) return RedirectToAction("Index");
			await LoadDropdowns();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateTrip(Trip trip)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			ModelState.Remove("Vehicle");
			ModelState.Remove("Driver");

			// --- FIXED: Strict validation to prevent forced assignments ---
			var fleetVehicle = await _vehicleService.GetVehicleDetailsAsync(trip.vehicleId);
			if (fleetVehicle != null && fleetVehicle.vehiclestatus != VehicleStatus.ACTIVE)
			{
				ModelState.AddModelError("vehicleId", "Cannot assign trip: This vehicle is either disabled or under maintenance.");
			}

			var assignedDriver = await _driverService.GetDriverDetailsAsync(trip.driverId);
			if (assignedDriver != null && assignedDriver.status != DriverStatus.AVAILABLE)
			{
				ModelState.AddModelError("driverId", "Cannot assign trip: This driver is disabled or currently unavailable.");
			}
			// --------------------------------------------------------------

			if (ModelState.IsValid)
			{
				try
				{
					await _tripService.CreateTripAsync(trip);
					TempData["Success"] = "Trip created successfully.";
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, "System Error: " + ex.Message);
				}
			}
			await LoadDropdowns();
			return View(trip);
		}

		[HttpGet]
		public async Task<IActionResult> GetTripPlan(int id)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");
			var tripData = await _tripService.GetTripPlanAsync(id);
			if (tripData == null) return NotFound();
			return View(tripData);
		}

		[HttpGet]
		public async Task<IActionResult> UpdateTripStatus(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			var tripData = await _tripService.GetTripPlanAsync(id);
			if (tripData == null) return NotFound();
			await LoadDropdowns();
			return View(tripData);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateTripStatus(Trip routeModel)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			ModelState.Remove("Vehicle");
			ModelState.Remove("Driver");

			// --- FIXED: Strict validation for updates as well ---
			var selectedVehicle = await _vehicleService.GetVehicleDetailsAsync(routeModel.vehicleId);
			if (selectedVehicle != null && selectedVehicle.vehiclestatus != VehicleStatus.ACTIVE)
			{
				ModelState.AddModelError("vehicleId", "This vehicle is either disabled or currently under service.");
			}

			var selectedDriver = await _driverService.GetDriverDetailsAsync(routeModel.driverId);
			if (selectedDriver != null && selectedDriver.status != DriverStatus.AVAILABLE)
			{
				ModelState.AddModelError("driverId", "This driver is disabled or currently unavailable.");
			}
			// ----------------------------------------------------

			if (ModelState.IsValid)
			{
				try
				{
					await _tripService.UpdateTripStatusAsync(routeModel);
					TempData["Success"] = "Trip updated successfully.";
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, "System Error: " + ex.Message);
				}
			}
			await LoadDropdowns();
			return View(routeModel);
		}

		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			var tripData = await _tripService.GetTripPlanAsync(id);
			if (tripData == null) return NotFound();
			return View(tripData);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			try
			{
				var tripData = await _tripService.GetTripPlanAsync(id);
				if (tripData != null && tripData.tripStatus == TripStatus.IN_PROGRESS)
				{
					TempData["Error"] = "Cannot delete a trip that is currently IN_PROGRESS.";
					return RedirectToAction("Index");
				}
				await _tripService.DeleteTripAsync(id);
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