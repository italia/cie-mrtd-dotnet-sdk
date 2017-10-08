using System;
using System.Collections.Generic;
using CIE.MRTD.SDK.EAC;
using CSJ2K.Util;
using CSJ2K;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;
using System.Web.Script.Serialization;

namespace CIE.MRTD.SDK.PARSERLIB
{

    /* Classe di utilità per manipolazione di array di byte*/
    static class ArrayUtils
    {
        //Esegue lo slice di un array
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        //array vuoto
        public static readonly int[] Empty = new int[0];

        //Localizza le occorenze di "candidate" in "self"
        public static int[] Locate(this byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return Empty;

            var list = new List<int>();

            for (int i = 0; i < self.Length; i++)
            {
                if (!IsMatch(self, i, candidate))
                    continue;

                list.Add(i);
            }

            return list.Count == 0 ? Empty : list.ToArray();
        }

        /* Verifica il mach tra array e candidate nella posizione array[position] */
        static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }

        /* Controllo, se è vero allora sicuramente non ci sono occorrenze */
        static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                || candidate == null
                || array.Length == 0
                || candidate.Length == 0
                || candidate.Length > array.Length;
        }
    }

    
    /*Classe che astrae le informazioni di una CIE, ha i metodi per la decodifica dell'icao e gli attributi delle
     informazioni presenti sulla carta, sono riportate le sole informazioni presenti sulla card di prova*/
    public class C_CIE
    {

        /* Le chiavi del tag icao */
        public static readonly byte[] KEY_FULL_NAME = new byte[]{ 0x5F, 0x0E };
        public static readonly byte[] KEY_BIRTH_ADDRESS = new byte[] { 0x5F, 0x11 };
        public static readonly byte[] KEY_ADDRESS = new byte[] { 0x5F, 0x42 };
        public static readonly byte[] KEY_CF = new byte[] { 0x5F, 0x10 };
        public static readonly byte[] KEY_MRZ = new byte[] { 0x5F, 0x1F };
        public static readonly byte[] KEY_DATE_ISSUE = new byte[] { 0x5F, 0x26 };

        /* questo pattern è l'inizio del file jpeg2000 presente nella card, si cerca questo pattern per poi
         * scartare i primi bytes*/
        public static byte[] jpg2k_magic_number = { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50,
            0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A, 0x00, 0x00, 0x00, 0x14, 0x66, 0x74,
            0x79, 0x70, 0x6A, 0x70, 0x32, 0x20, 0x00, 0x00, 0x00, 0x00, 0x6A, 0x70,
            0x32, 0x20, 0x00, 0x00, 0x00, 0x2D, 0x6A, 0x70, 0x32, 0x68, 0x00, 0x00,
            0x00, 0x16, 0x69, 0x68, 0x64, 0x72 };

        /*Estrae i byte dell'immagine jpeg2000 da un array di bytes usando il suo magic_number*/
        public static Byte[] Image_retrive(Byte[] blob)
        {
            int loc = ArrayUtils.Locate(blob, jpg2k_magic_number)[0];
            return blob.SubArray(loc, blob.Length - loc);

        }

        /* estrae i dati da un dg attraverso il tag */
        static String ICAOGetValueFromKey(byte[] key, byte[] dg)
        {

            int[] index = ArrayUtils.Locate(dg, key);

            if (index == ArrayUtils.Empty)
                return null;

            int i = index.Length == 1 ? index[0] : index[1]; //la prima occorrenza è quella della lista dei tag (eccezione per dg1)

            int sizeOfData = Convert.ToInt32(dg[i + key.Length]);

            return System.Text.Encoding.UTF8.GetString(dg.SubArray(i + key.Length + 1, sizeOfData));
        }

        /* Fa il parsing del FullName dividendolo in nome e cognome */
        static String[] parseFullName(String s)
        {
            return s.Split(new[] { "<<" }, StringSplitOptions.None); ;
        }

        /* Parsing di un indirizzo dividendolo in Via (eventuale), Città e provincia */
        static String[] parseAddress(String s)
        {           

            return s.Split(new[] { "<" }, StringSplitOptions.None); ;
        }

        /* Costruttore, riempie i campi dell'oggetto interrogando l'oggetto EAC */
        public C_CIE(CIE.MRTD.SDK.EAC.EAC eac)
        {
            var dg14 = eac.ReadDG(DG.DG14); //Si assicura che sia stato chiamato il dg14 almeno una volta

            var dg11 = eac.ReadDG(DG.DG11);

            String[] s1 = parseFullName( ICAOGetValueFromKey(KEY_FULL_NAME, dg11) );
            firstName = s1[0];
            lastName = s1[1];

            String[] s2 = parseAddress(ICAOGetValueFromKey(KEY_BIRTH_ADDRESS, dg11));
            birthCity = s2[0];
            birthProv = s2[1];

            String[] s3 = parseAddress(ICAOGetValueFromKey(KEY_ADDRESS, dg11));
            address = s3[0];
            city = s3[1];
            prov = s3[2];

            cf = ICAOGetValueFromKey(KEY_CF, dg11);

            var dg1 = eac.ReadDG(DG.DG1);
            mrz = ICAOGetValueFromKey(KEY_MRZ, dg1);

            var dg12 = eac.ReadDG(DG.DG12);
            dateIssue = ICAOGetValueFromKey(KEY_DATE_ISSUE, dg12);

            var dg2 = eac.ReadDG(DG.DG2);
            cie_jpg2k_image = Image_retrive(dg2);

            


        }

        /*Costruttore vuoto*/
        public C_CIE()
        {

        }

        /*  VARS  */
        [XmlElement("first-name")]
        public String firstName;
        [XmlElement("last-name")]
        public String lastName;
        [XmlElement("birth-city")]
        public String birthCity;
        [XmlElement("birth-prov")]
        public String birthProv;
        [XmlElement("address")]
        public String address;
        [XmlElement("prov")]
        public String prov;
        [XmlElement("cf")]
        public String cf;
        [XmlElement("mrz")]
        public String mrz;
        [XmlElement("date-issue")]
        public String dateIssue;
        [XmlElement("city")]
        public String city;
        [XmlElement("photo")]
        public byte[] cie_jpg2k_image;

        /*Converte l'array di byte codificato in jpeg2000 in una bitmap*/
        public Bitmap ret_cie_bitmap()
        {
            BitmapImageCreator.Register();
            var por = J2kImage.FromBytes(cie_jpg2k_image);
            return por.As<Bitmap>();
        }

        
        /* Serializza la classe in un file XML */
        public void saveOnXML( String path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(C_CIE));

            using (StreamWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, this);
            }
        }

        /* Deserializza da un file XML*/
        static public C_CIE readFromXML( String path)
        {
            C_CIE cie = null;

            XmlSerializer serializer = new XmlSerializer(typeof(C_CIE));

            if (!File.Exists(path))
            {
                return null;
            }

            StreamReader reader = new StreamReader(path);
            cie = (C_CIE)serializer.Deserialize(reader);
            reader.Close();

            return cie;
        }

        /* Serializza in un file JSon*/
        public void saveOnJSON(String path)
        {
            var json = new JavaScriptSerializer().Serialize(this);
            System.IO.File.WriteAllText(path, json);
        }

        /* Deserializza da file JSON*/
        static public C_CIE readFromJSON(String path)
        {

            if (!File.Exists(path))
            {
                return null;
            }

            String read = System.IO.File.ReadAllText(path);
            return new JavaScriptSerializer().Deserialize<C_CIE>(read);// .DeserializeObject(json);
        }

        /* Serializza in un file CSV */
        public void saveOnCSV(string dest_file, string separator)
        {
            string string2save = firstName + separator + lastName + separator + birthCity + separator + birthProv + separator + address + separator + prov + separator + cf + separator + mrz + separator + dateIssue + separator + city + separator;
            string2save += Convert.ToBase64String(cie_jpg2k_image);
            System.IO.File.WriteAllText(dest_file, string2save);
        }

        /* Dserializza da un file CSV */
        public static C_CIE readFromCSV(string source_file, string separator)
        {
            if (!File.Exists(source_file))
            {
                return null;
            }

            C_CIE tmp = new C_CIE();
            string source = System.IO.File.ReadAllText(source_file);

            String[] substrings = source.Split(separator.ToCharArray());
            tmp.firstName = substrings[0];
            tmp.lastName = substrings[1];
            tmp.birthCity = substrings[2];
            tmp.birthProv = substrings[3];
            tmp.address = substrings[4];
            tmp.prov = substrings[5];
            tmp.cf = substrings[6];
            tmp.mrz = substrings[7];
            tmp.dateIssue = substrings[8];
            tmp.city = substrings[9];

            string cie_j = "";

            for (int i = 10; i < substrings.Length; i++)
            {
                cie_j += substrings[i];
            }

            tmp.cie_jpg2k_image = Convert.FromBase64String(cie_j);

            return tmp;

        }

        /* Parserizza la stringa contenente una data in fomrato aaaammgg in una con formato gg/mm/aaaa*/
        static public String getParsedData(String d)
        {
            return d.Substring(6, 2) + "/" + d.Substring(4, 2) + "/" + d.Substring(0, 4);
        }
    }

  
}
