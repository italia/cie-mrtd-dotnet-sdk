using CIE.MRTD.SDK.EAC;
using CIE.MRTD.SDK.PCSC;
using CIE.MRTD.SDK.Util;
using Net.Asn1.Reader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;




namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WebServer ws = new WebServer(SendResponse, "http://localhost:8080/card/");
            ws.Run();
        }

        JObject jsonToReturn = null;

        public string SendResponse(HttpListenerRequest request)
        {

            NameValueCollection coll = request.QueryString;
            string pin = coll.Get("can");

                generaEventiLettura(pin);

            return  jsonToReturn !=null ? jsonToReturn.ToString() : "{Exception: 'no value'}";
        }


        void getNis(SmartCard smc, string r)
        {

            if (!smc.Connect(r, Share.SCARD_SHARE_EXCLUSIVE, Protocol.SCARD_PROTOCOL_T1))
            {
                System.Diagnostics.Debug.WriteLine("Errore in connessione: " + smc.LastSCardResult.ToString("X08"));

            }


            byte[] nisVal = new byte[100];
            var selectIAS = new byte[] { 0x00, // CLA
		0xa4, // INS = SELECT FILE
		0x04, // P1 = Select By AID
		0x0c, // P2 = Return No Data
		0x0d, // LC = lenght of AID
		0xA0, 0x00, 0x00, 0x00, 0x30, 0x80, 0x00, 0x00, 0x00, 0x09, 0x81, 0x60, 0x01 // AID
		};
          var ris =  smc.Transmit(selectIAS);

            var selectCIE = new byte[] { 0x00, // CLA
			0xa4, // INS = SELECT FILE
			0x04, // P1 = Select By AID
			0x0c, // P2 = Return No Data
			0x06, // LC = lenght of AID
			0xA0, 0x00, 0x00, 0x00, 0x00, 0x39 // AID
		};

         var ris2=   smc.Transmit(selectCIE);

            var readNIS =  new byte[]{ 0x00, // CLA
				0xb0, // INS = READ BINARY
				0x81, // P1 = Read by SFI & SFI = 1
				0x00, // P2 = Offset = 0
				0x0c // LE = lenght of NIS
			};

          

          var ris3 =  smc.Transmit(readNIS,ref nisVal);

            string nisToPrint = new ByteArray(nisVal).ToASCII;
            jsonToReturn = new JObject();
            jsonToReturn.Add("nis", nisToPrint);

        }

        void generaEventiLettura(string pin)
        {
            try
            {
                SmartCard smc = new SmartCard();
                string JsonCardVal = string.Empty;

                smc.onRemoveCard += new CardEventHandler((r) => { label1.Text = "Waiting card..."; });

                if (!string.IsNullOrEmpty(pin))
                {
                    // All'inserimento del documento avvio la lettura
                    smc.onInsertCard += new CardEventHandler((r) =>
                    {
                        readCardData(smc, r, pin);
                    });

                } else
                {
                    smc.onInsertCard += new CardEventHandler((r) =>
                    {
                        getNis(smc, r);
                    });
                }

                smc.onRemoveCard += new CardEventHandler((r) => { smc.EndTransaction(Disposition.SCARD_LEAVE_CARD); });

                // Imposto l'interfaccia per l'invio delle notifiche. In questo modo posso interagire con
                // il form senza dover usare delle Invoke.
                smc.InterfaceForEvents = this;

                // Avvio il monitoraggio dei lettori
                smc.StartMonitoring(true);
                smc.InterfaceForEvents = this;
                Thread.Sleep(5000);

            }
            catch (Exception e1)
            {
                txtStatus.Text += "err2 " + e1.Message;
                //return e1.Message;
            }
            
        }


        JObject readCardData(SmartCard smc, string r, string pin)
        {
            try
            {
                smc = new SmartCard();
                // siamo all'interno dell'event handler del form, quindi per aggiornare la label devo eseguire il Message Loop
                Application.DoEvents();
            // avvio la connessione al lettore richiedendo l'accesso esclusivo al chip
            if (!smc.Connect(r, Share.SCARD_SHARE_EXCLUSIVE, Protocol.SCARD_PROTOCOL_T1))
            {
                System.Diagnostics.Debug.WriteLine("Errore in connessione: " + smc.LastSCardResult.ToString("X08"));
                label1.Text = "Errore in connessione: " + smc.LastSCardResult.ToString("X08");
               JObject j = new JObject(); j.Add("Exception", "Errore in connessione: " + smc.LastSCardResult.ToString("X08"));
                    jsonToReturn = j;
                    return j;
            }

            // Creo l'oggetto EAC per l'autenticazione e la lettura, passando la smart card su cui eseguire i comandi
            EAC a = new EAC(smc);
            // Verifico se il chip è SAC
            if (a.IsSAC())
            {
                // Effettuo l'autenticazione PACE.
                // In un caso reale prima di avvare la connessione al chip dovrei chiedere all'utente di inserire il CAN  
                txtStatus.Text += "chip SAC - PACE" + "\n";
                a.PACE(pin);
                    // a.PACE("641230", new DateTime(2022, 12, 30), "CA00000AA");
                }
            else
            {
                    // Per fare BAC dovrei fare la scansione dell'MRZ e applicare l'OCR all'imagine ottenuta. In questo caso ritorno errore.
                    // a.BAC("641230", new DateTime(2022, 12, 30), "CA00000AA");                    //a.BAC()
                    // label1.Text = "BAC non disponibile";  
                    txtStatus.Text += "chip BAC" + "\n";

            }

            // Per poter fare la chip authentication devo prima leggere il DG14
           var dg14 = a.ReadDG(DG.DG14);

            // Effettuo la chip authentication
            a.ChipAuthentication();

                ASN1Tag asn = ASN1Tag.Parse(a.ReadDG(DG.DG11));
                //creao il json da inviare alla webform
                var jsonObject = new JObject();
                jsonObject.Add("nis", "?");

                string nomeCognome = new ByteArray(asn.Child(1).Data).ToASCII.ToString();
                jsonObject.Add("surname",nomeCognome.Split(new[] { "<<" }, StringSplitOptions.None)[0]);
                jsonObject.Add("name", nomeCognome.Split(new[] { "<<" }, StringSplitOptions.None)[1]);

                string codiceFiscale = new ByteArray(asn.Child(2).Data).ToASCII.ToString();
                jsonObject.Add("fiscal_code",  codiceFiscale);

                string residenza=  new ByteArray(asn.Child(4).Data).ToASCII.ToString();
                jsonObject.Add("res_addr", residenza.Split('<')[0]);
                jsonObject.Add("res_place", residenza.Split('<')[1]);
                jsonObject.Add("res_prov", residenza.Split('<')[2]);

                string birth = new ByteArray(asn.Child(3).Data).ToASCII.ToString();
                jsonObject.Add("birth_place", birth.Split('<')[0]);
                jsonObject.Add("birth_prov", birth.Split('<')[1]);

                jsonObject.Add("birth_date", CF.GetDateFromFiscalCode(codiceFiscale));
                

                txtStatus.Text += jsonObject.ToString();

                // Leggo il DG2 contenente la foto
                var dg2 = a.ReadDG(DG.DG2);

            // Disconnessione dal chip
            smc.Disconnect(Disposition.SCARD_RESET_CARD);
                jsonToReturn = jsonObject;
                return jsonObject;
            }
            catch(Exception e)
            {
                txtStatus.Text += "Eccezione: " + e.Message;
                JObject j = new JObject(); j.Add("Exception", e.Message);
                jsonToReturn = j;
                return j;

            }
        }

    }
}
