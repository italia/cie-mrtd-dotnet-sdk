using CIE.MRTD.SDK.Util;
using System;
using System.Security.Cryptography;

namespace CIE.MRTD.SDK.Crypto
{
    class MAC
    {
        static byte[] k1 = new byte[8];
        static byte[] k2 = new byte[8];
        static byte[] k3 = new byte[8];
        static byte[] IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        static DESCryptoServiceProvider td1 = new DESCryptoServiceProvider();
        static DESCryptoServiceProvider td2 = new DESCryptoServiceProvider();
        static DESCryptoServiceProvider td3 = new DESCryptoServiceProvider();
        public ByteArray MAC3(byte[] key, byte[] data)
        {
            lock (IV)
            {
                td1.Padding = PaddingMode.Zeros;
                td2.Padding = PaddingMode.Zeros;
                td3.Padding = PaddingMode.Zeros;
                td1.IV = IV;
                td2.IV = IV;
                td3.IV = IV;
                Array.Copy(key, 0, k1, 0, 8);
                Array.Copy(key, (key.Length>=16 ? 8 : 0), k2, 0, 8);
                Array.Copy(key, (key.Length >= 24 ? 16 : 0), k3, 0, 8);
                td1.Key = k1;
                td2.Key = k2;
                td3.Key = k3;
                byte[] mid1 = td1.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
                byte[] mid2 = td2.CreateDecryptor().TransformFinalBlock(mid1, mid1.Length - 8, 8);
                byte[] mid3 = td3.CreateEncryptor().TransformFinalBlock(mid2, 0, 8);
                return mid3;
            }
        }
    }
}
