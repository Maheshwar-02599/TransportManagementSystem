using System.Collections.Generic;
using TransportationManagement.Data;
using TransportationManagement.Models;
using TransportationManagement.Repositories;
using TransportationManagement.ViewModels;

namespace TransportationManagement.Services
{
    public class AccountService
    {
        private readonly AccountRepository _repo;
        public AccountService(AccountRepository repo) { _repo = repo; }

        public List<User> GetAllUsers() => _repo.GetAllUsers();
        public int GetTotalUserCount() => _repo.GetUserCount();
        public User? GetUserById(int id) => _repo.GetUserById(id);
        public bool IsUsernameTaken(string username) => _repo.CheckIfUserExists(username);

        public void CreateAccount(RegisterViewModel model)
        {
            var user = new User
            {
                Username = model.Username,
                Password = PasswordHelper.HashPassword(model.Password),
                Role = model.Role
            };
            _repo.RegisterUser(user);
        }


        public void UpdateUser(User user) => _repo.UpdateUser(user);
        public void RemoveUser(int id) => _repo.DeleteUser(id);

        public User? Authenticate(string username, string password)
        {
            string hashedPassword = PasswordHelper.HashPassword(password);
            return _repo.GetUserByCredentials(username, hashedPassword);
        }
    }
}