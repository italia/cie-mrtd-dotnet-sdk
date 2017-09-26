using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CIE.MRTD.SDK.Crypto;

namespace CIE.MRTD.SDK.EAC
{
    internal class CertChain
    {

        List<CVCert> certificates;
        public CertChain(List<CVCert> certificates)
        {
            this.certificates = certificates;
        }
        public List<CVCert> getPath(String CVCAName, String ISName)
        { 
            String curCertLabel=ISName;
            List<CVCert> chain=new List<CVCert>();
            if (certPathRecur(chain, CVCAName, curCertLabel))
            {
                // verifico che la firma torni
                for (int i = 0; i < chain.Count-1; i++) { 
                    if (!chain[i+1].IssuedBy(chain[i]))
                        return null;
                }
                return chain;
            }
            else
                return null;
        }
        public bool certPathRecur(List<CVCert> chain, String CVCAName, String curCertLabel)
        {

            if (curCertLabel == CVCAName)
                return true; // non includo nella catena il certificato di CA perchè potrebbe non esserci! (nel caso di link cerificate)
            foreach(var curElem in certificates) {
                if (curElem.Name!=curCertLabel)
                    continue;
                if ((curCertLabel == curElem.Issuer && CVCAName == null) || // è un certificato di root e la CVCA non è specificata)
                    (curCertLabel == CVCAName )) // è il certificato richiesto
                {
                    if (CVCAName==null || CVCAName==curCertLabel)
                    chain.Add(curElem);
                    return true;
                }
                else {
                    if (curElem.Issuer != curCertLabel)
                    {
                        if (certPathRecur(chain, CVCAName, curElem.Issuer))
                        {
                            chain.Add(curElem);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
