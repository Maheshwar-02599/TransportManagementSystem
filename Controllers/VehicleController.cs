using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Models;
using TransportationManagement.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TransportationManagement.Controllers
{
	public class VehicleController : Controller
	{
		private readonly VehicleService _vehicleSvc;
		private readonly TripService _routeSvc;
		private readonly MaintenanceService _maintenanceSvc;

		public VehicleController(VehicleService vehicleService, TripService tripService, MaintenanceService maintenanceService)
		{
			_vehicleSvc = vehicleService;
			_routeSvc = tripService;
			_maintenanceSvc = maintenanceService;
		}

		private bool CanView()
		{
			var userRole = HttpContext.Session.GetString("Role");
			return userRole == "Admin" || userRole == "FleetManager";
		}

		private bool CanEdit()
		{
			var userRole = HttpContext.Session.GetString("Role");
			return userRole == "FleetManager";
		}

		public async Task<IActionResult> Index()
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			var allTrips = await _routeSvc.GetAllTripsAsync();
			var unfinishedRoutes = allTrips
									.Where(t => t.tripStatus != TripStatus.COMPLETED)
									.ToList();

			ViewBag.BusyVehicleIds = unfinishedRoutes.Select(t => t.vehicleId).ToList();

			var vehicles = await _vehicleSvc.GetAllVehiclesAsync();
			return View(vehicles);
		}

		[HttpGet]
		public IActionResult AddVehicle()
		{
			if (!CanEdit()) return RedirectToAction("Index");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddVehicle(Vehicle vehicleData)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			var allVehicles = await _vehicleSvc.GetAllVehiclesAsync();
			if (allVehicles.Any(v => v.vehicleNumber.Trim().ToLower() == vehicleData.vehicleNumber.Trim().ToLower()))
			{
				ModelState.AddModelError("vehicleNumber", "The vehicle number already exists.");
			}
			if (ModelState.IsValid)
			{
				// Ensure new vehicles start as in service
				vehicleData.vehiclestatus = VehicleStatus.ACTIVE;

				await _vehicleSvc.AddVehicleAsync(vehicleData);
				TempData["Success"] = "Vehicle added successfully.";
				return RedirectToAction("Index");
			}
			return View(vehicleData);
		}

		[HttpGet]
		public async Task<IActionResult> GetVehicleDetails(int id)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			var vehicleData = await _vehicleSvc.GetVehicleDetailsAsync(id);
			if (vehicleData == null) return NotFound();

			var allTrips = await _routeSvc.GetAllTripsAsync();
			ViewBag.IsOnActiveTrip = allTrips.Any(t => t.vehicleId == id && t.tripStatus != TripStatus.COMPLETED);

			return View(vehicleData);
		}

		[HttpGet]
		public async Task<IActionResult> UpdateVehicle(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");

			var vehicleData = await _vehicleSvc.GetVehicleDetailsAsync(id);
			if (vehicleData == null) return NotFound();

			return View(vehicleData);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateVehicle(Vehicle vehicleData)
		{
			if (!CanEdit()) return RedirectToAction("Index");

			var allVehicles = await _vehicleSvc.GetAllVehiclesAsync();
			if (allVehicles.Any(v => v.vehicleNumber.Trim().ToLower() == vehicleData.vehicleNumber.Trim().ToLower() && v.vehicleId != vehicleData.vehicleId))
			{
				ModelState.AddModelError("vehicleNumber", "The vehicle number already exists.");
			}
			if (ModelState.IsValid)
			{
				try
				{
					// Fetch existing vehicle to preserve the Status since it's removed from the Edit UI
					var existingVehicle = await _vehicleSvc.GetVehicleDetailsAsync(vehicleData.vehicleId);
					if (existingVehicle != null)
					{
						vehicleData.vehiclestatus = existingVehicle.vehiclestatus;
					}

					await _vehicleSvc.UpdateVehicleAsync(vehicleData);
					TempData["Success"] = "Vehicle updated successfully.";
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Exception: " + ex.Message);
				}
			}
			return View(vehicleData);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ToggleVehicleStatus(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");

			// LOGIC CHECK #1: Do not allow changing status if it is currently on a trip
			var allTrips = await _routeSvc.GetAllTripsAsync();
			bool isOnRoute = allTrips.Any(t => t.vehicleId == id && t.tripStatus != TripStatus.COMPLETED);

			if (isOnRoute)
			{
				TempData["Error"] = "Cannot change status. This vehicle is currently ON_TRIP.";
				return RedirectToAction(nameof(Index));
			}

			var vehicle = await _vehicleSvc.GetVehicleDetailsAsync(id);
			if (vehicle == null) return NotFound();

			// LOGIC CHECK #2: Prevent enabling if it is undergoing maintenance.
			// We can just check the vehicle's own status since MaintenanceController updates it!
			if (vehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
			{
				TempData["Error"] = "Cannot enable this vehicle. It is currently undergoing maintenance.";
				return RedirectToAction(nameof(Index));
			}

			// Toggle the Enum status between ACTIVE and RETIRED (Disabled)
			if (vehicle.vehiclestatus == VehicleStatus.ACTIVE)
			{
				vehicle.vehiclestatus = VehicleStatus.RETIRED;
			}
			else
			{
				// If it reaches here, we know it is RETIRED, so we can safely enable it.
				vehicle.vehiclestatus = VehicleStatus.ACTIVE;
			}

			await _vehicleSvc.UpdateVehicleAsync(vehicle);
			TempData["Success"] = $"Vehicle status changed to {vehicle.vehiclestatus}.";

			return RedirectToAction(nameof(Index));
		}
	}
}