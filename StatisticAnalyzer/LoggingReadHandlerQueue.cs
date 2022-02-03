using ASUTP.Database;
using ASUTP.Helper;
using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace StatisticAnalyzer
{
    partial class PanelAnalyzer_DB
    {
        private partial class HLoggingReadHandlerQueue : HHandlerQueue
        {
            protected override int StateCheckResponse (int state, out bool error, out object outobj)
            {
                throw new NotImplementedException ();
            }

            protected override INDEX_WAITHANDLE_REASON StateErrors (int state, int req, int res)
            {
                throw new NotImplementedException ();
            }

            protected override int StateRequest (int state)
            {
                throw new NotImplementedException ();
            }

            protected override int StateResponse (int state, object obj)
            {
                throw new NotImplementedException ();
            }

            protected override void StateWarnings (int state, int req, int res)
            {
                throw new NotImplementedException ();
            }
        }
    }
}
