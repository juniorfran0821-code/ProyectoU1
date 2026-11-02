using System.Security.Cryptography;
using System.Text;

namespace ProyectoU1.Helpers
{
    public static class SecurityHelper
    {
        public static string HashPassword(string plainTextPassword)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainTextPassword);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            var sb = new StringBuilder(64);
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

        public static bool VerifyPassword(string plainTextPassword, string storedHash)
        {
            string computedHash = HashPassword(plainTextPassword);
            return string.Equals(computedHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
