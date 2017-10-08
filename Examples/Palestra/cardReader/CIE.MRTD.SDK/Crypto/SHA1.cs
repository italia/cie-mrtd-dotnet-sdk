using CIE.MRTD.SDK.Util;
using System.Security.Cryptography;

namespace CIE.MRTD.SDK.Crypto
{
    internal class SHA1
    {
        static SHA1Managed sha = new SHA1Managed();
        public ByteArray Digest(byte[] data)
        {
            return sha.ComputeHash(data);
        }
    }
}
