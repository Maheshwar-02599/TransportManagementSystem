using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TransportationManagement.Data;
using TransportationManagement.Models;
using TransportationManagement.Services;
using TransportationManagement.ViewModels;

namespace TransportationManagement.Controllers
{
	public class AdminController : Controller
	{
		private readonly AccountService _accountService;
		private readonly VehicleService _vehicleService;
		private readonly DriverService _driverService;
		private readonly TripService _tripService;
		private readonly MaintenanceService _maintenanceService;
		private readonly FuelService _fuelService;

		public AdminController(AccountService accountService, VehicleService vehicleService,
			DriverService driverService, TripService tripService,
			MaintenanceService maintenanceService, FuelService fuelService)
		{
			_accountService = accountService;
			_vehicleService = vehicleService;
			_driverService = driverService;
			_tripService = tripService;
			_maintenanceService = maintenanceService;
			_fuelService = fuelService;
		}

		private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
		public async Task<IActionResult> Dashboard()
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");

			// 1. Fetch data asynchronously
			var vehicles = await _vehicleService.GetAllVehiclesAsync();
			var drivers = await _driverService.GetAllDriversAsync();
			var trips = await _tripService.GetAllTripsAsync();
			var maintenance = await _maintenanceService.GetAllMaintenanceRecordsAsync();
			var fuel = await _fuelService.GetAllFuelEntriesAsync();

			// 2. Map to ViewModel
			var model = new AdminDashboardViewModel
			{
				TotalVehicles = vehicles.Count,
				TotalDrivers = drivers.Count,
				TotalTrips = trips.Count,
				TotalMaintenance = maintenance.Count,
				TotalFuelEntries = fuel.Count,
				TotalUsers = await _accountService.GetTotalUserCount()
			};

			return View(model);
		}

		public async Task<IActionResult> Users()
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");
			return View(await _accountService.GetAllUsers());
		}

		[HttpGet]
		public IActionResult CreateUser()
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");
			return View(new RegisterViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateUser(RegisterViewModel model)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");
			if (ModelState.IsValid)
			{
				if (await _accountService.IsUsernameTaken(model.Username))
				{
					ModelState.AddModelError("Username", "Email already exists.");
					return View(model);
				}

				await _accountService.CreateAccount(model);

				TempData["Success"] = "User created successfully.";
				return RedirectToAction("Users");
			}
			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> EditUser(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");
			var user = await _accountService.GetUserById(id);
			if (user == null) return NotFound();
			return View(user);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditUser(User user)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");
			await _accountService.UpdateUser(user);

			TempData["Success"] = "User updated.";
			return RedirectToAction("Users");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteUser(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");

			_accountService.RemoveUser(id);
			TempData["Success"] = "User deleted.";
			return RedirectToAction("Users");
		}

		// Updated Quick Action list methods
		public async Task<IActionResult> Vehicles() { if (!IsAdmin()) return RedirectToAction("Login", "Account"); return View(await _vehicleService.GetAllVehiclesAsync()); }
		public async Task<IActionResult> Drivers() { if (!IsAdmin()) return RedirectToAction("Login", "Account"); return View(await _driverService.GetAllDriversAsync()); }
		public async Task<IActionResult> Trips() { if (!IsAdmin()) return RedirectToAction("Login", "Account"); return View(await _tripService.GetAllTripsAsync()); }
		public async Task<IActionResult> Maintenance() { if (!IsAdmin()) return RedirectToAction("Login", "Account"); return View(await _maintenanceService.GetAllMaintenanceRecordsAsync()); }
		public async Task<IActionResult> Fuel() { if (!IsAdmin()) return RedirectToAction("Login", "Account"); return View(await _fuelService.GetAllFuelEntriesAsync()); }
	}
}