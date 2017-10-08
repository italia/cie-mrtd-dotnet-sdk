using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace CIE.MRTD.SDK.Util
{
    /// <summary>
    /// La classe ByteArray incapsula un array di bytes, aggiungendo varie funzioni di manipolazione
    /// </summary>
    public class ByteArray : ICloneable
    {
        /// <summary>
        /// Converte la sequenza di bytes in un intero unsigned
        /// </summary>
        public uint ToUInt
        {
            get
            {
                if (data == null)
                    return 0;
                uint val = 0;
                for (int i = 0; i < data.Length; i++)
                    val = (val << 8) | data[i];
                return val;
            }
        }

        /// <summary>
        /// Verifica se due array di bytes sono uguali in dimensione e contenuto
        /// </summary>
        /// <param name="b">Array di bytes con cui confrontare il contenuto dell'oggetto</param>
        /// <returns>true le i due array di bytes sono uguali, false altrimenti</returns>
        public bool IsEqual(byte[] b)
        {
            if ((b == null) != (data == null))
                return false;
            if (data.Length != b.Length)
                return false;
            for (int i = 0; i < data.Length; i++)
                if (data[i] != b[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Converte l'array di bytes in una stringa codificata in Base64
        /// </summary>
        public String ToBase64
        {
            get
            {
                if (data == null)
                    return "";
                return Convert.ToBase64String(data);
            }
        }

        /// <summary>
        /// Converte l'array di bytes in una stringa che contiene il dump dei byte in esadecimale
        /// </summary>
        public String ToHex
        {
            get
            {
                if (data == null)
                    return "";
                return ToString();
            }
        }

        /// <summary>
        /// Converte l'array di bytes in una stringa convertendo i bytes in caratteri ASCII
        /// </summary>
        public String ToASCII
        {
            get
            {
                if (data == null)
                    return "";
                int index = Array.FindIndex<byte>(data, b => (b == 0));
                if (index >= 0)
                    return ASCIIEncoding.ASCII.GetString(data, 0, index);
                return ASCIIEncoding.ASCII.GetString(data);
            }
        }
        byte[] data;

        /// <summary>
        /// Ritorna la lunghezza dell'array di bytes
        /// </summary>
        public int Size { get { return data != null ? data.Length : 0; } }

        /// <summary>
        /// Ritorna l'array di bytes come tipo nativo
        /// </summary>
        public byte[] Data { get { return data; } }

        /// <summary>
        /// Ritorna un array di bytes ottenuto rimuovendo il padding BT1 dal parametro data. 
        /// Il padding BT1 è costituito dalla sequenza di bytes {0x00,0x01,0xff,...,0xff,0x00} anteposta all'array iniziale
        /// Solleva un'eccezione se il padding non è corretto
        /// </summary>
        /// <param name="data">Array di bytes da cui levare il padding</param>
        /// <returns>Array di bytes senza il padding</returns>
        public static ByteArray RemoveBT1(ByteArray data)
        {
            if (data[0] != 0)
                throw new Exception("Padding BT1 non valido");
            if (data[1] != 1)
                throw new Exception("Padding BT1 non valido");
            int i = 0;
            for (i = 2; i < data.Size - 1; i++)
            {
                if (data[i] != 0xff)
                {
                    if (data[i] != 0x00)
                        throw new Exception("Padding BT1 non valido");
                    else
                        break;
                }
            }
            return data.Sub(i + 1);
        }

        /// <summary>
        /// Restiutisce un array di bytes ottenuto a partire da un intero e anteponendo zeri come padding sino ad arrivare alla lunghezza
        /// desiderata
        /// </summary>
        /// <param name="value">Il valore da cui costruire l'array di bytes</param>
        /// <param name="size">La lunghezza desiderata</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public static ByteArray PadInt(ulong value, int size)
        {
            ByteArray sz = ((ByteArray)new BigInteger(value)).Right(size);
            if (sz.Size < size)
                return ByteArray.Fill(size - sz.Size, 0).Append(sz);
            else
                return sz;

        }

        /// <summary>
        /// Returuisce un array di bytes della lunghezza desiderata riempito del valore specificato
        /// </summary>
        /// <param name="size">La lughezza dell'array di bytes desiderata</param>
        /// <param name="content">Il valore a cui sono impostati tutti i bytes dell'array</param>
        /// <returns>L'array ottenuto</returns>
        public static ByteArray Fill(int size, byte content)
        {
            byte[] data = new byte[size];
            for (int i = 0; i < size; i++)
                data[i] = content;
            return data;
        }

        /// <summary>
        /// Ritorna un array di bytes ottenuto rimuovendo il padding ISO dal parametro data. 
        /// Il padding ISO è costituito dalla sequenza di bytes {0x80,0x00,...,0x00} posposta all'array iniziale
        /// Solleva un'eccezione se il padding non è corretto
        /// </summary>
        /// <param name="data">Array di bytes da cui levare il padding</param>
        /// <returns>Array di bytes senza il padding</returns>
        public static ByteArray ISORemove(byte[] data)
        {
            int i;
            for (i = data.Length - 1; i >= 0; i--)
            {
                if (data[i] == 0x80)
                    break;
                if (data[i] != 0)
                    throw new Exception("Padding ISO non presente");
            }
            return new ByteArray(data).Left(i);
        }


        /// <summary>
        /// Aggiunge un padding BT1 all'array di bytes fino ad arrivare alla lunghezza richiesta
        /// </summary>
        /// <param name="data">L'array di bytes a cui aggiungere il padding</param>
        /// <param name="lenght">La lunghezza deiderata</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public static ByteArray BT1Pad(ByteArray data, int lenght)
        {
            if (data.Size > (lenght - 3))
                throw new Exception("Dati da paddare troppo lunghi");
            return new ByteArray(new byte[] { 0, 1 }).Append(ByteArray.Fill(lenght - data.Size - 3, 0xff)).Append(0).Append(data);
        }

        /// <summary>
        /// Aggiunge un padding ISO all'array di bytes in modo tale che la lunghezza sia un multiplo di 8 bytes
        /// </summary>
        /// <param name="data">L'array di bytes a cui aggiungere il padding</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public static ByteArray ISOPad(byte[] data)
        {
            int padLen;
            if ((data.Length & 0x7) == 0)
                padLen = data.Length + 8;
            else
                padLen = data.Length - (data.Length & 0x7) + 0x08;

            byte[] padData = new byte[padLen];
            data.CopyTo(padData, 0);
            padData[data.Length] = 0x80;
            for (int i = data.Length + 1; i < padData.Length; i++)
                padData[i] = 0;
            return padData;
        }

        static System.Random rnd = new Random();

        /// <summary>
        /// Imposta l'array di bytes ad una sequenza di bytes casuali della lunghezza desiderata 
        /// </summary>
        /// <param name="size">La lunghezza dell'array desiderata</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public void Random(int size)
        {
            data = new byte[size];
            rnd.NextBytes(data);
        }

        /// <summary>
        /// Ritorna i primi bytes (più a sinistra) dell'array di bytes. Se il numero di bytes richiesto è superiore alla lunghezza
        /// dell'array di bytes ritorna una copia dell'array.
        /// </summary>
        /// <param name="num">Il numero di bytes più a sinistra da estrarre</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public ByteArray Left(int num)
        {
            if (num > this.data.Length)
                return this.Clone() as ByteArray;
            byte[] data = new byte[num];
            Array.Copy(this.data, data, num);
            return data;
        }

        /// <summary>
        /// Ritorna un array di bytes ottenuto scambiando l'ordine dei bytes dell'oggetto
        /// </summary>
        /// <returns>L'array di bytes ottenuto</returns>
        public ByteArray Reverse()
        {
            ByteArray rev = data.Clone() as byte[];
            Array.Reverse(rev.data);
            return rev;
        }

        /// <summary>
        /// Ritorna un array di bytes ottenuto estraendo <paramref name="num"/> bytes a partire dall'indice <paramref name="start"/>
        /// </summary>
        /// <param name="start">L'indice a partire dal quale viene estratto l'array di bytes</param>
        /// <param name="num">Il numero di elemeti dell'array di bytes</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public ByteArray Sub(int start, int num)
        {
            byte[] data = new byte[num];
            Array.Copy(this.data, start, data, 0, num);
            return data;
        }

        /// <summary>
        /// Ritorna un array di bytes ottenuto estraendo tutti i bytes a partire dall'indice <paramref name="start"/>
        /// </summary>
        /// <param name="start">L'indice a partire dal quale viene estratto l'array di bytes</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public ByteArray Sub(int start)
        {
            byte[] data = new byte[this.data.Length - start];
            Array.Copy(this.data, start, data, 0, data.Length);
            return data;
        }

        /// <summary>
        /// Ritorna gli ultimi bytes (più a destra) dell'array di bytes. Se il numero di bytes richiesto è superiore alla lunghezza
        /// dell'array di bytes ritorna una copia dell'array.
        /// </summary>
        /// <param name="num">Il numero di bytes più a destra da estrarre</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public ByteArray Right(int num)
        {
            if (num > this.data.Length)
                return this.Clone() as ByteArray;
            byte[] data = new byte[num];
            Array.Copy(this.data, this.data.Length - num, data, 0, num);
            return data;
        }

        /// <summary>
        /// Ritorna un array di bytes che codifica un tag ASN1 espresso come numero intero unsigned
        /// </summary>
        /// <param name="value">Il numero di tag da convertire in array di bytes</param>
        /// <returns>L'array di bytes ottenuto</returns>
        static byte[] tagToBytes(ulong value)
        {
            if (value <= 0xff)
            {
                return new byte[] { (byte)value };
            }
            else if (value <= 0xffff)
            {
                return new byte[] { (byte)(value >> 8), (byte)(value & 0xff) };
            }
            else if (value <= 0xffffff)
            {
                return new byte[] { (byte)(value >> 16), (byte)((value >> 8) & 0xff), (byte)(value & 0xff) };
            }
            else if (value <= 0xffffffff)
            {
                return new byte[] { (byte)(value >> 24), (byte)((value >> 16) & 0xff), (byte)((value >> 8) & 0xff), (byte)(value & 0xff) };
            }
            throw new Exception("tag troppo lungo");
        }

        /// <summary>
        /// Ritorna un array di bytes che codifica la lunghezza di un tag ASN1 espresso come numero intero unsigned
        /// </summary>
        /// <param name="value">Il numero di tag da convertire in array di bytes</param>
        /// <returns>L'array di bytes ottenuto</returns>
        static byte[] lenToBytes(ulong value)
        {
            if (value < 0x80)
            {
                return new byte[] { (byte)value };
            }
            if (value <= 0xff)
            {
                return new byte[] { 0x81, (byte)value };
            }
            else if (value <= 0xffff)
            {
                return new byte[] { 0x82, (byte)(value >> 8), (byte)(value & 0xff) };
            }
            else if (value <= 0xffffff)
            {
                return new byte[] { 0x83, (byte)(value >> 16), (byte)((value >> 8) & 0xff), (byte)(value & 0xff) };
            }
            else if (value <= 0xffffffff)
            {
                return new byte[] { 0x84, (byte)(value >> 24), (byte)((value >> 16) & 0xff), (byte)((value >> 8) & 0xff), (byte)(value & 0xff) };
            }
            throw new Exception("dati troppo lunghi");
        }

        /// <summary>
        /// Ritorna un array di bytes che contiene la codifica in ASN1 dell'array di bytes contenuto nell'oggetto, inserito all'interno del
        /// tag ASN1 specificato
        /// </summary>
        /// <param name="tag">Il numero del tag che deve contenere l'array di bytes</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public ByteArray ASN1Tag(ulong tag)
        {
            byte[] _tag = tagToBytes(tag);
            byte[] _len = lenToBytes((ulong)this.data.Length);
            byte[] data = new byte[_tag.Length + _len.Length + this.data.Length];
            Array.Copy(_tag, 0, data, 0, _tag.Length);
            Array.Copy(_len, 0, data, _tag.Length, _len.Length);
            Array.Copy(this.data, 0, data, _tag.Length + _len.Length, this.data.Length);
            return data;
        }

        /// <summary>
        /// Ritorna un array di bytes ottenuto concatenando a destra dell'array di bytes contenuto nell'oggetto quello specificato tramite stringa esadecimale
        /// </summary>
        /// <param name="data">L'array di bytes da concatenare specificato tramite stringa esadecimale</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public ByteArray Append(String data)
        {
            return Append(new ByteArray(data));
        }

        /// <summary>
        /// Ritorna un array di bytes ottenuto concatenando a destra dell'array di bytes il byte specificato 
        /// </summary>
        /// <param name="data">Il byte da concatenare</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public ByteArray Append(byte data)
        {
            return Append(new byte[] { data });
        }

        /// <summary>
        /// Ritrena o imposta un elemento dell'array di bytes
        /// </summary>
        /// <param name="i">L'indice dell'elemento</param>
        /// <returns>L'elemento dell'array di bytes</returns>
        public byte this[int i]
        {
            get { return data[i]; }
            set { data[i] = value; }
        }

        /// <summary>
        /// Ritorna un array di bytes ottenuto concatenando a destra dell'array di bytes contenuto nell'oggetto quello specificato 
        /// </summary>
        /// <param name="data">L'array di bytes da concatenare</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public ByteArray Append(byte[] data)
        {
            if (data == null)
            {
                if (this.data == null)
                    return new ByteArray();
                else
                    return this.data.Clone() as byte[];
            }
            else if (this.data == null)
                return data.Clone() as byte[];
            byte[] newData = new byte[data.Length + this.data.Length];
            this.data.CopyTo(newData, 0);
            data.CopyTo(newData, this.data.Length);
            return newData;
        }

        /// <summary>
        /// Converte un oggetto di tipo BigInteger in un array di bytes
        /// </summary>
        /// <param name="bi">L'oggetto BigInteger da convertire</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public static implicit operator ByteArray(BigInteger bi)
        {
            byte[] data = bi.ToByteArray();
            Array.Reverse(data);
            int i = 0;
            for (i = 0; i < data.Length; i++)
                if (data[i] != 0)
                    break;
            if (i == 0)
                return new ByteArray(data);
            byte[] data2 = new byte[data.Length - i];
            Array.Copy(data, i, data2, 0, data2.Length);
            return new ByteArray(data2);
        }

        /// <summary>
        /// Converte una stringa esadecimale in un array di bytes. La strniga deve contenere una sequenza di bytes in formato esadecimale.
        /// Es. "10 FF 6E 2B"
        /// </summary>
        /// <param name="str">La stringa da convertire</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public static implicit operator ByteArray(String str)
        {
            return new ByteArray(str);
        }

        /// <summary>
        /// Converte array di byte nativo in un oggeto di tipo ByteArray
        /// </summary>
        /// <param name="ba">L'array di bytes da convertire</param>
        /// <returns>L'array di bytes ottenuto</returns>

        public static implicit operator ByteArray(Byte[] ba)
        {
            return new ByteArray(ba);
        }

        /// <summary>
        /// Converte l'array di bytes in un oggetto di tipo BigInteger
        /// </summary>
        /// <param name="ba">L'array di bytes da convertire</param>
        /// <returns>L'oggetto BigInteger ottenuto</returns>
        public static implicit operator BigInteger(ByteArray ba)
        {
            if ((ba[0] & 0x80) != 0)
                return new BigInteger(ba.Reverse().Append(0));
            else
                return new BigInteger(ba.Reverse());
        }

        /// <summary>
        /// Converte la stringa di caratteri ASCII in un array di bytes
        /// </summary>
        /// <param name="data">La string da convertire</param>
        /// <returns>L'array di bytes </returns>
        static public ByteArray FromASCII(String data)
        {
            return ASCIIEncoding.ASCII.GetBytes(data);
        }

        /// <summary>
        /// Riturna un array di bytes vuoto
        /// </summary>
        public ByteArray() { }

        /// <summary>
        /// Riturna un array di bytes contenente solo l'elemento specificato
        /// </summary>
        /// <param name="data">L'elemento da inserire nell'array</param>
        public ByteArray(byte data)
        {
            this.data = new byte[] { data };
        }

        /// <summary>
        /// Riturna un array di bytes contenente l'array specificato
        /// </summary>
        /// <param name="data">L'array di bytes che incapsula l'oggetto</param>
        public ByteArray(byte[] data)
        {
            this.data = data;
        }

        /// <summary>
        /// Riturna un array di bytes costruito a partire dalla string specificata che deve contenere una sequenza di bytes in formato esadecimale
        /// </summary>
        /// <param name="hexData">La stringa contenente una sequenza di bytes in fromato esadecimale</param>
        public ByteArray(string hexData)
        {
            data = ReadHexData(hexData);
        }

        /// <summary>
        /// Converte un oggetto della classe nel tipo arry di bytes nativo
        /// </summary>
        /// <param name="ba">L'oggetto da convertire</param>
        /// <returns>L'array di bytes nativo ottenuto</returns>
        public static implicit operator byte[](ByteArray ba)
        {
            if (ba!=null)
                return ba.data;
            return null;
        }

        /// <summary>
        /// Costruisce uno stream di memoria il cui contenuto è l'array di bytes 
        /// </summary>
        /// <param name="ba">L'oggetto da convertire</param>
        /// <returns>Lo stream di memoria ottenuto</returns>
        public static implicit operator MemoryStream(ByteArray ba)
        {
            return new MemoryStream(ba.data);
        }

        static byte hex2byte(char h)
        {
            if (h >= '0' && h <= '9') return (byte)(h - '0');
            if (h >= 'A' && h <= 'F') return (byte)(h + 10 - 'A');
            if (h >= 'a' && h <= 'f') return (byte)(h + 10 - 'a');
            return 0;
        }

        static bool IsHexDigit(char c)
        {
            if (c >= '0' && c <= '9') return true;
            if (c >= 'a' && c <= 'f') return true;
            if (c >= 'A' && c <= 'F') return true;
            return false;
        }

        /// <summary>
        /// Converte l'array di bytes in una stringa contenente la sequenza di bytes in formato esadecimale
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (data == null)
                return "";
            StringBuilder sb = new StringBuilder(data.Length * 3);
            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("X02") + " ");
            return sb.ToString();
        }

        /// <summary>
        /// Ritorna un array di bytes ottenuto da una stringa contenente una sequenza di bytes in formato esadecimale
        /// </summary>
        /// <param name="data">La stringa contenente una sequenza di bytes in fromato esadecimale</param>
        /// <returns>L'array di bytes ottenuto</returns>
        public static byte[] ReadHexData(String data)
        {
            List<byte> dt = new List<byte>();

            int slen = data.Length;
            for (int i = 0; i < slen; i++)
            {
                Char c = data[i];
                if (Char.IsWhiteSpace(c) || c == ',') continue;
                if (!IsHexDigit(c))
                {
                    throw new Exception("Carattere non valido:" + c);
                }

                if ((i < slen - 3) && c == '0' && data[i + 3] == 'h')
                    continue;

                if ((i < slen - 2) && c == '0' && data[i + 1] == 'x')
                {
                    i += 1;
                    continue;
                }
                byte v = hex2byte(c);
                i++;
                Char d = data[i];
                if (i < slen)
                {
                    if (IsHexDigit(d))
                    {
                        v <<= 4;
                        v |= hex2byte(d);
                    }
                    else if (!Char.IsWhiteSpace(d))
                        throw new Exception("richiesto spazio");
                }
                dt.Add(v);

                if (i < (slen - 1) && data[i + 1] == 'h')
                    i++;
            }
            return dt.ToArray();
        }

        #region ICloneable Members

        /// <summary>
        /// Ritorna una copa dell'oggetto
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new ByteArray((byte[])data.Clone());
        }

        #endregion
    }
}
