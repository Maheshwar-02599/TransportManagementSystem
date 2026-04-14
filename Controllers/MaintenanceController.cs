using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	public class MaintenanceController : Controller
	{
		private readonly MaintenanceService _maintenanceService;
		private readonly VehicleService _vehicleService;

		public MaintenanceController(MaintenanceService maintenanceService, VehicleService vehicleService)
		{
			_maintenanceService = maintenanceService;
			_vehicleService = vehicleService;
		}

		// --- NEW RBAC SECURITY LOGIC ---

		// 1. View Access: Admin, FleetManager, and MaintenanceEngineer
		private bool CanView()
		{
			var r = HttpContext.Session.GetString("Role");
			return r == "Admin" || r == "FleetManager" || r == "MaintenanceEngineer";
		}

		// 2. Edit Access: ONLY FleetManager and MaintenanceEngineer
		private bool CanEdit()
		{
			var r = HttpContext.Session.GetString("Role");
			return r == "FleetManager" || r == "MaintenanceEngineer";
		}

		// -------------------------------

		private void LoadVehicles()
		{
			var vehicles = _vehicleService.GetAllVehicles();
			ViewBag.Vehicles = new SelectList(vehicles, "vehicleId", "vehicleNumber");
		}

		public IActionResult Index()
		{
			if (!CanView()) return RedirectToAction("Login", "Account");
			return View(_maintenanceService.GetAllMaintenanceRecords());
		}

		[HttpGet]
		public IActionResult ScheduleMaintenance()
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			LoadVehicles();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ScheduleMaintenance(MaintenanceRecord record)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock

			if (ModelState.IsValid)
			{
				_maintenanceService.ScheduleMaintenance(record);

				// Force the vehicle status to change in the database
				var fleetVehicle = _vehicleService.GetVehicleDetails(record.vehicleId);
				if (fleetVehicle != null)
				{
					fleetVehicle.vehiclestatus = VehicleStatus.IN_SERVICE;
					_vehicleService.UpdateVehicle(fleetVehicle);
				}

				TempData["Success"] = "Maintenance scheduled and vehicle successfully marked as IN_SERVICE.";
				return RedirectToAction("Index");
			}
			LoadVehicles();
			return View(record);
		}

		[HttpGet]
		public IActionResult UpdateServiceRecord(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			var record = _maintenanceService.GetMaintenanceById(id);
			if (record == null) return NotFound();
			LoadVehicles();
			return View(record);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateServiceRecord(MaintenanceRecord record)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			if (ModelState.IsValid)
			{
				_maintenanceService.UpdateServiceRecord(record);
				TempData["Success"] = "Record updated.";
				return RedirectToAction("Index");
			}
			LoadVehicles();
			return View(record);
		}

		public IActionResult GetMaintenanceHistory(int vehicleId)
		{
			if (!CanView()) return RedirectToAction("Login", "Account"); // Allowed to view
			var records = _maintenanceService.GetMaintenanceHistory(vehicleId);
			ViewBag.VehicleId = vehicleId;
			return View(records);
		}

		[HttpGet]
		public IActionResult Delete(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			var record = _maintenanceService.GetMaintenanceById(id);
			if (record == null) return NotFound();
			return View(record);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // Security lock
			_maintenanceService.DeleteMaintenance(id);
			TempData["Success"] = "Record deleted.";
			return RedirectToAction("Index");
		}

		[HttpGet]
		public IActionResult GetMaintenanceDetails(int id)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			var record = _maintenanceService.GetMaintenanceById(id);
			if (record == null) return NotFound();

			return View(record);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CompleteMaintenance(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index"); // FleetManager or MaintenanceEngineer

			var record = _maintenanceService.GetMaintenanceById(id);
			if (record != null)
			{
				// 1. FREEZE THE RECORD: Safely append a hidden completed flag to the remarks
				if (string.IsNullOrEmpty(record.remarks))
					record.remarks = "[COMPLETED]";
				else if (!record.remarks.Contains("[COMPLETED]"))
					record.remarks += " [COMPLETED]";

				_maintenanceService.UpdateServiceRecord(record);

				// 2. FREE THE VEHICLE: Find the vehicle and set it back to ACTIVE!
				var fleetVehicle = _vehicleService.GetVehicleDetails(record.vehicleId);
				if (fleetVehicle != null && fleetVehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
				{
					fleetVehicle.vehiclestatus = VehicleStatus.ACTIVE;
					_vehicleService.UpdateVehicle(fleetVehicle);
				}

				TempData["Success"] = "Service completed! Record frozen and Vehicle is now ACTIVE.";
			}
			return RedirectToAction("Index");
		}
	}
}