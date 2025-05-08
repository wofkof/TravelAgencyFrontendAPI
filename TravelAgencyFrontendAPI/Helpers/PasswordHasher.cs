using System.Security.Cryptography;
using System.Text;

namespace TravelAgencyFrontendAPI.Helpers
{
    public static class PasswordHasher
    {
        public static void CreatePasswordHash(string password, out string hash, out string salt)
        {
            using var hmac = new HMACSHA512();
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = hmac.ComputeHash(passwordBytes);
            hash = Convert.ToBase64String(hashBytes);
            salt = Convert.ToBase64String(hmac.Key);
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var keyBytes = Convert.FromBase64String(storedSalt);
            using var hmac = new HMACSHA512(keyBytes);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var computedHash = hmac.ComputeHash(passwordBytes);
            var computedHashString = Convert.ToBase64String(computedHash);
            return computedHashString == storedHash;
        }
    }
}
