using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
    public class AccountRepository
    {
        private readonly ApplicationDbContext _context;
        public AccountRepository(ApplicationDbContext context) { _context = context; }

        public async Task<List<User>> GetAllUsers() => await _context.Users.ToListAsync();

        public async Task<int> GetUserCount() =>await _context.Users.CountAsync();

        public async Task<User?> GetUserById(int id) => await _context.Users.FindAsync(id);

        public async Task<bool> CheckIfUserExists(string username) => await _context.Users.AnyAsync(u => u.Username == username);

		public async Task<int> RegisterUser(User user)
		{
			await _context.Users.AddAsync(user);
			await _context.SaveChangesAsync();
			return user.Id; // Entity Framework automatically fills this in after SaveChanges!
		}

		public async Task UpdateUser(User user)
        {
            var existing =await _context.Users.FindAsync(user.Id);
            if (existing != null)
            {
                existing.Username = user.Username;
                existing.Role = user.Role;
                await _context.SaveChangesAsync();
            }
        }

        public void DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public User? GetUserByCredentials(string username, string hashedPassword) =>
            _context.Users.FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);
    }
}