using ASUTP.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestFunc {
    class DbSources {
        private BackgroundWorker [] arThreadDbConn;
        //private List<int> listListenerId = new List<int>();
        private Semaphore m_semaConsole;

        public DbSources ()
        {
            m_semaConsole = new Semaphore (1, 1);

            ConnectionSettings [] arConnSett = new ConnectionSettings []
            {
                    new ConnectionSettings (671, @"Statistic-CentreV", "10.100.104.18", "", 1433, @"techsite-2.X.X", @"client", @"client")
                    , new ConnectionSettings (63, @"Oracle", "10.220.2.5", "", 1521, @"ORCL", @"arch_viewer", @"1")
                    , new ConnectionSettings (675, @"Statistic-CentreS", "10.100.204.63", "", 1433, @"techsite-2.X.X", @"client", @"client")
            };

            arThreadDbConn = new BackgroundWorker [arConnSett.Length];
            int i = -1;
            for (i = 0; i < arThreadDbConn.Length; i++) {
                arThreadDbConn [i] = new BackgroundWorker ();
                arThreadDbConn [i].DoWork += fThreadProc;
                arThreadDbConn [i].RunWorkerAsync (arConnSett [i]);
            }

            ASUTP.Database.DbSources.Sources ().UnRegister ();
        }

        private void fThreadProc (object obj, DoWorkEventArgs ev)
        {
            int iListenerId = -1;
            for (int j = 0; j < 16; j++) {
                iListenerId = ASUTP.Database.DbSources.Sources ().Register (ev.Argument as ConnectionSettings, true, (ev.Argument as ConnectionSettings).name + @"-DbInterface");
                //listListenerId.Add(iListenerId);
                m_semaConsole.WaitOne ();
                Console.WriteLine (@"ConnectionSettings.ID=" + (ev.Argument as ConnectionSettings).id + @", подписка.ID=" + iListenerId);
                m_semaConsole.Release (1);
            }
        }
    }
}
