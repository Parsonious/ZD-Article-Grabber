using System.Text;
namespace ZD_Article_Grabber.Helpers
{
    public class FNV1aHashHelper
    {
        private const uint FnvPrime = 16777619;
        private const uint FnvOffsetBasis = 2166136261;
        internal uint GenerateID<T>(T input) //internal to prevent MethodSelectHelper from calling it
        {
            byte[] data = ObjectToByteArray(input);
            return ComputeFNV1aHash(data);
        }
        public string GenerateIDAsString<T>(T input)
        {
            uint hash = GenerateID(input);
            return hash.ToString();
        }
        public string GenerateIDAsHexString<T>(T input)
        {
         return ConvertHashToHexString(GenerateID(input));
        }
        public string GnerateIDAsBase64String<T>(T input)
        {
            return ConvertHashToBase64String(GenerateID(input));
        }
        public string GenerateIDAsBase64UrlString<T>(T input)
        {
            return ConvertHashToBase64UrlString(GenerateID(input));
        }
        private static byte[] ObjectToByteArray<T>(T obj)
        {
            return obj switch
            {
                null => Array.Empty<byte>(),
                string str => Encoding.UTF8.GetBytes(str),
                byte[] bytes => bytes,
                int i => BitConverter.GetBytes(i),
                uint u => BitConverter.GetBytes(u),
                ushort s => BitConverter.GetBytes(s),
                long l => BitConverter.GetBytes(l),
                float f => BitConverter.GetBytes(f),
                double d => BitConverter.GetBytes(d),
                IConvertible => BitConverter.GetBytes(Convert.ToDouble(obj)),
                _ => throw new System.ArgumentException("Unsupported type"),
            };
        }
        private protected static string ConvertHashToHexString(uint hash)
        {
            return hash.ToString("X8"); //pads out to 8 characters with 0s if needed
        }
        private protected static string ConvertHashToBase64String(uint hash)
        {
            return Convert.ToBase64String(BitConverter.GetBytes(hash));
        }
        private protected static string ConvertHashToBase64UrlString(uint hash)
        {
            return Convert.ToBase64String(BitConverter.GetBytes(hash))
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
        private protected uint ComputeFNV1aHash(byte[] data)
        {
            uint hash = FnvOffsetBasis;
            for (int i = 0; i < data.Length; i++ )
            {
                hash ^= data[i]; // XOR the low 8 bits of the byte into the bottom of the hash
                hash *= FnvPrime; // Multiply by the 32 bit FNV prime
            }
            return hash;
        }
    }
}
