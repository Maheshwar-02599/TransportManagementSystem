using System.Collections.Generic;
using System.Threading.Tasks;
using TransportationManagement.Models;

namespace TransportationManagement.Interfaces
{
	public interface IDriverRepository
	{
		Task<List<Driver>> GetAllDriversAsync();
		Task<Driver?> GetDriverByIdAsync(int driverId);
		Task<Driver?> GetDriverByUserIdAsync(int userId);
		Task AddDriverAsync(Driver driver);
		Task UpdateDriverAsync(Driver driver);
		Task DeleteDriverAsync(int driverId);
	}
}