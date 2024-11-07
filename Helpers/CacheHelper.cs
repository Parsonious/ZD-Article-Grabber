using System.Security.Cryptography;
using System.Text;
namespace ZD_Article_Grabber.Helpers 
{
    public class CacheHelper
    {
        public static string GenerateCacheKey(string prefix, string input)
        {
            // Sanitize input
            string sanitizedInput = PathHelper.SanitizeInput(input);

            // Compute hash
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sanitizedInput));
            string hashString = BitConverter.ToString(hashBytes).Replace('-', '_').ToLowerInvariant();
            return $"{prefix}_{hashString}";
        }

    }
}
