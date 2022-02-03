using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestFunc {
    class GenericSorted {
        public GenericSorted ()
        {
            DateTime dtRes;
            List<DateTime> listDT = new List<DateTime> ();

            listDT.Add (DateTime.Now);
            listDT.Add (DateTime.Now.AddHours (-1));
            listDT.Add (DateTime.Now.AddHours (1));

            dtRes = listDT.FirstOrDefault ();

            listDT.Sort ();
            dtRes = listDT.LastOrDefault ();
        }
    }
}
