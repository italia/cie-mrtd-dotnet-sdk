using CIE.MRTD.SDK.Util;
using System;
using System.Numerics;

namespace CIE.MRTD.SDK.Crypto
{
    internal class DiffieHellmann
    {

        public static void GenerateKey(ByteArray Group, ByteArray Prime, ref ByteArray PrivateKey, ref ByteArray PublicKey)
        {
            while (Prime[0] == 0)
                Prime = Prime.Sub(1);
            byte[] privData = new byte[Prime.Size];
            Random rnd = new Random();
            for (int i = Prime.Size - 20; i < Prime.Size; i++)
            {
                privData[i] = (byte)(rnd.Next() % 256);
            }
            privData[Prime.Size - 20] = 1;
            PrivateKey = privData;

            PublicKey = BigInteger.ModPow(Group, PrivateKey, Prime);
        }
        public static ByteArray ComputeKey(ByteArray group, ByteArray prime, ByteArray privateKey, ByteArray dhOtherPub)
        {
            var key = (ByteArray)BigInteger.ModPow(dhOtherPub, privateKey, prime);
            while (prime[0] == 0)
                prime = prime.Sub(1);
            if (key.Size != prime.Size)
                key = ByteArray.Fill(prime.Size - key.Size, 0).Append(key);

            return key;
        }

    }
}
