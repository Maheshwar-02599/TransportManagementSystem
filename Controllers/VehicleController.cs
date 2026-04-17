using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Models;
using TransportationManagement.Services;
using System;
using System.Linq;

namespace TransportationManagement.Controllers
{
	public class VehicleController : Controller
	{
		private readonly VehicleService _vehicleSvc;
		private readonly TripService _routeSvc;

        public VehicleController(VehicleService vehicleService, TripService tripService)
        {
            _vehicleSvc = vehicleService;
            _routeSvc = tripService;
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

		// 1. ADDED ASYNC
		public async Task<IActionResult> Index()
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			// 2. ADDED AWAIT
			var allTrips = await _routeSvc.GetAllTripsAsync();
			var unfinishedRoutes = allTrips
									.Where(t => t.tripStatus != TripStatus.COMPLETED)
									.ToList();

			ViewBag.BusyVehicleIds = unfinishedRoutes.Select(t => t.vehicleId).ToList();

			// 3. ADDED AWAIT
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

			if (ModelState.IsValid)
			{
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

			if (ModelState.IsValid)
			{
				try
				{
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

		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");

			var vehicleData = await _vehicleSvc.GetVehicleDetailsAsync(id);
			if (vehicleData == null) return NotFound();

			return View(vehicleData);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");

			var allTrips = await _routeSvc.GetAllTripsAsync();
			bool isActivelyDeployed = allTrips.Any(t => t.vehicleId == id && t.tripStatus != TripStatus.COMPLETED);

			if (isActivelyDeployed)
			{
				TempData["Error"] = "Constraint Failed: Cannot delete this vehicle because it is currently ON_TRIP.";
				return RedirectToAction("Index");
			}

			try
			{
				await _vehicleSvc.DeleteVehicleAsync(id);
				TempData["Success"] = "Vehicle deleted successfully.";
			}
			catch (Exception)
			{
				TempData["Error"] = "Cannot delete this vehicle because it has associated historical records in the database.";
			}
			return RedirectToAction("Index");
		}
        
    }
}