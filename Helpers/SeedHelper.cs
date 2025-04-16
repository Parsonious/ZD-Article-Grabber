using System.Security.Cryptography;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Extensions.Options;
namespace ZD_Article_Grabber.Helpers
{
    public class SeedHelper
    {
        public uint GetHybridSeed(uint hash)
        {
            byte[] bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            uint randomNumber = BitConverter.ToUInt32(bytes, 0);
            return (hash ^ randomNumber); //explicit cast and combination of random and deterministic seed use XOR
        }
        public uint GetRandomUInt()
        {
            byte[] bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            uint randomNumber = BitConverter.ToUInt32(bytes, 0);
            return randomNumber;
        }
        public ushort GetRandomUShort()
        {
            byte[] bytes = new byte[2];
            RandomNumberGenerator.Fill(bytes);
            ushort randomNumber = BitConverter.ToUInt16(bytes, 0);
            return randomNumber;
        }
    }
}
