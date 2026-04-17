using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

		// Made async to await the VehicleService
		private async Task LoadVehicles()
		{
			var vehicles = await _vehicleService.GetAllVehiclesAsync();
			ViewBag.Vehicles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(vehicles, "vehicleId", "vehicleNumber");
		}

		public async Task<IActionResult> Index()
		{
			if (!CanView()) return RedirectToAction("Login", "Account");
			var entries = await _fuelService.GetAllFuelEntriesAsync();
			return View(entries);
		}

		[HttpGet]
		public async Task<IActionResult> AddFuelEntry()
		{
			if (!CanEdit()) return RedirectToAction("Index");
			await LoadVehicles();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddFuelEntry(FuelEntry fuelEntry)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			if (ModelState.IsValid)
			{
				await _fuelService.AddFuelEntryAsync(fuelEntry);
				TempData["Success"] = "Fuel entry added.";
				return RedirectToAction("Index");
			}
			await LoadVehicles();
			return View(fuelEntry);
		}

		public async Task<IActionResult> GetFuelConsumption(int vehicleId)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");
			var entries = await _fuelService.GetFuelConsumptionAsync(vehicleId);
			ViewBag.VehicleId = vehicleId;
			return View(entries);
		}

		public async Task<IActionResult> GenerateFuelReport()
		{
			if (!CanView()) return RedirectToAction("Login", "Account");
			var report = await _fuelService.GenerateFuelReportAsync();
			return View(report);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			var entry = await _fuelService.GetFuelEntryByIdAsync(id);
			if (entry == null) return NotFound();
			await LoadVehicles();
			return View(entry);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(FuelEntry fuelEntry)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			if (ModelState.IsValid)
			{
				await _fuelService.UpdateFuelEntryAsync(fuelEntry);
				TempData["Success"] = "Fuel entry updated.";
				return RedirectToAction("Index");
			}
			await LoadVehicles();
			return View(fuelEntry);
		}

		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			var entry = await _fuelService.GetFuelEntryByIdAsync(id);
			if (entry == null) return NotFound();
			return View(entry);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (!CanEdit()) return RedirectToAction("Index");
			await _fuelService.DeleteFuelEntryAsync(id);
			TempData["Success"] = "Fuel entry deleted.";
			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<IActionResult> GetFuelEntryDetails(int id)
		{
			if (!CanView()) return RedirectToAction("Login", "Account");

			var entry = await _fuelService.GetFuelEntryByIdAsync(id);
			if (entry == null) return NotFound();

			return View(entry);
		}
	}
}