using System;

namespace CIE.MRTD.SDK.PCSC
{
	/// <summary>
	/// La classe Apdu rappresenta un comando da inviare alla smart card.
    /// L'APDU è formata da un Header di 4 bytes (CLA, INS, P1, P2), un campo dati opzionale e una lunghezza ritornata opzionale
	/// </summary>
    public class Apdu
	{
        /// <summary>Byte INS</summary>
		public Byte INS;
        /// <summary>Byte CLA</summary>
		public Byte CLA;
        /// <summary>Byte P1</summary>
		public Byte P1;
        /// <summary>Byte P2</summary>
		public Byte P2;
        /// <summary>Campo dati</summary>
		public byte[] Data;
        /// <summary>Lunghezza ritornata</summary>
		public Byte? LE;

        /// <summary>
        /// Costruisce un APDU caso 4 (Campo dati ed LE)
        /// </summary>
        /// <param name="_CLA">Byte CLA</param>
        /// <param name="_INS">Byte INS</param>
        /// <param name="_P1">Byte P1</param>
        /// <param name="_P2">Byte P2</param>
        /// <param name="_Data">Campo Dati</param>
        /// <param name="_LE">Lunghezza aspettata</param>
        public Apdu(Byte _CLA, Byte _INS, Byte _P1, Byte _P2, byte[] _Data, Byte _LE)
		{
			INS=_INS;
			CLA=_CLA;
			P1=_P1;
			P2=_P2;
			Data=_Data;
			LE=_LE;
		}

        /// <summary>
        /// Costruisce un APDU caso 3 (Campo dati)
        /// </summary>
        /// <param name="_CLA">Byte CLA</param>
        /// <param name="_INS">Byte INS</param>
        /// <param name="_P1">Byte P1</param>
        /// <param name="_P2">Byte P2</param>
        /// <param name="_Data">Campo Dati</param>
        public Apdu(Byte _CLA, Byte _INS, Byte _P1, Byte _P2, byte[] _Data)
		{
			INS=_INS;
			CLA=_CLA;
			P1=_P1;
			P2=_P2;
			Data=_Data;
            LE = null;
		}

        /// <summary>
        /// Costruisce un APDU caso 2 (LE)
        /// </summary>
        /// <param name="_CLA">Byte CLA</param>
        /// <param name="_INS">Byte INS</param>
        /// <param name="_P1">Byte P1</param>
        /// <param name="_P2">Byte P2</param>
        /// <param name="_LE">Lunghezza aspettata</param>
        public Apdu(Byte _CLA, Byte _INS, Byte _P1, Byte _P2, Byte _LE)
		{
			INS=_INS;
			CLA=_CLA;
			P1=_P1;
			P2=_P2;
            Data = null;
            LE = _LE;
		}

        /// <summary>
        /// Costruisce un APDU caso 1 (Solo header)
        /// </summary>
        /// <param name="_CLA">Byte CLA</param>
        /// <param name="_INS">Byte INS</param>
        /// <param name="_P1">Byte P1</param>
        /// <param name="_P2">Byte P2</param>
        public Apdu(Byte _CLA, Byte _INS, Byte _P1, Byte _P2)
		{
			INS=_INS;
			CLA=_CLA;
			P1=_P1;
			P2=_P2;
			LE=0;
			Data=null;
            LE=null;
		}

        /// <summary>
        /// Converte l'Apdu una stringa esadecimale
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String txt = String.Format("{0:X2} {1:X2} {2:X2} {3:X2} ", CLA, INS, P1, P2);
            if (Data != null) {
                txt += Data.Length.ToString("X2")+ " ";
                foreach (byte b in Data) {
                    txt += b.ToString("X2") + " ";
                }
            }
            if (LE.HasValue)
                txt += LE.Value.ToString("X2");
            return txt;
        }

        /// <summary>
        /// Ritorna un'Apdu rappresentata come sequenza di bytes, gestendo anche il caso di Campo dati di lunghezza maggiore di 256 bytes
        /// (Extended Apdu)
        /// </summary>
        /// <returns>L'array di bytes che rappresenta l'Apdu</returns>
        public byte[] GetBytes() {
            // devo gestire le extended APDU;
            byte[] pbtAPDU = null;
            if (Data == null || Data.Length < 256)
            {
                int iAPDUSize = 4;
                if (Data != null)
                    iAPDUSize += Data.Length + 1;
                if (LE.HasValue)
                    iAPDUSize++;

                pbtAPDU = new byte[iAPDUSize];
                pbtAPDU[0] = CLA;
                pbtAPDU[1] = INS;
                pbtAPDU[2] = P1;
                pbtAPDU[3] = P2;
                if (Data != null && LE.HasValue)
                {
                    pbtAPDU[4] = (byte)Data.Length;
                    Data.CopyTo(pbtAPDU, 5);
                    pbtAPDU[5 + Data.Length] = (byte)LE;
                }
                else if (Data != null && !LE.HasValue)
                {
                    pbtAPDU[4] = (byte)Data.Length;
                    Data.CopyTo(pbtAPDU, 5);
                }
                else if (Data == null && LE.HasValue)
                {
                    pbtAPDU[4] = (byte)LE;
                }
            }
            else
            {
                // è una apdu estesa
                int iAPDUSize = 7 + Data.Length;
                if (LE.HasValue)
                    iAPDUSize += 2;

                pbtAPDU = new byte[iAPDUSize];
                pbtAPDU[0] = CLA;
                pbtAPDU[1] = INS;
                pbtAPDU[2] = P1;
                pbtAPDU[3] = P2;
                pbtAPDU[4] = (byte)(Data.Length >> 16);
                pbtAPDU[5] = (byte)((Data.Length >> 8) & 0xff);
                pbtAPDU[6] = (byte)(Data.Length & 0xff);
                Data.CopyTo(pbtAPDU, 7);
                if (LE.HasValue)
                {
                    pbtAPDU[7 + Data.Length] = (byte)(LE >> 8);
                    pbtAPDU[8 + Data.Length] = (byte)(LE & 0xff);
                }
            }
            return pbtAPDU;
        }

        /// <summary>
        /// Costruisce un oggetto Apdu che rappresenta un comando di ReadBinary
        /// </summary>
        /// <param name="start">Offset dall'inizio dell'EF da cui leggere</param>
        /// <param name="size">Numero di bytes da leggere</param>
        /// <returns>L'Apdu di lettura</returns>
        public static Apdu ReadBinary(int start, byte size)
        {
            return new Apdu(0x00, 0xB0, (byte)(start >> 8), (byte)(start & 0xff), size);
        }
    }
}
