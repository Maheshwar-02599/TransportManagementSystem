using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportationManagement.Models
{
    public class FuelEntry
    {
        [Key]
        public int fuelId { get; set; }

        [Required]
        public int vehicleId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal fuelQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal fuelCost { get; set; }

        [Required]
        public int odometerReading { get; set; }

        [Required]
        public DateTime entryDate { get; set; }

        [ForeignKey("vehicleId")]
        public Vehicle? Vehicle { get; set; }
    }
}
