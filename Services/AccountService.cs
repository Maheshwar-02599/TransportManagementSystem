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

        public async Task<List<User>> GetAllUsers() => await _repo.GetAllUsers();
        public async Task<int> GetTotalUserCount() =>await  _repo.GetUserCount();
        public async Task<User?> GetUserById(int id) =>await  _repo.GetUserById(id);
        public async Task<bool> IsUsernameTaken(string username) => await _repo.CheckIfUserExists(username);

        public async Task CreateAccount(RegisterViewModel model)
        {
            var user = new User
            {
                Username = model.Username,
                Password = PasswordHelper.HashPassword(model.Password),
                Role = model.Role
            };
            await _repo.RegisterUser(user);
        }


        public async Task UpdateUser(User user) => await _repo.UpdateUser(user);
        public void RemoveUser(int id) => _repo.DeleteUser(id);

        public User? Authenticate(string username, string password)
        {
            string hashedPassword = PasswordHelper.HashPassword(password);
            return _repo.GetUserByCredentials(username, hashedPassword);
        }
    }
}