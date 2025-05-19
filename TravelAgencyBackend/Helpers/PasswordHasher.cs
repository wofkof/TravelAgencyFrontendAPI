using System.Security.Cryptography;
using System.Text;

namespace TravelAgencyBackend.Helpers
{
    public class PasswordHasher
    {
        //public static string Hash(string password)
        //    => BCrypt.Net.BCrypt.HashPassword(password);

        //public static bool Verify(string inputPassword, string hashedPassword)
        //    => BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);

        // 雜湊密碼：輸出 hash + salt
        public static void CreatePasswordHash(string password, out string hash, out string salt)
        {
            using var hmac = new HMACSHA512();
            var saltBytes = hmac.Key;
            salt = Convert.ToBase64String(saltBytes);
            hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        // 驗證密碼
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var key = Convert.FromBase64String(storedSalt);
            using var hmac = new HMACSHA512(key);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(computedHash) == storedHash;
        }
    }
}
