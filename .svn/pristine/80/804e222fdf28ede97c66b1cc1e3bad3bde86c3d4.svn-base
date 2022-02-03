using StatisticCommon;
using StatisticTrans.Contract.ModesCentre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace StatisticTrans.Contract
{
    public class ServiceCallback : Contract.IServiceCallback
    {
        public event Action<Contract.ServiceCallbackResultEventArgs> EventRaise;

        public void Raise (Contract.ServiceCallbackResultEventArgs arg)
        {
            EventRaise?.Invoke (arg);
        }

        public ServiceCallback (Action<Contract.ServiceCallbackResultEventArgs> handler)
        {
            EventRaise += new Action<Contract.ServiceCallbackResultEventArgs> (handler);
        }
    }

    [DataContract]
    public class ServiceCallbackResultEventArgs : EventArgs
    {
        [DataMember]
        public IdPseudoDelegate Id
        {
            get;

            private set;
        }

        [DataMember]
        public DateTime Stamp
        {
            get;

            private set;
        }

        [DataMember]
        public object Data
        {
            get;

            private set;
        }

        [DataMember]
        public IList<HAdmin.RDGStruct> Values
        {
            get;

            private set;
        }

        private ServiceCallbackResultEventArgs (DateTime datetimeStamp)
            : base ()
        {
            Id = IdPseudoDelegate.Unknown;
            Stamp = datetimeStamp;
            Data = null;
            Values = null;
        }

        private ServiceCallbackResultEventArgs ()
            : this (DateTime.MinValue)
        {
        }

        public ServiceCallbackResultEventArgs (IdPseudoDelegate id)
            : this (DateTime.MaxValue)
        {
            Id = id;
        }

        public ServiceCallbackResultEventArgs (IdPseudoDelegate id, string messageData)
            : this (DateTime.MaxValue)
        {
            Id = id;
            Data = messageData;
        }

        public ServiceCallbackResultEventArgs (IdPseudoDelegate id, bool bData)
            : this (DateTime.MaxValue)
        {
            Id = id;
            Data = bData;
        }

        public ServiceCallbackResultEventArgs (IdPseudoDelegate id, DateTime datetimeData)
            : this (DateTime.MaxValue)
        {
            Id = id;
            Data = datetimeData;
        }

        public ServiceCallbackResultEventArgs (IdPseudoDelegate id, int iData)
            : this (DateTime.MaxValue)
        {
            Id = id;
            Data = iData;
        }

        public ServiceCallbackResultEventArgs (IdPseudoDelegate id, DateTime datetimeValues, bool bResult, IList<HAdmin.RDGStruct> values)
            : this ()
        {
            Id = id;
            Stamp = datetimeValues;
            Data = bResult;
            Values = values.ToArray ();
        }

        new public static ServiceCallbackResultEventArgs Empty
        {
            get
            {
                return new ServiceCallbackResultEventArgs ();
            }
        }
    }

    public interface IServiceCallback
    {
        [OperationContract (IsOneWay = true)]
        void Raise (ServiceCallbackResultEventArgs arg);
    }
}
