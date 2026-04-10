using System.ComponentModel.DataAnnotations;

namespace TransportationManagement.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;   // will store email

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;   // hashed password

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;       // Admin, FleetManager, Driver, MaintenanceEngineer
    }
}
