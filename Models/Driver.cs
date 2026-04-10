using System.ComponentModel.DataAnnotations;

namespace TransportationManagement.Models
{
    public class Driver
    {
        [Key]
        public int driverId { get; set; }

        [Required]
        [StringLength(100)]
        public string name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string licenseNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string contactNumber { get; set; } = string.Empty;

        public DriverStatus status { get; set; } 


		public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
	public enum DriverStatus { AVAILABLE, ON_TRIP, INACTIVE }
}
