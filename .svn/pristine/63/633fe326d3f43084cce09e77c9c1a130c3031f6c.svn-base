using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security;

using StatisticCommon;
using StatisticTrans;
using ASUTP;

namespace trans_mc
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ASUTP.Helper.ProgramBase.Start(Logging.LOG_MODE.FILE_EXE, true);

            FormMainTransMC formMain = null;
            try
            { formMain = new FormMainTransMC(); }
            catch (Exception e)
            { Logging.Logg().Exception(e, "Ошибка запуска приложения.", Logging.INDEX_MESSAGE.NOT_SET); }

            if (!(formMain == null))
                try
                { Application.Run(formMain); }
                catch (Exception e)
                { Logging.Logg().Exception(e, "Ошибка выполнения приложения.", Logging.INDEX_MESSAGE.NOT_SET); }
            else ;

            ASUTP.Helper.ProgramBase.Exit();
        }
    }
}

