namespace TravelAgencyBackend.Helpers
{
    public class PasswordHasher
    {
        public static string Hash(string password)
            => BCrypt.Net.BCrypt.HashPassword(password);

        public static bool Verify(string inputPassword, string hashedPassword)
            => BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);
    }
}
