using System.Collections.Generic;
using System.Linq;
using TransportationManagement.Data;
using TransportationManagement.Models;

namespace TransportationManagement.Repositories
{
    public class AccountRepository
    {
        private readonly ApplicationDbContext _context;
        public AccountRepository(ApplicationDbContext context) { _context = context; }

        public List<User> GetAllUsers() => _context.Users.ToList();

        public int GetUserCount() => _context.Users.Count();

        public User? GetUserById(int id) => _context.Users.Find(id);

        public bool CheckIfUserExists(string username) => _context.Users.Any(u => u.Username == username);
        
        public void RegisterUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            var existing = _context.Users.Find(user.Id);
            if (existing != null)
            {
                existing.Username = user.Username;
                existing.Role = user.Role;
                _context.SaveChanges();
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