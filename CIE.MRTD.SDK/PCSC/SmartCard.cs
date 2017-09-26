using CIE.MRTD.SDK.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace CIE.MRTD.SDK.PCSC
{
	/// <summary>
    /// Stato di una Smartcard. Vedi la struttura <a href="https://msdn.microsoft.com/it-it/library/windows/desktop/aa379808(v=vs.85).aspx">SCARD_READERSTATE </a>
	/// </summary>
    [Flags]
	public enum Status : uint 
	{
        SCARD_STATE_UNAWARE =	0x0000,	/* App wants status */
        SCARD_STATE_IGNORE	=	0x0001,	/* Ignore this reader */
        SCARD_STATE_CHANGED	=	0x0002,	/* State has changed */
        SCARD_STATE_UNKNOWN	=	0x0004,	/* Reader unknown */
        SCARD_STATE_UNAVAILABLE	=	0x0008,	/* Status unavailable */
        SCARD_STATE_EMPTY	=	0x0010,	/* Card removed */
        SCARD_STATE_PRESENT	=	0x0020,	/* Card inserted */
        SCARD_STATE_EXCLUSIVE	=	0x0080,	/* Exclusive Mode */
        SCARD_STATE_INUSE =		0x0100,	/* Shared Mode */
        SCARD_STATE_MUTE =		0x0200,	/* Unresponsive card */
        SCARD_STATE_UNPOWERED	=	0x0400	/* Unpowered card */
	}

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct SCARD_READERSTATE
	{
		internal string szReader;
		internal IntPtr pvUserData;
		internal uint dwCurrentState;
		internal uint dwEventState;
		internal uint cbAtr;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=0x24, ArraySubType=UnmanagedType.U1)]
		internal byte[] rgbAtr;
	}

	[StructLayout(LayoutKind.Sequential)] 
	internal class SCARD_IO_REQUEST
	{
        [MarshalAs(UnmanagedType.U4)]
		internal protected Protocol Protocol;
        [MarshalAs(UnmanagedType.U4)]
        internal UInt32 PciLength = 0;
		public SCARD_IO_REQUEST()
		{
			Protocol = Protocol.SCARD_PROTOCOL_UNDEFINED;
		}
	}

    internal enum Scope : uint
	{
		SCARD_SCOPE_USER=0,
		SCARD_SCOPE_TERMINAL=1,
		SCARD_SCOPE_SYSTEM=2
	};

    /// <summary>
    /// Modalità di sharing di una smart card. Vedi la funzione <a href="https://msdn.microsoft.com/it-it/library/windows/desktop/aa379473(v=vs.85).aspx">SCardConnect</a>
    /// </summary>
    public enum Share : uint
	{
		SCARD_SHARE_EXCLUSIVE=1,
		SCARD_SHARE_SHARED=2,
		SCARD_SHARE_DIRECT=3
	}

    /// <summary>
    /// Protocollo di comunicazione con una smart card. Vedi la funzione <a href="https://msdn.microsoft.com/it-it/library/windows/desktop/aa379473(v=vs.85).aspx">SCardConnect</a>
    /// </summary>
    public enum Protocol : uint
	{
		SCARD_PROTOCOL_UNDEFINED=0x00000000,
		SCARD_PROTOCOL_T0=0x00000001,
		SCARD_PROTOCOL_T1=0x00000002,
		SCARD_PROTOCOL_T0orT1 = 0x00000003,
		SCARD_PROTOCOL_RAW = 0x00010000
	}

    /// <summary>
    /// Azione da intraprendere al momento della chiusura della connessione. Vedi la funzione <a href="https://msdn.microsoft.com/it-it/library/windows/desktop/aa379475(v=vs.85).aspx">SCardDisconnect</a>
    /// </summary>
    public enum Disposition : uint 
	{
		SCARD_LEAVE_CARD=0,
		SCARD_RESET_CARD=1,
		SCARD_UNPOWER_CARD=2,
		SCARD_EJECT_CARD=3
	}

    /// <summary>
    /// Delegate chiamata dalla classe Smartcard per gli eventi di inserimento e rimozione carte dai lettori
    /// </summary>
    /// <param name="reader">Nome del lettore in cui è avvenuto l'evento</param>
	public delegate void CardEventHandler(String reader);

    /// <summary>
    /// Log dell'invio di un comando Apdu
    /// </summary>
    public class CommandLog {
        /// <summary>
        /// Array di byte del comando Apdu
        /// </summary>
        public byte[] Command;
        /// <summary>
        /// Array di byte della risposta inviata dalla smart card
        /// </summary>
        public byte[] Response;
        /// <summary>
        /// Ultima Status Word letta dall'oggetto Smartcard
        /// </summary>
        public uint LastSCardResult;
        /// <summary>
        /// Il codice di errore di windows nell'invio dell'Apdu alla smart card
        /// </summary>
        public int LastWinError;
        /// <summary>
        /// La status word della risposta all'Apdu
        /// </summary>
        public ushort SW;
    }

    /// <summary>
    /// Tipo di comunicazione loggata dalla smart card
    /// </summary>
    public enum CommType {
        /// <summary>Apdu inviata alla smart card</summary>
        APDU,
        /// <summary>Risposta ricevuta dalla smart card</summary>
        Response
    };

    /// <summary>
    /// Delegate chiamata dalla classe Smartcard per gli eventi di invio Apdu e ricezione risposta
    /// </summary>
    /// <param name="type">Tipo di comunicazione</param>
    /// <param name="buffer">Dati inviat io ricevuti</param>
    public delegate void SmarCardCommunicationDelegate(CommType type, byte[] buffer);
    
    /// <summary>
    /// L'oggetto SmatCard permette di effettuare operazioni sui lettori di smart card, monitorare l'inserimento e
    /// la rimozione di una smart card, aprire un connessione e inviare comandi.
    /// </summary>
    public class SmartCard : IDisposable
	{
		static SCARD_IO_REQUEST rgSCardT0Pci=null;
		static SCARD_IO_REQUEST rgSCardT1Pci=null;

        /// <summary>
        /// Evento generato all'inserimento di una smart card in un lettore
        /// </summary>
		public event CardEventHandler onInsertCard;
        /// <summary>
        /// Evento generato alla rimozione di una smart card da un lettore
        /// </summary>
		public event CardEventHandler onRemoveCard;
        /// <summary>
        /// Evento generato alla variazione dell'elenco dei lettori di sart card collegati al sistema
        /// </summary>
		public event CardEventHandler onReadersChange;
        /// <summary>
        /// Evento generato durante la comunicazione con la smart card
        /// </summary>
        public event SmarCardCommunicationDelegate SmarCardCommunication;

		[DllImport("winscard.dll")]
		static extern uint SCardControl(IntPtr hCard,uint controlCode,byte[] inBuffer,int inBufferSize,byte[] outBuffer,int outBufferSize,ref int bytesReturned);
		[DllImport("winscard.dll")]
		static extern uint SCardGetAttrib(IntPtr hCard,uint AttrId,byte[] Attrib,ref int AttribLen);
        [DllImport("winscard.dll", SetLastError = true)]
        static extern uint SCardIsValidContext(IntPtr context);
        [DllImport("winscard.dll")]
		static extern uint SCardEstablishContext(Scope Scope,IntPtr  reserved1,IntPtr reserved2, out IntPtr context);
		[DllImport("winscard.dll", EntryPoint="SCardReconnect")]
		static extern uint SCardReconnect(IntPtr handle,Share ShareMode,Protocol PreferredProtocols,Disposition Disposition,out Protocol ActiveProtocol);
		[DllImport("winscard.dll", EntryPoint="SCardConnectA", CharSet=CharSet.Ansi)]
		static extern uint SCardConnect(IntPtr context,String reader,Share ShareMode,Protocol PreferredProtocols,out IntPtr cardHandle,out Protocol ActiveProtocol);
		[DllImport("winscard.dll", EntryPoint="SCardListReadersA", CharSet=CharSet.Ansi)]
		static extern uint SCardListReaders(IntPtr hContext,byte[] mszGroups,byte[] mszReaders,ref UInt32 pcchReaders);
		[DllImport("winscard.dll")]
		static extern uint SCardDisconnect(IntPtr hCard, Disposition Disposition);
		[DllImport("winscard.dll")]
		static extern uint SCardBeginTransaction(IntPtr hCard);
		[DllImport("winscard.dll")]
		static extern uint SCardEndTransaction(IntPtr hCard, Disposition Disposition);
		[DllImport("winscard.dll")]
		static extern uint SCardTransmit(IntPtr hCard, SCARD_IO_REQUEST pioSendPci, byte[] pbSendBuffer, int cbSendLength, SCARD_IO_REQUEST pioRecvPci, byte[] pbRecvBuffer, ref int pcbRecvLength);
		[DllImport("winscard.dll", EntryPoint="SCardGetStatusChangeW", CharSet=CharSet.Ansi)]
		static extern uint SCardGetStatusChange(IntPtr hContext, int Timeout, [In, Out]  SCARD_READERSTATE[] rgReaderStates, int cReaders);
		[DllImport("winscard.dll")]
		static extern uint SCardCancel(IntPtr hContext);
		[DllImport("kernel32.dll")]
		private extern static void FreeLibrary(IntPtr handle) ;
        [ DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr handle, string procName);
        [ DllImport("kernel32.dll")]
		private extern static IntPtr LoadLibrary(String lib) ;

        /// <summary>
        /// Azione da intraprendere al momento del Dispose se la connessione è ancora attiva
        /// </summary>
        public Disposition DispositionOnDispose { get; set; }
        /// <summary>
        /// Specifica se mantenere la connessione aperta quando viene eseguito il Dispose
        /// </summary>
        public bool KeepConnectionOpen { get; set; }
        /// <summary>
        /// Specifica se mantenere la transazione aperta quando viene eseguito il Dispose
        /// </summary>
        public bool KeepTransactionOpen { get; set; }

		static IntPtr context=IntPtr.Zero;
		internal IntPtr contextReaders=IntPtr.Zero;
        internal IntPtr cardHandle = IntPtr.Zero;
        internal Protocol activeProtocol;
        internal Thread monitorThread = null;
        internal Thread monitorReadersThread = null;
		internal String[] monitoredReaders=null;
		internal uint ris;
		internal byte[] ATR;
        internal ISynchronizeInvoke interfaceForEvents = null;

        /// <summary>
        /// Se viene valorizzato, gli eventi di inserimento e rimozione smart card vengono chiamati tramite Invoke su questo oggetto
        /// </summary>
        public ISynchronizeInvoke InterfaceForEvents {
            get { return interfaceForEvents; }
            set { interfaceForEvents = value; }
        }
        bool abortMonitoring = false;
		bool abortMonitoringReaders=false;
		Mutex mutexMonitoring=new Mutex();
		Mutex mutexMonitoringReaders=new Mutex();

        internal List<CommandLog> Log = null;
        bool logEnabled = false;

        /// <summary>
        /// Se il Log è abilitato, tutti i comandi Apdu vengono memorizzati in una lista e possono essere consultati
        /// </summary>
        public bool LogEnabled { set {
            logEnabled = value;
            if (value)
                Log = new List<CommandLog>();
            else
                Log = null;
        } }

        /// <summary>
        /// Se il log è abilitato ritorna la lista dei comandi inviati alla smart card e le relative risposte
        /// </summary>
        /// <returns>Lal ista dei comandi inviati</returns>
        public List<CommandLog> GetLog() { return Log; }

        /// <summary>
        /// Ritorna true se c'è un monitoraggio di lettori in corso
        /// </summary>
        public bool IsMonitoringReaders { get { return monitorReadersThread != null; } }

        /// <summary>
        /// Inizia il monitoraggio dei lettori. Viene sollevato l'evento onReadersChange quando vengono collegati o scollegati
        /// dei lettori di smart card.
        /// Il monitoraggio viene avviato in modalità asincrona, all'interno di un thread specifico. Il thread rimane in esecuzione
        /// finché non viene chiamata StopMonitoring.
        /// </summary>
        public void StartMonitoringReaders() 
		{
			mutexMonitoringReaders.WaitOne();
			ThreadStart start=new ThreadStart(MonitoringReadersThread);
			monitorReadersThread=new Thread(start);
			monitorReadersThread.Name="MonitorReaders"+threadCount.ToString();
			threadCount++;
			monitorReadersThread.Start();
			mutexMonitoringReaders.ReleaseMutex();
		}
		
        /// <summary>
        /// Ferma il monitoraggio dei lettori. Equivalente a StopMonitoringReaders(false) 
        /// </summary>
		public void StopMonitoringReaders() 
		{
			StopMonitoringReaders(false);
		}

        /// <summary>
        /// Restituisce lo stato della smart card nel lettore specificato
        /// </summary>
        /// <param name="reader">Il nome del lettore (ritornato da ListReaders)</param>
        /// <returns>Lo stato del lettore</returns>
        public Status GetState(string reader)
        {
            SCARD_READERSTATE[] rgReaderStates = new SCARD_READERSTATE[1];
            rgReaderStates[0] = new SCARD_READERSTATE();
            rgReaderStates[0].szReader = reader;
            rgReaderStates[0].pvUserData = IntPtr.Zero;
            rgReaderStates[0].dwCurrentState = 0;
            rgReaderStates[0].dwEventState = 0;
            rgReaderStates[0].cbAtr = 0x24;
            rgReaderStates[0].rgbAtr = new byte[0x24];
            SCardGetStatusChange(context, 0, rgReaderStates, 1);
            return (Status)(rgReaderStates[0].dwEventState & 0xffff & (~(uint)Status.SCARD_STATE_CHANGED));
        }

        /// <summary>
        /// Restituisce l'ATR della smart card nel lettore specificato
        /// </summary>
        /// <param name="reader">Il nome del lettore (ritornato da ListReaders)</param>
        /// <returns>L'ATR del chip</returns>
        public byte[] GetATR(string reader)
        { 
			SCARD_READERSTATE[] rgReaderStates=new SCARD_READERSTATE[1];
			rgReaderStates[0]=new SCARD_READERSTATE();
			rgReaderStates[0].szReader = reader;
			rgReaderStates[0].pvUserData = IntPtr.Zero;
			rgReaderStates[0].dwCurrentState = 0;
			rgReaderStates[0].dwEventState = 0;
			rgReaderStates[0].cbAtr = 0x24;
			rgReaderStates[0].rgbAtr = new byte[0x24];
            SCardGetStatusChange(context, 0, rgReaderStates, 1);
            return new ByteArray(rgReaderStates[0].rgbAtr).Left((int)rgReaderStates[0].cbAtr);
        }

        /// <summary>
        /// Ferma il monitoraggio dei lettori, specificando se attendere il termine degli handler degli eventi
        /// </summary>
        /// <param name="Wait">true per attendere il ritorno delle routine di gestione degli eventi, false altrimenti</param>
        public void StopMonitoringReaders(bool Wait) 
		{
			Thread tempMonitor=monitorReadersThread;
			if (monitorReadersThread!=null) 
			{
				mutexMonitoringReaders.WaitOne();
				if (monitorReadersThread!=null) 
				{
					Debug.Write("abortMonitoringReaders true");
					abortMonitoringReaders=true;
					SCardCancel(contextReaders);
					monitorReadersThread=null;
				}
				mutexMonitoringReaders.ReleaseMutex();
				if (Wait) 
				{
					Debug.Write("Joining:"+tempMonitor.Name);
					tempMonitor.Join();
					Debug.Write("abortMonitoringReaders false");
					abortMonitoringReaders=false;
				}
			}
		}

		private void MonitoringReadersThread() 
		{
			SCARD_READERSTATE[] rgReaderStates=new SCARD_READERSTATE[1];
			rgReaderStates[0]=new SCARD_READERSTATE();
			rgReaderStates[0].szReader = @"\\?PNP?\Notification";
			rgReaderStates[0].pvUserData = IntPtr.Zero;
			rgReaderStates[0].dwCurrentState = 0;
			rgReaderStates[0].dwEventState = 0;
			rgReaderStates[0].cbAtr = 0x24;
			rgReaderStates[0].rgbAtr = new byte[0x24];
			SCardGetStatusChange(context,0,rgReaderStates, 1);
			rgReaderStates[0].dwCurrentState = rgReaderStates[0].dwEventState & (~(uint)2);
			while(!abortMonitoringReaders)
			{
				ris=SCardGetStatusChange(contextReaders,2147483647,rgReaderStates, 1);
				uint change=rgReaderStates[0].dwEventState ^ rgReaderStates[0].dwCurrentState;
				rgReaderStates[0].dwCurrentState = rgReaderStates[0].dwEventState & (~(uint)2);
				if (ris==0) 
				{
					Debug.Write("change:"+change.ToString("X8"));
					if (change==0x30002)
					{
						Thread.Sleep(1000);
						invokeReadersChange("");
					}
				}
			}
			mutexMonitoringReaders.WaitOne();
			Debug.Write("abortMonitoringReaders false");
			abortMonitoringReaders=false;
			monitorReadersThread=null;
			mutexMonitoringReaders.ReleaseMutex();
		}

        /// <summary>
        /// Costruisce un oggetto smart card
        /// </summary>
		public SmartCard()
		{
            KeepConnectionOpen = false;
            KeepTransactionOpen = false;
            DispositionOnDispose = Disposition.SCARD_RESET_CARD;

			if (context==IntPtr.Zero || SCardIsValidContext(context)!=0) 
			{
				SCardEstablishContext(Scope.SCARD_SCOPE_SYSTEM,IntPtr.Zero,IntPtr.Zero, out context);
			}
            if (contextReaders == IntPtr.Zero || SCardIsValidContext(contextReaders)!=0) 
			{
				SCardEstablishContext(Scope.SCARD_SCOPE_SYSTEM,IntPtr.Zero,IntPtr.Zero, out contextReaders);
			}
			if (rgSCardT0Pci ==null) {
				IntPtr handle = LoadLibrary("Winscard.dll") ;
                rgSCardT0Pci = (SCARD_IO_REQUEST)Marshal.PtrToStructure(GetProcAddress(handle, "g_rgSCardT0Pci"), typeof(SCARD_IO_REQUEST));
                rgSCardT1Pci = (SCARD_IO_REQUEST)Marshal.PtrToStructure(GetProcAddress(handle, "g_rgSCardT1Pci"), typeof(SCARD_IO_REQUEST));
				FreeLibrary(handle) ;
			}
		}

        /// <summary>
        /// Invia un comando di controllo a basso livello ad un lettore di smart card connesso. Vedi la funzione <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa379474(v=vs.85).aspx">SCardControl</a>
        /// </summary>
        /// <param name="controlCode">Codice di controllo</param>
        /// <param name="inBuffer">Dati richiesti per eseguire l'operazione</param>
        /// <returns>Dati ritornati dal lettore</returns>
		public byte[] Control(uint controlCode,byte[] inBuffer)
		{
			byte[] outBuf=new byte[1000];
			int bufLen=0;
			ris=SCardControl(cardHandle,controlCode,inBuffer,inBuffer.Length,outBuf,outBuf.Length,ref bufLen);
			if (ris!=0)
				return null;
			byte[] resp=new byte [bufLen];
			Array.Copy(outBuf,0,resp,0,bufLen);
			return resp;
		}

        /// <summary>
        /// Richiede un attributo del lettore di smart card connesso. Vedi la funzione <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa379559(v=vs.85).aspx">SCardGetAttrib</a>
        /// </summary>
        /// <param name="attrib">Valore dell'attributo da richiedere</param>
        /// <returns>Buffer contenente la risposta del lettore di smart card</returns>
        public byte[] GetAttrib(uint attrib) 
		{
			int AttrLen=0;
			ris=SCardGetAttrib(cardHandle,attrib,null,ref AttrLen);
			if (ris!=0)
				return null;
			byte[] Attr=new byte [AttrLen];
			ris=SCardGetAttrib(cardHandle,attrib,Attr,ref AttrLen);
			if (ris!=0)
				return null;
			return Attr;
		}

        /// <summary>
        /// Effettua una riconnessione ad un lettore di smart card già connesso, applicando la Diposition specificata
        /// </summary>
        /// <param name="ShareMode">Modalità di sharing</param>
        /// <param name="PreferredProtocols">Protocollo utilizzato</param>
        /// <param name="Disposition">Disposition alla chisura della connessione</param>
        /// <returns>true se la riconnessione è stata effettuata correttamente, false altrimenti</returns>
		public bool Reconnect(Share ShareMode,Protocol PreferredProtocols,Disposition Disposition) 
		{
			ris=SCardReconnect(cardHandle,ShareMode,PreferredProtocols,Disposition,out activeProtocol);
            if (ris == 0x80100002)
                ris = SCardReconnect(cardHandle, ShareMode, PreferredProtocols, Disposition, out activeProtocol);

			if (ris!=0)
				return false;
			return true;
		}

		internal string connectedReader=null;

        /// <summary>
        /// Restituisce il nome del lettore attualmente connesso
        /// </summary>
        public string ConnectedReader { get { return connectedReader; } }

        /// <summary>
        /// Effettua una connessione ad un lettore di smart card
        /// </summary>
        /// <param name="reader">Nome del lettore a cui connettersi (ritornato da ListReaders)</param>
        /// <param name="ShareMode">Modalità di sharing</param>
        /// <param name="PreferredProtocols">Protocollo utilizzato</param>
        /// <returns>true se la connessione è stata effettuata correttamente, false altrimenti </returns>

		public bool Connect(String reader,Share ShareMode,Protocol PreferredProtocols) 
		{
            LastSCardResult = 0;
            LastWinError = 0;
            ris = SCardConnect(context, reader, ShareMode, PreferredProtocols, out cardHandle, out activeProtocol);
            if (ris==0x80100002)
			ris=SCardConnect(context,reader,ShareMode,PreferredProtocols,out cardHandle,out activeProtocol);

			if (ris!=0)
            {
                LastSCardResult = ris;
                LastWinError = Marshal.GetLastWin32Error();
				return false;
            }

			connectedReader=reader;
			SCARD_READERSTATE[] rgReaderStates=new SCARD_READERSTATE[1];

			rgReaderStates[0].szReader = reader;
			rgReaderStates[0].pvUserData = IntPtr.Zero;
			rgReaderStates[0].dwCurrentState = 0;
			rgReaderStates[0].dwEventState = 0;
			rgReaderStates[0].cbAtr = 0x24;
			rgReaderStates[0].rgbAtr = new byte[0x24];
			ris=SCardGetStatusChange(context, 0, rgReaderStates, 1);

			if (ris != 0)
            {
                LastSCardResult = ris;
                LastWinError = Marshal.GetLastWin32Error();
				return false;
            }

			ATR=new byte[rgReaderStates[0].cbAtr];
			Array.Copy(rgReaderStates[0].rgbAtr,0,ATR,0,rgReaderStates[0].cbAtr);

			return true;
		}

        /// <summary>
        /// Effettua una disconnessione da un lettore di smart card, applicando la Diposition specificata
        /// </summary>
        /// <param name="Disposition">Disposition alla chisura della connessione</param>
        public void Disconnect(Disposition Disposition)
		{
			if (cardHandle!=IntPtr.Zero)
				SCardDisconnect(cardHandle,Disposition);
            inTransaction = false;
			cardHandle=IntPtr.Zero;
			connectedReader=null;
			ATR = null;
		}

        /// <summary>
        /// Ritrorna la lista dei lettori di smart card collegati al sistema
        /// </summary>
        /// <returns>Una lista di stringhe contenente i nomi dei lettori</returns>
		public String[] ListReaders() 
		{
            LastSCardResult = 0;
            LastWinError = 0;
			
			UInt32 pcchReaders=0;
			uint ris=SCardListReaders(context,null,null,ref pcchReaders);
            if (ris != 0)
            {
                LastSCardResult = ris;
                LastWinError = Marshal.GetLastWin32Error();
                return null;
            }

			byte[] mszReaders = new byte[pcchReaders];
            ris = SCardListReaders(context, null, mszReaders, ref pcchReaders);
            if (ris != 0)
            {
                LastSCardResult = ris;
                LastWinError = Marshal.GetLastWin32Error();
                return null;
            }
			System.Text.ASCIIEncoding asc = new System.Text.ASCIIEncoding();
			String[] Readers = asc.GetString( mszReaders ).Split( '\0' );
			if (Readers.Length>2) 
			{
				String[] res=new String[Readers.Length-2];
				int j=0;
				for (int i=0;i<Readers.Length;i++) 
				{
					if (Readers[i]!="" && Readers[i]!=null) 
					{
						res[j]=Readers[i];
						j++;
					}
				}
                return res;
			}
			else 
			{
                return new String[0];
			}
		}
        bool inTransaction = false;

        /// <summary>
        /// Inizia una transazione su una connessione esistente. All'interno della transazione, anche se lo ShareMode è impostato su SCARD_SHARE_SHARED,
        /// altre applicazioni non possono accedere alla smart card. Da Windows 8 in poi, se la transazione rimane appesa per 5 secondi 
        /// senza attività sulla smart card, viene automaticamente disconnessa
        /// </summary>
        /// <returns>true se la transazione è iniziata corretamente, false altrimenti</returns>
		public bool BeginTransaction() 
		{
            LastSCardResult = 0;
            LastWinError = 0;

			ris=SCardBeginTransaction(cardHandle);
			if (ris==0)
            {
                inTransaction = true;
				return true;
            }
            else
            {
                LastSCardResult = ris;
                LastWinError = Marshal.GetLastWin32Error();
			return false;
		}
		}

        /// <summary>
        /// Termina una transazione su una smart card, applicando la Diposition specificata
        /// </summary>
        /// <param name="Disposition">Disposition alla chisura della connessione</param>
        public void EndTransaction(Disposition Disposition) 
		{
            LastSCardResult = 0;
            LastWinError = 0;

			uint ris=SCardEndTransaction(cardHandle,Disposition);
            if (ris != 0)
            {
                LastSCardResult = ris;
                LastWinError = Marshal.GetLastWin32Error();
            }
            inTransaction = false;
		}			

        /// <summary>
        /// Trasmette un comando rappresentato da un oggetto Apdu ad una smart card
        /// </summary>
        /// <param name="apdu">L'Apdu da trasmettere</param>
        /// <returns>La Status Word ritornata dalla Smart Card</returns>
		public uint Transmit(Apdu apdu)
		{
			byte[] resp=null;
			byte[] APDUbytes = apdu.GetBytes();
			return Transmit(APDUbytes, ref resp);
		}

        /// <summary>
        /// Trasmette un comando rappresentato da un oggetto Apdu ad una smart card
        /// </summary>
        /// <param name="apdu">L'Apdu da trasmettere</param>
        /// <param name="resp">Il buffer in cui verrà memorizzata la risposta della smart card</param>
        /// <returns>La Status Word ritornata dalla Smart Card</returns>
        public uint Transmit(Apdu apdu, ref byte[] resp)
		{
			byte[] APDUbytes = apdu.GetBytes();
			return Transmit(APDUbytes, ref resp);
		}

        /// <summary>
        /// Trasmette un comando rappresentato da un'array di bytes ad una smart card
        /// </summary>
        /// <param name="SendBuffer">Il coando da trasmettere</param>
        /// <returns>La Status Word ritornata dalla Smart Card</returns>
		public uint Transmit(byte[] SendBuffer)
		{
			byte[] resp=null;
			return Transmit(SendBuffer,ref resp);
		}

        /// <summary>
        /// Ritorna l'ultima Status Word ottenuta dalla Smart card
        /// </summary>
        public uint LastSCardResult { get; set; }

        /// <summary>
        /// Ritorna l'ultimo codice di errore di Windows ottenuto durante la trasmissione
        ///di un'Apdu alla smart card
        /// </summary>
        public int LastWinError { get; set; }

        /// <summary>
        /// Trasmette un comando rappresentato da un'array di bytes ad una smart card
        /// </summary>
        /// <param name="SendBuffer">L'Apdu da trasmettere</param>
        /// <param name="resp">Il buffer in cui verrà memorizzata la risposta della smart card</param>
        /// <returns>La Status Word ritornata dalla Smart Card</returns>
        public uint Transmit(byte[] SendBuffer, ref byte[] resp)
        {
            ushort SW = 0;
            try
            {
                LastSCardResult = 0;
                LastWinError = 0;

                SCARD_IO_REQUEST req;
                if (activeProtocol == Protocol.SCARD_PROTOCOL_T0)
                    req = rgSCardT0Pci;
                else
                    req = rgSCardT1Pci;
                byte[] RecvBuffer = new byte[1100];
                int RecvLength = 1100;

                if (SmarCardCommunication != null)
                    SmarCardCommunication(CommType.APDU, SendBuffer);

                ris = SCardTransmit(cardHandle, req, SendBuffer, SendBuffer.Length, null, RecvBuffer, ref RecvLength);
                if (ris != 0)
                {
                    LastSCardResult = ris;
                    LastWinError = Marshal.GetLastWin32Error();

                    return 0;
                }

                if (SmarCardCommunication != null)
                {
                    resp = new byte[RecvLength];
                    Array.Copy(RecvBuffer, resp, RecvLength);
                    SmarCardCommunication(CommType.Response, resp);
                }

                resp = new byte[RecvLength - 2];
                Array.Copy(RecvBuffer, resp, RecvLength - 2);
                SW = (ushort)(RecvBuffer[RecvLength - 2] << 8 | RecvBuffer[RecvLength - 1]);
                return (uint)SW;
            }
            finally
            {
                if (logEnabled)
                    Log.Add(new CommandLog()
                    {
                        Command = new ByteArray(SendBuffer.Clone() as byte[]),
                        Response = new ByteArray(resp.Clone() as byte[]),
                        LastSCardResult = LastSCardResult,
                        LastWinError = LastWinError,
                        SW = SW
                    });
            }
        }

        /// <summary>
        /// Forza l'interruzioni di altre azioni in attesa della smart card all'interno del Context della connessione corrente. Vedi la funzione <a href="https://msdn.microsoft.com/it-it/library/windows/desktop/aa379470(v=vs.85).aspx">SCardCancel</a>
        /// </summary>
		public void Cancel() 
		{
            LastSCardResult = 0;
            LastWinError = 0;

			uint ris=SCardCancel(context);
            if (ris != 0)
            {
                LastSCardResult = ris;
                LastWinError = Marshal.GetLastWin32Error();
            }
		}

		static int threadCount=0;

        /// <summary>
        /// Inizia il monitoraggio delle smart card. Viene sollevato l'evento onInsertCard e onRemoveCard all'inserimento e alla rimozione
        /// di una smart card. Corrisponde a StartMonitoring(true)
        /// </summary>
        public void StartMonitoring() {
            StartMonitoring(true);
        }

        /// <summary>
        /// Ritorna true se c'è un monitoraggio di smart card in corso
        /// </summary>
        public bool IsMonitoring { get { return monitorThread != null; } }


        /// <summary>
        /// Inizia il monitoraggio delle smart card. Viene sollevato l'evento onInsertCard e onRemoveCard all'inserimento e alla rimozione
        /// di una smart card. Se una smart card è già presente al momento della chiamata, l'evento onInsertCard viene sollevato se
        /// initialEvent è impostato a true
        /// Il monitoraggio viene avviato in modalità asincrona, all'interno di un thread specifico. Il thread rimane in esecuzione
        /// finché non viene chiamata StopMonitoring.
        /// </summary>
        /// <param name="initialEvent">Se è true viene sollevato l'evento di insert se la smart card è già inserita</param>
        public void StartMonitoring(bool initialEvent)
		{
			Debug.Write("ListingReaders");
			String []readerName=ListReaders();
            if (readerName==null || readerName.Length == 0)
                throw new Exception("Nessun lettore di smart card presente");
			mutexMonitoring.WaitOne();
			if (monitorThread!=null)
				return;
			monitoredReaders=readerName;
            ParameterizedThreadStart start = new ParameterizedThreadStart(MonitoringThread);
			monitorThread=new Thread(start);
			monitorThread.Name="Monitor"+threadCount.ToString();
			threadCount++;
            if (initialEvent)
                MonitoringThread(true);
            if (monitorThread!=null)
			    monitorThread.Start(false);
			mutexMonitoring.ReleaseMutex();
		}

        /// <summary>
        /// Verifica se c'è una smart card all'interno del lettore specificato
        /// </summary>
        /// <param name="reader">Il nome del lettore (ritornato da ListReaders)</param>
        /// <returns></returns>
        public bool IsCardInReader(string reader)
        {
            LastSCardResult = 0;
            LastWinError = 0;

			SCARD_READERSTATE[] rs=new SCARD_READERSTATE[1];
			rs[0]=new SCARD_READERSTATE();
			rs[0].dwCurrentState=0;
			rs[0].szReader=reader;
			uint ris=SCardGetStatusChange(context,0,rs,1);
            if (ris != 0)
            {
                LastSCardResult = ris;
                LastWinError = Marshal.GetLastWin32Error();
                return false;
            }
			if ((rs[0].dwEventState & (uint)Status.SCARD_STATE_PRESENT)== (uint)Status.SCARD_STATE_PRESENT)
				return true;
			return 
				false;
		}

        /// <summary>
        /// Inizia il monitoraggio delle smart card sui lettori specificati. Viene sollevato l'evento onInsertCard e onRemoveCard all'inserimento e alla rimozione
        /// di una smart card. Corrisponde a StartMonitoring(readerName,true).
        /// Il monitoraggio viene avviato in modalità asincrona, all'interno di un thread specifico. Il thread rimane in esecuzione
        /// finché non viene chiamata StopMonitoring.
        /// </summary>
        /// <param name="readerName">Array di stringhe contenenti l'elenco dei lettori da monitorare</param>
        public void StartMonitoring(String[] readerName)
        {
            StartMonitoring(readerName, true);
        }

        /// <summary>
        /// Inizia il monitoraggio delle smart card sui lettori specificati. Viene sollevato l'evento onInsertCard e onRemoveCard all'inserimento e alla rimozione
        /// di una smart card. Se una smart card è già presente al momento della chiamata, l'evento onInsertCard viene sollevato se
        /// initialEvent è impostato a true.
        /// Il monitoraggio viene avviato in modalità asincrona, all'interno di un thread specifico. Il thread rimane in esecuzione
        /// finché non viene chiamata StopMonitoring.
        /// </summary>
        /// <param name="readerName">Array di stringhe contenenti l'elenco dei lettori da monitorare</param>
        /// <param name="initialEvent">Se è true viene sollevato l'evento di insert se la smart card è già inserita</param>
        public void StartMonitoring(String[] readerName, bool initialEvent)
		{
			mutexMonitoring.WaitOne();
			if (monitorThread!=null) 
			{
				mutexMonitoring.ReleaseMutex();
				return;
			}
			monitoredReaders=readerName;
            ParameterizedThreadStart start = new ParameterizedThreadStart(MonitoringThread);
			monitorThread=new Thread(start);
			Debug.Write("abortMonitoring false");
			abortMonitoring=false;
            if (initialEvent)
                MonitoringThread(true);
			monitorThread.Start(false);
			mutexMonitoring.ReleaseMutex();
		}

        /// <summary>
        /// Ferma il monitoraggio delle smart card. Equivalente a StopMonitoring(false) 
        /// </summary>
        public void StopMonitoring()
        {
			StopMonitoring(false);
		}

        /// <summary>
        /// Ferma il monitoraggio delle smart card, specificando se attendere il termine degli handler degli eventi
        /// </summary>
        /// <param name="Wait">true per attendere il ritorno delle routine di gestione degli eventi, false altrimenti</param>
        public void StopMonitoring(bool Wait) 
		{
			Thread tempMonitor=monitorThread;
			if (monitorThread!=null) 
			{
				mutexMonitoring.WaitOne();
				if (monitorThread!=null) 
				{
					Debug.Write("abortMonitoring true");
					abortMonitoring=true;
					SCardCancel(context);
					monitorThread=null;
				}
				mutexMonitoring.ReleaseMutex();
				if (Wait) 
				{
                    if (tempMonitor.ThreadState==System.Threading.ThreadState.Running)
					tempMonitor.Join();
					Debug.Write("abortMonitoring false");
					abortMonitoring=false;
				}
			}
		}

		void invokeInsertCard(String reader) {
			if (onInsertCard!=null) 
			{

				if (interfaceForEvents==null || !interfaceForEvents.InvokeRequired) 
				{
					try 
					{
						onInsertCard(reader);
					}
					catch
					{
					}
				}
				else 
				{
					interfaceForEvents.BeginInvoke(new CardEventHandler(invokeInsertCard),new object[]{reader});
				}
			}
		}

		void invokeRemoveCard(String reader) 
		{
			if (onRemoveCard!=null) 
			{

				if (interfaceForEvents==null || !interfaceForEvents.InvokeRequired) 
				{
					try 
					{
						onRemoveCard(reader);
					}
					catch
					{
					}
				}
				else 
				{
					interfaceForEvents.BeginInvoke(new CardEventHandler(invokeRemoveCard),new object[]{reader});
				}
			}
		}
		void invokeReadersChange(string reader) 
		{
			if (onReadersChange!=null) 
			{

				if (interfaceForEvents==null || !interfaceForEvents.InvokeRequired) 
				{
					try 
					{
						onReadersChange(reader);
					}
					catch
					{
					}
				}
				else 
				{
					interfaceForEvents.BeginInvoke(new CardEventHandler(invokeReadersChange),new object[]{reader});
				}
			}
		}

        private void MonitoringThread(object bInitial)
        {
            try
            {
                bool initialEvent = (bool)bInitial;
                Debug.Write("MonitoringThread:readersLen:" + monitoredReaders.Length.ToString());
                if (monitoredReaders.Length == 0)
                    return;
                SCARD_READERSTATE[] rgReaderStates = new SCARD_READERSTATE[monitoredReaders.Length];
                for (int i = 0; i < monitoredReaders.Length; i++)
                {
                    rgReaderStates[i].szReader = monitoredReaders[i];
                    rgReaderStates[i].pvUserData = new IntPtr(i);
                    rgReaderStates[i].dwCurrentState = 0;
                    rgReaderStates[i].dwEventState = 0;
                    rgReaderStates[i].cbAtr = 0x24;
                    rgReaderStates[i].rgbAtr = new byte[0x24];
                }
                ris = SCardGetStatusChange(context, 0, rgReaderStates, rgReaderStates.Length);
                if (initialEvent)
                {
                    for (int i = 0; i < monitoredReaders.Length; i++)
                    {
                        if ((rgReaderStates[i].dwEventState & (uint)Status.SCARD_STATE_PRESENT) == (uint)Status.SCARD_STATE_PRESENT)
                        {
                            invokeInsertCard(rgReaderStates[i].szReader);
                        }
                    }
                    return;
                }

                while (!abortMonitoring)
                {
                    for (int i = 0; i < monitoredReaders.Length; i++)
                    {
                        rgReaderStates[i].dwCurrentState = rgReaderStates[i].dwEventState & (~(uint)Status.SCARD_STATE_CHANGED);
                    }
                    ris = SCardGetStatusChange(context, 2147483647, rgReaderStates, rgReaderStates.Length);

                    for (int i = 0; i < monitoredReaders.Length; i++)
                    {
                        if (rgReaderStates[i].dwCurrentState != rgReaderStates[i].dwEventState)
                        {
                            if ((rgReaderStates[i].dwCurrentState & (uint)Status.SCARD_STATE_PRESENT) != (uint)Status.SCARD_STATE_PRESENT &&
                                (rgReaderStates[i].dwEventState & (uint)Status.SCARD_STATE_PRESENT) == (uint)Status.SCARD_STATE_PRESENT)
                            {
                                invokeInsertCard(rgReaderStates[i].szReader);
                            }
                            if ((rgReaderStates[i].dwCurrentState & (uint)Status.SCARD_STATE_PRESENT) == (uint)Status.SCARD_STATE_PRESENT &&
                                (rgReaderStates[i].dwEventState & (uint)Status.SCARD_STATE_PRESENT) != (uint)Status.SCARD_STATE_PRESENT)
                            {
                                invokeRemoveCard(rgReaderStates[i].szReader);
                            }
                        }
                    }
                }
                mutexMonitoring.WaitOne();
                Debug.Write("abortMonitoring false");
                abortMonitoring = false;
                monitorThread = null;
                mutexMonitoring.ReleaseMutex();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Ritorna una stringa di errore corrispondente ad una Status Word, come definito dall'ISO7816
        /// </summary>
        /// <param name="statusWord">Status word da tradurre in stringa</param>
        /// <returns>Descizione dell'errore</returns>
		public string StatusWordString(uint statusWord) 
		{ 
			String msg="";
			switch (statusWord) 
			{
				case 0x6283: msg="File is deactivated"; break;
				case 0x6285: msg="File is terminated"; break;
				case 0x6300: msg="Authentication failed"; break;
				case 0x6581: msg="EEPROM error; command aborted"; break;
				case 0x6700: msg="LC invalid"; break;
				case 0x6881: msg="Logical channel not supported"; break;
				case 0x6882: msg="SM mode not supported"; break;
				case 0x6884: msg="Chaining error"; break;
				case 0x6981: msg="Command cannot be used for file structure"; break;
				case 0x6982: msg="Required access right not granted"; break;
				case 0x6983: msg="BS object blocked"; break;
				case 0x6984: msg="BS object has invalid format"; break;
				case 0x6985: msg="Conditions of use not satisfied; no random number available"; break;
				case 0x6986: msg="No current EF selected"; break;
				case 0x6987: msg="Key object for SM not found"; break;
				case 0x6988: msg="Key object used for SM has invalid format"; break;
				case 0x6A80: msg="Invalid parameters in data field"; break;
				case 0x6A81: msg="Function / mode not supported"; break;
				case 0x6A82: msg="File not found"; break;
				case 0x6A83: msg="Record / object not found"; break;
				case 0x6A84: msg="Not enough memory in file / in file system"; break;
				case 0x6A85: msg="LC does not fit the TLV structure of the data field"; break;
				case 0x6A86: msg="P1/P2 invalid"; break;
				case 0x6A87: msg="LC does not fit P1/P2"; break;
				case 0x6A88: msg="Object not found (GET DATA)"; break;
				case 0x6A89: msg="File already exists"; break;
				case 0x6A8A: msg="DF name already exists"; break;
				case 0x6C00: msg="LE does not fit the data to be sent"; break;
				case 0x6D00: msg="INS invalid"; break;
				case 0x6E00: msg="CLA invalid (Hi nibble)"; break;
				case 0x6F00: msg="Technical error"; break;
				case 0x6F01: msg="Card life cycle was set to death"; break;
				case 0x6F02: msg="Code file corrupted and terminated"; break;
				case 0x6F81: msg="File is invalid because of checksum error"; break;
				case 0x6F82: msg="Not enough memory available in XRAM"; break;
				case 0x6F83: msg="Transaction error"; break;
				case 0x6F84: msg="General protection fault"; break;
				case 0x6F85: msg="Internal failure of PK-API (wrong CCMS format?)"; break;
				case 0x6F86: msg="Key object not found"; break;
				case 0x6F87: msg="Internal hardware attack detected, change to life cycle death"; break;
				case 0x6F88: msg="Transaction buffer too smal"; break;
				case 0x6FFF: msg="Internal assertion"; break;
				case 0x9000: msg="OK"; break;
				case 0x9001: msg="OK, EEPROM written in second tria"; break;
				case 0x9850: msg="Overflow through INCREASE / Underflow through DECREASE"; break;
				default: msg="Unknown status code"; break;
			}
			return msg;
		}

        /// <summary>
        /// Distruttore dell'oggetto Smartcard
        /// </summary>
        ~SmartCard() {
            if (!disposed)
                Dispose();
        }

        bool disposed = false;
        /// <summary>
        /// Effettua il DIspose di un oggetto. Ferma il monitoraggio di lettori e smart card.
        /// Se la transazione è ancora aperta e KeepTransactionOpen è false la termina
        /// Se la connessione è ancora aperta e KeepConnectionOpen è false la chiude
        /// </summary>
        public void Dispose()
        {
            disposed = true;
            StopMonitoring(true);
            StopMonitoringReaders(true);
            if (!KeepTransactionOpen && inTransaction)
                EndTransaction(DispositionOnDispose);

            if (!KeepConnectionOpen)
                Disconnect(DispositionOnDispose);
        }
    }

    /// <summary>
    /// Attributi di un lettore. Vedi la funzione <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa379559(v=vs.85).aspx">SCardGetAttrib</a>
    /// </summary>
    public enum Attribute : uint
    {
        SCARD_CLASS_VENDOR_INFO = 1,   // Vendor information definitions
        SCARD_CLASS_COMMUNICATIONS = 2,// Communication definitions
        SCARD_CLASS_PROTOCOL = 3,// Protocol definitions
        SCARD_CLASS_POWER_MGMT = 4,// Power Management definitions
        SCARD_CLASS_SECURITY = 5,// Security Assurance definitions
        SCARD_CLASS_MECHANICAL = 6,// Mechanical characteristic definitions
        SCARD_CLASS_VENDOR_DEFINED = 7,// Vendor specific definitions
        SCARD_CLASS_IFD_PROTOCOL = 8,// Interface Device Protocol options
        SCARD_CLASS_ICC_STATE = 9,// ICC State specific definitions
        SCARD_CLASS_PERF = 0x7ffe,// performace counters
        SCARD_CLASS_SYSTEM = 0x7fff,// System-specific definitions

        SCARD_ATTR_VENDOR_NAME = (SCARD_CLASS_VENDOR_INFO << 16) | 0x0100,
        SCARD_ATTR_VENDOR_IFD_TYPE = (SCARD_CLASS_VENDOR_INFO << 16) | 0x0101,
        SCARD_ATTR_VENDOR_IFD_VERSION = (SCARD_CLASS_VENDOR_INFO << 16) | 0x0102,
        SCARD_ATTR_VENDOR_IFD_SERIAL_NO = (SCARD_CLASS_VENDOR_INFO << 16) | 0x0103,
        SCARD_ATTR_CHANNEL_ID = (SCARD_CLASS_COMMUNICATIONS << 16) | 0x0110,
        SCARD_ATTR_PROTOCOL_TYPES = (SCARD_CLASS_PROTOCOL << 16) | 0x0120,
        SCARD_ATTR_ASYNC_PROTOCOL_TYPES = (SCARD_CLASS_PROTOCOL << 16) | 0x0120,
        SCARD_ATTR_DEFAULT_CLK = (SCARD_CLASS_PROTOCOL << 16) | 0x0121,
        SCARD_ATTR_MAX_CLK = (SCARD_CLASS_PROTOCOL << 16) | 0x0122,
        SCARD_ATTR_DEFAULT_DATA_RATE = (SCARD_CLASS_PROTOCOL << 16) | 0x0123,
        SCARD_ATTR_MAX_DATA_RATE = (SCARD_CLASS_PROTOCOL << 16) | 0x0124,
        SCARD_ATTR_MAX_IFSD = (SCARD_CLASS_PROTOCOL << 16) | 0x0125,
        SCARD_ATTR_SYNC_PROTOCOL_TYPES = (SCARD_CLASS_PROTOCOL << 16) | 0x0126,
        SCARD_ATTR_POWER_MGMT_SUPPORT = (SCARD_CLASS_POWER_MGMT << 16) | 0x0131,
        SCARD_ATTR_USER_TO_CARD_AUTH_DEVICE = (SCARD_CLASS_SECURITY << 16) | 0x0140,
        SCARD_ATTR_USER_AUTH_INPUT_DEVICE = (SCARD_CLASS_SECURITY << 16) | 0x0142,
        SCARD_ATTR_CHARACTERISTICS = (SCARD_CLASS_MECHANICAL << 16) | 0x0150,

        SCARD_ATTR_CURRENT_PROTOCOL_TYPE = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x0201,
        SCARD_ATTR_CURRENT_CLK = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x0202,
        SCARD_ATTR_CURRENT_F = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x0203,
        SCARD_ATTR_CURRENT_D = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x0204,
        SCARD_ATTR_CURRENT_N = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x0205,
        SCARD_ATTR_CURRENT_W = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x0206,
        SCARD_ATTR_CURRENT_IFSC = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x0207,
        SCARD_ATTR_CURRENT_IFSD = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x0208,
        SCARD_ATTR_CURRENT_BWT = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x0209,
        SCARD_ATTR_CURRENT_CWT = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x020a,
        SCARD_ATTR_CURRENT_EBC_ENCODING = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x020b,
        SCARD_ATTR_EXTENDED_BWT = (SCARD_CLASS_IFD_PROTOCOL << 16) | 0x020c,

        SCARD_ATTR_ICC_PRESENCE = (SCARD_CLASS_ICC_STATE << 16) | 0x0300,
        SCARD_ATTR_ICC_INTERFACE_STATUS = (SCARD_CLASS_ICC_STATE << 16) | 0x0301,
        SCARD_ATTR_CURRENT_IO_STATE = (SCARD_CLASS_ICC_STATE << 16) | 0x0302,
        SCARD_ATTR_ATR_STRING = (SCARD_CLASS_ICC_STATE << 16) | 0x0303,
        SCARD_ATTR_ICC_TYPE_PER_ATR = (SCARD_CLASS_ICC_STATE << 16) | 0x0304,

        SCARD_ATTR_ESC_RESET = (SCARD_CLASS_VENDOR_DEFINED << 16) | 0xA000,
        SCARD_ATTR_ESC_CANCEL = (SCARD_CLASS_VENDOR_DEFINED << 16) | 0xA003,
        SCARD_ATTR_ESC_AUTHREQUEST = (SCARD_CLASS_VENDOR_DEFINED << 16) | 0xA005,
        SCARD_ATTR_MAXINPUT = (SCARD_CLASS_VENDOR_DEFINED << 16) | 0xA007,

        SCARD_ATTR_DEVICE_UNIT = (SCARD_CLASS_SYSTEM << 16) | 0x0001,
        SCARD_ATTR_DEVICE_IN_USE = (SCARD_CLASS_SYSTEM << 16) | 0x0002,
        SCARD_ATTR_DEVICE_FRIENDLY_NAME_A = (SCARD_CLASS_SYSTEM << 16) | 0x0003,
        SCARD_ATTR_DEVICE_SYSTEM_NAME_A = (SCARD_CLASS_SYSTEM << 16) | 0x0004,
        SCARD_ATTR_DEVICE_FRIENDLY_NAME_W = (SCARD_CLASS_SYSTEM << 16) | 0x0005,
        SCARD_ATTR_DEVICE_SYSTEM_NAME_W = (SCARD_CLASS_SYSTEM << 16) | 0x0006,
        SCARD_ATTR_SUPRESS_T1_IFS_REQUEST = (SCARD_CLASS_SYSTEM << 16) | 0x0007,

        SCARD_PERF_NUM_TRANSMISSIONS = (SCARD_CLASS_PERF << 16) | 0x0001,
        SCARD_PERF_BYTES_TRANSMITTED = (SCARD_CLASS_PERF << 16) | 0x0002,
        SCARD_PERF_TRANSMISSION_TIME = (SCARD_CLASS_PERF << 16) | 0x0003
    }
}
