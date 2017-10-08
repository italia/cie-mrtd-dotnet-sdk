using CIE.MRTD.SDK.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace CIE.MRTD.SDK.Crypto
{
    public class CVCert
    {
        public static List<CVCert> ReadAllCertInDirectory(String path)
        {
            List<CVCert> certs = new List<CVCert>();
            foreach (var v in Directory.GetFiles(path))
            {
                try
                {
                    certs.Add(new CVCert(File.ReadAllBytes(v)));
                }
                catch { }
            }
            return certs;
        }
        public int Version { get; set; }
        public String Name { get; set; }
        public ByteArray PubKeyAlgoOID { get; set; }
        public ByteArray PubKeyModule { get; set; }
        public ByteArray PubKeyExponent { get; set; }
        public String Issuer { get; set; }
        public ByteArray CertificateTemplateOID { get; set; }
        public ByteArray CertificateTemplateValue { get; set; }
        public ByteArray ValidFrom { get; set; }
        public ByteArray Expire { get; set; }
        public ByteArray Signature { get; set; }
        public ByteArray RawCert { get; set; }
        public DateTime ValidFromDate
        {
            get
            {
                int year = ValidFrom[0] * 10 + ValidFrom[1] + 2000;
                int month = ValidFrom[2] * 10 + ValidFrom[3];
                int day = ValidFrom[4] * 10 + ValidFrom[5];
                return new DateTime(year, month, day);
            }
        }
        public DateTime ExpiresDate
        {
            get
            {
                int year = Expire[0] * 10 + Expire[1] + 2000;
                int month = Expire[2] * 10 + Expire[3];
                int day = Expire[4] * 10 + Expire[5];
                return new DateTime(year, month, day);
            }
        }

        public bool IssuedBy(CVCert parent)
        {
            try
            {
                RSA rsa = new RSA();
                var decrypt = rsa.RawRsa(parent.PubKeyModule, parent.PubKeyExponent, Signature);
                var hash = ByteArray.RemoveBT1(decrypt);
                if (hash.Size < decrypt.Size)
                    return true;
            }
            catch { }
            return false;
        }
        public CVCert(byte[] data)
        {
            ASN1Tag cert = ASN1Tag.Parse(data, false);
            cert.CheckTag(0x7F21);
            ASN1Tag certContent = cert.Child(0, 0x7F4E);
            Signature = cert.Child(1, 0x5F37).Data;
            Version = (int)new ByteArray(certContent.Child(0, 0x5F29).Data).ToUInt;
            Issuer = new ByteArray(certContent.Child(1, 0x42).Data).ToASCII;
            Name = new ByteArray(certContent.Child(3, 0x5F20).Data).ToASCII;
            ValidFrom = certContent.Child(5, 0x5F25).Data;
            Expire = certContent.Child(6, 0x5F24).Data;
            ASN1Tag PubKey = certContent.Child(2, 0x7F49);
            PubKeyAlgoOID = PubKey.Child(0, 0x06).Data;
            PubKeyModule = PubKey.Child(1, 0x81).Data;
            PubKeyExponent = PubKey.Child(2, 0x82).Data;
            ASN1Tag certTemplate = certContent.Child(4, 0x7F4C);
            CertificateTemplateOID = certTemplate.Child(0, 0x06).Data;
            CertificateTemplateValue = certTemplate.Child(1, 0x53).Data;
            RawCert = new ByteArray(data).Left((int)cert.EndPos);
        }
        public override string ToString()
        {
            return Name + " from " + Issuer;
        }
    }
}
