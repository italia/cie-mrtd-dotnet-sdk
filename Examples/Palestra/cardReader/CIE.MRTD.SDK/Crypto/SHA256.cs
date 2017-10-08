using CIE.MRTD.SDK.Util;
using System.Security.Cryptography;

namespace CIE.MRTD.SDK.Crypto
{
    internal class SHA256
    {
        static SHA256Managed sha = new SHA256Managed();
        public ByteArray Digest(byte[] data)
        {
            return sha.ComputeHash(data);
        }
    }
}
