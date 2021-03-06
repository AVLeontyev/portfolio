using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Drawing; //Color


using System.Linq;
using ASUTP.Core;
using ASUTP;
using ASUTP.Database;
using static ASUTP.Control.HTabCtrlEx;

namespace StatisticCommon
{
    public class TecViewTMPower : TecView
    {
        public TecViewTMPower(FormChangeMode.KeyDevice key)
            : base(key, TECComponentBase.TYPE.ELECTRO)
        {
        }

        public override void ChangeState()
        {
            lock (m_lockState) { GetRDGValues(FormChangeMode.KeyDevice.Empty, DateTime.MinValue); }

            base.ChangeState();
        }

        public override void GetRDGValues(FormChangeMode.KeyDevice key, DateTime date)
        {
            ClearStates();

            if (m_tec.GetReadySensorsStrings(_type) == false)
                AddState((int)StatesMachine.InitSensors);
            else ;

            AddState((int)TecView.StatesMachine.CurrentTimeView);
            AddState((int)TecView.StatesMachine.LastValue_TM_Gen);
            AddState((int)TecView.StatesMachine.LastValue_TM_SN);
        }
    }

    public class TecViewStandard : TecView
    {
        public TecViewStandard(FormChangeMode.KeyDevice key)
            : base(key, TECComponentBase.TYPE.ELECTRO)
        {
        }

        public override void ChangeState()
        {
            lock (m_lockState) { GetRDGValues(CurrentKey, DateTime.MinValue); }

            base.ChangeState(); //Run
        }

        public override void GetRDGValues(FormChangeMode.KeyDevice key, DateTime date)
        {
            ClearStates();

            adminValuesReceived = false;

            if (m_tec.GetReadySensorsStrings(_type) == true)
            {
                if (currHour == true)
                {
                    AddState((int)StatesMachine.CurrentTimeView);
                }
                else
                {
                    //selectedTime = m_pnlQuickData.dtprDate.Value.Date;
                }
            }
            else
            {
                AddState((int)StatesMachine.InitSensors);
                AddState((int)StatesMachine.CurrentTimeView);
            }

            //????...
            if (m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO)
            {
                AddState((int)StatesMachine.Hours_Fact);
                if (currHour == true)
                    AddState((int)StatesMachine.Hour_TM);
                else
                    ;
            }
            else
                if (m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE)
                    AddState((int)StatesMachine.Hours_Fact);
                else
                    if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                        || (m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN))
                        AddState((int)StatesMachine.Hours_TM);
                    else
                        ;
            //??????...
            if (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO)
            {
                AddState((int)StatesMachine.CurrentMins_Fact);
                if (currHour == true)
                    AddState((int)StatesMachine.CurrentMin_TM);
                else
                    ;
            }
            else
                if (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                    AddState((int)StatesMachine.CurrentMins_Fact);
                else
                    if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                        || (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN))
                        AddState((int)StatesMachine.CurrentMins_TM);
                    else
                        ;
            if (m_bLastValue_TM_Gen == true)
                AddState((int)StatesMachine.LastValue_TM_Gen);
            else
                ;
            AddState((int)StatesMachine.LastMinutes_TM);
            AddState((int)StatesMachine.PPBRValues);
            AddState((int)StatesMachine.AdminValues);
        }

        // ??? ??? ?????????? ???????????? ? ?????? ?????????????? ?????? ???
        public enum PBR_STATE { NORM = 0, ERR = 1 }

        private PBR_STATE _PBRstate = PBR_STATE.ERR;

        public PBR_STATE PBRState
        {
            get {
                if (string.IsNullOrEmpty(lastLayout) == false)
                    if (currHour == true)
                        _PBRstate = (GetValidatePBR(lastLayout) == true)
                            ? PBR_STATE.NORM
                                : PBR_STATE.ERR;
                    else
                        _PBRstate = PBR_STATE.NORM;
                        //_PBRstate = (lastLayout.Equals(string.Format("???{0}", 24)) == true)
                        //    ? PBR_STATE.NORM
                        //        : _PBRstate = PBR_STATE.ERR;
                else
                    _PBRstate = PBR_STATE.ERR;

                return _PBRstate;
            }
        }

        public static int NUM_MINUTE_PBR_APPLY = 40;

        public static IEnumerable<string> EtalonPBR
        {
            get {
                List<string> listRes = new List<string>();

                DateTime datetimeMscTimezoneNow;
                int iNowMinute = -1
                    , iNowHour = -1;

                datetimeMscTimezoneNow = HDateTime.ToMoscowTimeZone();
                iNowHour = datetimeMscTimezoneNow.Hour;

                listRes.Add(string.Format("???{0}", iNowHour + 1));

                iNowMinute = datetimeMscTimezoneNow.Minute;
                if (iNowMinute > NUM_MINUTE_PBR_APPLY)
                    listRes.Add(string.Format("???{0}", iNowHour + 2));
                else
                    ;

                return listRes;
            }
        }

        public static bool GetValidatePBR(string pbr) { return TecViewStandard.EtalonPBR.Where(delegate (string etalonPBR) { return etalonPBR.Equals(pbr) == true; }).Count() == 1; }
    }

    public abstract class TecView : HAdmin
    {
        /// <summary>
        /// ???????????? - ?????????????? ????? ???????? ??????? ????????
        /// </summary>
        public enum ID_AISKUE_PARNUMBER : uint { FACT_03 = 2, FACT_30 = 12 };
        /// <summary>
        /// ?????? ???? ?????? ? ????????? ??????? ??????????? ??????? ??????
        ///  , ?????????? ???????????? ????????? ????? ????????
        /// </summary>
        public ID_AISKUE_PARNUMBER m_idAISKUEParNumber;
        /// <summary>
        /// ?????????(???????????) ??? ?????????? ??????? ?????? ??????? ????????
        ///  ? ~ ?? ?? ??????? ?????????? ? ?? (???????? ??????????????)
        /// </summary>
        public int PeriodMultiplier {
            get {
                return m_idAISKUEParNumber == ID_AISKUE_PARNUMBER.FACT_03 ? 1 :
                    m_idAISKUEParNumber == ID_AISKUE_PARNUMBER.FACT_30 ? 10 : -1;
            }
        }
        /// <summary>
        /// ??? ??? ????????????? ???????? ?????? ????????? ? ??????? ??????????????? ???????? ? ??????? "??? - ??????"
        /// ? ~ ?? ????? ????????? ??????????????
        /// </summary>
        public int IntervalMultiplier {
            get {
                return m_idAISKUEParNumber == ID_AISKUE_PARNUMBER.FACT_03 ? 1 :
                    m_idAISKUEParNumber == ID_AISKUE_PARNUMBER.FACT_30 ? 10 : -1;
            }
        }

        public TEC.TEC_TYPE TecType
        {
            get
            {
                return m_tec.Type;
            }
        }

        protected enum StatesMachine
        {
            InitSensors, //0 - ????????????? ????? ? ???????????????? ??? (??) ??? ??????????? ????????????? ? ????????
            CurrentTimeAdmin, //1 - ????? ???????, ???????? ??????: ?????? ? ????????????????? ??????????
            CurrentTimeView, //2 - ????? ???????, ???????? ??????: ...
            Hour_TM, //3 - ??????? ???, ????????
            Hours_Fact, //4 - ????????? ?????, ??????
            CurrentMin_TM, //??????? ???????????? ??????? (3 ??? 1???), ???????? - ???????????
            CurrentMinDetail_TM, //??????? ???????????? ??????? (1???!), ????????
            RetroMinDetail_TM, //??????????????? ???????????? ??????? (1???!), ????????
            CurrentMins_Fact, //??????? ?????/???, ??????
            Hours_TM, //????????? ?????, ????????
            CurrentMins_TM, //10 - ??????? ?????/???, ????????
            CurrentHours_TM_SN_PSUM, //11- ??????? ????? ??? ??????????? ?????, ????????
            LastValue_TM_Gen, //12 - ??????? ???????? ??? ?????????, ???????? - ?????? ??????????? ??????????
            LastValue_TM_SN, //13 - ??????? ???????? ??? ??????????? ?????, ????????
            LastMinutes_TM, //???????? ??????? ????? ???? ?? ????????? ?????, ????????
            //RetroHours,
            RetroMins_Fact, //????????? ?????/???, ??????
            RetroMin_TM_Gen, //????????? ?????/???, ???????? - ?????? ??????????? ??????????
            RetroMins_TM, //????????? ?????/???, ????????
            //AdminDates, //????????? ?????? ??????????? ??????? ????????
            //PPBRDates,
            HoursTMTemperatureValues, //??????????? ?????????? ?????
            HoursVzletTDirectValues, //???????????, ?????? ??????, ?????-????????, ????? ??-???????, ?????
            CurrentVzletTDirectValues, //???????????, ?????? ??????, ?????-????????, ???????, ?????
            AdminValues, //????????? ????????????????/??? ????????
            PPBRValues,
        }

        public DelegateFunc setDatetimeView;
        public IntDelegateIntIntFunc updateGUI_Fact;
        public DelegateFunc updateGUI_TM_Gen
            , updateGUI_TM_SN
            , updateGUI_LastMinutes;

        public double m_dblTotalPower_TM_SN;
        public DateTime m_dtLastChangedAt_TM_Gen;
        public DateTime m_dtLastChangedAt_TM_SN;
        public static int SEC_VALIDATE_TMVALUE = -1;
        public double [] m_arValueCurrentTM_Gen;

        //private AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

        protected DataTable m_tablePPBRValuesResponse
                    //, m_tableRDGExcelValuesResponse
                    ;

        public abstract class values
        {
            public double valuesLastMinutesTM;

            public double valuesPBR;
            public bool valuesForeignCommand;
            public double valuesPmin;
            public double valuesPmax;
            public double valuesPBRe;
            public double valuesUDGe;

            public double valuesDiviation; //???????? ? ??.???.

            public double valuesREC;
        }

        public class valuesLowPointDev : Object {
            /// <summary>
            ///  ??? ???./???????? ? ??????? ??????
            /// </summary>
            public double[] m_powerSeconds;
            /// <summary>
            ///  ??? ???./???????? ? ??????? ????
            /// </summary>
            public double[] m_powerMinutes;
            /// <summary>
            ///  ??? ???./???????? ? ??????? ????
            /// </summary>
            public bool m_bPowerMinutesRecieved;
            /// <summary>
            ///  ??? ????/??????? ????????? ???????? ???????? ??
            /// </summary>
            public DateTime m_dtCurrent_TM;
            /// <summary>
            ///  ??? ???????? ???????? ??
            /// </summary>
            public double m_powerCurrent_TM;
            /// <summary>
            /// ????????????? ????????? ???????? ????????
            /// </summary>
            public int m_id_TM; 
            /// <summary>
            ///  ??? 59-? ??? ??????? ????
            /// </summary>
            public double [] m_power_LastMinutesTM;
        }

        public class valuesTECComponent : values
        {
            //public volatile double[] valuesREC;
            /// <summary>
            /// ??????? ??.???. 'valuesDIV'
            /// </summary>
            public double valuesISPER;
            /// <summary>
            /// ???????? ?? ??
            /// </summary>
            public double valuesDIV;
        }

        public class valuesTEC : values
        {
            public double valuesFact;
            public double valuesTMSNPsum;
        }

        public object m_lockValue;

        //'public' ??? ??????? ?? ??????? m_panelQuickData ?????? 'PanelQuickData'
        public volatile bool currHour;
        //private bool m_bCurrHour; public volatile bool CurrHour { }

        public volatile int lastHour;
        public volatile int lastReceivedHour;
        public volatile int lastMin;
        /// <summary>
        /// ???????????? - ???????(????) ??? ??????? 'm_markWarning'
        /// </summary>
        public enum INDEX_WARNING { LAST_MIN, LAST_HOUR, LAST_HOURHALF, CURR_MIN_TM_GEN
                                    , COUNT_WARNING };
        /// <summary>
        /// ?????? ??? ???????? ????????? ??????? ????????? 'INDEX_WARNING' ????? ??????????????
        /// </summary>
        public HMark m_markWarning;

        public volatile string lastLayout;

        //'public' ??? ??????? ?? ??????? m_panelQuickData ?????? 'PanelQuickData'
        public volatile valuesTEC[] m_valuesMins;
        public volatile valuesTEC[] m_valuesHours;

        //public volatile int m_indx_TEC;
        //public volatile int m_indx_TECComponent;

        /// <summary>
        /// ?????? ??????????? ??????????? ??? ??????????? ??????? ???????????
        /// </summary>
        protected List <TECComponentBase> _localTECComponents;
        public List <TECComponentBase> LocalTECComponents {
            get {
                return /*_type == TECComponentBase.TYPE.TEPLO ? _localParamVyvod :
                    _type == TECComponentBase.TYPE.ELECTRO ?*/ _localTECComponents /*: null*/
                    ;
            }
        }
        public volatile Dictionary<int, TecView.valuesTECComponent> [] m_dictValuesTECComponent;
        public volatile Dictionary<int, TecView.valuesLowPointDev>m_dictValuesLowPointDev;

        public CONN_SETT_TYPE[] m_arTypeSourceData;
        public int[] m_arIdListeners; //?????????????? ??????? ???????? ???????????? ?

        //'public' ??? ??????? ?? ??????? m_panelQuickData ?????? 'PanelQuickData'
        public double recomendation;

        //'public' ??? ??????? ?? ??????? m_panelQuickData ?????? 'PanelQuickData'
        public volatile bool adminValuesReceived;

        //'public' ??? ??????? ?? ??????? m_panelQuickData ?????? 'PanelQuickData'
        public volatile bool recalcAver;

        //'public' ??? ??????? ?? ??????? m_panelQuickData ?????? 'PanelQuickData'
        public volatile bool m_bLastValue_TM_Gen;

        public StatisticCommon.TEC m_tec {
            get { return m_list_tec [0]; }
        }

        public string NameShr
        {
            get
            {
                return ((CurrentKey.Mode == FormChangeMode.MODE_TECCOMPONENT.Unknown)
                    || (CurrentKey.Mode == FormChangeMode.MODE_TECCOMPONENT.ANY))
                    ? $"??????, ?????=<{CurrentKey.Mode}>"
                        : (CurrentKey.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                            ? m_tec.name_shr
                                : $"{m_tec.name_shr} - {m_tec.ListTECComponents.Find(comp => comp.m_id == CurrentKey.Id).name_shr}";
            }
        }

        public List<TECComponentBase> ListLowPointDev
        {
            get
            {
                if (CurrentKey.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                    return m_tec.GetListLowPointDev (_type);
                else
                    return m_tec.ListTECComponents.FirstOrDefault(comp => comp.m_id == CurrentKey.Id).ListLowPointDev;
            }
        }

        public int CountLowPointDev
        {
            get
            {
                return ListLowPointDev.Count;
            }
        }

        protected override void Initialize () {
            base.Initialize ();

            m_markWarning = new HMark (0);

            currHour = true;
            lastHour = 0;
            lastMin = 0;

            recalcAver = true;

            m_lockValue = new object();

            m_bLastValue_TM_Gen = false;

            m_valuesMins = new valuesTEC [21];
            m_valuesHours = new valuesTEC [24];

            m_dictValuesLowPointDev = new Dictionary<int,valuesLowPointDev> ();

            //_localParamVyvod = new List<TECComponentBase>();
            _localTECComponents = new List<TECComponentBase>();
            //tgsName = new List<System.Windows.Forms.Label>();

            m_arTypeSourceData = new CONN_SETT_TYPE [(int)HDateTime.INTERVAL.COUNT_ID_TIME];

            m_arValueCurrentTM_Gen = new double[(int)HDateTime.INTERVAL.COUNT_ID_TIME] { -1F, -1F };

            ClearStates();
        }

        public void InitializeTECComponents () {
            //int positionXName = 515, positionXValue = 504, positionYName = 6, positionYValue = 19;
            //countTG = 0;
            List<int> tg_ids = new List<int>(); //????????? ?????? ??????????????? ??

            //_localParamVyvod.Clear();
            //07.09.2015 ??? ????? ??????????? (PanelSOSTIASSO) - ??????????????? ?? ????? ??????????
            _localTECComponents.Clear ();
            m_dictValuesLowPointDev.Clear ();

            if (CurrentKey.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC) // ?????? ???? view ????? ????????? ??? ???? ???
            {
                switch (_type)
                {
                    case TECComponentBase.TYPE.TEPLO:
                        m_tec.ListTECComponents.ForEach(v => {
                            if (v.IsVyvod == true) {
                                _localTECComponents.Add(v);
                                initDictValuesLowPointDev(v);
                            } else
                                ;
                        });
                        break;
                    case TECComponentBase.TYPE.ELECTRO:
                        foreach (TECComponent tc in m_tec.ListTECComponents)
                        {
                            if (tc.Type == TECComponentBase.TYPE.ELECTRO)
                            {// ?????? ??? ???????-???????????
                                if ((tc.IsGTP == true)
                                    || (tc.IsGTP_LK == true))
                                {
                                    _localTECComponents.Add(tc);

                                    //m_dictValuesTECComponent.Add (c.m_id, new valuesTECComponent(25));
                                }
                                else
                                    ;

                                initDictValuesLowPointDev(tc);
                            }
                            else
                                ; // ????????? ?????
                        }
                        break;
                    default:
                        break;
                }
            }
            else
                foreach (TG tg in ListLowPointDev)
                    foreach (TECComponent c in m_tec.ListTECComponents)
                        if (tg.m_id == c.m_id) {
                            _localTECComponents.Add(c);

                            initDictValuesLowPointDev(c);
                        }
                        else
                            ;

            initDictValuesTECComponent (24 + 1);
        }

        //private List<TECComponentBase> getLocalTECComponents()
        //{
        //    List<TECComponentBase> listRes = null;

        //    switch (_type)
        //    {
        //        case TECComponentBase.TYPE.TEPLO:
        //            //listRes = new List<TECComponentBase>();
        //            //foreach (TECComponent tc in m_tec.list_TECComponents)
        //            //    foreach (TECComponentBase tcb in tc.ListLowPointDev)
        //            //        if ((tc.IsVyvod == true)
        //            //            && ((tcb as Vyvod.ParamVyvod).m_id_param == Vyvod.ID_PARAM.T_PV))
        //            //            listRes.Add(tcb);
        //            //        else
        //            //            ;
        //            listRes = _localTECComponents;
        //            break;
        //        case TECComponentBase.TYPE.ELECTRO:
        //            listRes = _localTECComponents;
        //            break;
        //        default:
        //            break;
        //    }

        //    return listRes;
        //}

        private void initDictValuesTECComponent(int cnt)
        {
            //List<TECComponentBase> localTECComponents = getLocalTECComponents();
            
            m_dictValuesTECComponent = new Dictionary<int, valuesTECComponent>[cnt];
            
            foreach (TECComponentBase c in _localTECComponents)
                for (int i = 0; i < m_dictValuesTECComponent.Length; i ++) {
                    if (m_dictValuesTECComponent[i] == null) m_dictValuesTECComponent[i] = new Dictionary<int,valuesTECComponent> (); else ;
                    m_dictValuesTECComponent[i].Add(c.m_id, new TecView.valuesTECComponent ());
                }
        }

        protected virtual void initDictValuesLowPointDev(TECComponent comp)
        {
            foreach (TECComponentBase dev in comp.ListLowPointDev)
                if (m_dictValuesLowPointDev.ContainsKey(dev.m_id) == false)
                    m_dictValuesLowPointDev.Add(dev.m_id, new valuesLowPointDev());
                else
                    ;
        }

        //private void initDictValuesParamVyvod(TECComponent v)
        //{
        //    //foreach (Vyvod.ParamVyvod pv in v.m_listParam)
        //    foreach (Vyvod.ParamVyvod pv in v.ListLowPointDev)
        //        if (m_dictValuesLowPointDev.ContainsKey(pv.m_id) == false)
        //            m_dictValuesLowPointDev.Add(pv.m_id, new valuesLowPointDev());
        //        else
        //            ;
        //}

        //public TecView(bool[] arMarkSavePPBRValues, TYPE_PANEL type, int indx_tec, int indx_comp)
        public TecView(FormChangeMode.KeyDevice key, TECComponentBase.TYPE type)
            : base(type)
        {
            m_idAISKUEParNumber = ID_AISKUE_PARNUMBER.FACT_03;

            CurrentKey = key;

            m_arIdListeners = new int[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            for (int i = (int)CONN_SETT_TYPE.ADMIN; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                m_arIdListeners[i] = -1;
            }
        }        

        public TECComponent TECComponentCurrent { get { return allTECComponents.FirstOrDefault(comp => comp.m_id == CurrentKey.Id); } }
        /// <summary>
        /// ?????????? ??????? ??????? ???????? ?? ????????? ???
        /// </summary>
        /// <param name="hour">????? ????</param>
        /// <returns>??????? ??????? ????????</returns>
        public bool IsHourValues(int hour)
        {
            return (!(m_valuesHours == null))
                && ((hour < m_valuesHours.Length) && (!(hour < 0)))
                && (!(m_valuesHours[hour] == null));
        }
        /// <summary>
        /// ?????????? ????????? ???????? ???????? ??????? ???????????
        ///  (?? ?????????/??????? ??? ? ??????? ???????? ??????????????)
        /// </summary>
        /// <param name="hour">????? ????</param>
        /// <returns>????????? ????????</returns>
        public virtual double GetSummaFactValues(int hour = -1) //lastHour
        {
            double dblRes = 0F;

            int id = -1
                , min = lastMin;

            if (!(min == 0)) min--; else ;

            for (int i = 0; i < ListLowPointDev.Count; i++)
            {
                id = ListLowPointDev[i].m_id;

                if (m_dictValuesLowPointDev[id].m_bPowerMinutesRecieved == false)
                    //?? ????????? ??, ??? ???????? ?? ???? ???????? ?? ?????? ????????
                    continue;
                else
                    ;

                if (!(m_dictValuesLowPointDev[id].m_powerMinutes[min] < 0))
                    if (m_dictValuesLowPointDev[id].m_powerMinutes[min] > 1)
                        dblRes += m_dictValuesLowPointDev[id].m_powerMinutes[min];
                    else ;
                else
                {
                    //bMinValuesReceived = false;

                    //break;
                }
            }

            return dblRes;
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (bRes == true)
                ClearStates();
            else
                ;

            return bRes;
        }

        private string getDbInterfaceIdListeners (/*string func*/)
        {
            string dbgMes = string.Empty;

            if (m_dictIdListeners.Count > 0) {
                dbgMes += "[ ";

                foreach (int iConnSett in m_dictIdListeners.Keys)
                    dbgMes += string.Format ("[{0}], ", string.Join (", ", Array.ConvertAll (m_dictIdListeners [iConnSett], i => i.ToString ())));

                dbgMes = dbgMes.Remove (dbgMes.Length - 2, 2); // ?????? ??????? + ??????
                dbgMes += " ]";
            } else
                dbgMes = "?? ?????????";

            //Console.WriteLine (string.Format ("TecView.{0}() - {1}...", func, dbgMes));
            return dbgMes;
        }

        public override void Start()
        {
            ClearValues();

            base.Start ();

            StartDbInterfaces ();

            // ??? ???????? ?????????? ?????????? ??? ?????????? ??????????????? ????????
            //consoleDebugDbInterfaceIdListeners ("Start");
            Logging.Logg().Debug(string.Format("TecView::Start() - ?????????????? ???????? ????????? ? ?? = {0}...", getDbInterfaceIdListeners()), Logging.INDEX_MESSAGE.NOT_SET);
        }

        public override void Stop()
        {
            // ??? ???????? ?????????? ?????????? ??? ?????????? ??????????????? ????????
            //consoleDebugDbInterfaceIdListeners ("Stop");
            Logging.Logg ().Debug (string.Format ("TecView::Stop() - ?????????????? ???????? ????????? ? ?? = {0}...", getDbInterfaceIdListeners ()), Logging.INDEX_MESSAGE.NOT_SET);

            base.Stop();
        }

        public override void InitTEC(List<StatisticCommon.TEC> listTEC, ASUTP.Core.HMark markQueries)
        {
            base.InitTEC(listTEC, markQueries);

            InitializeTECComponents ();
        }

        public void ReInitTEC(StatisticCommon.TEC tec, FormChangeMode.KeyDevice key, ASUTP.Core.HMark markQueries)
        {
            CurrentKey = key;

            InitTEC (new List<TEC> () { tec }, markQueries);
        }

        public override bool WasChanged()
        {
            throw new NotImplementedException();
        }

        //public override void  ClearValues(int cnt = -1)
        public override void  ClearValues()
        {
            //Debug.WriteLine(@"TecView::ClearValues () - ????...");

            ClearValuesMins();
            //ClearValuesHours(cnt);
            ClearValuesHours();

            ClearValuesLastMinutesTM();
        }

        public void ClearValuesLastMinutesTM()
        {
            //Debug.WriteLine(@"TecView::ClearValuesLastMinutesTM () - ????...");

            int id = -1;
            //List<TECComponentBase> localTECComponents = getLocalTECComponents();

            try
            {
                if (!(m_dictValuesTECComponent.Length - m_valuesHours.Length == 1))
                {
                    m_dictValuesTECComponent = null;
                    m_dictValuesTECComponent = new Dictionary<int, valuesTECComponent>[m_valuesHours.Length + 1];
                }
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"TecView::ClearValuesLastMinutesTM ()- ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            for (int i = 0; i < (m_valuesHours.Length + 1); i++)
            {
                if (i < m_valuesHours.Length)
                    m_valuesHours[i].valuesLastMinutesTM = 0.0;
                else
                    ;

                _localTECComponents.ForEach(g =>
                //localTECComponents.ForEach(g =>
                {
                    //if (g is TECComponent)
                        foreach (TECComponentBase tc in (g as TECComponent).ListLowPointDev)
                        {
                            id = tc.m_id;

                            if (m_dictValuesLowPointDev[id].m_power_LastMinutesTM == null)
                            // 1-? ????????? ??????
                                m_dictValuesLowPointDev[id].m_power_LastMinutesTM = new double[m_dictValuesTECComponent.Length + 0];
                            else
                            // ????????? ????????? ??????
                                if (!(m_dictValuesLowPointDev[id].m_power_LastMinutesTM.Length == m_dictValuesTECComponent.Length))
                                {// ?????? ? ?????? ???????? ? ???-?? ?????????
                                    m_dictValuesLowPointDev[id].m_power_LastMinutesTM = null;
                                    m_dictValuesLowPointDev[id].m_power_LastMinutesTM = new double[m_dictValuesTECComponent.Length + 0];
                                }
                                else
                                    ;

                            m_dictValuesLowPointDev[id].m_power_LastMinutesTM[i] = 0F;
                        }
                    //else ;

                    switch (_type)
                    {
                        case TECComponentBase.TYPE.TEPLO:
                            id = g.m_id;
                            break;
                        case TECComponentBase.TYPE.ELECTRO:
                            id = g.m_id;
                            break;
                        default:
                            break;
                    }

                    if (m_dictValuesTECComponent[i] == null) m_dictValuesTECComponent[i] = new Dictionary<int, valuesTECComponent>(); else ;

                    if (m_dictValuesTECComponent[i].ContainsKey(id) == false)
                        m_dictValuesTECComponent[i].Add(id, new valuesTECComponent());
                    else
                        ;

                    if (m_dictValuesTECComponent[i][id] == null) m_dictValuesTECComponent[i][id] = new valuesTECComponent(); else ;

                    m_dictValuesTECComponent[i][id].valuesLastMinutesTM = 0.0;
                });
            }
        }

        #region ?????????????? ??????? - ?????????? ??? ????? 'Admin'
        public override void CopyCurToPrevRDGValues()
        {//!!! ?? ????????????
        }

        public override void getCurRDGValues(HAdmin source)
        {//!!! ?? ????????????
        }

        protected override void getPPBRDatesRequest(DateTime date)
        {//!!! ?? ????????????            
        }

        protected override void getPPBRValuesRequest(StatisticCommon.TEC t, IDevice comp, DateTime date/*, AdminTS.TYPE_FIELDS mode*/)
        {//!!! ?? ????????????
        }

        protected override int getPPBRDatesResponse(System.Data.DataTable table, DateTime date)
        {//!!! ?? ????????????
            int iRes = 0;

            return iRes;
        }

        protected override int getPPBRValuesResponse(System.Data.DataTable table, DateTime date)
        {//!!! ?? ????????????
            int iRes = 0;

            return iRes;
        }
        #endregion

        private bool GetSensorsTEC()
        {
            if (m_tec.GetReadySensorsStrings(_type) == false) {
                m_tec.InitSensorsTEC ();
            }
            else
                ;

            return m_tec.GetReadySensorsStrings(
                //??? CurrentKey.TECComponentType 'Unknown' ??? TEC
                _type
            );
        }

        private void getCurrentTMGenRequest()
        {
            //Debug.WriteLine ($"::LAST_VALUE_TM_GET_REQUEST (iListenerId={m_dictIdListeners [m_tec.m_id] [(int)CONN_SETT_TYPE.DATA_SOTIASSO]}, id_tec={m_tec.m_id}, indxTECComponent={indxTECComponents}) - ...");

            Request (m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.currentTMRequest(m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO)));
        }

        private int getCurrentTMGenResponse(DataTable table)
        {
            //Debug.WriteLine ($"::LAST_VALUE_TM_GET_RESPONSE (iListenerId={m_dictIdListeners [m_tec.m_id] [(int)CONN_SETT_TYPE.DATA_SOTIASSO]}, id_tec={m_tec.m_id}, indxTECComponent={indxTECComponents}) - ...");

            int iRes = 0;
            int i = -1
                , id = -1
                , id_tm = -1;
            string kks_name = string.Empty;
            float value = -1;
            DateTime dtLastChangedAt = m_dtLastChangedAt_TM_Gen
                , dtServer = serverTime.Add(-HDateTime.TS_MSK_OFFSET_OF_UTCTIMEZONE);
            TG tgTmp;

            foreach (TECComponent g in _localTECComponents)
            {
                foreach (TECComponentBase tc in g.ListLowPointDev)
                {
                    id = tc.m_id;

                    m_dictValuesLowPointDev[id].m_powerCurrent_TM = -1F;
                    m_dictValuesLowPointDev[id].m_dtCurrent_TM = DateTime.MinValue;
                    m_dictValuesLowPointDev[id].m_id_TM = -1;
                }
            }


            if (table.Columns.Count > 0) {
                iRes = CheckNameFieldsOfTable(table, new string[] { @"KKS_NAME", @"value", @"last_changed_at", @"ID_SOURCE" }) == true ? 0 : -1;

                if (iRes == 0)
                {
                    for (i = 0; i < table.Rows.Count; i++)
                    {
                        kks_name = table.Rows[i]["KKS_NAME"].ToString();

                        tgTmp = m_tec.FindTGById(kks_name, TG.INDEX_VALUE.TM, (HDateTime.INTERVAL)(-1));

                        if (tgTmp == null)
                            return -1;
                        else
                            ;

                        if (!(table.Rows[i]["value"] is DBNull))
                            if (float.TryParse(table.Rows[i]["value"].ToString(), out value) == false)
                                return -1;
                            else
                                ;
                        else
                            value = -1F;

                        //????????? ????/????? ??? "??????????" (>= 1) ????????
                        //if ((!(value < 1)) && (DateTime.TryParse(table.Rows[i]["last_changed_at"].ToString(), out dtLastChangedAt) == false))
                        if ((!(value < 1)) && (!(table.Rows[i]["last_changed_at"].GetType() == typeof(DateTime))))
                            //?????? ?????????? ????/????? ??? "??????????" (>= 1) ????????
                            return -1;
                        else
                            dtLastChangedAt = (DateTime)table.Rows[i]["last_changed_at"];

                        if ((!(table.Rows[i]["ID_SOURCE"] is DBNull))
                            && (table.Rows[i]["ID_SOURCE"].GetType () == typeof(int)))
                            id_tm = (int)table.Rows[i]["ID_SOURCE"];
                        else
                            id_tm = -1;

                        if (m_dtLastChangedAt_TM_Gen > dtLastChangedAt) {
                            m_dtLastChangedAt_TM_Gen = dtLastChangedAt;

                            if ((!(value < 1)) && (ValidateDatetimeTMValue(dtServer, m_dtLastChangedAt_TM_Gen) == false)
                                && (m_markWarning.IsMarked((int)TecView.INDEX_WARNING.CURR_MIN_TM_GEN) == false))
                            {
                                m_markWarning.Marked((int)TecView.INDEX_WARNING.CURR_MIN_TM_GEN);
                                iRes = 1;

                                Logging.Logg().Warning(@"TecView::GetCurrentTMGenResponse (" + CurrentKey + @") - currentMinuteTM_GenWarning=" + true.ToString ()
                                    , Logging.INDEX_MESSAGE.W_001);

                                //return true;
                                //break; //bRes ??-???????? == true ???
                            }
                            else
                                ;
                        }
                        else
                            ;

                        switch (m_tec.Type)
                        {
                            case StatisticCommon.TEC.TEC_TYPE.COMMON:
                                break;
                            case StatisticCommon.TEC.TEC_TYPE.BIYSK:
                                //value *= 20;
                                break;
                            default:
                                break;
                        }

                        if (!(m_dictValuesLowPointDev[tgTmp.m_id].m_powerCurrent_TM == value))
                            // ???????? ?? ?????????
                            if (! (value < 0))
                            {// ?????? ??? = 0
                                m_dictValuesLowPointDev[tgTmp.m_id].m_dtCurrent_TM = HDateTime.ToMoscowTimeZone(dtLastChangedAt);

                                //if (!(value < 1))
                                //    // ?????? ??? = 1
                                //    ;
                                //else ; // ?????? 1

                                m_dictValuesLowPointDev[tgTmp.m_id].m_powerCurrent_TM = value;
                                m_dictValuesLowPointDev[tgTmp.m_id].m_id_TM = id_tm;
                            }
                            else
                                ; // ?????? 0
                        else
                            ; // ???????? ?????????
                    }

                    if (! (m_dtLastChangedAt_TM_Gen == DateTime.MaxValue))
                        //?????????????? ?? UTC ? ??? ??? ? 26.10.2014 ?. ? ?? ?????? ?? ??? !!! ??? ???????? "??? ????"
                        try { m_dtLastChangedAt_TM_Gen = HDateTime.ToMoscowTimeZone(m_dtLastChangedAt_TM_Gen); }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"TecView::GetCurrentTMGenResponse () - HAdmin.ToCurrentTimeZone () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    else
                        ;
                }
                else
                    Logging.Logg().Error($"TecView::GetCurrentTMGenResponse () - ??? ID={m_tec.m_id}, ?? ?????? ???? ??? ????????? ?? ????????? ???????? ...", Logging.INDEX_MESSAGE.NOT_SET);
            } else
                Logging.Logg().Error($"TecView::GetCurrentTMGenResponse () - ??? ID={m_tec.m_id}, ? ???????-?????????? ??? ?? ?????? ??????? ...", Logging.INDEX_MESSAGE.NOT_SET);

            return iRes;
        }

        public static bool ValidateDatetimeTMValue (DateTime dt_srv, DateTime dt_val) {
            if (SEC_VALIDATE_TMVALUE > 0)
            {
                int delta = (int)(dt_srv - dt_val).TotalSeconds;
                return (!(delta > SEC_VALIDATE_TMVALUE)) || (delta < 0);
            }
            else
                return true;
        }

        private void getCurrentTMSNRequest()
        {
            //Debug.WriteLine ($"::LAST_VALUE_TM_SN_REQUEST (iListenerId={m_dictIdListeners [m_tec.m_id] [(int)CONN_SETT_TYPE.DATA_SOTIASSO]}, id_tec={m_tec.m_id}) - ...");

            Request (m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.currentTMSNRequest());
        }

        private int getCurrentTMSNResponse(DataTable table)
        {
            //Debug.WriteLine ($"::LAST_VALUE_TM_SN_RESPONSE (iListenerId={m_dictIdListeners [m_tec.m_id] [(int)CONN_SETT_TYPE.DATA_SOTIASSO]}, id_tec={m_tec.m_id}) - ...");

            int iRes = 0;
            int id = -1;

            m_dtLastChangedAt_TM_SN = DateTime.Now;

            iRes = CheckNameFieldsOfTable(table, new string[] { @"ID_TEC", @"SUM_P_SN", @"LAST_UPDATE" }) == true ? 0 : -1;
            if ((iRes == 0) && (table.Rows.Count == 1))
            {
                if (int.TryParse(table.Rows[0]["ID_TEC"].ToString(), out id) == false)
                    return -1;
                else
                    ;

                if (!(table.Rows[0]["SUM_P_SN"] is DBNull))
                    if (double.TryParse(table.Rows[0]["SUM_P_SN"].ToString(), out m_dblTotalPower_TM_SN) == false)
                        return -1;
                    else
                        ;
                else
                    m_dblTotalPower_TM_SN = 0.0;

                if ((!(m_dblTotalPower_TM_SN < 1)) && (DateTime.TryParse(table.Rows[0]["LAST_UPDATE"].ToString(), out m_dtLastChangedAt_TM_SN) == false))
                    return -1;
                else
                    ;
            }
            else
            {
                iRes = -1;
            }

            return iRes;
        }
            
        protected override int StateCheckResponse(int state, out bool error, out object table)
        {
            int iRes = 0;
            
            error = false;
            table = null;

            StatesMachine stateMachine = (StatesMachine)state;

            switch (stateMachine)
            {
                case StatesMachine.InitSensors:
                    //????? ??? ??????? ? ?? ? 'StateResponse'
                    //?????? ?? ?????????, ?.?. ????????? ???????? ??? ?????? 'InitTEC'
                    break;
                case StatesMachine.CurrentTimeAdmin:
                case StatesMachine.CurrentTimeView:
                case StatesMachine.Hours_Fact:
                case StatesMachine.Hour_TM:
                case StatesMachine.Hours_TM:
                case StatesMachine.CurrentMins_Fact:
                case StatesMachine.CurrentMin_TM:
                case StatesMachine.CurrentMinDetail_TM:
                case StatesMachine.RetroMinDetail_TM:
                case StatesMachine.CurrentMins_TM:
                case StatesMachine.CurrentHours_TM_SN_PSUM:
                case StatesMachine.LastValue_TM_Gen:
                case StatesMachine.LastValue_TM_SN:
                case StatesMachine.LastMinutes_TM:
                //case StatesMachine.RetroHours:
                case StatesMachine.RetroMins_Fact:
                case StatesMachine.RetroMin_TM_Gen:
                case StatesMachine.RetroMins_TM:
                case StatesMachine.HoursTMTemperatureValues:
                case StatesMachine.HoursVzletTDirectValues:
                case StatesMachine.CurrentVzletTDirectValues:
                //case StatesMachine.PPBRDates:
                case StatesMachine.PPBRValues:
                //case StatesMachine.AdminDates:
                case StatesMachine.AdminValues:
                    //bRes = Response(m_IdListenerCurrent, out error, out table);
                    iRes = response(out error, out table);
                    break;
                default:
                    iRes = -1;
                    break;
            }

            error = ! (iRes == 0);

            return iRes;
        }

        private string getNameInterval () {
            string strRes = string.Empty;
            switch (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES])
            {
                case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                    strRes = @"3";
                    break;
                case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                    strRes = @"1";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        protected override ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON StateErrors (int state, int request, int result)
        {
            ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON reasonRes = ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON.SUCCESS;
            
            string reason = string.Empty,
                    waiting = string.Empty,
                    msg = string.Empty;

            switch ((StatesMachine)state)
            {
                case StatesMachine.InitSensors:
                    reason = @"????????? ??????????????? ????????";
                    waiting = @"??????? ? ????????";
                    //AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    reasonRes = ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON.ERROR;
                    break;
                case StatesMachine.CurrentTimeAdmin:
                case StatesMachine.CurrentTimeView:
                    //if (request == 0)
                    //{
                    //    reason = @"???????";
                    //}
                    //else
                    //{
                    //    reason = @"?????????";
                    //}

                    reason = @" ???????? ??????? ???????";
                    waiting = @"??????? ? ????????";

                    break;
                case StatesMachine.Hours_Fact:
                    reason = @"??????????? ????????";
                    waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";
                    //AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    reasonRes = ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON.ERROR;
                    break;
                case StatesMachine.Hour_TM:
                    reason = @"??????. ?? ??? ????????????";
                    waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";
                    break;
                case StatesMachine.Hours_TM:
                    reason = @"??????? ????????";
                    waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";
                    //AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    reasonRes = ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON.ERROR;
                    break;
                case StatesMachine.CurrentMins_Fact:
                    reason = @"3-? ???????? ????????";
                    waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";
                    //AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    reasonRes = ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON.ERROR;
                    break;
                case StatesMachine.CurrentMin_TM:
                    reason = @"??????. ?? ???????? ????????????";
                    waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";
                    break;
                case StatesMachine.CurrentMinDetail_TM:
                case StatesMachine.RetroMinDetail_TM:
                    reason = @"?????????? ?? ???????? ????????????";
                    waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";
                    break;
                case StatesMachine.CurrentMins_TM:
                    reason += getNameInterval () + @"-???????? ????????";
                    waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";
                    //AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    reasonRes = ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON.ERROR;
                    break;
                case StatesMachine.CurrentHours_TM_SN_PSUM:
                    reason = @"??????? ???????? (??????. ?????)";
                    waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";
                    break;
                case StatesMachine.LastValue_TM_Gen:
                    reason = @"??????? ???????? (?????????)";
                    waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";
                    //AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    reasonRes = ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON.ERROR;
                    break;
                case StatesMachine.LastValue_TM_SN:
                    reason = @"??????? ???????? (??????. ?????)";
                    waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";
                    break;
                case StatesMachine.LastMinutes_TM:
                    reason = @"??????? ???????? 59 ???.";
                    waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";
                    break;
                //case StatesMachine.RetroHours:
                //    reason = @"??????????? ????????";
                //    waiting = @"??????? ? ????????";
                //    break;
                case StatesMachine.RetroMins_Fact:
                    reason = @"3-? ???????? ????????";
                    waiting = @"??????? ? ????????";
                    break;
                case StatesMachine.RetroMin_TM_Gen:
                    reason += getNameInterval () + @"-???????? ????????";
                    waiting = @"??????? ? ????????";
                    break;
                case StatesMachine.RetroMins_TM:
                    reason += getNameInterval () + @"-???????? ????????";
                    waiting = @"??????? ? ????????";
                    break;
                //case StatesMachine.PPBRDates:
                //    if (request == 0)
                //    {
                //        reason = @"???????";
                //    }
                //    else
                //    {
                //        reason = @"?????????";
                //    }

                //    reason += @" ??????????? ??????? ???????? (PPBR)";
                //    waiting = @"??????? ? ????????";
                //    break;
                case StatesMachine.HoursTMTemperatureValues:
                    reason += @"??-??????? ??????????? ???.???????";
                    waiting = @"??????? ? ????????";
                    break;
                case StatesMachine.HoursVzletTDirectValues:
                    reason += @"??-??????? ??????????? ?????? ??????";
                    waiting = @"??????? ? ????????";
                    break;
                case StatesMachine.CurrentVzletTDirectValues:
                    reason += @"???.????. ??????????? ?????? ??????";
                    waiting = @"??????? ? ????????";
                    break;
                case StatesMachine.PPBRValues:
                    reason = @"?????? ?????";
                    //AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    reasonRes = ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON.ERROR;
                    break;
                //case StatesMachine.AdminDates:
                //    break;
                case StatesMachine.AdminValues:
                    reason = @"???????????????? ????????";
                    //AbortThreadRDGValues(INDEX_WAITHANDLE_REASON.ERROR);
                    reasonRes = ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON.ERROR;
                    break;
                default:
                    msg = @"??????????? ???????...";
                    break;
            }

            if (request == 0)
                reason = @"??????? " + reason;
            else
                reason = @"????????? " + reason;

            msg = m_tec.name_shr;

            if (waiting.Equals(string.Empty) == true)
                msg += ". ?????? " + reason + ". " + waiting + ".";
            else
                msg += ". ?????? " + reason + ".";

            //if (!(m_typePanel == TYPE_PANEL.ADMIN_ALARM))
            //if (! (ErrorReport == null))
                ErrorReport(msg);
            //else ;

            if (waiting.Equals (string.Empty) == true)
                waiting = @"????? ???????? ??????????";
            else
                ;

            Logging.Logg().Error($@"TecView::StateErrors ({m_tec.name_shr}, [KeyComponent={CurrentKey}])"
                + $@" - ?????? {reason}. {waiting}", Logging.INDEX_MESSAGE.NOT_SET);

            return reasonRes;
        }

        protected override void StateWarnings(int /*StatesMachine*/ state, int request, int result)
        {
            string reason = string.Empty,
                    waiting = string.Empty,
                    msg = string.Empty;

            switch ((StatesMachine)state)
            {
                case StatesMachine.Hours_Fact:
                    if (m_markWarning.IsMarked ((int)INDEX_WARNING.LAST_HOUR) == true)
                        reason = @"?? ???????? ???? ???????? ?? ???????";
                    else
                        if (m_markWarning.IsMarked ((int)INDEX_WARNING.LAST_HOURHALF) == true)
                            reason = @"?? ??????? ??? ?? ???????? ????????? ??????????? ????????";
                        else
                            ;
                    break;
                case StatesMachine.CurrentMins_Fact:
                case StatesMachine.CurrentMins_TM:
                    if (m_markWarning.IsMarked ((int)INDEX_WARNING.LAST_MIN) == true)
                        reason = @"?? ???????? ????????? ????????? ???????? ?? ???????";
                    else
                        ;
                    break;
                case StatesMachine.LastValue_TM_Gen:
                    reason = @"??? ???????? ???????? (?????????) ?? ?????? ?? ?? ??? ???????? ????????";
                    break;
                case StatesMachine.RetroMin_TM_Gen:
                    reason = @"??? ???????? ???????? (?????????) ?? ?? ?????? ?? ??";
                    break;
                case StatesMachine.HoursTMTemperatureValues:
                    reason += @"??? ????????? ??????? ???????? ??????????? ???.???????";
                    break;
                case StatesMachine.HoursVzletTDirectValues:
                    reason += @"??? ????????? ??????? ???????? ??????????? ?????? ??????";
                    break;
                case StatesMachine.CurrentVzletTDirectValues:
                    reason += @"?? ??????? ??????? ???????? ??????????? ?????? ??????";
                    break;
                default:
                    break;
            }

            waiting = @"???????? " + PanelStatistic.POOL_TIME.ToString() + " ??????";

            msg = m_tec.name_shr;

            msg += @". " + reason + ".";
            if (waiting.Equals(string.Empty) == true)
                msg += @" " + waiting + ".";
            else
                ;

            //if (!(m_typePanel == TYPE_PANEL.ADMIN_ALARM))
                WarningReport(msg);
            //else ;

            Logging.Logg().Warning($"{reason}: ???={m_tec.name_shr}, [KeyComponent]={CurrentKey} ..."
                , Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;

            string msg = string.Empty;

            switch ((StatesMachine)state)
            {
                case StatesMachine.InitSensors:
                    msg = @"??????????????? ????????";
                    //?????? ?? ?????????...
                    break;
                case StatesMachine.CurrentTimeAdmin:
                case StatesMachine.CurrentTimeView:
                    msg = @"???????? ??????? ???????";
                    GetCurrentTimeRequest();
                    break;
                case StatesMachine.Hours_Fact:
                    msg = @"(????-)??????? ????????";
                    getHoursFactRequest(m_curDate.Date.Add(-m_tsOffsetToMoscow));
                    break;
                case StatesMachine.Hour_TM:
                    msg = @"??????. ?? ??? ????????????";
                    getHourTMRequest(m_curDate.Date, lastHour);
                    break;
                case StatesMachine.Hours_TM:
                    msg = @"??????? ????????";
                    getHoursTMRequest(m_curDate.Date);
                    break;
                case StatesMachine.CurrentMins_Fact:
                    msg = $"{getNameInterval ()}-???????? ????????";
                    getMinsFactRequest(lastHour);
                    break;
                case StatesMachine.CurrentMin_TM:
                    msg = @"??????. ?? ???????? ????????????";
                    getMinTMRequest(m_curDate.Date, lastHour, lastMin);
                    break;
                case StatesMachine.CurrentMinDetail_TM:
                case StatesMachine.RetroMinDetail_TM:
                    msg = @"??????. ?? ???????? ????????????";
                    getMinDetailTMRequest(m_curDate.Date, lastHour, lastMin);
                    break;
                case StatesMachine.CurrentMins_TM:
                    msg = $"{getNameInterval ()}-???????? ????????";
                    getMinsTMRequest(lastHour);
                    break;
                case StatesMachine.CurrentHours_TM_SN_PSUM:
                    msg = @"??????? ???????? (??????. ?????)";
                    getHoursTMSNPsumRequest();
                    break;
                case StatesMachine.LastValue_TM_Gen:
                    msg = @"??????? ???????? (?????????)";
                    getCurrentTMGenRequest ();
                    break;
                case StatesMachine.LastValue_TM_SN:
                    msg = @"??????? ???????? (??????. ?????)";
                    getCurrentTMSNRequest();
                    break;
                case StatesMachine.LastMinutes_TM:
                    msg = @"??????? ???????? 59 ???";
                    getLastMinutesTMRequest();
                    break;
                //case StatesMachine.RetroHours:
                //    msg = @"??????????? ????????";
                //    adminValuesReceived = false;
                //    GetHoursRequest(m_curDate.Date);
                //    break;
                case StatesMachine.RetroMins_Fact:
                    msg = $"{getNameInterval ()}-???????? ??????????????? ????????";
                    getMinsFactRequest(lastHour);
                    break;
                case StatesMachine.RetroMin_TM_Gen:
                    msg = @"M-???????? ????????";
                    getMinTMGenRequest(m_curDate.Date, lastHour, lastMin - 1);
                    break;
                case StatesMachine.RetroMins_TM:
                    msg = $"{getNameInterval ()}-???????? ????????";
                    getMinsTMRequest(lastHour);
                    break;
                case StatesMachine.HoursTMTemperatureValues:
                    msg = @"??-??????? ??????????? ???.???????";
                    // ???????? ? UTC, ? ????? ? ?????????? ???????? ?????
                    getHoursTMTemperatureRequest(m_curDate.Date);
                    break;
                case StatesMachine.HoursVzletTDirectValues:
                    msg = @"??-??????? ??????????? ?????? ??????";
                    //getHoursVzletTDirectRequest(m_curDate.Date); ?????????? ? "????" ?????
                    break;
                case StatesMachine.CurrentVzletTDirectValues:
                    msg = @"???.????. ??????????? ?????? ??????";
                    //getCurrentVzletTDirectRequest(); ?????????? ? "????" ?????
                    break;
                //case StatesMachine.PPBRDates:
                //    msg = @"?????? ??????????? ??????? ????????";
                //    GetPPBRDatesRequest(m_curDate);
                //    break;
                case StatesMachine.PPBRValues:
                    msg = @"?????? ?????";
                    getPPBRValuesRequest();
                    break;
                //case StatesMachine.AdminDates:
                //    break;
                case StatesMachine.AdminValues:
                    msg = @"???????????????? ??????";
                    getAdminValuesRequest(/*s_typeFields*/);
                    break;
                default:
                    iRes = -1;
                    break;
            }

            //if (!(m_typePanel == TYPE_PANEL.ADMIN_ALARM))
                ActionReport (@"????????? " + msg + @".");
            //else ;

            //Logging.Logg().Debug(@"TecView::StateRequest () - TECname=" + m_tec.name_shr + @", state=" + state.ToString() + @", result=" + bRes.ToString() + @" - ?????...");

            return iRes;
        }

        protected override int StateResponse(int state, object table)
        {
            int iRes = 0;

            switch ((StatesMachine)state)
            {
                case StatesMachine.InitSensors:
                    switch (m_tec.Type)
                        {
                            case StatisticCommon.TEC.TEC_TYPE.COMMON:
                            case StatisticCommon.TEC.TEC_TYPE.BIYSK:
                                iRes = (GetSensorsTEC() == true) ? 0 : -1;
                                break;
                            default:
                                break;
                        }
                        if (iRes == 0)
                        {
                        }
                        else
                            ;
                    break;
                case StatesMachine.CurrentTimeAdmin:
                    iRes = getCurrentTimeAdminResponse(table as System.Data.DataTable);
                    if (iRes == 0)
                    {
                        if (using_date == true) {
                            m_prevDate = serverTime.Date;
                            m_curDate = m_prevDate;

                            setDatetime?.Invoke(m_curDate);
                        }
                        else
                            ;
                    }
                    else
                        ;
                    break;
                case StatesMachine.CurrentTimeView:
                    iRes = getCurrentTimeViewResponse(table as System.Data.DataTable);
                    if (iRes == 0)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.CurrentTime");
                        m_curDate = m_curDate.AddSeconds(-1 * PanelStatistic.ERROR_DELAY);
                        //this.BeginInvoke(delegateSetNowDate, true);
                        if (!(setDatetimeView == null)) setDatetimeView(); else ;
                    }
                    else
                        ;
                    break;
                case StatesMachine.Hours_Fact:
                    ClearValuesHours ();
                    //GenerateHoursTable(seasonJumpE.SummerToWinter, 3, table);
                    iRes = getHoursFactResponse(table as System.Data.DataTable);
                    if (! (iRes < 0))
                    {//?????...
                    }
                    else
                        ;
                    break;
                case StatesMachine.Hour_TM:
                    iRes = getHourTMResponse(table as System.Data.DataTable);
                    break;
                case StatesMachine.Hours_TM:
                    ClearValuesHours();
                    iRes = getHoursTMResponse(table as System.Data.DataTable);
                    break;
                case StatesMachine.CurrentMins_Fact:
                    ClearValuesMins();
                    iRes = getMinsFactResponse(table as System.Data.DataTable);
                    if (iRes == 0)
                    {
                        //this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                    }
                    else
                        ;
                    break;
                case StatesMachine.CurrentMin_TM:
                    iRes = getMinTMResponse(table as System.Data.DataTable);
                    break;
                case StatesMachine.CurrentMinDetail_TM:
                    iRes = getMinDetailTMResponse(table as System.Data.DataTable);
                    break;
                case StatesMachine.RetroMinDetail_TM:
                    iRes = getMinDetailTMResponse(table as System.Data.DataTable);
                    updateGUI_Fact (/*lastHour*/-1, lastMin); //-1 - ??????? ?????????? ?????? "?????? ??-????????"
                    break;
                case StatesMachine.CurrentMins_TM:
                    ClearValuesMins();
                    iRes = getMinsTMResponse(table as System.Data.DataTable);
                    break;
                case StatesMachine.CurrentHours_TM_SN_PSUM:
                    iRes = getHoursTMSNPsumResponse(table as System.Data.DataTable);
                    if (iRes == 0) {
                    } else
                        ;
                    break;
                case StatesMachine.LastValue_TM_Gen:
                    iRes = getCurrentTMGenResponse(table as System.Data.DataTable);
                    if (! (iRes < 0))
                        updateGUI_TM_Gen?.Invoke ();
                    else
                        ;
                    break;
                case StatesMachine.LastValue_TM_SN:
                    iRes = getCurrentTMSNResponse(table as System.Data.DataTable);
                    if (iRes == 0)
                        updateGUI_TM_SN?.Invoke ();
                    else
                        ;
                    break;
                case StatesMachine.LastMinutes_TM:
                    ClearValuesLastMinutesTM ();
                    iRes = getLastMinutesTMResponse(table as System.Data.DataTable, m_curDate);
                    if (iRes == 0)
                        updateGUI_LastMinutes?.Invoke();
                    else
                        ;
                    break;
                //case StatesMachine.RetroHours:
                //    ClearValues();
                //    bRes = getHoursResponse(table);
                //    if (bRes == true)
                //    {
                //    }
                //    else
                //        ;
                //    break;
                case StatesMachine.RetroMin_TM_Gen:
                    iRes = getMinTMGenResponse(table as System.Data.DataTable);
                    //14.04.2015 ???
                    //if (iRes == 0)
                        updateGUI_TM_Gen ();
                    //else ;
                    break;
                case StatesMachine.RetroMins_Fact:
                case StatesMachine.RetroMins_TM:
                    ClearValuesMins();
                    if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                        || (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                        iRes = getMinsFactResponse(table as System.Data.DataTable);
                    else
                        if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                            || (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN))
                            iRes = getMinsTMResponse(table as System.Data.DataTable);
                        else
                            ;
                    //14.04.2015 - ?????? ????? ??????? ?? 'AdminValues'
                    //if (iRes == 0)
                    //{
                    //    //this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                    //    updateGUI_Fact(lastHour, lastMin);
                    //}
                    //else ;
                    break;
                case StatesMachine.HoursTMTemperatureValues:
                    iRes = getHoursTMTemperatureResponse(table as System.Data.DataTable);
                    break;
                //?????????? ????????? ???????, ?.?. ??? ?????????????? ? "?????" ??????
                //case StatesMachine.HoursVzletTDirectValues:
                //    break;
                //?????????? ????????? ???????, ?.?. ??? ?????????????? ? "?????" ??????
                //case StatesMachine.CurrentVzletTDirectValues:
                //    break;
                //??? ????????????? ?????????? ???? ???, ?.?.?????? ?? ????????? ???
                //case StatesMachine.PPBRDates:
                //    ClearPPBRDates();
                //    iRes = getPPBRDatesResponse(table as System.Data.DataTable, m_curDate);
                //    if (iRes == 0)
                //    {
                //    }
                //    else
                //        ;
                //    break;
                case StatesMachine.PPBRValues:
                    ClearPBRValues();
                    iRes = getPPBRValuesResponse(table as System.Data.DataTable);
                    if (iRes == 0)
                    {
                    }
                    else
                        ;
                    break;
                //??? ????????????? ?????????? ???? ?????.????., ?.?.?????? ?? ?? ?????????
                //case StatesMachine.AdminDates:
                //    break;
                case StatesMachine.AdminValues:
                    ClearAdminValues();
                    iRes = getAdminValuesResponse(table as System.Data.DataTable);
                    if (iRes == 0)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.AdminValues");
                        ComputeRecomendation(lastHour - 0);
                        adminValuesReceived = true;
                        //this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                        if (! (updateGUI_Fact == null)) iRes = updateGUI_Fact(lastHour, lastMin); else ;
                    }
                    else
                        ;
                    break;
                default:
                    iRes = -1;
                    break;
            }

            if ((! (iRes < 0))
                //&& (! (m_typePanel == TYPE_PANEL.ADMIN_ALARM))
                && (isLastState (state) == true))
                ReportClear (false);
            else
                //Console.WriteLine(@"iRes=" + iRes + @"; StatesMachine=" + state.ToString ())
                    ;

            //Logging.Logg().Debug(@"TecView::StateResponse () - TECname=" + m_tec.name_shr + @", state=" + state.ToString() + @", bRes=" + bRes.ToString() + @" - ?????...");

            return iRes;
        }

        public virtual void ChangeState()
        {
            Run(@"TecView::ChangeState () - ...");
        }

        protected void GetCurrentTimeRequest()
        {
            DbInterface.DB_TSQL_INTERFACE_TYPE typeDB = DbInterface.DB_TSQL_INTERFACE_TYPE.UNKNOWN;
            int iListenerId = -1;

            if (IsCanUseTECComponents == true)
            {
                typeDB = DbTSQLInterface.getTypeDB(CurrentDevice.tec.connSetts[(int)CONN_SETT_TYPE.ADMIN].port);
                iListenerId = m_dictIdListeners[CurrentDevice.tec.m_id][(int)CONN_SETT_TYPE.ADMIN];
            }
            else
            {
                typeDB = DbTSQLInterface.getTypeDB(m_tec.connSetts[(int)CONN_SETT_TYPE.ADMIN].port);
                iListenerId = m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.ADMIN];
            }

            GetCurrentTimeRequest(typeDB, iListenerId);
        }

        private int getCurrentTimeAdminResponse(DataTable table)
        {
            return GetCurrentTimeResponse(table);
        }

        private int getCurrentTimeViewResponse(DataTable table)
        {
            int iRes = 0;
            
            if (table.Rows.Count == 1)
            {
                try
                {
                    m_curDate = ((DateTime)table.Rows[0][0]).Add(m_tsOffsetToMoscow);
                    serverTime = m_curDate;
                }
                catch (Exception excpt)
                {
                    Logging.Logg().Exception(excpt, "TecView::GetCurrentTimeViewReponse () - (DateTime)table.Rows[0][0]", Logging.INDEX_MESSAGE.NOT_SET);

                    iRes = -1;
                }
            }
            else
            {
                //selectedTime = System.TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3 + 1);
                //ErrorReport("?????? ????????? ???????? ??????? ???????. ???????????? ????????? ?????.");
                iRes = -1;
            }

            return iRes;
        }

        public void GetRetroHours()
        {
            lock (m_lockValue)
            {
                ClearValuesHours();

                ClearStates();

                adminValuesReceived = false;

                if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE)
                    || (m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    AddState((int)StatesMachine.Hours_Fact);
                else
                    if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                        || (m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN))
                        AddState((int)StatesMachine.Hours_TM);
                    else
                        ;

                AddState((int)StatesMachine.PPBRValues);
                AddState((int)StatesMachine.AdminValues);

                Run(@"TecView::GetRetroHours ()");
            }
        }

        public virtual void GetRetroValues()
        {
            CONN_SETT_TYPE typeSrcData = CONN_SETT_TYPE.UNKNOWN;

            lock (m_lockValue)
            {
                ClearValues();

                ClearStates();

                adminValuesReceived = false;

                //????...
                typeSrcData = m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS];
                if ((typeSrcData == CONN_SETT_TYPE.DATA_AISKUE)
                    || (typeSrcData == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    AddState((int)StatesMachine.Hours_Fact);
                else
                    if ((typeSrcData == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                        || (typeSrcData == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN))
                        AddState((int)StatesMachine.Hours_TM);
                    else
                        if (typeSrcData == CONN_SETT_TYPE.DATA_VZLET)
                            AddState((int)StatesMachine.HoursVzletTDirectValues);
                        else
                            ;

                //??????...
                typeSrcData = m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES];
                if (!(typeSrcData == CONN_SETT_TYPE.UNKNOWN))
                {
                    if ((typeSrcData == CONN_SETT_TYPE.DATA_AISKUE)
                        || (typeSrcData == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                        AddState((int)StatesMachine.RetroMins_Fact);
                    else
                        if ((typeSrcData == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                            || (typeSrcData == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN))
                            AddState((int)StatesMachine.RetroMins_TM);
                        else
                            ;
                    if (m_bLastValue_TM_Gen == true)
                        AddState((int)StatesMachine.RetroMin_TM_Gen);
                    else ;
                    //??? ??? ?? ?? ????????, ????? ?. ??????? 'ClearValuesLastMinutesTM'
                    AddState((int)StatesMachine.LastMinutes_TM);
                }
                else
                    ;

                AddState((int)StatesMachine.PPBRValues);
                AddState((int)StatesMachine.AdminValues);

                Run(@"TecView::GetRetroValues ()");
            }
        }

        public void GetRetroMinTMGen()
        {
            if (m_bLastValue_TM_Gen == true)
            {
                lock (m_lockValue)
                {
                    ClearStates();

                    AddState((int)StatesMachine.RetroMin_TM_Gen);

                    Run(@"TecView::getRetroMinTMGen ()");
                }
            }
            else ;
        }

        private void getRetroMins(int indxHour)
        {
            lock (m_lockValue)
            {
                currHour = false;

                //??????? ???
                if (indxHour < 0)
                {
                    string strMes = @"TecView::getRetroMins (indxHour = " + indxHour + @") - ...";
                    //Logging.Logg().Error(strMes);
                    //throw new Exception(strMes);
                }
                else ;
                lastHour = indxHour;

                ClearValuesMins();

                ClearStates();

                adminValuesReceived = false;

                if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                    || (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    AddState((int)StatesMachine.RetroMins_Fact);
                else
                    if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                        || (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN))
                        AddState((int)StatesMachine.RetroMins_TM);
                    else
                        ;
                if (m_bLastValue_TM_Gen)
                    AddState((int)StatesMachine.RetroMin_TM_Gen);
                else ;
                ////???
                //AddState((int)StatesMachine.LastMinutes_TM);

                AddState((int)StatesMachine.PPBRValues);
                AddState((int)StatesMachine.AdminValues);

                Run(@"TecView::getRetroMins ()");
            }
        }

        public void GetRetroMins()
        {
            getRetroMins(lastHour);
        }
        /// <summary>
        /// ????? ?? ??????????? ??????? - ?????????????? ????????? ????????? ?????? ????, ??? ??????? ?? ??? 'ZedGraph'-????
        ///  ??? ??????? ???????????? ???? (?????-?-?????, ????-?-???????)
        /// </summary>
        /// <param name="indx">?????? ????????</param>
        /// <returns>??????? - ???????? ?? ???????? ???????????????</returns>
        public bool zedGraphHours_MouseUpEvent (int indx) {
            bool bRes = true;

            if ((indx == serverTime.Add(m_tsOffsetToMoscow).Hour)
                && (m_curDate.Date.Equals(serverTime.Add(m_tsOffsetToMoscow).Date) == true)
                && (serverTime.Minute > 2)
                )
                bRes = false;
            else
                getRetroMins(indx);

            return bRes;
        }        

        private void initValuesMinLength()
        {
            if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE) && (! (m_valuesMins.Length == 21))
                || (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO) && (! (m_valuesMins.Length == 21))
                || (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN) && (! (m_valuesMins.Length == 21))) {
                m_valuesMins = null;
            }
            else
                if ((m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN) && (! (m_valuesMins.Length == 61))) {
                    m_valuesMins = null;
                }
                else
                    ;

            if (m_valuesMins == null) {
                int cnt = -1;
                
                switch (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES]) {
                    case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                        cnt = 61;
                        break;
                    case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                    case CONN_SETT_TYPE.DATA_AISKUE:
                    case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                    default:
                        cnt = 21;
                        break;
                }

                m_valuesMins = new valuesTEC[cnt];

                //????????????? ? ??? ?? ????????? ???????? ?????? ???????
                foreach (TECComponent g in _localTECComponents)
                {
                    foreach (TECComponentBase tc in g.ListLowPointDev)
                    {
                        this.m_dictValuesLowPointDev[tc.m_id].m_powerMinutes = null;
                        this.m_dictValuesLowPointDev[tc.m_id].m_powerMinutes = new double[cnt];
                    }
                }
            }
            else
                ;
        }
        /// <summary>
        /// ????????, ??????????????????? ???????? ????????
        /// </summary>
        protected void ClearValuesMins()
        {
            int id = -1;
            
            initValuesMinLength ();

            for (int i = 0; i < m_valuesMins.Length; i++)
            {
                if (m_valuesMins[i] == null) m_valuesMins[i] = new valuesTEC(); else ;

                m_valuesMins[i].valuesFact =
                m_valuesMins[i].valuesDiviation =
                m_valuesMins[i].valuesPBR =
                m_valuesMins[i].valuesPBRe =
                m_valuesMins[i].valuesUDGe = 0;
            }

            //foreach (TECComponent g in m_tec.list_TECComponents)
            foreach (TECComponent comp in _localTECComponents)
            {
                foreach (TECComponentBase tc in comp.ListLowPointDev)
                {
                    id = tc.m_id;

                    //????????? ?????? ??????? ??? ???./???. ???????? ? ??????? ????
                    if ((! (m_dictValuesLowPointDev[id].m_powerMinutes == null)) && (!(m_dictValuesLowPointDev[id].m_powerMinutes.Length == m_valuesMins.Length)))
                        m_dictValuesLowPointDev[id].m_powerMinutes = null;
                    else
                        ;
                    //???????? ?????? (??? ?????????????)
                    if (m_dictValuesLowPointDev[id].m_powerMinutes == null)
                        m_dictValuesLowPointDev[id].m_powerMinutes = new double[m_valuesMins.Length];
                    else
                        ;
                    //???????????????? ??? ???????? ??????? ???./???. ???????? ? ??????? ????
                    m_dictValuesLowPointDev[id].m_bPowerMinutesRecieved = false;
                    for (int i = 0; i < m_dictValuesLowPointDev[id].m_powerMinutes.Length; i++)
                    {
                        m_dictValuesLowPointDev[id].m_powerMinutes[i] = -1; //??????? ??????????? ??????
                    }

                    clearLowPointDevValuesSecs(m_dictValuesLowPointDev[id]);
                }
            }

            m_markWarning.UnMarked((int)INDEX_WARNING.LAST_MIN);

            m_dtLastChangedAt_TM_Gen = DateTime.MaxValue;
            m_arValueCurrentTM_Gen [(int)HDateTime.INTERVAL.MINUTES] = -1F;
            m_markWarning.UnMarked((int)INDEX_WARNING.CURR_MIN_TM_GEN);
        }

        protected void clearLowPointDevValuesSecs (valuesLowPointDev vals)
        {
            //???????? ?????? (??? ?????????????) ??? ?????. ???????? ? ??????? ???
            if (vals.m_powerSeconds == null)
                vals.m_powerSeconds = new double[60]; //!!! ? ?????? ?????? 60 ???
            else
                ;
            //???????????????? ??? ???????? ??????? ?????. ???????? ? ??????? ???
            for (int i = 0; i < vals.m_powerSeconds.Length; i++)
            {
                vals.m_powerSeconds[i] = -1; //??????? ??????????? ??????
            }
        }

        //protected void ClearValuesHours(int cnt = -1)
        protected virtual void ClearValuesHours()
        {
            int cntHours = -1;
            
            if (m_curDate.Date.Equals (HAdmin.SeasonDateTime.Date) == false)
            {
                if (m_valuesHours.Length > 24)
                {
                    m_valuesHours = null;
                    cntHours = 24;
                }
                else
                {
                }
            }
            else
            {
                if (m_valuesHours.Length < 25)
                {
                    m_valuesHours = null;
                    cntHours = 25;
                }
                else
                {
                }
            }

            if (m_valuesHours == null)
                m_valuesHours = new valuesTEC [cntHours];
            else
                ;

            for (int i = 0; i < m_valuesHours.Length; i++)
            {
                if (m_valuesHours[i] == null) m_valuesHours[i] = new valuesTEC(); else ;
                
                m_valuesHours[i].valuesFact =
                m_valuesHours[i].valuesTMSNPsum =
                m_valuesHours[i].valuesDiviation =
                m_valuesHours[i].valuesPBR =
                m_valuesHours[i].valuesPmin =
                m_valuesHours[i].valuesPmax =
                m_valuesHours[i].valuesREC =
                m_valuesHours[i].valuesPBRe =
                m_valuesHours[i].valuesUDGe = 0F;

                m_valuesHours[i].valuesForeignCommand = false;
            }

            //m_valuesHours.valuesFactAddon =
            //m_valuesHours.valuesDiviationAddon =
            //m_valuesHours.valuesPBRAddon =
            //m_valuesHours.valuesPBReAddon =
            //m_valuesHours.valuesUDGeAddon = 0;
            //m_valuesHours.season = seasonJumpE.None;
            //m_valuesHours.hourAddon = 0;
            //m_valuesHours.addonValues = false;

            m_markWarning.UnMarked((int)INDEX_WARNING.LAST_HOUR);
            m_markWarning.UnMarked((int)INDEX_WARNING.LAST_HOURHALF);

            m_arValueCurrentTM_Gen[(int)HDateTime.INTERVAL.HOURS] = -1F;
        }

        private void ClearPBRValues()
        {
        }

        private void ClearAdminTECComponentValues(int indx)
        {
            //List<TECComponentBase> localTECComponents = getLocalTECComponents();
            int id = -1;

            for (int j = 0; j < _localTECComponents.Count; j ++) {
                id = _localTECComponents[j].m_id;
                
                if (m_dictValuesTECComponent[indx][id] == null) m_dictValuesTECComponent[indx][id] = new valuesTECComponent(); else ;

                m_dictValuesTECComponent[indx][id].valuesDiviation =
                m_dictValuesTECComponent[indx][id].valuesPBR =
                m_dictValuesTECComponent[indx][id].valuesPmin =
                m_dictValuesTECComponent[indx][id].valuesPmax =
                m_dictValuesTECComponent[indx][id].valuesREC =
                m_dictValuesTECComponent[indx][id].valuesPBRe =
                m_dictValuesTECComponent[indx][id].valuesUDGe = 0F;

                m_dictValuesTECComponent[indx][id].valuesForeignCommand = false;
            }
        }

        private void ClearAdminValues()
        {
            int i = -1;

            if (! ((m_dictValuesTECComponent.Length - m_valuesHours.Length) == 1)) {
                m_dictValuesTECComponent = null;
                initDictValuesTECComponent(m_valuesHours.Length + 1);
            } else { }

            for (i = 0; i < m_valuesHours.Length; i++)
            {
                m_valuesHours[i].valuesDiviation =
                m_valuesHours[i].valuesPBR =
                m_valuesHours[i].valuesPmin =
                m_valuesHours[i].valuesPmax =
                m_valuesHours[i].valuesREC =
                m_valuesHours[i].valuesPBRe =
                m_valuesHours[i].valuesUDGe = 0;

                m_valuesHours[i].valuesForeignCommand = false;

                ClearAdminTECComponentValues(i);
            }

            ClearAdminTECComponentValues(i);

            initValuesMinLength ();

            for (i = 0; i < m_valuesMins.Length; i++)
                m_valuesMins[i].valuesDiviation =
                m_valuesMins[i].valuesPBR =
                m_valuesMins[i].valuesPBRe =
                m_valuesMins[i].valuesUDGe = 0;
        }

        protected virtual void setValuesHours (TECComponentBase.ID idComp = TECComponentBase.ID.MAX)
        {
            double currPBRe = -1F;

            switch (idComp)
            {
                case TECComponentBase.ID.GTP:
                    break;
                case TECComponentBase.ID.MAX:
                    for (int i = 0; i < m_valuesHours.Length; i++) //??? m_valuesHours.Length == m_dictValuesTECComponent.Length + 1
                    {
                        foreach (int id in m_dictValuesTECComponent[i + 1].Keys)
                        //for (int j = 0; j < localTECComponents.Count; j++)
                        {
                            //int id = localTECComponents [j].m_id;

                            m_valuesHours[i].valuesPBR += m_dictValuesTECComponent[i + 1][id].valuesPBR;
                            m_valuesHours[i].valuesPmin += m_dictValuesTECComponent[i + 1][id].valuesPmin;
                            m_valuesHours[i].valuesPmax += m_dictValuesTECComponent[i + 1][id].valuesPmax;
                            if (i == 0)
                            {
                                currPBRe = (m_dictValuesTECComponent[i + 1][id].valuesPBR + m_dictValuesTECComponent[0][id].valuesPBR) / 2;
                            }
                            else
                            {
                                currPBRe = (m_dictValuesTECComponent[i + 1][id].valuesPBR + m_dictValuesTECComponent[i][id].valuesPBR) / 2;
                            }

                            m_dictValuesTECComponent[i + 1][id].valuesPBRe = currPBRe;
                            m_valuesHours[i].valuesPBRe += currPBRe;

                            m_valuesHours[i].valuesForeignCommand |= m_dictValuesTECComponent[i + 1][id].valuesForeignCommand;

                            m_valuesHours[i].valuesREC += m_dictValuesTECComponent[i + 1][id].valuesREC;

                            m_dictValuesTECComponent[i + 1][id].valuesUDGe = currPBRe + m_dictValuesTECComponent[i + 1][id].valuesREC;
                            m_valuesHours[i].valuesUDGe += currPBRe + m_dictValuesTECComponent[i + 1][id].valuesREC;

                            if (m_dictValuesTECComponent[i + 1][id].valuesISPER == 1)
                            {
                                m_dictValuesTECComponent[i + 1][id].valuesDiviation = (currPBRe + m_dictValuesTECComponent[i + 1][id].valuesREC) * m_dictValuesTECComponent[i + 1][id].valuesDIV / 100;
                            }
                            else
                            {
                                m_dictValuesTECComponent[i + 1][id].valuesDiviation = m_dictValuesTECComponent[i + 1][id].valuesDIV;
                            }
                            m_valuesHours[i].valuesDiviation += m_dictValuesTECComponent[i + 1][id].valuesDiviation;
                        }
                    }

                    //if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                    //{
                    //    m_valuesHours.valuesPBRAddon = m_valuesHours.valuesPBR[m_valuesHours.hourAddon];
                    //    m_valuesHours.valuesPBReAddon = m_valuesHours.valuesPBRe[m_valuesHours.hourAddon];
                    //    m_valuesHours.valuesUDGeAddon = m_valuesHours.valuesUDGe[m_valuesHours.hourAddon];
                    //    m_valuesHours.valuesDiviationAddon = m_valuesHours.valuesDiviation[m_valuesHours.hourAddon];
                    //}
                    //else
                    //    ;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// ?????????? ????????? ?????????? ??????? ???????? ??? ? ???????????????? ????????
        /// </summary>
        /// <param name="table_in">????????? ??????? ? ????????????????? ??????????</param>
        /// <returns>??????? ?????? ??? ????????? ??????????? ????????</returns>
        private int getAdminValuesResponse(DataTable table_in)
        {
            // ??? ??????? '_localTECComponents' ??? ????????? ??? ? ????? ??? ????? ? ???????
            List<TECComponentBase> localTECComponents = null;

            DateTime date = m_curDate.Add (m_tsOffsetToMoscow) //m_pnlQuickData.dtprDate.Value.Date
                    , dtPBR;
            int h = -1, hour = -1, day = -1;
            bool bSeason = false;
            int offsetPrev = -1
                //, tableRowsCount = table_in.Rows.Count; //tableRowsCount = m_tablePPBRValuesResponse.Rows.Count
                , i = -1, j = -1
                , offsetUDG, offsetPlan, offsetLayout // ???????? ???????? ???, ???, ?????? ??? ?? 0-?? ??????? ? ?????? ????? ?????????? ???????
                , id_comp = -1, id_part_field = -1; // ????????????? ??????????, ????????????? ?????????? ??? ????? ???????????? ???? ?????????? ??????? (??? ??????? ?????????)
            string tmpLayout = string.Empty; // ??? ??????????? ???????????? ?????? ???

            lastLayout = "---";

            //?????????? ??????? ???? ???????? ??????? (???????, ?? ??? ???????? ? ?????)
            if (HAdmin.SeasonDateTime.Date.CompareTo (m_curDate.Date) == 0)
                bSeason = true;
            else
                ;

            for (i = 0; i < m_tablePPBRValuesResponse.Rows.Count; i++)
                m_tablePPBRValuesResponse.Rows[i][@"DATE_PBR"] = ((DateTime)m_tablePPBRValuesResponse.Rows[i][@"DATE_PBR"]).Add(m_tsOffsetToMoscow);
            
            for (i = 0; i < table_in.Rows.Count; i++)
                table_in.Rows[i][@"DATE_ADMIN"] = ((DateTime)table_in.Rows[i][@"DATE_ADMIN"]).Add(m_tsOffsetToMoscow);

            if (!(CurrentKey.Mode == FormChangeMode.MODE_TECCOMPONENT.GTP))
            {
            #region ??? ???, ? ??????????? ??, ??
                offsetUDG = 1;
                offsetPlan = /*offsetUDG + 3 * tec.list_TECComponents.Count +*/ 1; //ID_COMPONENT
                offsetLayout = -1;

                localTECComponents = _localTECComponents; //getLocalTECComponents();

                m_tablePPBRValuesResponse = restruct_table_pbrValues(m_tablePPBRValuesResponse
                    , _type
                    , _type == TECComponentBase.TYPE.TEPLO ? m_tec.ListTECComponents :
                        _type == TECComponentBase.TYPE.ELECTRO ? m_tec.ListTECComponents : null
                    , CurrentKey
                    , m_tsOffsetToMoscow);
                offsetLayout = (!(m_tablePPBRValuesResponse.Columns.IndexOf("PBR_NUMBER") < 0)) ?
                    (offsetPlan + localTECComponents.Count * 3) :
                        m_tablePPBRValuesResponse.Columns.Count;

                DataTable tableAdminValuesResponse = null;
                if (bSeason == true)
                    //????????? ??????? ? ?????./????.
                    tableAdminValuesResponse = table_in.Copy();
                else
                    ;
                table_in = restruct_table_adminValues(table_in
                    , _type
                    , _type == TECComponentBase.TYPE.TEPLO ? m_tec.ListTECComponents :
                        _type == TECComponentBase.TYPE.ELECTRO ? m_tec.ListTECComponents : null
                    , CurrentKey
                    , m_tsOffsetToMoscow);

                // ????? ? ??????? ?????? ?? ?????????? ?????? (???? ??, ????? ????)
                for (i = 0; i < m_tablePPBRValuesResponse.Rows.Count && offsetPrev < 0; i++)
                {
                    if (!(m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {
                        try
                        {
                            //m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"] = ((DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"]).Add (m_tsOffsetToMoscow);
                            dtPBR = ((DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"])/*.Add (m_tsOffsetToMoscow)*/;

                            hour = dtPBR.Hour;
                            if ((hour == 0) && (dtPBR.Day == date.Day))
                            {
                                offsetPrev = i;
                                //foreach (TECComponent g in tec.list_TECComponents)
                                for (j = 0; j < localTECComponents.Count; j++)
                                {
                                    int id = localTECComponents[j].m_id;

                                    if ((offsetPlan + j * 3) < m_tablePPBRValuesResponse.Columns.Count)
                                    {
                                        if (double.TryParse(m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3 + 0].ToString(), out m_dictValuesTECComponent[0][id].valuesPBR) == false)
                                            m_dictValuesTECComponent[0][id].valuesPBR = 0F;
                                        else
                                            ;
                                        if (double.TryParse(m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3 + 1].ToString(), out m_dictValuesTECComponent[0][id].valuesPmin) == false)
                                            m_dictValuesTECComponent[0][id].valuesPmin = 0F;
                                        else
                                            ;
                                        if (double.TryParse(m_tablePPBRValuesResponse.Rows[i][offsetPlan + j * 3 + 2].ToString(), out m_dictValuesTECComponent[0][id].valuesPmax) == false)
                                            m_dictValuesTECComponent[0][id].valuesPmax = 0F;
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        m_dictValuesTECComponent[0][id].valuesPBR = 0.0;
                                        m_dictValuesTECComponent[0][id].valuesPmin = 0.0;
                                        m_dictValuesTECComponent[0][id].valuesPmax = 0.0;
                                    }
                                    //j++;
                                }
                            }
                            else
                                ;
                        }
                        catch (Exception excpt) {
                            Logging.Logg().Exception(excpt, "TecView::getAdminValuesResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                    else
                    {
                        try
                        {
                            //table_in.Rows[i]["DATE_ADMIN"] = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Add(m_tsOffsetToMoscow);
                            hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                            hour += 0//(int)m_tsOffsetToMoscow.TotalHours
                                ;

                            day = ((DateTime)table_in.Rows[i]["DATE_ADMIN"])/*.Add(m_tsOffsetToMoscow)*/.Day;
                            if (hour == 0 && day == date.Day)
                            {
                                offsetPrev = i;
                            }
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"TecView::getAdminValuesresponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                }

                // ?????? ????????? ????????
                for (i = 0; i < m_tablePPBRValuesResponse.Rows.Count; i++)
                {
                    if (i == offsetPrev)
                        continue;
                    else
                        ;

                    if (!(m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {//???????? ??? ????/??????? ? ???? ?????? ????
                        try
                        {
                            //m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"] = ((DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"]).Add(m_tsOffsetToMoscow);
                            dtPBR = ((DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"])/*.Add (m_tsOffsetToMoscow)*/;

                            hour = dtPBR.Hour;
                            if (hour == 0 && ((DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"]).Day != date.Day)
                                hour = 24;
                            else
                                if (hour == 0)
                                    continue;
                                else
                                    ;

                            //GetSeasonHours(ref prev_hour, ref hour);
                            hour += GetSeasonHourOffset(hour);

                            //foreach (TECComponent g in tec.list_TECComponents)
                            for (j = 0; j < localTECComponents.Count; j++)
                            {
                                id_comp =
                                id_part_field =
                                    -1;

                                try
                                {
                                    id_comp = localTECComponents[j].m_id;
                                    id_part_field = _type == TECComponentBase.TYPE.TEPLO ? (localTECComponents[j] as TECComponent).ListLowPointDev.Find(tcb => { return (tcb as Vyvod.ParamVyvod).m_id_param == Vyvod.ID_PARAM.T_PV; }).m_id :
                                        _type == TECComponentBase.TYPE.ELECTRO ? localTECComponents[j].m_id :
                                            -1;

                                    if ((offsetPlan + (j * 3) < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3)] is System.DBNull)))
                                    {
                                        //valuesPBR[j, hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3)];
                                        m_dictValuesTECComponent[hour - 0][id_comp].valuesPBR = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3)];
                                        //valuesPmin[j, hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 1];
                                        m_dictValuesTECComponent[hour - 0][id_comp].valuesPmin = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 1];
                                        //valuesPmax[j, hour - 1] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 2];
                                        m_dictValuesTECComponent[hour - 0][id_comp].valuesPmax = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + (j * 3) + 2];
                                    }
                                    else
                                    {
                                        m_dictValuesTECComponent[hour - 0][id_comp].valuesPBR = 0.0;
                                        //m_dictValuesTECComponent[id].valuesPmin[hour - 1] = 0.0;
                                        //m_dictValuesTECComponent[id].valuesPmax[hour - 1] = 0.0;
                                    }

                                    DataRow[] row_in;
                                    //????? ????? ?? ??????? ??? ? ????? + ????? 'AdminTS'
                                    if (bSeason == true) {
                                        if ((hour - 1) == (HAdmin.SeasonDateTime.Hour + 1)) {
                                            m_dictValuesTECComponent[hour - 1][id_comp].valuesPBR = m_dictValuesTECComponent[hour - 2][id_comp].valuesPBR;
                                            m_dictValuesTECComponent[hour - 1][id_comp].valuesPmin = m_dictValuesTECComponent[hour - 2][id_comp].valuesPmin;
                                            m_dictValuesTECComponent[hour - 1][id_comp].valuesPmax = m_dictValuesTECComponent[hour - 2][id_comp].valuesPmax;
                                        } else {
                                        }

                                        if (hour == HAdmin.SeasonDateTime.Hour)
                                        {
                                            //row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString(@"yyyy-MM-dd HH:mm:ss") + @"'");
                                            row_in = tableAdminValuesResponse.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "' AND ID_COMPONENT = " + id_part_field.ToString ());
                                            //????? ? 'AdminRS'
                                            if (row_in.Length > 0)
                                            {
                                                h = -1;
                                                foreach (DataRow r in row_in)
                                                {
                                                    h = dtPBR.Hour;

                                                    GetSeasonHourIndex(Int32.Parse(r[@"SEASON"].ToString()), ref h);

                                                    m_dictValuesTECComponent[h - 0][id_comp].valuesForeignCommand = (byte)r[@"FC"] == 1;

                                                    m_dictValuesTECComponent[h - 0][id_comp].valuesREC = (double)r[@"REC"];
                                                    m_dictValuesTECComponent[h - 0][id_comp].valuesISPER = (int)r[@"IS_PER"];
                                                    m_dictValuesTECComponent[h - 0][id_comp].valuesDIV = (double)r[@"DIVIAT"];
                                                }
                                            }
                                            else
                                            {//?????? ... ???
                                                Logging.Logg().Error(@"TecView::GetAdminValueResponse () - ... ??? ?? ????? ?????? ??? [HAdmin.SeasonDateTime.Hour] = " + hour, Logging.INDEX_MESSAGE.NOT_SET);
                                            }
                                        }
                                        else
                                        {
                                        }
                                    } else {
                                    }

                                    if (((!(hour == HAdmin.SeasonDateTime.Hour)) && (bSeason == true)) ||
                                        (bSeason == false))
                                    {                                    
                                        row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                        //if (i < table_in.Rows.Count)
                                        if (row_in.Length > 0)
                                        {
                                            if (row_in.Length > 1)
                                                ; //??????....
                                            else
                                                ;
                                            if (!(row_in[0]["FC_" + id_part_field.ToString()] is System.DBNull))
                                                m_dictValuesTECComponent[hour - 0][id_comp].valuesForeignCommand = (byte)row_in[0]["FC_" + id_part_field.ToString()] == 1;
                                            else
                                                m_dictValuesTECComponent[hour - 0][id_comp].valuesForeignCommand = false;
                                            //if (!(row_in[0][offsetUDG + j * 3] is System.DBNull))
                                            if (!(row_in[0]["REC_" + id_part_field.ToString()] is System.DBNull))
                                                //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
                                                //valuesREC[j, hour - 1] = (double)row_in[0][offsetUDG + j * 3];
                                                //m_dictValuesTECComponent[hour - 0][id].valuesREC = (double)row_in[0][offsetUDG + j * 3];
                                                m_dictValuesTECComponent[hour - 0][id_comp].valuesREC = (double)row_in[0]["REC_" + id_part_field.ToString()];
                                            else
                                                //valuesREC[j, hour - 1] = 0;
                                                m_dictValuesTECComponent[hour - 0][id_comp].valuesREC = 0.0;

                                            //if (!(row_in[0][offsetUDG + 1 + j * 3] is System.DBNull))
                                            if (!(row_in[0]["IS_PER_" + id_part_field.ToString()] is System.DBNull))
                                                //m_dictValuesTECComponent[hour - 0][id].valuesISPER = (int)row_in[0][offsetUDG + 1 + j * 3];
                                                m_dictValuesTECComponent[hour - 0][id_comp].valuesISPER = (int)row_in[0]["IS_PER_" + id_part_field.ToString()];
                                            else
                                                ;

                                            //if (!(row_in[0][offsetUDG + 2 + j * 3] is System.DBNull))
                                            if (!(row_in[0]["DIVIAT_" + id_part_field.ToString()] is System.DBNull))
                                                //m_dictValuesTECComponent[hour - 0][id].valuesDIV = (double)row_in[0][offsetUDG + 2 + j * 3];
                                                m_dictValuesTECComponent[hour - 0][id_comp].valuesDIV = (double)row_in[0]["DIVIAT_" + id_part_field.ToString()];
                                            else
                                                ;
                                        }
                                        else
                                        {
                                            m_dictValuesTECComponent[hour - 0][id_comp].valuesForeignCommand = false;

                                            m_dictValuesTECComponent[hour - 0][id_comp].valuesREC = 0.0;
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logging.Logg().Exception(e, @"PanelTecViewBase::GetAdminValuesResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                                }
                                //j++;
                            }

                            tmpLayout = "";
                            //if ((m_tablePPBRValuesResponse.Columns.Contains ("PBR_NUMBER")) && !(m_tablePPBRValuesResponse.Rows[i][offsetLayout] is System.DBNull))
                            if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetLayout] is System.DBNull)))
                                tmpLayout = (string)m_tablePPBRValuesResponse.Rows[i][offsetLayout];
                            else
                                ;

                            if (equalePBR(lastLayout, tmpLayout) < 0)
                                lastLayout = tmpLayout;
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"TecView::getAdminValuesResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                    else
                    {//???????? ??? ????/??????? ? ???? ?????? ??? - ???????????? ?????. ????/?????
                        int cntFields = 5; //REC, IS_PER, DIV, SEASON, FC

                        try
                        {
                            hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                            if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day != date.Day)
                                hour = 24;
                            else
                                if (hour == 0)
                                    continue;
                                else
                                    ;

                            //foreach (TECComponent g in tec.list_TECComponents)
                            for (j = 0; j < localTECComponents.Count; j++)
                            {
                                id_comp = -1;

                                try
                                {
                                    id_comp = localTECComponents[j].m_id;

                                    m_dictValuesTECComponent[hour - 0][id_comp].valuesPBR = 0;

                                    if (i < table_in.Rows.Count)
                                    {
                                        if (!(table_in.Rows[i][offsetUDG + 4 + j * cntFields] is System.DBNull))
                                            m_dictValuesTECComponent[hour - 0][id_comp].valuesForeignCommand = (byte)table_in.Rows[i][offsetUDG + 4 + j * cntFields] == 1;
                                        else
                                            m_dictValuesTECComponent[hour - 0][id_comp].valuesForeignCommand = false;
                                        
                                        if (!(table_in.Rows[i][offsetUDG + j * cntFields] is System.DBNull))
                                            //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * cntFields] is System.DBNull)))
                                            m_dictValuesTECComponent[hour - 0][id_comp].valuesREC = (double)table_in.Rows[i][offsetUDG + j * cntFields];
                                        else
                                            m_dictValuesTECComponent[hour - 0][id_comp].valuesREC = 0;

                                        if (!(table_in.Rows[i][offsetUDG + 1 + j * cntFields] is System.DBNull))
                                            m_dictValuesTECComponent[hour - 0][id_comp].valuesISPER = (int)table_in.Rows[i][offsetUDG + 1 + j * cntFields];
                                        else
                                            ;

                                        if (!(table_in.Rows[i][offsetUDG + 2 + j * cntFields] is System.DBNull))
                                            m_dictValuesTECComponent[hour - 0][id_comp].valuesDIV = (double)table_in.Rows[i][offsetUDG + 2 + j * cntFields];
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        m_dictValuesTECComponent[hour - 0][id_comp].valuesREC = 0.0;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logging.Logg().Exception(e, @"TecView::getAdminValuesResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                                }
                            }

                            tmpLayout = "";
                            if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetLayout] is System.DBNull)))
                                tmpLayout = (string)m_tablePPBRValuesResponse.Rows[i][offsetLayout];
                            else
                                ;

                            if (equalePBR(lastLayout, tmpLayout) < 0)
                                lastLayout = tmpLayout;
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"TecView::getAdminValuesResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                }

                setValuesHours();
            #endregion
            }
            else
            {
            #region ??? ???
                double currPBRe = -1F;
                int lValues = m_valuesHours.Length + 1;

                double[] valuesPBR = new double[lValues];
                double[] valuesPmin = new double[lValues];
                double[] valuesPmax = new double[lValues];
                bool[] valuesForeignCmd = new bool[lValues];
                double[] valuesREC = new double[lValues];
                int[] valuesISPER = new int[lValues];
                double[] valuesDIV = new double[lValues];

                offsetUDG = 1;
                offsetPlan = 1;
                offsetLayout = (!(m_tablePPBRValuesResponse.Columns.IndexOf("PBR_NUMBER") < 0)) ? offsetPlan + 3 : m_tablePPBRValuesResponse.Columns.Count;

                // ????? ? ??????? ?????? ?? ?????????? ?????? (???? ??, ????? ????)
                for (i = 0; i < m_tablePPBRValuesResponse.Rows.Count && offsetPrev < 0; i++)
                {
                    if (!(m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {
                        try
                        {
                            dtPBR = (DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"];

                            hour = dtPBR.Hour;
                            if (hour == 0 && ((DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"]).Day == date.Day)
                            {
                                offsetPrev = i;
                                valuesPBR[0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan];
                                valuesPmin[0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 1];
                                valuesPmax[0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 2];
                            }
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"TecView::getAdminValuesResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                    else
                    {
                        try
                        {
                            hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                            if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)
                            {
                                offsetPrev = i;
                            }
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"TecView::getAdminValuesResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                }

                // ?????? ????????? ????????
                for (i = 0; i < m_tablePPBRValuesResponse.Rows.Count; i++)
                {
                    if (i == offsetPrev)
                        continue;
                    else
                        ;

                    if (!(m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {
                        try
                        {
                            dtPBR = (DateTime)m_tablePPBRValuesResponse.Rows[i]["DATE_PBR"];

                            hour = dtPBR.Hour;
                            if ((hour == 0) && (!(dtPBR.Day == date.Day)))
                                hour = 24;
                            else
                                if (hour == 0)
                                    continue;
                                else
                                    ;

                            //GetSeasonHours (ref prev_hour, ref hour);
                            hour += GetSeasonHourOffset(hour);

                            if ((offsetPlan < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetPlan] is System.DBNull)))
                            {
                                valuesPBR[hour - 0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan];
                                valuesPmin[hour - 0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 1];
                                valuesPmax[hour - 0] = (double)m_tablePPBRValuesResponse.Rows[i][offsetPlan + 2];
                            }
                            else
                                ;

                            DataRow[] row_in;
                            //????? ?????? ?? ??????? ?????????? ??? + ????? 'AdminTS'
                            if (bSeason == true)
                            {
                                if ((hour - 1) == (HAdmin.SeasonDateTime.Hour + 1))
                                {
                                    valuesPBR[hour - 1] = valuesPBR[hour - 2];
                                    valuesPmin[hour - 1] = valuesPmin[hour - 2];
                                    valuesPmax[hour - 1] = valuesPmax[hour - 2];
                                }
                                else
                                {
                                }

                                if (hour == HAdmin.SeasonDateTime.Hour)
                                {
                                    row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                    //????? ? 'AdminRS'
                                    if (row_in.Length > 0)
                                    {
                                        foreach (DataRow r in row_in)
                                        {
                                            hour = dtPBR.Hour;
                                            GetSeasonHourIndex(Int32.Parse(r[@"SEASON"].ToString()), ref hour);

                                            valuesForeignCmd[hour - 1] = (byte)r[@"FC"] == 1;

                                            valuesREC [hour - 1] = (double)r[@"REC"];
                                            valuesISPER [hour - 1] = (int)r[@"IS_PER"];
                                            valuesDIV[hour - 1] = (double)r[@"DIVIAT"];
                                        }
                                    }
                                    else
                                    {//?????? ... ???
                                        Logging.Logg().Error(@"TecView::GetAdminValueResponse () - ... ??? ?? ????? ?????? ??? [HAdmin.SeasonDateTime.Hour] = " + hour, Logging.INDEX_MESSAGE.NOT_SET);
                                    }
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }

                            if (((!(hour == HAdmin.SeasonDateTime.Hour)) && (bSeason == true)) ||
                                        (bSeason == false))
                            {
                                row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                //if (i < table_in.Rows.Count)
                                if (row_in.Length > 0)
                                {
                                    if (row_in.Length > 1)
                                        ; //??????....
                                    else
                                        ;

                                    if (!(row_in[0][offsetUDG + 5] is System.DBNull))
                                        valuesForeignCmd[hour - 0] = (byte)row_in[0][offsetUDG + 5] == 1;
                                    else
                                        valuesForeignCmd[hour - 0] = false;

                                    if (!(row_in[0][offsetUDG] is System.DBNull))
                                        //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG] is System.DBNull)))
                                        valuesREC[hour - 0] = (double)row_in[0][offsetUDG + 0];
                                    else
                                        valuesREC[hour - 0] = 0;

                                    if (!(row_in[0][offsetUDG + 1] is System.DBNull))
                                        valuesISPER[hour - 0] = (int)row_in[0][offsetUDG + 1];
                                    else
                                        ;

                                    if (!(row_in[0][offsetUDG + 2] is System.DBNull))
                                        valuesDIV[hour - 0] = (double)row_in[0][offsetUDG + 2];
                                    else
                                        ;
                                }
                                else
                                {
                                    valuesForeignCmd[hour - 0] = false;

                                    valuesREC[hour - 0] = 0;
                                    //valuesISPER[hour - 1] = 0;
                                    //valuesDIV[hour - 1] = 0;
                                }
                            }
                            else
                            {
                            }

                            tmpLayout = "";
                            if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetLayout] is System.DBNull)))
                                tmpLayout = (string)m_tablePPBRValuesResponse.Rows[i][offsetLayout];
                            else
                                ;

                            if (equalePBR(lastLayout, tmpLayout) < 0)
                                lastLayout = tmpLayout;
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, "PanelTecViewBase::GetAdminValueResponse ()...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                    else
                    {
                        try
                        {
                            hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                            if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day != date.Day)
                                hour = 24;
                            else
                                if (hour == 0)
                                    continue;
                                else
                                    ;

                            //GetSeasonHours (ref prev_hour, ref hour);
                            //hour += GetSeasonHourOffset(hour);
                            GetSeasonHourIndex(Int32.Parse(table_in.Rows[i]["SEASON"].ToString()), ref hour);

                            valuesPBR[hour - 0] = 0;

                            if (i < table_in.Rows.Count)
                            {
                                if (!(table_in.Rows[i][@"FC"] is System.DBNull))
                                    //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull)))
                                    valuesForeignCmd[hour - 0] = (byte)table_in.Rows[i][@"FC"] == 1;
                                else
                                    valuesREC[hour - 0] = 0;

                                if (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull))
                                    //if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull)))
                                    valuesREC[hour - 0] = (double)table_in.Rows[i][offsetUDG + 0];
                                else
                                    valuesREC[hour - 0] = 0;

                                if (!(table_in.Rows[i][offsetUDG + 1] is System.DBNull))
                                    valuesISPER[hour - 0] = (int)table_in.Rows[i][offsetUDG + 1];
                                else
                                    ;

                                if (!(table_in.Rows[i][offsetUDG + 2] is System.DBNull))
                                    valuesDIV[hour - 0] = (double)table_in.Rows[i][offsetUDG + 2];
                                else
                                    ;
                            }
                            else
                            {
                                valuesForeignCmd [hour - 0] = false;

                                valuesREC[hour - 0] = 0;
                                //valuesISPER[hour - 1] = 0;
                                //valuesDIV[hour - 1] = 0;
                            }

                            tmpLayout = "";
                            if ((offsetLayout < m_tablePPBRValuesResponse.Columns.Count) && (!(m_tablePPBRValuesResponse.Rows[i][offsetLayout] is System.DBNull)))
                                tmpLayout = (string)m_tablePPBRValuesResponse.Rows[i][offsetLayout];
                            else
                                ;

                            if (equalePBR(lastLayout, tmpLayout) < 0)
                                lastLayout = tmpLayout;
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, @"TecView::getAdminValuesResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                }

                //setValuesHours(TECComponentBase.ID.GTP);
                for (i = 0; i < m_valuesHours.Length; i++)
                {
                    m_valuesHours[i].valuesPBR = valuesPBR[i + 1];
                    m_valuesHours[i].valuesPmin = valuesPmin[i + 1];
                    m_valuesHours[i].valuesPmax = valuesPmax[i + 1];

                    if (i == 0)
                    {
                        currPBRe = (valuesPBR[i + 1] + valuesPBR[0]) / 2;
                        m_valuesHours[i].valuesPBRe = currPBRe;
                    }
                    else
                    {
                        currPBRe = (valuesPBR[i + 1] + valuesPBR[i - 0]) / 2;
                        m_valuesHours[i].valuesPBRe = currPBRe;
                    }

                    m_valuesHours[i].valuesForeignCommand = valuesForeignCmd[i + 1];

                    m_valuesHours[i].valuesREC = valuesREC[i + 1];

                    m_valuesHours[i].valuesUDGe = currPBRe + valuesREC[i + 1];

                    if (valuesISPER[i + 1] == 1)
                        m_valuesHours[i].valuesDiviation = (currPBRe + valuesREC[i + 1]) * valuesDIV[i + 1] / 100;
                    else
                        m_valuesHours[i].valuesDiviation = valuesDIV[i + 1];
                }

                //if (m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                //{
                //    m_valuesHours.valuesPBRAddon = m_valuesHours.valuesPBR[m_valuesHours.hourAddon];
                //    m_valuesHours.valuesPBReAddon = m_valuesHours.valuesPBRe[m_valuesHours.hourAddon];
                //    m_valuesHours.valuesUDGeAddon = m_valuesHours.valuesUDGe[m_valuesHours.hourAddon];
                //    m_valuesHours.valuesDiviationAddon = m_valuesHours.valuesDiviation[m_valuesHours.hourAddon];
                //}
                //else
                //    ;
            #endregion
            }

            hour = lastHour;

            //??????? ???
            if (hour == 0)
                ; //hour = 1;
            else
                ;

            if (hour == m_valuesHours.Length)
                hour = m_valuesHours.Length - 1;
            else
                ;

            for (i = 0; i < m_valuesMins.Length; i++)
            {
                //??? [hour - 1] vs [hour - 0] 26.10.2014 ? ?????? ????, ??? ?????????? ?????? '00:00' ?? [24] -> [0]
                m_valuesMins[i].valuesPBR = m_valuesHours[hour - 0].valuesPBR;
                m_valuesMins[i].valuesPBRe = m_valuesHours[hour - 0].valuesPBRe;
                m_valuesMins[i].valuesUDGe = m_valuesHours[hour - 0].valuesUDGe;
                m_valuesMins[i].valuesDiviation = m_valuesHours[hour - 0].valuesDiviation;
            }

            return 0;
        }

        private void ComputeRecomendation(int hour)
        {
            if (! (hour < m_valuesHours.Length))
                //???
                hour = m_valuesHours.Length - 1;
            else
                ;

            if (m_valuesHours[hour].valuesUDGe == 0)
            {
                recomendation = 0;
                return;
            }

            if (currHour == false)
            {
                recomendation = m_valuesHours[hour].valuesUDGe;
                return;
            } else {
            }

            if (lastMin < 2)
            {
                recomendation = m_valuesHours[hour].valuesUDGe;
                return;
            }
            else {
            }

            double factSum = 0;
            for (int i = 1; i < lastMin; i++)
                factSum += m_valuesMins[i].valuesFact;

            if (lastMin == m_valuesMins.Length)
                recomendation = 0;
            else
                recomendation = (m_valuesHours[hour].valuesUDGe * (m_valuesMins.Length - 1) - factSum) / ((m_valuesMins.Length - 1) - (lastMin - 1));

            if (recomendation < 0)
                recomendation = 0;
            else
                ;

            Logging.Logg().Debug(@"recomendation=" + recomendation.ToString(@"F3")
                                + @" (factSum=" + factSum.ToString(@"F3")
                                + @"; valuesUDGe=" + m_valuesHours[hour].valuesUDGe.ToString(@"F3")
                                + @") [" + hour + @", " + lastMin + @"]", Logging.INDEX_MESSAGE.D_003);
        }

        public static DataTable restruct_table_pbrValues(DataTable table_in, TECComponentBase.TYPE type, List<TECComponent> listTECComp, FormChangeMode.KeyDevice key, TimeSpan tsOffsetToMoscow)
        {
            DataTable table_in_restruct = new DataTable();
            List<DataColumn> cols_data = new List<DataColumn>();
            DataRow[] dataRows;
            int i = -1, j = -1, k = -1
                , id = -1
                , count_comp = -1;
            string nameFieldDate = "DATE_PBR"; // tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_DATETIME]

            //List<TECComponent> list_TECComponents = null; // ???????????? ??? 'num_comp < 0' (??? ? ?????)
            List<TECComponentBase> list_TECComponents = null; // ???????????? ??? 'num_comp < 0' (??? ? ?????)
            //List<TECComponentBase> list_LowPointDev = null; // ???????????? ??? 'num_comp ??< 0'

            for (i = 0; i < table_in.Columns.Count; i++)
            {
                if (table_in.Columns[i].ColumnName == "ID_COMPONENT")
                {
                    //?????????????? ???????
                    break;
                }
                else
                    ;
            }

            if (i < table_in.Columns.Count)
            {
                count_comp = -1;

                if (key.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                {
                    list_TECComponents = new List<TECComponentBase>();
                    for (i = 0; i < listTECComp.Count; i++)
                    {
                        if ((type == TECComponentBase.TYPE.ELECTRO)
                            && (listTECComp[i].IsGTP == true))
                            list_TECComponents.Add(listTECComp[i]);
                        else
                            if ((type == TECComponentBase.TYPE.TEPLO)
                                && (listTECComp[i].IsVyvod == true))
                                //list_TECComponents.Add((listTECComp[i] as Vyvod).m_listParam[0]);
                                list_TECComponents.Add(listTECComp[i].ListLowPointDev.Find(dev => { return (dev as Vyvod.ParamVyvod).m_id_param == Vyvod.ID_PARAM.T_PV; }));
                            else
                                ;
                    }
                }
                else
                    //list_LowPointDev = listTECComp[num_comp].ListLowPointDev
                    list_TECComponents = listTECComp.FirstOrDefault(comp => comp.m_id == key.Id).ListLowPointDev
                    ;

                //?????????????? ???????
                for (i = 0; i < table_in.Columns.Count; i++)
                {
                    if ((!(table_in.Columns[i].ColumnName.Equals("ID_COMPONENT") == true))
                        && (!(table_in.Columns[i].ColumnName.Equals(nameFieldDate) == true))
                        //&& (!(table_in.Columns[i].ColumnName.Equals (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true))
                        && (!(table_in.Columns[i].ColumnName.Equals(@"PBR_NUMBER") == true))
                    )
                    //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                    {
                        cols_data.Add(table_in.Columns[i]);
                    }
                    else
                        if ((table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                            //|| (table_in.Columns[i].ColumnName.Equals (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true)
                            || (table_in.Columns[i].ColumnName.Equals(@"PBR_NUMBER") == true)
                        )
                        {
                            table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                        }
                        else
                            ;
                }

                //if (num_comp < 0)
                    count_comp = list_TECComponents.Count;
                //else
                //    count_comp = list_LowPointDev.Count;

                for (i = 0; i < count_comp; i++)
                {
                    for (j = 0; j < cols_data.Count; j++)
                    {
                        table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);
                        //if (num_comp < 0)
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        //else
                        //    table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_LowPointDev[i].m_id;
                    }
                }

                //if (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                //table_in_restruct.Columns[tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]].SetOrdinal(table_in_restruct.Columns.Count - 1);
                table_in_restruct.Columns[@"PBR_NUMBER"].SetOrdinal(table_in_restruct.Columns.Count - 1);
                //else
                //    ;

                List<DataRow[]> listDataRows = new List<DataRow[]>();

                for (i = 0; i < count_comp; i++)
                {
                    id = list_TECComponents[i].m_id;
                    //if (num_comp < 0)
                        dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);
                    //else
                    //    dataRows = table_in.Select("ID_COMPONENT=" + list_LowPointDev[i].m_id);

                    listDataRows.Add(new DataRow[dataRows.Length]);
                    dataRows.CopyTo(listDataRows[i], 0);

                    int indx_row = -1;
                    for (j = 0; j < listDataRows[i].Length; j++)
                    {
                        for (k = 0; k < table_in_restruct.Rows.Count; k++)
                        {
                            if (table_in_restruct.Rows[k][nameFieldDate].Equals(listDataRows[i][j][nameFieldDate]) == true)
                                break;
                            else
                                ;
                        }

                        if (!(k < table_in_restruct.Rows.Count))
                        {
                            table_in_restruct.Rows.Add();

                            indx_row = table_in_restruct.Rows.Count - 1;

                            //?????????? DATE_ADMIN (?????????? ???????)
                            table_in_restruct.Rows[indx_row][nameFieldDate] = listDataRows[i][j][nameFieldDate];
                            //if (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                            //table_in_restruct.Rows[indx_row][tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]] = listDataRows[i][j][tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]];
                            table_in_restruct.Rows[indx_row][@"PBR_NUMBER"] = listDataRows[i][j][@"PBR_NUMBER"];
                            //else
                            //    ;
                        }
                        else
                            indx_row = k;

                        for (k = 0; k < cols_data.Count; k++)
                        {
                            //if (num_comp < 0)
                                table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + id] = listDataRows[i][j][cols_data[k].ColumnName];
                            //else
                            //    table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_LowPointDev[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                        }
                    }
                }
            }
            else
                table_in_restruct = table_in;

            //for (i = 0; i < table_in_restruct.Rows.Count; i++)
            //    table_in_restruct.Rows[i][nameFieldDate] = ((DateTime)table_in_restruct.Rows[i][nameFieldDate]).Add(tsOffsetToMoscow);

            return table_in_restruct;
        }

        public static DataTable restruct_table_adminValues(DataTable table_in, TECComponentBase.TYPE type, List<TECComponent> listTECComp, FormChangeMode.KeyDevice key, TimeSpan tsOffsetToMoscow)
        {
            DataTable table_in_restruct = new DataTable();
            List<DataColumn> cols_data = new List<DataColumn>();
            DataRow[] dataRows;
            int i = -1, j = -1, k = -1;
            string nameFieldDate = "DATE_ADMIN"; // tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.ADMIN_DATETIME]

            //List<TECComponentBase> list_LowPointDev = null;
            List<TECComponentBase> list_TECComponents = null;

            for (i = 0; i < table_in.Columns.Count; i++)
            {
                if (table_in.Columns[i].ColumnName == "ID_COMPONENT")
                {
                    //?????????????? ???????
                    break;
                }
                else
                    ;
            }

            if (i < table_in.Columns.Count)
            {
                int count_comp = -1;

                if (key.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                {
                    list_TECComponents = new List<TECComponentBase>();
                    for (i = 0; i < listTECComp.Count; i++)
                    {
                        if ((type == TECComponentBase.TYPE.ELECTRO)
                            && (listTECComp[i].IsGTP == true))
                            list_TECComponents.Add(listTECComp[i]);
                        else
                            if ((type == TECComponentBase.TYPE.TEPLO)
                                && (listTECComp[i].IsVyvod == true))
                                //list_TECComponents.Add((listTECComp[i] as Vyvod).m_listParam[0]);
                                list_TECComponents.Add(listTECComp[i].ListLowPointDev.Find(dev => { return (dev as Vyvod.ParamVyvod).m_id_param == Vyvod.ID_PARAM.T_PV; }));
                            else
                                ;
                    }
                }
                else
                    //list_LowPointDev = listTECComp[num_comp].ListLowPointDev
                    list_TECComponents = listTECComp.FirstOrDefault(comp => comp.m_id == key.Id).ListLowPointDev;

                //?????????????? ???????
                for (i = 0; i < table_in.Columns.Count; i++)
                {
                    if ((!(table_in.Columns[i].ColumnName == "ID_COMPONENT")) && (!(table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)))
                    //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                    {
                        cols_data.Add(table_in.Columns[i]);
                    }
                    else
                        if (table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                        {
                            table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                        }
                        else
                            ;
                }

                //if (num_comp < 0)
                    count_comp = list_TECComponents.Count;
                //else
                //    count_comp = list_LowPointDev.Count;

                for (i = 0; i < count_comp; i++)
                {
                    for (j = 0; j < cols_data.Count; j++)
                    {
                        table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);
                        //if (num_comp < 0)
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        //else
                        //    table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_LowPointDev[i].m_id;
                    }
                }

                List<DataRow[]> listDataRows = new List<DataRow[]>();

                for (i = 0; i < count_comp; i++)
                {
                    //if (num_comp < 0)
                        dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);
                    //else
                    //    dataRows = table_in.Select("ID_COMPONENT=" + list_LowPointDev[i].m_id);
                    listDataRows.Add(new DataRow[dataRows.Length]);
                    dataRows.CopyTo(listDataRows[i], 0);

                    int indx_row = -1;
                    for (j = 0; j < listDataRows[i].Length; j++)
                    {
                        for (k = 0; k < table_in_restruct.Rows.Count; k++)
                        {
                            if (table_in_restruct.Rows[k][nameFieldDate].Equals(listDataRows[i][j][nameFieldDate]) == true)
                                break;
                            else
                                ;
                        }

                        if (!(k < table_in_restruct.Rows.Count))
                        {
                            table_in_restruct.Rows.Add();

                            indx_row = table_in_restruct.Rows.Count - 1;

                            //?????????? DATE_ADMIN (?????????? ???????)
                            table_in_restruct.Rows[indx_row][nameFieldDate] = listDataRows[i][j][nameFieldDate];
                        }
                        else
                            indx_row = k;

                        for (k = 0; k < cols_data.Count; k++)
                        {
                            //if (num_comp < 0)
                                table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TECComponents[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                            //else
                            //    table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_LowPointDev[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                        }
                    }
                }
            }
            else
                table_in_restruct = table_in;

            //for (i = 0; i < table_in_restruct.Rows.Count; i++)
            //    table_in_restruct.Rows[i][nameFieldDate] = ((DateTime)table_in_restruct.Rows[i][nameFieldDate]).Add(tsOffsetToMoscow);

            return table_in_restruct;
        }

        private int getHoursFactResponse(DataTable table)
        {
            int i, j
                , cntTG = -1
                , half = 0 //?????? ????????
                , hour = 0
                , prev_season = 0, season = 0, offset_season = 0;
            double hourVal = 0, halfVal = 0, value; //???????? ?? ?????? ???????
            DateTime dt , dtNeeded, dtServer;
            DataRow[] tgRows = null;
            TG tg = null;

            cntTG = CountLowPointDev;
            dt = dtNeeded = DateTime.Now;

            dtServer = serverTime/*.Add(m_tsOffsetToMoscow)*/;
            if ((currHour == true) && (dtServer.Minute < 2))
                dtServer = dtServer.AddMinutes(-1 * (dtServer.Minute + 1));
            else
                ;

            double[, ,] powerHourHalf = new double[ListLowPointDev.Count, 2, m_valuesHours.Length];

            if (currHour == true)
                lastHour = lastReceivedHour = 0;
            else
                ;

            /*Form2 f2 = new Form2();
            f2.FillHourTable(table);*/

            //????????????, ??? ?? ???????? ?? ?????? ???????? ?? ?? ???? ??????????? ???????? ???
            //foreach (TECComponent g in m_tec.list_TECComponents)
            //{
                i = 0;
                foreach (TECComponentBase tc in ListLowPointDev)
                {
                    //t.power.Length == t.receivedHourHalf1.Length == t.receivedHourHalf2.Length
                    for (j = 0; j < m_valuesHours.Length; j++)
                    {
                        powerHourHalf[i, 0, j] = powerHourHalf[i, 1, j] = -1F;
                    }

                    i ++;
                }
            //}

            //???????? ??????? ? ??????? ??????????? ?????
            if (CheckNameFieldsOfTable(table, new string[] { @"ID", @"DATA_DATE", @"SEASON", @"VALUE0" }) == false)
                return -1;
            else
                ;

            //???????? ??????? ? ??????? ?????
            if (! (table.Rows.Count > 0))
            {//?????? - ????????? ?????????? ???????
                if (currHour == true)
                {//???????????? ??????? ???
                    if (! (dtServer.Hour == 0))
                    {//?? ?????? ?????
                        lastHour = lastReceivedHour = dtServer.Hour;
                        //??????? ????????? ??????
                        m_markWarning.Marked ((int)INDEX_WARNING.LAST_HOUR);

                        return 1;
                    }
                    else
                        ;
                }
                else
                    ;

                //????????? ???????? ?????????? ???????, ?? ????????? ??????? ?????? ???
                return 0;
            }
            else
                ;

            i = 0;
            //???? ?? ?? 
            foreach (TECComponentBase tc in ListLowPointDev) {
                tgRows = new DataRow [] { };

                try {
                    tg = tc as TG;
                    tgRows =
                        //table.Select(@"ID=" + tg.m_arIds_fact[(int)HDateTime.INTERVAL.HOURS], @"DATA_DATE")
                        // ChrjapiAN 27.12.2017 ??????? ?? "OBJECT/ITEM"
                        //table.Select($"OBJECT={tg.m_arIds_fact[(int)HDateTime.INTERVAL.HOURS].IdObject} AND ITEM={tg.m_arIds_fact [(int)HDateTime.INTERVAL.HOURS].IdItem}", @"DATA_DATE")
                        (from row in table.Rows.Cast<DataRow> ()
                         where new TG.AISKUE_KEY () { IdObject = (int)row ["OBJECT"], IdItem = (int)row ["ITEM"] }.Equals (tg.m_aiskue_keys [(int)HDateTime.INTERVAL.HOURS])
                         select row).ToArray ()
                        ;
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, $"TecView::getHoursFactResponse () - ????? ?????? ? ???????-?????????? ??? ??={tc.name_shr}...", Logging.INDEX_MESSAGE.NOT_SET);
                } finally {
                }

                hour = -1;
                offset_season = 0;
                foreach (DataRow r in tgRows) {
                    try
                    {
                        if (DateTime.TryParse(r[@"DATA_DATE"].ToString(), out dt) == false)
                            return -1;
                        else
                            ;
                        dt = dt.Add(m_tsOffsetToMoscow);

                        if (int.TryParse(r[@"SEASON"].ToString(), out season) == false)
                            return -1;
                        else
                            ;

                        if (hour < 0)
                        {
                            if (season > DateTime.Now.Year)
                                getSeason(dt, season, out season);
                            else
                                ;
                            prev_season = season;
                            hour = 0;
                            dtNeeded = dt;
                        }
                        else
                            ;

                        ////??????? ???
                        ////if ((dt.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0) && (! (dt.Hour < HAdmin.SeasonDateTime.Hour)))
                        //if (((dt.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0) && ((dt.Hour + dt.Minute / 30) > HAdmin.SeasonDateTime.Hour)) ||
                        //    ((dt.Hour == 0) && (dt.Minute == 0) && (dt.AddDays(-1).CompareTo(HAdmin.SeasonDateTime.Date) == 0)))
                        //{
                        //    if (HAdmin.SeasonAction < 0)
                        //        season = (int)HAdmin.seasonJumpE.SummerToWinter;
                        //    else
                        //        if (HAdmin.SeasonAction > 0)
                        //            season = (int)HAdmin.seasonJumpE.WinterToSummer;
                        //        else
                        //            season = (int)HAdmin.seasonJumpE.None;

                        //    season += DateTime.Now.Year * 2;
                        //}
                        //else
                        //{
                        //}                    

                        if (double.TryParse(r[@"VALUE0"].ToString(), out value) == false)
                            return -1;
                        else
                            ;

                        switch (m_tec.Type)
                        {
                            case TEC.TEC_TYPE.COMMON:
                                break;
                            case TEC.TEC_TYPE.BIYSK:
                                value *= 2;
                                break;
                            default:
                                break;
                        }

                        //??? ??? ???????? ????? ??????? ?????
                        dtNeeded = dt;

                        hour = (dt.Hour + dt.Minute / 30);
                        if (hour == 0)
                            if (!(dt.Date == dtServer.Date))
                                //hour = m_valuesHours.Length;
                                hour = 24;
                            else
                                ;
                        else
                            ;

                        //if (!(prev_season == (int)HAdmin.seasonJumpE.None))
                        //{
                            //??????? ???
                            if (season > DateTime.Now.Year)
                                getSeason(dt, season, out season);
                            else
                                ;

                            if ((! (season == (int)HAdmin.seasonJumpE.None))
                                && (! (prev_season == season)))
                                if (offset_season == 1)
                                    //?????? ??? 2 ???????? ?? ?????
                                    ;
                                else
                                    if (prev_season == (int)HAdmin.seasonJumpE.None)
                                        if (season == (int)HAdmin.seasonJumpE.SummerToWinter)
                                            offset_season = 1;
                                        else
                                            //prev_season == (int)HAdmin.seasonJumpE.WinterToSummer
                                            ; // offset_season = -1; ??? 26.10.2014 ??? ???????? ????-????
                                    else
                                        if (prev_season == (int)HAdmin.seasonJumpE.WinterToSummer)
                                            offset_season = 1;
                                        else
                                            //prev_season == (int)HAdmin.seasonJumpE.SummerToWinter
                                            ; // offset_season = -1; ??? 26.10.2014 ??? ???????? ????-????
                            else
                                ; // ??? ?????? ??? ????./????? ????? ?????./??????
                        //} else ;

                        //??????? ???
                        if (season > DateTime.Now.Year)
                            getSeason(dt, season, out season);
                        else
                            ;

                        prev_season = season;

                        powerHourHalf[i, ((dt.Minute / 30) == 0) ? 1 : 0, (hour > 0) ? hour - 1/* + offset_season*/ : (24 - 1)] = (value / 2000);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"PanelTecViewBase::GetHoursResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }

                //??? ????? ??? ???????? ????? ??????? ?????
                dtNeeded = dtNeeded.AddMinutes(30);

                i ++;
            }

            i = 0;
            for (hour = 0; hour < m_valuesHours.Length; hour ++) {
                hourVal = -1F;

                for (j = 0; j < 2; j ++) {
                    halfVal = -1F;
                    //?????? ??
                    for (i = 0; i < cntTG; i ++)
                    {
                        if (powerHourHalf[i, j, hour] < 0)
                        {
                            //??? ?????? ??? ??
                            //break;
                        }
                        else
                        {
                            if (halfVal < 0) halfVal = 0F; else ;
                            halfVal += powerHourHalf[i, j, hour];
                        }
                    }

                    if (i < cntTG) {
                        //??? ?????? ??? ?????? ?? ??
                        //break;
                    } else {
                        //??? ???? ?? ???? ??????
                        if (! (halfVal < 0))
                        {
                            if (hourVal < 0) hourVal = 0F; else ;
                            hourVal += halfVal;
                        } else {
                            //??? ?????? ?? ??????? ?? ??? ?????? ??
                            if (currHour == true)
                                break;
                            else
                                ;
                        }
                    }
                }

                //Logging.Logg().Debug(@"TecView::GetHoursFactReuest () - hour=" + hour + @"; indxHalf=" + j + @"; halfVal=" + halfVal + @"; hourVal=" + hourVal);

                if (j < 2)
                    //??? ?????? ?? ???? ?? ?????????
                    if (!(hour > serverTime/*.Add(m_tsOffsetToMoscow)*/.Hour))
                        break;
                    else
                        //hour > m_curDate.Hour
                        if (j == 0)
                            //1-?? ???????
                            ; //break;
                        else
                            //2-?? ???????
                            ;
                else
                    if (! (hourVal < 0))
                        m_valuesHours [hour].valuesFact += hourVal;
                    else
                        ; //??? ?????? ?? ???
            }

            if (hour == m_valuesHours.Length)
                hour = 24;
            else
                if (hour == 0)
                    ; //hour = 1;
                else
                    ;

            int iRes = 0;

            //Logging.Logg().Debug(@"TecView::GetHoursFactReuest () - hour=" + hour);

            if (currHour == true)
            {//??????????? ???./????
                if (hour < 0)
                {
                    string strMes = @"TecView::GetHoursFactResponse (hour = " + hour + @") - ...";
                    //Logging.Logg().Error(strMes);
                    //throw new Exception(strMes);
                }
                else ;

                lastHour = hour;

                if (lastHour < dtServer.Hour)
                {//?????? ????????? ??????? ????????
                    m_markWarning.Marked((int)INDEX_WARNING.LAST_HOUR);
                    lastHour = dtServer.Hour;
                    iRes = 1;
                }
                else
                {
                    if ((dtServer.Hour == 0) && (!(lastHour == 24)) && (!(dtNeeded.Date == dtServer.Date)))
                    {//??????? ????? ??????? ?????
                        m_markWarning.Marked((int)INDEX_WARNING.LAST_HOUR);
                        lastHour = 24;
                        iRes = 1;
                    }
                    else
                    {
                        if (! (lastHour == 0))
                        {//?? ?????? ?????
                            for (i = 0; i < cntTG; i++)
                            {
                                if ((half & 1) == 1)
                                {
                                    //MessageBox.Show("sensor " + sensorId2TG[i].name + ", h1 " + sensorId2TG[i].receivedHourHalf1[lastHour - 1].ToString());
                                    if (powerHourHalf[i, 0, lastHour - 1] < 0)
                                    {//?????? ????????? 1-??? ???????????? ????????
                                        m_markWarning.Marked((int)INDEX_WARNING.LAST_HOURHALF);
                                        iRes = 2;
                                        break;
                                    }
                                    else
                                        ;
                                }
                                else
                                {
                                    //MessageBox.Show("sensor " + sensorId2TG[i].name + ", h2 " + sensorId2TG[i].receivedHourHalf2[lastHour - 1].ToString());
                                    if (powerHourHalf[i, 1, lastHour - 1] < 0)
                                    {
                                        //?????? ????????? 2-??? ???????????? ????????
                                        m_markWarning.Marked((int)INDEX_WARNING.LAST_HOURHALF);
                                        iRes = 2;
                                        break;
                                    }
                                    else
                                        ;
                                }
                            }
                        }
                        else
                            ; //?????? ?????
                    }
                }
            }
            else
                ; //??????????? ?????????????

            //Logging.Logg().Debug(@"TecView::GetHoursFactReuest () - lastHour=" + lastHour);

            lastReceivedHour = lastHour;

            return iRes;
        }

        private int getHourTMResponse(DataTable table)
        {
            //Logging.Logg().Debug(@"TecView::GetHoursTMResponse (lastHour=" + lastHour + @") - Rows.Count=" + table.Rows.Count);

            int iRes = -1
                , hour = -1;

            string nameFieldValue = currHour == true ? @"VALUE" : @"VALUE0";
            string [] checkFields = null;

            switch (TEC.s_SourceSOTIASSO) {
                case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                    checkFields = new string[] { @"KKS_NAME", @"VALUE", @"tmdelta", @"last_changed_at" };
                    break;
                case TEC.SOURCE_SOTIASSO.AVERAGE:
                case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                    checkFields = new string[] { nameFieldValue, @"HOUR" };
                    break;
                default:
                    break;
            }

            iRes = ! (checkFields == null) ? CheckNameFieldsOfTable(table, checkFields) == true ? 0 : -1 : -1;

            if (iRes == -1)
                ;
            else {
                if (table.Rows.Count == 0)
                    if (serverTime.Minute < 3)
                    {
                        //...? ?????? ???? ???????? ????? ?? ???? ???
                        iRes = 1;
                    }
                    else
                        iRes = -1;
                else ;

                if (iRes == 0)
                    switch (TEC.s_SourceSOTIASSO)
                    {
                        case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                            hour = lastHour;

                            List<string> listSensors = new List<string>(m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO, HDateTime.INTERVAL.MINUTES).Split(','));
                            double[] valHours = new double[listSensors.Count + 1];
                            //60 ??? * 60 ??? = 1 ???
                            valHours = avgInterval(table
                                                    , m_curDate.Date.AddHours(hour)
                                                    , 60 * 60
                                                    , listSensors
                                                    , out iRes);
                            //if (iRes == 0)
                                m_valuesHours[hour].valuesFact = valHours[listSensors.Count];
                            //else ;
                            break;
                        case TEC.SOURCE_SOTIASSO.AVERAGE:
                        case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                            double val = -1F;

                            foreach (DataRow r in table.Rows)
                            {
                                if (Int32.TryParse(r[@"HOUR"].ToString(), out hour) == false) {
                                    iRes = -1;
                                    break;
                                }
                                else
                                    ;

                                if (double.TryParse(r[nameFieldValue].ToString(), out val) == false) {
                                    iRes = -1;
                                    break;
                                }
                                else
                                    ;

                                if (val < 1)
                                    val = 0;
                                else
                                    ;

                                m_valuesHours[hour].valuesFact += val;
                            }
                            break;
                        default:
                            break;
                    }
                else
                    ; //??? ????? ?? ??????? ??????? ??? ?????????
            }

            switch (iRes)
            {
                case -12:
                    iRes = 0;
                    break;
                case -2:
                    if (! (hour < serverTime.Hour))
                        iRes = 0;
                    else
                        ;
                    break;
                default:
                    break;
            }

            if (iRes < 0)
                ;
            else
                if (currHour == true)
                    if (hour < 0)
                    {
                        string strMes = @"TecView::GetHourTMResponse () - hour = " + hour + @" ...";
                        //Logging.Logg().Error(strMes);
                        //throw new Exception(strMes);
                    }
                    else ;
                else
                    ;

            return 0;
        }

        private int getHoursTMResponse(DataTable table, bool bErrorCritical = true)
        {
            int iRes = -1
                , hour = -1;
            double val = -1F;

            string nameFieldValue = currHour == true ? @"VALUE" : @"VALUE0";
            string[] checkFields = null;

            switch (TEC.s_SourceSOTIASSO)
            {
                case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                    checkFields = new string[] { @"KKS_NAME", @"VALUE", @"tmdelta", @"last_changed_at" };
                    break;
                case TEC.SOURCE_SOTIASSO.AVERAGE:
                case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                    checkFields = new string[] { @"VALUE", @"HOUR" };
                    break;
                default:
                    break;
            }

            iRes = !(checkFields == null) ? CheckNameFieldsOfTable(table, checkFields) == true ? 0 : -1 : -1;

            if (iRes == 0) {
                //Logging.Logg().Debug(@"TecView::GetHoursTMResponse (lastHour=" + lastHour + @") - Rows.Count=" + table.Rows.Count);

                if (table.Rows.Count == 0)
                    if (serverTime.Minute < 3)
                    {
                        //...? ?????? ???? ???????? ????? ?? ???? ???
                        iRes = 1;
                    }
                    else
                        iRes = -1;
                else ;

                if (iRes == 0)
                    switch (TEC.s_SourceSOTIASSO)
                    {
                        case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                            List <string> listSensors = new List <string> (m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO, HDateTime.INTERVAL.MINUTES).Split (','));
                            double[] valHours = new double[listSensors.Count];
                            for (hour = 0; (hour < (m_valuesHours.Length + 0)) && (iRes == 0); hour ++)
                            {
                                valHours = avgInterval (table
                                                        , m_curDate.Date.AddHours(hour)
                                                        , 60 * 60
                                                        , listSensors
                                                        , out iRes);
                                if (iRes == 0)
                                    m_valuesHours[hour].valuesFact = valHours[listSensors.Count];
                                else
                                    ;

                                //Console.WriteLine (@"TecView::GetHoursTMResponse () - hour=" + hour + @", iRes=" + iRes);
                            }
                            break;
                        case TEC.SOURCE_SOTIASSO.AVERAGE:
                        case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                            foreach (DataRow r in table.Rows)
                            {
                                if (Int32.TryParse(r[@"HOUR"].ToString(), out hour) == false) {
                                    iRes = -22;
                                    break;
                                }
                                else
                                    ;

                                if (double.TryParse(r[nameFieldValue].ToString(), out val) == false) {
                                    iRes = -21;
                                    break;
                                }
                                else
                                    ;

                                m_valuesHours[hour].valuesFact += val;
                            }
                            break;
                        default:
                            break;
                    }
                else
                    ;
            }
            else
                ;

            switch (TEC.s_SourceSOTIASSO)
            {
                case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                    switch (iRes)
                    {
                        case -12:
                            iRes = 0;
                            break;
                        case -2:
                            if (!(hour < serverTime.Hour))
                                iRes = 0;
                            else
                                ;
                            break;
                        default:
                            break;
                    }
                    break;
                case TEC.SOURCE_SOTIASSO.AVERAGE:
                case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                    break;
                default:
                    break;
            }

            if (iRes < 0)
            {
                if (bErrorCritical == true)
                    lastHour = serverTime.Hour; //24
                else
                    ;
            }
            else
            {
                if (bErrorCritical == true) {
                    m_markWarning.UnMarked ((int)INDEX_WARNING.LAST_HOUR);
                    m_markWarning.UnMarked((int)INDEX_WARNING.LAST_HOURHALF);
                }
                else
                    ;

                if (currHour == true)
                {
                    if (hour < 0)
                    {
                        string strMes = @"TecView::GetHoursTMResponse () - hour = " + hour + @" ...";
                        //Logging.Logg().Error(strMes);
                        //throw new Exception(strMes);
                    }
                    else ;

                    if (bErrorCritical == true)
                        if (iRes == 0)
                            switch (TEC.s_SourceSOTIASSO)
                            {
                                case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                                    switch (hour)
                                    {
                                        case 0:
                                            lastHour = 0;
                                            break;
                                        case 1:
                                            lastHour = 0;
                                            m_valuesHours[lastHour + 1].valuesFact = 0F;
                                            break;
                                        default:
                                            lastHour = hour - 2;
                                            m_valuesHours[lastHour + 1].valuesFact =
                                            m_valuesHours[lastHour + 2].valuesFact = 0F;
                                            break;
                                    }
                                    break;
                                case TEC.SOURCE_SOTIASSO.AVERAGE:
                                case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                                    lastHour = hour + 0;
                                    break;
                                default:
                                    break;
                            }
                        else
                            if (iRes == 1)
                                lastHour = serverTime.AddHours (-1).Hour + 1;
                            else
                                ;
                    else
                        ;

                    ////?? ?????????? ???????? ?? ??????? ??? ???
                    //if (lastHour < (m_valuesHours.Length - 1))
                    //    m_valuesHours[lastHour].valuesFact = 0F;
                    //else ;
                }
                else
                    ;
            }

            return (bErrorCritical == true) ? iRes : 0;
        }

        private int getHoursTMSNPsumResponse(DataTable table)
        {
            int iRes = 0;
            int i = -1
                , hour = -1;

            if (table.Rows.Count > 0)
                for (i = 0; i < table.Rows.Count; i++)
                {
                    hour = Int32.Parse(table.Rows[i][@"HOUR"].ToString());

                    m_valuesHours[hour].valuesTMSNPsum = double.Parse(table.Rows[i][@"VALUE"].ToString());
                }
            else
                iRes = -1;

            return iRes;
        }

        private int getLastMinutesTMResponse(DataTable table_in, DateTime dtReq)
        {
            int iRes = -1;
            int i = -1,
                hour = -1
                ////26.10.2014 u/ ???
                //, offsetUTC = (int)HDateTime.GetUTCOffsetOfMoscowTimeZone().TotalHours
                ;
            double val = -1F;
            TG tg = null;
            DateTime dtVal = DateTime.Now;
            string [] checkFields = null;

            switch (TEC.s_SourceSOTIASSO) {
                case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                    checkFields = new string[] { @"KKS_NAME", @"value", @"tmdelta", @"last_changed_at" };
                    break;
                case TEC.SOURCE_SOTIASSO.AVERAGE:
                case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                    checkFields = new string[] { @"KKS_NAME", @"VALUE", @"last_changed_at" };
                    break;
                default:
                    break;
            }

            iRes = ! (checkFields == null) ? CheckNameFieldsOfTable(table_in, checkFields) == true ? 0 : -1 : -1;

            if (iRes == -1)
                ;
            else {
                if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.AVERAGE)
                {
                    DataRow []tgRows = null;

                    if (CurrentKey.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                    #region ???
                    {
                        foreach (TECComponent g in _localTECComponents)
                        {
                            foreach (TECComponentBase tc in g.ListLowPointDev)
                            {
                                tg = tc as TG;

                                //for (i = 0; i < tg.m_power_LastMinutesTM.Length; i++)
                                //{
                                //    tg.m_power_LastMinutesTM[i] = 0;
                                //}

                                tgRows = table_in.Select(@"[KKS_NAME]='" + tg.m_SensorsString_SOTIASSO + @"'");

                                for (i = 0; i < tgRows.Length; i++)
                                {
                                    if (!(tgRows[i]["value"] is DBNull))
                                        if (double.TryParse(tgRows[i]["value"].ToString(), out val) == false)
                                            return -1;
                                        else
                                            ;
                                    else
                                        val = 0F;

                                    //if ((!(value < 1)) && (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false))
                                    if (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false)
                                        return -1;
                                    else
                                        ;

                                    ////26.10.2014 u/ ???
                                    //dtVal = dtVal.AddHours(offsetUTC);
                                    hour = dtVal.Hour + 1; //?.?. ???.59 ?? ?????????? ????
                                    //if (!(hour < 24)) hour -= 24; else ;
                                    if ((hour > 0) && (! (hour > m_valuesHours.Length)))
                                    {
                                        m_dictValuesLowPointDev[tg.m_id].m_power_LastMinutesTM[hour - 0] = val;

                                        //?????? ? ?????? ???????? ???????? ????? ?????
                                        if (val > 1)
                                        {
                                            m_valuesHours[hour - 1].valuesLastMinutesTM += val;
                                            m_dictValuesTECComponent[hour - 0][tg.m_keys_owner.Find(key => key.Mode == FormChangeMode.MODE_TECCOMPONENT.GTP).Id].valuesLastMinutesTM += val;
                                        }
                                        else
                                            ;
                                    }
                                    else
                                        ;
                                }
                            }
                        }
                    }
                    #endregion
                    else
                    #region ?????????? ???
                    {
                        foreach (TECComponent comp in _localTECComponents)
                        {
                            //for (i = 0; i < comp.ListLowPointDev [0].m_power_LastMinutesTM.Length; i++)
                            //{
                            //    comp.ListLowPointDev[0].m_power_LastMinutesTM[i] = 0;
                            //}

                            tgRows = table_in.Select(@"[KKS_NAME]='" + (comp.ListLowPointDev[0] as TG).m_SensorsString_SOTIASSO + @"'");

                            for (i = 0; i < tgRows.Length; i++)
                            {
                                if (tgRows[i] == null)
                                    continue;
                                else
                                    ;

                                try
                                {
                                    if (double.TryParse(tgRows[i]["value"].ToString(), out val) == false)
                                        return -1;
                                    else
                                        ;

                                    if (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false)
                                        return -1;
                                    else
                                        ;
                                }
                                catch (Exception e)
                                {
                                    Logging.Logg().Exception(e, @"PanelTecViewBase::GetLastMinutesTMResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);

                                    dtVal = DateTime.Now.Date;
                                }

                                ////26.10.2014 u/ ???
                                //dtVal = dtVal.AddHours(offsetUTC);
                                hour = dtVal.Hour + 1;
                                if ((hour > 0) && (! (hour > m_valuesHours.Length)))
                                {
                                    m_dictValuesLowPointDev[comp.ListLowPointDev[0].m_id].m_power_LastMinutesTM[hour - 0] = val;

                                    if (val > 1)
                                        m_valuesHours[hour - 1].valuesLastMinutesTM += val;
                                    else
                                    ;
                                } else ;
                            }
                        }
                    }
                    #endregion
                } else {
                    if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_APP)
                    {
                        dtVal = dtReq.Date.AddMinutes(59);
                        List<string> listSensors = new List<string>(m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO, HDateTime.INTERVAL.MINUTES).Split(','));
                        int[] arIds = new int[listSensors.Count]
                            , arOwnerGTPIds = new int[listSensors.Count];
                        double[] valLastMins = new double[listSensors.Count + 1];
                        foreach (string strId in listSensors)
                        {
                            tg = m_tec.FindTGById(Int32.Parse(strId), TG.INDEX_VALUE.TM, HDateTime.INTERVAL.MINUTES);
                            if (tg == null)
                                return -1;
                            else
                                ;

                            arIds[listSensors.IndexOf(strId)] = tg.m_id;
                            arOwnerGTPIds[listSensors.IndexOf(strId)] = tg.m_keys_owner.Find (key => key.Mode == FormChangeMode.MODE_TECCOMPONENT.GTP).Id;
                        }

                        for (hour = 0; (hour < m_valuesHours.Length) && (iRes == 0); hour++, dtVal = dtVal.AddHours(1))
                        {
                            valLastMins = avgInterval(table_in
                                            , dtVal
                                            , 60
                                            , listSensors
                                            , out iRes);

                            if (iRes == 0)
                            {
                                foreach (string strId in listSensors)
                                {
                                    int indx = listSensors.IndexOf(strId);
                                    m_dictValuesLowPointDev[arIds[indx]].m_power_LastMinutesTM[hour + 1] = valLastMins[indx];
                                    if (CurrentKey.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                                        m_dictValuesTECComponent[hour + 1][arOwnerGTPIds[indx]].valuesLastMinutesTM += valLastMins[indx];
                                    else
                                        ;
                                }

                                m_valuesHours[hour + 0].valuesLastMinutesTM = valLastMins[listSensors.Count];
                            }
                            else
                                ;
                        }

                        switch (iRes)
                        {
                            case -12:
                                iRes = 0;
                                break;
                            case -2:
                                if (!(hour < dtReq.Hour))
                                    iRes = 0;
                                else
                                    ;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                        ;
                }
            }

            return iRes;
        }

        private int getMinsFactResponse(DataTable table)
        {
            int iRes = 0;

            double minuteVal = 0, value;
            TG tgTmp;
            bool end = false;
            DateTime dt
                , dtNeeded = DateTime.MinValue;
            int i, j = 0, min = 0
                , id_object = -1, id_item = -1
                , season = 0, need_season = 0, max_season = 0
                , intervalMin = m_idAISKUEParNumber == ID_AISKUE_PARNUMBER.FACT_03 ? 3 :
                    m_idAISKUEParNumber == ID_AISKUE_PARNUMBER.FACT_30 ? 30 :
                        0;
            TG.AISKUE_KEY id;
            bool jump = false; // ??????? ??????? ????? ???????? ???????????????

            if (CheckNameFieldsOfTable(table, new string[] { @"ID", @"DATA_DATE", @"SEASON", @"VALUE0", "OBJECT", "ITEM" }) == false)
                iRes = -1;
            else
                ;
            //??????? ???????? ??????? ??????????? ????? ? ???????-??????????
            if (iRes == 0)
            {
                lastMin = 0;

                if (table.Rows.Count > 0)
                {
                    //?????????? 1-? ??????? ??????? ? ????? ??? ???? ??????? ???????
                    //if (table.Columns.Contains(@"DATA_DATE") == true)
                        if (DateTime.TryParse(table.Rows[0][@"DATA_DATE"].ToString(), out dt) == false)
                            iRes = -1;
                        else
                            ;
                    //else
                    //    return false;

                    if (iRes == 0)
                    //if (table.Columns.Contains(@"SEASON") == true)
                        if (int.TryParse(table.Rows[0][@"SEASON"].ToString(), out season) == false)
                            iRes = -4;
                        else
                            ;
                    //else
                    //    return false;
                    else
                        ;

                    if (iRes == 0)
                    {
                        need_season = max_season = season;
                        min = (int)(dt.Minute / intervalMin);
                        dtNeeded = dt;
                    }
                    else
                        ;
                }
                else
                {
                    //?????? - ??? ?? ????? ??????
                    if (currHour == true)
                        if (!((m_curDate.Minute / intervalMin) == 0))
                        {//?????? - ????? 3-???? > 1
                            m_markWarning.Marked((int)INDEX_WARNING.LAST_MIN);
                            //lastMin = ((m_curDate.Minute) / 3) + 1;
                        }
                        else
                            ; //?????
                    else
                        ;

                    return 0;
                }

                //????????? ??????? ????? ??????? "???????" ?????? (? ??????? ?????? ???????)
                for (i = 0; i < table.Rows.Count; i++)
                {
                    if (!int.TryParse(table.Rows[i][@"SEASON"].ToString(), out season)) {
                        //return -4;
                        iRes = -4;
                        break;
                    }
                    else
                        ;

                    if (max_season < season)
                    {
                        max_season = season;
                        break;
                    }
                    else
                        ;
                }

                if ((iRes == 0) && (dtNeeded > DateTime.MinValue))
                {
                    if (currHour == true)
                    {//?? ??????????? ?????? "???????" ???
                        if (! (need_season == max_season))
                        {//????? ?????????? ??????? - ?????? ? ??????? ????????
                            //m_valuesHours.addonValues = true;
                            //m_valuesHours.hourAddon = lastHour - 1;
                            need_season = max_season;
                        }
                        else
                            ; //????? ???????? ??? ???? ???????
                    }
                    else
                    {//?? ??????????? ??????? "?????????????"
                        //if (m_valuesHours.addonValues == true)
                            need_season = max_season;
                        //else ;
                    }

                    for (i = 0; (end == false) && (min < m_valuesMins.Length); min += IntervalMultiplier)
                    {
                        //??? 1-? ??????? ?????? == false
                        if (jump == true)
                        {
                            min -= IntervalMultiplier;
                        }
                        else
                        {//?????? ??????????? ??? 1-?? ???????
                            m_valuesMins[min].valuesFact = 0;
                            minuteVal = 0;
                        }

                        //
                        jump = false;
                        for (j = 0; j < CountLowPointDev; j++, i++)
                        {
                            if (i >= table.Rows.Count)
                            {
                                end = true; //????????? ???????? ?????? ?? ????? 'i'
                                break; //????? ?? ????? 'j'
                            }
                            else
                                ;

                            try
                            {
                                if (!DateTime.TryParse(table.Rows[i][@"DATA_DATE"].ToString(), out dt)) {
                                    //return -1;
                                    iRes = -1;
                                    break;
                                }
                                else
                                    ;

                                if (!int.TryParse(table.Rows[i][@"SEASON"].ToString(), out season)) {
                                    //return -4;
                                    iRes = -4;
                                    break;
                                }
                                else
                                    ;
                            }
                            catch (Exception e)
                            {
                                Logging.Logg().Exception(e, @"PanelTecViewBase::GetLastMinutesTMResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);

                                dt = DateTime.Now.Date;
                            }

                            if (!(season == need_season))
                            {
                                jump = true;
                                i++;
                                break;
                            }
                            else
                                ;

                            if (! (dt.CompareTo(dtNeeded) == 0))
                            {
                                break;
                            }
                            else
                                ;

                            //if (!int.TryParse(table.Rows[i][@"ID"].ToString(), out id)) {
                            //    //return -1;
                            //    iRes = -1;
                            //    break;
                            //}
                            //else
                            //    ;
                            // ChrjapinAN, 26.12.2017 ??????? ?? ????????? ???? "OBJECT/ITEM"
                            if ((int.TryParse (table.Rows [i] [@"OBJECT"].ToString (), out id_object) == false)
                                || (int.TryParse (table.Rows [i] [@"ITEM"].ToString (), out id_item) == false)) {
                                //return -1;
                                iRes = -1;
                                break;
                            } else
                                id = new TG.AISKUE_KEY () { IdObject = id_object, IdItem = id_item };

                            tgTmp = m_tec.FindTGById(id, TG.INDEX_VALUE.FACT, HDateTime.INTERVAL.MINUTES);

                            if (tgTmp == null) {
                                //return -2;
                                iRes = -2;
                                break;
                            }
                            else
                                ;

                            if (!double.TryParse(table.Rows[i][@"VALUE0"].ToString(), out value)) {
                                //return -3;
                                iRes = -3;
                                break;
                            }
                            else
                                ;

                            switch (m_tec.Type)
                            {
                                case TEC.TEC_TYPE.COMMON:
                                    break;
                                case TEC.TEC_TYPE.BIYSK:
                                    value *= 20;
                                    break;
                                default:
                                    break;
                            }

                            minuteVal += value;
                            m_dictValuesLowPointDev[tgTmp.m_id].m_powerMinutes [min] = value / 1000;
                            //tgTmp.receivedMin[min] = true;

                            //??????? ????????? ???????? ???? ?? ?? ???? ????????
                            if (m_dictValuesLowPointDev[tgTmp.m_id].m_bPowerMinutesRecieved == false) m_dictValuesLowPointDev[tgTmp.m_id].m_bPowerMinutesRecieved = true; else ;
                        } //??? for (j = 0; j < CountTG; j++, i++)

                        //if ((j < CountTG) && ((end == false) || (jump == false)))
                        if (! (iRes == 0))
                            break;
                        else
                            ;

                        if (jump == false)
                        {
                            dtNeeded = dtNeeded.AddMinutes(intervalMin);

                            //MessageBox.Show("end " + end.ToString() + ", minVal " + (minVal / 1000).ToString());

                            //????????? 27.08.2015 ?.
                            // ????????? ????????, ????? ? ?? ??????????? ????????
                            // 1-?? ??? ????? ?? ??? ?????????? (???, ??, ???????)
                            // , ?? ???? ???????? ???? ?? ??? 1-??
                            // , ? ????????? ???????? ??? ????????? > 0
                            bool bMinInc = false;
                            //????????? ???????? ?? ?????? ??? ?????????
                            if (end == false)
                                //?????????? ??????? ?????????? ??????? ???????? 3-? ??? ?????????
                                bMinInc = true;
                            else
                                //????????? 18.11.2015 ?.
                                // ?????? ????????? ????????, ????? ? ?? ??????????? ???????? ...
                                //if (end == true)
                                //    //????? ??? ????????? - ???
                                //    //????????? ??????? ???????? ???? ?? ??? 1-?? ?? ??
                                //    // ? ???????? ??? ????????? > 0
                                //    if ((j > 0) && ((minuteVal / 1000) > 1))
                                //        //?????????? ??????? ?????????? ??????? ???????? 3-? ??? ?????????
                                //        bMinInc = true;
                                //    else
                                //        ;
                                //else
                                //    //true, false, ???
                                    ;
                            //????????? ??????? ?????????? ??????? ???????? 3-? ??? ?????????
                            if (bMinInc == true)
                            {
                                //????????? ???????? ??? ?????????? ?????????
                                m_valuesMins[min].valuesFact = minuteVal / 1000;
                                //????????? ??????
                                lastMin = min + IntervalMultiplier;
                            }
                            else
                                ;
                        }
                        else
                            ;
                    } //??? for (i = 0; (end == false) && (min < m_valuesMins.Length); min++)

                    /*f2.FillMinValues(lastMin, selectedTime, m_tecView.m_valuesMins.valuesFact);
                    f2.ShowDialog();*/

                    if (! (lastMin > ((m_curDate.Minute - 1) / intervalMin)))
                    {
                        m_markWarning.Marked((int)INDEX_WARNING.LAST_MIN);
                        //lastMin = ((selectedTime.Minute - 1) / 3) + 1;
                    } else {
                    }

                    if (lastMin < 0)
                    {
                        string strMes = @"TecView::GetMinsFactResponse () - lastMin = " + lastMin;
                        //Logging.Logg().Error(strMes);
                        throw new Exception(strMes);
                    }
                    else
                        ;
                }
                else
                    ; //???????? ??????? "???????" ??????
            }
            else
                ; //???????? ??????? ???????? ? ???????

            return iRes;
        }

        private double [] avgInterval (DataTable table, DateTime dtReqBegin, int secInterval, List <string> listSensors, out int iRes) {
            // -1 - 
            // -2 - ?? ????./???????? ???-?? ????? < ???-?? ??
            // -31 - ????????????? ???????? ???? [ID]
            // -32 - ... [VALUE]
            // -33 - ... [tmdelt]a
            // -34 - ... [last_changed_at]
            // -35 - ... [MINUTE]
            // -4 - ?? ?????? ?????? ?? ?? ?????????????? [ID]
            // -5 - 
            // -6 -  
            // -7 - ??? ???????? ?? ????????./????????? ???, ??? ?????., ?????? ?? ??
            // -8 - ????? ????????????? = 0 ???, ??? ?????., ?????? ?? ??
            // -9 - ????/??. 1-?? ????. ?? ???/???????? ??????????? ?????????
            // -10 - ???-?? ??????? ?? ??????? ??????? = 0
            // -11 -
            // -12 - ??? ????/????? 1-?? ???????? ?? ???./????????
            // -13 - ??? ????/??. ??./????. ?? ???./????????
            iRes = 0;
            //double dblRes = 0F;

            int i = -1, indx = -1
                , id = -1
                , tmDelta = -1
                ;
            double val = -1F;
            string strId = string.Empty;

            double [] tgPrevValues = new double [listSensors.Count]
                , tgCurValues = new double[listSensors.Count + 1];
            for (i = 0; i < listSensors.Count; i++) { listSensors[i] = listSensors [i].Trim (); tgPrevValues[i] = tgCurValues[i] = 0F; }
            tgCurValues [listSensors.Count] = 0F;

            DateTime dtReqEnd = dtReqBegin.AddSeconds(secInterval);

            DataRow[] rowsPrevInterval = table.Select(@"last_changed_at<'" + dtReqBegin.ToString(@"yyyy-MM-dd HH:mm:ss.fff") + @"'" +
                    @" AND last_changed_at>='" + dtReqBegin.AddSeconds(-1 * 60).ToString(@"yyyy-MM-dd HH:mm:ss.fff") + @"'")
                , rowsCurInterval = table.Select(@"last_changed_at>='" + dtReqBegin.ToString(@"yyyy-MM-dd HH:mm:ss.fff") + @"'" +
                    @" AND last_changed_at<'" + dtReqEnd.ToString(@"yyyy-MM-dd HH:mm:ss.fff") + @"'");

            DateTime lastChangedAt = DateTime.MaxValue;
            DateTime[] arBeginInterval
                , arEndInterval = new DateTime[listSensors.Count];

            if (rowsPrevInterval.Length < listSensors.Count)
                iRes = -2;
            else {
                for (i = 0; i < arEndInterval.Length; i++) arEndInterval[i] = DateTime.MinValue;
                foreach (DataRow r in rowsPrevInterval) {
                    //??? ID ??? KKS_NAME
                    strId = r[@"ID"].ToString();
                    if (Int32.TryParse(strId, out id) == false)
                    {
                        iRes = -31;
                        break;
                    }
                    else
                        ;

                    if (double.TryParse(r[@"VALUE"].ToString(), out val) == false)
                    {
                        iRes = -32;
                        break;
                    }
                    else
                        ;

                    if (Int32.TryParse(r[@"tmdelta"].ToString(), out tmDelta) == false)
                    {
                        iRes = -33;
                        break;
                    }
                    else
                        ;

                    if (DateTime.TryParse(r[@"last_changed_at"].ToString(), out lastChangedAt) == false)
                    {
                        iRes = -34;
                        break;
                    }
                    else
                        lastChangedAt = (DateTime)r[@"last_changed_at"];

                    indx = listSensors.IndexOf (strId);
                    if (arEndInterval[indx] < lastChangedAt)
                    {
                        arEndInterval[indx] = lastChangedAt;
                        tgPrevValues [indx] = val;
                    }
                    else
                        ;
                }
            }

            if (iRes == 0)
            {
                for (indx = 0; indx < listSensors.Count; indx ++)
                    if (tgPrevValues [indx] < 0)
                    {
                        iRes = -7;
                    }
                    else
                        ;

                if (iRes == 0)
                {
                    int [] sumDelta = new int [listSensors.Count];
                    double [] tgValuesEnd = new double [listSensors.Count];
                    int [] arDeltaEnd =  new int [listSensors.Count];

                    lastChangedAt = DateTime.MaxValue;
                    arBeginInterval = new DateTime [listSensors.Count];
                    for (i = 0; i < arBeginInterval.Length; i++) arBeginInterval [i] = DateTime.MaxValue;
                    for (i = 0; i < arEndInterval.Length; i++) { arEndInterval[i] = DateTime.MinValue; tgValuesEnd[i] = 0F; arDeltaEnd [i] = -1; }

                    foreach (DataRow r in rowsCurInterval) {
                        //??? ID ??? KKS_NAME
                        strId = r[@"ID"].ToString();
                        if (Int32.TryParse(strId, out id) == false)
                        {
                            iRes = -31;
                            break;
                        }
                        else
                            ;

                        if (double.TryParse(r[@"VALUE"].ToString(), out val) == false)
                        {
                            iRes = -32;
                            break;
                        }
                        else
                            ;

                        if (Int32.TryParse(r[@"tmdelta"].ToString(), out tmDelta) == false)
                        {
                            iRes = -33;
                            break;
                        }
                        else
                            ;

                        if (DateTime.TryParse(r[@"last_changed_at"].ToString(), out lastChangedAt) == false)
                        {
                            iRes = -34;
                            break;
                        }
                        else
                            //???????? 'fff' (??????????)
                            lastChangedAt = (DateTime)r[@"last_changed_at"];

                        //?????? ??
                        indx = listSensors.IndexOf (strId);
                        //"?????" ????? ????????? ??? ??
                        if (arBeginInterval [indx] > lastChangedAt)
                            arBeginInterval [indx] = lastChangedAt;
                        else
                            ;
                        //"??????" ????? ????????? ??? ??
                        if (arEndInterval [indx] < lastChangedAt)
                        {
                            arEndInterval[indx] = lastChangedAt;
                            tgValuesEnd [indx] = val;
                            arDeltaEnd [indx] = tmDelta;
                        }
                        else
                            ;

                        //???????? ?? ?????./????????
                        //if ((val > 1) && (tmDelta > 0))
                        if (tmDelta > 0)
                        {
                            if (val > 0F)
                                tgCurValues [indx] += val * tmDelta;
                            else
                                ;

                            sumDelta [indx] += tmDelta;
                        }
                        else
                            ;
                    }

                    int msecInterval = -1;
                    if (iRes == 0) {                        
                        for (indx = 0; indx < listSensors.Count; indx ++) {
                            //????????? ????/??. 1-?? ????. ?? ???./????????
                            if (! (arBeginInterval[indx] == DateTime.MaxValue))
                            {
                                msecInterval = (int)(arBeginInterval[indx] - dtReqBegin).TotalMilliseconds;
                                if (! (msecInterval < 0))
                                {
                                    tgCurValues[indx] += tgPrevValues[indx] * msecInterval;
                                    sumDelta [indx] += msecInterval;
                                }
                                else {
                                    iRes = -9;
                                    break;
                                }
                            }
                            else {
                                iRes = -12;
                                break;
                            }

                            //????????? ??????? ?? ????/??. ??./????. ?? ???./????????
                            if (!(arEndInterval[indx] == DateTime.MinValue))
                            {
                                //????????? ?? ??????????? ???./????. ?? ???./????????
                                tgCurValues[indx] -= tgValuesEnd[indx] * arDeltaEnd[indx];
                                sumDelta[indx] -= arDeltaEnd[indx];

                                //msecInterval = (int)(arEndInterval[indx].AddMilliseconds(arDeltaEnd[indx]) - dtReqEnd).TotalMilliseconds;
                                //if (! (msecInterval < 0)) {
                                    //??????? ????? ??????? ????????? - ????????? ?????????? ??????
                                    msecInterval = (int)(dtReqEnd - arEndInterval[indx]).TotalMilliseconds;
                                //} else {
                                    ////??? ???????? ????? ??????? ????????? -
                                    //// - ????????? ?????? ?? ????? ?????????
                                    //msecInterval = (int)(dtReqEnd - arEndInterval[indx]).TotalMilliseconds;
                                //}

                                if (tgCurValues[indx] == -1F)
                                    tgCurValues[indx] = 0F;
                                else
                                    ;
                                tgCurValues[indx] += tgValuesEnd[indx] * msecInterval;
                                sumDelta [indx] += msecInterval;
                            } else {
                                iRes = -13;
                                break;
                            }

                            if (sumDelta[indx] > 0) // == (dtReqEnd - dtReqBegin).TotalMilliseconds
                            {
                                tgCurValues[indx] = tgCurValues[indx] / sumDelta[indx];
                                if (tgCurValues[listSensors.Count] == -1F)
                                    tgCurValues[listSensors.Count] = 0F;
                                else
                                    ;
                                tgCurValues[listSensors.Count] += tgCurValues[indx];
                            }
                            else {
                                iRes = -8;
                                break;
                            }
                        }

                        if (! (iRes == 0))
                            tgCurValues[listSensors.Count] = 0F;
                        else
                            ;
                    }
                    else
                        ; //?????? ??? ?????????? ???????? ???./?????????
                }
                else
                    ; //iRes == -7 (??? ???????? ?? ????????./????????? ???, ??? ?????. ?????? ?? ??)
            }
            else
                ; //?????? ??? ?????????? ???????? ????????./?????????

            return tgCurValues;
        }

        private int getMinTMResponse(DataTable table)
        {
            //Logging.Logg().Debug(@"TecView::GetMinTMResponse (lastHour=" + lastHour + @", lastMin=" + lastMin + @") - Rows.Count=" + table.Rows.Count);

            if (lastMin == 21)
                return 0;
            else
                ;

            string  [] checkFields = null;

            if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.AVERAGE)
                checkFields = new string[] { @"VALUE" };
            else
                if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_APP)
                    checkFields = new string[] { @"KKS_NAME", @"VALUE", @"tmdelta", @"last_changed_at" };
                else
                    if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_TSQL)
                        checkFields = new string[] { @"KKS_NAME", @"VALUE" };
                    else
                        ;

            int iRes = ! (checkFields == null) ? CheckNameFieldsOfTable(table, checkFields) == true ? 0 : -1 : -1;

            int min = -1;
            double val = -1F;

            try
            {
                if (iRes == 0)
                {
                    iRes = table.Rows.Count > 0 ? 0 : -10;

                    if (iRes == 0)
                    {
                        //???
                        if (lastMin == 0) min = lastMin + 1; else min = lastMin;

                        if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.AVERAGE)
                        {
                            if (double.TryParse (table.Rows [0][@"VALUE"].ToString (), out val) == true)
                                m_valuesMins[min].valuesFact = val;
                            else
                                iRes = -32;
                        }
                        else
                            if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_APP)
                            {
                                int hour = lastHour - GetSeasonHourOffset(lastHour)
                                    ;

                                List<string> listSensors = new List<string>(m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO, HDateTime.INTERVAL.MINUTES).Split(','));
                                m_valuesMins[min].valuesFact = avgInterval(table
                                                                    , m_curDate.Date.AddHours(hour).AddSeconds(180 * (min - 1))
                                                                    , 180
                                                                    , listSensors
                                                                    , out iRes)[listSensors.Count];
                            }
                            else
                                if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_TSQL)
                                {
                                    string strId = string.Empty;

                                    foreach (DataRow r in table.Rows) {
                                        //??? ID ??? KKS_NAME
                                        if ((strId = r[@"ID"].ToString()).Equals (string.Empty) == true)
                                        {
                                            iRes = -31;
                                            break;
                                        }
                                        else
                                            ;

                                        if (double.TryParse(r[@"VALUE"].ToString(), out val) == false)
                                        {
                                            iRes = -32;
                                            break;
                                        }
                                        else
                                            ;

                                        //??????? ???
                                        if (!(val > 0))
                                            val = 0F;
                                        else
                                            ;

                                        m_valuesMins[min].valuesFact += val;
                                    }
                                }
                                else
                                    ;
                    }
                    else
                        ; //??? ????? ?? ??????? ???????
                }
                else
                    ; //-1 ??? ????????? ????? ?? ??????? ???????
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"TecView::GetMinTMResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            ////???
            //if (bRes == false)
            //{
            //}
            //else
            //{
            //}

            //???
            return 0;
        }

        private int getMinDetailTMResponse(DataTable table)
        {
            //Logging.Logg().Debug(@"TecView::GetMinTMResponse (lastHour=" + lastHour + @", lastMin=" + lastMin + @") - Rows.Count=" + table.Rows.Count);

            if ((lastMin == 61)
                && (currHour == true))
                //??? ??? ???? 'lastMin' ? ??????? ?? ????????????
                return 0;    
            else
                ;

            string[] checkFields = null;
            checkFields = new string[] { @"KKS_NAME", @"VALUE", @"tmdelta" };

            int iRes = !(checkFields == null) ? CheckNameFieldsOfTable(table, checkFields) == true ? 0 : -1 : -1;

            TG tgTmp;
            Dictionary <string, TG> dictTG = new Dictionary<string,TG> ();

            //int min = -1;
            double val = -1F;

            DateTime dtLastChangedAt = DateTime.MinValue;

            try
            {
                if (iRes == 0)
                {
                    iRes = table.Rows.Count > 0 ? 0 : -10;

                    if (iRes == 0)
                    {
                        //???
                        //if (lastMin == 0) min = lastMin + 1; else min = lastMin;

                        string kks_name = string.Empty;

                        foreach (DataRow r in table.Rows)
                        {
                            kks_name = r["KKS_NAME"].ToString();

                            if (dictTG.ContainsKey(kks_name) == false)
                                tgTmp = m_tec.FindTGById(kks_name, TG.INDEX_VALUE.TM, (HDateTime.INTERVAL)(-1));
                            else
                                tgTmp = dictTG[kks_name];

                            if (tgTmp == null)
                                return -1;
                            else
                                ;

                            if (!(r["value"] is DBNull))
                                if (double.TryParse(r["value"].ToString(), out val) == false)
                                    return -1;
                                else
                                    ;
                            else
                                val = -1F;

                            //????????? ????/????? ??? "??????????" (>= 1) ????????
                            if ((!(val < 1)) && (DateTime.TryParse(r["last_changed_at"].ToString(), out dtLastChangedAt) == false))
                                //?????? ?????????? ????/????? ??? "??????????" (>= 1) ????????
                                return -1;
                            else
                                ;

                            //??????? ???
                            if (!(val > 0))
                                val = 0F;
                            else
                                ;

                            //????????? ????????
                            if (m_dictValuesLowPointDev[tgTmp.m_id].m_powerSeconds[dtLastChangedAt.Second] < 0)
                                m_dictValuesLowPointDev[tgTmp.m_id].m_powerSeconds[dtLastChangedAt.Second] = val;
                            else
                                //?????????????
                                m_dictValuesLowPointDev[tgTmp.m_id].m_powerSeconds[dtLastChangedAt.Second] = (m_dictValuesLowPointDev[tgTmp.m_id].m_powerSeconds[dtLastChangedAt.Second] + val) / 2;
                        }

                        //Console.WriteLine(@"TecView::GetMinDetailTMResponse () - ???-?? ?????=" + table.Rows.Count);
                    }
                    else
                        ; //??? ????? ?? ??????? ???????
                }
                else
                    ; //-1 ??? ????????? ????? ?? ??????? ???????
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"TecView::GetMinTMResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            ////???
            //if (bRes == false)
            //{
            //}
            //else
            //{
            //}

            //???
            return 0;
        }

        private int getMinsTMResponse(DataTable table)
        {
            string [] checkFields = null;

            if ((TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_TSQL)
                || (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.AVERAGE))
                checkFields = new string[] { @"KKS_NAME", @"VALUE", @"MINUTE" };
            else
                if (TEC.s_SourceSOTIASSO == TEC.SOURCE_SOTIASSO.INSATANT_APP)
                    checkFields = new string[] { @"KKS_NAME", @"VALUE", @"tmdelta", @"last_changed_at" };
                else
                    ;

            int iRes = ! (checkFields == null) ? CheckNameFieldsOfTable(table, checkFields) == true ? 0 : -1 : -1
                , min = -1
                , maxMin = -1;
            double val = -1F;

            maxMin = (60 / GetIntervalOfTypeSourceData(HDateTime.INTERVAL.MINUTES)) + 1;

            if (iRes == 0)
            {
                iRes = table.Rows.Count > 0 ? 0 : -10; //??? ?????? -1, ???? ??? ?????? ??? ??????, ??? ?????????? ??????????? ?????

                if (iRes == 0)
                    switch (TEC.s_SourceSOTIASSO) {
                        case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                            int
                                //, id = -1
                                indx = -1;
                            int [] arIds = null;

                            DateTime dtReq = m_curDate.Date.AddHours(lastHour);
                            List<string> listSensors = new List<string>(m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO, HDateTime.INTERVAL.MINUTES).Split(','));
                            arIds = new int[listSensors.Count];
                            double[] valMins = new double[listSensors.Count + 1];

                            foreach (string strId in listSensors)
                            {
                                TG tg = m_tec.FindTGById (strId, TG.INDEX_VALUE.TM, HDateTime.INTERVAL.MINUTES);
                                if (tg == null) {
                                    //return -4;
                                    iRes = -4;
                                    break;
                                }
                                else
                                    ;

                                arIds[listSensors.IndexOf(strId)] = tg.m_id;
                            }

                            if (iRes == 0)
                                for (min = 0; (min < 60) && (iRes == 0); min ++, dtReq = dtReq.AddMinutes (1))
                                {
                                    valMins = avgInterval (table
                                                    , dtReq
                                                    , 60
                                                    , listSensors
                                                    , out iRes);

                                    if (iRes == 0)
                                    {
                                        foreach (string strId in listSensors)
                                        {                            
                                            indx = listSensors.IndexOf (strId);
                                            m_dictValuesLowPointDev[arIds [indx]].m_powerMinutes[min + 1] = valMins[indx];
                                        }

                                        m_valuesMins[min + 1].valuesFact = valMins[listSensors.Count];
                                    }
                                    else
                                        ;
                                }
                            else
                                ;
                            break;
                        case TEC.SOURCE_SOTIASSO.AVERAGE:
                        case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                            string kks_name = string.Empty;
                            int[] cntRecievedValues = new int[m_valuesMins.Length + 1];
                            TG tgTmp = null;
                            Dictionary<string, TG> dictTGRecievedValues = new Dictionary<string, TG>();

                            foreach (DataRow r in table.Rows)
                            {
                                kks_name = r[@"KKS_NAME"].ToString();

                                if (double.TryParse(r[@"VALUE"].ToString(), out val) == false)
                                {
                                    iRes = -32;
                                    break;
                                }
                                else
                                    ;

                                if (Int32.TryParse(r[@"MINUTE"].ToString(), out min) == false)
                                {
                                    iRes = -35; //last_changed_at
                                    break;
                                }
                                else
                                    ;

                                tgTmp = null;
                                if (dictTGRecievedValues.ContainsKey(kks_name) == false)
                                {
                                    tgTmp = m_tec.FindTGById(kks_name, TG.INDEX_VALUE.TM, HDateTime.INTERVAL.MINUTES);

                                    if (! (tgTmp == null))
                                        dictTGRecievedValues.Add(kks_name, tgTmp);
                                    else
                                        ;
                                }
                                else
                                    tgTmp = dictTGRecievedValues[kks_name];

                                if (tgTmp == null)
                                {
                                    iRes = -4;
                                    break;
                                }
                                else
                                    ;

                                //??????? ???
                                if (!(val > 0))
                                //if (val < 1)
                                    val = 0F;
                                else
                                    ;

                                min ++;
                                m_dictValuesLowPointDev[tgTmp.m_id].m_powerMinutes[min] = val;
                                m_valuesMins [min].valuesFact += val;

                                //??????? ????????? ???????? ???? ?? ?? ???? ????????
                                if (m_dictValuesLowPointDev[tgTmp.m_id].m_bPowerMinutesRecieved == false) m_dictValuesLowPointDev[tgTmp.m_id].m_bPowerMinutesRecieved = true; else ;

                                cntRecievedValues [min] ++;
                            }

                            switch (min)
                            {
                                case -1:
                                    break;
                                case 0:
                                    break;
                                case 1:
                                    break;
                                default:
                                    if (cntRecievedValues [min] < cntRecievedValues [min - 1])
                                    {
                                        foreach (valuesLowPointDev vals in m_dictValuesLowPointDev.Values)
                                            vals.m_powerMinutes[min] = 0;

                                        m_valuesMins[min].valuesFact = 0;

                                        min --;
                                    }
                                    else
                                        ;
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                else
                    ; //???-?? ????? == 0
            }
            else
                ;

            switch (iRes) {
                case -12:
                    iRes = 0;
                    break;
                case -2:
                    break;
                default:
                    break;
            }

            if (! (iRes == 0))
            {
                lastMin = maxMin;
            }
            else
            {
                switch (TEC.s_SourceSOTIASSO) {
                    case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                        if (min > 0)
                            lastMin = min - 0;
                        else
                            lastMin = 0;
                        break;
                    case TEC.SOURCE_SOTIASSO.AVERAGE:
                    case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                        lastMin = currHour == true ? min + 1 : maxMin;
                        break;
                    default:
                        break;
                }
            }

            m_markWarning.Set((int)INDEX_WARNING.LAST_MIN, !(iRes == 0));

            int interval = GetIntervalOfTypeSourceData (HDateTime.INTERVAL.MINUTES);

            if (interval > 0)
                if ((m_markWarning.IsMarked((int)INDEX_WARNING.LAST_MIN) == false) && (!(lastMin > ((m_curDate.Minute - 1) / interval))))
                {
                    m_markWarning.Marked(((int)INDEX_WARNING.LAST_MIN));
                    //lastMin = ((selectedTime.Minute - 1) / 1) + 1;
                }
                else
                    ;
            else
                m_markWarning.Marked(((int)INDEX_WARNING.LAST_MIN));

            return iRes;
        }

        private int getMinTMGenResponse(DataTable table)
        {
            //Logging.Logg().Debug(@"TecView::GetMinTMResponse (lastHour=" + lastHour + @", lastMin=" + lastMin + @") - Rows.Count=" + table.Rows.Count);

            if (lastMin > m_valuesMins.Length)
                return 0;
            else
                ;

            string  [] checkFields = null;

            switch (TEC.s_SourceSOTIASSO) {
                case TEC.SOURCE_SOTIASSO.AVERAGE:
                    checkFields = new string[] { @"KKS_NAME", @"VALUE" };
                    break;
                case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                    break;
                case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                    break;
                default:
                    break;
            }

            int iRes = -1;

            foreach (TECComponent g in _localTECComponents)
            {
                foreach (TECComponentBase tc in g.ListLowPointDev)
                {
                    m_dictValuesLowPointDev[tc.m_id].m_powerCurrent_TM = -1F;
                    m_dictValuesLowPointDev[tc.m_id].m_dtCurrent_TM = DateTime.MinValue;
                    m_dictValuesLowPointDev[tc.m_id].m_id_TM = -1;
                }
            }

            if ((!(checkFields == null)) && (checkFields.Length > 0))
            {
                iRes = ! (checkFields == null) ? CheckNameFieldsOfTable(table, checkFields) == true ? 0 : -1 : -1;

                if (iRes == 0)
                {
                    iRes = table.Rows.Count > 0 ? 0 : 10;

                    if (iRes == 0)
                    {
                        switch (TEC.s_SourceSOTIASSO) {
                            case TEC.SOURCE_SOTIASSO.AVERAGE:
                                string strIdTM = string.Empty;
                                double val = -1F;
                                TG tgTmp;
                                Dictionary <string, TG> dictTGRecievedValues =  new Dictionary <string, TG> ();
                                
                                foreach (DataRow r in table.Rows) {
                                    strIdTM = r[@"KKS_NAME"].ToString();
                                    
                                    tgTmp = null;
                                    if (dictTGRecievedValues.ContainsKey(strIdTM) == false)
                                    {
                                        tgTmp = m_tec.FindTGById(strIdTM, TG.INDEX_VALUE.TM, HDateTime.INTERVAL.MINUTES);

                                        if (! (tgTmp == null))
                                            dictTGRecievedValues.Add(strIdTM, tgTmp);
                                        else
                                            ;
                                    }
                                    else
                                        tgTmp = dictTGRecievedValues[strIdTM];

                                    if (tgTmp == null)
                                    {
                                        iRes = -4;
                                        break;
                                    }
                                    else
                                        ;

                                    //??????? ???
                                    if (!(val > 0))
                                        val = 0F;
                                    else
                                        ;

                                    double.TryParse(r[@"VALUE"].ToString(), out m_dictValuesLowPointDev[tgTmp.m_id].m_powerCurrent_TM);
                                }
                                break;
                            case TEC.SOURCE_SOTIASSO.INSATANT_APP:
                                break;
                            case TEC.SOURCE_SOTIASSO.INSATANT_TSQL:
                                break;
                            default:
                                break;
                        }
                    }
                    else
                        ;
                }
                else
                    ;
            }
            else
                ;

            return iRes;
        }

        private void getSeason(DateTime date, int db_season, out int season)
        {
            season = db_season - date.Year - date.Year;
            if (season < 0)
                season = 0;
            else
                if (season > 2)
                    season = 2;
                else
                    ;
        }

        private int equalePBR(string l1, string l2)
        {
            int iRes = 0;

            int num1 = -1,
                num2 = -1;

            //??? ?? ????????? ????????? ??????????? ?????? ???
            num1 = GetPBRNumber (l1, out iRes, false);
            num2 = GetPBRNumber (l2, out iRes, false);

            iRes = num1 > num2 ? 1
                : num1 < num2 ? -1
                    : 0;

            return iRes;
        }

        private int getPPBRValuesResponse(DataTable table)
        {
            int iRes = 0;

            if (!(table == null))
                m_tablePPBRValuesResponse = table.Copy();
            else
                ;

            return iRes;
        }

        private void getHoursFactRequest(DateTime date)
        {
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.hoursRequest(date, m_tec.GetSensorsString(indx_TEC, CONN_SETT_TYPE.DATA_ASKUE, HDateTime.INTERVAL.HOURS)));
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.hoursRequest(date, m_tec.GetSensorsString(m_indx_TECComponent, CONN_SETT_TYPE.DATA_ASKUE, HDateTime.INTERVAL.HOURS)));
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_AISKUE], m_tec.hoursFactRequest(date, m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_AISKUE, HDateTime.INTERVAL.HOURS)));

            //Debug.WriteLine(@"TecView::GetHoursFactRequest () - DATE=" + date.ToString());
        }

        private void getHourTMRequest(DateTime date, int lh)
        {
            int interval = GetIntervalOfTypeSourceData(HDateTime.INTERVAL.HOURS);

            if (interval > 0)
                Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.hourTMRequest(date, lh, m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO, HDateTime.INTERVAL.HOURS), interval));
            else
                ;
        }

        private void getHoursTMRequest(DateTime date)
        {
            int interval = GetIntervalOfTypeSourceData(HDateTime.INTERVAL.HOURS);

            if (interval > 0)
                Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.hoursTMRequest(date, m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO, HDateTime.INTERVAL.HOURS), interval));
            else
                ;
        }

        private void getMinsFactRequest(int hour)
        {
            //tec.Request(CONN_SETT_TYPE.DATA_ASKUE, tec.minsRequest(selectedTime, hour, sensorsStrings_Fact[(int)HDateTime.INTERVAL.MINUTES]));
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.minsRequest(selectedTime, hour, m_tec.GetSensorsString(indx_TEC, CONN_SETT_TYPE.DATA_ASKUE, HDateTime.INTERVAL.MINUTES)));
            //m_tec.Request(CONN_SETT_TYPE.DATA_ASKUE, m_tec.minsRequest(selectedTime, hour, m_tec.GetSensorsString(m_indx_TECComponent, CONN_SETT_TYPE.DATA_ASKUE, HDateTime.INTERVAL.MINUTES)));
            //26.10.2014 ?.
            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_AISKUE]
                , m_tec.minsFactRequest(m_curDate.Date.Add(-m_tsOffsetToMoscow)
                    , hour - GetSeasonHourOffset(hour)
                    , m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_AISKUE, HDateTime.INTERVAL.MINUTES)
                    , m_idAISKUEParNumber
                )
            );
        }

        private void getMinTMRequest(DateTime date, int lh, int lm)
        {
            int interval = GetIntervalOfTypeSourceData(HDateTime.INTERVAL.MINUTES);

            if (interval > 0)
                Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.minTMRequest(m_curDate, lh - GetSeasonHourOffset(lh), lm, m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO, HDateTime.INTERVAL.MINUTES), interval));
            else ;
        }

        private void getMinDetailTMRequest(DateTime date, int lh, int lm)
        {
            int interval = 1; //GetIntervalOfTypeSourceData(HDateTime.INTERVAL.MINUTES);

            if (interval > 0)
                Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO]
                    , m_tec.minTMDetailRequest(m_curDate, lh - GetSeasonHourOffset(lh)
                    //, lm > 60 ? 60 : lm
                    , lm
                    , m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO, HDateTime.INTERVAL.MINUTES)
                    , interval));
            else ;
        }

        private void getMinTMGenRequest(DateTime date, int lh, int lm)
        {
            int interval = GetIntervalOfTypeSourceData(HDateTime.INTERVAL.MINUTES);

            if (interval > 0)
                Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.minTMAverageRequest(m_curDate, lh - GetSeasonHourOffset(lh), lm, m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO, HDateTime.INTERVAL.MINUTES), interval));
            else ;
        }

        public int GetIntervalOfTypeSourceData (HDateTime.INTERVAL id_time) {
            int iRes = -1;
            switch (m_arTypeSourceData[(int)id_time])
            {
                case CONN_SETT_TYPE.DATA_AISKUE:
                case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                    if (id_time == HDateTime.INTERVAL.MINUTES)
                        iRes = 3;
                    else
                        if (id_time == HDateTime.INTERVAL.HOURS)
                            switch (m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES])
                            {
                                case CONN_SETT_TYPE.DATA_AISKUE:
                                case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                                case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                                case CONN_SETT_TYPE.DATA_SOTIASSO: //...??? AdminAlarm???
                                    iRes = 3;
                                    break;
                                case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                                    iRes = 1;
                                    break;
                                default:
                                    break;
                            }
                        else
                            ;
                    break;
                case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                case CONN_SETT_TYPE.DATA_SOTIASSO: //...??? AdminAlarm???
                    iRes = 3;
                    break;
                case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                    iRes = 1;
                    break;
                default:
                    break;
            }

            return iRes;
        }
        
        private void getMinsTMRequest(int hour)
        {
            int interval = GetIntervalOfTypeSourceData(HDateTime.INTERVAL.MINUTES);

            if (interval > 0)
                Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.minsTMRequest(m_curDate, hour - GetSeasonHourOffset(hour), m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO, HDateTime.INTERVAL.MINUTES), interval));
            else
                Logging.Logg().Error(@"TecView::GetMinsTMRequest (hour=" + hour + @") - ?? ?????? ???????? ??? ???????...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        private void getHoursTMTemperatureRequest(DateTime dt)
        {
            //Debug.WriteLine(string.Format (@"TecView::getHoursTMTemperatureRequest () - ????, ????={0}...", dt.ToString()));

            TimeSpan tsOffset = HDateTime.TS_MSK_OFFSET_OF_UTCTIMEZONE + m_tsOffsetToMoscow;
            DateTime dtReq = dt.Date.Add(-tsOffset);

            string strQuery = @"SELECT AVG([VALUE]) as [VALUE], DATEPART(HH, DATEADD (HH, " + (int)tsOffset.TotalHours + @", [last_changed_at])) as [HOUR]"
                + @" FROM [techsite-2.X.X].[dbo].[ALL_PARAM_SOTIASSO_KKS]"
                + @" WHERE [KKS_NAME] = 'T2#temperature_V'"
                    + @" AND NOT [last_changed_at] < '" + dtReq.ToString(@"yyyyMMdd HH:00:00")
                        + @"' AND [last_changed_at] < '" + dtReq.AddDays(1).ToString(@"yyyyMMdd HH:00:00") + @"'"
                + @" GROUP BY DATEPART(HH, DATEADD (HH, " + (int)tsOffset.TotalHours + @", [last_changed_at]))"
                + @" ORDER BY [HOUR]";

            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], strQuery);
        }        

        private int getHoursTMTemperatureResponse(DataTable table)
        {
            int iRes = 0;

            try {
                foreach (DataRow r in table.Rows)
                    m_valuesHours[(int)r[@"HOUR"]].valuesLastMinutesTM = (double)r[@"VALUE"];
            }
            catch (Exception e) {
                iRes = -1;

                Logging.Logg().Exception(e, @"TecView::getHoursTMTemperatureResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            return iRes;
        }        

        private void getHoursTMSNPsumRequest()
        {
            if (m_curDate.Equals(DateTime.MinValue) == false)
                Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.hoursTMSNPsumRequest(m_curDate));
            else
                ;
        }

        private void getLastMinutesTMRequest()
        {
            DateTime dtReq = m_curDate.Date;
            int cnt = HAdmin.CountHoursOfDate (dtReq);

            Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.DATA_SOTIASSO], m_tec.lastMinutesTMRequest(dtReq.Date, m_tec.GetSensorsString(CurrentKey, CONN_SETT_TYPE.DATA_SOTIASSO), cnt));
        }

        protected virtual void getPPBRValuesRequest()
        {
            if (m_curDate.Equals(DateTime.MinValue) == false)
                Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.PBR], m_tec.GetPBRValueQuery(CurrentKey, m_curDate.Date.Add(-m_tsOffsetToMoscow), _type));
            else
                ;
        }

        protected virtual void getAdminValuesRequest()
        {
            if (m_curDate.Equals(DateTime.MinValue) == false)
                Request(m_dictIdListeners[m_tec.m_id][(int)CONN_SETT_TYPE.ADMIN], m_tec.GetAdminValueQuery(CurrentKey, m_curDate.Date.Add(-m_tsOffsetToMoscow), _type));
            else
                ;
        }

        protected override void InitializeSyncState(int capacity = 1)
        {
            base.InitializeSyncState ((int)INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON);

            AddSyncState (new INDEX_WAITHANDLE_REASON [] { INDEX_WAITHANDLE_REASON.ERROR, INDEX_WAITHANDLE_REASON.BREAK }
                , new Type [] { typeof(ManualResetEvent), typeof (ManualResetEvent) }
                , new bool [] { false, false });
        }
    }
}
