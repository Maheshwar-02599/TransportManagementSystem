using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TransportationManagement.Models;
using TransportationManagement.Services;
using TransportationManagement.ViewModels;

namespace TransportationManagement.Controllers
{
	public class DriverController : Controller
	{
		private readonly DriverService _driverService;
		private readonly TripService _tripService;
		private readonly AccountService _accountService;

		public DriverController(DriverService driverService, AccountService accountService, TripService tripService)
		{
			_driverService = driverService;
			_tripService = tripService;
			_accountService = accountService;
		}

		private bool CanView()
		{
			var role = HttpContext.Session.GetString("Role");
			return role == "Admin" || role == "FleetManager";
		}

		private bool CanEdit()
		{
			var role = HttpContext.Session.GetString("Role");
			return role == "FleetManager";
		}

		public async Task<IActionResult> Index()
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			var allTrips = await _tripService.GetAllTripsAsync();
			var activeRoutes = allTrips.Where(t => t.tripStatus != TripStatus.COMPLETED).ToList();

			ViewBag.EngagedStaffIds = activeRoutes.Select(t => t.driverId).ToList();

			var drivers = await _driverService.GetAllDriversAsync();
			return View(drivers);
		}

		[HttpGet]
		public IActionResult AddDriver()
		{
			if (!CanEdit()) return RedirectToAction("Index");
			return View(new CreateDriverViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddDriver(CreateDriverViewModel model)
		{
			if (!CanEdit()) return RedirectToAction("Index");

			if (await _accountService.IsUsernameTaken(model.Username))
			{
				ModelState.AddModelError("Username", "This email is already registered.");
			}

			var allDrivers = await _driverService.GetAllDriversAsync();

			if (allDrivers.Any(d => d.licenseNumber.Trim().ToLower() == model.Driver.licenseNumber.Trim().ToLower()))
			{
				ModelState.AddModelError("Driver.licenseNumber", "This License Number is already registered to another driver.");
			}

			if (allDrivers.Any(d => d.contactNumber.Trim() == model.Driver.contactNumber.Trim()))
			{
				ModelState.AddModelError("Driver.contactNumber", "This Mobile Number is already in use.");
			}

			if (ModelState.IsValid)
			{
				var userAccount = new RegisterViewModel
				{
					Username = model.Username,
					Password = model.Password,
					ConfirmPassword = model.ConfirmPassword,
					Role = "Driver"
				};

				int newUserId = await _accountService.CreateAccount(userAccount);
				model.Driver.UserId = newUserId;

				// Ensure new drivers start as available
				model.Driver.status = DriverStatus.AVAILABLE;

				await _driverService.AddDriverAsync(model.Driver);

				TempData["Success"] = "Driver and login created successfully.";
				return RedirectToAction("Index");
			}

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> GetDriverDetails(int id)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			var driverData = await _driverService.GetDriverDetailsAsync(id);
			if (driverData == null) return NotFound();

			var allTrips = await _tripService.GetAllTripsAsync();
			ViewBag.IsCurrentlyDeployed = allTrips.Any(t => t.driverId == id && t.tripStatus != TripStatus.COMPLETED);

			return View(driverData);
		}

		[HttpGet]
		public IActionResult AssignTrip(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			return RedirectToAction("CreateTrip", "Trip");
		}

		[HttpGet]
		public async Task<IActionResult> GetAssignedTrips(int id)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			var allTrips = await _tripService.GetAllTripsAsync();
			var trips = allTrips.Where(t => t.driverId == id).ToList();

			var driverInfo = await _driverService.GetDriverDetailsAsync(id);
			ViewBag.DriverName = driverInfo?.name ?? "Driver";
			ViewBag.DriverId = id;

			return View(trips);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			var driverData = await _driverService.GetDriverDetailsAsync(id);
			if (driverData == null) return NotFound();
			return View(driverData);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(Driver driverData)
		{
			if (!CanEdit()) return RedirectToAction("Index");

			var allDrivers = await _driverService.GetAllDriversAsync();

			if (allDrivers.Any(d => d.licenseNumber.Trim().ToLower() == driverData.licenseNumber.Trim().ToLower() && d.driverId != driverData.driverId))
			{
				ModelState.AddModelError("licenseNumber", "This License Number is already registered to another driver.");
			}

			if (allDrivers.Any(d => d.contactNumber.Trim() == driverData.contactNumber.Trim() && d.driverId != driverData.driverId))
			{
				ModelState.AddModelError("contactNumber", "This Mobile Number is already in use.");
			}

			if (ModelState.IsValid)
			{
				try
				{
					// Fetch existing driver to preserve the Status since it's removed from the Edit UI
					var existingDriver = await _driverService.GetDriverDetailsAsync(driverData.driverId);
					if (existingDriver != null)
					{
						driverData.status = existingDriver.status;
					}

					await _driverService.UpdateDriverAsync(driverData);
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

		// --- NEW: Enable/Disable Toggle Action ---
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ToggleDriverStatus(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");

			// LOGIC CHECK: Do not allow disabling if they are currently on a trip
			var allTrips = await _tripService.GetAllTripsAsync();
			bool isBusy = allTrips.Any(t => t.driverId == id && t.tripStatus != TripStatus.COMPLETED);

			if (isBusy)
			{
				TempData["Error"] = "Cannot change status. This driver is currently ON_TRIP.";
				return RedirectToAction(nameof(Index));
			}

			var driver = await _driverService.GetDriverDetailsAsync(id);
			if (driver == null) return NotFound();

			// Toggle the Enum status
			if (driver.status == DriverStatus.AVAILABLE)
			{
				driver.status = DriverStatus.INACTIVE;
			}
			else
			{
				driver.status = DriverStatus.AVAILABLE;
			}

			await _driverService.UpdateDriverAsync(driver);
			TempData["Success"] = $"Driver status changed to {driver.status}.";

			return RedirectToAction(nameof(Index));
		}
	}
}