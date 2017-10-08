using CIE.MRTD.SDK.Crypto;
using CIE.MRTD.SDK.PCSC;
using CIE.MRTD.SDK.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CIE.MRTD.SDK.EAC
{

    /// <summary>
    /// La classe EACException rappresenta un'eccezione incontrata durante l'esecuzione del protocollo EAC (BAC/PACE - ChipAuth - TerminalAuth)
    /// </summary>
    public class EACException : Exception
    {
        /// <summary>
        /// Costruttore di un'eccezione EAC
        /// </summary>
        /// <param name="innerException">L'eccezione interna</param>
        /// <param name="Message">Il messaggio di errore</param>
        public EACException(String Message, Exception innerException) : base(Message, innerException) { }
    }


    /// <summary>
    /// La classe ApduException rappresenta un'eccezione lanciata durante l'invio di una Apdu alla smart card
    /// </summary>
    public class ApduException : Exception
    {
        uint lastSCardResult = 0;
        int lastWinError = 0;
        uint sw = 0;

        /// <summary>
        /// Ritorna l'ultima status word restuita dalla smart card
        /// </summary>
        public uint LastSCardResult { get { return lastSCardResult; } }

        /// <summary>
        /// Ritorna l'ultimo codice di errore di windows restituito in fase di trasmissione di una APDU
        /// </summary>
        public int LastWinError { get { return lastWinError; } }

        /// <summary>
        /// Ritorna la Status Word che ha provocato l'eccezione
        /// </summary>
        public uint SW { get { return sw; } }

        /// <summary>
        /// Costruttore una APDUException a partire da una status word e un messaggio di errore
        /// </summary>
        /// <param name="message">Il messaggio di errore</param>
        /// <param name="sw">La Status Word ritornata dalla smart card</param>
        public ApduException(uint sw, string message)
            : base(message + " (sw: " + sw.ToString("X04") + ")")
        {
            this.sw = sw;
        }

        /// <summary>
        /// Costruttore una APDUException a partire da una Status Word, un messaggio di errore e un oggetto Smartcard
        /// da cui verranno ricavati il codice di errore di windows e l'ultima Status Word
        /// </summary>
        /// <param name="message">Il messaggio d'errore</param>
        /// <param name="sc">L'oggetto Smartcard in cui si è verificato l'errore</param>
        /// <param name="sw">La Status Word ritornata dalla smart card</param>
        public ApduException(SmartCard sc, uint sw, string message)
            : base(message + " (sw: " + sw.ToString("X04") + " SC Result: " + sc.LastSCardResult.ToString("X04") + " WinError: " + sc.LastWinError.ToString("X04") + ")")
        {
            this.sw = sw;
            lastSCardResult = sc.LastSCardResult;
            lastWinError = sc.LastWinError;
        }
    }

    /// <summary>
    /// Il TABinding è il binding effettuato in fase di Termoianl Authentication (Static per BAC e Dynamic per PACE)
    /// </summary>
    public enum TABinding
    {
        /// <summary>binding Statico</summary>
        Static,
        /// <summary>binding Dinamico</summary>
        Dynamic 
    }

    /// <summary>
    /// L'enumerazione DG contiene i DG e i file di dati che possono essere letti da un MRTD
    /// </summary>
    public enum DG : int
    {
        /// <summary>DG1 - MRZ</summary>
        DG1 = 1,
        /// <summary>DG2 - Encoded Face</summary>
        DG2 = 2,
        /// <summary>DG3 - Encoded Finger(s)</summary>
        DG3 = 3,
        /// <summary>DG4 - Encoded Eye(s)</summary>
        DG4 = 4,
        /// <summary>DG5 - Displayed Portrait</summary>
        DG5 = 5,
        /// <summary>DG6 - RFU</summary>
        DG6 = 6,
        /// <summary>DG7 - Displayed Signature or Usual Mark</summary>
        DG7 = 7,
        /// <summary>DG8 - Data Feature(s)</summary>
        DG8 = 8,
        /// <summary>DG9 - Structure Feature(s)</summary>
        DG9 = 9,
        /// <summary>DG10 - Substance Feature(s)</summary>
        DG10 = 10,
        /// <summary>DG11 - Additional Personal Detail(s)</summary>
        DG11 = 11,
        /// <summary>DG12 - Additional Document Detail(s)</summary>
        DG12 = 12,
        /// <summary>DG13 - Optional Detail(s)</summary>
        DG13 = 13,
        /// <summary>DG14 - Security Options</summary>
        DG14 = 14,
        /// <summary>DG15 - Active Authentication Public Key Info</summary>
        DG15 = 15,
        /// <summary>DG16 - Person(s) to Notify</summary>
        DG16 = 16,
        /// <summary>EF.SOD - Document Security Object</summary>
        SOD = 29,
        /// <summary>EF.COM - Common information</summary>
        COM = 30,
        /// <summary>EF.CVCA - Name of CVCA</summary>
        CVCA = 28
    }

    /// <summary>
    /// I metodi della classe EAC permettono di effettuare le operazioni di autenticazione e di leggere i data group di un MRTD
    /// </summary>
    public class EAC
    {
        /// <summary>
        /// Delegate chiamata dalla classe EAC per effettuare la firma dell'Inspection System 
        /// </summary>
        /// <param name="toSign">Dati da firmare (challenge e dati statici o dinamici)</param>
        /// <returns>Response firmata trmite la chiave privata dell'IS</returns>
        public delegate byte[] SignDelegate(byte[] toSign);

        /// <summary>
        /// Feedback fornito dalla libreria sulle fasi di avanzamento delle operazioni
        /// </summary>
        public event Action<string> feedBack;

        SmartCard sc=null;

        /// <summary>
        /// Costruttore dell'oggetto EAC. 
        /// </summary>
        /// <param name="sc">Un oggetto smart card connesso ad un MRTD a cui verranno inviati i comandi di autenticazione e lettura</param>
        public EAC(SmartCard sc) { 
            this.sc=sc;
            DGlist = new List<byte>();
        }

        byte checkdigit(ByteArray data)
        {
            int i;
            int tot = 0;
            int curval = 0;
            int[] weight = new int[] { 7, 3, 1 };
            for (i = 0; i < data.Size; i++)
            {
                char ch = Char.ToUpper((char)data[i]);
                if (ch >= 'A' && ch <= 'Z')
                    curval = (int)(ch - 'A' + 10);
                else
                {
                    if (ch >= '0' && ch <= '9')
                        curval = (int)(ch - '0');
                    else
                    {
                        if (ch == '<')
                            curval = 0;
                        else
                            throw new Exception("errore nel calcolo della check digit");
                    }
                }
                tot += curval * weight[i % 3];
            }
            tot = tot % 10;
            return (byte)('0' + tot);
        }

        bool isAuth { get { return KSessEnc != null; } }
        ByteArray KSessEnc,KSessMac;
        ByteArray seq;

        /// <summary>
        /// Dictionary contenente i DG letti dal chip
        /// </summary>
        public Dictionary<int, ByteArray> DG { get { return dg; } }
        Dictionary<int,ByteArray> dg = new Dictionary<int,ByteArray>();
        internal ByteArray efCVCA { get; set; }
        internal ByteArray efSOD { get; set; }
        internal ByteArray efCOM { get; set; }
        internal List<byte> DGlist { get; set; }

        DES des = new DES();
        MAC mac = new MAC();
        SHA1 sha = new SHA1();
        SHA256 sha256 = new SHA256();
        RSA rsa = new RSA();

        Dictionary<string, PACEAlgo> PACEAlgo;
        string DH_GM_DES_Oid = "BAB/AAcCAgQBAQ==";

        /// <summary>
        /// Verifica se l'MRTD implementa il protocollo SAC. Prova a selezionare il CardAccess e verifica che questo contenga l'OID
        /// di un algoritmo supportato
        /// </summary>
        /// <returns>true se è possibile affettuare i lprotocollo PACE sull'MRTD, false altrimenti</returns>
        public bool IsSAC() {
            try
            {
                if (feedBack != null)
                    feedBack("Verifica protezione SAC/BAC");

                byte[] CardAccess = leggiCardAccess();
                // il cardAccess contiene gli algoritmi PACE supportati
                var caTag = ASN1Tag.Parse(CardAccess, false);
                caTag.CheckTag(0x31);
                PACEAlgo = new Dictionary<string, PACEAlgo>();
                foreach (var t in caTag.children)
                    PACEAlgo[Convert.ToBase64String(t.CheckTag(0x30).Child(0, 6).Data)] = new PACEAlgo(t);

                if (!PACEAlgo.ContainsKey(DH_GM_DES_Oid))
                    return false;


                Trace.TraceInformation("CardAccess letto correttamente; il chip è SAC");
                return true;
            }
            catch (Exception ex) {
                Trace.TraceInformation("Errore nella lettura del CardAccess (" + ex.Message + "); il chip non è SAC");
            }
            return false;
        }

        byte[] dynamicBindingData;

        /// <summary>
        /// Effettua il protocollo di autenticazione PACE tramite CAN
        /// </summary>
        /// <param name="CAN">Le 6 cifre del CAN</param>
        public void PACE(string CAN)
        {
            PACE(ASCIIEncoding.ASCII.GetBytes(CAN), PACEMode.CAN);
        }

        /// <summary>
        /// Effettua il protocollo di autenticazione PACE tramite MRZ. Tutti i parametri devono essere passati senza checkDigit
        /// </summary>
        /// <param name="MRZbirthDate">La data di nascita così come riportata nell'MRZ</param>
        /// <param name="expDate">La data di scadenza in formato DateTime</param>
        /// <param name="passNumber">Il numero di serie dell'MRTD</param>
        public void PACE(string MRZbirthDate, DateTime expDate, String passNumber)
        {
                // la password che si ottiene dall'MRZ è K=SHA1(Serial Number || Date of Birth || Date of Expiry), tutti con la propria check digit
            ByteArray PN = ByteArray.FromASCII(passNumber);
            ByteArray Birth = ByteArray.FromASCII(MRZbirthDate);
            ByteArray Expire = ByteArray.FromASCII(expDate.ToString("yyMMdd"));
            ByteArray PasswordSeedData = PN.Append(checkdigit(PN)).
                Append(Birth.Append(checkdigit(Birth))).
                Append(Expire.Append(checkdigit(Expire)));

            PACE(sha.Digest(PasswordSeedData), PACEMode.MRZ);
        }

        internal enum PACEMode : byte
        {
            MRZ = 1,
            CAN = 2
        }
        void PACE(ByteArray PasswordSeedData,PACEMode mode) {
            try
            {
                if (feedBack != null)
                    feedBack("Esecuzione autenticazione PACE");

                byte[] resp = null;
                uint sw = 0;
                // tag 80: oid dell'algoritmo PACE
                // tag 83: tipo di autenticazione, 1=MRZ
                var algo = PACEAlgo[DH_GM_DES_Oid];
                ByteArray MSEData = new ByteArray(algo.DG14Tag.Child(0, 6).Data).ASN1Tag(0x80).Append(
                    new ByteArray((byte)mode).ASN1Tag(0x83));

                sw = sc.Transmit(new Apdu(0x00,0x22,0xc1,0xa4,MSEData));
                if (sw != 0x9000)
                    throw new Exception("Errore nel protocollo PACE:MSE Set AT - " + sw.ToString());

                // i comandi general authenticate sono in chaining! La classe è 0x10!

                // il primo GA serve a chiedere il nonce cifrato. Non devo inviare nulla
                ByteArray GAData1 = new ByteArray(new byte[] {}).ASN1Tag(0x7c);
                sw = sc.Transmit(new Apdu(0x10, 0x86, 0x00, 0x00, GAData1, 0x00),ref resp);
                if (sw != 0x9000)
                    throw new Exception("Errore nel protocollo PACE:General Authenticate 1 - " + sw.ToString());

                SHA1 sha=new SHA1();
                var encryptedNonce = ASN1Tag.Parse(resp,false).CheckTag(0x7c).Child(0, 0x80).Data;

                // la chiave per decifrare il nonce è SHA1(K||00000003);
                var keyNonce = sha.Digest(PasswordSeedData.Append("00 00 00 03")).Left(16);

                var nonce = des.DES3Dec(keyNonce, encryptedNonce);
                var key1 = algo.GenerateEphimeralKey1();

                // il secondo GA serve a inviare la chiave pubblica effimera
                ByteArray GAData2 = new ByteArray(key1.Public.ASN1Tag(0x81)).ASN1Tag(0x7c);
                sw = sc.Transmit(new Apdu(0x10, 0x86, 0x00, 0x00, GAData2, 0x00), ref resp);
                if (sw != 0x9000)
                    throw new Exception("Errore nel protocollo PACE:General Authenticate 2 - " + sw.ToString());

                var otherPubKey1 = ASN1Tag.Parse(resp, false).CheckTag(0x7c).Child(0, 0x82).Data;

                var secret1 = algo.GetSharedSecret1(otherPubKey1);
                algo.DoMapping(secret1, nonce);
                var key2 = algo.GenerateEphimeralKey2();

                // il terzo GA serve a inviare la chiave pubblica effimera nei nuovi parametri di dominio
                ByteArray GAData3 = new ByteArray(key2.Public.ASN1Tag(0x83)).ASN1Tag(0x7c);
                sw = sc.Transmit(new Apdu(0x10, 0x86, 0x00, 0x00, GAData3, 0x00), ref resp);
                if (sw != 0x9000)
                    throw new Exception("Errore nel protocollo PACE:General Authenticate 3 - " + sw.ToString());

                var otherPubKey2 = ASN1Tag.Parse(resp, false).CheckTag(0x7c).Child(0, 0x84).Data;
                dynamicBindingData = otherPubKey2;
                var secret2 = new ByteArray(algo.GetSharedSecret2(otherPubKey2));

                KSessMac = sha.Digest(secret2.Append("00 00 00 02")).Left(16);
                KSessEnc = sha.Digest(secret2.Append("00 00 00 01")).Left(16);

                var oidTag=algo.DG14Tag.Child(0);
                var authData = new ByteArray(oidTag.Data).ASN1Tag(oidTag.tagRawNumber).Append(new ByteArray(otherPubKey2).ASN1Tag(0x84)).ASN1Tag(0x7F49);
                var authToken = mac.MAC3(KSessMac, ByteArray.ISOPad(authData));

                // il quarto e ultimo GA serve a inviare l'authentication token
                // fine del command chaining
                ByteArray GAData4 = new ByteArray(authToken.ASN1Tag(0x85)).ASN1Tag(0x7c);
                sw = sc.Transmit(new Apdu(0x00, 0x86, 0x00, 0x00, GAData4, 0x00), ref resp);
                if (sw != 0x9000)
                    throw new Exception("Errore nel protocollo PACE:General Authenticate 4 - " + sw.ToString());

                var otherAuthData = new ByteArray(oidTag.Data).ASN1Tag(oidTag.tagRawNumber).Append(new ByteArray(key2.Public).ASN1Tag(0x84)).ASN1Tag(0x7F49);
                var otherAuthToken = ASN1Tag.Parse(resp, false).CheckTag(0x7c).Child(0, 0x86).Data;
                var otherAuthTokenCalc = mac.MAC3(KSessMac, ByteArray.ISOPad(otherAuthData));

                if (!otherAuthTokenCalc.IsEqual(otherAuthToken))
                    throw new Exception("Errore nel protocollo PACE:Authentication token non corrispondente");

                seq = new ByteArray("00 00 00 00 00 00 00 00");

                sw = sc.Transmit(SM(KSessEnc, KSessMac, new ByteArray("0c a4 04 0c 07 A0 00 00 02 47 10 01 00"), seq),ref resp);
                if (sw != 0x9000)
                    throw new ApduException(sc, sw, "Errore nella selezione dell'LDF");

                respSM(KSessEnc, KSessMac, resp, seq);
            }
            catch (Exception ex)
            {
                throw new Exception("Errore nella verifica PACE", ex);
            }

        }

        /// <summary>
        /// Effettua il protocollo di autenticazione BAC. Tutti i parametri devono essere passati senza checkDigit
        /// </summary>
        /// <param name="MRZbirthDate">La data di nascita così come riportata nell'MRZ</param>
        /// <param name="expDate">La data di scadenza in formato DateTime</param>
        /// <param name="passNumber">Il numero di serie dell'MRTD</param>
        public void BAC(string MRZbirthDate, DateTime expDate, String passNumber)
        {
            try
            {
                if (feedBack != null)
                    feedBack("Esecuzione Basic Access Control");
                ByteArray PN = ByteArray.FromASCII(passNumber);

                byte[] resp = null;
                uint sw;
                sw = sc.Transmit(new ByteArray("00 a4 04 0c 07 A0 00 00 02 47 10 01"));
                if (sw != 0x9000)
                    throw new ApduException(sc, sw, "Errore nella selezione dell'LDF");
                byte[] challenge = null;
                sw = sc.Transmit(new ByteArray("00 84 00 00 08"), ref challenge);
                if (sw != 0x9000)
                    throw new ApduException(sc, sw, "Errore nella richiesta del challenge");
                ByteArray RNDmrtd = new ByteArray(challenge);

                ByteArray Birth = ByteArray.FromASCII(MRZbirthDate);
                ByteArray Expire = ByteArray.FromASCII(expDate.ToString("yyMMdd"));

                ByteArray BACseedData = PN.Append(checkdigit(PN)).
                    Append(Birth.Append(checkdigit(Birth))).
                    Append(Expire.Append(checkdigit(Expire)));

                ByteArray BACenc = sha.Digest(sha.Digest(BACseedData).Left(16).Append("00 00 00 01")).Left(16);
                ByteArray BACmac = sha.Digest(sha.Digest(BACseedData).Left(16).Append("00 00 00 02")).Left(16);

                ByteArray RNDis1 = new ByteArray();
                ByteArray Kis = new ByteArray();
                RNDis1.Random(8);
                Kis.Random(16);

                ByteArray Eis1 = des.DES3Enc(BACenc, RNDis1.Append(RNDmrtd).Append(Kis));
                ByteArray EisMAC = mac.MAC3(BACmac, ByteArray.ISOPad(Eis1));

                sw = sc.Transmit(new ByteArray("00 82 00 00 28").Append(Eis1).Append(EisMAC).Append(0x28), ref resp);  // mutual authenticate
                if (sw != 0x9000)
                    throw new ApduException(sc,sw, "Errore nella mutua autenticazione BAC");
                ByteArray responseData = new ByteArray(resp);

                ByteArray KisMAC = mac.MAC3(BACmac, ByteArray.ISOPad(responseData.Left(32)));
                ByteArray KisMAC2 = responseData.Right(8);
                if (!KisMAC.IsEqual(KisMAC2))
                    throw new Exception("Errore nell'autenticazione dell'MRTD");

                ByteArray decresp = des.DES3Dec(BACenc, responseData.Left(32));
                ByteArray Kmrtd = decresp.Right(16);

                ByteArray Kseed = StringXor(Kis, Kmrtd);

                KSessMac = sha.Digest(Kseed.Append("00 00 00 02")).Left(16);
                KSessEnc = sha.Digest(Kseed.Append("00 00 00 01")).Left(16);

                seq = decresp.Sub(4, 4).Append(decresp.Sub(12, 4));
            }
            catch (Exception ex)
            {
                throw new EACException("Errore nella BAC", ex);
            }
        }

        /// <summary>
        /// Legge un DG dal chip e restituisce il contenuto
        /// </summary>
        /// <param name="dgNum">DG o file dati da leggere</param>
        /// <returns>Il contenuto del file</returns>
        public byte[] ReadDG(DG dgNum)
        {
            if (!isAuth)
                throw new EACException("Impossibile leggere i DG senza effettaure BAC o PACE",null);
            dg[(int)dgNum] = leggiDG((int)dgNum);
            return dg[(int)dgNum];
        }

        /// <summary>
        /// Legge il contenuto di tutti i DG dell'MRTD
        /// </summary>
        public void ReadDGs()
        {
            try
            {
                if (feedBack != null)
                    feedBack("Lettura EF.COM");

                efCOM = leggiDG(30);
                ASN1Tag comTag = ASN1Tag.Parse((byte[])efCOM, false);
                comTag.CheckTag(0x60).Child(0, new byte[] { 0x5f, 0x01 }).Verify(ASCIIEncoding.ASCII.GetBytes("0107"));
                comTag.Child(1, new byte[] { 0x5f, 0x36 }).Verify(ASCIIEncoding.ASCII.GetBytes("040000"));
                byte[] dhList = comTag.Child(2, 0x5c).Data;
                foreach (byte dhNum in dhList)
                {
                    DGlist.Add(dhNum);
                    string desc = null;
                    int dgNum = 0;
                    switch (dhNum)
                    {
                        case 0x61:
                            dgNum = 1; break;
                        case 0x75:
                            dgNum = 2; break;
                        case 0x63:
                            dgNum = 0; break;
                        case 0x76:
                            dgNum = 4; break;
                        case 0x65:
                            dgNum = 5; break;
                        case 0x66:
                            dgNum = 6; break;
                        case 0x67:
                            dgNum = 7; break;
                        case 0x68:
                            dgNum = 8; break;
                        case 0x69:
                            dgNum = 9; break;
                        case 0x6a:
                            dgNum = 10; break;
                        case 0x6b:
                            dgNum = 11; break;
                        case 0x6c:
                            dgNum = 12; break;
                        case 0x6d:
                            dgNum = 13; break;
                        case 0x6E:
                            dgNum = 14; break;
                        case 0x6F:
                            dgNum = 15; break;
                        case 0x70:
                            dgNum = 16; break;
                        case 0x77:
                            dgNum = 29; desc = "EF.SOD"; break;
                    }
                    if (dgNum != 0)
                    {
                        if (feedBack != null)
                        {
                            if (desc == null)
                                feedBack("Lettura DG" + dgNum);
                            else
                                feedBack("Lettura " + desc);
                        }
                        dg[dgNum] = leggiDG(dgNum);
                    }
                }
                if (!DGlist.Contains(0x63))
                    throw new Exception("Il PSE non continee il DG3!");

                if (dg.ContainsKey(29))
                    efSOD = dg[29];
                else
                {
                    if (feedBack != null)
                        feedBack("Lettura EF.SOD");

                    efSOD = leggiDG(29);
                }
                if (dg.ContainsKey(28))
                    efCVCA = dg[28];
                else
                {
                    if (feedBack != null)
                        feedBack("Lettura EF.CVCA");
                    efCVCA = leggiDG(28);
                }
            }
            catch (Exception ex)
            {
                throw new EACException("Errore nella Lettura dei DG", ex);
            }
        }

        /// <summary>
        /// Verifica che il SOD e i DG siano integri e non siano stati alterati. Per verificare che i DG siano integri, 
        /// devono essere stati letti prima di chiamare la funzione.
        /// Se viene passata una lista di CSCA, verifica che il Document Signer sia stato emesso da una di queste.
        /// Non verifica lo stato doi revoca del Document Signer
        /// </summary>
        /// <param name="sod">Il contenuto dell'EF.SOD</param>
        /// <param name="CSCA">Una lista di CSCA per verificare la catena del Document Signer</param>
        /// <param name="validateOnlyReadDG">Se è true vengono solo validati gli hash dei DG che sono stati letti in precedenza. 
        /// Se è false viene ritornata un'eccezione se un DG si trova nel SOD ma non è stato letto</param>
        public void VerificaSOD(byte[] sod, List<X509Certificate2> CSCA, bool validateOnlyReadDG)
        {
            if (feedBack != null)
                feedBack("Verifica EF.SOD");

            ByteArray SOD = new ByteArray(sod);
            ASN1Tag SODTag = ASN1Tag.Parse(sod, false);
            ASN1Tag temp;
            Trace.TraceInformation("Verifica OID contentInfo");
            (temp = SODTag.Child(0, 0x30)).Child(0, 06).Verify(new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x02 });
            (temp = temp.Child(1, 0xA0).Child(0, 0x30)).Child(0,2).Verify(new byte[] { 0x03 });

            temp.Child(1, 0x31).Child(0, 0x30).Child(0, 6).Verify(new byte[] { 0x2B, 0x0E, 0x03, 0x02, 0x1A });
            // i dati dell'algoritmo possono essere null o assenti! salto la verifica
            //temp.Child(1, 0x31).Child(0, 0x30).Child(1, 5).Verify(new byte[] {});
            temp.Child(2, 0x30).Child(0, 06).Verify(new byte[] { 0x67, 0x81, 0x08, 0x01, 0x01, 0x01 });
            var ttData=new ByteArray(temp.Child(2, 0x30).Child(1, 0xA0).Child(0, 04).Data);
            ASN1Tag tt = ASN1Tag.Parse(ttData.Data, false);
            ASN1Tag signedData = tt.CheckTag(0x30);
            ASN1Tag signerCert = temp.Child(3, 0xA0).Child(0, 0x30);
            (temp = temp.Child(4, 0x31).Child(0, 0x30)).Child(0, 02).Verify(new byte[] { 01 });
            ASN1Tag issuerName = temp.Child(1, 0x30).Child(0, 0x30);
            ASN1Tag signerCertSerialNumber = temp.Child(1, 0x30).Child(1, 02);
            temp.Child(2, 0x30).Child(0, 06).Verify(new byte[] { 0x2B, 0x0E, 0x03, 0x02, 0x1A });
            // i dati dell'algoritmo possono essere null o assenti! salto la verifica
            //temp.Child(2, 0x30).Child(1, 05).Verify(new byte[] { });

            Trace.TraceInformation("Verifica signerInfo");
            ASN1Tag signerInfo = temp.Child(3, 0xA0);
            signerInfo.Child(0, 0x30).Child(0, 06).Verify(new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x09, 0x03 });
            signerInfo.Child(0, 0x30).Child(1, 0x31).Child(0, 06).Verify(new byte[] { 0x67, 0x81, 0x08, 0x01, 0x01, 0x01 });
            signerInfo.Child(1, 0x30).Child(0, 06).Verify(new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x09, 0x04 });
            ASN1Tag digest = temp.Child(3, 0xA0).Child(1, 0x30).Child(1, 0x31).Child(0, 04);

            temp.Child(4, 0x30).Child(0, 06).Verify(new byte[] { 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x05 });

            // i dati dell'algoritmo possono essere null o assenti! salto la verifica
            //temp.Child(4, 0x30).Child(1, 05).Verify(new byte[] { });
            ASN1Tag signature = temp.Child(5, 04);

            // ok,ho tutto... adesso devo verificare che la firma corrisponda e il certificato vada bene

            Trace.TraceInformation("Verifica Digest");
            if (!sha.Digest(ttData.Sub((int)signedData.StartPos, (int)(signedData.EndPos - signedData.StartPos))).IsEqual(digest.Data))
                throw new Exception("Digest non corrispondente ai dati");

            Trace.TraceInformation("Verifica certificato firma");
            X509Certificate2 certDS = new X509Certificate2(SOD.Sub((int)signerCert.StartPos, (int)(signerCert.EndPos - signerCert.StartPos)));

            Trace.TraceInformation("Verifica firma");
            var pubKeyData = certDS.PublicKey.EncodedKeyValue.RawData;
            ASN1Tag pubKey = ASN1Tag.Parse(pubKeyData, false);
            var modTag=pubKey.Child(0, 02);
            ByteArray mod;
            if (modTag.children != null)
                mod = ASN1Tag.Parse(new ByteArray(pubKeyData).Sub((int)modTag.StartPos, (int)(modTag.EndPos - modTag.StartPos)).Data, false).Data;
            else
                mod = modTag.Data;
            var expTag = pubKey.Child(1, 02);
            ByteArray exp ;
            if (expTag.children != null)
                exp = ASN1Tag.Parse(new ByteArray(pubKeyData).Sub((int)expTag.StartPos, (int)(expTag.EndPos - expTag.StartPos)).Data, false).Data;
            else
                exp = expTag.Data;
            ByteArray signatureData;
            if (signature.children != null)
                signatureData = ASN1Tag.Parse(SOD.Sub((int)signature.StartPos, (int)(signature.EndPos - signature.StartPos)).Data, false).Data;
            else
                signatureData = signature.Data;
            ByteArray decryptedSignature = rsa.RawRsa(mod, exp, signatureData);
            decryptedSignature = ByteArray.RemoveBT1(decryptedSignature);
            decryptedSignature = rsa.RemoveSha1(decryptedSignature);
            ByteArray toSign = SOD.Sub((int)signerInfo.children[0].StartPos, (int)(signerInfo.children[signerInfo.children.Count - 1].EndPos - signerInfo.children[0].StartPos));
            ByteArray digestSignature=sha.Digest(toSign.ASN1Tag(0x31));
            if (!digestSignature.IsEqual(decryptedSignature))
                throw new Exception("Firma del SOD non valida");

            // il confronto esatto sull'issuer richiede l'uso dell'algoritmo descritto in RFC 4518 basato su StringPrep.
            // è eccessivo, dato che conosco già il certificato dell'isser, quindi mi limito a un confronto più semplice,
            // verificado che le componenti del nome e la sequenza coincidano
            Trace.TraceInformation("Verifica issuer");
            var SODIssuer = ASN1Tag.Parse(SOD.Sub((int)issuerName.StartPos, (int)(issuerName.EndPos - issuerName.StartPos)).Data, false);
            var CertIssuer = ASN1Tag.Parse(certDS.IssuerName.RawData, false);
            if (SODIssuer.children.Count != CertIssuer.children.Count)
                throw new Exception("Issuer name non corrispondente");
            for (int i = 0; i < SODIssuer.children.Count; i++) {
                var certElem = CertIssuer.children[i].children[0];
                var SODElem = SODIssuer.children[i].children[0];
                certElem.children[0].Verify(SODElem.children[0].Data);
                certElem.children[1].Verify(SODElem.children[1].Data);
            }

            if (!new ByteArray(certDS.SerialNumber).IsEqual(signerCertSerialNumber.Data))
                throw new Exception("Serial Number del certificato non corrispondente");

            // ora verifico gli hash dei DG
            Trace.TraceInformation("Verifica hash DG");
            signedData.Child(0, 02).Verify(new byte[] { 00 });
            signedData.Child(1, 0x30).Child(0, 06).Verify(new byte[] { 0x2B, 0x0E, 0x03, 0x02, 0x1A });
            // i dati dell'algoritmo possono essere null o assenti! salto la verifica
            //signedData.Child(1, 0x30).Child(1, 05).Verify(new byte[] { });
            ASN1Tag hashTag = signedData.Child(2, 0x30);
            foreach (ASN1Tag hashDG in hashTag.children) {
                ASN1Tag dgNum = hashDG.CheckTag(0x30).Child(0, 02);
                ASN1Tag dgHash = hashDG.Child(1, 04);
                int num = (int)new ByteArray(dgNum.Data).ToUInt;
                if (!dg.ContainsKey(num) || dg[num] == null)
                    if (validateOnlyReadDG)
                        continue;
                    else
                        throw new Exception("DG" + num.ToString() + " non letto");
                var hashVal = ASN1Tag.Parse(ttData.Sub((int)dgHash.StartPos, (int)(dgHash.EndPos - dgHash.StartPos)).Data, false);
                if (!sha.Digest(dg[num]).IsEqual(hashVal.Data))
                    throw new Exception("Digest non corrispondente per il DG" + num.ToString());
            }

            if (CSCA != null && CSCA.Count > 0)
            {
                Trace.TraceInformation("Verifica catena CSCA");
                X509CertChain chain = new X509CertChain(CSCA);
                var certChain=chain.getPath(certDS);
                if (certChain == null)
                    throw new Exception("Il certificato di Document Signer non è valido");

                var rootCert = certChain[0];
                if (!new ByteArray(rootCert.SubjectName.RawData).IsEqual(rootCert.IssuerName.RawData))
                    throw new Exception("Impossibile validare il certificato di Document Signer");

                //X509Chain chain = new X509Chain(false);
                //foreach (var cert in CSCA)
                //    chain.ChainPolicy.ExtraStore.Add(cert);
                //chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                //chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                //if (!chain.Build(certDS))
                //    throw new Exception("Il certificato di Document Signer non è valido");
                //var rootCert=chain.ChainElements[chain.ChainElements.Count-1];
                //if (!new ByteArray(rootCert.Certificate.SubjectName.RawData).IsEqual(rootCert.Certificate.IssuerName.RawData))
                //    throw new Exception("Impossibile validare il certificato di Document Signer");
                //// il certificato di root deve essere uno di quelli inviati fra le CSCA valide
                //if (!CSCA.Contains(rootCert.Certificate))
                //    throw new Exception("Il certificato di CSCA non è affidabile");
            }

        }

        /// <summary>
        /// Effettua il protocollo di Terminal Authentication. Viene ricostruita la catena di certificati dalla CVCA presente sull'MRTD
        /// fino al certificato di inspection system contenuto in IS usando i certificati contenuti nella lista chain.
        /// I dati da firmare vengono elaborati tramite il binding specificato e la firma viene effettuata tramite il delegate signFunc.
        /// </summary>
        /// <param name="chain">La catena di certificati (anche non ordinata) dalla CVCA all'IS</param>
        /// <param name="IS">Il certificato di Inspection System</param>
        /// <param name="signFunc">La funzione usata per firmare il challenge</param>
        /// <param name="binding">Il binding utilizzato</param>
        public void TerminalAuthentication(List<CVCert> chain,CVCert IS,SignDelegate signFunc,TABinding binding)
        {
            try
            {
                if (feedBack != null)
                    feedBack("Esecuzione Terminal Authentication");

                // leggo il file CVCA
                var AsnCVCAName = leggiDG(0x1c);
                var CVCAName = ASCIIEncoding.ASCII.GetString(ASN1Tag.Parse(AsnCVCAName.Data, false).Data);

                chain.Add(IS);
                CertChain buildChain = new CertChain(chain);
                Trace.TraceInformation("Ricostruzione catena di certificati per " + CVCAName);
                List<CVCert> certChain = buildChain.getPath(CVCAName, IS.Name);

                if (certChain == null)
                    throw new Exception("Impossibile ricostruire la catena di certificati");

                CVCert curCert = null;
                byte[] resp = null;
                uint sw;
                for (int i = 0; i < certChain.Count; i++)
                {
                    curCert = certChain[i];

                    Trace.TraceInformation("Presentazione certificato " + curCert.Name);

                    ByteArray certIssuer = ByteArray.FromASCII(curCert.Issuer);
                    sw = sc.Transmit(SM(KSessEnc, KSessMac, new ByteArray("0c 22 81 b6 ").Append((byte)(certIssuer.Size + 2)).Append(0x83).Append((byte)certIssuer.Size).Append(certIssuer), seq), ref resp);// ' mse set DST
                    if (sw != 0x9000)
                        throw new ApduException(sc, sw, "Errore nell'impostazione del certificato " + curCert.Issuer);
                    respSM(KSessEnc, KSessMac, resp, seq);

                    sw = sc.Transmit(LongSM(KSessEnc, KSessMac, new ByteArray("0c 2A 00 BE"), ASN1Tag.Parse(curCert.RawCert.Data, false).Data, seq), ref resp);// ' verify cert
                    if (sw != 0x9000)
                        throw new ApduException(sc, sw, "Errore nella verifica del certificato " + curCert.Name);
                    respSM(KSessEnc, KSessMac, resp, seq);
                }

                ByteArray ISName = ByteArray.FromASCII(curCert.Name);
                sw = sc.Transmit(SM(KSessEnc, KSessMac, new ByteArray("0c 22 81 a4 ").Append((byte)(ISName.Size + 2)).Append(0x83).Append((byte)ISName.Size).Append(ISName), seq), ref resp);// ' mse set AT
                if (sw != 0x9000)
                    throw new ApduException(sc, sw, "Errore nell'impostazione del certificato " + curCert.Name);
                respSM(KSessEnc, KSessMac, resp, seq);

                sw = sc.Transmit(SM(KSessEnc, KSessMac, new ByteArray("0C 84 00 00 08"), seq), ref resp);// ' get challenge
                if (sw != 0x9000)
                    throw new ApduException(sc, sw, "Errore nella richiesta del challenge");
                ByteArray TAchallenge = respSM(KSessEnc, KSessMac, resp, seq);

                ByteArray TAcalcResp = null;
                if (binding == TABinding.Static)
                {
                    ASN1Tag MRZasn = ASN1Tag.Parse(dg[1].Data, false); //bindingData = DG1
                    ByteArray MRZ = MRZasn.CheckTag(0x61).Child(0, new byte[] { 0x5f, 0x1f }).Data;

                    if (MRZ.Size != 90)
                        TAcalcResp = MRZ.Sub(44, 10);
                    else
                        TAcalcResp = MRZ.Sub(5, 10);
                }
                else
                    TAcalcResp = sha.Digest(dynamicBindingData); // bindingData = PACE pubKey               

                TAcalcResp = TAcalcResp.Append(TAchallenge).Append(sha.Digest(publicKey));

                Trace.TraceInformation("Firma del challenge");
                ByteArray TAResp = signFunc(TAcalcResp);

                if (TAResp.Size > 128)
                    sw = sc.Transmit(LongSM(KSessEnc, KSessMac, new ByteArray("0c 82 00 00 "), TAResp, seq), ref resp);// ' EXTERNAL AUTHENTICATE
                else
                    sw = sc.Transmit(SM(KSessEnc, KSessMac, new ByteArray("0c 82 00 00 ").Append((byte)TAResp.Size).Append(TAResp).Append(00), seq), ref resp);// ' EXTERNAL AUTHENTICATE
                if (sw != 0x9000)
                    throw new ApduException(sc, sw, "Errore nell'external authentication");
                respSM(KSessEnc, KSessMac, resp, seq);
                if (feedBack != null)
                    feedBack("Lettura DG3");
                if (DGlist.Contains(0x63))
                    dg[3] = leggiDG(3);
            }
            catch (Exception ex)
            {
                throw new EACException("Errore nella Terminal Authentication", ex);
            }
        }

        ASN1Tag searchInSet(ASN1Tag setTag, ByteArray OID) {
            foreach (var v in setTag.children) {
                try {
                if (v.tagConstructed) {
                    if (OID.IsEqual(v.Child(0, 0x06).Data))
                        return v;
                    }
                }
                catch {}
            }
            return null;
        }

        ByteArray privateKey = null, publicKey = null;

        /// <summary>
        /// Effettua il protocollo di Chip Authentication usando i parametri di dominio e la chiave pubblica del chip contenuta
        /// nel DG14. Il DG14 deve essere letto prima di chiamare questa funzione
        /// </summary>
        public void ChipAuthentication()
        {
            try
            {
                if (feedBack != null)
                    feedBack("Esecuzione Chip Authentication");
                if (!dg.ContainsKey(14))
                    throw new EACException("E' necessario leggere il DG14 prima di effettuare la Chip Authentication", null);
                ASN1Tag root = ASN1Tag.Parse((byte[])dg[14], false);
                var DHset = root.Child(0, 0x31);
                var DHattr1 = searchInSet(DHset, new ByteArray("04 00 7F 00 07 02 02 02"));
                var DHattr2 = searchInSet(DHset, new ByteArray("04 00 7F 00 07 02 02 03 01 01"));
                var DHparams = searchInSet(DHset, new ByteArray("04 00 7F 00 07 02 02 01 01"));
                DHattr1.Child(1, 2).Verify(new byte[] { 1 });
                DHattr2.Child(1, 2).Verify(new byte[] { 1 });
                ByteArray prime = new ByteArray(DHparams.Child(1, 0x30).Child(0, 0x30).Child(1, 0x30).Child(0, 02).Data);
                ByteArray group = new ByteArray(DHparams.Child(1, 0x30).Child(0, 0x30).Child(1, 0x30).Child(1, 02).Data);
                ByteArray ttData = new ByteArray(DHparams.Child(1, 0x30).Child(1, 03).Data);
                ASN1Tag tt=ASN1Tag.Parse(ttData.Sub(1).Data,false);
                ByteArray OtherPub=tt.CheckTag(02).Data;
                DiffieHellmann.GenerateKey(group, prime, ref privateKey, ref publicKey);
                ByteArray sharedKey = DiffieHellmann.ComputeKey(group, prime, privateKey, OtherPub);

                byte[] resp = null;
                ByteArray dhs = publicKey.ASN1Tag(0x91);
                uint sw=0;
                if (dhs.Size > 255)
                    sw = sc.Transmit(LongSM(KSessEnc, KSessMac, new ByteArray("0c 22 41 a6"),dhs, seq), ref resp); //  ' mse set
                else
                    sw = sc.Transmit(SM(KSessEnc, KSessMac, new ByteArray("0c 22 41 a6").Append((byte)dhs.Size).Append(dhs), seq), ref resp); //  ' mse set
                if (sw != 0x9000)
                    throw new ApduException(sc, sw, "Errore nello scambio di chiavi Diffie Hellman");
                respSM(KSessEnc, KSessMac, resp, seq);

                KSessMac = sha.Digest(sharedKey.Append("00 00 00 02")).Left(16);
                KSessEnc = sha.Digest(sharedKey.Append("00 00 00 01")).Left(16);

                seq = new ByteArray("00 00 00 00 00 00 00 00");
            }
            catch (Exception ex)
            {
                throw new EACException("Errore nella Chip Authentication", ex);
            }
        }


        ByteArray leggiCardAccess()
        {
            ByteArray data = new ByteArray();
            try
            {
                byte[] resp = null;
                uint sw;
                // leggo con SFI per selezionare l'EF corrente
                sw = sc.Transmit(new ByteArray("00 A4 02 0C 02 01 1C"), ref resp);
                //if (sw != 0x9000) throw new ApduException(sc, sw, "Errore nella selezione del CardAccess");

                byte[] chunkLen = null;
                sw = sc.Transmit(Apdu.ReadBinary(0, 6), ref chunkLen);// ' read DG
                if (sw != 0x9000) throw new ApduException(sc, sw, "Errore nella lettura del CardAccess");

                int maxLen = ASN1Tag.ParseLength(chunkLen);
                while (data.Size < maxLen)
                {
                    int readLen = Math.Min(200, maxLen - data.Size);
                    byte[] chunk =null;
                    sw = sc.Transmit(Apdu.ReadBinary(data.Size, (byte)readLen), ref chunk);// ' read DG
                    if (sw != 0x9000)
                        throw new ApduException(sc, sw, "Errore nella lettura del CardAccess");

                    data = data.Append(chunk);
                }
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception("Errore nella lettura del CardAccess", ex);
            }
        }

        ByteArray leggiDG(int numDG)
        {
            ByteArray data = new ByteArray();
            try
            {
                byte[] resp = null;
                uint sw;
                // leggo con SFI per selezionare l'EF corrente
                sw = sc.Transmit(SM(KSessEnc, KSessMac, new ByteArray("0c b0 " + (numDG + 0x80).ToString("X2") + " 00 06"), seq), ref resp);// ' read DG

                if (sw != 0x9000)
                    throw new ApduException(sc, sw, "Errore nella selezione del DG" + numDG.ToString());
                byte[] chunkLen=null;
                chunkLen = respSM(KSessEnc, KSessMac, new ByteArray(resp), seq);

                int maxLen=ASN1Tag.ParseLength(chunkLen);
                while (data.Size < maxLen)
                {
                    int readLen = Math.Min(0xe0, maxLen - data.Size);
                    sw = sc.Transmit(SM(KSessEnc, KSessMac, new ByteArray("0c b0 ").Append((byte)(((byte)(data.Size / 256)) & (byte)0x7f)).Append((byte)(data.Size & 0xff)).Append((byte)readLen), seq), ref resp);// ' read DG

                    if (sw != 0x9000)
                        throw new ApduException(sc, sw, "Errore nella lettura del DG" + numDG.ToString());

                    byte[] chunk = null;
                    chunk = respSM(KSessEnc, KSessMac, new ByteArray(resp), seq);

                    data = data.Append(chunk);
                }
                return data;
            }
            catch (Exception ex) {
                throw new Exception("Errore nella lettura del DG" + numDG, ex);
            }
        }
        void increment(ByteArray val)
        {
            for (int i = val.Size - 1; i >= 0; i--)
            {
                if (val[i] < 0xff)
                {
                    val[i]++;
                    for (int j = i + 1; j < val.Size; j++)
                        val[j] = 0;
                    return;
                }
            }
        }

        ByteArray StringXor(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length)
                throw new Exception("Le due stringhe hanno lunghezza diversa!");
            byte[] data = new byte[b1.Length];
            for (int i = 0; i < b1.Length; i++)
                data[i] = (byte)(b1[i] ^ b2[i]);
            return new ByteArray(data);
        }
        ByteArray respSM(ByteArray keyEnc, ByteArray keySig, ByteArray resp, ByteArray seq) {
            return respSM(keyEnc, keySig, resp, seq, false);
        }

        ByteArray respSM(ByteArray keyEnc, ByteArray keySig, ByteArray resp, ByteArray seq,bool odd)
        {
            try
            {
                increment(seq);
                // cerco il tag 87
                int index = 0;
                ByteArray encData = null;
                ByteArray encObj = null;
                ByteArray DataObj = null;
                do
                {

                    if (resp[index] == 0x99)
                    {
                        if (resp[index + 1] != 0x02)
                            throw new Exception("Errore nella verifica del SM - lunghezza del DataObject");
                        DataObj = resp.Sub(index, 4);
                        index += 4;
                        continue;
                    }
                    if (resp[index] == 0x8e)
                    {
                        ByteArray calcMac = mac.MAC3(keySig, ByteArray.ISOPad(seq.Append(encObj).Append(DataObj)));
                        index++;
                        if (resp[index] != 0x08)
                            throw new Exception("Errore nella verifica del SM - lunghezza del MAC errata");
                        index++;
                        if (!calcMac.IsEqual(resp.Sub(index, 8)))
                            throw new Exception("Errore nella verifica del SM - MAC non corrispondente");
                        index += 8;
                        continue;
                    }
                    if (resp[index] == 0x87)
                    {
                        if (resp[index + 1] > 0x80)
                        {

                            int lgn = 0;
                            int llen = resp[index + 1] - 0x80;
                            if (llen == 1)
                                lgn = resp[index + 2];
                            if (llen == 2)
                                lgn = (resp[index + 2] << 8) | resp[index + 3];
                            encObj = resp.Sub(index, llen + lgn + 2);
                            encData = resp.Sub(index + llen + 3, lgn - 1); // ' levo il padding indicator
                            index += llen + lgn + 2;
                        }
                        else
                        {
                            encObj = resp.Sub(index, resp[index + 1] + 2);
                            encData = resp.Sub(index + 3, resp[index + 1] - 1); // ' levo il padding indicator
                            index += resp[index + 1] + 2;
                        }
                        continue;
                    }

                    else
                        if (resp[index] == 0x85)
                        {
                            if (resp[index + 1] > 0x80)
                            {
                                int lgn = 0;
                                int llen = resp[index + 1] - 0x80;
                                if (llen == 1)
                                    lgn = resp[index + 2];
                                if (llen == 2)
                                    lgn = (resp[index + 2] << 8) | resp[index + 3];
                                encObj = resp.Sub(index, llen + lgn + 2);
                                encData = resp.Sub(index + llen + 2, lgn); // ' levo il padding indicator
                                index += llen + lgn + 2;
                            }
                            else
                            {
                                encObj = resp.Sub(index, resp[index + 1] + 2);
                                encData = resp.Sub(index + 2, resp[index + 1]);
                                index += resp[index + 1] + 2;

                            }
                            continue;
                        }
                        else
                            throw new Exception("Tag non previsto nella risposta in SM");
                    //index = index + resp[index + 1] + 1;
                }
                while (index < resp.Size);
                if (encData != null)
                {
                    if (odd)
                    {
                        var SMresp = ByteArray.ISORemove(des.DES3Dec(keyEnc, encData));
                        using (MemoryStream ms = new MemoryStream(SMresp))
                        {
                            var tag = ASN1Tag.Parse(ms, false);
                            return tag.Data;
                        }
                    }
                    else
                    {
                        var rsp=ByteArray.ISORemove(des.DES3Dec(keyEnc, encData));
                        return rsp;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Errore nella decodifica della risposta in SM", ex);
            }
        }
        ByteArray LongSM(byte[] keyEnc, byte[] keyMac, ByteArray Apdu, ByteArray data, ByteArray sequence) {
            return LongSM(keyEnc, keyMac, Apdu, data, null, sequence);
        }

        ByteArray LongSM(byte[] keyEnc, byte[] keyMac, ByteArray Apdu, ByteArray data, ByteArray le, ByteArray sequence)
        {
            try
            {
                increment(sequence);
                ByteArray calcMac = ByteArray.ISOPad(sequence.Append(Apdu.Left(4)));
                ByteArray datafield = null, doob = null;
                if (data != null && data.Size > 0)
                {
                    ByteArray enc = des.DES3Enc(keyEnc, ByteArray.ISOPad(data));
                    if ((Apdu[1] % 2) == 0)
                        doob = new ByteArray(1).Append(enc).ASN1Tag(0x87);
                    //doob = new ByteArray("87 82 ").Append(ByteArray.PadInt((ulong)enc.Size + 1, 2)).Append(01).Append(enc);
                    else
                        doob = enc.ASN1Tag(0x85);
                    //doob = new ByteArray("85 82 ").Append(ByteArray.PadInt((ulong)enc.Size + 1, 2)).Append(enc);
                    calcMac = calcMac.Append(doob);
                    datafield = doob.Clone() as ByteArray;
                }
                else
                    datafield = new ByteArray();
                if (le != null)
                {
                    doob = new ByteArray(new byte[] { 0x97, (byte)le.Size}).Append(le);
                    calcMac = calcMac.Append(doob);
                    datafield = datafield.Append(doob);
                }

                ByteArray smMac = mac.MAC3(keyMac, ByteArray.ISOPad(calcMac));
                datafield = datafield.Append(new byte[] { 0x8e, 0x08 }).Append(smMac);
                return Apdu.Left(4).Append(ByteArray.PadInt((ulong)datafield.Size, 3)).Append(datafield).Append(new byte[] { 0, 0 });
            }
            catch (Exception ex) {
                throw new Exception("Errore nella codifica in Long Secure Messaging", ex);
            }
        }

        ByteArray SM(byte[] keyEnc, byte[] keyMac, ByteArray Apdu, ByteArray sequence)
        {
            try
            {
                increment(sequence);
                ByteArray calcMac = ByteArray.ISOPad(sequence.Append(Apdu.Left(4)));
                ByteArray smMac;

                ByteArray datafield = new ByteArray();
                ByteArray doob;
                if (Apdu[4] != 0 && Apdu.Size > 5)
                {//' se c'è una parte dati dell'adpu
                    ByteArray enc = des.DES3Enc(keyEnc, ByteArray.ISOPad(Apdu.Sub(5, Apdu[4])));
					//' DA MODIFICARE!! IL TAG 0x87 e 0x85 possono essere più lunghi di 127 bytes, quindi bisogna mettere la dimensione in formato ASN1!!!
                    if (Apdu[1] % 2 == 0)
                    { //'INS even
                        doob = new ByteArray(1).Append(enc).ASN1Tag(0x87);
                        //doob = new ByteArray(new byte[] { 0x87, (byte)(enc.Size + 1), 1 }).Append(enc);
                    }
                    else
                    { //' INS odd
                        doob = enc.ASN1Tag(0x85);
                        //doob = new ByteArray(new byte[] { 0x85, (byte)(enc.Size) }).Append(enc);
                    }
                    calcMac = calcMac.Append(doob);
                    datafield = datafield.Append(doob);
                }
                if (Apdu.Size == 5 || Apdu.Size == Apdu[4] + 6)
                { // ' se c'è un le
                    doob = new ByteArray(new byte[] { 0x97, 0x01, Apdu[Apdu.Size - 1] });
                    calcMac = calcMac.Append(doob);
                    datafield = datafield.Append(doob);
                }

                smMac = mac.MAC3(keyMac, ByteArray.ISOPad(calcMac));
                datafield = datafield.Append(new byte[] { 0x8e, 0x08 }).Append(smMac);

                return Apdu.Left(4).Append((byte)datafield.Size).Append(datafield).Append(00);
            }
            catch (Exception ex)
            {
                throw new Exception("Errore nella codifica in Secure Messaging", ex);
            }
        }
    }
}
