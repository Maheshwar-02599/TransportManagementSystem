using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportationManagement.Models
{
    public class Driver
    {
        [Key]
        public int driverId { get; set; }

        [Required]
        [StringLength(100)]
        public string name { get; set; } = string.Empty;

		[Required(ErrorMessage = "License Number is required.")]
		[StringLength(16, MinimumLength = 16, ErrorMessage = "License number must be exactly 16 characters.")]
		public string licenseNumber { get; set; } = string.Empty;

		[Required(ErrorMessage = "Mobile Number is required.")]
		[RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits.")]

		public string contactNumber { get; set; } = string.Empty;

        public DriverStatus status { get; set; } 

        // Link to application user (optional). Requires a migration to persist to DB.
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

		


		public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
	public enum DriverStatus { AVAILABLE, ON_TRIP, INACTIVE }
}
