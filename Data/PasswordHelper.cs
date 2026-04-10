using System.Security.Cryptography;
using System.Text;

namespace TransportationManagement.Data
{
    public static class PasswordHelper
    {
        // Hash a plain password using SHA256
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash  = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // Verify plain password against stored hash
        public static bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }
    }
}
