using StatisticTrans.Contract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;

namespace strans
{
    /// <summary>
    /// Контейнер клиентов служб
    ///  , обеспечивает взаимосвязь между непосредственно клентами служб и конфигурацией приложения развертывания
    /// </summary>
    internal partial class Client : Deployment
    {
        /// <summary>
        /// Список объектов - клиентов служб
        /// </summary>
        private List<IWorker> _workers;

        /// <summary>
        /// Конструктор - основной
        /// </summary>
        /// <param name="mode">Режим развертывания для передачи в базовый класс</param>
        public Client (Creator.MODE mode)
            : base(mode)
        {
            Dictionary<Creator.NameEndPoint, Type> workerTypes = new Dictionary<Creator.NameEndPoint, Type> {
                { Creator.NameEndPoint.EndPointServiceTransModesTerminale, typeof(WorkerModesTerminale) }
                , { Creator.NameEndPoint.EndPointServiceTransModesCentre, typeof(WorkerModesCentre) }
            };

            ConfigSectionModesTrans clientSettings = null;
            IWorker worker;
            Service service;
            bool clientTurn = false;

            int iReadFileConnSettResult = -1;
            string messageReadFileConnSettResult = string.Empty;
            ASUTP.Database.FIleConnSett fileConnSett;
            List<ASUTP.Database.ConnectionSettings> listConnSett;
            StatisticTrans.MODE_MASHINE modeMashine = StatisticTrans.MODE_MASHINE.UNKNOWN;
            string [] fetchWaking;
            TimeSpan tsFetchWaking;
            List<int> listIdTECNotUse;

            try {
                clientSettings =
                    (ConfigSectionModesTrans)ConfigurationManager.GetSection ($"{ConfigSectionModesTrans.NameSection}");

                _workers = new List<IWorker> ();

                clientSettings.ModesTransSettings.Cast<ModesTransClientCollection> ().ToList ().ForEach (settings => {
                    clientTurn = false;
                    worker = null;
                    if (bool.TryParse (settings.GetValue ("turn"), out clientTurn) == true)
                        if (clientTurn == true) {
                            service = GetService (settings.NameEndPoint);

                            worker = (IWorker)Activator.CreateInstance(workerTypes[settings.NameEndPoint], settings.NameEndPoint, GetTypeContract(settings.NameEndPoint), service.Type);

                            fileConnSett = new ASUTP.Database.FIleConnSett (settings.GetValue ("conn-sett"), ASUTP.Database.FIleConnSett.MODE.FILE);
                            fileConnSett.ReadSettingsFile (-1, out listConnSett, out iReadFileConnSettResult, out messageReadFileConnSettResult);
                            modeMashine = (StatisticTrans.MODE_MASHINE)Enum.Parse (typeof (StatisticTrans.MODE_MASHINE), settings.GetValue ("ModeMashine"));
                            listIdTECNotUse = (from id in settings.GetValue ("ID_TECNotUse").Split (',') select int.Parse (id)).ToList ();

                            if (iReadFileConnSettResult == 0) {
                                //TODO: Initialize
                                switch (settings.NameEndPoint) {
                                    case Creator.NameEndPoint.EndPointServiceTransModesTerminale:
                                        worker.Initialize (new InitializeArgument { m_ConnSett = listConnSett [0]
                                            , m_iMainSourceData = int.Parse (settings.GetValue ("Main DataSource"))
                                            , m_ModeMashine = modeMashine
                                            , m_tsFetchWaking = TimeSpan.Zero
                                            , m_ModeTECComponent = StatisticCommon.FormChangeMode.MODE_TECCOMPONENT.GTP
                                            , m_listID_TECNotUse = listIdTECNotUse
                                        });
                                        break;
                                    case Creator.NameEndPoint.EndPointServiceTransModesCentre:
                                        //TODO: получить доп./параметры из командной строки, из секции конфигурации
                                        fetchWaking = settings.GetValue ("FetchWaking").Split (';');
                                        tsFetchWaking =
                                            StatisticTrans.FileAppSettings.ParseTimeSpan (settings.GetValue ("FetchWaking").Split (new string [] { StatisticTrans.FileAppSettings.DELIM }, StringSplitOptions.RemoveEmptyEntries), DateTime.Now, StatisticTrans.FileAppSettings.TIMESPAN_PARSE_FUNC.DIFFERENCE, StatisticTrans.Contract.Default.FetchWaking);

                                        worker.Initialize (new InitializeModesCentreArgument { m_ConnSett = listConnSett [0]
                                            , m_iMainSourceData = int.Parse (settings.GetValue ("Main DataSource"))
                                            , m_HostSource = settings.GetValue ("MCServiceHost")
                                            , m_ModeMashine = modeMashine
                                            , m_tsFetchWaking = tsFetchWaking
                                            , m_jEventListener = settings.GetValue ("JEventListener")
                                            , m_ModeTECComponent = StatisticCommon.FormChangeMode.MODE_TECCOMPONENT.GTP
                                            , m_listID_TECNotUse = listIdTECNotUse
                                        });
                                        break;
                                    default:
                                        break;
                                }

                                if (Equals (worker, null) == false)
                                    _workers.Add (worker);
                                else
                                    ;
                            } else {
                                ASUTP.Logging.Logg ().Warning ($@"client NameEndPoint=<{settings.NameEndPoint}> file [{settings.GetValue ("conn - sett")}] not recognized...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                                Console.WriteLine ($@"client NameEndPoint=<{settings.NameEndPoint}> file [{settings.GetValue ("conn-sett")}] not recognized...");
                            }
                        } else {
                            ASUTP.Logging.Logg ().Warning ($@"client NameEndPoint=<{settings.NameEndPoint}> is turn={clientTurn}...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                            Console.WriteLine ($@"client NameEndPoint=<{settings.NameEndPoint}> is turn={clientTurn}...");
                        }
                    else {
                        ASUTP.Logging.Logg ().Warning ($@"client NameEndPoint=<{settings.NameEndPoint}> is turn not recognized...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                        Console.WriteLine ($@"client NameEndPoint=<{settings.NameEndPoint}> is turn not recognized...");
                    }
                });
            } catch (Exception e) {
                ASUTP.Logging.Logg ().Exception (e, "strans.Client::ctor () - ...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        public override void Start ()
        {
            _workers?.ForEach (worker => worker?.Start());
        }

        public override void Stop ()
        {
            _workers?.ForEach (worker => worker?.Stop ());
        }
    }

    internal class ServiceChannelFactory<T>
    {
        // Вариант №2 (T - требуемый тип контракта)

        #region Вариант №3 (T - базовый класс контрактов IServiceTransModes)
        //Type _typeChannelFactory
        //    , _typeContract
        //    , _typeService;
        #endregion

        public ServiceChannelFactory (
            //// вариант №3
            //Type typeChannelFactory, Type typeContract, Type typeService
            )
        {
            #region Вариант №3
            //if (typeContract.IsAssignableFrom (typeService) == true) {
            //    _typeChannelFactory = typeChannelFactory;
            //    _typeContract = typeContract;
            //    _typeService = typeService;
            //} else
            //    throw new InvalidCastException ($"<{_typeContract.FullName}> can't cast to target type <{_typeService.FullName}>");
            #endregion
        }

        public T Create (Action<ServiceCallbackResultEventArgs> fCallback, string nameEndPoint)
        {
            T tRes;

            InstanceContext instContext;

            instContext = new InstanceContext (new ServiceCallback (fCallback));

            #region Вариант №3
            //object channelFactory;
            //MethodInfo createChannel;

            //channelFactory = Activator.CreateInstance (_typeChannelFactory.MakeGenericType (new Type [] { _typeContract }), new object [] {instContext , nameEndPoint });
            //createChannel = _typeChannelFactory.GetMethod ("CreateChannel", new Type [] { });
            #endregion

            tRes =
                //(T2)Activator.CreateInstance(typeof(T1), new InstanceContext (new ServiceCallback (fCallback)), nameEndPoint)
                // вариант №2
                new System.ServiceModel.DuplexChannelFactory<T> (instContext, nameEndPoint).CreateChannel()
                //// вариант №3, исключение, т.к. '_typeChannelFactory' содержит обязательный тип при инициализации
                //(T)createChannel.Invoke (channelFactory, null)
                ;

            return tRes;
        }
    }
}
