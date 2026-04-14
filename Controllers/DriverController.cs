using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Models;
using TransportationManagement.Services;
using TransportationManagement.Data;
using TransportationManagement.ViewModels;

namespace TransportationManagement.Controllers
{
	public class DriverController : Controller
	{
        private readonly DriverService _driverService;
		private readonly TripService _tripService;
		private readonly ApplicationDbContext _context;

		public DriverController(DriverService driverService, TripService tripService, ApplicationDbContext context)
		{
			_driverService = driverService;
			_tripService = tripService;
			_context = context;
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
			return View(new CreateDriverViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult AddDriver(CreateDriverViewModel model)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock

			// Validate both nested Driver and credentials
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// Check duplicate user email
			var exists = _context.Users.Any(u => u.Username == model.Username);
			if (exists)
			{
				ModelState.AddModelError("Username", "This email is already registered.");
				return View(model);
			}

			try
			{
                // 1. Create user account for driver (so we get the Id)
				var user = new User
				{
					Username = model.Username,
					Password = Data.PasswordHelper.HashPassword(model.Password),
					Role = "Driver"
				};
				_context.Users.Add(user);
				_context.SaveChanges();

				// 2. Add driver and link to user
				model.Driver.UserId = user.Id;
				_driverService.AddDriver(model.Driver);

				TempData["Success"] = "Driver and login created successfully.";
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, "Error creating driver: " + ex.Message);
				return View(model);
			}
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