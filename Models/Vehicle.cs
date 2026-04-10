using System.ComponentModel.DataAnnotations;

namespace TransportationManagement.Models
{
    public class Vehicle
    {
        [Key]
        public int vehicleId { get; set; }

        [Required]
        [StringLength(50)]
        public string vehicleNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string model { get; set; } = string.Empty;

        [Required]
        public int capacity { get; set; }
        public VehicleStatus vehiclestatus { get; set; } 

		public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public ICollection<FuelEntry> FuelEntries { get; set; } = new List<FuelEntry>();
    }
	public enum VehicleStatus
	{
		ACTIVE, IN_SERVICE, RETIRED
	}
}
