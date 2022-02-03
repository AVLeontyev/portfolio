using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace strans
{
    static class itssAUX
    {
        /// <summary>
        /// Выводит сообщение об ошибке на консоль красным шрифтом и (или) в журнал Windows.
        /// </summary>
        public static void PrintErrorMessage (string sErrMess, bool bConsoleOutput = true, bool bWriteToWinEventLog = true)
        {
            if (bConsoleOutput) {
                ConsoleColor cc = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine (sErrMess);
                Console.ForegroundColor = cc;
            } else
                ;

            if ((Program.WriteToWinEventLog == true) && (bWriteToWinEventLog == true) && (Environment.OSVersion.Version.Major < 6))     //На Windows Vista и выше в журнал таким способом записать прав не хватит
            {
                //Для Win7 надо палочкой махнуть, но не кашерно: Try giving the following registry key Read permission for NETWORK SERVICE: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog\Security
                string sAppName = string.Empty;
                sAppName = ASUTP.Helper.ProgramBase.AppName + ".exe";
                //sAppName = "trans_mc_cmd.exe";
                System.Diagnostics.EventLog.WriteEntry (sAppName, sErrMess, System.Diagnostics.EventLogEntryType.Error);
            } else
                ;
        }
    }
}
