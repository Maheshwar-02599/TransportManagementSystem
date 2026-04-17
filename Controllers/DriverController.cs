using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Models;
using TransportationManagement.Services;
using TransportationManagement.ViewModels;
using System;
using Microsoft.AspNetCore.Http;

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

        // Helper methods for Session-based Role validation
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

        // --- UPDATED: This now matches asp-action="Edit" in your Index.cshtml ---
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!CanEdit())
            {
                TempData["Error"] = "Permission Denied: You must be a Fleet Manager to edit drivers.";
                return RedirectToAction("Index");
            }

            var driver = _driverService.GetDriverDetails(id);
            if (driver == null) return NotFound();

            // If your view file is named 'UpdateDriver.cshtml', use: return View("UpdateDriver", driver);
            // If it's named 'Edit.cshtml', use: return View(driver);
            return View(driver);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Driver driver)
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

        // Optional: Keep UpdateDriver as an alias so existing links don't break
        [HttpGet] public IActionResult UpdateDriver(int id) => RedirectToAction("Edit", new { id });

        [HttpGet]
        public IActionResult GetDriverDetails(int id)
        {
            var driver = _driverService.GetDriverDetails(id);
            if (driver == null) return NotFound();
            return View(driver);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            return DeleteConfirmed(id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");

            var driver = _driverService.GetDriverDetails(id);
            if (driver == null) return NotFound();

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