using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var model = new AdminDashboardViewModel
            {
                TotalVehicles = _vehicleService.GetAllVehicles().Count,
                TotalDrivers = _driverService.GetAllDrivers().Count,
                TotalTrips = _tripService.GetAllTrips().Count,
                TotalMaintenance = _maintenanceService.GetAllMaintenanceRecords().Count,
                TotalFuelEntries = _fuelService.GetAllFuelEntries().Count,
                TotalUsers = _accountService.GetTotalUserCount()
            };
            return View(model);
        }

        public IActionResult Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(_accountService.GetAllUsers());
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(RegisterViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                if (_accountService.IsUsernameTaken(model.Username))
                {
                    ModelState.AddModelError("Username", "Email already exists.");
                    return View(model);
                }
                _accountService.CreateAccount(model);
                TempData["Success"] = "User created successfully.";
                return RedirectToAction("Users");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var user = _accountService.GetUserById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User user)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            _accountService.UpdateUser(user);
            TempData["Success"] = "User updated.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            _accountService.RemoveUser(id);
            TempData["Success"] = "User deleted.";
            return RedirectToAction("Users");
        }

        // --- View Redirects ---
        public IActionResult Vehicles() { if (!IsAdmin()) return RedirectToAction("Login", "Account"); return View(_vehicleService.GetAllVehicles()); }
        public IActionResult Drivers() { if (!IsAdmin()) return RedirectToAction("Login", "Account"); return View(_driverService.GetAllDrivers()); }
        public IActionResult Trips() { if (!IsAdmin()) return RedirectToAction("Login", "Account"); return View(_tripService.GetAllTrips()); }
        public IActionResult Maintenance() { if (!IsAdmin()) return RedirectToAction("Login", "Account"); return View(_maintenanceService.GetAllMaintenanceRecords()); }
        public IActionResult Fuel() { if (!IsAdmin()) return RedirectToAction("Login", "Account"); return View(_fuelService.GetAllFuelEntries()); }
    }
}