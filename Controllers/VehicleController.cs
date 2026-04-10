using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	public class VehicleController : Controller
	{
		private readonly VehicleService _vehicleSvc;
		private readonly TripService _routeSvc; // Injected to check active routes

		public VehicleController(VehicleService vehicleService, TripService tripService)
		{
			_vehicleSvc = vehicleService;
			_routeSvc = tripService;
		}

		// --- NEW RBAC SECURITY LOGIC ---

		// 1. View Access: Both Admin and FleetManager can read data
		private bool CanView()
		{
			var userRole = HttpContext.Session.GetString("Role");
			return userRole == "Admin" || userRole == "FleetManager";
		}

		// 2. Edit Access: ONLY FleetManager can perform CRUD operations
		private bool CanEdit()
		{
			var userRole = HttpContext.Session.GetString("Role");
			return userRole == "FleetManager";
		}

		// -------------------------------

		public IActionResult Index()
		{
			if (!CanView()) return RedirectToAction("Login", "Account"); // Admin can view

			// Fetch active trips to determine busy vehicles dynamically
			var unfinishedRoutes = _routeSvc.GetAllTrips()
									.Where(t => t.tripStatus != TripStatus.COMPLETED)
									.ToList();

			ViewBag.BusyVehicleIds = unfinishedRoutes.Select(t => t.vehicleId).ToList();

			return View(_vehicleSvc.GetAllVehicles());
		}

		[HttpGet]
		public IActionResult AddVehicle()
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Kicks Admin back to the list
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult AddVehicle(Vehicle vehicleData)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock

			if (ModelState.IsValid)
			{
				_vehicleSvc.AddVehicle(vehicleData);
				TempData["Success"] = "Vehicle added successfully.";
				return RedirectToAction("Index");
			}
			return View(vehicleData);
		}

		[HttpGet]
		public IActionResult GetVehicleDetails(int id)
		{
			if (!CanView()) return RedirectToAction("Login", "Account"); // Admin can view

			var vehicleData = _vehicleSvc.GetVehicleDetails(id);
			if (vehicleData == null) return NotFound();

			// Dynamic check for details view
			ViewBag.IsOnActiveTrip = _routeSvc.GetAllTrips()
									  .Any(t => t.vehicleId == id && t.tripStatus != TripStatus.COMPLETED);

			return View(vehicleData);
		}

		[HttpGet]
		public IActionResult UpdateVehicle(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Kicks Admin out
			var vehicleData = _vehicleSvc.GetVehicleDetails(id);
			if (vehicleData == null) return NotFound();
			return View(vehicleData);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateVehicle(Vehicle vehicleData)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock

			if (ModelState.IsValid)
			{
				try
				{
					_vehicleSvc.UpdateVehicle(vehicleData);
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
		public IActionResult Delete(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Kicks Admin out
			var vehicleData = _vehicleSvc.GetVehicleDetails(id);
			if (vehicleData == null) return NotFound();
			return View(vehicleData);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock

			// ... (Existing pre-deletion checks - ON_TRIP, Historical Records) ...

			// 1. PRE-CHECK: Is it actually on a trip right now?
			bool isActivelyDeployed = _routeSvc.GetAllTrips()
				.Any(t => t.vehicleId == id && t.tripStatus != TripStatus.COMPLETED);

			if (isActivelyDeployed)
			{
				// Block deletion and tell them it's on a trip
				TempData["Error"] = "Constraint Failed: Cannot delete this vehicle because it is currently ON_TRIP.";
				return RedirectToAction("Index");
			}

			// 2. Database Deletion Attempt
			try
			{
				_vehicleSvc.DeleteVehicle(id);
				TempData["Success"] = "Vehicle deleted successfully.";
			}
			catch (Exception)
			{
				// If we get here, it wasn't on a trip, but the database still blocked it (e.g., past fuel records)
				TempData["Error"] = "Cannot delete this vehicle because it has associated historical records in the database (Trips, Fuel, or Maintenance).";
			}

			return RedirectToAction("Index");
		}
	}
}