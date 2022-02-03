using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

//using HClassLibrary;
using StatisticCommon;
using ASUTP.Helper;
using ASUTP;

namespace StatisticAnalyzer
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ProgramBase.Start(Logging.LOG_MODE.FILE_EXE, true);

            Application.Run(new FormMain_DB());

            ProgramBase.Exit ();
        }
    }
}
