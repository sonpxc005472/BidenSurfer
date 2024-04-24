using System;
using System.Security.Cryptography;
using System.Text;

namespace BidenSurfer.WebApi.Helpers
{
    public static class SecurityHelper
    {
        public static string GenerateHashedPassword(string password)
        {
            SHA1 sha1 = System.Security.Cryptography.SHA1.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
            byte[] hash = sha1.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
