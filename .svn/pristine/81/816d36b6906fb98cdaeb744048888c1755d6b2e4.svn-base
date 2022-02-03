using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace trans_mc
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "Service1" в коде и файле конфигурации.
    public class ServiceModesCentre : IServiceModesCentre
    {
        public DateTime GetDate ()
        {
            return DateTime.UtcNow;
        }

        public RDGValues[] GetRDGValues (DateTime date)
        {
            RDGValues [] valRes;

            valRes = new RDGValues [24];

            for (int h = 0; h < 24; h++)
                valRes [h] = new RDGValues (date.Date.AddHours (h + 1));

            return valRes;
        }
    }
}
