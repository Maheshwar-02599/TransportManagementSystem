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
				// 1. Create the User Account FIRST
				var userAccount = new RegisterViewModel
				{
					Username = model.Username,
					Password = model.Password,
					ConfirmPassword = model.ConfirmPassword,
					Role = "Driver"
				};

				// 2. Grab the new generated UserId
				int newUserId = await _accountService.CreateAccount(userAccount);

				// 3. Link the Driver to the new User Account
				model.Driver.UserId = newUserId;

				// 4. NOW save the Driver to the database
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

		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			var driverData = await _driverService.GetDriverDetailsAsync(id);
			if (driverData == null) return NotFound();
			return View(driverData);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");

			var allTrips = await _tripService.GetAllTripsAsync();
			bool isDriverBusy = allTrips.Any(t => t.driverId == id && t.tripStatus != TripStatus.COMPLETED);

			if (isDriverBusy)
			{
				TempData["Error"] = $"Constraint Failed: Cannot delete this driver because he is currently ON_TRIP.";
				return RedirectToAction("Index");
			}

			try
			{
				// 1. Fetch the driver to safely grab the UserId before we delete them
				var driverToDelete = await _driverService.GetDriverDetailsAsync(id);

				if (driverToDelete != null)
				{
					int? userIdToRemove = driverToDelete.UserId;

					// 2. Delete the Driver record
					await _driverService.DeleteDriverAsync(id);

					// 3. Delete the Admin User Account using your new AccountService!
					if (userIdToRemove.HasValue)
					{
						_accountService.RemoveUser(userIdToRemove.Value);
					}

					TempData["Success"] = "Driver profile and login account deleted successfully.";
				}
			}
			catch (Exception)
			{
				TempData["Error"] = "Cannot delete this driver because they have associated route history in the database.";
			}

			return RedirectToAction("Index");
		}
	}
}