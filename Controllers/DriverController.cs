using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	public class DriverController : Controller
	{
		private readonly DriverService _driverService;
		private readonly TripService _tripService;

		public DriverController(DriverService driverService, TripService tripService)
		{
			_driverService = driverService;
			_tripService = tripService;
		}

		// --- NEW RBAC SECURITY LOGIC ---

		// 1. View Access: Both Admin and FleetManager can read data
		private bool CanView()
		{
			var role = HttpContext.Session.GetString("Role");
			return role == "Admin" || role == "FleetManager";
		}

		// 2. Edit Access: ONLY FleetManager can perform CRUD operations
		private bool CanEdit()
		{
			var role = HttpContext.Session.GetString("Role");
			return role == "FleetManager";
		}

		// -------------------------------

		public IActionResult Index()
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			// Fetch active trips to determine busy drivers dynamically
			var activeRoutes = _tripService.GetAllTrips()
									.Where(t => t.tripStatus != TripStatus.COMPLETED)
									.ToList();

			// Pass the list of busy driver IDs to the view
			ViewBag.EngagedStaffIds = activeRoutes.Select(t => t.driverId).ToList();

			return View(_driverService.GetAllDrivers());
		}

		[HttpGet]
		public IActionResult AddDriver()
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult AddDriver(Driver driverData)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			if (ModelState.IsValid)
			{
				_driverService.AddDriver(driverData);
				TempData["Success"] = "Driver added successfully.";
				return RedirectToAction("Index");
			}
			return View(driverData);
		}

		[HttpGet]
		public IActionResult GetDriverDetails(int id)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");
			var driverData = _driverService.GetDriverDetails(id);
			if (driverData == null) return NotFound();

			// Dynamic check for details view
			ViewBag.IsCurrentlyDeployed = _tripService.GetAllTrips()
										  .Any(t => t.driverId == id && t.tripStatus != TripStatus.COMPLETED);

			return View(driverData);
		}

		[HttpGet]
		public IActionResult AssignTrip(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			return RedirectToAction("CreateTrip", "Trip");
		}

		[HttpGet]
		public IActionResult GetAssignedTrips(int id) // 'id' is the driverId passed from the Index
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			// Fetch only trips belonging to this specific driverId
			var trips = _tripService.GetAllTrips()
									.Where(t => t.driverId == id)
									.ToList();

			// Get driver details for the page heading
			var driverInfo = _driverService.GetDriverDetails(id);
			ViewBag.DriverName = driverInfo?.name ?? "Driver";
			ViewBag.DriverId = id;

			return View(trips);
		}

		[HttpGet]
		public IActionResult Edit(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			var driverData = _driverService.GetDriverDetails(id);
			if (driverData == null) return NotFound();
			return View(driverData);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Driver driverData)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock

			if (ModelState.IsValid)
			{
				try
				{
					_driverService.UpdateDriver(driverData);
					TempData["Success"] = "Driver updated successfully.";
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Exception: " + ex.Message);
				}
			}
			return View(driverData);
		}

		[HttpGet]
		public IActionResult Delete(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			var driverData = _driverService.GetDriverDetails(id);
			if (driverData == null) return NotFound();
			return View(driverData);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock

			// 1. Pre-Check: Prevent deletion if actively on a route
			bool isDriverBusy = _tripService.GetAllTrips()
				.Any(t => t.driverId == id && t.tripStatus != TripStatus.COMPLETED);

			if (isDriverBusy)
			{
				TempData["Error"] = "Constraint Failed: Cannot delete this driver because they are currently ON_TRIP.";
				return RedirectToAction("Index");
			}

			// 2. Safely attempt database deletion
			try
			{
				_driverService.DeleteDriver(id);
				TempData["Success"] = "Driver deleted successfully.";
			}
			catch (Exception)
			{
				TempData["Error"] = "Cannot delete this driver because they have associated route history in the database.";
			}

			return RedirectToAction("Index");
		}
	}
}