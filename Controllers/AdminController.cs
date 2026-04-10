using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Data;
using TransportationManagement.Models;
using TransportationManagement.Services;
using TransportationManagement.ViewModels;

namespace TransportationManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly VehicleService _vehicleService;
        private readonly DriverService _driverService;
        private readonly TripService _tripService;
        private readonly MaintenanceService _maintenanceService;
        private readonly FuelService _fuelService;

        public AdminController(ApplicationDbContext context, VehicleService vehicleService,
            DriverService driverService, TripService tripService,
            MaintenanceService maintenanceService, FuelService fuelService)
        {
            _context = context;
            _vehicleService = vehicleService;
            _driverService = driverService;
            _tripService = tripService;
            _maintenanceService = maintenanceService;
            _fuelService = fuelService;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private bool IsLoggedIn() => HttpContext.Session.GetString("Username") != null;

        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var model = new AdminDashboardViewModel
            {
                TotalVehicles    = _vehicleService.GetAllVehicles().Count,
                TotalDrivers     = _driverService.GetAllDrivers().Count,
                TotalTrips       = _tripService.GetAllTrips().Count,
                TotalMaintenance = _maintenanceService.GetAllMaintenanceRecords().Count,
                TotalFuelEntries = _fuelService.GetAllFuelEntries().Count,
                TotalUsers       = _context.Users.Count()
            };
            return View(model);
        }

        public IActionResult Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(_context.Users.ToList());
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
                if (_context.Users.Any(u => u.Username == model.Username))
                { ModelState.AddModelError("Username", "Email already exists."); return View(model); }
                _context.Users.Add(new User { Username = model.Username, Password = PasswordHelper.HashPassword(model.Password), Role = model.Role });
                _context.SaveChanges();
                TempData["Success"] = "User created successfully.";
                return RedirectToAction("Users");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User user)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var existing = _context.Users.Find(user.Id);
            if (existing == null) return NotFound();
            existing.Username = user.Username;
            existing.Role = user.Role;
            _context.SaveChanges();
            TempData["Success"] = "User updated.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var user = _context.Users.Find(id);
            if (user != null) { _context.Users.Remove(user); _context.SaveChanges(); TempData["Success"] = "User deleted."; }
            return RedirectToAction("Users");
        }

        public IActionResult Vehicles()    { if (!IsAdmin()) return RedirectToAction("Login","Account"); return View(_vehicleService.GetAllVehicles()); }
        public IActionResult Drivers()     { if (!IsAdmin()) return RedirectToAction("Login","Account"); return View(_driverService.GetAllDrivers()); }
        public IActionResult Trips()       { if (!IsAdmin()) return RedirectToAction("Login","Account"); return View(_tripService.GetAllTrips()); }
        public IActionResult Maintenance() { if (!IsAdmin()) return RedirectToAction("Login","Account"); return View(_maintenanceService.GetAllMaintenanceRecords()); }
        public IActionResult Fuel()        { if (!IsAdmin()) return RedirectToAction("Login","Account"); return View(_fuelService.GetAllFuelEntries()); }
    }
}
