using ASUTP;
using StatisticCommon;
using StatisticTrans;
using StatisticTrans.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace strans
{
    partial class Client
    {
        private interface IWorker
        {
            void Start ();

            void Stop ();

            bool Initialize (IInitializeArgument arg);

            MODE_MASHINE ModeMashine { get; set; }
        }

        private interface IInitializeArgument
        {
            ASUTP.Database.ConnectionSettings m_ConnSett { get; set; }

            int m_iMainSourceData { get; set; }

            MODE_MASHINE m_ModeMashine { get; set; }

            TimeSpan m_tsFetchWaking { get; set; }

            FormChangeMode.MODE_TECCOMPONENT m_ModeTECComponent { get; set; }

            List<int> m_listID_TECNotUse { get; set; }
        }

        private class InitializeArgument : IInitializeArgument
        {
            public ASUTP.Database.ConnectionSettings m_ConnSett { get; set; }

            public int m_iMainSourceData { get; set; }

            public MODE_MASHINE m_ModeMashine { get; set; }

            public TimeSpan m_tsFetchWaking { get; set; }

            public FormChangeMode.MODE_TECCOMPONENT m_ModeTECComponent { get; set; }

            public List<int> m_listID_TECNotUse { get; set; }
        }

        private class InitializeModesTerminaleArgument : InitializeArgument
        {
        }

        private class InitializeModesCentreArgument : InitializeArgument
        {
            public string m_HostSource { get; set; }

            public string m_jEventListener { get; set; }
        }

        private abstract class Worker : IWorker
        {
            public Creator.NameEndPoint NameEndPoint { get; private set; }

            protected StatisticTrans.Contract.IServiceTransModes _client;

            private System.Threading.Timer timerClient;

            protected MODE_MASHINE _modeMashine;
            public MODE_MASHINE ModeMashine { get { return _modeMashine; } set { _modeMashine = value; } }

            public Worker (Creator.NameEndPoint nameEndPoint, Type contractType, Type serviceType)
            {
                NameEndPoint = nameEndPoint;
                _client = createClient(nameEndPoint.ToString());
                _modeMashine = MODE_MASHINE.UNKNOWN;
            }

            public virtual bool Initialize (IInitializeArgument arg)
            {
                _modeMashine = arg.m_ModeMashine;

                return true;
            }

            protected abstract IServiceTransModes createClient (string nameEndPoint);

            public virtual void Start ()
            {
                Task.Factory.StartNew (() => _client.Start ()).Wait();

                switch (ModeMashine) {
                    case MODE_MASHINE.SERVICE_ON_EVENT:
                        _client.Activate (StatisticTrans.CONN_SETT_TYPE.SOURCE, true);
                        break;
                    default:
                        break;
                }

                initializeTimerClient ();
                startTimerClient ();
            }

            public virtual void Stop ()
            {
                stopTimerClient ();

                switch (ModeMashine) {
                    case MODE_MASHINE.SERVICE_ON_EVENT:
                        _client.Activate (StatisticTrans.CONN_SETT_TYPE.SOURCE, false);
                        break;
                    default:
                        break;
                }

                // нельзя в одном методе выполнить 'OneWay=true' и 'OneWay=false'
                Task.Factory.StartNew(() => {
                    _client.Stop ();
                })
                .Wait()
                    ;
            }

            /// <summary>
            /// Инициализация таймера для выполнения регулярных/периодических опросов
            ///  , аналог 'FormMainTrans::InitializeTimerService'
            /// </summary>
            private void initializeTimerClient ()
            {
                if (Equals(timerClient, null) == true) {
                    timerClient =
                        //new System.Windows.Forms.Timer ()
                        new System.Threading.Timer(this.timerClient_Tick)
                        ;
                    timerClient.Change (System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    //TODO: как определить 1-ый запуск
                    //timerService.Interval = ProgramBase.TIMER_START_INTERVAL; //Первый запуск
                    //timerService.Tick += new System.EventHandler (this.timerService_Tick);
                } else
                    ;
            }

            private void startTimerClient ()
            {
                timerClient.Change (0, System.Threading.Timeout.Infinite);
            }

            private void stopTimerClient ()
            {
                timerClient.Change (System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }

            protected virtual void timerClient_Tick (object obj)
            {
                int due = System.Threading.Timeout.Infinite;

                //TODO: как определить 1-ый запуск

                trans_auto_start ();

                // очередная итерация
                timerClient.Change (due, System.Threading.Timeout.Infinite);
            }

            private void trans_auto_start ()
            {
                _client.PrepareActionRDGValues (StatisticTrans.CONN_SETT_TYPE.SOURCE);

                trans_auto_next ();
            }

            private void trans_auto_next ()
            {
                FormChangeMode.KeyDevice key = _client.GetFirstTECComponentKey (StatisticTrans.CONN_SETT_TYPE.SOURCE);

                if (key.Equals (FormChangeMode.KeyDevice.Empty) == false)
                    _client.GetRDGValues (StatisticTrans.CONN_SETT_TYPE.SOURCE, key, DateTime.Now.Date);
                else
                    trans_auto_stop();
            }

            protected virtual void trans_auto_stop ()
            {
                //TODO: только для возможности вызывать '_client.FetchEvent'
            }

            protected virtual void _callback_EventRaise (ServiceCallbackResultEventArgs arg1)
            {
                DateTime date;
                bool bResult = false;
                HAdmin.RDGStruct [] values;

                Action<ServiceCallbackResultEventArgs> eventRaise = delegate (ServiceCallbackResultEventArgs arg2) {
                    switch (arg2.Id) {
                        case IdPseudoDelegate.Started:
                            delegateStartCompleted ();
                            break;
                        case IdPseudoDelegate.Stopped:
                            delegateStopCompleted ();
                            break;
                        //case IdPseudoDelegate.Closed:
                        //    delegateCloseCompleted ();
                        //    break;
                        case IdPseudoDelegate.WaitStart:
                            delegateStartWait ();
                            break;
                        case IdPseudoDelegate.WaitStop:
                            delegateStopWait ();
                            break;
                        case IdPseudoDelegate.WaitStatus:
                            break;
                        case IdPseudoDelegate.ReportError:
                            ErrorReport ((string)arg2.Data);
                            break;
                        case IdPseudoDelegate.ReportWarning:
                            WarningReport ((string)arg2.Data);
                            break;
                        case IdPseudoDelegate.ReportAction:
                            ActionReport ((string)arg2.Data);
                            break;
                        case IdPseudoDelegate.ReportClear:
                            ReportClear ((bool)arg2.Data);
                            break;
                        case IdPseudoDelegate.SetForceDate:
                            setDatetimePicker ((DateTime)arg2.Data);
                            break;
                        case IdPseudoDelegate.Ready:
                            setDataGridViewAdmin (arg2.Stamp, (bool)arg2.Data, arg2.Values);
                            break;
                        case IdPseudoDelegate.Error:
                            errorDataGridViewAdmin ((int)arg2.Data);
                            break;
                        case IdPseudoDelegate.SaveCompleted:
                            break;
                        default:
                            ErrorReport ("неизвестный ответ от службы");
                            break;
                    }

                    debug ($"::_callback_EventRaise () - NameEndPoint:{NameEndPoint}, Id:<{arg1.Id}>...");
                };

                Task.Factory.StartNew (() => eventRaise(arg1));
            }

            private void debug (string mes)
            {
                Console.WriteLine (mes);
                Logging.Logg ().Debug (mes, Logging.INDEX_MESSAGE.NOT_SET);
            }

            #region Указания о готовности к работе/завершению работы
            private void delegateStartCompleted ()
            {
            }

            private void delegateStopCompleted ()
            {
                Action delegateClose = async () => {
                    await Task.Factory.StartNew (() => {
                        _client.Close ();
                    })
                    .ContinueWith((taskCompleted) => {
                        _client = null;
                    });
                };

                delegateClose ();
            }

            //private void delegateCloseCompleted ()
            //{
            //    _client = null;
            //}
            #endregion

            #region Указания о длительно выполняющейся операции
            private void delegateStartWait ()
            {
            }

            private void delegateStopWait ()
            {
            }
            #endregion

            #region Сообщения об ошибках/предупреждениях/действиях
            private void ErrorReport (string report)
            {
                debug ($"::ErrorReport () - NameEndPoint:{NameEndPoint}, Report: [{report}]...");
            }

            private void WarningReport (string report)
            {
            }

            private void ActionReport (string report)
            {
                debug ($"::ActionReport () - NameEndPoint:{NameEndPoint}, Report: [{report}]...");
            }

            private void ReportClear (bool bClear)
            {
                debug ($"::ReportClear () - NameEndPoint:{NameEndPoint}, Report: [{bClear}]...");
            }
            #endregion

            #region Возвращение результата длительно выполненяющихся операций
            private void setDataGridViewAdmin (DateTime date, bool bResult, IEnumerable<HAdmin.RDGStruct> values)
            {
                debug ($"::setDataGridViewAdmin () - NameEndPoint:{NameEndPoint}, Date: [{date.Date}], Values(count: [{(Equals (values, null) == false ? values.Count().ToString() : "не известно")}])...");
            }

            private void errorDataGridViewAdmin (int iState)
            {
                debug ($"::errorDataGridViewAdmin () - NameEndPoint:{NameEndPoint}, State: [{iState}]...");
            }

            private void setDatetimePicker (DateTime date)
            {
            }
            #endregion
        }

        private class WorkerModesTerminale : Worker
        {
            public WorkerModesTerminale (Creator.NameEndPoint nameEndPoint, Type contractType, Type serviceType)
                : base (nameEndPoint, contractType, serviceType)
            {
            }

            protected override IServiceTransModes createClient (string nameEndPoint)
            {
                return new ServiceChannelFactory
                        <StatisticTrans.Contract.ModesTerminale.IServiceModesTerminale> // вариант №2
                        // вариант №3 <IServiceTransModes>
                    (
                        // вариант №2 - нет аргументов
                        // вариант №3 typeof(System.ServiceModel.DuplexChannelFactory<>), contractType, serviceType
                    ).Create (_callback_EventRaise, nameEndPoint);
            }

            public override bool Initialize (IInitializeArgument arg)
            {
                return Equals (_client, null) == false
                    && base.Initialize (arg) == true
                    && ((StatisticTrans.Contract.ModesTerminale.IServiceModesTerminale)_client).Initialize (arg.m_ConnSett
                        , arg.m_iMainSourceData
                        , arg.m_ModeMashine
                        , arg.m_tsFetchWaking
                        , arg.m_ModeTECComponent
                        , arg.m_listID_TECNotUse);
            }
        }

        private class WorkerModesCentre : Worker
        {
            public WorkerModesCentre (Creator.NameEndPoint nameEndPoint, Type contractType, Type serviceType)
                : base (nameEndPoint, contractType, serviceType)
            {
            }

            protected override IServiceTransModes createClient (string nameEndPoint)
            {
                return new ServiceChannelFactory
                        <StatisticTrans.Contract.ModesCentre.IServiceModesCentre> // вариант №2
                        // вариант №3 <IServiceTransModes>
                    (
                        // вариант №2 - нет аргументов
                        // вариант №3 typeof(System.ServiceModel.DuplexChannelFactory<>), contractType, serviceType
                    ).Create (_callback_EventRaise, nameEndPoint);
            }

            public override bool Initialize (IInitializeArgument arg)
            {
                return Equals (_client, null) == false
                    && base.Initialize (arg) == true
                    && ((StatisticTrans.Contract.ModesCentre.IServiceModesCentre)_client).Initialize (arg.m_ConnSett
                        , arg.m_iMainSourceData
                        , (arg as InitializeModesCentreArgument).m_HostSource
                        , arg.m_ModeMashine
                        , arg.m_tsFetchWaking
                        , (arg as InitializeModesCentreArgument).m_jEventListener
                        , arg.m_ModeTECComponent
                        , arg.m_listID_TECNotUse);
            }

            protected override void timerClient_Tick (object obj)
            {
                base.timerClient_Tick (obj);
            }

            protected override void trans_auto_stop ()
            {
                if (((StatisticTrans.Contract.ModesCentre.IServiceModesCentre)_client).IsServiceOnEvent == true)
                    ((StatisticTrans.Contract.ModesCentre.IServiceModesCentre)_client).FetchEvent (true);
                else
                    ;

                base.trans_auto_stop ();
            }
        }
    }
}
