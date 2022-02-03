using ASUTP.Core;
using ASUTP.Database;
using StatisticCommon;
using StatisticTrans;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace StatisticTrans.Contract
{
    namespace ModesTerminale
    {
        [ServiceContract (SessionMode = SessionMode.Required, CallbackContract = typeof (IServiceCallback))]
        public interface IServiceModesTerminale : IServiceTransModes
        {
            [OperationContract]
            bool Initialize (ASUTP.Database.ConnectionSettings connSett
                , int iMainSourceData
                , MODE_MASHINE modeMashine
                , TimeSpan tsFetchWaking
                , FormChangeMode.MODE_TECCOMPONENT modeTECComponent
                , List<int> listID_TECNotUse);
        }
    }
}
