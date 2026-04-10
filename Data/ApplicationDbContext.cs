using Microsoft.EntityFrameworkCore;
using TransportationManagement.Models;
using System.Security.Cryptography;
using System.Text;

namespace TransportationManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Only these 6 tables will be created in your database
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<FuelEntry> FuelEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Vehicle).WithMany(v => v.Trips)
                .HasForeignKey(t => t.vehicleId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Driver).WithMany(d => d.Trips)
                .HasForeignKey(t => t.driverId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.Vehicle).WithMany(v => v.MaintenanceRecords)
                .HasForeignKey(m => m.vehicleId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FuelEntry>()
                .HasOne(f => f.Vehicle).WithMany(v => v.FuelEntries)
                .HasForeignKey(f => f.vehicleId).OnDelete(DeleteBehavior.Cascade);

            // Seed default Admin
            modelBuilder.Entity<User>().HasData(new User
            {
                Id       = 1,
                Username = "admin@transport.com",
                Password = HashPassword("Admin@123"),
                Role     = "Admin"
            });
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash  = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
