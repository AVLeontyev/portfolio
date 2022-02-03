using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace trans_mc
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IService1" в коде и файле конфигурации.
    [ServiceContract]
    public interface IServiceModesCentre
    {
        [OperationContract]
        DateTime GetDate ();

        [OperationContract]
        RDGValues[] GetRDGValues (DateTime date);

        // TODO: Добавьте здесь операции служб
    }

    // Используйте контракт данных, как показано на следующем примере, чтобы добавить сложные типы к сервисным операциям.
    [DataContract]
    public class RDGValues
    {
        DateTime _stamp;

        private float _PBR
            , _PBRmin
            , _PBRmax;

        public RDGValues (DateTime stamp)
        {
            Stamp = stamp;
        }

        [DataMember]
        public DateTime Stamp
        {
            get
            {
                return _stamp;
            }

            set
            {
                _stamp = value;
            }
        }

        [DataMember]
        public float PBR
        {
            get
            {
                return _PBR;
            }

            set
            {
                _PBR = value;
            }
        }

        [DataMember]
        public float PBRmin
        {
            get
            {
                return _PBRmin;
            }

            set
            {
                _PBRmin = value;
            }
        }

        [DataMember]
        public float PBRmax
        {
            get
            {
                return _PBRmax;
            }

            set
            {
                _PBRmax = value;
            }
        }
    }
}
