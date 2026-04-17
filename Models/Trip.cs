using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TransportationManagement.Models
{
    public class Trip
    {
        [Key]
        public int tripId { get; set; }

        [Required]
        public int vehicleId { get; set; }

        [Required]
        public int driverId { get; set; }

		[Required(ErrorMessage = "Origin is required")]
		[RegularExpression(@".*[a-zA-Z]+.*", ErrorMessage = "Origin must contain at least one letter. Numbers alone are not valid.")]
		public string origin { get; set; } = string.Empty;

        
        [Required(ErrorMessage = "Destination is required")]
        [RegularExpression(@".*[a-zA-Z]+.*", ErrorMessage = "Destination must contain at least one letter. Numbers alone are not valid.")]
		public string destination { get; set; } = string.Empty;

        public string? plannedRoute { get; set; }
        
        public TripStatus tripStatus { get; set; } 
		[ForeignKey("vehicleId")]
      
        public Vehicle? Vehicle { get; set; }

        [ForeignKey("driverId")]
        public Driver? Driver { get; set; }

		
	}
	public enum TripStatus { PLANNED, IN_PROGRESS, COMPLETED }
}
