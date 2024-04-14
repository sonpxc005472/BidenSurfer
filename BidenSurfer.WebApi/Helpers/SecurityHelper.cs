using System;
using System.Security.Cryptography;
using System.Text;

namespace BidenSurfer.WebApi.Helpers
{
    public static class SecurityHelper
    {
        public static string GenerateHashedPassword(string password)
        {
            using (var hmac = new HMACSHA256())
            {
                // Compute hash from the password
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string
                return Convert.ToBase64String(hash);
            }
        }
    }
}
