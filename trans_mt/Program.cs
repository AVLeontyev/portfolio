using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using StatisticCommon;
using StatisticTrans;
using ASUTP;

namespace trans_mt
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

            FormMainTransMT formMain = null;

            try { formMain = new FormMainTransMT(); }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, "!Ошибка! запуска приложения.", Logging.INDEX_MESSAGE.NOT_SET);
            }
            try
            {
                if (!(formMain == null))
                    Application.Run(formMain);
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, "Ошибка выполнения приложения.", Logging.INDEX_MESSAGE.NOT_SET);
            }

            ASUTP.Helper.ProgramBase.Exit();
        }
    }
}
