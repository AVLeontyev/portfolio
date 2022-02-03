using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestFunc {
    class DomainName {
        public DomainName ()
        {
            string strMes = string.Empty;

            strMes = @"Доменное имя пользователя: ";
            Console.Write (strMes);
            try {
                strMes = Environment.UserDomainName;
                Console.Write (strMes + @"\");
            } catch (Exception e) { Console.WriteLine (e.Message); }

            try {
                strMes = Environment.UserName;
                Console.Write (strMes);
            } catch (Exception e) { Console.WriteLine (e.Message); }

            Console.WriteLine ();

            strMes = @"IP: ";
            Console.Write (strMes);
            strMes = string.Empty;
            System.Net.IPAddress [] listIP = System.Net.Dns.GetHostEntry (System.Net.Dns.GetHostName ()).AddressList;
            int indxIP = -1;
            for (indxIP = 0; indxIP < listIP.Length; indxIP++) {
                strMes += @"[" + indxIP + @"]=" + listIP [indxIP].ToString () + @", ";
            }
            strMes = strMes.Substring (0, strMes.Length - 2);
            Console.WriteLine (strMes + Environment.NewLine);
        }
    }
}
