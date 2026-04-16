using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
    public class FuelController : Controller
    {
        private readonly FuelService _fuelService;
        private readonly VehicleService _vehicleService;

        public FuelController(FuelService fuelService, VehicleService vehicleService)
        {
            _fuelService = fuelService;
            _vehicleService = vehicleService;
        }

        // --- Security Logic ---
        private bool CanView()
        {
            var r = HttpContext.Session.GetString("Role");
            return r == "Admin" || r == "FleetManager";
        }

        private bool CanEdit()
        {
            var r = HttpContext.Session.GetString("Role");
            return r == "FleetManager";
        }

        private void LoadVehicles()
        {
            var vehicles = _vehicleService.GetAllVehicles();
            // Matching property names: vehicleId and vehicleNumber
            ViewBag.Vehicles = new SelectList(vehicles, "vehicleId", "vehicleNumber");
        }

        // --- Actions ---

        public IActionResult Index()
        {
            if (!CanView()) return RedirectToAction("Login", "Account");
            return View(_fuelService.GetAllFuelEntries());
        }

        public IActionResult GetFuelEntryDetails(int id)
        {
            if (!CanView()) return RedirectToAction("Login", "Account");
            var entry = _fuelService.GetFuelEntryById(id);
            if (entry == null) return NotFound();
            return View(entry);
        }

        [HttpGet]
        public IActionResult AddFuelEntry()
        {
            if (!CanEdit()) return RedirectToAction("Index");
            LoadVehicles();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddFuelEntry(FuelEntry fuelEntry)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            if (ModelState.IsValid)
            {
                _fuelService.AddFuelEntry(fuelEntry);
                TempData["Success"] = "Fuel entry added.";
                return RedirectToAction("Index");
            }
            LoadVehicles();
            return View(fuelEntry);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            var entry = _fuelService.GetFuelEntryById(id);
            if (entry == null) return NotFound();
            LoadVehicles();
            return View(entry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FuelEntry fuelEntry)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            if (ModelState.IsValid)
            {
                _fuelService.UpdateFuelEntry(fuelEntry);
                TempData["Success"] = "Fuel entry updated.";
                return RedirectToAction("Index");
            }
            LoadVehicles();
            return View(fuelEntry);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            var entry = _fuelService.GetFuelEntryById(id);
            if (entry == null) return NotFound();
            return View(entry);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            _fuelService.DeleteFuelEntry(id);
            TempData["Success"] = "Fuel entry deleted.";
            return RedirectToAction("Index");
        }

        public IActionResult GenerateFuelReport()
        {
            if (!CanView()) return RedirectToAction("Login", "Account");
            return View(_fuelService.GenerateFuelReport());
        }

        public IActionResult GetFuelConsumption(int vehicleId)
        {
            if (!CanView()) return RedirectToAction("Login", "Account");
            var entries = _fuelService.GetFuelConsumption(vehicleId);
            ViewBag.VehicleId = vehicleId;
            return View(entries);
        }
    }
}