using CIE.MRTD.SDK.EAC;
using CIE.MRTD.SDK.PCSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    static class Program
    {

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }

        /*
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var f = new Form1();
            // crea un aggetto SmartCard per il monitoraggio dei lettori e la connessione al chip

            SmartCard sc = new SmartCard();

            //Imposta un listener dell'evento di comunicazione con il chip
            sc.SmarCardCommunication += new SmarCardCommunicationDelegate((x, b) =>
            {
                // scrivo in debug i dati inviati e ricevuti dal chip
                System.Diagnostics.Debug.WriteLine(x.ToString() + ":" + BitConverter.ToString(b));
                f.txtStatus.Text=(x.ToString() + ":" + BitConverter.ToString(b));
            });

            // Alla rimozione del documento aggiorno la label del form
            sc.onRemoveCard += new CardEventHandler((r) =>
            {
                f.label1.Text = "Appoggiare la carta sul lettore";
            });

            // All'inserimento del documento aggiorno la label e avvio la lettura
            sc.onInsertCard += new CardEventHandler ( (r) =>
            {

                f.label1.Text = "Lettura in corso";
                // siamo all'interno dell'event handler del form, quindi per aggiornare la label devo eseguire il Message Loop
                Application.DoEvents();

                // avvio la connessione al lettore richiedendo l'accesso esclusivo al chip
                if (!sc.Connect(r, Share.SCARD_SHARE_EXCLUSIVE, Protocol.SCARD_PROTOCOL_T1))
                {
                    System.Diagnostics.Debug.WriteLine("Errore in connessione: " + sc.LastSCardResult.ToString("X08"));
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
                    f.label1.Text = "BAC non disponibile";
                    return;
                }

                // Per poter fare la chip authentication devo prima leggere il DG14
                var dg14 = a.ReadDG(DG.DG14);

                // Effettuo la chip authentication
                a.ChipAuthentication();

                // Leggo il DG2 contenente la foto
                var dg2 = a.ReadDG(DG.DG2);

                // Disconnessione dal chip
                sc.Disconnect(Disposition.SCARD_RESET_CARD);

                // Aggiorno la laber del form
                f.label1.Text = "OK!";
            });

            // Alla chiusura del Form sospendo il monitoraggio dei lettori, altrimenti l'applicazione
            // si blocca in attesa del terine del thread
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
        } */
    }
}
