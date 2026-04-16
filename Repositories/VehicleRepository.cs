using System.Collections.Generic;
using System.Linq;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDbContext _context;
        public VehicleRepository(ApplicationDbContext context) { _context = context; }

        public List<Vehicle> GetAllVehicles() => _context.Vehicles.ToList();

        public Vehicle? GetVehicleById(int vehicleId) => _context.Vehicles.Find(vehicleId);

        public void AddVehicle(Vehicle vehicle) { _context.Vehicles.Add(vehicle); _context.SaveChanges(); }

        public void UpdateVehicle(Vehicle vehicle) { _context.Vehicles.Update(vehicle); _context.SaveChanges(); }

        public void DeleteVehicle(int vehicleId)
        {
            var v = _context.Vehicles.Find(vehicleId);
            if (v != null) { _context.Vehicles.Remove(v); _context.SaveChanges(); }
        }
    }
}