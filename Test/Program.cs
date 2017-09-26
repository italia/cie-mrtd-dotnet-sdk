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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var f = new Form1();
            SmartCard sc = new SmartCard();
            sc.SmarCardCommunication += new SmarCardCommunicationDelegate((x, b) => {
                System.Diagnostics.Debug.WriteLine(x.ToString() + ":" + BitConverter.ToString(b));
            });

            sc.onRemoveCard += new CardEventHandler((r) =>
            {
                f.label1.Text = "Appoggiare la carta sul lettore";
            });

            sc.onInsertCard += new CardEventHandler((r) => {
                f.label1.Text = "Lettura in corso";
                Application.DoEvents();

                if (!sc.Connect(r, Share.SCARD_SHARE_EXCLUSIVE, Protocol.SCARD_PROTOCOL_T1))
                {
                    System.Diagnostics.Debug.WriteLine("Errore in connessione: " + sc.LastSCardResult.ToString("X08"));
                    return;
                }

                EAC a = new EAC(sc);
                if (a.IsSAC())
                    a.PACE("123456");
                var dg14 = a.ReadDG(DG.DG14);
                a.ChipAuthentication();
                var dg2 = a.ReadDG(DG.DG2);
                sc.Disconnect(Disposition.SCARD_RESET_CARD);
                f.label1.Text = "OK!";
            });

            f.FormClosing += new FormClosingEventHandler((o, e) =>
            {
                sc.StopMonitoring();
            });
            sc.InterfaceForEvents = f;
            sc.StartMonitoring(true);
            f.ShowDialog();
        }
    }
}
