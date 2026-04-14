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

			// Repository already includes Driver/Vehicle data
			var allTrips = _tripService.GetAllTrips();
			var role = HttpContext.Session.GetString("Role");

            // Prefer reliable lookup by UserId when available
			var sessionUserId = HttpContext.Session.GetInt32("UserId");
			var currentUserEmail = HttpContext.Session.GetString("Username")?.ToLower().Trim();

			if (role == "Driver")
			{
                if (sessionUserId.HasValue)
				{
					// Find driver linked to this user
					var linkedDriver = _driverService.GetDriverByUserId(sessionUserId.Value);
					if (linkedDriver != null)
					{
						var driverTrips = allTrips.Where(t => t.driverId == linkedDriver.driverId).ToList();
						return View(driverTrips);
					}
				}

				// Fallback to email-based matching if UserId link not present
				if (!string.IsNullOrEmpty(currentUserEmail))
				{
					// 1. Get the prefix (the part before the '@')
					var emailPrefix = currentUserEmail.Split('@')[0].ToLower().Trim();

					// 2. More robust matching: normalize names and compare tokens so
					//    drivers created with slightly different name formats still match.
					string NormalizeLetters(string s) => string.IsNullOrWhiteSpace(s)
						? string.Empty
						: new string(s.ToLower().Where(char.IsLetter).ToArray());

					var emailAlpha = NormalizeLetters(emailPrefix); // remove digits so name match can succeed

					var driverTrips = allTrips.Where(t => t.Driver != null && (
									  // match when normalized driver name contains the email alpha prefix
									  NormalizeLetters(t.Driver.name).Contains(emailAlpha) ||
									  // or when any name token equals the email alpha prefix
									  t.Driver.name.ToLower().Split(' ').Any(p => NormalizeLetters(p) == emailAlpha) ||
									  // or when email prefix contains the driver's normalized name (handles short usernames)
									  emailAlpha.Contains(NormalizeLetters(t.Driver.name))
								  ))
								  .ToList();

					// Fallback: if no trips matched by name, attempt to match by contact number or exact name contains
					if (!driverTrips.Any())
					{
						driverTrips = allTrips.Where(t => t.Driver != null && (
											(!string.IsNullOrWhiteSpace(t.Driver.contactNumber) && t.Driver.contactNumber.ToLower().Contains(emailPrefix)) ||
											(!string.IsNullOrWhiteSpace(t.Driver.name) && t.Driver.name.ToLower().Contains(emailPrefix))
										))
										.ToList();
					}

					return View(driverTrips);
				}

				// If nothing matched, fall through to empty list
				return View(new List<Trip>());
			}

			// Admins and Fleet Managers see all trips
			return View(allTrips);
		}

	
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult StartTrip(int id) // 'id' matches asp-route-id
		{
			var trip = _tripService.GetTripPlan(id);
			if (trip != null)
			{
				trip.tripStatus = TripStatus.IN_PROGRESS; // Matches model lowercase 't'
				_tripService.UpdateTripStatus(trip);
				TempData["Success"] = "Trip started! Drive safely.";
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult EndTrip(int id) // 'id' matches asp-route-id
		{
			var trip = _tripService.GetTripPlan(id);
			if (trip != null)
			{
				trip.tripStatus = TripStatus.COMPLETED;
				_tripService.UpdateTripStatus(trip);
				TempData["Success"] = "Trip completed! Driver and Vehicle are now AVAILABLE.";
			}
			return RedirectToAction("Index");
		}

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