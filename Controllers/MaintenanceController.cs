using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using TransportationManagement.Models;
using TransportationManagement.Services;

namespace TransportationManagement.Controllers
{
	public class MaintenanceController : Controller
	{
		private readonly MaintenanceService _maintenanceService;
		private readonly VehicleService _vehicleService;
		private readonly TripService _tripService;

		public MaintenanceController(MaintenanceService maintenanceService, VehicleService vehicleService, TripService tripService)
		{
			_maintenanceService = maintenanceService;
			_vehicleService = vehicleService;
			_tripService = tripService;
		}

		private bool CanView()
		{
			var r = HttpContext.Session.GetString("Role");
			return r == "Admin" || r == "FleetManager" || r == "MaintenanceEngineer";
		}

		private bool CanSchedule()
		{
			var r = HttpContext.Session.GetString("Role");
			// ONLY FleetManager and Admin can schedule
			return r == "FleetManager" || r == "Admin";
		}

		private bool CanManageRecords()
		{
			var r = HttpContext.Session.GetString("Role");
			// ONLY MaintenanceEngineer and Admin can Edit, Delete, or Complete
			return r == "MaintenanceEngineer" || r == "Admin";
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

		// --- SCHEDULING (Fleet Manager Only) ---

		[HttpGet]
		public async Task<IActionResult> ScheduleMaintenance()
		{
			if (!CanSchedule()) return RedirectToAction("Index");
			await LoadVehicles();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ScheduleMaintenance(MaintenanceRecord record)
		{
			if (!CanSchedule()) return RedirectToAction("Index");

			var allTrips = await _tripService.GetAllTripsAsync();
			bool isVehicleAssigned = allTrips.Any(t => t.vehicleId == record.vehicleId && t.tripStatus != TripStatus.COMPLETED);

			if (isVehicleAssigned)
			{
				ModelState.AddModelError("vehicleId", "Cannot schedule maintenance: This vehicle is already assigned to a Planned or Active trip.");
			}

			if (ModelState.IsValid)
			{
				await _maintenanceService.ScheduleMaintenanceAsync(record);

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

		// --- MANAGING RECORDS (Maintenance Engineer Only) ---

		[HttpGet]
		public async Task<IActionResult> UpdateServiceRecord(int id)
		{
			if (!CanManageRecords()) return RedirectToAction("Index");
			var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
			if (record == null) return NotFound();
			await LoadVehicles();
			return View(record);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateServiceRecord(MaintenanceRecord record)
		{
			if (!CanManageRecords()) return RedirectToAction("Index");
			if (ModelState.IsValid)
			{
				await _maintenanceService.UpdateServiceRecordAsync(record);
				TempData["Success"] = "Record updated.";
				return RedirectToAction("Index");
			}
			await LoadVehicles();
			return View(record);
		}

		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			if (!CanManageRecords()) return RedirectToAction("Index");
			var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
			if (record == null) return NotFound();
			return View(record);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (!CanManageRecords()) return RedirectToAction("Index");
			await _maintenanceService.DeleteMaintenanceAsync(id);
			TempData["Success"] = "Record deleted.";
			return RedirectToAction("Index");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CompleteMaintenance(int id)
		{
			if (!CanManageRecords()) return RedirectToAction("Index");

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

		// --- VIEWING DETAILS (Everyone) ---

		public async Task<IActionResult> GetMaintenanceHistory(int vehicleId)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");
			var records = await _maintenanceService.GetMaintenanceHistoryAsync(vehicleId);
			ViewBag.VehicleId = vehicleId;
			return View(records);
		}

		[HttpGet]
		public async Task<IActionResult> GetMaintenanceDetails(int id)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			var record = await _maintenanceService.GetMaintenanceByIdAsync(id);
			if (record == null) return NotFound();

			return View(record);
		}
	}
}