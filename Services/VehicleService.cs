using System.Collections.Generic;
using TransportationManagement.Interfaces;
using TransportationManagement.Models;

namespace TransportationManagement.Services
{
    public class VehicleService
    {
        private readonly IVehicleRepository _repo;
        public VehicleService(IVehicleRepository repo) { _repo = repo; }
        public List<Vehicle> GetAllVehicles() => _repo.GetAllVehicles();
        public Vehicle? GetVehicleDetails(int id) => _repo.GetVehicleById(id);
        public void AddVehicle(Vehicle v) => _repo.AddVehicle(v);
        public void UpdateVehicle(Vehicle v) => _repo.UpdateVehicle(v);
        public void DeleteVehicle(int id) => _repo.DeleteVehicle(id);
        public List<Vehicle> ListAllVehicles() => _repo.GetAllVehicles();
    }
}