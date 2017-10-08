using CIE.MRTD.SDK.EAC;
using CIE.MRTD.SDK.PCSC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CIE.MRTD.SDK.PARSERLIB;

namespace Test
{

    static class Program
    {
        //Costanti che codificano lo stato dei radio buttons
        const int ENCODING_XML = 1;
        const int ENCODING_CSV = 2;
        const int ENCODING_JSON = 3;

        //path di salvataggio/recuopero dei file xml/json/csv
        const String savePath = "data/";

        static Form1 f = null;

        static C_CIE persona = null;

        static int fileEncoding = 0; //stato dei radio buttons

        /*Callbk pressine dei radio buttons*/
        static private void radioButtons_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (f.radioButtonXml.Checked)
            {
                fileEncoding = ENCODING_XML;
            }
            else if (f.radioButtonCSV.Checked)
            {
                fileEncoding = ENCODING_CSV;

            } else if (f.radioButtonJSon.Checked)
            {
                fileEncoding = ENCODING_JSON;
            }
        }

        /*Callbk pressione pulsante export*/
        static void clickExport(Object sender,
                           EventArgs e)
        {
            if (persona == null) //controlli
            {
                f.status_.Text = "Appoggiare la carta sul lettore"; //Status, messaggi utente          
                f.status_.ForeColor = Color.Red;
                f.status_.BackColor = f.status_.BackColor;
                return;
            }

            switch (fileEncoding)
            {
                case ENCODING_XML:
                    {
                        persona.saveOnXML(savePath + persona.cf+ "-" + persona.dateIssue + ".xml");
                        break;
                    }

                case ENCODING_CSV:
                    {
                        persona.saveOnCSV(savePath + persona.cf + "-" + persona.dateIssue + ".csv",";");
                        break;
                    }

                case ENCODING_JSON:
                    {
                        persona.saveOnJSON(savePath + persona.cf + "-" + persona.dateIssue + ".json");
                        break;
                    }

                default:
                    {
                        f.status_.Text = "SELEZIONARE UNA CODIFICA"; //Status, messaggi utente          
                        f.status_.ForeColor = Color.Red;
                        f.status_.BackColor = f.status_.BackColor;
                        return;
                    }
            }

            f.status_.Text = "Esportato!"; //Status, messaggi utente          
            f.status_.ForeColor = Color.Green;
            f.status_.BackColor = f.status_.BackColor;

        }

        /*Callbk pulsante Verifica*/
        static void clickVerifica(Object sender,
                           EventArgs e)
        {
            if (persona == null)
            {
                f.status_.Text = "Appoggiare la carta sul lettore"; //Status, messaggi utente          
                f.status_.ForeColor = Color.Red;
                f.status_.BackColor = f.status_.BackColor;
                return;
            }

            C_CIE persona_ver = null;
            switch (fileEncoding)
            {
                case ENCODING_XML:
                    {
                        persona_ver = C_CIE.readFromXML(savePath + persona.cf + "-" + persona.dateIssue + ".xml");
                        break;
                    }

                case ENCODING_CSV:
                    {
                        persona_ver = C_CIE.readFromCSV(savePath + persona.cf + "-" + persona.dateIssue + ".csv",";");
                        break;
                    }

                case ENCODING_JSON:
                    {
                        persona_ver = C_CIE.readFromJSON(savePath + persona.cf + "-" + persona.dateIssue + ".json");
                        break;
                    }

                default:
                    {
                        f.status_.Text = "SELEZIONARE UNA CODIFICA"; //Status, messaggi utente          
                        f.status_.ForeColor = Color.Red;
                        f.status_.BackColor = f.status_.BackColor;
                        return;
                    }
            }

            if (persona_ver != null && persona_ver.mrz == persona.mrz) //è null se non trova il file
            {
                f.status_.Text = "Identità Conosciuta"; //Status, messaggi utente          
                f.status_.ForeColor = Color.Green;
                f.status_.BackColor = f.status_.BackColor;
            }
            else
            {
                f.status_.Text = "Identità non Conosciuta"; //Status, messaggi utente          
                f.status_.ForeColor = Color.Red;
                f.status_.BackColor = f.status_.BackColor;
            }

        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            f = new Form1();

            f.status_.Text = "Appoggiare la carta sul lettore"; //Status, messaggi utente          
            f.status_.ForeColor = Color.Black;
            f.status_.BackColor = f.status_.BackColor;

            //CallbkRadioButt
            f.radioButtonXml.CheckedChanged += new EventHandler(radioButtons_CheckedChanged);
            f.radioButtonCSV.CheckedChanged += new EventHandler(radioButtons_CheckedChanged);
            f.radioButtonJSon.CheckedChanged += new EventHandler(radioButtons_CheckedChanged);
            //CallbkButt
            f.buttonExport.Click += new EventHandler(clickExport);
            f.buttonVerifica.Click += new EventHandler(clickVerifica);


            // crea un aggetto SmartCard per il monitoraggio dei lettori e la connessione al chip
            SmartCard sc = new SmartCard();

            //Imposta un listener dell'evento di comunicazione con il chip
            sc.SmarCardCommunication += new SmarCardCommunicationDelegate((x, b) =>
            {
                // scrivo in debug i dati inviati e ricevuti dal chip
                System.Diagnostics.Debug.WriteLine(x.ToString() + ":" + BitConverter.ToString(b));
            });

            // Alla rimozione del documento aggiorno la label del form
            sc.onRemoveCard += new CardEventHandler((r) =>
            {
                f.status_.Text = "Appoggiare la carta sul lettore";
                f.status_.ForeColor = Color.Black;
                f.status_.BackColor = f.status_.BackColor;
            });

            // All'inserimento del documento aggiorno la label e avvio la lettura
            sc.onInsertCard += new CardEventHandler((r) =>
            {

                f.status_.Text = "Lettura in corso...";
                f.status_.ForeColor = Color.Blue;
                f.status_.BackColor = f.status_.BackColor;
                // siamo all'interno dell'event handler del form, quindi per aggiornare la label devo eseguire il Message Loop
                Application.DoEvents();

                // avvio la connessione al lettore richiedendo l'accesso esclusivo al chip
                if (!sc.Connect(r, Share.SCARD_SHARE_EXCLUSIVE, Protocol.SCARD_PROTOCOL_T1))
                {
                    System.Diagnostics.Debug.WriteLine("Errore in connessione: " + sc.LastSCardResult.ToString("X08"));
                    f.status_.Text = "ERRORE DI CONNESSIONE! RIPROVARE...";
                    f.status_.ForeColor = Color.Red;
                    f.status_.BackColor = f.status_.BackColor;
                    return;
                }

                // Creo l'oggetto EAC per l'autenticazione e la lettura, passando la smart card su cui eseguire i comandi
                EAC a = new EAC(sc);

               // Verifico se il chip è SAC
                if (a.IsSAC())
                    // Effettuo l'autenticazione PACE.
                    // In un caso reale prima di avvare la connessione al chip dovrei chiedere all'utente di inserire il CAN                    
                    a.PACE("123456");
                else
                {
                    // Per fare BAC dovrei fare la scansione dell'MRZ e applicare l'OCR all'imagine ottenuta. In questo caso ritorno errore.
                    // a.BAC(MRZbirthDate, expiryDate, passNumber)
                    f.status_.Text = "BAC non disponibile";
                    f.status_.ForeColor = Color.Red;
                    f.status_.BackColor = f.status_.BackColor;
                    return;
                }

                // Per poter fare la chip authentication devo prima leggere il DG14
                var dg14 = a.ReadDG(DG.DG14);

                // Effettuo la chip authentication
                a.ChipAuthentication();

                persona = new C_CIE(a); //creazione di una E_CIE

                // Disconnessione dal chip
                sc.Disconnect(Disposition.SCARD_RESET_CARD);

                //Aggiornamento interfaccia grafica
                f.nome.Text = persona.firstName;
                f.cognome.Text = persona.lastName;
                f.luogoNascita.Text = persona.birthCity;
                f.provinciaNascita.Text = persona.birthProv;               
                f.provincia.Text = persona.prov;
                f.indirizzo.Text = persona.address;
                //f.indirizzo.Text = System.Text.Encoding.UTF8.GetString(dg12);  //debug
                f.cf.Text = persona.cf;
                f.mrz.Text = persona.mrz;
                f.rilascio.Text = C_CIE.getParsedData( persona.dateIssue );
                f.pictureBox.Image = persona.ret_cie_bitmap();
                f.city.Text = persona.city;

                f.status_.Text = "Carta Letta";
                f.status_.ForeColor = Color.Green;
                f.status_.BackColor = f.status_.BackColor;

            });

            // Alla chiusura del Form sospendo il monitoraggio dei lettori, altrimenti l'applicazione
            // si blocca in attesa del termine del thread
            f.FormClosing += new FormClosingEventHandler((o, e) =>
            {
                sc.StopMonitoring();
            });

            // Imposto l'interfaccia per l'invio delle notifiche. In questo modo posso interagire con
            // il form senza dover usare delle Invoke.
            sc.InterfaceForEvents = f;

            // Avvio il monitoraggio dei lettori
            sc.StartMonitoring(true);

            // Visualizzo il Form informativo
            f.ShowDialog();
        }
    }
}
