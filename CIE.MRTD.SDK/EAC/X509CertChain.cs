using CIE.MRTD.SDK.Crypto;
using CIE.MRTD.SDK.Util;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CIE.MRTD.SDK.EAC
{
    internal class X509CertChain
    {

        List<X509Certificate2> certificates;
        public X509CertChain(List<X509Certificate2> certificates)
        {
            this.certificates = certificates;
        }
        public List<X509Certificate2> getPath(X509Certificate2 lastCert)
        {
            List<X509Certificate2> chain = new List<X509Certificate2>();
            var certStore = new List<X509Certificate2>(certificates);
            if (certPathRecur(chain, lastCert,certStore))
                return chain;
            else
                return null;
        }

        public string HexDump(byte[] data)
        {
            if (data == null)
                return "";
            StringBuilder sb = new StringBuilder(data.Length * 2);
            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("X02"));
            return sb.ToString();
        }

        bool IssuedBy(X509Certificate2 cert, X509Certificate2 issuer)
        {
            try
            {
                if (!new ByteArray(cert.IssuerName.RawData).IsEqual(issuer.SubjectName.RawData)) { 
                    // verifico attributo per attributo
                    var IssuerTag = ASN1Tag.Parse(cert.IssuerName.RawData,false);
                    var SubjectTag = ASN1Tag.Parse(issuer.SubjectName.RawData,false);
                    Dictionary<string,byte[]> IssuerComponents=new Dictionary<string,byte[]>();
                    Dictionary<string,byte[]> SubjectComponents=new Dictionary<string,byte[]>();
                    foreach (var c in IssuerTag.children) {
                        var comp = c.Child(0);
                        IssuerComponents[ASN1ObjIdDisplay.singleton.contentString(comp.Child(0))] = comp.Child(1).Data;
                    }
                    foreach (var c in SubjectTag.children)
                    {
                        var comp = c.Child(0);
                        SubjectComponents[ASN1ObjIdDisplay.singleton.contentString(comp.Child(0))] = comp.Child(1).Data;
                    }
                    string[] keys=new string[IssuerComponents.Count];
                    IssuerComponents.Keys.CopyTo(keys, 0);
                    foreach (var o in keys)
                    {
                        if (!SubjectComponents.ContainsKey(o))
                            return false;
                        var sub=UTF8Encoding.UTF8.GetString(SubjectComponents[o]);
                        var iss=UTF8Encoding.UTF8.GetString(IssuerComponents[o]);
                        if (sub!=iss)
                            return false;
                        IssuerComponents.Remove(o);
                        SubjectComponents.Remove(o);
                    }
                    if (IssuerComponents.Count > 0 || SubjectComponents.Count > 0)
                        return false;
                }

                var akiExt = cert.Extensions["2.5.29.35"];
                if (akiExt != null)
                {
                    ASN1Tag akiTag = ASN1Tag.Parse(akiExt.RawData,false);
                    var aki = akiTag.CheckTag(0x30);
                    var aki2Tag = ASN1Tag.Parse(aki.Data,false);
                    var akiData = aki2Tag.CheckTag(0x80).Data;


                    var skiExt = issuer.Extensions["2.5.29.14"] as X509SubjectKeyIdentifierExtension;
                    if (skiExt == null)
                        return false;

                    if (skiExt.SubjectKeyIdentifier != HexDump(akiData))
                        return false;
                }

                // verifico che la firma torni
                var certTag = ASN1Tag.Parse(cert.RawData, false);
                var signature = certTag.Child(2, 0x03).Data;
                var keyTag = ASN1Tag.Parse(issuer.PublicKey.EncodedKeyValue.RawData);
                var module = keyTag.Child(0, 2).Data;
                var pubExp = keyTag.Child(1, 2).Data;

                var rsa = new RSA();
                var decSignature = ByteArray.RemoveBT1(rsa.RawRsa(module, pubExp, signature));
                byte[] signedHash = null, certHash = null;
                var toHash = new ByteArray(cert.RawData).Sub((int)certTag.children[0].StartPos, (int)(certTag.children[0].EndPos - certTag.children[0].StartPos));
                if (cert.SignatureAlgorithm.Value == "1.2.840.113549.1.1.5")
                {
                    signedHash = rsa.RemoveSha1(decSignature);
                    certHash = new SHA1().Digest(toHash);
                }
                else if (cert.SignatureAlgorithm.FriendlyName == "1.2.840.113549.1.1.11")
                {
                    signedHash = rsa.RemoveSha256(decSignature);
                    certHash = new SHA256().Digest(toHash);
                }
                else
                    throw new Exception("Algoritmo non supportato");
                if (!new ByteArray(signedHash).IsEqual(certHash))
                    return false;
                return true;
            }
            catch { }
            return false;
        }
        bool IsRoot(X509Certificate2 cert) {
            if (!new ByteArray(cert.IssuerName.RawData).IsEqual(cert.SubjectName.RawData))
                return false;
            // verifico che in basic constraints ci sia l'uso CA
            var bcExt = cert.Extensions["2.5.29.19"] as X509BasicConstraintsExtension;
            if (bcExt==null || !bcExt.CertificateAuthority)
                return false;

            var kuExt = cert.Extensions["2.5.29.15"] as X509KeyUsageExtension;
            if (kuExt==null || (kuExt.KeyUsages & X509KeyUsageFlags.KeyCertSign) != X509KeyUsageFlags.KeyCertSign)
                return false;

            return IssuedBy(cert, cert);
        }
        bool certPathRecur(List<X509Certificate2> chain, X509Certificate2 lastCert,List<X509Certificate2> certStore)
        {
            // verifico se il certificato è autofirmato
            if (IsRoot(lastCert))
            {
                chain.Add(lastCert);
                return true;
            }

            // verifico se questo certificao è firmato da uno di quelli dello store
            foreach (var c in certStore)
            {
                if (IssuedBy(lastCert, c)) {
                    var modCertStore = new List<X509Certificate2>(certStore);
                    modCertStore.Remove(c);
                    if (certPathRecur(chain, c, modCertStore))
                    {
                        chain.Add(lastCert);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
