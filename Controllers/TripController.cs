using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TransportationManagement.Models;
using TransportationManagement.Services;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TransportationManagement.Controllers
{
    public class TripController : Controller
    {
        private readonly TripService _tripService;
        private readonly VehicleService _vehicleService;
        private readonly DriverService _driverService;
        private readonly FuelService _fuelService;

        public TripController(TripService tripService, VehicleService vehicleService,
                              DriverService driverService, FuelService fuelService)
        {
            _tripService = tripService;
            _vehicleService = vehicleService;
            _driverService = driverService;
            _fuelService = fuelService;
        }

        // --- Security Logic ---
        private bool CanView()
        {
            var r = HttpContext.Session.GetString("Role");
            return r == "Admin" || r == "FleetManager" || r == "Driver";
        }

        private bool CanEdit()
        {
            var r = HttpContext.Session.GetString("Role");
            return r == "FleetManager";
        }

        private void LoadDropdowns()
        {
            var vehicles = _vehicleService.GetAllVehicles();
            var drivers = _driverService.GetAllDrivers();
            ViewBag.Vehicles = new SelectList(vehicles, "vehicleId", "vehicleNumber");
            ViewBag.Drivers = new SelectList(drivers, "driverId", "name");
        }

        // --- Actions ---

        public IActionResult Index()
        {
            if (!CanView()) return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("Role");
            var username = HttpContext.Session.GetString("Username");

            if (role == "Driver" && !string.IsNullOrEmpty(username))
            {
                // Find the Driver record that matches this login session
                var driverRecord = _driverService.GetAllDrivers()
                    .FirstOrDefault(d => d.name.Equals(username, StringComparison.OrdinalIgnoreCase) ||
                                         username.StartsWith(d.name, StringComparison.OrdinalIgnoreCase));

                if (driverRecord != null)
                {
                    var myTrips = _tripService.GetAllTrips()
                        .Where(t => t.driverId == driverRecord.driverId)
                        .ToList();
                    return View(myTrips);
                }
                return View(new List<Trip>());
            }

            return View(_tripService.GetAllTrips());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartTrip(int id)
        {
            var trip = _tripService.GetTripPlan(id);
            if (trip != null)
            {
                trip.tripStatus = TripStatus.IN_PROGRESS;
                _tripService.UpdateTripStatus(trip);
                TempData["Success"] = "Trip started! Drive safely.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EndTrip(int id, int distanceTraveled)
        {
            var trip = _tripService.GetTripPlan(id);

            if (trip != null && distanceTraveled > 0)
            {
                trip.tripStatus = TripStatus.COMPLETED;
                _tripService.UpdateTripStatus(trip);

                decimal mileageKmPerLiter = 10.0m;
                decimal costPerLiter = 100.0m;
                decimal calculatedFuelQty = Math.Round((decimal)distanceTraveled / mileageKmPerLiter, 2);
                decimal calculatedFuelCost = Math.Round(calculatedFuelQty * costPerLiter, 2);

                var vehicleFuelHistory = _fuelService.GetFuelConsumption(trip.vehicleId);
                int lastOdometer = vehicleFuelHistory.Any() ? vehicleFuelHistory.Max(f => f.odometerReading) : 0;
                int newOdometerReading = lastOdometer + distanceTraveled;

                var autoFuelEntry = new FuelEntry
                {
                    vehicleId = trip.vehicleId,
                    fuelQuantity = calculatedFuelQty,
                    fuelCost = calculatedFuelCost,
                    odometerReading = newOdometerReading,
                    entryDate = DateTime.Now
                };

                _fuelService.AddFuelEntry(autoFuelEntry);
                TempData["Success"] = $"Trip completed! System auto-logged {calculatedFuelQty}L of fuel for {distanceTraveled}km traveled.";
            }
            else if (distanceTraveled <= 0)
            {
                TempData["Error"] = "Distance traveled must be greater than 0 to complete the trip.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult CreateTrip()
        {
            if (!CanEdit()) return RedirectToAction("Index");
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTrip(Trip trip)
        {
            if (!CanEdit()) return RedirectToAction("Index");

            // VALIDATION: Origin and Destination must not be the same
            if (!string.IsNullOrEmpty(trip.origin) && !string.IsNullOrEmpty(trip.destination))
            {
                if (trip.origin.Trim().Equals(trip.destination.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("destination", "Destination cannot be the same as the Origin location.");
                }
            }

            ModelState.Remove("Vehicle");
            ModelState.Remove("Driver");

            var fleetVehicle = _vehicleService.GetVehicleDetails(trip.vehicleId);
            if (fleetVehicle != null && fleetVehicle.vehiclestatus == VehicleStatus.IN_SERVICE)
            {
                ModelState.AddModelError("vehicleId", "DENIED: This vehicle is under maintenance.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _tripService.CreateTrip(trip);
                    TempData["Success"] = "Trip created successfully.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "System Error: " + ex.Message);
                }
            }
            LoadDropdowns();
            return View(trip);
        }

        [HttpGet]
        public IActionResult UpdateTripStatus(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");

            var trip = _tripService.GetTripPlan(id);
            if (trip == null) return NotFound();

            LoadDropdowns();
            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateTripStatus(Trip trip)
        {
            if (!CanEdit()) return RedirectToAction("Index");

            // VALIDATION: Origin and Destination must not be the same
            if (!string.IsNullOrEmpty(trip.origin) && !string.IsNullOrEmpty(trip.destination))
            {
                if (trip.origin.Trim().Equals(trip.destination.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("destination", "Destination cannot be the same as the Origin location.");
                }
            }

            ModelState.Remove("Vehicle");
            ModelState.Remove("Driver");

            if (ModelState.IsValid)
            {
                try
                {
                    _tripService.UpdateTripStatus(trip);
                    TempData["Success"] = "Trip details updated successfully.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Update failed: " + ex.Message);
                }
            }

            LoadDropdowns();
            return View(trip);
        }

        public IActionResult GetTripPlan(int id)
        {
            if (!CanView()) return RedirectToAction("Login", "Account");
            var tripData = _tripService.GetTripPlan(id);
            if (tripData == null) return NotFound();
            return View(tripData);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            var tripData = _tripService.GetTripPlan(id);
            if (tripData == null) return NotFound();
            return View(tripData);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!CanEdit()) return RedirectToAction("Index");
            try
            {
                var tripData = _tripService.GetTripPlan(id);
                if (tripData != null && tripData.tripStatus == TripStatus.IN_PROGRESS)
                {
                    TempData["Error"] = "Cannot delete a trip that is currently IN_PROGRESS.";
                    return RedirectToAction("Index");
                }
                _tripService.DeleteTrip(id);
                TempData["Success"] = "Trip deleted.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting trip: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}