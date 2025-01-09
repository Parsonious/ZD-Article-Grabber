using System.Security.Cryptography;
using System.Text;
using ZD_Article_Grabber.Interfaces;

namespace ZD_Article_Grabber.Helpers 
{
    public class CacheHelper : ICacheHelper
    {
        public string GenerateCacheKey(string prefix, string input)
        {
            // Sanitize input
            string sanitizedInput = PathHelper.SanitizeInput(input);

            // Compute hash
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sanitizedInput));
            StringBuilder hashBuilder = new StringBuilder(hashBytes.Length * 2);
            foreach ( byte b in hashBytes )
            {
                hashBuilder.Append(b.ToString("x2"));
            }
            string hashString = hashBuilder.ToString();
            return $"{prefix}_{hashString}";
        }

    }
}
