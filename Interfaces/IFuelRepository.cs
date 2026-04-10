using TransportationManagement.Models;
namespace TransportationManagement.Interfaces
{
    public interface IFuelRepository
    {
        List<FuelEntry> GetAllFuelEntries();
        FuelEntry? GetFuelEntryById(int fuelId);
        List<FuelEntry> GetFuelEntriesByVehicleId(int vehicleId);
        void AddFuelEntry(FuelEntry fuelEntry);
        void UpdateFuelEntry(FuelEntry fuelEntry);
        void DeleteFuelEntry(int fuelId);
    }
}
