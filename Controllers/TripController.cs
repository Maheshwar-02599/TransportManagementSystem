using System.Linq.Expressions;
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

		public TripController(TripService tripService, VehicleService vehicleService, DriverService driverService)
		{
			_tripService = tripService;
			_vehicleService = vehicleService;
			_driverService = driverService;
		}

		// --- NEW RBAC SECURITY LOGIC ---

		// 1. View Access: Admin, FleetManager, and Driver can view trips
		private bool CanView()
		{
			var r = HttpContext.Session.GetString("Role");
			return r == "Admin" || r == "FleetManager" || r == "Driver";
		}

		// 2. Edit Access: ONLY FleetManager can perform CRUD operations
		private bool CanEdit()
		{
			var r = HttpContext.Session.GetString("Role");
			return r == "FleetManager";
		}

		// -------------------------------

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
			return View(_tripService.GetAllTrips());
		}

		[HttpGet]
		public IActionResult CreateTrip()
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			LoadDropdowns();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CreateTrip(Trip trip)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock

			ModelState.Remove("Vehicle");
			ModelState.Remove("Driver");

			// --- IRONCLAD MAINTENANCE CONSTRAINT ---
			var fleetVehicle = _vehicleService.GetVehicleDetails(trip.vehicleId);
			if (fleetVehicle != null && fleetVehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
			{
				ModelState.AddModelError("vehicleId", "DENIED: This vehicle is under maintenance.");
			}
			// ---------------------------------------

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
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			var tripData = _tripService.GetTripPlan(id);
			if (tripData == null) return NotFound();
			LoadDropdowns();
			return View(tripData);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateTripStatus(Trip routeModel)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock

			ModelState.Remove("Vehicle");
			ModelState.Remove("Driver");

			// --- NEW MAINTENANCE CONSTRAINT ---
			var selectedVehicle = _vehicleService.GetVehicleDetails(routeModel.vehicleId);
			if (selectedVehicle != null && selectedVehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
			{
				ModelState.AddModelError("vehicleId", "This vehicle is currently under service and cannot be assigned to a trip.");
			}
			// ----------------------------------

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
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			var tripData = _tripService.GetTripPlan(id);
			if (tripData == null) return NotFound();
			return View(tripData);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock

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