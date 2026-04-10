using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportationManagement.Models
{
    public class MaintenanceRecord
    {
        public enum MaintainenceStatus{
        SCHEDULED, IN_SERVICE, COMPLETED
        }
        [Key]
        public int maintenanceId { get; set; }

        [Required]
        public int vehicleId { get; set; }

        [Required]
        [StringLength(100)]
        public string serviceType { get; set; } = string.Empty;

        [Required]
        public DateTime serviceDate { get; set; }

        [StringLength(255)]
        public string? remarks { get; set; }

        [ForeignKey("vehicleId")]
        public Vehicle? Vehicle { get; set; }
    }
}
