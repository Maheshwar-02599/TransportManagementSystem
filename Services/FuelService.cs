using TransportationManagement.Interfaces;
using TransportationManagement.Models;
namespace TransportationManagement.Services
{
    public class FuelService
    {
        private readonly IFuelRepository _repo;
        public FuelService(IFuelRepository repo) { _repo = repo; }
        public List<FuelEntry> GetAllFuelEntries() => _repo.GetAllFuelEntries();
        public FuelEntry? GetFuelEntryById(int id) => _repo.GetFuelEntryById(id);
        public List<FuelEntry> GetFuelConsumption(int vehicleId) => _repo.GetFuelEntriesByVehicleId(vehicleId);
        public List<FuelEntry> GenerateFuelReport() => _repo.GetAllFuelEntries();
        public void AddFuelEntry(FuelEntry f) => _repo.AddFuelEntry(f);
        public void UpdateFuelEntry(FuelEntry f) => _repo.UpdateFuelEntry(f);
        public void DeleteFuelEntry(int id) => _repo.DeleteFuelEntry(id);
    }
}
