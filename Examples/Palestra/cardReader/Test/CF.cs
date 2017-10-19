using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class CF
    {
        public static string GetDateFromFiscalCode(string fiscalCode)
        {
            try
            {
                Dictionary<string, string> month = new Dictionary<string, string>();
                // To Upper
                fiscalCode = fiscalCode.ToUpper();
                month.Add("A", "01");
                month.Add("B", "02");
                month.Add("C", "03");
                month.Add("D", "04");
                month.Add("E", "05");
                month.Add("H", "06");
                month.Add("L", "07");
                month.Add("M", "08");
                month.Add("P", "09");
                month.Add("R", "10");
                month.Add("S", "11");
                month.Add("T", "12");
                // Get Date
                string date = fiscalCode.Substring(6, 5);
                int y = int.Parse(date.Substring(0, 2));
                string yy = ((y < 9) ? "20" : "19") +y.ToString("00");
                string m = month[date.Substring(2, 1)];
                int d = int.Parse(date.Substring(3, 2));
                if (d > 31)
                    d -= 40;
                // Return Date
                return yy+"-"+m+"-"+d;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
