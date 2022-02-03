using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Threading;
using System.Data.Common;
using System.Data.OleDb;

using Modes;
using ModesApiExternal;
using ASUTP;
using ASUTP.Core;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace trans_mc
{
    public class DbMCInterface : ASUTP.Database.DbInterface
    {
        private IApiExternal m_MCApi;
        private Modes.BusinessLogic.IModesTimeSlice m_modesTimeSlice;
        private IList <PlanFactorItem> m_listPFI;

        public enum EVENT { Unknown = -1
            , OnData53500Modified, OnMaket53500Changed, OnPlanDataChanged
            , OnModesEvent
        }

        public enum ID_EVENT : short { Unknown = -1, HANDLER_CONNECT
            , GENOBJECT_MODIFIED
            , RELOAD_PLAN_VALUES, NEW_PLAN_VALUES
            , PHANTOM_RELOAD_PLAN_VALUES, REQUEST_PLAN_VALUES
            , MC_SERVICE
        }

        public static DbMCInterface.ID_EVENT TranslateEvent (DbMCInterface.EVENT ev)
        {
            DbMCInterface.ID_EVENT evRes = ID_EVENT.Unknown;

            TranslateEvent (ev, out evRes);

            return evRes;
        }

        public static DbMCInterface.EVENT TranslateEvent (DbMCInterface.ID_EVENT ev)
        {
            DbMCInterface.EVENT evRes = EVENT.Unknown;

            TranslateEvent (ev, out evRes);

            return evRes;
        }

        public static void TranslateEvent (DbMCInterface.EVENT ev, out DbMCInterface.ID_EVENT evRes)
        {
            evRes = ID_EVENT.Unknown;

            switch (ev) {
                case EVENT.OnData53500Modified:
                    evRes = ID_EVENT.GENOBJECT_MODIFIED;
                    break;
                case EVENT.OnMaket53500Changed:
                    evRes = ID_EVENT.RELOAD_PLAN_VALUES;
                    break;
                case EVENT.OnPlanDataChanged:
                    evRes = ID_EVENT.NEW_PLAN_VALUES;
                    break;
                case EVENT.OnModesEvent:
                    evRes = ID_EVENT.MC_SERVICE;
                    break;
                default:
                    break;
            }
        }

        public static void TranslateEvent (DbMCInterface.ID_EVENT ev, out DbMCInterface.EVENT evRes)
        {
            evRes = EVENT.Unknown;

            switch (ev) {
                case ID_EVENT.GENOBJECT_MODIFIED:
                    evRes = EVENT.OnData53500Modified;
                    break;
                case ID_EVENT.NEW_PLAN_VALUES:
                case ID_EVENT.REQUEST_PLAN_VALUES:
                    evRes = EVENT.OnPlanDataChanged;
                    break;
                case ID_EVENT.RELOAD_PLAN_VALUES:
                case ID_EVENT.PHANTOM_RELOAD_PLAN_VALUES:
                    evRes = EVENT.OnMaket53500Changed;
                    break;
                case ID_EVENT.MC_SERVICE:
                    evRes = EVENT.OnModesEvent;
                    break;
                default:
                    break;
            }
        }

        public enum ID_GEN_OBJECT_TYPE { GOU = 15, TEC = 1, RGE = 3 }

        public enum Operation : short { Unknown = -1, InitIGO, MaketEquipment, PPBR }
        /// <summary>
        /// Делегат для ретрансляции событий Модес-Центр
        /// </summary>
        private Action<object> delegateMCApiHandler;

        private List <Modes.BusinessLogic.IGenObject> m_listIGO;

        protected override int Timeout { get; set; }

        private Newtonsoft.Json.Linq.JObject _jsonEventListener;

        /// <summary>
        /// Пользовательский конструктор
        /// </summary>
        /// <param name="name">имя</param>
        public DbMCInterface(string name, Action<object>mcApiHandler, Newtonsoft.Json.Linq.JObject jsonEventListener)
            //Вызов конструктора из базового класса DbInterface
            : base(name)
        {
            m_listIGO = new List<Modes.BusinessLogic.IGenObject> ();

            // нет соединения, соединение ни разу не устанавливалось
            m_iConnectCounter = -1;

            delegateMCApiHandler = mcApiHandler;
            _jsonEventListener = jsonEventListener;

            mcApiEventLocked = new object();
        }

        private void mcApi_OnClose (object sender, EventArgs e)
        {
            Disconnect();
        }

        public override bool EqualeConnectionSettings(object cs)
        {
            return string.Equals((string)m_connectionSettings, (string)cs);
        }

        public override bool IsEmptyConnectionSettings
        {
            get
            {
                return string.IsNullOrWhiteSpace((string)m_connectionSettings);
            }
        }

        /// <summary>
        /// Реализация абстрактного метода ("Задать настройки подключения") из базового класса
        /// </summary>
        /// <param name="mcHostName">Модес центр имя хоста</param>
        /// <param name="bStarted">Начато</param>
        public override void SetConnectionSettings(object mcHostName, bool bStarted)
        {
            lock (lockConnectionSettings) // изменение настроек подключения и выставление флага для переподключения - атомарная операция
            {
                //!!! обязательный вызов
                setConnectionSettings(mcHostName);
                //полю "Настройки соединения" присвоить имя хоста
                m_connectionSettings = mcHostName;
            }
            //!!! обязательный вызов
            setConnectionSettings();
        }

        static private IEnumerable<Delegate/*MethodInfo*/> getHandlerExists (object classInstanse)
        {
            IEnumerable<Delegate/*MethodInfo*/> listRes;

            Func<EventInfo, FieldInfo> ei2fi =
                ei => classInstanse.GetType ().GetField (ei.Name,
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.GetField);

            listRes = from eventInfo in classInstanse.GetType ().GetEvents ()
                let eventFieldInfo = ei2fi (eventInfo)
                let eventFieldValue = (System.Delegate)eventFieldInfo?.GetValue (classInstanse)
                    where (Equals(eventFieldInfo, null) == false)
                        && (Equals (eventFieldValue, null) == false)
                        from subscribedDelegate in eventFieldValue?.GetInvocationList ()
                            select subscribedDelegate/*?.Method*/;

            //// безопасный вариант 
            //IEnumerable<FieldInfo> infoFields;
            //IEnumerable<Delegate> valueFields;

            //infoFields = from eventInfo in classInstanse.GetType ().GetEvents ()
            //    select ei2fi (eventInfo);

            //if (Equals (infoFields, null) == false) {
            //    valueFields = from fieldInfo in infoFields
            //        where Equals(fieldInfo, null) == false
            //        select (System.Delegate)fieldInfo?.GetValue (classInstanse);

            //    if (Equals (valueFields, null) == false) {
            //        listRes = from fieldValue in valueFields
            //            where Equals(fieldValue, null) == false
            //            let listMethodInfo = fieldValue.GetInvocationList ()
            //                from methodInfo in listMethodInfo
            //                where Equals (methodInfo, null) == false
            //                    select methodInfo.Method;
            //    } else
            //        listRes = new List<MethodInfo>();
            //} else
            //    listRes = new List<MethodInfo> ();

            return listRes;
        }

        static private IEnumerable<Delegate/*MethodInfo*/> getHandlerExists (object classInstance, string eventName)
        {
            IEnumerable<Delegate/*MethodInfo*/> listRes;

            Type classType = classInstance.GetType ();

            FieldInfo eventField = classType.GetField (eventName, BindingFlags.GetField
                | BindingFlags.NonPublic
                | BindingFlags.Instance);

            Delegate eventDelegate = (Delegate)eventField.GetValue (classInstance);

            // eventDelegate will be null if no listeners are attached to the event
            if (Equals (eventDelegate, null) == false) {
                listRes = (from method in eventDelegate.GetInvocationList ()
                    select method/*.Method*/);
            } else
                listRes = new List<Delegate/*MethodInfo*/> ();

            return listRes;
        }

        private bool unregisterHandler ()
        {
            bool bRes = false;

            IEnumerable<Delegate/*MethodInfo*/> handlers; ;

            #region добавить обработчики для проверки возможности их удаления
            //m_MCApi.OnClose += mcApi_OnClose;
            //m_MCApi.OnData53500Modified += new EventHandler<Modes.NetAccess.EventRefreshData53500> (mcApi_OnEventHandler);
            //m_MCApi.OnMaket53500Changed += mcApi_OnEventHandler;
            //m_MCApi.OnPlanDataChanged += mcApi_OnEventHandler;
            #endregion

            #region Отмена регистрация неуниверсальных обработчиков
            //List<ParameterInfo> handlerParameters;
            //handlers = getHandlerExists (m_MCApi);

            //foreach (Delegate handler in handlers.ToList ()) {
            //    handlerParameters = handler.Method.GetParameters ().ToList();
            //    if (handlerParameters.Count () == 2)
            //        if (typeof (Modes.NetAccess.EventRefreshData53500).IsAssignableFrom (handlerParameters [1].GetType ()) == true)
            //            m_MCApi.OnData53500Modified -= (EventHandler<Modes.NetAccess.EventRefreshData53500>)handler;
            //        else if (typeof (Modes.NetAccess.EventRefreshJournalMaket53500).IsAssignableFrom (handlerParameters [1].GetType ()) == true)
            //            m_MCApi.OnMaket53500Changed -= (EventHandler<Modes.NetAccess.EventRefreshJournalMaket53500>)handler;
            //        else if (typeof (Modes.NetAccess.EventPlanDataChanged).IsAssignableFrom (handlerParameters [1].GetType ()) == true)
            //            m_MCApi.OnPlanDataChanged -= (EventHandler<Modes.NetAccess.EventPlanDataChanged>)handler;
            //        else
            //            ;
            //    else if (handlerParameters.Count () == 0)
            //        m_MCApi.OnClose -= (EventHandler)handler;
            //    else
            //        ;
            //}
            //// проверить
            //handlers = getHandlerExists (m_MCApi);
            #endregion

            #region Отмена регистрации универсального обработчика
            List<string> eventNames = new List<string> () { "OnClose"
                , "OnData53500Modified", "OnMaket53500Changed", "OnPlanDataChanged"
            };

            foreach (string eventName in eventNames) {
                foreach (Delegate handler in getHandlerExists (m_MCApi, eventName)) {
                    switch (eventNames.IndexOf (eventName)) {
                        case 0:
                            m_MCApi.OnClose -= (EventHandler)handler;
                            break;
                        case 1:
                            m_MCApi.OnData53500Modified -= (EventHandler<Modes.NetAccess.EventRefreshData53500>)handler;
                            break;
                        case 2:
                            m_MCApi.OnMaket53500Changed -= (EventHandler<Modes.NetAccess.EventRefreshJournalMaket53500>)handler;
                            break;
                        case 3:
                            m_MCApi.OnPlanDataChanged -= (EventHandler<Modes.NetAccess.EventPlanDataChanged>)handler;
                            break;
                        default:
                            break;
                    }
                }
            }
            // проверить
            handlers = getHandlerExists (m_MCApi);
            #endregion

            return handlers.Count () == 0;
        }

        private bool registerHandler ()
        {
            int counter = -1;
            bool bEventHandler = false;

            m_MCApi.OnClose += mcApi_OnClose;
            counter = 1;

            // добавить обработчики в соответствии с конфигурацией
            if (_jsonEventListener.Count > 0)
                foreach (DbMCInterface.EVENT nameEvent in Enum.GetValues (typeof (DbMCInterface.EVENT))) {
                    if (nameEvent == DbMCInterface.EVENT.Unknown)
                        continue;
                    else
                        ;

                    bEventHandler = bool.Parse (_jsonEventListener.Value<string> (nameEvent.ToString ()));

                    delegateMCApiHandler (Tuple.Create<EVENT, bool> (nameEvent, bEventHandler));

                    if (bEventHandler == true) {
                        counter++;

                        switch (nameEvent) {
                            case EVENT.OnData53500Modified:
                                m_MCApi.OnData53500Modified += mcApi_OnEventHandler;
                                break;
                            case EVENT.OnMaket53500Changed:
                                m_MCApi.OnMaket53500Changed += mcApi_OnEventHandler;
                                break;
                            case EVENT.OnPlanDataChanged:
                                m_MCApi.OnPlanDataChanged += mcApi_OnEventHandler;
                                break;
                            case EVENT.OnModesEvent:
                                m_MCApi.OnModesEvent += mcApi_OnModesEvent;
                                break;
                            default:
                                break;
                        }
                    } else
                        ;
                }
            else
            // нет ни одного правила для (отмены)регистрации события
                ;

            // проверить
            return getHandlerExists (m_MCApi).Count() == counter;
        }

        private CancellationTokenSource _cancelTokenSourceInitialized;

        /// <summary>
        /// Установить соединение с Модес-Центром и подготовить объект соединения к запросам
        /// </summary>
        /// <returns>Результат установления соединения и инициализации</returns>
        protected override bool Connect()
        {
            string msgLog = string.Empty;
            bool result = false
                , bRes = false;
            //Task<bool> taskInitialized;
            CancellationToken cancelTokenInitialized;

            if (m_connectionSettings == null)
                return false;
            else
                ;

            if (m_connectionSettings.GetType().Equals(typeof(string)) == false)
                return false;
            else
                ;

            if (!(((string)m_connectionSettings).Length > 0))
                return false;
            else
                ;

            result =
            bRes =
                false;

            //??? 'bRes' не м.б. 'True'
            try {
                if (bRes == true)
                    return bRes;
                else
                    bRes = true;
            } catch (Exception e) {
                Logging.Logg().Exception(e, "DbMCInterface::Connect ()", Logging.INDEX_MESSAGE.NOT_SET);
            }

            lock (lockConnectionSettings)
            {
                if (IsNeedReconnectNew == true) // если перед приходом в данную точку повторно были изменены настройки, то подключения со старыми настройками не делаем
                    return false;
                else
                    ;
            }

            msgLog = string.Format("Соединение с Modes-Centre ({0})", (string)m_connectionSettings);

            try {
                Logging.Logg ().Debug (string.Format (@"{0} - ...", msgLog), Logging.INDEX_MESSAGE.NOT_SET);

                _cancelTokenSourceInitialized = new CancellationTokenSource ();
                cancelTokenInitialized = _cancelTokenSourceInitialized.Token;

                using (Task<bool> taskInitialized = Task<bool>.Factory.StartNew (delegate () {
                    ModesApiFactory.Initialize ((string)m_connectionSettings);

                    return ModesApiFactory.IsInitilized;
                }, cancelTokenInitialized)) {
                    taskInitialized.Wait (cancelTokenInitialized);

                    bRes =
                    result =
                        taskInitialized.Status == TaskStatus.RanToCompletion ? taskInitialized.Result : false;
                }

                _cancelTokenSourceInitialized.Dispose (); _cancelTokenSourceInitialized = null;
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"{0} - ...", msgLog), Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (bRes == true) {
                // на случай перезагрузки сервера Модес-центр
                try {
                    m_iConnectCounter++;

                    m_MCApi = ModesApiFactory.GetModesApi();
                    m_modesTimeSlice = m_MCApi.GetModesTimeSlice(DateTime.Now.Date.LocalHqToSystemEx(), SyncZone.First, TreeContent.PGObjects, true);
                    m_listPFI = m_MCApi.GetPlanFactors();

                    result = unregisterHandler ();

                    if (result == true) {
                        result = registerHandler ();
                    } else
                        ;

                    if (result == true)
                        Logging.Logg ().Debug (string.Format (@"{0} - {1}...", msgLog, @"УСПЕХ"), Logging.INDEX_MESSAGE.NOT_SET);
                    else
                        Logging.Logg ().Error (string.Format (@"{0} - {1}; не выполнена регистрация/отмена регистрации подписчиков на события...", msgLog, @"ОШИБКА"), Logging.INDEX_MESSAGE.NOT_SET);
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"{0} - ...", msgLog), Logging.INDEX_MESSAGE.NOT_SET);

                    result = false;
                }
            } else
                Logging.Logg().Debug(string.Format(@"{0} - {1}...", msgLog, @"ОШИБКА"), Logging.INDEX_MESSAGE.NOT_SET);

            lock (mcApiEventLocked) {
                delegateMCApiHandler?.Invoke (bRes);
            }

            return result;
        }

        private void mcApi_OnModesEvent (object sender, Modes.NetAccess.MEvent e)
        {
            Logging.Logg ().Action ($"::mcApi_OnModesEvent () - Id={e.Id}, Message=[{e.Message}]{Environment.NewLine}Detail: [Host={e.Host}, Date={e.Date}, User={e.User}, UserGuid={e.UserUid}]..."
                , Logging.INDEX_MESSAGE.NOT_SET);
        }

        private object mcApiEventLocked;

        private void mcApi_OnEventHandler(object obj, EventArgs e)
        {
            object[] sendToTrans;

            if (e.GetType().Equals(typeof(Modes.NetAccess.EventRefreshData53500)) == true) {
                Modes.NetAccess.EventRefreshData53500 ev = e as Modes.NetAccess.EventRefreshData53500;

                sendToTrans = new object[] {
                    ID_EVENT.GENOBJECT_MODIFIED
                    , ev
                };
            } else if (e.GetType().Equals(typeof(Modes.NetAccess.EventRefreshJournalMaket53500)) == true) {
                Modes.NetAccess.EventRefreshJournalMaket53500 ev = e as Modes.NetAccess.EventRefreshJournalMaket53500;

                sendToTrans = new object[] {
                    ID_EVENT.RELOAD_PLAN_VALUES
                    , ev
                };
            } else if (e.GetType().Equals(typeof(Modes.NetAccess.EventPlanDataChanged)) == true) {
                Modes.NetAccess.EventPlanDataChanged ev = e as Modes.NetAccess.EventPlanDataChanged;

                sendToTrans = new object[] {
                    ID_EVENT.NEW_PLAN_VALUES
                    , ev
                };
            } else
                sendToTrans = new object[] { ID_EVENT.Unknown };

            lock (mcApiEventLocked) {
            // простая ретрансляция
                delegateMCApiHandler?.Invoke(sendToTrans);
            }
        }

        private int m_iConnectCounter;

        protected override bool Disconnect()
        {
            bool result = true
                , bRes = false;

            bRes = !(m_iConnectCounter < 0);

            if (bRes == true) {
                // прервать установку соединения, если она выполняется (не 'null')
                _cancelTokenSourceInitialized?.Cancel();

                lock (mcApiEventLocked) {
                    delegateMCApiHandler?.Invoke (false);
                }

                m_iConnectCounter++;

                Logging.Logg ().Error ($"DbMCInterface::Disconnect () - Host={(string)m_connectionSettings}, соединение разорвано...", Logging.INDEX_MESSAGE.NOT_SET);
            } else
                ;

            return result;
        }

        public override void Disconnect(out int err)
        {
            err = 0;
        }

        private Modes.BusinessLogic.IGenObject addIGO (int idInner)
        {
            return addIGO (m_modesTimeSlice.GenTree, idInner);
        }

        private Modes.BusinessLogic.IGenObject addIGO (ReadOnlyCollection<Modes.BusinessLogic.IGenObject>tree, int idInnner)
        {
            foreach (Modes.BusinessLogic.IGenObject igo in tree)
            {
                //Console.WriteLine(igo.Description + " [" + igo.GenObjType.Description + "]");
                //ProcessParams(IGO);
                addIGO(igo, 1, idInnner, delegate (Modes.BusinessLogic.IGenObject newIGO) { m_listIGO.Add (newIGO); });
            }

            return findIGO(idInnner);
        }

        void addIGO (Modes.BusinessLogic.IGenObject igo, int Level, int idInner, Action<Modes.BusinessLogic.IGenObject> fAdd)
        {
            foreach (Modes.BusinessLogic.IGenObject child in igo.Children) {
                if (!((ID_GEN_OBJECT_TYPE)child.GenObjType.Id == ID_GEN_OBJECT_TYPE.GOU)) 
                {
                    //Console.WriteLine(new System.String('-', Level) + IGOch.Description + " [" + IGOch.GenObjType.Description + "]  P:" + IGOch.VarParams.Count.ToString() + " Id:" + IGOch.Id.ToString() + " IdInner:" + IGOch.IdInner.ToString());
                    //ProcessParams(IGOch);
                    if (((ID_GEN_OBJECT_TYPE)child.GenObjType.Id == ID_GEN_OBJECT_TYPE.RGE)
                        && (child.IdInner == idInner)) {
                        fAdd(child);

                        break;
                    } else
                    //У оборудования типа Электростанция (id = 1) нет параметров - только дочерние элементы
                        ;

                    addIGO (child, Level + 1, idInner, fAdd);
                } else
                //Оборудование типа ГОУ исключаем - по ним нет ни параметров, ни дочерних элементов
                    ;
            }
        }

        private Modes.BusinessLogic.IGenObject findIGO(int idInnner)
        {
            return (from igo in m_listIGO where igo.IdInner == idInnner  select igo).ElementAtOrDefault(0);
        }

        private void getData_OnFillError(FillErrorEventArgs arg)
        {
            arg.Continue = false;

            //TODO:
            //getData_OnFillError (m_MCApi, arg);
        }

        protected override bool GetData (DataTable table, object query)
        {
            bool result = false;

            table.Reset();
            table.Locale = System.Globalization.CultureInfo.CurrentCulture;

            try {
                using (ParseMCGetData getData = new ParseMCGetData (m_MCApi
                        //, m_MCTimeSlice
                        , m_listPFI
                        , query
                        , findIGO
                        , addIGO
                        , getData_OnFillError
                    ))
                    result = getData.Result (table);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, $"DbMCInterface::GetData () - query={query}...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            //Logging.Logg().Debug("DbMCInterface::GetData () - " + query + "...", Logging.INDEX_MESSAGE.NOT_SET);

            return result;
        }

        protected override void GetDataCancel ()
        {
            //TODO: как остановить длительно выполняющийся метод 
            // m_MCApi.GetPlanValuesActual
        }
    }
}
