using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using StatisticCommon;
using StatisticTransModes;
using System.Collections.ObjectModel;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Modes;
using ModesApiExternal;

using ASUTP;
using System.Threading;

namespace trans_mc
{
    public class AdminMC : AdminModes
    {
        public const string s_FetchWaking = "00:47:47";

        /// <summary>
        /// Интерфейс аргумента события
        /// </summary>
        public interface IEventArgs
        {
            /// <summary>
            /// Идентификатор события, внутренний
            /// </summary>
            DbMCInterface.ID_EVENT m_id { get; }
            /// <summary>
            /// Целевая дата/время
            /// </summary>
            DateTime m_Date
            {
                get;
            }
            /// <summary>
            /// Целевой тип аргумента события, ~ от идентификатора события
            /// </summary>
            Type m_type { get; }
        }

        /// <summary>
        /// Аргумент события, полученного от Модес-Центра для помещения в очередь обработки
        /// </summary>
        /// <typeparam name="T">Тип целевого объекта в аргументе</typeparam>
        public class EventArgs<T> : System.EventArgs, IEventArgs
        {
            /// <summary>
            /// Идентификатор события, внутренний
            /// </summary>
            public DbMCInterface.ID_EVENT m_id { get; }
            /// <summary>
            /// Целевая дата/время
            /// </summary>
            public DateTime m_Date { get; }
            /// <summary>
            /// Целевой тип аргумента события, ~ от идентификатора события
            /// </summary>
            public Type m_type { get { return typeof(T); } }

            public ReadOnlyCollection<T> m_listParameters;

            public EventArgs (DbMCInterface.ID_EVENT id, DateTime date, ReadOnlyCollection<T> listParameters)
                : base ()
            {
                m_id = id;

                m_Date = date;

                m_listParameters = new ReadOnlyCollection<T> (listParameters);
            }
        }

        private ObservableCollection<EventArgs> _listMCEventArgs;

        private AutoResetEvent _autoResetEvent_MCArgs_CollectionChanged;

        private event Action<object, System.Collections.Specialized.NotifyCollectionChangedEventArgs> _eventFetch_listMCEventArgs;

        private IEnumerable<Guid> _maketIdentifiers;

        private string m_strMCServiceHost;

        private System.Threading.ManualResetEvent _eventConnected;

        private System.Threading.Timer _timerFetchWaking;
        /// <summary>
        /// Текущий установленный период для таймера ожидания инициирования принудительной обработки аргументов очереди
        ///  , 'volatile' для использования в признаке 'IsTimerFetchWakingActivated'
        /// </summary>
        private volatile int _dueTimerFetchWaking;

        protected enum StatesMachine
        {
            Unknown = -1

            , PPBRValues
            , PPBRDates
            
            , MaketEquipment
        }

        public AdminMC(string strMCServiceHost)
            : base()
        {
            _dictNotify = new Dictionary<DbMCInterface.ID_EVENT, EventHandler> ();

            m_strMCServiceHost = strMCServiceHost;

            _eventConnected = new System.Threading.ManualResetEvent (false);

            activateTimerFetchWaking(true);
        }

        public override void Start ()
        {
            base.Start ();

            if (IsServiceOnEvent == true) {
                _autoResetEvent_MCArgs_CollectionChanged = new AutoResetEvent (true);
                _listMCEventArgs = new ObservableCollection<EventArgs> ();
                _listMCEventArgs.CollectionChanged += listMCEventArgs_CollectionChanged;
                _eventFetch_listMCEventArgs += listMCEventArgs_CollectionChanged;
            } else
                ;
        }

        /// <summary>
        /// Метод обратного вызова при истечении интервала для таймера '_timerFetchWaking'
        ///  , происходит при отсутствии событий от сервиса
        /// </summary>
        /// <param name="obj">Аргумент при вызове метода</param>
        private void fTimerFetchWaking (object obj)
        {
            DateTime reqDate = ASUTP.Core.HDateTime.ToMoscowTimeZone ().Date;

            Logging.Logg ().Action ($"AdminMC::fTimerFetchWaking () - Date={reqDate}", Logging.INDEX_MESSAGE.NOT_SET);

            ToDateRequest (reqDate);
        }

        /// <summary>
        /// Возобновить/остановить работу таймера для принудительного инициирования обработки событий (при их наличии)
        ///  , необходимость может возникнуть когда:
        ///   1) закончилась обработка всех событий
        ///   2) связь с сервисом Модес-Центр разорвана
        ///   3) подписчики событий не вызываются, т.к. требуется актуальное соединение с сервисом
        ///   4) актуальное соединение восстанавливается только при попытке запроса, т.е. вызова подписчика => см. п.3
        /// </summary>
        private void activateTimerFetchWaking (bool bActivated)
        {
            if (Equals (_timerFetchWaking, null) == true) {
                _timerFetchWaking = new Timer (fTimerFetchWaking
                    , null
                    , System.Threading.Timeout.Infinite
                    , System.Threading.Timeout.Infinite
                );
            } else
                ;

            _dueTimerFetchWaking = bActivated == true ? (int)StatisticTrans.FileAppSettings.This ().FetchWaking (s_FetchWaking).TotalMilliseconds : System.Threading.Timeout.Infinite;

            _timerFetchWaking.Change (_dueTimerFetchWaking, System.Threading.Timeout.Infinite);

            Logging.Logg ().Debug ($"AdminMC::activateTimerFetchWaking (Activated={bActivated}) - интервал={TimeSpan.FromMilliseconds(_dueTimerFetchWaking).ToString()}..."
                , Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Признак ожидания ноых собщений
        ///  аналогичен проверке количества аргументов в очереди обработки (коллекция аргументов 'IEventArgs')
        /// </summary>
        private bool IsTimerFetchWakingActivated
        {
            get
            {
                return !(_dueTimerFetchWaking == System.Threading.Timeout.Infinite);
            }
        }

        /// <summary>
        /// Возвратить признак наличия обработчика события указанного в аргументе типа
        /// </summary>
        /// <param name="idMCEvent">Тип события Модес-Центр</param>
        /// <returns>Признак наличия обработчика</returns>
        private bool isHandlerMCEvent(DbMCInterface.ID_EVENT idMCEvent)
        {
            return (_dictNotify.ContainsKey(idMCEvent) == true)
                ? (Equals (_dictNotify[idMCEvent], null) == false)
                    : false;
        }

        /// <summary>
        /// Словарь событий 
        /// </summary>
        private static Dictionary<DbMCInterface.ID_EVENT, EventHandler> _dictNotify;

        /// <summary>
        /// Признак, что родительская форма выполняется в режиме "обработка событий Модес-Центр"
        /// </summary>
        public bool IsServiceOnEvent
        {
            get
            {
                return _dictNotify.Keys.ToList().Exists(key => isHandlerMCEvent (key) == true);
            }
        }

        public void AddEventHandler (DbMCInterface.ID_EVENT id_event, EventHandler handler)
        {
            _dictNotify.Add (id_event, handler);
        }

        private void listMCEventArgs_CollectionChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IEventArgs arg = null;
            DbMCInterface.ID_EVENT id = trans_mc.DbMCInterface.ID_EVENT.Unknown;
            string mesLog = string.Empty;

            //Func<IEventArgs, DbMCInterface.ID_EVENT> contextId = delegate (IEventArgs ev) {
            //    DbMCInterface.ID_EVENT id_event = DbMCInterface.ID_EVENT.Unknown;

            //    if (typeof (Guid).IsAssignableFrom (ev.) == true)
            //        id_event = DbMCInterface.ID_EVENT.RELOAD_PLAN_VALUES;
            //    else if (typeof (FormChangeMode.KeyDevice).IsAssignableFrom (ev.m_type) == true)
            //        id_event = DbMCInterface.ID_EVENT.NEW_PLAN_VALUES;
            //    else
            //        ;

            //    return id_event;
            //};

            Func<IEventArgs, bool> doWork = delegate (IEventArgs ev) {
                bool bRes = false;

                bRes = isHandlerMCEvent(ev.m_id);

                if (bRes == true)
                    // "?" на всякий случай, т.к. проверка уже выполнена
                    _dictNotify [ev.m_id]?.Invoke (this, (ev as EventArgs));
                else
                    ;

                return bRes;
            }
            , delegateDoWork = null;

            switch (e.Action) {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    id =
                        //contextId ((IEventArgs)e.NewItems[0])
                        ((IEventArgs)e.NewItems [0]).m_id
                        ;

                    if (e.NewStartingIndex == 0) {
                        arg = (IEventArgs)e.NewItems [0];

                        activateTimerFetchWaking (false);
                    } else
                    // новые элементы будут ожидать обработки
                        ;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if ((sender as ObservableCollection<EventArgs>).Count > 0) {
                        arg = (IEventArgs)(sender as ObservableCollection<EventArgs>) [0];
                        id =
                            //contextId (arg)
                            arg.m_id
                            ;
                    } else {
                    //??? в случае 'Reset' - исключение, т.к. 'e.OldItems' = NullReference
                        try {
                            id =
                                //contextId ((IEventArgs)e.OldItems [0])
                                ((IEventArgs)e.OldItems [0]).m_id
                                ;
                        } catch (Exception excp) {
                            Logging.Logg ().Exception (excp, $"AdminMC::listMCEventArgs_CollectionChanged (Act.={e.Action}) - Connected={isConnected}, ", Logging.INDEX_MESSAGE.NOT_SET);
                        } finally {
                            activateTimerFetchWaking (true);
                        }
                    }
                    break;
                default:
                    break;
            }

            mesLog = $"AdminMC::listMCEventArgs_CollectionChanged (Act.={e.Action}, ID_EVENT={id}) - Connected={isConnected}, ";

            if ((Equals (arg, null) == false)
                && isConnected == true) {
                delegateDoWork = doWork;

                mesLog = $"{mesLog}Count={_listMCEventArgs.Count}, NewIndex={e.NewStartingIndex}, OldIndex={e.OldStartingIndex}...";
            } else {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    mesLog = $@"{mesLog}ожидание обработки({e.NewStartingIndex}-й в списке): {((IEventArgs)(sender as ObservableCollection<EventArgs>) [e.NewStartingIndex]).m_id.ToString()}...";
                else if ((e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    && ((sender as ObservableCollection<EventArgs>).Count == 0))
                    mesLog = $@"{mesLog}из коллекции удален крайний элемент ...";
                else
                    ;
            }

            Logging.Logg ().Debug (mesLog, Logging.INDEX_MESSAGE.NOT_SET);

            // не выпполнять, если 'delegateDoWork' = null (при 'arg' = null)
            delegateDoWork?.Invoke (arg);

            _autoResetEvent_MCArgs_CollectionChanged.Set ();
        }

        /// <summary>
        /// Добавить аргумент в список для последующей обработки
        /// </summary>
        /// <param name="arg">Аргумент для постановки в очередь для обработки</param>
        public void AddEvent (EventArgs arg)
        {
            _autoResetEvent_MCArgs_CollectionChanged.WaitOne ();

            _listMCEventArgs.Add (arg);
        }

        /// <summary>
        /// Инициировать обработку очередного аргумента
        /// </summary>
        /// <param name="bRemove">Признак удаления 0-го из списка (как выполненного)</param>
        public void FetchEvent (bool bRemove)
        {
            try {
                if ((Equals(_listMCEventArgs, null) == false)
                    && (_listMCEventArgs.Count > 0)) {
                    _autoResetEvent_MCArgs_CollectionChanged.WaitOne ();

                    if ((bRemove == true)
                        && (isConnected == true)) // не факт, что событие было обработано должным образом
                        _listMCEventArgs?.RemoveAt (0);
                    else
                        _eventFetch_listMCEventArgs (_listMCEventArgs, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
                } else
                    Logging.Logg ().Warning ($@"trans_mc.AdminMC::FetchEvent (REMOVE={bRemove}) - исходный размер = 0...{Environment.NewLine}Стэк={Environment.StackTrace}", Logging.INDEX_MESSAGE.NOT_SET);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, $@"trans_mc.AdminMC::FetchEvent () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Подготовить список идентификаторов ГТП для формирования запроса на получение данных
        /// </summary>
        /// <returns>Ключ 0-го оборудования из списка</returns>
        public override FormChangeMode.KeyDevice PrepareActionRDGValues ()
        {
            List<FormChangeMode.KeyDevice> listKey;

            if ((IsServiceOnEvent == true)
                && (_listMCEventArgs.Count > 0))
                listKey = new List<FormChangeMode.KeyDevice> ((_listMCEventArgs [0] as EventArgs<FormChangeMode.KeyDevice>).m_listParameters);
            else
            // даже и при режиме 'ON_EVENT', при  отсутствии MC-аргумента выполним опрос по полному списку оборудования
                listKey = GetListKeyTECComponent (FormChangeMode.MODE_TECCOMPONENT.GTP, true);

            if (_listTECComponentKey == null)
                _listTECComponentKey = new List<FormChangeMode.KeyDevice> ();
            else
                ;

            try {
                // проверить на наличие дубликатов
                if (listKey.Count - listKey.Distinct ().Count () == 0) {
                    _listTECComponentKey.Clear ();
                    listKey.ForEach ((key) => {
                        if (_listTECComponentKey.Contains (key) == false)
                            _listTECComponentKey.Add (key);
                        else
                            Logging.Logg ().Error (string.Format ("trans_mc.AdminMC::PrepareExportRDGValues () - добавление повторяющегося индекса {0}...", key.ToString ()), Logging.INDEX_MESSAGE.NOT_SET);
                    });

                    //TODO:
                    // аварийно прекратить выполнение предыдущей операции сохранения ПБР-значений
                } else
                    Logging.Logg ().Error (string.Format ("trans_mc.AdminMC::PrepareExportRDGValues () - в переданном списке <{0}> есть дубликаты...", string.Join (",", listKey.Select (key => key.ToString ()).ToArray ()))
                        , Logging.INDEX_MESSAGE.NOT_SET);

                Logging.Logg ().Action ($"trans_mc.AdminMC::PrepareExportRDGValues () - подготовлен список для опроса: <{string.Join (", ", _listTECComponentKey.ConvertAll<string> (key => key.Id.ToString ()).ToArray ())}>..."
                    , Logging.INDEX_MESSAGE.NOT_SET);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, string.Format ("trans_mc.AdminMC::PrepareExportRDGValues () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }

            return base.PrepareActionRDGValues ();
        }

        protected override void getPPBRValuesRequest(TEC t, IDevice comp, DateTime date/*, AdminTS.TYPE_FIELDS mode*/)
        {
            string query = DbMCInterface.Operation.PPBR.ToString();
            int i = -1;

            //Logging.Logg().Debug("AdminMC::GetPPBRValuesRequest (TEC, TECComponent, DateTime, AdminTS.TYPE_FIELDS) - вХод...: query=" + query, Logging.INDEX_MESSAGE.NOT_SET);

            query += ";";
            for (i = 0; i < comp.ListMCentreId.Count; i++)
            {
                query += comp.ListMCentreId[i];

                if ((i + 1) < comp.ListMCentreId.Count) query += ","; else ;
            }

            query += ";";
            query += date.ToOADate().ToString();

            DbMCSources.Sources().Request(m_IdListenerCurrent, query);

            Logging.Logg ().Debug ($"AdminMC::getPPBRValuesRequest (TEC={t.name_shr}, IDevice={comp.name_shr}, Stamp=[{date.ToString()}]) - вЫход...: query=" + query, Logging.INDEX_MESSAGE.D_002);
        }

        protected override void getPPBRDatesRequest (DateTime date)
        {
        }

        private void getMaketEquipmentRequest (IEnumerable<Guid> listIdentifiers, DateTime date)
        {
            string query = DbMCInterface.Operation.MaketEquipment.ToString();
            int i = -1;
            // добавить идентификаторы макетов
            query += ";";
            listIdentifiers.ToList ().ForEach (id => { query += string.Format("{0},", id.ToString()); });
            query = query.Substring (0, query.Length - 1);
            // добавить дату
            query += ";";
            query += date.ToOADate ().ToString ();

            DbMCSources.Sources ().Request (m_IdListenerCurrent, query);

            Logging.Logg ().Debug ($"AdminMC::getMaketEquipmentRequest (Identifiers.Count={listIdentifiers.Count()}, Stamp=[{date.ToString()}]) - вЫход...: query=" + query, Logging.INDEX_MESSAGE.D_002);
        }

        protected override int getPPBRValuesResponse(DataTable table, DateTime date)
        {
            int iRes = 0;
            int i = -1, j = -1,
                hour = -1,
                offsetPBR = 2
                , offset = 0;
            string msgDebug = string.Format(@"Модес-Центр, получено строк={0}, [{1}] на {2}: ", table.Rows.Count, @"PBR_NUMBER", date);

            for (i = 0; i < table.Rows.Count; i++)
            {
                try
                {
                    hour = ((DateTime)table.Rows[i]["DATE_PBR"]).Hour;
                    if ((hour == 0)
                        && (!(((DateTime)table.Rows[i]["DATE_PBR"]).Day == date.Day))) {
                    // это крайний час текущих суток
                        hour = 24;

                        msgDebug = msgDebug.Replace(@"PBR_NUMBER", table.Rows[i][@"PBR_NUMBER"].ToString());
                    } else
                        if (hour == 0)
                        // это предыдущие сутки
                            continue;
                        else
                            ;

                    hour += offset;

                    m_curRDGValues[hour - 1].pbr_number = table.Rows[i][@"PBR_NUMBER"].ToString();
                    // проблема решается ранее, в 'DbMCInterface::GetData'
                    //if (m_curRDGValues[hour - 1].pbr_number.IndexOf(HAdmin.PBR_PREFIX) < 0)
                    //    m_curRDGValues[hour - 1].pbr_number = HAdmin.PBR_PREFIX + m_curRDGValues[hour - 1].pbr_number;
                    //else
                    //    ;

                    //Logging.Logg().Debug(string.Format(@"AdminMC::getPPBRValuesResponse () - hour={0}, PBRNumber={1}...", hour, m_curRDGValues[hour - 1].pbr_number), Logging.INDEX_MESSAGE.NOT_SET);

                    //for (j = 0; j < 3 /*4 для SN???*/; j ++)
                    //{
                    j = 0;
                    if (!(table.Rows[i][offsetPBR + j] is DBNull))
                        m_curRDGValues[hour - 1].pbr = (double)table.Rows[i][offsetPBR + j];
                    else
                        m_curRDGValues[hour - 1].pbr = 0;
                    //}

                    j = 1;
                    if (!(table.Rows[i][offsetPBR + j] is DBNull))
                        m_curRDGValues[hour - 1].pmin = (double)table.Rows[i][offsetPBR + j];
                    else
                        m_curRDGValues[hour - 1].pmin = 0;

                    j = 2;
                    if (!(table.Rows[i][offsetPBR + j] is DBNull))
                        m_curRDGValues[hour - 1].pmax = (double)table.Rows[i][offsetPBR + j];
                    else
                        m_curRDGValues[hour - 1].pmax = 0;

                    m_curRDGValues[hour - 1].recomendation = 0;
                    m_curRDGValues[hour - 1].deviationPercent = false;
                    m_curRDGValues[hour - 1].deviation = 0;

                    //Копирование при переходе лето-зима (-1)
                    if ((m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == true) && (hour == (HAdmin.SeasonDateTime.Hour - 0))) {
                        m_curRDGValues[hour].From(m_curRDGValues[hour - 1]);

                        offset++;
                    } else {
                    }

                    msgDebug += string.Format(@"[Час={0}, знач={1}],", hour, m_curRDGValues[hour - 1].pbr);
                } catch(Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"AdminMC::getPPBRValuesResponse () - строка={0}", i), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            msgDebug = msgDebug.Substring(0, msgDebug.Length - 1); // удалить лишнюю запятую
            Logging.Logg().Debug(msgDebug, Logging.INDEX_MESSAGE.D_002);

            return iRes;
        }

        protected override int getPPBRDatesResponse (DataTable table, DateTime date)
        {
            int iRes = 0;

            return iRes;
        }

        private int getMaketEquipmentResponse (DataTable table, DateTime date)
        {
            int iRes = 0;

            ReadOnlyCollection<FormChangeMode.KeyDevice> listKeyDevice = null;

            listKeyDevice = new ReadOnlyCollection<FormChangeMode.KeyDevice> ((from row in table.Rows.Cast<DataRow> ()
                select new FormChangeMode.KeyDevice { Id = int.Parse(row ["IdInner"].ToString())
                    , Mode = FormChangeMode.MODE_TECCOMPONENT.GTP })
                .ToList());

            if (listKeyDevice.Count > 0) {
                AddEvent (new EventArgs<FormChangeMode.KeyDevice> (DbMCInterface.ID_EVENT.NEW_PLAN_VALUES, date, listKeyDevice));
            } else
                Logging.Logg().Warning($@"AdminMC::getMaketEquipmentResponse () - получен пустой список с оборудованием...", Logging.INDEX_MESSAGE.NOT_SET);

            FetchEvent (true);

            return iRes;
        }

        protected override bool InitDbInterfaces()
        {
            bool bRes = true;
            int i = -1;

            DbMCSources.Sources ().SetMCApiHandler (dbMCSources_OnEventHandler
                , IsServiceOnEvent == true
                    ? JsonConvert.DeserializeObject<JObject> (StatisticTrans.FileAppSettings.This().GetValue("JEventListener"))
                        : new JObject ());
            m_IdListenerCurrent = ASUTP.Database.DbSources.Sources().Register(m_strMCServiceHost, true, @"Modes-Centre");

            return bRes;
        }

        private void dbMCSources_OnEventHandler(object obj)
        {
            DbMCInterface.ID_EVENT id_event = DbMCInterface.ID_EVENT.Unknown;
            EventArgs argEventChanged = null; // оборудование для которого произошло событие
            TEC tec; // для оборудования которой произошло событие
            Tuple<DbMCInterface.EVENT, bool> tupleConnectHandler;

            Func<DateTime, TimeSpan> difference = delegate (DateTime target) {
                // разница целевой даты объекта события и текущей даты
                return target - ASUTP.Core.HDateTime.ToMoscowTimeZone ();
            };

            Func<DateTime, bool> isRequired = delegate (DateTime target) {
                // разница целевой даты объекта события и текущей даты; для проверки необходимости обработки события (только текущие сутки)
                TimeSpan diff = difference(target);

                return diff.TotalDays > -1
                    && diff.TotalDays < ASUTP.Core.HDateTime.TS_MSK_OFFSET_OF_UTCTIMEZONE.TotalDays;
            };

            Func<DbMCInterface.ID_EVENT, DateTime, DateTime> translate = delegate (DbMCInterface.ID_EVENT id, DateTime datetime) {
                return ((id == DbMCInterface.ID_EVENT.RELOAD_PLAN_VALUES) || (id == DbMCInterface.ID_EVENT.NEW_PLAN_VALUES))
                    ? datetime.SystemToLocalHqEx ()
                        : ((id == DbMCInterface.ID_EVENT.PHANTOM_RELOAD_PLAN_VALUES) || (id == DbMCInterface.ID_EVENT.REQUEST_PLAN_VALUES))
                            ? datetime
                                : datetime;
            };

            if (obj is Array) {
                id_event = (DbMCInterface.ID_EVENT)(obj as object []) [0];

                if (id_event == DbMCInterface.ID_EVENT.GENOBJECT_MODIFIED) {
                    Modes.NetAccess.EventRefreshData53500 ev = (obj as object []) [1] as Modes.NetAccess.EventRefreshData53500;

                    #region Подготовка текста сообщения в журнал о событии
                    string msg = string.Empty
                        , listEquipment = string.Empty;
                    msg = string.Format (@"AdminMC::dbMCSources_OnEventHandler((ID_MC_EVENT={1}) - обработчик события - изменения[кол-во={2}]{0}для оборудования {3}..."
                        , Environment.NewLine, id_event, ev.Equipments.Count, @"СПИСОК");
                    foreach (KeyValuePair<DateTime, List<int>> pair in ev.Equipments)
                        listEquipment += string.Format (@"[Дата={0}, список=({1})],", pair.Key.ToString (), string.Join (", ", pair.Value));
                    listEquipment = listEquipment.Remove (listEquipment.Length - 1);
                    msg = msg.Replace (@"СПИСОК", listEquipment);
                    #endregion

                    Logging.Logg ().Action (msg, Logging.INDEX_MESSAGE.NOT_SET);
                } else if ((id_event == DbMCInterface.ID_EVENT.RELOAD_PLAN_VALUES)
                    || (id_event == DbMCInterface.ID_EVENT.PHANTOM_RELOAD_PLAN_VALUES)) {
                    Modes.NetAccess.EventRefreshJournalMaket53500 ev = (obj as object []) [1] as Modes.NetAccess.EventRefreshJournalMaket53500;

                    DateTime dateTarget;
                    ReadOnlyCollection<Guid> makets;
                    string abbr = string.Empty
                        , taskModes = string.Empty;

                    dateTarget = translate (id_event, ev.dtTarget.GetValueOrDefault ());
                    makets = ev.makets as ReadOnlyCollection<Guid>;
                    abbr = ev.Task.GetAbbr ();
                    taskModes = ev.Task.ModesTaskToString ();

                    Logging.Logg ().Action (string.Format (@"AdminMC::dbMCSources_OnEventHandler(ID_MC_EVENT={0}) - обработчик события - переопубликация[на дату={1}, кол-во макетов={2}], Аббр={3}, описание={4}..."
                            , id_event.ToString (), dateTarget.ToString (), makets.Count, abbr, taskModes)
                        , Logging.INDEX_MESSAGE.NOT_SET);

                    if (isRequired (dateTarget) == true)
                        argEventChanged = new EventArgs<Guid> (id_event, dateTarget, makets);
                    else
                        Logging.Logg ().Debug ($"AdminMC::dbMCSources_OnEventHandler(ID_MC_EVENT={id_event.ToString ()}) - дата нового не актуальна; Day=[{dateTarget}], разн.(сутки)=[{difference (dateTarget).TotalDays}]..."
                           , Logging.INDEX_MESSAGE.NOT_SET);
                } else if ((id_event == DbMCInterface.ID_EVENT.NEW_PLAN_VALUES)
                    || (id_event == DbMCInterface.ID_EVENT.REQUEST_PLAN_VALUES)) {
                    Modes.NetAccess.EventPlanDataChanged ev = (obj as object []) [1] as Modes.NetAccess.EventPlanDataChanged;

                    DateTime day
                        , version;
                    string pbr_number = string.Empty
                        , id_mc_tec = string.Empty;
                    int id_gate = -1;

                    day = translate (id_event, ev.Day);
                    pbr_number = ev.Type.PlanTypeToString ();
                    version = translate (id_event, ev.Version);
                    id_mc_tec = ev.ClientId;
                    id_gate = ev.IdGate;

                    Logging.Logg ().Action (string.Format (@"AdminMC::dbMCSources_OnEventHandler(ID_MC_EVENT={0}) - обработчик события - {6} план[на дату={1}, номер={2}, версия={3}, для подразделения={4}, IdGate={5}]..."
                            , id_event.ToString (), day.ToString (), pbr_number, version.ToString (), id_mc_tec, id_gate
                                , id_event == DbMCInterface.ID_EVENT.NEW_PLAN_VALUES ? "новый" : id_event == DbMCInterface.ID_EVENT.REQUEST_PLAN_VALUES ? "<запрос>" : "НЕ ИЗВЕСТНО")
                        , Logging.INDEX_MESSAGE.NOT_SET);

                    // проверить дату за которую получен новый план: только сегодняшние и следующие сутки сутки
                    if (isRequired (day) == true) {
                        tec = m_list_tec.Find (t => {
                            return t.name_MC.Trim ().Equals (id_mc_tec.ToString ());
                        });

                        argEventChanged = new EventArgs<FormChangeMode.KeyDevice> (id_event, day, new ReadOnlyCollection<FormChangeMode.KeyDevice> (
                            allTECComponents.FindAll (comp => {
                                return (comp.IsGTP == true)
                                    && (comp.tec.m_id == tec.m_id);
                            }).ConvertAll (comp => new FormChangeMode.KeyDevice () { Id = comp.m_id, Mode = FormChangeMode.MODE_TECCOMPONENT.GTP })
                        ));
                    } else
                        Logging.Logg ().Debug ($"AdminMC::dbMCSources_OnEventHandler(ID_MC_EVENT={id_event.ToString ()}) - дата не актуальная; Day=[{day}], разн.(сутки)=[{difference (day).TotalDays}]..."
                            , Logging.INDEX_MESSAGE.NOT_SET);
                } else
                    ;
                // проверить сфорирован ли аргумент для события
                // , имеется ли обработчик; иначе из коллекции не смогут быть удалены элементы(удаление только из-вне)
                // , а значит коллекция увеличивается без ограничений, а элементы никаким образом не обрабатываются
                if ((Equals (argEventChanged, null) == false)
                    && (isHandlerMCEvent (id_event) == true)) {
                    AddEvent (argEventChanged);
                } else
                    ;
            } else if (typeof (Tuple<DbMCInterface.EVENT, bool>).IsAssignableFrom (obj.GetType ()) == true) {
                //TODO: ретранслировать для формы произошла подписка/отписка от события Модес-Центра
                tupleConnectHandler = obj as Tuple<DbMCInterface.EVENT, bool>;

                _dictNotify [DbMCInterface.ID_EVENT.HANDLER_CONNECT]?.Invoke (this, new EventArgs<bool>(DbMCInterface.TranslateEvent(tupleConnectHandler.Item1), DateTime.MinValue, new ReadOnlyCollection<bool> (new List<bool>() { tupleConnectHandler.Item2 })));
            } else if (typeof (bool).IsAssignableFrom (obj.GetType ()) == true) {
                id_event = DbMCInterface.ID_EVENT.Unknown;

                //TODO: проверить результат попытки установки соединения
                Logging.Logg ().Action ($"AdminMC::dbMCSources_OnEventHandler(ID_MC_EVENT={id_event.ToString ()}) - изменение состояния соединения УСТАНОВЛЕНО = {(bool)obj})...", Logging.INDEX_MESSAGE.NOT_SET);

                switch ((bool)obj == true ? 1 : 0) {
                    case 0:
                        _eventConnected.Reset ();
                        break;
                    case 1:
                        _eventConnected.Set ();
                        FetchEvent (false);
                        break;
                    default:
                        break;
                }
            } else
                throw new Exception (@"AdminMC::dbMCSources_OnEventHandler () - неизвестное событие от DbMCSources...");
        }

        private bool isConnected
        {
            get
            {
                return _eventConnected.WaitOne(0);
            }
        }

        private bool waitConnected (int msec = System.Threading.Timeout.Infinite)
        {
            // если указана бесконечность, то ожидать 'MAX_WATING'
            return _eventConnected.WaitOne (msec == System.Threading.Timeout.Infinite ? ASUTP.Core.Constants.MAX_WATING : msec);
        }

        protected override int StateRequest(int /*StatesMachine*/ state)
        {
            int result = 0;

            string msg = string.Empty;
            StatesMachine stateMachine = StatesMachine.Unknown;
            TECComponent comp;

            stateMachine = (StatesMachine)state;

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                    comp = CurrentDevice as TECComponent;

                    ActionReport ("Получение данных плана.");
                    getPPBRValuesRequest(comp.tec, comp, m_curDate.Date/*, AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS*/);
                    break;
                case StatesMachine.PPBRDates:
                    if ((serverTime.Date > m_curDate.Date)
                        && (m_ignore_date == false))
                    {
                        result = -1;
                        break;
                    }
                    else
                        ;
                    ActionReport("Получение списка сохранённых часовых значений.");
                    //GetPPBRDatesRequest(m_curDate);
                    break;

                case StatesMachine.MaketEquipment:
                    ActionReport ("Получение заголовков макетов.");
                    getMaketEquipmentRequest (_maketIdentifiers, m_curDate.Date);
                    break;
                default:
                    break;
            }

            //Logging.Logg().Debug(@"AdminMC::StateRequest () - state=" + state.ToString() + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
        
            return result;
        }

        protected override int StateCheckResponse(int /*StatesMachine*/ state, out bool error, out object table)
        {
            int iRes = -1;

            error = true;
            table = null;

            StatesMachine stateMachine = (StatesMachine)state;

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                case StatesMachine.PPBRDates:
                case StatesMachine.MaketEquipment:
                    //bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, false*/);
                    iRes = response(m_IdListenerCurrent, out error, out table/*, false*/);
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int /*StatesMachine*/ state, object table)
        {
            int result = -1;

            StatesMachine stateMachine = (StatesMachine)state;

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                    delegateStopWait();

                    result = getPPBRValuesResponse(table as DataTable, m_curDate);
                    if (result == 0)
                    {
                        readyData(m_curDate, true);
                    }
                    else
                        ;
                    break;
                case StatesMachine.PPBRDates:
                    clearPPBRDates();
                    result = getPPBRDatesResponse(table as DataTable, m_curDate);
                    if (result == 0)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.MaketEquipment:
                    delegateStopWait ();

                    result = getMaketEquipmentResponse (table as DataTable, m_curDate);
                    if (result == 0) {
                    } else
                        ;
                    break;
                default:
                    break;
            }

            if (result == 0)
                ReportClear(false);
            else
                ;

            //Logging.Logg().Debug(@"AdminMC::StateResponse () - state=" + state.ToString() + @", result=" + result.ToString() + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
            //tPBR.InsertData();
            return result;
        }

        protected override INDEX_WAITHANDLE_REASON StateErrors(int /*StatesMachine*/ state, int request, int result)
        {
            INDEX_WAITHANDLE_REASON reasonRes = INDEX_WAITHANDLE_REASON.SUCCESS;

            bool bClear = false;

            StatesMachine stateMachine = (StatesMachine)state;
            string mesError = string.Empty;

            delegateStopWait ();

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                    if (request == 0)
                        mesError = "Ошибка разбора данных плана. Переход в ожидание.";
                    else
                    {
                        mesError = "Ошибка получения данных плана. Переход в ожидание.";

                        bClear = true;
                    }
                    break;
                case StatesMachine.PPBRDates:
                    try
                    {
                        if (request == 0)
                        {
                            mesError = "Ошибка разбора сохранённых часовых значений (PPBR). Переход в ожидание.";
                            //saveResult = Errors.ParseError;
                        }
                        else
                        {
                            mesError = "Ошибка получения сохранённых часовых значений (PPBR). Переход в ожидание.";
                            //saveResult = Errors.NoAccess;
                        }

                        //semaDBAccess.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.MaketEquipment:
                    if (request == 0)
                        mesError = "Ошибка разбора содержания макетов. Переход в ожидание.";
                    else
                        mesError = "Ошибка получения содержания макетов. Переход в ожидание.";
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(mesError) == false)
                ErrorReport (mesError);

            if (bClear)
                ClearValues();

            errorData?.Invoke((int)stateMachine);

            return reasonRes;
        }

        protected override void StateWarnings(int state, int request, int result)
        {
        }

        public override bool Activate (bool active)
        {
            bool bRes = base.Activate (active);

            if (bRes == true)
                if (active == true)
                    FetchEvent (false);
                else
                    ;
            else
                ;

            return bRes;
        }

        public override void ClearValues ()
        {
            base.ClearValues ();
        }

        public void DebugEventReloadPlanValues ()
        {
            Action doWork = delegate () {
                DateTime datetimeReq =
                    //new DateTime (DateTime.UtcNow.Ticks, DateTimeKind.Unspecified)
                    //new DateTime (2018, 3, 24, 21, 0, 0, 0)
                    ASUTP.Core.HDateTime.ToMoscowTimeZone ()
                    ;

                List<Guid> identifiers = new List<Guid> () {
                    new Guid("9fa9a66e-0b49-4c1f-82ee-535992be2f88")
                    , new Guid("81c5dc9c-432a-47b9-9118-655052a55cf1")
                    , new Guid("83d4f3f5-021a-489c-8618-70b8b21ea7c0")
                    , new Guid("f9279a73-8fc7-4adf-b19b-7a1a97c28fe7")
                    , new Guid("65ea4ad6-d9dd-4a40-9f50-7e967c39c8c1")
                    , new Guid("a1946f77-d006-4250-b925-90ff1b5a8b30")
                    , new Guid("dfc38950-91e0-43b4-98cb-9b2acf5cfc06")
                    , new Guid("99fc4980-1509-43b9-87e3-bec1830f4a25")
                    , new Guid("615cd82e-66b0-4dfb-9aa8-bf7dc967cd0d")
                };

                identifiers.ForEach (guid => {
                    Thread.Sleep (4567);

                    dbMCSources_OnEventHandler (new object [] { DbMCInterface.ID_EVENT.PHANTOM_RELOAD_PLAN_VALUES
                        , new Modes.NetAccess.EventRefreshJournalMaket53500(guid
                            , datetimeReq
                            , ModesTaskType.OU
                            , -1)
                        , true // debug
                    });
                });
            };

            new Thread (new ThreadStart (doWork)).Start ();
        }

        public void ToDateRequest (DateTime date, int ms_sleep = 567)
        {
            Action doWork = delegate () {
                DateTime datetimeReq =
                    //new DateTime (DateTime.UtcNow.Ticks, DateTimeKind.Unspecified)
                    //new DateTime (2018, 3, 24, 21, 0, 0, 0)
                    ASUTP.Core.HDateTime.ToMoscowTimeZone()
                    ;

                foreach (TEC t in m_list_tec) {
                    Thread.Sleep (ms_sleep);

                    dbMCSources_OnEventHandler (new object [] { DbMCInterface.ID_EVENT.REQUEST_PLAN_VALUES
                        , new Modes.NetAccess.EventPlanDataChanged((PlanType)Enum.Parse(typeof(PlanType), getNamePBRNumber(ASUTP.Core.HDateTime.ToMoscowTimeZone().Hour))
                            , datetimeReq.Date
                            , datetimeReq
                            , 0
                            , t.name_MC)
                        , true // debug
                    });
                }
            };

            new Thread (new ThreadStart (doWork)).Start ();
        }

        public void GetMaketEquipment (FormChangeMode.KeyDevice key, EventArgs<Guid>identifiers, DateTime date)
        {
            delegateStartWait ();

            if (isConnected == false)
                waitConnected ();
            else
                ;

            lock (m_lockState) {
                ClearStates ();

                CurrentKey = key;
                _maketIdentifiers = new List<Guid>(identifiers.m_listParameters);

                ClearValues ();

                using_date = false;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                AddState ((int)StatesMachine.MaketEquipment);

                Run (@"AdminMC::GetMaketHeader ()");
            }
        }

        public override void GetRDGValues(FormChangeMode.KeyDevice key, DateTime date)
        {
            delegateStartWait();

            if (isConnected == false)
                waitConnected ();
            else
                ;

            lock (m_lockState)
            {
                ClearStates();

                CurrentKey = key;

                ClearValues();

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                //if (m_listIGO.Count == 0)
                //{
                //    AddState((int)StatesMachine.InitIGO);
                //}
                //else
                //    ;

                AddState((int)StatesMachine.PPBRValues);

                Run(@"AdminMC::GetRDGValues ()");
            }
        }
    }
}
