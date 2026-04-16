using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Models;
using TransportationManagement.Services;
using TransportationManagement.ViewModels;
using System;

namespace TransportationManagement.Controllers
{
    public class DriverController : Controller
    {
        private readonly DriverService _driverService;
        private readonly AccountService _accountService;

        public DriverController(DriverService driverService, AccountService accountService)
        {
            _driverService = driverService;
            _accountService = accountService;
        }

        private bool CanView() => HttpContext.Session.GetString("Role") == "Admin" || HttpContext.Session.GetString("Role") == "FleetManager";
        private bool CanEdit() => HttpContext.Session.GetString("Role") == "FleetManager";

        public IActionResult Index()
        {
            if (!CanView()) return RedirectToAction("Login", "Account");
            return View(_driverService.GetAllDrivers());
        }

        [HttpGet]
        public IActionResult AddDriver()
        {
            if (!CanEdit()) return RedirectToAction("Index");
            return View(new CreateDriverViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDriver(CreateDriverViewModel model)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            if (_accountService.IsUsernameTaken(model.Username))
            {
                ModelState.AddModelError("Username", "This email is already registered.");
            }

            if (ModelState.IsValid)
            {
                _driverService.AddDriver(model.Driver);
                var userAccount = new RegisterViewModel
                {
                    Username = model.Username,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    Role = "Driver"
                };
                _accountService.CreateAccount(userAccount);
                TempData["Success"] = "Driver created successfully!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult UpdateDriver(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            var driver = _driverService.GetDriverDetails(id);
            if (driver == null) return NotFound();
            return View(driver);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDriver(Driver driver)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            if (ModelState.IsValid)
            {
                _driverService.UpdateDriver(driver);
                TempData["Success"] = "Driver updated successfully.";
                return RedirectToAction("Index");
            }
            return View(driver);
        }

        // --- ADDED THIS GET METHOD TO CAPTURE THE LINK CLICK ---
        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");

            // This captures the GET request from your <a> tag and redirects to the logic below
            return DeleteConfirmed(id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");

            var driver = _driverService.GetDriverDetails(id);
            if (driver == null) return NotFound();

            // BUSINESS RULE: Cannot delete if Driver is currently on a trip
            if (driver.status == DriverStatus.ON_TRIP)
            {
                TempData["Error"] = $"Constraint Failed: Cannot delete {driver.name} while they are ON_TRIP.";
                return RedirectToAction("Index");
            }

            try
            {
                _driverService.DeleteDriver(id);
                TempData["Success"] = "Driver deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Cannot delete driver due to historical trip records.";
            }
            return RedirectToAction("Index");
        }
    }
}