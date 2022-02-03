using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using StatisticCommon;

namespace trans_tg
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ASUTP.Helper.ProgramBase.Start(ASUTP.Logging.LOG_MODE.FILE_EXE, true);

            FormMainTransTG formMain = null;

                try { formMain = new FormMainTransTG(); }
                catch (Exception e)
                {
                    ASUTP.Logging.Logg().Exception(e, "!Ошибка! запуска приложения."
                        , ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                }

                if (!(formMain == null))
                    Application.Run(formMain);
                else
                    ;
            ASUTP.Helper.ProgramBase.Exit();
        }
    }
}
