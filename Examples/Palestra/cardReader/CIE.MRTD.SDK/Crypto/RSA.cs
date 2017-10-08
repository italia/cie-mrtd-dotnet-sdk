using CIE.MRTD.SDK.Util;
using System;
using System.Numerics;

namespace CIE.MRTD.SDK.Crypto
{
    internal class RSA
    {
        static public ByteArray SHA1Algo = new ByteArray(new byte[] { 0x30, 0x21, 0x30, 0x09, 0x06, 0x05, 0x2b, 0x0e, 0x03, 0x02, 0x1a, 0x05, 0x00, 0x04, 0x14 });
        static public ByteArray SHA256Algo = new ByteArray(new byte[] { 0x30, 0x31, 0x30, 0x0D, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x01, 0x05, 0x00, 0x04, 0x20 });

        public ByteArray RawRsa(ByteArray mod, ByteArray exp, ByteArray data)
        {
            int i = 0;
            for (i = 0; i < mod.Size; i++)
                if (mod[i] != 0)
                    break;
            if (i > 0)
                mod = mod.Sub(i);
            ByteArray resp = BigInteger.ModPow(data, exp, mod);
            if (resp.Size < mod.Size)
                resp = ByteArray.Fill(mod.Size - resp.Size, 0).Append(resp);
            return resp;
        }

        public ByteArray Rsa(ByteArray mod, ByteArray exp, ByteArray data)
        {
            int i = 0;
            for (i = 0; i < mod.Size; i++)
                if (mod[i] != 0)
                    break;
            if (i > 0)
                mod = mod.Sub(i);
            return BigInteger.ModPow(ByteArray.BT1Pad(data, mod.Size), exp, mod);
        }
        public ByteArray RsaWithSha1(ByteArray mod, ByteArray exp, ByteArray data)
        {
            int i = 0;
            for (i = 0; i < mod.Size; i++)
                if (mod[i] != 0)
                    break;
            if (i > 0)
                mod = mod.Sub(i);
            if (data.Size != 20)
                throw new Exception("La dimensione del digest SHA1 deve essere di 20 bytes");
            return BigInteger.ModPow(ByteArray.BT1Pad(SHA1Algo.Append(data), mod.Size), exp, mod);
        }
        public ByteArray RsaWithSha256(ByteArray mod, ByteArray exp, ByteArray data)
        {
            int i = 0;
            for (i = 0; i < mod.Size; i++)
                if (mod[i] != 0)
                    break;
            if (i > 0)
                mod = mod.Sub(i);
            if (data.Size != 32)
                throw new Exception("La dimensione del digest SHA256 deve essere di 32 bytes");
            return BigInteger.ModPow(ByteArray.BT1Pad(SHA256Algo.Append(data), mod.Size), exp, mod);
        }
        public ByteArray RemoveSha1(ByteArray signature)
        {
            if (signature.Left(SHA1Algo.Size).IsEqual(SHA1Algo))
                return signature.Sub(SHA1Algo.Size);
            throw new Exception("OID Algoritmo SHA1 non presente");
        }
        public ByteArray RemoveSha256(ByteArray signature)
        {
            if (signature.Left(SHA256Algo.Size).IsEqual(SHA256Algo))
                return signature.Sub(SHA256Algo.Size);
            throw new Exception("OID Algoritmo SHA256 non presente");
        }
    }
}
