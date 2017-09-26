using CIE.MRTD.SDK.Util;
using System.Security.Cryptography;

namespace CIE.MRTD.SDK.Crypto
{
    class DES
    {
        static TripleDES td1 = new TripleDESCryptoServiceProvider();
        static byte[] IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public ByteArray DES3Enc(byte[] masterKey, byte[] data)
        {
            lock (IV)
            {
                td1.Padding = PaddingMode.None;
                td1.IV = IV;
                td1.Key = masterKey;
                byte[] encrypted = td1.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
                return encrypted;
            }
        }

        public ByteArray DES3Dec(byte[] masterKey, byte[] data)
        {
            lock (IV)
            {
                td1.Padding = PaddingMode.None;
                td1.IV = IV;
                td1.Key = masterKey;
                byte[] encrypted = td1.CreateDecryptor().TransformFinalBlock(data, 0, data.Length);
                return encrypted;
            }
        }
    }
}
