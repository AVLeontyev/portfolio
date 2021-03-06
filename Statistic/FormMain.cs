using System;
using System.Linq;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
//using System..SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Media;

using ASUTP;
using ASUTP.Core;
using ASUTP.Helper;
using ASUTP.Database;
using ASUTP.Forms;
using ASUTP.Control;

using StatisticCommon;
using StatisticDiagnostic;
using StatisticTimeSync;
using StatisticAlarm;
using StatisticAnalyzer;
using CommonAux;

namespace Statistic
{
    /// <summary>
    /// Открытый частичный класс FormMain наследуется от FormMainBaseWithStatusStrip
    /// </summary>
    public partial class FormMain : ASUTP.Forms.FormMainBaseWithStatusStrip
    {
        //10001 = ADMIN_KOM_DISP, 10002 = ADMIN_NSS (FormChangeMode)
        private enum ID_ADDING_TAB
        {
            CUR_POWER = 10101, TM_SN_POWER, MONITOR_LAST_MINUTES, SOBSTV_NYZHDY, CUSTOM_2X2_1, CUSTOM_2X3_1
            , DATETIMESYNC_SOURCE_DATA
            , CUSTOM_2X2_2, CUSTOM_2X3_2, CUSTOM_2X2_3, CUSTOM_2X3_3, CUSTOM_2X2_4, CUSTOM_2X3_4
            , SOTIASSO_HOUR, DIAGNOSTIC, ANALYZER, TEC_Component, USERS
            , VZLET_TDIRECT, SOBSTV_NYZHDY_NEW
            // KhryapinAN, 2017-06
            , AIISKUE_SOTIASSO_DAY
        };

        private enum INDEX_CUSTOM_TAB { TAB_2X2, TAB_2X3, TAB_MULTI };

        private class ADDING_TAB
        {
            public ToolStripMenuItem menuItem;
            public PanelStatistic panel;
            //public string m_TabText;
            public HStatisticTabCtrlEx.TYPE_TAB m_typeTab;

            public ADDING_TAB(string menuItemName, string menuItemText/*, string tabText*/, HStatisticTabCtrlEx.TYPE_TAB typeTab)
            {
                menuItem = new System.Windows.Forms.ToolStripMenuItem();
                menuItem.CheckOnClick = true;
                menuItem.Size = new System.Drawing.Size(280, 22);
                menuItem.Name = menuItemName;
                menuItem.Text = menuItemText;
                menuItem.Enabled = false;
                panel = null;

                m_typeTab = typeTab;
            }
        };
        /// <summary>
        /// Признак процесса авто/загрузки вкладок
        /// для предотвращения сохранения их в режиме "реальное время"
        /// </summary>
        private static bool m_bAutoActionTabs = false;

        private Dictionary<int, Form> m_dictFormFloat;
        private PanelStatistic[] m_arPanelAdmin;
        private PanelAdmin PanelKomDisp { get { return m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP] as PanelAdmin; } }
        private List<PanelTecViewBase> m_listStandardTabs;
        private Dictionary<ID_ADDING_TAB, ADDING_TAB> m_dictAddingTabs;
        /// <summary>
        /// Список идентификаторов сервисных вкладок, для выделнеия их из полного перечня "дополнительных"
        /// </summary>
        private IEnumerable<ID_ADDING_TAB> m_listIdServiceTab = new List<ID_ADDING_TAB> () {
            ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA
            , ID_ADDING_TAB.DIAGNOSTIC
            , ID_ADDING_TAB.ANALYZER
            , ID_ADDING_TAB.TEC_Component
            , ID_ADDING_TAB.USERS
        };
        private static List<ID_ADDING_TAB>[] m_arIdCustomTabs = new List<ID_ADDING_TAB>[] {
            new List<ID_ADDING_TAB> () { ID_ADDING_TAB.CUSTOM_2X2_1, ID_ADDING_TAB.CUSTOM_2X2_2, ID_ADDING_TAB.CUSTOM_2X2_3, ID_ADDING_TAB.CUSTOM_2X2_4 }
            , new List<ID_ADDING_TAB> () { ID_ADDING_TAB.CUSTOM_2X3_1, ID_ADDING_TAB.CUSTOM_2X3_2, ID_ADDING_TAB.CUSTOM_2X3_3, ID_ADDING_TAB.CUSTOM_2X3_4 }
            , new List<ID_ADDING_TAB> () { ID_ADDING_TAB.MONITOR_LAST_MINUTES, ID_ADDING_TAB.VZLET_TDIRECT }
        };

        #region KhryapinAN 28.09.2017 для реализации изменения цветовой гаммы
        private HMark m_markPrevStatePanelAdmin;

        private int m_prevSelectedIndex;

        public Passwords m_passwords;

        private enum INDEX_FORM { PASSWORD_INPUT, PASSWORD_CHANGED, MODE_CHANGED, GRAPH_SETTINGS, SETUP, BYISK_TG }

        private FormPassword formPassword;
        private FormSetPassword formSetPassword;
        private FormChangeMode formChangeMode;
        public static FormGraphicsSettings formGraphicsSettings;
        public static FormParameters formParameters;
        private FormParametersTG formParametersTG;

        private List<Form> _listFormUtility;
        #endregion

        private static int m_iGO_Version;

        //TcpServerAsync m_TCPServer;
        private
            //System.Threading.Timer
            System.Windows.Forms.Timer
                m_timerAppReset;

        //private class ListPanelTecViewBase : List<PanelTecViewBase>
        //{
        //    public int IndexOfID(int id)
        //    {
        //        int iRes = -1;

        //        foreach (PanelTecViewBase panel in this)
        //            if (panel.TecViewKey == id)
        //            {
        //                iRes = this.IndexOf(panel);

        //                break;
        //            }
        //            else
        //                ;

        //        return iRes;
        //    }
        //}

        private ProfessionalColorTable _darkColorTable;

        public FormMain()
        {
            InitializeComponent();

            ProgramBase.s_iMessageShowUnhandledException = 1;

            //??? как рез-т проверка на запуск нового экземпляра... см. 'Program.cs'
            //m_TCPServer = new TcpServerAsync(IPAddress.Any, 6666);
            //m_TCPServer.delegateRead = ReadAnalyzer;

            //??? как рез-т проверка на запуск нового экземпляра... см. 'Program.cs'
            //if (!(m_TCPServer.Start() == 0)) Abort(@"Запуск дублирующего экземпляра приложения", true, false); else ;
            //m_TCPServer.Stop();

            AdminTS.m_sOwner_PBR = 1; //Признак владельца ПБР - пользователь

            //DelegateGetINIParametersOfID = new StringDelegateIntFunc(GetINIParametersOfID);

            tclTecViews.EventHTabCtrlExClose += delegateOnCloseTab;
            tclTecViews.EventHTabCtrlExFloat += delegateOnFloatTab;

            this.m_dictAddingTabs[ID_ADDING_TAB.SOBSTV_NYZHDY_NEW].menuItem.Enabled = true;
        }

        private int Initialize(out string msgError)
        {
            HMark markQueries
                , markSett;
            List<int> listIdProfilesUnit; // для проверки доступа к специальным вкладкам
            List<int> listIDs; // идентификаторы 

            //StartWait ();
            delegateStartWait();

            msgError = string.Empty;
            //MessageBox.Show((IWin32Window)null, @"FormMain::Initialize () - вХод...", @"Отладка!");

            int iRes = 0;
            int i = -1;

            m_prevSelectedIndex = 1; //??? = -1
            m_markPrevStatePanelAdmin = new HMark(0);

            m_listStandardTabs = new List<PanelTecViewBase> ();

            try
            {
                //!!! Один экземпляр для всего приложения на весь срок выполнения
                new DbTSQLConfigDatabase (s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett ());
                DbTSQLConfigDatabase.DbConfig ().Register ();
                //formParameters = new FormParameters_FIleINI("setup.ini");
                formParameters = new FormParameters_DB();

                Logging.LinkId(Logging.INDEX_MESSAGE.D_002, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGPBRNUMBER);
                Logging.LinkId(Logging.INDEX_MESSAGE.D_003, (int)FormParameters.PARAMETR_SETUP.TECVIEW_LOGRECOMENDATIONVAL);
                Logging.LinkId(Logging.INDEX_MESSAGE.D_004, (int)FormParameters.PARAMETR_SETUP.PANELQUICKDATA_LOGDEVIATIONEVAL);
                Logging.LinkId(Logging.INDEX_MESSAGE.D_005, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGQUERY);
                Logging.LinkId(Logging.INDEX_MESSAGE.W_001, (int)FormParameters.PARAMETR_SETUP.TECVIEW_GETCURRENTTMGEN_LOGWARNING);
                Logging.LinkId(Logging.INDEX_MESSAGE.D_001, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_CONTROLHANDLE_LOGERRORCREATE);
                // KhryapinAN 2018-02-14
                Logging.LinkId (Logging.INDEX_MESSAGE.D_006, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETADMINQUERY_LOGQUERY);

                Logging.DelegateGetINIParametersOfID = new StringDelegateIntFunc(formParameters.GetINIParametersOfID);

                updateParametersSetup();

                //Предустановленные в файле/БД конфигурации
                HUsers.s_REGISTRATION_INI[(int)HUsers.INDEX_REGISTRATION.DOMAIN_NAME] = formParameters.GetINIParametersOfID ((int)FormParameters.PARAMETR_SETUP.USERS_DOMAIN_NAME); //string.Empty; //@"Отладчик";
                HUsers.s_REGISTRATION_INI[(int)HUsers.INDEX_REGISTRATION.ID] = 0; //Неизвестный пользователь
                HUsers.s_REGISTRATION_INI[(int)HUsers.INDEX_REGISTRATION.ID_TEC] = Int32.Parse(formParameters.GetINIParametersOfID (FormParameters.PARAMETR_SETUP.USERS_ID_TEC)); //5
                HUsers.s_REGISTRATION_INI[(int)HUsers.INDEX_REGISTRATION.ROLE] = Int32.Parse(formParameters.GetINIParametersOfID (FormParameters.PARAMETR_SETUP.USERS_ID_ROLE)); //2;
                m_iGO_Version = Convert.ToInt32(formParameters.GetINIParametersOfID (FormParameters.PARAMETR_SETUP.IGO_VERSION));
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"FormMain::Initialize () ... загрузка предустановленных параметров ...", Logging.INDEX_MESSAGE.NOT_SET);

                msgError = e.Message;
                iRes = -5;
            }

            if (iRes == 0)
            {
                try
                {
                    //Т.к. все используемые члены-данные СТАТИЧЕСКИЕ
                    using (new HStatisticUsers(DbTSQLConfigDatabase.DbConfig().ListenerId, formParameters.GetINIParametersOfID (FormParameters.PARAMETR_SETUP.MODE_REGISTRATION))) { ; }
                }
                catch (Exception e)
                {
                    if (e is HException)
                    {
                        iRes = ((HException)e).m_code; //-2, -3, -4
                    }
                    else
                    {
                        iRes = -1;
                    }

                    msgError = e.Message;
                }

                if (iRes == 0)
                {
                    s_iMainSourceData = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAIN_DATASOURCE]);

                    //Режим изменения цветовой гаммы
                    _darkColorTable = new CustomColorTable (HStatisticUsers.GetAllowed ((int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_COLOR_CHANGESHEMA_FONT)
                        , HStatisticUsers.GetAllowed ((int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_COLOR_CHANGESHEMA_BACKGROUND));
                    ForeColorChanged += FormMain_ForeColorChanged;
                    ForeColorChanged += tclTecViews.FormMain_ForeColorChanged;
                    BackColorChanged += FormMain_BackColorChanged;
                    BackColorChanged += tclTecViews.FormMain_BackColorChanged;

                    markSett = new HMark(Int32.Parse(HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.AUTO_LOADSAVE_USERPROFILE)));
                    файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Enabled = markSett.IsMarked(0);
                    файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked = markSett.IsMarked(1);

                    //Инструмент администратора - доступ по роли
                    администрированиеToolStripMenuItem.Enabled =
                    параметрыToolStripMenuItem.Enabled =
                        m_dictAddingTabs[ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].menuItem.Enabled =
                        m_dictAddingTabs[ID_ADDING_TAB.DIAGNOSTIC].menuItem.Enabled =
                        m_dictAddingTabs[ID_ADDING_TAB.ANALYZER].menuItem.Enabled =
                        m_dictAddingTabs[ID_ADDING_TAB.TEC_Component].menuItem.Enabled =
                        m_dictAddingTabs[ID_ADDING_TAB.USERS].menuItem.Enabled =
                            HStatisticUsers.RoleIsAdmin;
                    //Инструмент администратора - проверка дополнительных настроек [profiles]
                    m_dictAddingTabs[ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].menuItem.Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_SETTING_PARAMETERS_SYNC_DATETIME_DB);
                    m_dictAddingTabs[ID_ADDING_TAB.DIAGNOSTIC].menuItem.Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_SETTING_PARAMETERS_DIAGNOSTIC);
                    m_dictAddingTabs[ID_ADDING_TAB.TEC_Component].menuItem.Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_SETTING_ADMIN_TECCOMPONENT_CHANGE);

                    администрированиеToolStripMenuItem.Enabled |=
                        m_dictAddingTabs[ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].menuItem.Enabled
                        || m_dictAddingTabs[ID_ADDING_TAB.DIAGNOSTIC].menuItem.Enabled
                        || m_dictAddingTabs[ID_ADDING_TAB.TEC_Component].menuItem.Enabled;

                    #region KhryapinAN 02.10.2017 цвета должны быть доступны к моменту инициализации 'DataGridView'
                    m_passwords = new Passwords ();
                    formPassword = new FormPassword (m_passwords);
                    formSetPassword = new FormSetPassword (m_passwords);
                    formGraphicsSettings = new FormGraphicsSettings (delegateUpdateActiveGui
                        , delegateHideGraphicsSettings
                        , bool.Parse(HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_COLOR_SHEMA)));
                    #endregion

                    if (Logging.s_mode == Logging.LOG_MODE.DB)
                    {
                        //Инициализация БД-логирования
                        int err = -1;
                        Logging.UserId = HUsers.Id;
                        Logging.ConnSett = new ConnectionSettings(DbTSQLConfigDatabase.DbConfig ().GetDataTableConnSettingsOfIdSource (s_iMainSourceData, -1, out err).Rows[0], 0);
                    } else { }

                    m_formAlarmEvent = new MessageBoxAlarmEvent(this);
                    m_formAlarmEvent.EventActivateTabPage += new DelegateBoolFunc(activateTabPage);

                    //m_arAdmin = new AdminTS[(int)FormChangeMode.MANAGER.COUNT_MANAGER];
                    m_arPanelAdmin = new PanelStatistic[(int)FormChangeMode.MANAGER.COUNT_MANAGER];

                    m_bAutoActionTabs = файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked;
                    // определить признаки автоматического отображения административных вкладок
                    listIdProfilesUnit = new List<int> { (int)HStatisticUsers.ID_ALLOWED.AUTO_TAB_PBR_KOMDISP
                        , (int)HStatisticUsers.ID_ALLOWED.AUTO_TAB_PBR_NSS
                        , (int)HStatisticUsers.ID_ALLOWED.AUTO_TAB_ALARM
                        , (int)HStatisticUsers.ID_ALLOWED.AUTO_TAB_LK_ADMIN
                        , (int)HStatisticUsers.ID_ALLOWED.AUTO_TAB_TEPLOSET_ADMIN };
                    listIDs = new List<int>();

                    for (i = 0; i < FormChangeMode.ID_ADMIN_TABS.Length; i++)
                        if (HStatisticUsers.IsAllowed(listIdProfilesUnit[i]) == true)
                            listIDs.Add(FormChangeMode.ID_ADMIN_TABS[i]);
                        else ;

                    //Добавить закладки автоматически...
                    //listIDs.Add(5); listIDs.Add(111);
                    if (m_bAutoActionTabs == true)
                    {
                        string[] ids = HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE).Split(';');
                        if ((ids.Length > 0) && (ids[0].Equals(string.Empty) == false))
                            foreach (string id in ids)
                                if (listIDs.IndexOf(Int32.Parse(id)) < 0)
                                    listIDs.Add(Int32.Parse(id));
                                else
                                    ;
                        else
                            ;
                    }
                    else
                        ;

                    if (!(formChangeMode == null))
                    {
                        formChangeMode.Dispose();
                        formChangeMode = null;
                    }
                    else
                        ;

                    formChangeMode = new FormChangeMode(DbTSQLConfigDatabase.DbConfig().InitTEC(FormChangeMode.MODE_TECCOMPONENT.ANY
                            , false
                            , new int [] { 0, (int)TECComponent.ID.PC }
                            , false)
                        , listIDs
                        , this.ContextMenuStrip);
                    formChangeMode.ev_сменитьРежим += сменитьРежимToolStripMenuItem_Click;
                    if (сменитьРежимToolStripMenuItem.Enabled == false) сменитьРежимToolStripMenuItem.Enabled = true; else ;

                    параметрыПриложенияToolStripMenuItem.Enabled = HStatisticUsers.RoleIsAdmin == true;
                    
                    this.Text = "Статистика - " + formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAIN_PRIORITY];

                    TecView.SEC_VALIDATE_TMVALUE = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.VALIDATE_TM_VALUE]);

                    if (iRes == 0)
                    {
                        Start(); //Старт 1-сек-го таймера для строки стостояния

                        stopTimerAppReset();
                        //int msecTimerAppReset = Int32.Parse (formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION_QUERY_INTERVAL]);
                        m_timerAppReset =
                            //new System.Threading.Timer(new TimerCallback(fTimerAppReset), null, msecTimerAppReset, msecTimerAppReset)
                            new System.Windows.Forms.Timer();
                            ;
                        m_timerAppReset.Interval = ProgramBase.TIMER_START_INTERVAL;
                        m_timerAppReset.Tick += new EventHandler(fTimerAppReset);
                        m_timerAppReset.Start();

                        //TODO: почему не вызвать 'fileProfileLoadStandardTab'
                        // , или еще проще выбрать п.меню  'файлПрофильЗагрузитьToolStripMenuItem.PerformClick()'
                        // очень непрозрачное открытие вкладок по профилю пользователя: почему идентификаторы собираются выше по коду?
                        //!!! трудность в возможном переходе от одной БД конфигурации к другой
                        // , поэтому идентификаторы собираются до закрытия соединения со предыдущей БД
                        string strIDsToLog = string.Empty;
                        listIDs.ForEach(id => strIDsToLog += id.ToString() + ';');
                        Logging.Logg().Action(@"АвтоЗагрузка профайла (" + HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE.ToString() + @"): ids=" + strIDsToLog, Logging.INDEX_MESSAGE.NOT_SET);
                        //С пустой строкой имитация нажатия "Ок"...
                        formChangeMode.LoadProfile(string.Empty);
                        //!!! только после стандартных вкладок
                        if (m_bAutoActionTabs == true)
                            fileProfileLoadAddingTab ();
                        else
                            ;

                        m_bAutoActionTabs = false;
                    }
                    else
                        ;
                }
                else
                    ;
            }
            else
            {
            }

            DbTSQLConfigDatabase.DbConfig ().UnRegister();

            delegateStopWait();

            return iRes;
        }

        private void stopTimerAppReset()
        {
            if (!(m_timerAppReset == null)) {
                //m_timerAppReset.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                m_timerAppReset.Stop ();
                m_timerAppReset.Dispose ();
                m_timerAppReset = null;
            } else
                ;
        }

        private void appReset()
        {
            IAsyncResult iar;
            DialogResult dlgRes;

            new Thread (new ParameterizedThreadStart (delegate (object obj) {
                iar = BeginInvoke (new Func<DialogResult> (() => {
                    stopTimerAppReset ();
                    activateTabPage (tclTecViews.SelectedIndex, false);

                    return MessageBox.Show (this, $"Доступно обновление для приложения <{((Version)obj).ToString ()}>...{Environment.NewLine}" +
                        @"Для применения обновления" + Environment.NewLine +
                        //@"будет произведен останов и повторный запуск на выполнение...",
                        @"требуется произвести останов и повторный запуск на выполнение...",
                        @"Обновление!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Stop);
                }));

                dlgRes = (DialogResult)EndInvoke (iar);

                BeginInvoke ((MethodInvoker) delegate () {
                    //ProgramBase.AppRestart();
                    ProgramBase.AppExit ();
                });
                
            })).Start(new Version(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION].Trim()));
        }

        private void updateParametersSetup()
        {
            //Параметры записи сообщений лог-а...
            Logging.UpdateMarkDebugLog();

            //Параметры обновления "основной панели"...
            PanelStatistic.POOL_TIME = Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]);
            PanelStatistic.ERROR_DELAY = Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ERROR_DELAY]);

            //Параметры перехода на сезонное времяисчисление...
            HAdmin.SeasonDateTime = DateTime.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.SEASON_DATETIME]);
            HAdmin.SeasonAction = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.SEASON_ACTION]);

            //Параметры обработки запросов к БД...
            Constants.MAX_RETRY = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAX_ATTEMPT]);
            Constants.MAX_WAIT_COUNT = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.WAITING_COUNT]);
            Constants.WAIT_TIME_MS = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.WAITING_TIME]);

            //Параметры валидности даты/времени получения данных СОТИАССО...
            TecView.SEC_VALIDATE_TMVALUE = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.VALIDATE_TM_VALUE]);

            PanelStatisticDiagnostic.UPDATE_TIME = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.DIAGNOSTIC_TIMER_UPDATE]);
            PanelStatisticDiagnostic.VALIDATE_ASKUE_TM = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.VALIDATE_ASKUE_VALUE]);

            //Параметрвы для ALARM...
            StatisticAlarm.AdminAlarm.MSEC_ALARM_TIMERUPDATE = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_TIMER_UPDATE]) * 1000;
            StatisticAlarm.AdminAlarm.MSEC_ALARM_EVENTRETRY = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_EVENT_RETRY]) * 1000;
            StatisticAlarm.AdminAlarm.MSEC_ALARM_TIMERBEEP = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_TIMER_BEEP]) * 1000;
            StatisticAlarm.AdminAlarm.FNAME_ALARM_SYSTEMMEDIA_TIMERBEEP = formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_SYSTEMMEDIA_TIMERBEEP];
        }

        //private void fTimerAppReset(object obj)
        private void fTimerAppReset(object obj, EventArgs ev)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo;

            try
            {
                if (m_timerAppReset.Interval == ProgramBase.TIMER_START_INTERVAL)
                {
                    //При 1-ом запуске ожидать один интервал
                    m_timerAppReset.Interval = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION_QUERY_INTERVAL]);
                    return;
                }
                else
                    ;

                int err = -1;

                DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
                DbTSQLConfigDatabase.DbConfig().Register();

                // прочитать актуальные значения из [setup]
                (formParameters as FormParameters_DB).Update(DbTSQLConfigDatabase.DbConfig ().ListenerId, out err);
                // прочитать и обновить актуальные индивидуальные групповые (пользовательские) параметры
                HStatisticUsers.Update (DbTSQLConfigDatabase.DbConfig ().ListenerId);
                DbTSQLConfigDatabase.PerformTECListUpdate(DbTSQLConfigDatabase.DbConfig ().ListenerId);

                DbTSQLConfigDatabase.DbConfig ().UnRegister();

                if (err == 0)
                {
                    //Динамическое обновление - применение актуальных параметров
                    updateParametersSetup();

                    if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.APP_AUTO_RESET) == true)
                        if (((formParameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.APP_VERSION].Equals (string.Empty) == false)
                                && (formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION].Equals(Application.ProductVersion/*StatisticCommon.Properties.Resources.TradeMarkVersion*/) == false))
                            || ((formParameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.IGO_VERSION].Trim ().Equals (string.Empty) == false)
                                && (formParameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.IGO_VERSION].Trim ().Equals (Convert.ToString (m_iGO_Version)) == false)))
                        {
                            if (IsHandleCreated/**/ == true)
                                if (InvokeRequired == true)
                                {
                                    /*IAsyncResult iar = */
                                    this.BeginInvoke(new DelegateFunc(appReset));
                                    //this.EndInvoke (iar);
                                }
                                else
                                    appReset();
                            else
                                ;

                            //ProgramBase.AppRestart();

                        }
                        else
                            ;
                    else
                    //При ошибке - восстанавливаем значение...
                        ;
                }
                else
                    ; //DbSources.Sources().UnRegister(idListenerConfigDB);
            }
            catch (Exception e) {
                Logging.Logg().Exception(e, @"FormMain::fTimerAppReset () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        #region Код для отображения сообщения о событии сигнализации

        private int m_iAlarmEventCounter;
        private int AlarmEventCounter { get { return m_iAlarmEventCounter; } set { m_iAlarmEventCounter = value; } }
        SoundPlayer m_sndAlarmEvent;
        private
            //System.Threading.Timer
            System.Windows.Forms.Timer
                m_timerAlarmEvent;

        //private void timerAlarmEvent (object obj)
        private void timerAlarmEvent (object obj, EventArgs ev)
        {
            //System.Media.SystemSounds.Question.Play();
            if (m_sndAlarmEvent == null)
                Console.Beep ();
            else
                m_sndAlarmEvent.Play ();
        }

        MessageBoxAlarmEvent m_formAlarmEvent;

        private void activateTabPage (bool active)
        {
            activateTabPage (tclTecViews.SelectedIndex, active);
        }
        
        private void OnPanelAlarmEventGUIReg(AlarmNotifyEventArgs ev)
        {
            try
            {
                //panelAdminKomDispEventGUIReg(text);
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new AlarmNotifyEventHandler(m_formAlarmEvent.MessageBoxShow), ev);
                else
                    Logging.Logg().Error(@"FormMain::OnPanelAlarmEventGUIReg () - ... BeginInvoke (m_formAlarmEvent.Show) - ...", Logging.INDEX_MESSAGE.D_001);
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"FormMain::OnPanelAlarmEventGUIReg (string) - text=" + ev.m_message_shr, Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        #endregion

        private struct ACTION_CLOSING_TAB
        {
            public Type TypePanel;

            public int IdAction;
        }

        void delegateOnCloseTab(object sender, ASUTP.Control.HTabCtrlExEventArgs e)
        {
            Type typePanel = Type.Missing.GetType();
            List<ACTION_CLOSING_TAB> listActionClosingTab;
            ACTION_CLOSING_TAB actionClosingTab;

            if ((e.TabIndex < tclTecViews.TabPages.Count)
                && tclTecViews.TabPages [e.TabIndex].Controls.Count > 0) {
                listActionClosingTab = new List<ACTION_CLOSING_TAB> () {
                    new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelTecViewStandard), IdAction = 0 }
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelLKView), IdAction = 0 }
                    //!!! Внимание - вкладки пользователей с расширенными правами
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelAdminKomDisp), IdAction = -1 } //  * ((int)FormChangeMode.MANAGER.DISP + 1)
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelAdminNSS), IdAction = -2 } //  * ((int)FormChangeMode.MANAGER.NSS + 1)
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelAlarm), IdAction = -3 } //  * ((int)FormChangeMode.MANAGER.ALARM + 1)
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelAdminLK), IdAction = -4 } //  * ((int)FormChangeMode.MANAGER.LK + 1)
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelAdminVyvod), IdAction = -5 } //  * ((int)FormChangeMode.MANAGER.VYVOD + 1)
                    //!!! Внимание - дополнительные вкладки
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelStatisticDiagnostic), IdAction = (int)ID_ADDING_TAB.DIAGNOSTIC }
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelCurPower), IdAction = (int)ID_ADDING_TAB.CUR_POWER }
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelTMSNPower), IdAction = (int)ID_ADDING_TAB.TM_SN_POWER }
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelLastMinutes), IdAction = (int)ID_ADDING_TAB.MONITOR_LAST_MINUTES }
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelSobstvNyzhdy), IdAction = (int)ID_ADDING_TAB.SOBSTV_NYZHDY }
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelCommonAux), IdAction = (int)ID_ADDING_TAB.SOBSTV_NYZHDY_NEW }
                    //!!! Внимание - идентификатор из события (??? остальные по аналогии)
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelCustomTecView), IdAction = e.Id }
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelSOTIASSOHour), IdAction = (int)ID_ADDING_TAB.SOTIASSO_HOUR }
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelVzletTDirect), IdAction = (int)ID_ADDING_TAB.VZLET_TDIRECT }
                    // KhryapinAN, 2017-06
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelAISKUESOTIASSODay), IdAction = (int)ID_ADDING_TAB.AIISKUE_SOTIASSO_DAY }
                    //!!! Внимание - вкладки администраторов (??? ничем не отличаются)
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelSourceData), IdAction = (int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA }
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelAnalyzer), IdAction = (int)ID_ADDING_TAB.ANALYZER }
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelTECComponent), IdAction = (int)ID_ADDING_TAB.TEC_Component }
                    , new ACTION_CLOSING_TAB() { TypePanel = typeof(PanelUser), IdAction = (int)ID_ADDING_TAB.USERS }
                    ,
                };

                typePanel = tclTecViews.TabPages [e.TabIndex].Controls [0].GetType ();

                actionClosingTab = (from action in listActionClosingTab
                    where (action.TypePanel.IsAssignableFrom (typePanel) == true)
                        || (action.TypePanel.Equals(typePanel) == true)
                    select action).FirstOrDefault();

                if (Equals(actionClosingTab, null) == false)
                    if (actionClosingTab.IdAction < 0) {
                        formChangeMode.SetItemChecked (actionClosingTab.IdAction, false);
                    } else if (actionClosingTab.IdAction == 0) {
                        formChangeMode.SetItemChecked (e.TabHeaderText, false);
                    } else if (actionClosingTab.IdAction > 0) {
                        if ((Enum.IsDefined(typeof(ID_ADDING_TAB), actionClosingTab.IdAction) == true)
                            && (m_dictAddingTabs.ContainsKey ((ID_ADDING_TAB)actionClosingTab.IdAction) == true))
                            m_dictAddingTabs [(ID_ADDING_TAB)actionClosingTab.IdAction].menuItem.Checked = false;
                        else
                        //??? throw
                            ;
                    }
                else
                    Logging.Logg().Error($@"FormMain::delegateOnCloseTab () - неизвестный тип панели на вкладке; индекс={e.TabIndex}, текст={e.TabHeaderText}, тип={e.TabType.ToString ()}...", Logging.INDEX_MESSAGE.NOT_SET);
            } else
                throw new Exception ($@"FormMain::delegateOnCloseTab () - нет панели на вкладке; индекс={e.TabIndex}, текст={e.TabHeaderText}, тип={e.TabType.ToString()}...");
        }

        void delegateOnFloatTab(object sender, ASUTP.Control.HTabCtrlExEventArgs e)
        {
            //Проверить создан ли ранее словарь...
            if (m_dictFormFloat == null)
                //Создать массив (размерность из массива с идентификаторами вкладок)...
                m_dictFormFloat = new Dictionary<int, Form>(); //new Form [m_arIdCustomTabs.GetLength(0), m_arIdCustomTabs.GetLength(1)];
            else
                ;

            this.BeginInvoke(new DelegateObjectFunc(showFormFloat), e);
        }

        private int getKeyFormFloat(Form f)
        {
            int iRes = -1;

            if (m_dictFormFloat.ContainsValue(f) == true)
                foreach (KeyValuePair<int, Form> pair in m_dictFormFloat)
                    if (pair.Value.Equals(f) == true)
                    {
                        iRes = pair.Key;

                        break;
                    }
                    else
                        ;
            else
                ;

            if (iRes < 0)
                throw new Exception(@"FormMain::getKeyFormFloat () - не найден ключ в словаре для формы...");
            else
                ;

            return iRes;
        }

        private void showFormFloat(object obj)
        {
            ASUTP.Control.HTabCtrlExEventArgs ev = obj as ASUTP.Control.HTabCtrlExEventArgs;

            ////Тест...
            //Form formFloat = new FormAbout();
            //formFloat.Text = text;
            //formFloat.FormClosing += new FormClosingEventHandler(FormMain_OnFormFloat_Closing);
            //formFloat.Load += new EventHandler(FormMain_OnFormFloat_Load);
            //formFloat.Show();

            ////Деактивировать, остановить "панель"
            //m_dictAddingTabs[(int)m_arIdCustomTabs[(int)indxTab, indxItem]].panel.Activate(false);
            //m_dictAddingTabs[(int)m_arIdCustomTabs[(int)indxTab, indxItem]].panel.Stop();
            //Получить панель "открепляемой" вкладки
            Panel panel = tclTecViews.TabPages[ev.TabIndex].Controls[0] as Panel;
            //Удалить вкладку с "главного" окна
            tclTecViews.RemoveTabPage(ev.TabIndex);
            //Создать вспомогательное окно...
            FormMainFloat formFloat = null;
            //formFloat = new FormMainFloat(m_dictAddingTabs[(int)m_arIdCustomTabs[(int)indxTab, indxItem]].panel);
            formFloat = new FormMainFloat(ev.TabHeaderText, panel, panel is PanelCustomTecView == false);
            formFloat.BackColor = formGraphicsSettings.BackgroundColor;
            //Назначить обработчики событий...
            formFloat.delegateFormClosing = FormMain_OnFormFloat_Closing;
            formFloat.delegateFormLoad = FormMain_OnFormFloat_Load;
            ////Сохранить значение в массиве "вспомогательных" форм
            ////m_dictFormFloat.Add((int)m_arIdCustomTabs[(int)indxTab, indxItem], formFloat);
            //int key = -1;
            //////Вариант №1
            ////key = getKeyOfPanel(panel);
            ////Вариант №2
            //if (panel is PanelCustomTecView)
            //{
            //    //Определить индексы в массиве
            //    string text = string.Empty;
            //    ////Вариант №1
            //    //text = tclTecViews.NameOfItemControl(panel);
            //    //Вариант №2
            //    text = ev.TabHeaderText;
            //    INDEX_CUSTOM_TAB indxTab = getIndexCustomTab(text);
            //    int indxItem = getIndexItemCustomTab(text);
            //    key = (int)m_arIdCustomTabs[(int)indxTab][indxItem];
            //}
            //else
            //{
            //    //if (panel is PanelTecViewBase)
            //    //    key = (panel as PanelTecViewBase).m_ID;
            //    //else
            //    //{
            //    //    throw new Exception(@"FormMain::showFormFloat () - невозможно определить идентификатор панели - неизвестный тип панели...");
            //    //}

            //    key = ev.Id;
            //}
            m_dictFormFloat.Add(ev.Id, formFloat);
            //Отобразить окно, установить на нем фокус...
            formFloat.Show(null);
            formFloat.Focus();
        }

        private void FormMain_OnFormFloat_Load(object pars)
        {
            //Параметры
            FormMainFloat formFloat = ((object[])pars)[0] as FormMainFloat;
            EventArgs ev = ((object[])pars)[1] as EventArgs;
            DelegateStringFunc[] arFuncRep = ((object[])pars)[2] as DelegateStringFunc[];
            DelegateBoolFunc fRepClr = ((object[])pars)[3] as DelegateBoolFunc;

            ////Определить индексы в массиве
            //INDEX_CUSTOM_TAB indxTab = getIndexCustomTab(formFloat.Text);
            //int indxItem = getIndexItemCustomTab(formFloat.Text);
            //Назначить новые делегаты для заполнения строки статуса...
            Panel panel = formFloat.GetPanel();
            if (panel is PanelTecView)
                ((PanelTecView)panel).m_tecView.SetDelegateReport(arFuncRep[0], arFuncRep[1], arFuncRep[2], fRepClr);
            else
                if (panel is PanelStatistic)
                    ((PanelStatistic)panel).SetDelegateReport(arFuncRep[0], arFuncRep[1], arFuncRep[2], fRepClr);
                else
                    throw new Exception(@"FormMain::FormMain_OnFormFloat_Load () - невозможно назначить делегаты обновления строки статуса...");
            //"Стартовать", активировать "панель"...
            //m_dictAddingTabs[(int)m_arIdCustomTabs[(int)indxTab, indxItem]].panel.Start();
            //m_dictAddingTabs[(int)m_arIdCustomTabs[(int)indxTab, indxItem]].panel.Activate(true);
            ((PanelStatistic)panel).Activate(true);
        }

        private void FormMain_OnFormFloat_Closing(object pars)
        {
            //Параметры
            FormMainFloat formFloat = ((object[])pars)[0] as FormMainFloat;
            Panel panel = formFloat.GetPanel();
            FormClosingEventArgs ev = ((object[])pars)[1] as FormClosingEventArgs;
            ////Определить индексы в массиве
            //INDEX_CUSTOM_TAB indxTab = getIndexCustomTab(formFloat.Text);
            //int indxItem = getIndexItemCustomTab(formFloat.Text);
            int keyTab = getKeyFormFloat(formFloat); //(int)m_arIdCustomTabs[(int)indxTab, indxItem];

            //Проверить авто/закрытие
            if (m_bAutoActionTabs == false)
            {
                //Восстановить старые делегаты для заполнения строки статуса...
                if (panel is PanelTecView)
                    ((PanelTecView)panel).m_tecView.SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
                else
                    if (panel is PanelStatistic)
                        ((PanelStatistic)panel).SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
                    else
                        throw new Exception(@"FormMain::FormMain_OnFormFloat_Closing () - невозможно определить тип панели...");
                //Добавить вкладку в "основное" окно
                tclTecViews.AddTabPage(formFloat.GetPanel(), formFloat.Text, keyTab, HStatisticTabCtrlEx.TYPE_TAB.FLOAT);

                //???Отладка
                Console.WriteLine(@"FormMain::FormMain_OnFormFloat_Closing () - TabCount=" + tclTecViews.TabCount + @", SelectedIndex=" + tclTecViews.SelectedIndex);
                //Проверить кол-во вкладок
                if (tclTecViews.TabCount > 1)
                    activateTabPage(tclTecViews.TabCount - 1, false);
                else
                    ;

                this.Focus();
            }
            else
                ;

            m_dictFormFloat[keyTab] = null;
            m_dictFormFloat.Remove(keyTab);
        }

        private void файлПрофильАвтоЗагрузитьСохранить_CheckedChanged(object sender, EventArgs e)
        {
            файлПрофильЗагрузитьToolStripMenuItem.Enabled =
            файлПрофильСохранитьToolStripMenuItem.Enabled =
                !файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked;

            if ((!(m_timer == null))
                //&& (! (m_timer.Interval == ProgramBase.TIMER_START_INTERVAL))
                )
            {
                HMark markSett = new HMark(0);
                markSett.Set(0, файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Enabled);
                markSett.Set(1, файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked);

                HStatisticUsers.SetAllowed(s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett ()
                    , HStatisticUsers.ID_ALLOWED.AUTO_LOADSAVE_USERPROFILE
                    , markSett.Value.ToString());
            }
            else
                ; //Загрузка формы...
        }

        private void файлПрофильЗагрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_bAutoActionTabs = true;

            fileProfileLoadStandardTab();

            fileProfileLoadAddingTab();

            m_bAutoActionTabs = false;
        }

        private void fileProfileLoadStandardTab()
        {
            string ids = HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE);
            Logging.Logg().Action(@"Загрузка профайла (" + HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE.ToString() + @"): ids=" + ids
                , Logging.INDEX_MESSAGE.NOT_SET);
            formChangeMode.LoadProfile(ids);
        }

        private void fileProfileLoadAddingTab()
        {
            string ids = string.Empty;
            int id = -1;
            string [] arProfie;

            ids = HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS);

            Logging.Logg().Action(@"Загрузка профайла (" + HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS.ToString() + @"): ids=" + ids
                , Logging.INDEX_MESSAGE.NOT_SET);

            if (ids.Equals(string.Empty) == false)
            {
                arProfie = ids.Split(';');
                foreach (string profile in arProfie)
                {
                    id = -1;
                    if (profile.IndexOf('=') < 0)
                        if (Int32.TryParse(profile, out id) == false)
                            id = (int)Enum.Parse (typeof (ID_ADDING_TAB), profile);
                        else
                            ;
                    else
                        if (Int32.TryParse(profile.Substring(0, profile.IndexOf('=')), out id) == false)
                            id = (int)Enum.Parse(typeof(ID_ADDING_TAB), profile.Substring (0, profile.IndexOf ('=')));
                        else
                            ;

                    if ((Enum.IsDefined (typeof (ID_ADDING_TAB), id) == true)
                        && (m_dictAddingTabs.ContainsKey ((ID_ADDING_TAB)id) == true)) {
                        // создание панели
                        m_dictAddingTabs[(ID_ADDING_TAB)id].menuItem.PerformClick();
                        // для сложных вкладок сформировать внешний вид
                        switch (id) {
                            case (int)ID_ADDING_TAB.CUSTOM_2X2_1:
                            case (int)ID_ADDING_TAB.CUSTOM_2X2_2:
                            case (int)ID_ADDING_TAB.CUSTOM_2X2_3:
                            case (int)ID_ADDING_TAB.CUSTOM_2X2_4:
                            case (int)ID_ADDING_TAB.CUSTOM_2X3_1:
                            case (int)ID_ADDING_TAB.CUSTOM_2X3_2:
                            case (int)ID_ADDING_TAB.CUSTOM_2X3_3:
                            case (int)ID_ADDING_TAB.CUSTOM_2X3_4:
                                ((PanelCustomTecView)m_dictAddingTabs[(ID_ADDING_TAB)id].panel).LoadProfile(profile.Substring(profile.IndexOf('=') + 1));
                                break;
                            default: //CUR_POWER, TM_SN_POWER...
                                break;
                        }
                    } else
                        Logging.Logg().Error(@"FormMain::fileProfileLoadAddingTab () - m_dictAddingTabs не содержит ключ=" + id
                            , Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            else
                ;
        }

        private void fileProfileSaveStandardTab()
        {
            DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
            DbTSQLConfigDatabase.DbConfig().Register();
            fileProfileSaveStandardTab(DbTSQLConfigDatabase.DbConfig ().ListenerId);
            DbTSQLConfigDatabase.DbConfig ().UnRegister();
        }

        private void fileProfileSaveStandardTab(int idListener)
        {
            //Сохранить список основных вкладок...
            HStatisticUsers.SetAllowed(idListener, (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE, formChangeMode.SaveProfile());
        }

        private void fileProfileSaveAddingTab()
        {
            DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
            DbTSQLConfigDatabase.DbConfig().Register();
            fileProfileSaveAddingTab(DbTSQLConfigDatabase.DbConfig ().ListenerId);
            DbTSQLConfigDatabase.DbConfig ().UnRegister();
        }

        private void fileProfileSaveAddingTab(int idListener)
        {
            string ids = string.Empty
                , recTab = string.Empty;

            foreach (ID_ADDING_TAB key in m_dictAddingTabs.Keys)
                if (m_dictAddingTabs[key].menuItem.Checked == true)
                {
                    recTab = string.Empty;
                    switch (key)
                    {
                        case ID_ADDING_TAB.CUSTOM_2X2_1:
                        case ID_ADDING_TAB.CUSTOM_2X2_2:
                        case ID_ADDING_TAB.CUSTOM_2X2_3:
                        case ID_ADDING_TAB.CUSTOM_2X2_4:
                        case ID_ADDING_TAB.CUSTOM_2X3_1:
                        case ID_ADDING_TAB.CUSTOM_2X3_2:
                        case ID_ADDING_TAB.CUSTOM_2X3_3:
                        case ID_ADDING_TAB.CUSTOM_2X3_4:
                            recTab = ((int)key).ToString() + @"=" + ((PanelCustomTecView)m_dictAddingTabs[key].panel).SaveProfile();
                            break;
                        default: //CUR_POWER, TM_SN_POWER...
                            recTab = ((int)key).ToString();
                            break;
                    }
                    ids += recTab + @";";
                }
                else
                    ;

            if (ids.Length > 0)
                ids = ids.Substring(0, ids.Length - 1);
            else
                ;
            HStatisticUsers.SetAllowed(idListener, (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS, ids);
        }

        private void файлПрофильСохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DbTSQLConfigDatabase.DbConfig ().Register();

            //Сохранить список основных вкладок...
            fileProfileSaveStandardTab(DbTSQLConfigDatabase.DbConfig ().ListenerId);

            //Сохранить список "дополнительных" вкладок...
            fileProfileSaveAddingTab(DbTSQLConfigDatabase.DbConfig ().ListenerId);

            DbTSQLConfigDatabase.DbConfig ().UnRegister();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void stop(object o)
        {
            Stop(o as FormClosingEventArgs);
        }

        private void Stop(FormClosingEventArgs ev)
        {
            List<FormChangeMode.MANAGER> indexes = new List<FormChangeMode.MANAGER> { FormChangeMode.MANAGER.DISP
                , FormChangeMode.MANAGER.NSS
                , FormChangeMode.MANAGER.ALARM
                , FormChangeMode.MANAGER.LK
                , FormChangeMode.MANAGER.TEPLOSET
            };

            m_bAutoActionTabs = true;

            if ((Equals (formChangeMode) == false)
                && Equals(m_arPanelAdmin, null) == false)
                foreach (int indx in indexes) {
                    if (Equals (m_arPanelAdmin [indx], null) == false) {
                        if (
                            (m_markPrevStatePanelAdmin.IsMarked ((int)indx) == true)
                            //(formChangeMode.m_markTabAdminChecked.IsMarked ((int)indx) == true)
                            // вкладку PanelAlarm обязательно останавливаем, т.к. даже будучи снятой с отображения, выполняется в фоновом режиме
                            || ((FormChangeMode.MANAGER)indx == FormChangeMode.MANAGER.ALARM))
                            if (!(m_arPanelAdmin [indx].MayToClose () < 0)) {
                                m_arPanelAdmin [indx].Stop ();

                                //!!! из-за автоматического старта - особенное разрушение
                                if ((FormChangeMode.MANAGER)indx == FormChangeMode.MANAGER.ALARM)
                                    m_arPanelAdmin [indx].Dispose ();
                                else
                                    ;
                            } else {
                                if (!(ev == null)) {
                                    ev.Cancel = true;
                                    break;
                                } else
                                    ;
                            }

                        m_arPanelAdmin [indx] = null;
                    } else
                        ;
                }
            else
                ;

            Stop();

            //??? Закрывыаются все вкладки
            // , но 15 строк выше "админ"-ские закрываются СНОВА
            StopTabPages();

            //if (! (m_TCPServer == null)) {
            //    try {
            //        m_TCPServer.Stop ();
            //        m_TCPServer = null;
            //    } catch (Exception e) {
            //        Logging.Logg().Exception(e, @"FormMain::Stop (FormClosingEventArgs...) - m_TCPServer.Stop () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            //    }
            //} else
            //    ;

            stopTimerAppReset();
        }

        private void StopTabPages()
        {
            if (!(m_listStandardTabs == null))
                clearTabPages(new List<FormChangeMode.KeyDevice> (), false);
            else
                ;
        }

        /// <summary>
        /// Закрыть (очистить) все вкладки (стандартные + административные)
        /// </summary>
        /// <param name="bAfterRunning">Признак присоединения обработчика события по изменению выбранной вкладки</param>
        private void clearTabPages(List<FormChangeMode.KeyDevice>listKeyTabKeep, bool bAfterRunning)
        {
            //Logging.Logg().Debug(@"FormMain::clearTabPages () - вХод...", Logging.INDEX_MESSAGE.NOT_SET);

            FormChangeMode.MANAGER indxManager = FormChangeMode.MANAGER.UNKNOWN;
            //!!! в строгом соответствии 'FormChangeMode.MANAGER'
            List<Type> listPanelAdminTypes = new List<Type> () {
                typeof(PanelAdminKomDisp)
                , typeof(PanelAdminNSS)
                , typeof(PanelAlarm)
                , typeof(PanelAdminLK)
                , typeof(PanelAdminVyvod)
            };

            activateTabPage(tclTecViews.SelectedIndex, false);

            int i = -1;
            TabPage tab;
            int idAddingTab = -1;
            bool bStopped = false // для стандартных вкладок
                , bPerformClick = false; // закрывать дополнительные/сервисные вкладки имитацией выбора п.меню
            List<int> listToRemove = new List<int>();
            List<ID_ADDING_TAB> listToPerformClick = new List<ID_ADDING_TAB> ();

            listToRemove.Clear();
            if (!(m_dictFormFloat == null))
                foreach (KeyValuePair<int, Form> pair in m_dictFormFloat)
                    if (
                        //(pair.Key < (int)TECComponent.ID.MAX) && // закрывать "плавающие" окна только со стандартными объектами отображения
                        (listKeyTabKeep.Any (key => key.Id == pair.Key) == true) // удерживать полученные в аргументе
                    )
                        listToRemove.Add(pair.Key);
                    else
                        //if (Enum.IsDefined(typeof(ID_ADDING_TAB), pair.Key) == true)
                        //    listToPerformClick.Add((ID_ADDING_TAB)pair.Key);
                        //else
                        ////??? throw
                            ;
            else
                ;

            // возвратить плавающие окна в главное окно
            for (i = listToRemove.Count - 1; !(i < 0); i--)
                if (m_dictFormFloat.ContainsKey (listToRemove [i]) == true)
                    m_dictFormFloat [listToRemove [i]].Close ();
                else
                    ;

            listToRemove.Clear();
            for (i = 0; i < tclTecViews.TabCount; i ++)
            {
                idAddingTab = -1;
                tab = tclTecViews.TabPages [i];
                bStopped = false;
                bPerformClick = false;

                try {
                    if ((tab.IsDisposed == false)
                        && (tab.Controls.Count > 0)) {
                        if (typeof (PanelTecViewBase).IsAssignableFrom (tab.Controls [0].GetType ()) == true) { // в т.ч. 'PanelLKView'
                            bStopped = !(listKeyTabKeep.Any (key => key.Id == ((PanelTecViewBase)tab.Controls [0]).TecViewKey.Id));
                        } else if ((typeof(PanelAdmin).IsAssignableFrom(tab.Controls[0].GetType()) == true)
                            || (typeof (PanelAlarm).IsAssignableFrom (tab.Controls [0].GetType ()) == true)) {
                            bStopped = !bAfterRunning;

                            if (bStopped == false) {
                                indxManager = (FormChangeMode.MANAGER)listPanelAdminTypes.IndexOf(tab.Controls[0].GetType());

                                bStopped = (!(indxManager == FormChangeMode.MANAGER.UNKNOWN))
                                    && (formChangeMode.m_markTabAdminChecked.IsMarked ((int)indxManager) == false);

                                if (bStopped == true)
                                    m_markPrevStatePanelAdmin.UnMarked ((int)indxManager);
                                else
                                    ;
                            } else
                                ;
                        } else if (typeof (PanelStatistic).IsAssignableFrom (tab.Controls [0].GetType ()) == true) {
                            idAddingTab = tclTecViews.GetTabPageId (i);

                            if (listKeyTabKeep.Any (key => key.Id == idAddingTab) == false) {
                                if ((Enum.IsDefined (typeof (ID_ADDING_TAB), idAddingTab) == true)
                                    && (m_dictAddingTabs.ContainsKey ((ID_ADDING_TAB)idAddingTab) == true))
                                // если предполагется дальнейшее выполнение, вкладку с отображения НЕ снимать
                                    bPerformClick = true && !bAfterRunning;
                                else
                                //??? throw
                                    ;
                            } else
                                ;
                        } else
                        //??? могут быть и др. типы вкладок
                            throw new Exception($"FormMain::clearTabPages () - вкладка <{tab.Controls [0].GetType().Name}> не обрабатывается...");

                        if (bStopped == true) {
                            ((PanelStatistic)tab.Controls [0]).Stop ();
                            listToRemove.Add (tclTecViews.TabPages.IndexOf (tab));
                        } else
                            ;

                        if (bPerformClick == true)
                            listToPerformClick.Add((ID_ADDING_TAB)idAddingTab);
                        else
                            ;
                    } else
                        Logging.Logg().Warning($"FormMain::clearTabPages () - вкладка Text={tab.Text} дочерние элементs удалены ранее (???-IDisposable)...", Logging.INDEX_MESSAGE.NOT_SET);
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, $@"FormMain::clearTabPages () - вкладка Text={tab.Text}...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            tclTecViews.SelectedIndexChanged -= tclTecViews_SelectedIndexChanged;

            for (i = listToRemove.Count - 1; !(i < 0); i--)
                tclTecViews.RemoveTabPage(listToRemove[i]);

            listToPerformClick.ForEach (keyTab => m_dictAddingTabs [keyTab].menuItem.PerformClick ());

            if (bAfterRunning == true)
            // для дальнейшего продолжения работы - присоединить обработчик
                tclTecViews.SelectedIndexChanged += tclTecViews_SelectedIndexChanged;
            else
                ;

            //selectedTecViews.Clear();

            //Logging.Logg().Debug(@"FormMain::clearTabPages () - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Активация текущей вкладки...
        /// </summary>
        private void activateTabPage()
        {
            activateTabPage(tclTecViews.SelectedIndex, true);
        }

        private void activateTabPage(int indx, bool active)
        {
            string strMsgDebug = string.Empty;

            if (!(indx < 0))
            {
                strMsgDebug = @"FormMain::activateTabPage () - indx=" + indx + @", active=" + active.ToString() + @", name=" + tclTecViews.TabPages[indx].Text.Trim();

                if (tclTecViews.TabPages [indx].Controls.Count > 0)
                    ((HPanelCommon)tclTecViews.TabPages [indx].Controls [0]).Activate (active);
                else
                    strMsgDebug = $"FormMain::activateTabPage () - indx={indx}, active={active.ToString ()} не применено: для вкладка вызван 'Dispose'";
            }
            else
                strMsgDebug = @"FormMain::activateTabPage () - indx=" + indx + @", active=" + active.ToString();

            Logging.Logg().Debug(strMsgDebug + @" - (вЫход)...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        private void ActivateTabPage()
        {
            //selectedTecViews[tclTecViews.SelectedIndex].Activate(true);
            if (!(tclTecViews.SelectedIndex < 0))
                activateTabPage(tclTecViews.SelectedIndex, true);
            else
                ;

            //Деактивация
            if ((!(m_prevSelectedIndex < 0))
                && (!(m_prevSelectedIndex == tclTecViews.SelectedIndex))
                && (m_prevSelectedIndex < tclTecViews.TabCount))
            {
                activateTabPage(m_prevSelectedIndex, false);
            }
            else
                ;

            m_prevSelectedIndex = tclTecViews.SelectedIndex;
        }

        private void OnEventFileConnSettSave(FIleConnSett.eventFileConnSettSave ev)
        {
            Properties.Settings.Default[@"connsett"] = new string(ev.hash, 0, ev.length);
            Properties.Settings.Default.Save();
        }

        private void FormMain_FormLoad(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo; //ru-Ru

            //Logging.Logg().Debug(@"FormMain_FormLoad () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);
            //m_fileConnSett = new FIleConnSett(new string [] {@"connsett", Properties.Settings.Default.Properties[@"connsett"].ToString ()});
            //m_fileConnSett = new FIleConnSett(Properties.Settings.Default.Properties [@"connsett"].DefaultValue.ToString (), FIleConnSett.MODE.SETTINGS);
            //m_fileConnSett = new FIleConnSett((string)Properties.Settings. [@"connsett"], FIleConnSett.MODE.SETTINGS);
            //m_fileConnSett = new FIleConnSett((string)Properties.Settings.Default[@"connsett"], FIleConnSett.MODE.SETTINGS);
            //MessageBox.Show((IWin32Window)null, @"FormMain::FormMain () - new FIleConnSett (...)", @"Отладка!");

            //Только для 'FIleConnSett.MODE.SETTINGS'
            //m_fileConnSett.EventFileConnSettSave += new FIleConnSett.DelegateOnEventFileConnSettSave(OnEventFileConnSettSave);

            string msg = string.Empty;
            bool bAbort = false;

            s_listFormConnectionSettings = new List<FormConnectionSettings>();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));
            s_listFormConnectionSettings.Add(null);

            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
#if MODE_STATIC_CONNECTION_LEAVING
                DbTSQLConfigDatabase.ModeStaticConnectionLeave = DbTSQLInterface.ModeStaticConnectionLeaving.Yes;
#endif
                switch (Initialize(out msg))
                {
                    case -1:
                        msg = FormMainStatistic.MSG_ERROR_INIT[(int)FormMainStatistic.INDEX_ERROR_INIT.UNKNOWN];
                        break;
                    case -3: //Не найден пользователь
                        //Остальные п.п. меню блокируются в 'сменитьРежимToolStripMenuItem_EnabledChanged'
                        // этот п. блокируется только при конкретной ошибке "-3"
                        this.настройкиСоединенияБДКонфToolStripMenuItem.Enabled =
                             false;
                        break;
                    case -2:
                    case -5:
                    case -4: //@"Необходимо изменить параметры соединения с БД"
                        //Сообщение получено из 'Initialize'
                        break;
                    default:
                        //Успех...
                        break;
                }
#if MODE_STATIC_CONNECTION_LEAVING
                DbTSQLConfigDatabase.ModeStaticConnectionLeave = DbTSQLInterface.ModeStaticConnectionLeaving.No;
#endif
            }
            else
            {//Файла с параметрами соединения нет совсем или считанные параметры соединения не валидны
                msg = @"Необходимо изменить параметры соединения с БД конфигурации";
            }

            if (msg.Equals(string.Empty) == false)
                Abort(msg, bAbort);
            else
                ;

            _listFormUtility = new List<Form> { formPassword, formSetPassword, formChangeMode, formGraphicsSettings, formParameters, formParametersTG };

            this.Activate();
        }

        public override void Close(bool bForce) 
        {
            if (bForce == false)
                base.Close(bForce); 
            else
                FormMain_FormClosing(this, new FormClosingEventArgs(CloseReason.ApplicationExitCall, true));
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Logging.Logg().Debug(@"FormMain_FormClosing () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            if (
                //(! (m_TCPServer == null)) ||
                (!(m_arPanelAdmin == null))
                || (!(m_timer == null))
                )
                if (e.Cancel == false)
                    if (e.CloseReason == CloseReason.UserClosing)
                        if (MessageBox.Show(this, "Вы уверены, что хотите закрыть приложение?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            //Нет, не закрываем
                            activateTabPage(tclTecViews.SelectedIndex, true);

                            //Продолжаем и устанавливаем признак: завершить обработку события 'e'
                            e.Cancel = true;
                        }
                        else
                        {
                            //Да, закрываем; признаку оставляем прежнее значение 'False': продолжить обработку события 'e'
                            if (InvokeRequired == true)
                                this.BeginInvoke(new DelegateObjectFunc(stop), e);
                            else
                                Stop(e);
                        }
                    else
                        //Да, закрываем; признаку оставляем прежнее значение 'False': продолжить обработку события 'e'
                        if (InvokeRequired == true)
                            this.BeginInvoke(new DelegateObjectFunc(stop), e);
                        else
                            Stop(e);
                else
                {
                    //Закрываем и устанавливаем признак: продолжить обработку события 'e'
                    e.Cancel = false;

                    //Да, закрываем; признаку оставляем прежнее значение 'False': продолжить обработку события 'e'
                    if (InvokeRequired == true)
                        this.BeginInvoke(new DelegateObjectFunc(stop), e);
                    else
                        Stop(e);
                }
            else
                ;
        }

        private int connectionSettings(CONN_SETT_TYPE type)
        {
            int iRes = -1;
            DialogResult result;
            result = s_listFormConnectionSettings[(int)type].ShowDialog(this);
            if (result == DialogResult.Yes)
            {
                StopTabPages();

                base.Stop();

                string msg = string.Empty;
                iRes = Initialize(out msg);
                if (!(iRes == 0))
                    //@"Ошибка инициализации пользовательских компонентов формы"
                    Abort(msg, false);
                else
                    ;
            }
            else
                ;

            return iRes;
        }

        private bool stopTabPages()
        {
            bool bRes = true;

            if (tclTecViews.TabCount > 0)
                if (MessageBox.Show(this, "Вы уверены, что хотите закрыть текущие вкладки?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    delegateStartWait();

                    StopTabPages();

                    if (!(m_listStandardTabs == null))
                        for (int i = 0; i < m_listStandardTabs.Count; i++)
                            m_listStandardTabs[i].Stop();
                    else
                        ;

                    //??? кроме 'Alarm'
                    m_markPrevStatePanelAdmin.UnMarked();

                    formChangeMode.btnClearAll_Click(formChangeMode, new EventArgs());

                    delegateStopWait();

                    this.Focus();
                }
                else
                {
                    bRes = false; //e.Cancel = true;
                }
            else
                ;

            return bRes;
        }

        private void настройкиСоединенияБДКонфToolStripMenuItem_Click(object sender, EventArgs e)
        {
            настройкиСоединенияToolStripMenuItem_Click(sender, e, CONN_SETT_TYPE.CONFIG_DB);
        }

        private void настройкиСоединенияБДИсточникToolStripMenuItem_Click(object sender, EventArgs e)
        {
            настройкиСоединенияToolStripMenuItem_Click(sender, e, CONN_SETT_TYPE.LIST_SOURCE);
        }

        ////Вариант №1
        //private void formAnalyzerCloused (object obj, FormClosedEventArgs ev)
        //{
        //    if (IsHandleCreated == true)
        //        if (InvokeRequired == true)
        //            this.BeginInvoke(new DelegateFunc(activateTabPage));
        //        else
        //            activateTabPage();
        //    else
        //        ;
        //}

        private void настройкиСоединенияToolStripMenuItem_Click(object sender, EventArgs e, CONN_SETT_TYPE type)
        {
            if (stopTabPages() == true)
            {
                //???
                //string strPassword = "password";
                //MD5CryptoServiceProvider md5;
                //md5 = new MD5CryptoServiceProvider();
                //StringBuilder strPasswordHashed = new StringBuilder ();
                //byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(strPassword));

                bool bShowFormConnectionSettings = false
                    , bConfigDbRegistred = false;
                ConnectionSettingsSource connSettSource;
                if (formPassword == null)
                {
                    bShowFormConnectionSettings = true;
                }
                else
                {
                    if ((!(s_listFormConnectionSettings == null)) // список форм создан
                        && (s_listFormConnectionSettings[(int)type] == null) //вызываемая форма ни разу не отображалась (объект не создан)
                        && (!(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB] == null))) // форма с праметрами соединенияя с БД конфигурации создана
                    {
                        DelegateReadConnSettFunc delegateRead = null;
                        DelegateSaveConnSettFunc delegateSave = null;

                        switch (type)
                        {
                            case CONN_SETT_TYPE.CONFIG_DB:
                                delegateRead = s_fileConnSett.ReadSettingsFile;
                                delegateSave = s_fileConnSett.SaveSettingsFile;
                                break;
                            case CONN_SETT_TYPE.LIST_SOURCE:
                                DbTSQLConfigDatabase.DbConfig().Register();

                                connSettSource = new ConnectionSettingsSource();

                                delegateRead = connSettSource.Read;
                                delegateSave = connSettSource.Save;
                                break;
                            default:
                                break;
                        }

                        if ((!(delegateRead == null))
                            && (!(delegateSave == null)))
                            s_listFormConnectionSettings[(int)type] = new FormConnectionSettings(DbTSQLConfigDatabase.DbConfig ().ListenerId, delegateRead, delegateSave);
                        else
                            Abort(@"параметры соединения с БД конфигурации", false);
                    }
                    else
                        ;
                    // повторная проверка
                    if ((!(s_listFormConnectionSettings[(int)type] == null)) // объект вызываемой формы создан
                        && (!(s_listFormConnectionSettings[(int)type].Ready == 0))) // объект вызываемой формы не готов к отображению
                    {
                        bShowFormConnectionSettings = true;
                    }
                    else
                    {
                        bConfigDbRegistred = type == CONN_SETT_TYPE.CONFIG_DB;
                        if (bConfigDbRegistred == true)
                            DbTSQLConfigDatabase.DbConfig ().Register ();
                        else
                            ;
                        formPassword.SetIdPass(DbTSQLConfigDatabase.DbConfig ().ListenerId, 0, Passwords.INDEX_ROLES.ADMIN);
                        DialogResult dlgRes = formPassword.ShowDialog(this);
                        if ((dlgRes == DialogResult.Yes) || (dlgRes == DialogResult.Abort))
                            bShowFormConnectionSettings = true;
                        else
                            ;
                    }
                }
                // отменять регистрацию именно ДО вызова 'connectionSettings'
                // , иначе идентификаторы старого и нового наборов парметров соединения с БД совпадут
                // , но только для БД_конфигурации, для списка БД 'idListener' сохранен в форме
                if (bConfigDbRegistred == true)
                    DbTSQLConfigDatabase.DbConfig ().UnRegister();
                else
                    ;
                if (bShowFormConnectionSettings == true)
                    connectionSettings(type);
                else
                    ;
                // отменить регистрацию подписчика, сохраненного в форме
                if (type == CONN_SETT_TYPE.LIST_SOURCE)
                    DbTSQLConfigDatabase.DbConfig ().UnRegister ();
                else
                    ;
                // в любом случае удалить объект с параметрами соединения списка источников данных
                // , чтобы при повторном вызове гарантированно назначить актуальный идентификатор соединения с БД конфигурации
                s_listFormConnectionSettings [(int)(int)CONN_SETT_TYPE.LIST_SOURCE] = null;
            }
            else
                ;
        }

        ///// <summary>
        ///// Структура для хранения параметров добавляемой панели
        ///// </summary>
        //private struct PANEL_TO_STANDARD_TAB
        //{
        //    /// <summary>
        //    /// Индекс пункта в списке всех доступных к отображению объектов
        //    /// </summary>
        //    public int indx_itemChangeMode;
        //    ///// <summary>
        //    ///// Индекс панели в списке всех доступных к добавлению на вкладке
        //    ///// </summary>
        //    //public int indx_tecView;
        //    /// <summary>
        //    /// Объект ТЭЦ для объекта отображения
        //    /// </summary>
        //    public TEC tec;
        //    /// <summary>
        //    /// Индекс ТЭЦ в списке ТЭЦ
        //    /// </summary>
        //    public FormChangeMode.KeyTECComponent key;
        //}

        private void addPanelTecView(TEC tec, FormChangeMode.KeyDevice key)
        {
            PanelTecViewBase panelTecView = null;

            if (tec.GetReadySensorsStrings (TECComponent.TYPE.ELECTRO) == false)
                tec.InitSensorsTEC();
            else
                ;

            if (tec.m_id > (int)TECComponent.ID.LK)
                panelTecView = new PanelLKView(tec, key);
            else
                panelTecView = new PanelTecView(tec, key, null/*, ErrorReport, WarningReport, ActionReport, ReportClear*/);

            panelTecView.SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
            panelTecView.SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
            m_listStandardTabs.Add(panelTecView);
        }

        private void сменитьРежимToolStripMenuItem_Click()
        {
            delegateStartWait();

            Int16 parametrsTGBiysk = 0;
            int //tecView_index = -1,
                i = -1;
            PanelTecViewBase panel;
            FormChangeMode.KeyDevice keyToAdding;
            List<FormChangeMode.KeyDevice> listKeyToAdding = new List<FormChangeMode.KeyDevice> ();
            List<FormChangeMode.KeyDevice> listKeyToKeep = new List<FormChangeMode.KeyDevice> ();

            #region отображаем вкладки ТЭЦ - аналог PanelCustomTecView::MenuItem_OnClick
            for (i = 0; i < formChangeMode.m_listItems.Count; i++) //или TECComponent_index.Count
            {
                //Только если элемент в списке имеет признак видимости 'true' и имеет признак 'выбран'
                if ((formChangeMode.m_listItems[i].bVisibled == true)
                    && (formChangeMode.m_listItems[i].bChecked == true))
                {
                    // не рассматривать не стандартные вкладки, наприммер 'ПБР-диспетчер', 'Сигн.-диспетчер', 'ПБР-НСС'
                    if (formChangeMode.m_listItems[i].key.Id > (int)TECComponent.ID.MAX)
                        continue;
                    else
                        ;
                    // поиск панели для вкладки начать с обнуления объекта
                    keyToAdding = FormChangeMode.KeyDevice.Empty;
                    //Найти индекс панели в списке для стандартных вкладок
                    panel = m_listStandardTabs.Find(p => p.m_tecView.CurrentKey.Id == formChangeMode.m_listItems[i].key.Id);

                    if (Equals(panel, null) == true)
                    {//Не найден элемент - создаем, добавляем
                        foreach (StatisticCommon.TEC t in formChangeMode.m_list_tec)
                        {
                            if (t.m_id == formChangeMode.m_listItems[i].key.Id)
                            {
                                keyToAdding = new FormChangeMode.KeyDevice () { Id = formChangeMode.m_listItems [i].key.Id, Mode = TECComponent.GetMode (formChangeMode.m_listItems [i].key.Id) };
                                // добавить панель
                                addPanelTecView (t, keyToAdding);
                                // сохранить индекс добавленной панели
                                listKeyToAdding.Add(keyToAdding);

                                break;
                            }
                            else
                                ;
                            // проверить наличие компонентов станции
                            if (t.ListTECComponents.Count > 0)
                            {
                                foreach (TECComponent g in t.ListTECComponents)
                                {
                                    if (g.m_id == formChangeMode.m_listItems[i].key.Id)
                                    {
                                        keyToAdding = new FormChangeMode.KeyDevice () { Id = formChangeMode.m_listItems [i].key.Id, Mode = TECComponent.GetMode (formChangeMode.m_listItems [i].key.Id) };
                                        // добавить панель
                                        addPanelTecView (t, keyToAdding);
                                        // сохранить индекс добавленной панели
                                        // и одновременно признак досрочного завершения внешнего цикла
                                        listKeyToAdding.Add(keyToAdding);

                                        break;
                                    }
                                    else
                                        ;
                                }
                            }
                            else
                                ;
                            // проверка необходима, т.к. возможен досрочный (при добавлении панели в список) выход из вложенного массива
                            if ((m_listStandardTabs.Count > 0)
                                && (keyToAdding == m_listStandardTabs.Last().TecViewKey))
                                break;
                            else
                                ;
                        }
                    }
                    else
                    {
                        //Панель ранее уже была добавлена в список
                        listKeyToAdding.Add (panel.TecViewKey);
                    }
                    // все ранее(предыдущие вызовы метода) добавленные в список, и необходимые к отображению сейчас
                    //  разместить в особый список - для удержания в текущем состоянии
                    if (keyToAdding.Equals (FormChangeMode.KeyDevice.Empty) == true)
                        listKeyToKeep.Add (panel.TecViewKey);
                    else
                        ;
                }
                else
                // элемент в списке имеет признак видимости 'false' или не имеет признак 'выбран'
                    ;
            }
            #endregion

            clearTabPages (listKeyToKeep, true);

            foreach (FormChangeMode.KeyDevice key in listKeyToAdding)
            {
                panel = m_listStandardTabs.Find (tab => tab.TecViewKey == key);
                // проверить признак объекта отображения принадлежности к Бийской ТЭЦ
                // т.к. для Бийской ТЭЦ есть индивидуальные параметры, для настройки которых активируется/блокируется соответствующий п. меню
                if ((panel.TecViewTecType == StatisticCommon.TEC.TEC_TYPE.BIYSK)
                    // && (параметрыТГБийскToolStripMenuItem.Visible == false)
                    )
                    // установить признак активации/блокировки п./меню
                    parametrsTGBiysk++;
                else
                    ;

                if ((tclTecViews.IndexOfID(key.Id) < 0)
                    && (m_dictFormFloat == null ? true : m_dictFormFloat.ContainsKey(key.Id) == false))
                {
                    // добавить вкладку
                    tclTecViews.AddTabPage(panel
                        , panel.TecViewNameShr
                        , panel.TecViewKey.Id
                        , HStatisticTabCtrlEx.TYPE_TAB.FLOAT);
                    // инициировать операции по инициализации панели
                    panel.Start();
                }
                else
                    ;
            }

            //if (!(m_timer.Interval == ProgramBase.TIMER_START_INTERVAL))
            if (m_bAutoActionTabs == false)
                //Сохранить список основных вкладок...
                if (файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked == true)
                    fileProfileSaveStandardTab();
                else
                    ;
            else
                ;

            if ((HStatisticUsers.allTEC == 0)
                || (HStatisticUsers.allTEC == TEC.ID_TEC_BIYSK))
            {
                параметрыТГБийскToolStripMenuItem.Visible = parametrsTGBiysk > 0;

                //m_formParametersTG = new FormParametersTG_FileINI(@"setup.ini");
                formParametersTG = new FormParametersTG_DB(formChangeMode.m_list_tec);
            }
            else
                ;

            delegateStopWait();

            addTabPagesAdmin();

            ActivateTabPage();
        }

        private void сменитьРежимToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                int i = -1;
                if ((!(formChangeMode == null)) && (formChangeMode.ShowDialog(this) == System.Windows.Forms.DialogResult.OK))
                {
                    // выбираем список отображаемых вкладок
                    сменитьРежимToolStripMenuItem_Click();
                }
                else
                    ;
            }
            else
                ; //Нет соединения с конфигурационной БД
        }

        private void сменитьРежимToolStripMenuItem_EnabledChanged(object sender, EventArgs e)
        {
            bool bPrevEnabled = видToolStripMenuItem.Enabled // предыдущее состояние по одному из (например, 1-му в списке) п.п. меню
                , bCurEnabled = (sender as ToolStripMenuItem).Enabled;

            if (!(bCurEnabled == bPrevEnabled))
            {
                видToolStripMenuItem.Enabled = bCurEnabled;

                настройкиСоединенияБДКонфToolStripMenuItem.Enabled =
                настройкиСоединенияБДИсточникToolStripMenuItem.Enabled =
                    bCurEnabled && (HStatisticUsers.RoleIsAdmin == true);
                ////Вариант №1 - индивидуальные пароли - установить индивидуально
                //изменитьПарольДиспетчераToolStripMenuItem.Enabled = bCurEnabled && ((HStatisticUsers.RoleIsAdmin == true) || (HStatisticUsers.RoleIsKomDisp == true));
                //изменитьПарольАдминистратораToolStripMenuItem.Enabled = bCurEnabled && (HStatisticUsers.RoleIsAdmin == true);
                //изменитьПарольНССToolStripMenuItem.Enabled = bCurEnabled && ((HStatisticUsers.RoleIsAdmin == true) || (HStatisticUsers.Role == HStatisticUsers.ID_ROLES.NSS));
                //изменитьПарольЛКДиспетчераToolStripMenuItem.Enabled = bCurEnabled && ((HStatisticUsers.RoleIsAdmin == true) || (HStatisticUsers.Role == HStatisticUsers.ID_ROLES.LK_DISP));
                //Вариант №2 - индивидуальные пароли - установить на общих основаниях
                изменитьПарольДиспетчераToolStripMenuItem.Enabled =
                изменитьПарольАдминистратораToolStripMenuItem.Enabled =
                изменитьПарольНССToolStripMenuItem.Enabled =
                изменитьПарольЛКДиспетчераToolStripMenuItem.Enabled =
                    bCurEnabled && (HStatisticUsers.RoleIsAdmin == true);

                параметрыToolStripMenuItem.Enabled = bCurEnabled;
                // разрешить использование дочерних п.п.
                if (bCurEnabled == true)
                {
                    m_dictAddingTabs[ID_ADDING_TAB.CUSTOM_2X2_1].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.CUSTOM_2X2_2].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.CUSTOM_2X2_3].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.CUSTOM_2X2_4].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.CUSTOM_2X3_1].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.CUSTOM_2X3_2].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.CUSTOM_2X3_3].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.CUSTOM_2X3_4].menuItem.Enabled =
                        bCurEnabled;
                    m_dictAddingTabs[ID_ADDING_TAB.CUR_POWER].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.TM_SN_POWER].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.MONITOR_LAST_MINUTES].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.SOBSTV_NYZHDY].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.SOTIASSO_HOUR].menuItem.Enabled =
                    // KhryapinAN, 2017-06
                    m_dictAddingTabs[ID_ADDING_TAB.AIISKUE_SOTIASSO_DAY].menuItem.Enabled =
                    m_dictAddingTabs[ID_ADDING_TAB.VZLET_TDIRECT].menuItem.Enabled =
                        bCurEnabled && (HStatisticUsers.allTEC < (int)TECComponent.ID.LK);

                    m_dictAddingTabs[ID_ADDING_TAB.SOTIASSO_HOUR].menuItem.Enabled &= HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_VIEW_VALUES_SOTIASSO);
                    // KhryapinAN, 2017-06
                    m_dictAddingTabs[ID_ADDING_TAB.AIISKUE_SOTIASSO_DAY].menuItem.Enabled &= HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_VIEW_VALUES_AIISKUE_SOTIASSO_DAY);
                    m_dictAddingTabs[ID_ADDING_TAB.VZLET_TDIRECT].menuItem.Enabled &= HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_VIEW_VZLET_TDIRECT);
                }
                else
                    ;
            }
            else
                ; // нет изменений
        }

        private void tclTecViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            //StatisticCommon.FormChangeMode.MANAGER modeAdmin = FormChangeMode.MANAGER.NSS;

            //if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
            //    modeAdmin = FormChangeMode.MANAGER.DISP;
            //else
            //    ;

            ActivateTabPage();
        }

        protected override int UpdateStatusString()
        {
            int have_msg = 0;
            m_lblDescMessage.Text = m_lblDateMessage.Text = string.Empty;
            PanelTecViewStandard selTecView = null;

            //for (int i = 0; i < selectedTecViews.Count; i++)
            //if ((selectedTecViews.Count > 0) /*&& (! (m_prevSelectedIndex < 0))*/)
            if ((!(m_prevSelectedIndex < 0)) && (m_prevSelectedIndex < tclTecViews.TabCount))
            {
                if ((tclTecViews.TabPages[m_prevSelectedIndex].Controls.Count > 0)
                    && (tclTecViews.TabPages[m_prevSelectedIndex].Controls[0] is PanelTecViewStandard))
                {
                    selTecView = (PanelTecViewStandard)tclTecViews.TabPages[m_prevSelectedIndex].Controls[0];

                    if (!(selTecView == null) && ((!(selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_AISKUE] == null))
                        && (!(selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_SOTIASSO] == null))))
                    {
                        if ((m_report.actioned_state == true)
                            //&& ((selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_AISKUE].ignore == false)
                            //&& (selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_SOTIASSO].ignore == false))
                            )
                        {
                            if (selTecView.Actived == true)
                            {
                                m_lblDateMessage.Text = m_report.last_time_action.ToString();
                                m_lblDescMessage.Text = m_report.last_action;
                            }
                            else
                                ;
                        }
                        else
                            ;

                        if ((m_report.warninged_state == true)
                            //&& ((selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_AISKUE].ignore == false)
                            //&& (selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_SOTIASSO].ignore == false))
                            )
                        {
                            have_msg = 1;
                            if (selTecView.Actived == true)
                            {
                                m_lblDateMessage.Text = m_report.last_time_warning.ToString();
                                m_lblDescMessage.Text = m_report.last_warning;
                            }
                            else
                                ;
                        }
                        else
                            ;

                        if ((m_report.errored_state == true)
                            //&& ((selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_AISKUE].ignore == false)
                            //&& (selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_SOTIASSO].ignore == false))
                            )
                        {
                            have_msg = -1;
                            if (selTecView.Actived == true)
                            {
                                m_lblDateMessage.Text = m_report.last_time_error.ToString();
                                m_lblDescMessage.Text = m_report.last_error;
                            }
                            else
                                ;
                        }
                        else
                            ;
                    }
                    else
                        ; //Вкладка не найдена
                }
                else
                {
                    if (m_report.actioned_state == true)
                    {
                        m_lblDateMessage.Text = m_report.last_time_action.ToString();
                        m_lblDescMessage.Text = m_report.last_action;
                    }
                    else
                        ;

                    if (m_report.warninged_state == true)
                    {
                        have_msg = 1;
                        m_lblDateMessage.Text = m_report.last_time_warning.ToString();
                        m_lblDescMessage.Text = m_report.last_warning;
                    }
                    else
                        ;

                    if (m_report.errored_state == true)
                    {
                        have_msg = -1;
                        m_lblDateMessage.Text = m_report.last_time_error.ToString();
                        m_lblDescMessage.Text = m_report.last_error;
                    }
                    else
                        ;
                }
            }
            else
                ;

            return have_msg;
        }

        private void addTabPagesAdmin()
        {
            List<FormChangeMode.MANAGER> indexes = new List<FormChangeMode.MANAGER> { FormChangeMode.MANAGER.DISP
                , FormChangeMode.MANAGER.NSS
                , FormChangeMode.MANAGER.ALARM
                , FormChangeMode.MANAGER.LK
                , FormChangeMode.MANAGER.TEPLOSET
            };

            DbTSQLConfigDatabase.DbConfig ().Register ();

            indexes.ForEach (indx => {
                if ((formChangeMode.m_markTabAdminChecked.IsMarked ((int)indx) == true)
                    && (m_markPrevStatePanelAdmin.IsMarked ((int)indx) == false)
                    )
                    addTabPageAdmin (DbTSQLConfigDatabase.DbConfig ().ListenerId, indx);
                else
                    ;
            });

            DbTSQLConfigDatabase.DbConfig ().UnRegister ();
        }

        /// <summary>
        /// Добавить вкладку(и) из интрументария 'администратор-диспетчер'
        /// </summary>
        /// <param name="idListener">Идентификатор установленного соединения с БД_конфигурации</param>
        /// <param name="modeAdmin">Тип добавляемой вкладки</param>
        private void addTabPageAdmin(int idListener, FormChangeMode.MANAGER modeAdmin)
        {
            int iRes = 0;
            StatisticCommon.FormChangeMode.MODE_TECCOMPONENT mode = FormChangeMode.MODE_TECCOMPONENT.ANY;

            List<FormChangeMode.MANAGER> indexes = new List<FormChangeMode.MANAGER> { FormChangeMode.MANAGER.DISP
                , FormChangeMode.MANAGER.NSS
                , FormChangeMode.MANAGER.ALARM
                , FormChangeMode.MANAGER.LK
                , FormChangeMode.MANAGER.TEPLOSET
            };
            //!!! должно быть строгое совпадение с предыдущим списком
            List<HStatisticUsers.ID_ALLOWED> alloweds = new List<HStatisticUsers.ID_ALLOWED> { HStatisticUsers.ID_ALLOWED.AUTO_TAB_PBR_KOMDISP
                , HStatisticUsers.ID_ALLOWED.AUTO_TAB_PBR_NSS
                , HStatisticUsers.ID_ALLOWED.AUTO_TAB_ALARM
                , HStatisticUsers.ID_ALLOWED.AUTO_TAB_LK_ADMIN
                , HStatisticUsers.ID_ALLOWED.AUTO_TAB_TEPLOSET_ADMIN
            };

            if (HStatisticUsers.RoleIsDisp == true)
            {
                Passwords.INDEX_ROLES indxRolesPassword = Passwords.INDEX_ROLES.ADMIN;
                DialogResult dlgRes = System.Windows.Forms.DialogResult.Yes;
                bool bPasswordAsked = false;

                bPasswordAsked = ! HStatisticUsers.IsAllowed ((int)alloweds [indexes.IndexOf (modeAdmin)]);

                if (bPasswordAsked == true)
                {
                    if ((modeAdmin == FormChangeMode.MANAGER.DISP)
                        || (modeAdmin == FormChangeMode.MANAGER.ALARM))
                        indxRolesPassword =
                            //Passwords.INDEX_ROLES.COM_DISP
                            // 04.04.2018 KhryapinAN - для возможности НСС устанавливать значения инициатив собственных
                            // , кроме этого требуется разрешить НСС использование вкладки "ПБР - диспетчер"
                            ((HStatisticUsers.RoleIsKomDisp == true)
                                || (HStatisticUsers.RoleIsAdmin == true))? Passwords.INDEX_ROLES.COM_DISP
                                : (HStatisticUsers.RoleIsDisp == true) ? Passwords.INDEX_ROLES.NSS
                                    : Passwords.INDEX_ROLES.COM_DISP;
                    else
                        if ((modeAdmin == FormChangeMode.MANAGER.NSS)
                            || (modeAdmin == FormChangeMode.MANAGER.TEPLOSET))
                            indxRolesPassword = Passwords.INDEX_ROLES.NSS;
                        else
                            if (modeAdmin == FormChangeMode.MANAGER.LK)
                                indxRolesPassword = Passwords.INDEX_ROLES.LK_DISP;
                            else
                                ;

                    if (indxRolesPassword == Passwords.INDEX_ROLES.ADMIN)
                        iRes = -1;
                    else
                        ;

                    formPassword.SetIdPass(idListener, 0, indxRolesPassword);
                    dlgRes = formPassword.ShowDialog(this);
                }
                else
                    ;

                if (iRes == 0)
                {
                    if (dlgRes == DialogResult.Yes)
                    {
                        //StartWait();
                        delegateStartWait();

                        if (Equals (m_arPanelAdmin [(int)modeAdmin], null) == true)
                            createPanelAdmin (modeAdmin);
                        else
                            ;
                        m_arPanelAdmin[(int)modeAdmin].Start();

                        switch (modeAdmin)
                        {
                            case FormChangeMode.MANAGER.DISP:
                            case FormChangeMode.MANAGER.LK:
                            case FormChangeMode.MANAGER.ALARM:
                                mode = FormChangeMode.MODE_TECCOMPONENT.GTP;
                                break;
                            case FormChangeMode.MANAGER.NSS:
                            case FormChangeMode.MANAGER.TEPLOSET:
                                mode = FormChangeMode.MODE_TECCOMPONENT.TEC; //TEC, PC или TG не важно
                                break;
                            default:
                                break;
                        }

                        tclTecViews.AddTabPage(m_arPanelAdmin[(int)modeAdmin]
                            , formChangeMode.getNameAdminValues(modeAdmin, mode)
                            , FormChangeMode.ID_ADMIN_TABS[(int)modeAdmin]
                            , HStatisticTabCtrlEx.TYPE_TAB.FIXED);

                        switch (modeAdmin)
                        {
                            case FormChangeMode.MANAGER.DISP:
                            case FormChangeMode.MANAGER.NSS:
                            case FormChangeMode.MANAGER.LK:
                            case FormChangeMode.MANAGER.TEPLOSET:
                                (m_arPanelAdmin[(int)modeAdmin] as PanelAdmin).InitializeComboBoxTecComponent(mode, true, !(modeAdmin == FormChangeMode.MANAGER.LK));
                                break;
                            case FormChangeMode.MANAGER.ALARM:
                                break;
                            default:
                                break;
                        }

                        delegateStopWait();

                        if (m_markPrevStatePanelAdmin.IsMarked ((int)modeAdmin) == false)
                            m_markPrevStatePanelAdmin.Set ((int)modeAdmin, true);
                        else
                            // повторный вызов вкладки на отображение
                            ;
                    }
                    else
                        ;
                }
                else
                    Logging.Logg().Error("FormMain : addTabPageAdmin - пароль администратора указан в качестве пароля для вкладки", Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
            //Не требуется отображать вкладку 'panelAdminXXX'
                ;
        }

        private void ReadAnalyzer(TcpClient res, string cmd)
        {
            //Message from Analyzer CMD;ARG1, ARG2,...,ARGN=RESULT
            switch (cmd.Split('=')[0].Split(';')[0])
            {
                case "INIT":
                    //m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + "OK");
                    break;
                case "LOG_LOCK":
                    //m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + "OK;" + Logging.Logg().Suspend());
                    break;
                case "LOG_UNLOCK":
                    Logging.Logg().Resume();

                    //m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + "OK");
                    break;
                case "TAB_VISIBLE":
                    int i = -1,
                        mode = formChangeMode.getModeTECComponent();
                    string strIdItems = string.Empty,
                            mes = "OK" + ";";
                    mes += mode;

                    strIdItems = formChangeMode.getIdsOfCheckedIndicies();
                    if (strIdItems.Equals(string.Empty) == false)
                        mes += "; " + strIdItems;
                    else
                        ;

                    //m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + mes);
                    break;
                case "DISONNECT":
                    break;
                case "":
                    break;
                default:
                    break;
            }
        }

        private void FormMain_PanelCustomTecView_EvtContentChanged()
        {
            if (m_bAutoActionTabs == false)
                this.BeginInvoke(new DelegateFunc(fileProfileSaveAddingTab));
            else
                ;
        }

        protected override void  timer_Start()
        {
 	        int i = -1;
        }

        void createPanelAdmin (FormChangeMode.MANAGER manager)
        {
            PanelStatistic panelAdmin = null;
            HMark markQueries;

            markQueries = new HMark (new int [] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR });

            switch (manager) {
                case FormChangeMode.MANAGER.DISP:
                    panelAdmin = new PanelAdminKomDisp (markQueries);
                    break;
                case FormChangeMode.MANAGER.LK:
                    panelAdmin = new PanelAdminLK (markQueries);
                    break;
                case FormChangeMode.MANAGER.TEPLOSET:
                    panelAdmin = new PanelAdminVyvod (markQueries);
                    break;
                case FormChangeMode.MANAGER.NSS:
                    panelAdmin = new PanelAdminNSS (markQueries);
                    break;
                case FormChangeMode.MANAGER.ALARM:
                    panelAdmin = new PanelAlarm (new HMark (new int [] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR, (int)CONN_SETT_TYPE.DATA_AISKUE, (int)CONN_SETT_TYPE.DATA_SOTIASSO })
                        , StatisticAlarm.MODE.ADMIN
                        , formGraphicsSettings.FontColor
                        , formGraphicsSettings.BackgroundColor);
                    (panelAdmin as PanelAlarm).EventGUIReg += new AlarmNotifyEventHandler (OnPanelAlarmEventGUIReg);
                    m_formAlarmEvent.EventFixed += new DelegateObjectFunc ((panelAdmin as PanelAlarm).OnEventFixed);
                    ////!!! автоматический старт
                    ////??? m_markPrevStatePanelAdmin - признак вызова на отображение
                    //m_markPrevStatePanelAdmin.Set ((int)i, true);
                    break;
                default:
                    break;
            }

            if (Equals (panelAdmin, null) == false) {
                panelAdmin.SetDelegateWait (delegateStartWait, delegateStopWait, delegateEvent);
                panelAdmin.SetDelegateReport (ErrorReport, WarningReport, ActionReport, ReportClear);

                m_arPanelAdmin [(int)manager] = panelAdmin;
            } else
                throw new Exception ($"не создана административная панель: {manager}");
        }

        /// <summary>
        /// Создать панель с диагностической информацией
        /// </summary>
        private void createPanelStatisticDiagnostic ()
        {
            m_dictAddingTabs [ID_ADDING_TAB.DIAGNOSTIC].panel = new PanelStatisticDiagnostic (HStatisticUsers.Role == HStatisticUsers.ID_ROLES.ADMIN
                    ? PanelStatisticDiagnostic.Mode.DEFAULT
                        : PanelStatisticDiagnostic.Mode.SOURCE_ONLY
                    , formGraphicsSettings.FontColor
                    , formGraphicsSettings.BackgroundColor);
            m_dictAddingTabs [ID_ADDING_TAB.DIAGNOSTIC].panel.SetDelegateReport (ErrorReport, WarningReport, ActionReport, ReportClear);
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormAbout formAbout = new FormAbout(this.Icon.ToBitmap() as Image))
            {
                formAbout.ShowDialog(this);
            }
        }

        private void панельГрафическихToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                if (панельГрафическихToolStripMenuItem.Checked == true)
                    formGraphicsSettings.Show();
                else
                    formGraphicsSettings.Hide();
            }
            else
                ;
        }

        protected override void HideGraphicsSettings()
        {
            панельГрафическихToolStripMenuItem.Checked = false;
        }

        private void значенияТекущаяМощностьГТПгТЭЦснToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[ID_ADDING_TAB.CUR_POWER].panel == null)
            {
                m_dictAddingTabs[ID_ADDING_TAB.CUR_POWER].panel = new PanelCurPower(formChangeMode.m_list_tec);
                ((PanelStatistic)m_dictAddingTabs[ID_ADDING_TAB.CUR_POWER].panel).SetDelegateWait(null, null, delegateEvent);
                m_dictAddingTabs[ID_ADDING_TAB.CUR_POWER].panel.SetDelegateReport (ErrorReport, WarningReport, ActionReport, ReportClear);
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB.CUR_POWER, (sender as ToolStripMenuItem).Text //@"P тек ГТПг, ТЭЦсн"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void значенияТекущаяМощностьТЭЦгТЭЦснToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[ID_ADDING_TAB.TM_SN_POWER].panel == null)
            {
                m_dictAddingTabs[ID_ADDING_TAB.TM_SN_POWER].panel = new PanelTMSNPower(formChangeMode.m_list_tec);
                ((PanelStatistic)m_dictAddingTabs[ID_ADDING_TAB.TM_SN_POWER].panel).SetDelegateWait(null, null, delegateEvent);
                ((PanelStatistic)m_dictAddingTabs[ID_ADDING_TAB.TM_SN_POWER].panel).SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB.TM_SN_POWER, (sender as ToolStripMenuItem).Text //@"P тек ТЭЦг, ТЭЦсн"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void мониторингПоследняяМинутаЧасToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[ID_ADDING_TAB.MONITOR_LAST_MINUTES].panel == null)
            {
                m_dictAddingTabs[ID_ADDING_TAB.MONITOR_LAST_MINUTES].panel = new PanelLastMinutes(formChangeMode.m_list_tec);
                ((PanelStatistic)m_dictAddingTabs[ID_ADDING_TAB.MONITOR_LAST_MINUTES].panel).SetDelegateWait(null, null, delegateEvent);
                ((PanelStatistic)m_dictAddingTabs[ID_ADDING_TAB.MONITOR_LAST_MINUTES].panel).SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB.MONITOR_LAST_MINUTES, (sender as ToolStripMenuItem).Text //@"Монитор P-d4%"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void ДиагностикаToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[ID_ADDING_TAB.DIAGNOSTIC].panel == null)
                createPanelStatisticDiagnostic ();
            else
                ;
            видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB.DIAGNOSTIC, (sender as ToolStripMenuItem).Text //"Диагностика"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void ПросмотрЖурналаToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[ID_ADDING_TAB.ANALYZER].panel == null)
            {
                DbTSQLConfigDatabase.DbConfig ().Register();
                m_dictAddingTabs[ID_ADDING_TAB.ANALYZER].panel = new PanelAnalyzer_DB(formChangeMode.m_list_tec, formGraphicsSettings.FontColor, formGraphicsSettings.BackgroundColor);
                m_dictAddingTabs[ID_ADDING_TAB.ANALYZER].panel.SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
                DbTSQLConfigDatabase.DbConfig ().UnRegister ();
            }
            else
                ;
            видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB.ANALYZER, (sender as ToolStripMenuItem).Text //"Журнал событий"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void СоставТЭЦToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[ID_ADDING_TAB.TEC_Component].panel == null)
            {
                DbTSQLConfigDatabase.DbConfig ().Register();
                m_dictAddingTabs[ID_ADDING_TAB.TEC_Component].panel = new PanelTECComponent(formChangeMode.m_list_tec);
                m_dictAddingTabs[ID_ADDING_TAB.TEC_Component].panel.SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
                DbTSQLConfigDatabase.DbConfig ().UnRegister ();

                (m_dictAddingTabs [ID_ADDING_TAB.TEC_Component].panel as PanelTECComponent).EventConfigGTPChanged +=
                    DbTSQLConfigDatabase.DbConfig ().UpadteDiagnosticSource;
            }
            else
                ;
            видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB.TEC_Component, (sender as ToolStripMenuItem).Text //"Состав ТЭЦ"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void собственныеНуждыToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[ID_ADDING_TAB.SOBSTV_NYZHDY].panel == null)
            {
                m_dictAddingTabs[ID_ADDING_TAB.SOBSTV_NYZHDY].panel = new PanelSobstvNyzhdy(formChangeMode.m_list_tec/*, ErrorReport, WarningReport, ActionReport, ReportClear*/);
                ((PanelSobstvNyzhdy)m_dictAddingTabs[ID_ADDING_TAB.SOBSTV_NYZHDY].panel).SetDelegateWait(null, null, delegateEvent);
                ((PanelSobstvNyzhdy)m_dictAddingTabs[ID_ADDING_TAB.SOBSTV_NYZHDY].panel).SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB.SOBSTV_NYZHDY
                , ((ToolStripMenuItem)sender).Text
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void собственныеНуждыНовToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[ID_ADDING_TAB.SOBSTV_NYZHDY_NEW].panel == null)
            {
                m_dictAddingTabs[ID_ADDING_TAB.SOBSTV_NYZHDY_NEW].panel = new PanelCommonAux(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.COMMON_AUX_PATH]
                    , formGraphicsSettings.FontColor
                    , formGraphicsSettings.BackgroundColor);
                ((PanelCommonAux)m_dictAddingTabs[ID_ADDING_TAB.SOBSTV_NYZHDY_NEW].panel).SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                ((PanelCommonAux)m_dictAddingTabs[ID_ADDING_TAB.SOBSTV_NYZHDY_NEW].panel).SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB.SOBSTV_NYZHDY_NEW
                , ((ToolStripMenuItem)sender).Text
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void выборОбъекты22ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            выборОбъектыToolStripMenuItem_CheckedChanged(sender, e, INDEX_CUSTOM_TAB.TAB_2X2, -1);
        }

        private void выборОбъекты23ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            выборОбъектыToolStripMenuItem_CheckedChanged(sender, e, INDEX_CUSTOM_TAB.TAB_2X3, -1);
        }

        private Size getPanelCustomTecViewSize(INDEX_CUSTOM_TAB type)
        {
            Size szRes = Size.Empty;

            switch (type)
            {
                case INDEX_CUSTOM_TAB.TAB_2X2:
                    szRes = new Size(2, 2);
                    break;
                case INDEX_CUSTOM_TAB.TAB_2X3:
                    szRes = new Size(3, 2);
                    break;
                default:
                    throw new Exception(@"FormMain::getPanelCustomTecViewSize () - невозможно определить размерность вкладки...");
            }

            return szRes;
        }

        private void выборОбъектыToolStripMenuItem_CheckedChanged(object sender, EventArgs e, INDEX_CUSTOM_TAB indx, int indxItem)
        {
            ToolStripMenuItem obj = sender as ToolStripMenuItem;
            if (indxItem < 0)
                indxItem = ((ToolStripMenuItem)obj.OwnerItem).DropDownItems.IndexOf(obj);
            else
                ;
            bool bStoped = true;
            ID_ADDING_TAB keyTab = m_arIdCustomTabs[(int)indx][indxItem];

            if ((obj.Checked == false)
                && (!(m_dictFormFloat == null))
                && (m_dictFormFloat.ContainsKey((int)keyTab) == true)
                && (!(m_dictFormFloat[(int)keyTab] == null)))
            {
                m_dictFormFloat[(int)keyTab].Close();
                bStoped = false;
            }
            else
                ;

            if ((m_dictAddingTabs[keyTab].panel == null)
                //&& (bStoped == true)
                //&& (obj.Checked == true)
                )
            {
                m_dictAddingTabs[keyTab].panel = new PanelCustomTecView(formChangeMode, getPanelCustomTecViewSize(indx)/*, ErrorReport, WarningReport, ActionReport, ReportClear*/);
                m_dictAddingTabs[keyTab].panel.SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
                ((PanelCustomTecView)m_dictAddingTabs[keyTab].panel).EventContentChanged += new DelegateFunc(FormMain_PanelCustomTecView_EvtContentChanged);
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged((ID_ADDING_TAB)keyTab
                , obj.OwnerItem.Text + @" - " + obj.Text
                , new bool[] { obj.Checked, bStoped });
        }

        private void рассинхронизацияДатаВремяСерверБДToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].panel == null)
                m_dictAddingTabs[ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].panel = new PanelSourceData(formGraphicsSettings.FontColor, formGraphicsSettings.BackgroundColor);
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA, (sender as ToolStripMenuItem).Text //"Дата/время серверов БД"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void значенияСОТИАССОЧасToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ID_ADDING_TAB keyTab = ID_ADDING_TAB.SOTIASSO_HOUR;

            if (m_dictAddingTabs[keyTab].panel == null)
            {
                DbTSQLConfigDatabase.DbConfig ().Register ();

                m_dictAddingTabs [keyTab].panel = new PanelSOTIASSOHour(DbTSQLConfigDatabase.DbConfig ().InitTEC (
                    FormChangeMode.MODE_TECCOMPONENT.ANY
                    , true
                    , new int [] { (int)TECComponent.ID.TEC, (int)TECComponent.ID.GTP }
                    , true));

                DbTSQLConfigDatabase.DbConfig ().UnRegister ();

                m_dictAddingTabs [keyTab].panel.SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
                formChangeMode.EventChangeMode += ((PanelSOTIASSOHour)(m_dictAddingTabs[keyTab].panel)).ChangeMode;
                formChangeMode.PerformChangeMode();
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(keyTab, "Значения СОТИАССО-час"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

#region // KhryapinAN, 2017-06
        private void значенияАИИСКУЭСОТИАССОСуткиToolStripMenuItem_CheckedChanged (object sender, EventArgs e)
        {
            ID_ADDING_TAB keyTab = ID_ADDING_TAB.AIISKUE_SOTIASSO_DAY;

            if (m_dictAddingTabs[keyTab].panel == null) {
                DbTSQLConfigDatabase.DbConfig ().Register();

                m_dictAddingTabs[keyTab].panel = new PanelAISKUESOTIASSODay(DbTSQLConfigDatabase.DbConfig ().ListenerId, formChangeMode.m_list_tec);
                m_dictAddingTabs[keyTab].panel.SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);

                DbTSQLConfigDatabase.DbConfig ().UnRegister();
            } else
                ;

            видSubToolStripMenuItem_CheckedChanged(keyTab,  (sender as ToolStripMenuItem).Text //"Значения СОТИАССО-сутки"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }
#endregion

        private void значенияВзлетТпрямаяToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[ID_ADDING_TAB.VZLET_TDIRECT].panel == null)
            {
                DbTSQLConfigDatabase.DbConfig ().Register ();

                m_dictAddingTabs[ID_ADDING_TAB.VZLET_TDIRECT].panel = new PanelVzletTDirect(DbTSQLConfigDatabase.DbConfig().InitTEC(
                    FormChangeMode.MODE_TECCOMPONENT.VYVOD
                    , true
                    , new int [] { (int)TECComponent.ID.TEC, (int)TECComponent.ID.LK }
                    , true
                ));

                DbTSQLConfigDatabase.DbConfig ().UnRegister ();

                m_dictAddingTabs [ID_ADDING_TAB.VZLET_TDIRECT].panel.SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
            }
            else
                ;

            int a;
            //a.ToString.

            видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB.VZLET_TDIRECT, "Расчет теплосети"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        /// <summary>
        /// Общий метод обработки события - выбор п. меню (установка/снятие признака "Отобразить")
        /// </summary>
        /// <param name="idAddingPanel">Идентификатор панели</param>
        /// <param name="nameTab">Текст-наименование панели</param>
        /// <param name="arCheckedStoped">Массив с признаками: отобразить/снять_с_отображения панель, необходимости принудительно останавливать панель (в противном случае панель была остановлена ранее)</param>
        private void видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB idAddingPanel, string nameTab, bool[] arCheckedStoped)
        {
            bool bRes = false;

            PanelStatistic panel = m_dictAddingTabs[idAddingPanel].panel;
            HStatisticTabCtrlEx.TYPE_TAB typeTab = m_dictAddingTabs[idAddingPanel].m_typeTab;
            int key = -1
                , indxItem = -1;
            INDEX_CUSTOM_TAB indxTab = INDEX_CUSTOM_TAB.TAB_2X2;

            /// <summary>
            /// Возвратить индекс типа настраиваемой вкладки по наименованию п. меню ИЛИ наименования вкладки
            /// </summary>
            /// <param name="text">Наименование п. меню или вкладки</param>
            /// <returns>Индекс (номер) типа</returns>
            Func<string, INDEX_CUSTOM_TAB> getIndexCustomTab = delegate (string name_tab) {
                INDEX_CUSTOM_TAB indxRes = INDEX_CUSTOM_TAB.TAB_2X2; //e.TabHeaderText.Contains(@"2X2") == true
                if (name_tab.Contains (@"2X3") == true)
                    indxRes = INDEX_CUSTOM_TAB.TAB_2X3;
                else
                    ;

                return indxRes;
            };

            /// <summary>
            /// Возвратить индекс (номер окна) по наименованию п. меню ИЛИ наименования вкладки
            /// </summary>
            /// <param name="text">Наименование п. меню или вкладки</param>
            /// <returns>Индекс (номер)</returns>
            Func<string, int> getIndexItemCustomTab = delegate (string text) {
                return Int32.Parse (text.Substring (text.Length - 1, 1)) - 1;
            };

            if (arCheckedStoped[0] == true)
            {
                if (typeTab == ASUTP.Control.HTabCtrlEx.TYPE_TAB.FLOAT)
                    if (nameTab.IndexOf(@"Окно") > -1)
                    {
                        indxTab = getIndexCustomTab(nameTab);
                        indxItem = getIndexItemCustomTab(nameTab);
                        key = (int)m_arIdCustomTabs[(int)indxTab][indxItem];
                        //typeTab = HTabCtrlEx.TYPE_TAB.FLOAT;
                    }
                    else
                    {
                        key = (int)idAddingPanel;
                    }
                else
                    key = (int)idAddingPanel; // FIXED

                tclTecViews.AddTabPage(panel, nameTab, key, typeTab);

                panel.Start();
                // вкладку активируем только, если не выполняется 'm_bAutoActionTabs == false' автоматическая загрузка профиля пользователя
                // ИЛИ в случае 1-ой добавленной вкладки при автоматической загрузке профиля  пользователя
                if ((m_bAutoActionTabs == false)
                    || ((m_bAutoActionTabs == true)
                        && (tclTecViews.TabCount == 1))
                    )
                    ActivateTabPage();
                else
                    ;
            }
            else
            {//arCheckedStoped[0] == false
                //??? текст заголовка вкладки НЕ уникальный
                indxItem = tclTecViews.IndexOfID ((int)idAddingPanel);

                if (!(indxItem < 0)) {
                    bRes = tclTecViews.RemoveTabPage(indxItem); //nameTab
                    // проверить необходимость остановки панели
                    if (arCheckedStoped[1] == true)
                    {
                        panel.Activate(false);
                        panel.Stop();
                    }
                    else
                        ;
                } else {
                //??? Activate(false) и Stop () выполняются без проверки
                    m_dictFormFloat [(int)idAddingPanel].Close ();
                }
            }

            if (m_bAutoActionTabs == false)
                //Сохранить список дополнительных вкладок...
                if (файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked == true)
                    fileProfileSaveAddingTab();
                else
                    ;
            else
                ;
        }

        private void setToolStripMenuItemForeColor (ToolStripItemCollection dropDownItems, Color foreColor) {
            (from ToolStripItem mainItem in dropDownItems.Cast<ToolStripItem> () select mainItem).ToList ().ForEach (mItem => {
                if (mItem.GetType ().Equals (typeof (ToolStripMenuItem)) == true) {
                    mItem.ForeColor = foreColor;

                    if ((mItem as ToolStripMenuItem).HasDropDownItems == true)
                        setToolStripMenuItemForeColor ((mItem as ToolStripMenuItem).DropDownItems, mItem.ForeColor);
                    else
                        ;
                } else
                    ;
            });
        }

        /// <summary>
        /// Обработчик события - изменение цвета шрифта главной формы
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (главная форма)</param>
        /// <param name="e">Аргумент события</param>
        private void FormMain_ForeColorChanged (object sender, EventArgs e)
        {
            //главное окно
            MainMenuStrip.ForeColor =
            ContextMenuStrip.ForeColor =
                formGraphicsSettings.FontColor;

            (from ToolStripMenuItem mainItem in MainMenuStrip.Items select mainItem).ToList ().ForEach (mItem => {                
                setToolStripMenuItemForeColor (mItem.DropDownItems, mItem.ForeColor);
            });

            // формы-утилиты
            _listFormUtility.ForEach (form => {
                if (Equals (form, null) == false) form.ForeColor = ForeColor; else;
            });

            if (tclTecViews.TabPages.Count > 0)
                tclTecViews.TabPages [0].ForeColor = ForeColor;
            else
                ;
        }

        /// <summary>
        /// Обработчик события - изменение цвета фона главной формы
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие (главная форма)</param>
        /// <param name="e">Аргумент события</param>
        private void FormMain_BackColorChanged (object sender, EventArgs e)
        {
            //главное окно
            MainMenuStrip.BackColor =
            ContextMenuStrip.BackColor =
                formGraphicsSettings.BackgroundColor;

            if (formGraphicsSettings.m_colorShema == FormGraphicsSettings.ColorShemas.Custom) {
                MainMenuStrip.RenderMode = ToolStripRenderMode.Professional;
                MainMenuStrip.Renderer = new ToolStripProfessionalRenderer (_darkColorTable);
            } else if (formGraphicsSettings.m_colorShema == FormGraphicsSettings.ColorShemas.System) {
                MainMenuStrip.RenderMode = ToolStripRenderMode.System;
                MainMenuStrip.Renderer = null;
            } else
                ;

            // формы-утилиты
            _listFormUtility.ForEach (form => {
                if (Equals (form, null) == false)
                    form.BackColor = BackColor;
                else
                    ;
            });

            if (tclTecViews.TabPages.Count > 0)
                tclTecViews.TabPages [0].BackColor = BackColor;
            else
                ;
        }

        protected override void UpdateActiveGui(int type)
        {
            Control ctrl = null;

#region Режим изменения цветовой гаммы
            Dictionary<HStatisticUsers.ID_ALLOWED, Color> dictSetAllowedDo = new Dictionary<HStatisticUsers.ID_ALLOWED, Color> () {
                { HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_COLOR_CHANGESHEMA_FONT, formGraphicsSettings.FontColor }
                , { HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_COLOR_CHANGESHEMA_BACKGROUND, formGraphicsSettings.BackgroundColor }
            };

            if (((FormGraphicsSettings.TYPE_UPDATEGUI)type == FormGraphicsSettings.TYPE_UPDATEGUI.COLOR_SHEMA)
                || ((FormGraphicsSettings.TYPE_UPDATEGUI)type == FormGraphicsSettings.TYPE_UPDATEGUI.COLOR_CHANGESHEMA_FONT)
                || ((FormGraphicsSettings.TYPE_UPDATEGUI)type == FormGraphicsSettings.TYPE_UPDATEGUI.COLOR_CHANGESHEMA_BACKGROUND)) {
                //TODO: произойдет событие "изменение" компонента цветовой схемы
                if ((FormGraphicsSettings.TYPE_UPDATEGUI)type == FormGraphicsSettings.TYPE_UPDATEGUI.COLOR_SHEMA) {                    
                    BackColor = formGraphicsSettings.BackgroundColor;
                    ForeColor = formGraphicsSettings.FontColor;
                } else if ((FormGraphicsSettings.TYPE_UPDATEGUI)type == FormGraphicsSettings.TYPE_UPDATEGUI.COLOR_CHANGESHEMA_FONT) {
                    dictSetAllowedDo.Remove (HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_COLOR_CHANGESHEMA_BACKGROUND);
                    ForeColor = formGraphicsSettings.FontColor;
                } else if ((FormGraphicsSettings.TYPE_UPDATEGUI)type == FormGraphicsSettings.TYPE_UPDATEGUI.COLOR_CHANGESHEMA_BACKGROUND) {
                    dictSetAllowedDo.Remove (HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_COLOR_CHANGESHEMA_FONT);
                    BackColor = formGraphicsSettings.BackgroundColor;
                } else
                    ;

                if (formGraphicsSettings.m_colorShema == FormGraphicsSettings.ColorShemas.Custom) { //??? обязательно ли проверять 'Custom', т.к. подписи доступны только в этом ('Custom') режиме
                    foreach (KeyValuePair<HStatisticUsers.ID_ALLOWED, Color> pair in dictSetAllowedDo)
                        HStatisticUsers.SetAllowed (s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett ()
                            , pair.Key
                            , (_darkColorTable as CustomColorTable).ColorToRGBString (pair.Value));
                } else
                    ;
            } else
                ;
#endregion

            if ((FormGraphicsSettings.TYPE_UPDATEGUI)type == FormGraphicsSettings.TYPE_UPDATEGUI.COLOR) {
                HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.ERROR].BackColor = formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.DIVIATION);
            } else
                ;

            if ((!(tclTecViews.SelectedIndex < 0))
                && (tclTecViews.TabPages [tclTecViews.SelectedIndex].Controls.Count > 0)) {
                ctrl = tclTecViews.TabPages [tclTecViews.SelectedIndex].Controls [0];

                if ((!(tclTecViews.SelectedIndex < 0))
                    && (tclTecViews.SelectedIndex < tclTecViews.TabCount))
                    if ((ctrl is PanelStatistic)
                        //|| (ctrl is PanelCustomTecView)
                        //|| (ctrl is PanelSobstvNyzhdy)
                        //|| (ctrl is PanelSOTIASSO)
                        //|| (ctrl is PanelLKView)
                        )
                        ((PanelStatistic)ctrl).UpdateGraphicsCurrent (type);
                    else
                        ;
                else
                    ;
            } else
            // нет ни одной вкладки
                ;

            if (!(m_dictFormFloat == null))
                foreach (KeyValuePair<int, Form> pair in m_dictFormFloat)
                    (pair.Value as FormMainFloat).UpdateGraphicsCurrent (type);
            else
                ;
        }

        private const int SW_SHOWNOACTIVATE = 4;
        private const int HWND_TOP = 0;
        private const uint SWP_NOACTIVATE = 0x0010;
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(
             int hWnd,      // window handle
             int hWndInsertAfter,   // placement-order handle
             int X,         // horizontal position
             int Y,         // vertical position
             int cx,        // width
             int cy,        // height
             uint uFlags);  // window positioning flags

        private void FormMain_Activated(object sender, EventArgs e)
        {
            Logging.Logg().Debug(@"FormMain::FormMain_Activated () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            if (панельГрафическихToolStripMenuItem.Checked == true)
            {
                ShowWindow(formGraphicsSettings.Handle, SW_SHOWNOACTIVATE);
                SetWindowPos(formGraphicsSettings.Handle.ToInt32(), HWND_TOP,
                    formGraphicsSettings.Left, formGraphicsSettings.Top, formGraphicsSettings.Width, formGraphicsSettings.Height,
                    SWP_NOACTIVATE);
            }
            else
                ;
        }

        private void параметрыПриложенияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0) {
                formParameters.ShowDialog (this, formGraphicsSettings.FontColor, formGraphicsSettings.BackgroundColor);
            } else
                ;
        }

        private void параметрыТГБийскToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                foreach (PanelTecViewStandard tv in m_listStandardTabs)
                    if (tv.m_tecView.m_tec.Type == StatisticCommon.TEC.TEC_TYPE.BIYSK)
                    {
                        if (!(formParametersTG == null))
                            //tv.tec.parametersTGForm.ShowDialog(this);
                            formParametersTG.ShowDialog(this);
                        else
                            Logging.Logg().Error(@"FormMain::параметрыТГБийскToolStripMenuItem_Click () - m_formParametersTG == null", Logging.INDEX_MESSAGE.NOT_SET);

                        break;
                    }
                    else
                        ;
            }
            else
                ;
        }

        private void изментьСоставПользовательToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[ID_ADDING_TAB.USERS].panel == null)
            {
                m_dictAddingTabs[ID_ADDING_TAB.USERS].panel = new PanelUser();
                m_dictAddingTabs[ID_ADDING_TAB.USERS].panel.SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
            }
            else
                ;
            видSubToolStripMenuItem_CheckedChanged(ID_ADDING_TAB.USERS, (sender as ToolStripMenuItem).Text //"Пользователи"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void изменитьПарольНССToolStripMenuItem_Click(object sender, EventArgs e)
        {
            изменитьПарольToolStripMenuItem_Click(sender, e, 0, Passwords.INDEX_ROLES.NSS);
        }

        private void изменитьПарольЛКДиспетчераToolStripMenuItem_Click(object sender, EventArgs e)
        {
            изменитьПарольToolStripMenuItem_Click(sender, e, 0, Passwords.INDEX_ROLES.LK_DISP);
        }

        private void изменитьПарольКоммерческогоДиспетчераToolStripMenuItem_Click(object sender, EventArgs e)
        {
            изменитьПарольToolStripMenuItem_Click(sender, e, 0, Passwords.INDEX_ROLES.COM_DISP);
        }

        private void изменитьПарольАдминистратораToolStripMenuItem_Click(object sender, EventArgs e)
        {
            изменитьПарольToolStripMenuItem_Click(sender, e, 0, Passwords.INDEX_ROLES.ADMIN);
        }

        private void изменитьПарольToolStripMenuItem_Click(object sender, EventArgs e, int id_ext, Passwords.INDEX_ROLES indx_role)
        {
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
                DbTSQLConfigDatabase.DbConfig ().Register();
                formPassword.SetIdPass(DbTSQLConfigDatabase.DbConfig ().ListenerId, id_ext, indx_role);
                DialogResult dlgRes = formPassword.ShowDialog(this);
                if (dlgRes == DialogResult.Yes)
                {
                    formSetPassword.SetIdPass(DbTSQLConfigDatabase.DbConfig ().ListenerId, 0, formPassword.GetIdRolePassword());
                    formSetPassword.ShowDialog(this);
                }
                else
                    if (dlgRes == DialogResult.Abort)
                        connectionSettings(CONN_SETT_TYPE.CONFIG_DB);
                    else
                        ;

                DbTSQLConfigDatabase.DbConfig ().UnRegister();
            }
            else
                ;
        }

        private void menuStrip_MenuActivate(object sender, EventArgs e)
        {
            activateTabPage(tclTecViews.SelectedIndex, false);
        }

        private ToolStripMenuItem getSelectedMenuItem(ToolStripMenuItem owner)
        {
            ToolStripMenuItem itemRes = null;

            foreach (ToolStripItem item in owner.DropDownItems)
            {
                if (item is ToolStripMenuItem)
                    if ((item as ToolStripMenuItem).DropDownItems.Count > 0 && item.Enabled == true)
                    {
                        itemRes = getSelectedMenuItem(item as ToolStripMenuItem);
                        if (!(itemRes == null))
                            break;
                        else
                            ;
                    }
                    else
                    {
                        if (item.Selected == true)
                        {
                            itemRes = item as ToolStripMenuItem;
                            break;
                        }
                        else
                            ;
                    }
                else
                    ;
            }

            return itemRes;
        }

        private void menuStrip_MenuDeactivate(object sender, EventArgs e)
        {
           bool bExit = false;
           ToolStripMenuItem selItem = null;

           foreach (ToolStripMenuItem item in ((MenuStrip)sender).Items)
           {
                if (item.DropDownItems.Count > 0 && item.Enabled == true)
                {
                    selItem = getSelectedMenuItem(item);
                    break;
                }
                else
                    ;
            }

           if ((!(selItem == null)) && (selItem.Text.Equals(@"Выход") == true))
                bExit = true;
            else
                ;

            if (bExit == false)
                activateTabPage(tclTecViews.SelectedIndex, true);
            else
                ;
        }

        private void contextMenuStrip_Closed(object sender, System.Windows.Forms.ToolStripDropDownClosedEventArgs e)
        {
            if (!(e.CloseReason == ToolStripDropDownCloseReason.ItemClicked))
                activateTabPage(tclTecViews.SelectedIndex, true);
            else
                ;
        }

        //FormChangeMode.MANAGER modePanelAdmin {
        //    get {
        //        FormChangeMode.MANAGER modeRes = FormChangeMode.MANAGER.UNKNOWN;

        //        if (formChangeMode.admin_was_checked == true) {
        //            switch (HStatisticUsers.Role) {
        //                case HStatisticUsers.ID_ROLES.ADMIN:
        //                    if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
        //                        modeRes = FormChangeMode.MANAGER.DISP;
        //                    else
        //                        modeRes = FormChangeMode.MANAGER.NSS;
        //                    break;
        //                case HStatisticUsers.ID_ROLES.KOM_DISP:
        //                    modeRes = FormChangeMode.MANAGER.DISP;
        //                    break;
        //                case HStatisticUsers.ID_ROLES.NSS:
        //                    modeRes = FormChangeMode.MANAGER.NSS;
        //                    break;
        //                default:
        //                    break;
        //            }

        //            return modeRes;
        //        }
        //        else
        //            return FormChangeMode.MANAGER.DISP;
        //    }
        //}

        //protected override void WndProc(ref Message m)
        //{
        //    base.WndProc(ref m);

        //    //Logging.Logg().Debug(@"FormMain::WndProc () - msg.ID=" + m.Msg + @", msg.Res=" + m.Result, Logging.INDEX_MESSAGE.NOT_SET);
        //    Console.WriteLine(@"FormMain::WndProc () - msg.ID=" + m.Msg + @", msg.Res=" + m.Result);
        //}
        /// <summary>
        /// Класс для обеспечения немедленного обновления дочерних панелей
        ///  при изменении масштаба отображения (по двойному нажатию на панели)
        /// </summary>
        private class FormMainFloat : FormMainFloatBase
        {
            public FormMainFloat(string text, Panel panel, bool bLabel)
                : base(text, panel, bLabel)
            {
            }

            /// <summary>
            /// Обновить текущее отображение в ~ от типа изменения
            /// </summary>
            /// <param name="type">Тип изменения параметров отображения</param>
            public void UpdateGraphicsCurrent(int type)
            {
                BackColor = formGraphicsSettings.BackgroundColor;
                ForeColor = formGraphicsSettings.FontColor;

                Panel panel = GetPanel();

                /*
                if (panel is PanelTecViewStandard)
                    (panel as PanelTecViewStandard).UpdateGraphicsCurrent(type);
                else
                    if (panel is PanelCustomTecView)
                        (panel as PanelCustomTecView).UpdateGraphicsCurrent(type);
                    else
                        ;
                */
                (panel as PanelStatistic).UpdateGraphicsCurrent (type);
            }
        }
    }
}