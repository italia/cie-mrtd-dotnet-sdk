using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CIE.MRTD.SDK.Util
{
    /// <summary>
    /// Enumerazione dei tag ASN1 predefiniti
    /// </summary>
    public enum ASN1TagType : byte
    {
        TAG_MASK = 0x1F,
        BOOLEAN = 0x01,
        INTEGER = 0x02,
        BIT_STRING = 0x03,
        OCTET_STRING = 0x04,
        TAG_NULL = 0x05,
        OBJECT_IDENTIFIER = 0x06,
        OBJECT_DESCRIPTOR = 0x07,
        EXTERNAL = 0x08,
        REAL = 0x09,
        ENUMERATED = 0x0a,
        UTF8_STRING = 0x0c,
        RELATIVE_OID = 0x0d,
        SEQUENCE = 0x10,
        SET = 0x11,
        NUMERIC_STRING = 0x12,
        PRINTABLE_STRING = 0x13,
        T61_STRING = 0x14,
        VIDEOTEXT_STRING = 0x15,
        IA5_STRING = 0x16,
        UTC_TIME = 0x17,
        GENERALIZED_TIME = 0x18,
        GRAPHIC_STRING = 0x19,
        VISIBLE_STRING = 0x1a,
        GENERAL_STRING = 0x1b,
        UNIVERSAL_STRING = 0x1C,
        BMPSTRING = 0x1E	/* 30: Basic Multilingual Plane/Unicode string */
    };

    /// <summary>
    /// Enumerazione delle classi di tag ASN1
    /// </summary>
    public enum ASN1TagClasses : byte
    {
        CLASS_MASK = 0xc0,
        UNIVERSAL = 0x00,
        CONSTRUCTED = 0x20,
        APPLICATION = 0x40,
        CONTEXT_SPECIFIC = 0x80,
        PRIVATE = 0xc0,
        UNKNOWN = 0xff
    };

    internal interface IASN1Display
    {
        string contentString(ASN1Tag tag);
    }

    internal class ASN1GenericDisplay : IASN1Display
    {
        public static ASN1GenericDisplay singleton=new ASN1GenericDisplay();
        public ASN1GenericDisplay() { 
        }

        public string contentString(ASN1Tag tag)
        {
            string val = "";
            if (tag.Data != null)
            {
                val += " Len " + tag.Data.Length.ToString("X2") + ":" + new ByteArray(tag.Data).Sub(0, Math.Min(30, tag.Data.Length));
            }
            return val;
        }
    }

    internal class ASN1NullDisplay : IASN1Display
    {
        public static ASN1NullDisplay singleton = new ASN1NullDisplay();
        public ASN1NullDisplay()
        {
        }

        public string contentString(ASN1Tag tag)
        {
            return "NULL";
        }
    }

    internal class ASN1StringDisplay : IASN1Display
    {
        public static ASN1StringDisplay  singleton = new ASN1StringDisplay ();
        public ASN1StringDisplay()
        {
        }

        public string contentString(ASN1Tag tag)
        {
            return ASCIIEncoding.ASCII.GetString(tag.Data);
        }
    }

    internal class ASN1UTCTimeDisplay : IASN1Display
    {
        public static ASN1UTCTimeDisplay  singleton = new ASN1UTCTimeDisplay ();
        public ASN1UTCTimeDisplay()
        {
        }

        public string contentString(ASN1Tag tag)
        {
            return ASCIIEncoding.ASCII.GetString(tag.Data);
        }
    }

    internal class ASN1ObjIdDisplay : IASN1Display
    {
        public static ASN1ObjIdDisplay  singleton = new ASN1ObjIdDisplay ();
        public ASN1ObjIdDisplay()
        {
        }

        public string contentString(ASN1Tag tag)
        {
            try
            {
                //primi due numeri;
                List<int> nums=new List<int>();
                int n2 = (tag.Data[0] % 40);
                int n1 = ((tag.Data[0] - n2) / 40);
                nums.Add(n1);
                nums.Add(n2);
                int pos = 1;
                int curNum = 0;
                bool doing = false;
                while (pos < tag.Data.Length)
                {
                    byte val = tag.Data[pos];
                    curNum = (curNum << 7) | (val & 127);
                    if ((val & 0x80) == 0)
                    {
                        nums.Add(curNum);
                        curNum = 0;
                        doing = false;
                    }
                    else
                        doing = true;
                    pos++;
                }
                if (doing)
                    return "INVALID OID";
                else {
                    String res="";
                    foreach (int i in nums)
                        res += i.ToString() + ".";                    
                    return res.Substring(0, res.Length - 1);
                }
            }
            catch 
            {
                return "INVALID OID";
            }
        }
    }

    /// <summary>
    /// La classe ASN1Tag definisce un tag ASN1 ed espone metodi per la codifica e la decodifica
    /// </summary>
    public class ASN1Tag
    {
        byte unusedBits=0;
        uint startPos, endPos;

        /// <summary>
        /// Ritorna la posizione di inizio del tag all'interno dello stream da cui è stato letto
        /// </summary>
        public uint StartPos { get { return startPos; } }

        /// <summary>
        /// Ritorna la posizione di fine del tag all'interno dello stream da cui è stato letto
        /// </summary>
        public uint EndPos { get { return endPos; } }

        internal IASN1Display display;

        /// <summary>
        /// L'array di bytes che compone il numero del tag
        /// </summary>
        internal byte[] tag;
        byte[] data;

        /// <summary>
        /// Ritorna il contenuto del tag, eventualmente codificando la sequenza di  sotto-tag
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (data != null)
                    return data;
                else
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        foreach(var v in children)
                            v.Encode(ms);
                        return ms.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// La lista dei sotto-tag
        /// </summary>
        internal List<ASN1Tag> children;
        static UInt32 BytesToInt(byte[] data) {
            UInt32 tot=0;
            for (int i = 0; i < data.Length; i++) {
                tot = (tot << 8) | data[i];
            }
            return tot;
        }

        /// <summary>
        /// Ritorna il numero del tag come intero senza segno e senza la conversione dal formato ASN1
        /// </summary>
        public UInt32 tagRawNumber {
            get {
                UInt32 num = tag[0];
                for (int i = 1; i < tag.Length; i++)
                {
                    num = (UInt32)(num << 8) | tag[i];
                }
                return num;
            }
        }

        /// <summary>
        /// Ritorna il numero del tag come intero senza segno, secondo le regole di conversione del formato ASN1
        /// </summary>
        public UInt32 tagNumber { 
            get {
                UInt32 num=0;
                num |= (UInt32)(tag[0] & 0x1f);
                for (int i = 1; i < tag.Length; i++) { 
                    int shift;
                    if (i == 1) shift = 5;
                    else shift = 7;
                    num = (UInt32)(num << shift) | tag[i];
                }
                return num;
            }
        }

        /// <summary>
        /// Ritorna true se il numreo del tag include il bit CONSTRUCTED
        /// </summary>
        public bool tagConstructed {
            get
            {
                return (tag[0] & 0x20) != 0;
            }
        }

        /// <summary>
        /// Ritorna la classe ricavata dal numero del tag
        /// </summary>
        public ASN1TagClasses tagClass
        {
            get { 
                switch (tag[0] & 0xc0) { 
                    case 0x00:
                        return ASN1TagClasses.UNIVERSAL;
                    case 0x40:
                        return ASN1TagClasses.APPLICATION;
                    case 0x80:
                        return ASN1TagClasses.CONTEXT_SPECIFIC;
                    case 0xc0:
                        return ASN1TagClasses.PRIVATE;
                };
                return ASN1TagClasses.UNKNOWN;
            }
        }

        static byte[] IntToBytes(UInt32 num)
        {
            if (num <= 0xff) return new byte[] { (byte)num };
            if (num <= 0xffff) return new byte[] { (byte)(num >> 8), (byte)(num & 0xff) };
            if (num <= 0xffffff) return new byte[] {(byte)(num >> 16), (byte)((num >> 8) & 0xff), (byte)(num & 0xff) };
            return new byte[] {(byte)(num >> 24), (byte)((num >> 16) & 0xff), (byte)((num >> 8) & 0xff), (byte)(num & 0xff) };
        }
        byte[] ASN1Length(UInt32 len) {
            if (len <= 0x7f) return new byte[] { (byte)len };
            if (len <= 0xff) return new byte[] { 0x81,(byte)len };
            if (len <= 0xffff) return new byte[] { 0x82, (byte)(len>>8) ,(byte)(len& 0xff) };
            if (len <= 0xffffff) return new byte[] { 0x83, (byte)(len >> 16), (byte)((len >> 8) & 0xff), (byte)(len & 0xff) };
            return new byte[] { 0x84, (byte)(len >> 24), (byte)((len >> 16) & 0xff),(byte)((len >> 8) & 0xff), (byte)(len & 0xff) };
        }

        /// <summary>
        /// Verifica che il tag corrisponda a quello specificato (sotto forma di intero senza segno raw) e ritorna l'oggetto stesso per costrutti di tipo fluent
        /// </summary>
        /// <param name="tagCheck">Il numero del tag da verificare</param>
        /// <returns>Lo stesso oggetto ASN1Tag</returns>
        public ASN1Tag CheckTag(uint tagCheck)
        {
            if (tagRawNumber != tagCheck)
                throw new Exception("Check del tag fallito");
            return this;
        }
        /// <summary>
        /// Verifica che il tag corrisponda a quello specificato (sotto forma di array di bytes) e ritorna l'oggetto stesso per costrutti di tipo fluent
        /// </summary>
        /// <param name="tagCheck">Il numero del tag da verificare</param>
        /// <returns>Lo stesso oggetto ASN1Tag</returns>
        public ASN1Tag CheckTag(byte[] tagCheck)
        {
            if (!AreEqual(tag, tagCheck))
                throw new Exception("Check del tag fallito");
            return this;
        }

        /// <summary>
        /// Ritorna un sotto-tag dell'oggetto
        /// </summary>
        /// <param name="tagNum">Il numero di sequenza del sotto-tag (a partire da 0)</param>
        /// <returns>L'oggetto che contiene il sotto-tag</returns>
        public ASN1Tag Child(int tagNum)
        {
            return children[tagNum];
        }

        /// <summary>
        /// Ritorna un sotto-tag dell'oggetto verificando che il suo numero di tag corrisponda a quello specificato (sotto forma di intero senza segno raw)
        /// </summary>
        /// <param name="tagNum">Il numero di sequenza  del sotto-tag (a partire da 0)</param>
        /// <param name="tagCheck">Il numero del sotto-tag da verificare</param>
        /// <returns>L'oggetto che contiene il sotto-tag</returns>
        public ASN1Tag Child(int tagNum, uint tagCheck)
        {
            ASN1Tag tag = children[tagNum];
            if (tag.tagRawNumber!=tagCheck)
                throw new Exception("Check del tag fallito");
            return tag;
        }
        /// <summary>
        /// Ritorna un sotto-tag che abbia il numero di tag (sotto forma di intero senza segno raw) corrispondente a quello specificato
        /// </summary>
        /// <param name="tagId">Il numero del tag da cercare</param>
        /// <returns>L'oggetto che contiene il sotto-tag, o null se non viene trovato</returns>
        public ASN1Tag ChildWithTagId(uint tagId)
        {
            foreach (var tag in children)
            {
                if (tag.tagRawNumber == tagId)
                    return tag;
            }
            return null;
        }
        /// <summary>
        /// Ritorna un sotto-tag che abbia il numero di tag (sotto forma di array di bytes) corrispondente a quello specificato
        /// </summary>
        /// <param name="tagId">Il numero del tag da cercare</param>
        /// <returns>L'oggetto che contiene il sotto-tag, o null se non viene trovato</returns>
        public ASN1Tag ChildWithTagId(byte[] tagId)
        {
            foreach (var tag in children)
            {
                if (AreEqual(tag.tag,tagId))
                    return tag;
            }
            return null;
        }
        bool AreEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Verifica che il contenuto del tag corrisponda all'array di bytes specficato. Solleva un'eccezione se non corrispondono
        /// </summary>
        /// <param name="dataCheck">L'array di bytes da verificare</param>
        public void Verify(byte[] dataCheck) {
            if (!AreEqual(data, dataCheck))
                throw new Exception("Check del contenuto fallito");
        }

        /// <summary>
        /// Ritorna un sotto-tag dell'oggetto verificando che il suo numero di tag corrisponda a quello specificato (sotto forma di intero senza segno raw)
        /// </summary>
        /// <param name="tagNum">Il numero di sequenza  del sotto-tag (a partire da 0)</param>
        /// <param name="tagCheck">Il numero del sotto-tag da verificare</param>
        /// <returns>L'oggetto che contiene il sotto-tag</returns>
        public ASN1Tag Child(int tagNum, byte[] tagCheck)
        {
            ASN1Tag subTag = children[tagNum];
            if (!AreEqual(subTag.tag, tagCheck))
                throw new Exception("Check del tag fallito");
            return subTag;
        }

        /// <summary>
        /// Crea un tag con l'identificativo e il contenuto specificato
        /// </summary>
        /// <param name="tag">Il numero del tag sotto forma di intero senza segno raw</param>
        /// <param name="data">L'array di bytes che contiene i dati del tag</param>
        public ASN1Tag(UInt32 tag, byte[] data)
        {
            this.tag = IntToBytes(tag);
            this.data = data;
            this.children = null;
            display = KnownDisplay(this.tag);
        }
        /// <summary>
        /// Crea un tag con l'identificativo e il contenuto specificato
        /// </summary>
        /// <param name="tag">Il numero del tag sotto forma di array di bytes</param>
        /// <param name="data">L'array di bytes che contiene i dati del tag</param>
        public ASN1Tag(byte[] tag, byte[] data)
        {
            this.tag = tag;
            this.data = data;
            this.children = null;
            display = KnownDisplay(this.tag);
        }
        /// <summary>
        /// Crea un tag vuoto con l'identificativo specificato
        /// </summary>
        /// <param name="tag">Il numero del tag sotto forma di array di bytes</param>
        public ASN1Tag(byte[] tag)
        {
            this.tag = tag;
            this.data = null;
            this.children=null;
            display = KnownDisplay(this.tag);
        }
        /// <summary>
        /// Crea un tag con l'identificativo e il contenuto specificato
        /// </summary>
        /// <param name="tag">Il numero del tag sotto forma di array di bytes</param>
        /// <param name="children">L'elenco di tag che contiene i sotto-tag dell'oggetto</param>
        public ASN1Tag(byte[] tag, IEnumerable<ASN1Tag> children)
        {
            this.tag = tag;
            this.data = null;
            this.children = new List<ASN1Tag>();
            this.children.AddRange(children);
            display = KnownDisplay(this.tag);
        }
        /// <summary>
        /// Crea un tag con l'identificativo e il contenuto specificato
        /// </summary>
        /// <param name="tag">Il numero del tag sotto forma di intero senza segno raw</param>
        /// <param name="children">L'elenco di tag che contiene i sotto-tag dell'oggetto</param>
        public ASN1Tag(UInt32 tag, IEnumerable<ASN1Tag> children)
        {
            this.tag = IntToBytes(tag);
            this.data = null;
            this.children = new List<ASN1Tag>();
            this.children.AddRange(children);
            display = KnownDisplay(this.tag);
        }

        /// <summary>
        /// Legge dallo stream una lunghezza codificata in ASN1 e la restituisce
        /// </summary>
        /// <param name="s">Lo stream da leggere</param>
        /// <returns></returns>
        public static int ParseLength(Stream s)
        {
            UInt32 size = 0;
            return ParseLength(s, 0, (UInt32)s.Length, ref size);
        }

        /// <summary>
        /// Legge dall'array di bytes una lunghezza codificata in ASN1 e la restituisce
        /// </summary>
        /// <param name="data">L'array di bytes che contiene la lunghezza</param>
        /// <returns></returns>
        public static int ParseLength(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return ParseLength(ms);
            }
        }

        /// <summary>
        /// Decodifica il contenuto di un array di bytes in una struttura ASN1
        /// </summary>
        /// <param name="data">I dati da decodificare</param>
        /// <param name="reparse">true per applicare il parse ai tag che hanno contenuto binario ed eventualmente rappresentarli cone struttura ASN1</param>
        /// <returns>L'oggetto ASN1Tag di livello più alto della struttura</returns>
        public static ASN1Tag Parse(byte[] data,bool reparse)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return Parse(ms, reparse);
            }
        }

        /// <summary>
        /// Decodifica il contenuto di un array di bytes in una struttura ASN1. Applica il parse ai tag che hanno contenuto binario ed eventualmente li rappresenta cone struttura ASN1
        /// </summary>
        /// <param name="data">I dati da decodificare</param>
        /// <returns>L'oggetto ASN1Tag di livello più alto della struttura</returns>
        public static ASN1Tag Parse(byte[] data)
        {
            using (MemoryStream ms=new MemoryStream(data)) {
                return Parse(ms,true);
            }
        }

        /// <summary>
        /// Decodifica il contenuto di uno stream in una struttura ASN1. Applica il parse ai tag che hanno contenuto binario ed eventualmente li rappresenta cone struttura ASN1
        /// </summary>
        /// <param name="s">Lo stream da decodificare</param>
        /// <returns>L'oggetto ASN1Tag di livello più alto della struttura</returns>
        public static ASN1Tag Parse(Stream s)
        {
            return Parse(s, true);
        }

        /// <summary>
        /// Decodifica il contenuto di uno stream in una struttura ASN1
        /// </summary>
        /// <param name="s">Lo stream da decodificare</param>
        /// <param name="reparse">true per applicare il parse ai tag che hanno contenuto binario ed eventualmente rappresentarli cone struttura ASN1</param>
        /// <returns>L'oggetto ASN1Tag di livello più alto della struttura</returns>

        public static ASN1Tag Parse(Stream s,bool reparse)
        {
            UInt32 size = 0;
            return Parse(s, 0, (UInt32)s.Length, ref size,reparse);
        }

        /// <summary>
        /// Codifica il tag contenuto nell'oggetto in un uno stream
        /// </summary>
        /// <param name="s">Lo stream in cui codificare l'oggetto</param>
        public void Encode(Stream s)
        {
            List<byte[]> childs = null;
            s.Write(tag, 0, tag.Length);
            uint len = 0;
            if (data != null)
                len = (uint)data.Length;
            else
            {
                childs = new List<byte[]>();
                len = 0;
                foreach (ASN1Tag t in children)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        t.Encode(ms);
                        byte[] dat = ms.ToArray();
                        len += (uint)dat.Length;
                        childs.Add(dat);
                    }
                }

            }
            if (tag[0] == 3 && tag.Length==1 && data==null)
                len++;
            if (len < 128)
                s.WriteByte((byte)len);
            else if (len < 256)
            {
                s.WriteByte(0x81);
                s.WriteByte((byte)len);
            }
            else if (len <= 0xffff)
            {
                s.WriteByte(0x82);
                s.WriteByte((byte)(len >> 8));
                s.WriteByte((byte)(len & 0xff));
            }
            else if (len <= 0xffffff)
            {
                s.WriteByte(0x83);
                s.WriteByte((byte)(len >> 16));
                s.WriteByte((byte)((len >> 8) & 0xff));
                s.WriteByte((byte)(len & 0xff));
            }
            else
            {
                s.WriteByte(0x84);
                s.WriteByte((byte)(len >> 24));
                s.WriteByte((byte)((len >> 16) & 0xff));
                s.WriteByte((byte)((len >> 8) & 0xff));
                s.WriteByte((byte)(len & 0xff));
            }
            if (tag[0] == 3 && tag.Length==1 && data==null)
                s.WriteByte(unusedBits);

            if (data != null)
                s.Write(data, 0, data.Length);
            else
            {
                foreach (byte[] t in childs)
                {
                    s.Write(t, 0, t.Length);
                }
            }
        }
        internal static int ParseLength(Stream s, UInt32 start, UInt32 length, ref UInt32 size)
        {
            UInt32 readPos = 0;
            if (readPos == length)
                throw new Exception();
            // leggo il tag
            List<byte> tagVal = new List<byte>();
            int tag = s.ReadByte();
            readPos++;
            tagVal.Add((byte)tag);
            if ((tag & 0x1f) == 0x1f)
            {
                // è un tag a più bytes; proseguo finchè non trovo un bit 8 a 0
                while (true)
                {
                    if (readPos == length)
                        throw new Exception();
                    tag = s.ReadByte();
                    readPos++;
                    tagVal.Add((byte)tag);
                    if ((tag & 0x80) != 0x80)
                        // è l'ultimo byte del tag
                        break;
                }
            }
            // leggo la lunghezza
            if (readPos == length)
                throw new Exception();
            UInt32 len = (UInt32)s.ReadByte();
            readPos++;
            if (len > 0x80)
            {
                UInt32 lenlen = len - 0x80;
                len = 0;
                for (int i = 0; i < lenlen; i++)
                {
                    if (readPos == length)
                        throw new Exception();
                    len = (UInt32)((len << 8) | (byte)s.ReadByte());
                    readPos++;
                }
            }
            size = (UInt32)(readPos + len);
            return (int)size;

        }

        internal static ASN1Tag Parse(Stream s, UInt32 start, UInt32 length, ref UInt32 size)
        {
            return Parse(s, start, length, ref size, true);
        }

        internal static ASN1Tag Parse(Stream s, UInt32 start, UInt32 length, ref UInt32 size, bool reparse)
        {
            UInt32 readPos = 0;
            if (readPos == length)
                throw new Exception();
            // leggo il tag
            List<byte> tagVal = new List<byte>();
            int tag = s.ReadByte();
            readPos++;
            tagVal.Add((byte)tag);
            if ((tag & 0x1f) == 0x1f)
            {
                // è un tag a più bytes; proseguo finchè non trovo un bit 8 a 0
                while (true)
                {
                    if (readPos == length)
                        throw new Exception();
                    tag = s.ReadByte();
                    readPos++;
                    tagVal.Add((byte)tag);
                    if ((tag & 0x80) != 0x80)
                        // è l'ultimo byte del tag
                        break;
                }
            }
            // leggo la lunghezza
            if (readPos == length)
                throw new Exception();
            UInt32 len = (UInt32)s.ReadByte();
            readPos++;
            if (len > 0x80)
            {
                UInt32 lenlen = len - 0x80;
                len = 0;
                for (int i = 0; i < lenlen; i++)
                {
                    if (readPos == length)
                        throw new Exception();
                    len = (UInt32)((len << 8) | (byte)s.ReadByte());
                    readPos++;
                }
            }
            else if (len == 0x80)
            {
                throw new Exception("Lunghezza indefinita non supportata");
            }
            size = (UInt32)(readPos + len);
            if (size > length)
                throw new Exception("ASN1 non valido");
            if (tagVal.Count == 1 && tagVal[0] == 0 && len == 0)
            {
                return null;
            }
            byte[] data = new byte[len];
            s.Read(data, 0, (int)len);
            MemoryStream ms = new MemoryStream(data);
            // quando devo parsare i sotto tag??
            // in teoria solo se il tag è constructed, ma
            // spesso una octetstring o una bitstring sono
            // delle strutture ASN1...
            ASN1Tag newTag = new ASN1Tag(tagVal.ToArray());
            List<ASN1Tag> childern = null;
            UInt32 parsedLen = 0;
            bool parseSubTags = false;
            if (newTag.tagConstructed)
                parseSubTags = true;
            else if (reparse && KnownTag(newTag.tag) == "OCTET STRING")
                parseSubTags = true;
            else if (reparse && KnownTag(newTag.tag) == "BIT STRING")
            {
                parseSubTags = true;
                newTag.unusedBits = (byte)ms.ReadByte();
                parsedLen++;
            }

            if (parseSubTags)
            {
                childern = new List<ASN1Tag>();
                while (true)
                {
                    UInt32 childSize = 0;
                    try
                    {
                        ASN1Tag child = Parse(ms, start + readPos + parsedLen, (UInt32)(len - parsedLen), ref childSize, reparse);
                        if (child != null)
                            childern.Add(child);
                    }
                    catch
                    {
                        childern = null;
                        break;
                    }
                    parsedLen += childSize;
                    if (parsedLen > len)
                    {
                        childern = null;
                        break;
                    }
                    else if (parsedLen == len)
                    {
                        data = null;
                        break;
                    }
                }
            }
            newTag.startPos = start;
            newTag.endPos = start + size;
            if (childern == null)
            {
                newTag.data = data;
            }
            else
            {
                newTag.children = childern;
            }
            return newTag;
        }
        public override string ToString()
        {
            String val = KnownTag(tag);
            if (val==null)
                val=tagClass.ToString() + " " + tagNumber.ToString("X2");
            if (tagConstructed) val += " Constructed ";
            val += " (" + new ByteArray(tag).ToString() + ") ";
            if (display == null)
                val += ASN1GenericDisplay.singleton.contentString(this);
            else
                val += display.contentString(this);
            return val;
        }
        static IASN1Display KnownDisplay(byte[] tag) {
            if (tag.Length == 1)
            {
                switch (tag[0])
                {
                    case 5: return ASN1NullDisplay.singleton;
                    case 6: return ASN1ObjIdDisplay.singleton;
                    case 12: return ASN1StringDisplay.singleton;
                    case 19: return ASN1StringDisplay.singleton;
                    case 20: return ASN1StringDisplay.singleton;
                    case 22: return ASN1StringDisplay.singleton;
                    case 23: return ASN1UTCTimeDisplay.singleton;
                }
            }
            return null;
        }
        static String KnownTag(byte[] tag)
        {
            if (tag.Length == 1) {
                switch (tag[0])
                {
                    case 2: return "INTEGER";
                    case 3: return "BIT STRING";
                    case 4: return "OCTET STRING";
                    case 5: return "NULL";
                    case 6: return "OBJECT IDENTIFIER";
                    case 0x30: return "SEQUENCE";
                    case 0x31: return "SET";
                    case 12: return "UTF8 String";
                    case 19: return "PrintableString";
                    case 20: return "T61String";
                    case 22: return "IA5String";
                    case 23: return "UTCTime";
                }
            }
            return null;
        }
    }
}
