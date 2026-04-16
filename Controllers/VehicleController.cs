using Microsoft.AspNetCore.Mvc;
using TransportationManagement.Models;
using TransportationManagement.Services;
using System;
using System.Linq;

namespace TransportationManagement.Controllers
{
    public class VehicleController : Controller
    {
        private readonly VehicleService _vehicleSvc;
        private readonly TripService _routeSvc;

        public VehicleController(VehicleService vehicleService, TripService tripService)
        {
            _vehicleSvc = vehicleService;
            _routeSvc = tripService;
        }

        private bool CanView() => HttpContext.Session.GetString("Role") == "Admin" || HttpContext.Session.GetString("Role") == "FleetManager";
        private bool CanEdit() => HttpContext.Session.GetString("Role") == "FleetManager";

        public IActionResult Index()
        {
            if (!CanView()) return RedirectToAction("Login", "Account");
            var unfinishedRoutes = _routeSvc.GetAllTrips().Where(t => t.tripStatus != TripStatus.COMPLETED).ToList();
            ViewBag.BusyVehicleIds = unfinishedRoutes.Select(t => t.vehicleId).ToList();
            return View(_vehicleSvc.GetAllVehicles());
        }

        [HttpGet]
        public IActionResult AddVehicle()
        {
            if (!CanEdit()) return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddVehicle(Vehicle vehicleData)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            if (ModelState.IsValid)
            {
                _vehicleSvc.AddVehicle(vehicleData);
                TempData["Success"] = "Vehicle added successfully.";
                return RedirectToAction("Index");
            }
            return View(vehicleData);
        }

        [HttpGet]
        public IActionResult UpdateVehicle(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            var vehicleData = _vehicleSvc.GetVehicleDetails(id);
            if (vehicleData == null) return NotFound();
            return View(vehicleData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateVehicle(Vehicle vehicleData)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            if (ModelState.IsValid)
            {
                _vehicleSvc.UpdateVehicle(vehicleData);
                TempData["Success"] = "Vehicle updated successfully.";
                return RedirectToAction("Index");
            }
            return View(vehicleData);
        }

        // --- ADDED THIS METHOD TO FIX REDIRECTION ---
        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");

            // Instead of showing a new page, we process the logic and redirect
            // If you want a confirmation page, return View(vehicleData) here.
            // Since you want it to work with your current frontend link:
            return DeleteConfirmed(id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");

            var vehicle = _vehicleSvc.GetVehicleDetails(id);
            if (vehicle == null) return NotFound();

            // Check for Active Trips
            bool isActivelyDeployed = _routeSvc.GetAllTrips()
                .Any(t => t.vehicleId == id && t.tripStatus != TripStatus.COMPLETED);

            // BUSINESS RULE: Cannot delete if In Service or currently On Trip
            if (isActivelyDeployed || vehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
            {
                TempData["Error"] = $"Deletion Denied: Vehicle {vehicle.vehicleNumber} is currently IN_SERVICE or assigned to an active trip.";
                return RedirectToAction("Index");
            }

            try
            {
                _vehicleSvc.DeleteVehicle(id);
                TempData["Success"] = "Vehicle deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Cannot delete vehicle due to existing history (Trips/Fuel).";
            }
            return RedirectToAction("Index");
        }

        // Added detail view just in case it's needed for the eye icon
        [HttpGet]
        public IActionResult GetVehicleDetails(int id)
        {
            if (!CanView()) return RedirectToAction("Login", "Account");
            var vehicleData = _vehicleSvc.GetVehicleDetails(id);
            if (vehicleData == null) return NotFound();
            return View(vehicleData);
        }
    }
}