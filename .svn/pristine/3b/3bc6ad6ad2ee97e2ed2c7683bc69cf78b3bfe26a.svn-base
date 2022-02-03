using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using StatisticTrans;

using StatisticCommon;
using ASUTP.Database;
using ASUTP.Helper;
using ASUTP;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.ServiceModel.Description;
using System.Reflection;

namespace strans
{
    class Program
    {
        static bool g_bList;
        static bool g_bWriteToWinEventLog;
        static DateTime g_dtList;

        public static FileINI m_fileINI;
        private static string m_strProgramNameSectionDB_INI = "Параметры соединения с БД (" + ProgramBase.AppName + @".exe" + ")";
        private static ASUTP.Core.Crypt m_crypt;

        private static string SignLogStartStop
        {
            get
            {
                return string.Concat (Enumerable.Repeat ("*", 22));
            }
        }

        static void Main(string[] args)
        {
            Logging.SetMode (Logging.LOG_MODE.FILE_EXE);
            // 'AppName' устанавливается в конструкторе 'Creator'
            FileAppSettings.RequiredForced = false;

            Creator creator;

            using (CmdArg cmdArg = new CmdArg (false))
                Logging.AppName = cmdArg.AppName;
            // только после установки значения 'AppName'
            Logging.Logg ().PostStart ($"{SignLogStartStop}Старт{SignLogStartStop}");

            m_fileINI = new FileINI ("setup.ini", false);

            creator = new Creator ();

            if (creator.Error == Creator.ERROR.Ok)
                creator.Start ();
            else {
                Console.WriteLine ($"error: {creator.Error}...");
            }

            if (creator.Error == Creator.ERROR.Ok) {
                Console.Write ("to shutdown press any key...");
                Console.ReadKey (false); Console.WriteLine ();

                creator.Stop ();
            } else
                ;

            Console.Write ($"{(creator.Error == Creator.ERROR.Ok ? Environment.NewLine : string.Empty)}to exit program press any key...");
            Console.ReadKey (false);

            Logging.Logg ().PostStop ($"{SignLogStartStop}Стоп{SignLogStartStop}");
        }

        #region в наследство
        public static bool WriteToWinEventLog
        {
            get
            {
                return g_bWriteToWinEventLog;
            }
        }
        #endregion
    }
}
