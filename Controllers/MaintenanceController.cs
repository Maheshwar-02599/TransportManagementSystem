using System.Threading.Tasks;
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

		private bool CanView()
		{
			var r = HttpContext.Session.GetString("Role");
			return r == "Admin" || r == "FleetManager" || r == "MaintenanceEngineer";
		}

		private bool CanEdit()
		{
			var r = HttpContext.Session.GetString("Role");
			return r == "FleetManager" ||r =="MaintenanceEngineer";
		}

		private async Task LoadVehicles()
		{
			var vehicles = await _vehicleService.GetAllVehiclesAsync();
			ViewBag.Vehicles = new SelectList(vehicles, "vehicleId", "vehicleNumber");
		}

		public async Task<IActionResult> Index()
		{
			if (!CanView()) return RedirectToAction("Login", "Account");
			var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();
			return View(records);
		}

		[HttpGet]
		public async Task<IActionResult> ScheduleMaintenance()
		{
			if (!CanEdit()) return RedirectToAction("Index");
			await LoadVehicles();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ScheduleMaintenance(MaintenanceRecord record)
		{
			if (!CanEdit()) return RedirectToAction("Index");

			if (ModelState.IsValid)
			{
				await _maintenanceService.ScheduleMaintenanceAsync(record);

				// Now using async Vehicle service
				var fleetVehicle = await _vehicleService.GetVehicleDetailsAsync(record.vehicleId);
				if (fleetVehicle != null)
				{
					fleetVehicle.vehiclestatus = VehicleStatus.IN_SERVICE;
					await _vehicleService.UpdateVehicleAsync(fleetVehicle);
				}

				TempData["Success"] = "Maintenance scheduled and vehicle successfully marked as IN_SERVICE.";
				return RedirectToAction("Index");
			}
			await LoadVehicles();
			return View(record);
		}

		[HttpGet]
		public async Task<IActionResult> UpdateServiceRecord(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
			if (record == null) return NotFound();
			await LoadVehicles();
			return View(record);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateServiceRecord(MaintenanceRecord record)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			if (ModelState.IsValid)
			{
				await _maintenanceService.UpdateServiceRecordAsync(record);
				TempData["Success"] = "Record updated.";
				return RedirectToAction("Index");
			}
			await LoadVehicles();
			return View(record);
		}

		public async Task<IActionResult> GetMaintenanceHistory(int vehicleId)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");
			var records = await _maintenanceService.GetMaintenanceHistoryAsync(vehicleId);
			ViewBag.VehicleId = vehicleId;
			return View(records);
		}

		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
			if (record == null) return NotFound();
			return View(record);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			await _maintenanceService.DeleteMaintenanceAsync(id);
			TempData["Success"] = "Record deleted.";
			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<IActionResult> GetMaintenanceDetails(int id)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
			if (record == null) return NotFound();

			return View(record);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CompleteMaintenance(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");

			var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
			if (record != null)
			{
				if (string.IsNullOrEmpty(record.remarks))
					record.remarks = "[COMPLETED]";
				else if (!record.remarks.Contains("[COMPLETED]"))
					record.remarks += " [COMPLETED]";

				await _maintenanceService.UpdateServiceRecordAsync(record);

				var fleetVehicle = await _vehicleService.GetVehicleDetailsAsync(record.vehicleId);
				if (fleetVehicle != null && fleetVehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
				{
					fleetVehicle.vehiclestatus = VehicleStatus.ACTIVE;
					await _vehicleService.UpdateVehicleAsync(fleetVehicle);
				}

				TempData["Success"] = "Service completed! Record frozen and Vehicle is now ACTIVE.";
			}
			return RedirectToAction("Index");
		}
	}
}