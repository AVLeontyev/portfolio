using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

using StatisticCommon;
using ASUTP;
using ASUTP.Core;
using ASUTP.Database;
using ASUTP.Helper;

namespace StatisticTrans
{
    /// <summary>
    /// Класс "Главная форма Trans (Передача?)"
    /// </summary>
    public abstract partial class FormMainTrans : FormMainStatistic
    {
        public enum INDEX_APP_SETTINS { }
        /// <summary>
        /// Объект для чтения/хранения параметров конфигурации приложения
        /// </summary>
        //protected static FileINI m_sFileINI; // setup.ini

        protected ComponentTesting CT;

        private const Int32 TIMER_SERVICE_MIN_INTERVAL = 66666;
        /// <summary>
        /// //Перечисление "Режим машины" (интерактивный,дата, сервис,неизвестный)
        /// </summary>
        protected enum MODE_MASHINE : ushort { INTERACTIVE, SERVICE_TO_DATE, SERVICE_PERIOD, SERVICE_ON_EVENT, UNKNOWN };
        /// <summary>
        ///Перечисление "Типы настроек"
        /// </summary>
        public enum CONN_SETT_TYPE : short { SOURCE, DEST, COUNT_CONN_SETT_TYPE };
        /// <summary>
        ///Перечисление "Управление" (IP сервера,порт, имя БД, ID пользователя, пароль, кол-во эл-ов управления)
        /// </summary>
        protected enum INDX_UICONTROLS { SERVER_IP, PORT, NAME_DATABASE, USER_ID, PASS, COUNT_INDX_UICONTROLS };
        /// <summary>
        /// Двумерный массив типа Control
        /// </summary>
        protected System.Windows.Forms.Control[,] m_arUIControls;

        protected
            //Таймер вызывает событие через определенный интервал времени
            System.Windows.Forms.Timer
            //System.Threading.Timer
                timerService
                ;
        //protected handlerCmd m_hCmd;
        protected HAdmin[] m_arAdmin;
        protected GroupBox[] m_arGroupBox;

        protected DataGridViewAdmin m_dgwAdminTable;

        protected FormChangeMode.MODE_TECCOMPONENT m_modeTECComponent;

        //protected MODE_MASHINE s_modeMashine = MODE_MASHINE.INTERACTIVE;

        protected CheckBox m_checkboxModeMashine;

        public static Label m_labelTime;

        //protected HMark m_markQueries;

        protected bool IsService
        {
            get
            {
                //return WindowState == FormWindowState.Minimized ? true : false;
                //return ((WindowState == FormWindowState.Minimized) && (ShowInTaskbar == false) && (notifyIconMain.Visible == true));

                //return !timerMain.Enabled;

                return ((handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_TO_DATE)
                    || (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_PERIOD)
                    || (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_ON_EVENT))
                        ? true
                            : false;
            }
        }
        /// <summary>
        /// Признак включения/блокировки элементов управления на форме
        /// </summary>
        protected bool m_bEnabledUIControl = true;

        protected Int16 m_IndexDB
        {
            get
            {
                CONN_SETT_TYPE i;
                for (i = CONN_SETT_TYPE.SOURCE; i < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                {
                    if (m_arGroupBox[(Int16)i].BackColor == SystemColors.Info)
                        break;
                }

                return (Int16)i;
            }

            //set;
        }

        //private string getINIParametersOfID(int param)
        //{
        //    return m_sFileINI.GetValueOfID(param);
        //}

        //private string getINIParametersOfKey(string keyParam)
        //{
        //    return m_sFileINI.GetMainValueOfKey(keyParam);
        //}

        private string getINIParametersOfID(int id)
        {
            return
                //m_sFileINI.GetMainValueOfKey(FormParameters.GetNameParametersOfIndex(id))
                FileAppSettings.This ().GetValueOfMainIndexParameter(id);
                ;
        }

        /// <summary>
        /// Класс обработки "своих" команд
        /// </summary>
        protected class handlerCmd : HCmd_Arg
        {
            private enum CMD_PARAMETER { unknown = -1, start, minimize, stop, service, date, debug }

            private Dictionary<CMD_PARAMETER, Func<string, bool>> _dictCommandParameterHandler;

            private static string _throwMessage;

            private static MODE_MASHINE s_modeServiceMashineDefault = MODE_MASHINE.SERVICE_ON_EVENT;
            private static MODE_MASHINE _modeMashine;

            private static DateTime _date;

            private static int _timerInterval;

            protected static bool _debugTurn;

            public static string ThrowMessage { get { return _throwMessage; } }

            public static MODE_MASHINE ModeMashine { get { return _modeMashine; } }

            public static DateTime Date { get { return _date; } }

            public static int TimerInterval { get { return _timerInterval; } }

            public static bool DebugTurn { get { return _debugTurn; } }

            public static void IncTimerInterval ()
            {
                _timerInterval++;
            }

            public static void SetModeMashine (MODE_MASHINE newMode)
            {
                _modeMashine = newMode;
            }

            public static void SetModeServiceMashineDefault ()
            {
                SetModeMashine(s_modeServiceMashineDefault);
            }

            /// <summary>
            /// Конструктор - основной (с параметрами)
            /// </summary>
            /// <param name="id_app">Идентификатор приложения из файла конфигурации</param>
            /// <param name="args">Массив аргументов командной строки</param>
            public handlerCmd (ProgramBase.ID_APP id_app, string [] args)
                : base(args)
            {
                CMD_PARAMETER cmdKey = CMD_PARAMETER.unknown;
                int timerInterval = -1;
                s_modeServiceMashineDefault = id_app == ProgramBase.ID_APP.TRANS_MODES_CENTRE
                    ? MODE_MASHINE.SERVICE_ON_EVENT
                        : MODE_MASHINE.SERVICE_PERIOD;

                _modeMashine = MODE_MASHINE.INTERACTIVE;
                _date = DateTime.Now.Date;
                _timerInterval = TIMER_SERVICE_MIN_INTERVAL; //Милисекунды

                _dictCommandParameterHandler = new Dictionary<CMD_PARAMETER, Func<string, bool>> {
                    // 'start', 'stop' обрабытываютяс в родительском классе
                    { CMD_PARAMETER.start, delegate (string arg) {
                        bool bRes = true;

                        _modeMashine = s_modeServiceMashineDefault;
                        _timerInterval = TIMER_SERVICE_MIN_INTERVAL;

                        return bRes;
                    } }
                    // обработка параметра 'service'
                    , { CMD_PARAMETER.service, delegate (string arg) {
                        bool bRes = true;

                        _modeMashine = s_modeServiceMashineDefault;

                        if ((arg.Equals(string.Empty) == true)
                            || (arg.Equals(@"default") == true))
                        // оставить значение 'm_arg_interval' по умолчанию
                            ;
                        else if(Int32.TryParse(arg, out timerInterval) == true) {
                        // указано и распознано значение интервала
                            // указать корректное значение итнтервала
                            if (!(timerInterval < TIMER_SERVICE_MIN_INTERVAL)) {
                                _modeMashine = MODE_MASHINE.SERVICE_PERIOD;
                                _timerInterval = timerInterval;
                            } else {
                                _modeMashine = MODE_MASHINE.UNKNOWN;
                                _throwMessage = "Интервал задан меньше необходимого значения";
                            }
                        } else if (arg.Equals (@"on_event") == true) {
                        // режим работы по событиям Модес-Центр
                            _modeMashine = MODE_MASHINE.SERVICE_ON_EVENT;
                            _timerInterval = TIMER_SERVICE_MIN_INTERVAL;
                        } else {
                            _throwMessage = $"FormMain::RunCmd() - неизвестный сервисный режим <{arg}> работы...";
                            _modeMashine = MODE_MASHINE.UNKNOWN;
                        }

                        return bRes;
                    } }
                    // обработка параметра 'date'
                    , { CMD_PARAMETER.date, delegate (string arg) {
                        bool bRes = true;

                        _modeMashine = MODE_MASHINE.SERVICE_TO_DATE;

                        if (arg == "default")
                            _date = DateTime.Now.AddDays(1);
                        else if (arg == "now")
                            _date = DateTime.Now;
                        else if (DateTime.TryParse(arg, out _date) == false)
                            _date = DateTime.Now;
                        else
                            ;

                        return bRes;
                    } }
                    // обработка
                    , { CMD_PARAMETER.debug, delegate (string arg) {
                        bool bRes = true;

                        if (bool.TryParse (arg, out _debugTurn) == false)
                            _debugTurn = false;
                        else
                            ;

                        return bRes;
                    } }
                };

                foreach (CMD_PARAMETER parameter in Enum.GetValues (typeof (CMD_PARAMETER))) {
                    if ((parameter == CMD_PARAMETER.unknown)
                        //|| (parameter == CMD_PARAMETER.start) обработано, но назначается режим работы по умолчанию
                        || (parameter == CMD_PARAMETER.minimize)
                        || (parameter == CMD_PARAMETER.stop))
                    // обработан ранее в родительском классе
                        continue;
                    else if (_dictCommandParameterHandler.ContainsKey (parameter) == true)
                    // словарь обработчиков содержит требуемый метод
                        if (m_dictCmdArgs.ContainsKey (parameter.ToString()) == true)
                        // указан в командной строке - вызвать метод
                            _dictCommandParameterHandler [parameter] (m_dictCmdArgs[parameter.ToString()]);
                        else
                        // проверить наличие ключа в файле конфигурации
                            if (FileAppSettings.This ().IsNotNullOrEmpty(parameter.ToString ()) == true)
                            // прочитать из файла конфигурации - вызвать метод
                                _dictCommandParameterHandler [parameter] (FileAppSettings.This().GetValue(parameter.ToString()));
                            else
                            // в файле конфигурации параметр тоже не указан
                                ;
                    else
                    // для параметра не указан обработчик
                        Logging.Logg ().Warning ($"FormMainTrans.handlerCmd::ctor () - для параметра <{parameter}> не указан обработчик..."
                            , Logging.INDEX_MESSAGE.NOT_SET);
                }

                foreach (KeyValuePair<string, string> pair in m_dictCmdArgs) {
                    if (Enum.TryParse<CMD_PARAMETER> (pair.Key, out cmdKey) == false)
                    // параметр не распознан как известный к обработке
                        Logging.Logg ().Warning ($"FormMainTrans.handlerCmd::ctor () - не известный параметр <{pair.Key}>, значение=<{pair.Value}>..."
                            , Logging.INDEX_MESSAGE.NOT_SET);
                    else
                        ;
                }
            }
        }

        public FormMainTrans (Func<ProgramBase.ID_APP> fGettingIdApplication, KeyValuePair<string, string> [] config)
            : this (fGettingIdApplication(), config)
        {
        }

        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="id_app">Идентификатор приложения из файла конфигурации</param>
        /// <param name="par">Наименования-ключи параметров для файла конфигурации</param>
        /// <param name="val">Значения для параметров в файле конфигурации</param>
        public FormMainTrans(ProgramBase.ID_APP id_app, KeyValuePair<string, string>[]config)
            : base(id_app)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo; //ru-Ru 

            InitializeComponent();

            m_report = new HReports();

            //DelegateGetINIParametersOfID = new StringDelegateIntFunc(GetINIParametersOfID);
            Logging.DelegateGetINIParametersOfID = new StringDelegateIntFunc(getINIParametersOfID);

            //m_sFileINI = new FileINI(@"setup.ini"
            //    , false
            //    , (from pair in config select pair.Key).ToArray()
            //    , (from pair in config select pair.Value).ToArray());
            FileAppSettings.This ().AddRequired (config
                .Concat(new KeyValuePair<string, string> [] {
                    //new KeyValuePair<string, string> (@"service", "126667") ??? сервис указывается только в командной строке
                }) 
            );

            string keyPar = string.Empty
                , valDefPar = string.Empty;

            s_iMainSourceData = Int32.Parse(FileAppSettings.This ().GetValueOfMainIndexParameter(FormParameters.PARAMETR_SETUP.MAIN_DATASOURCE));

            //Вариант №1
            keyPar = @"iapp"; valDefPar = id_app.ToString();
            ProgramBase.s_iAppID = id_app; // FileAppSettings.This ().GetValue(keyPar)
            //Вариант №2
            //ProgramBase.s_iAppID = id_app;

            ////Если ранее тип логирования не был назанчен...
            //if (Logging.s_mode == Logging.LOG_MODE.UNKNOWN)
            //{
            //    //назначить тип логирования - БД
            //    Logging.s_mode = Logging.LOG_MODE.DB;
            //}
            //else { }

            //if (Logging.s_mode == Logging.LOG_MODE.DB)
            //{
            //    //Инициализация БД-логирования
            //    int err = -1;
            //    StatisticCommon.Logging.ConnSett = new ConnectionSettings(InitTECBase.getConnSettingsOfIdSource(TYPE_DATABASE_CFG.CFG_200, idListenerConfigDB, s_iMainSourceData, -1, out err).Rows[0]);
            //}
            //else { }

            this.Text =
            this.notifyIconMain.Text = @"Статистика: " + FileAppSettings.This ().GetValue(@"ОкноНазначение");

            m_listID_TECNotUse = new List<int>();
            string[] arStrID_TECNotUse = FileAppSettings.This ().GetValue(@"ID_TECNotUse").Split(',');

            foreach (string str in arStrID_TECNotUse)
            {
                if (str.Equals(string.Empty) == false)
                    m_listID_TECNotUse.Add(Int32.Parse(str));
                else
                    ;
            }
            try
            {
                string strChecked = string.Empty;
                bool bRes = false
                    , bChecked = false;
                strChecked = FileAppSettings.This ().GetValue (@"ОпросСохранениеППБР");
                if (bool.TryParse(strChecked.Split(',')[0], out bChecked) == true)
                    ОпросППБРToolStripMenuItem.Checked = bChecked;
                else
                    ОпросППБРToolStripMenuItem.Checked = false;
                if (bool.TryParse(strChecked.Split(',')[1], out bChecked) == true)
                    СохранППБРToolStripMenuItem.Checked = bChecked;
                else
                    СохранППБРToolStripMenuItem.Checked = false;

                bRes =
                bChecked =
                    false;
                strChecked = FileAppSettings.This ().GetValue (@"ОпросСохранениеАдминЗнач");
                if (bool.TryParse(strChecked.Split(',')[0], out bChecked) == true)
                    ОпросАдминЗначенияToolStripMenuItem.Checked = bChecked;
                else
                    ОпросАдминЗначенияToolStripMenuItem.Checked = false;
                if (bool.TryParse(strChecked.Split(',')[1], out bChecked) == true)
                    СохранАдминЗначенияToolStripMenuItem.Checked = bChecked;
                else
                    СохранАдминЗначенияToolStripMenuItem.Checked = false;
            }
            catch (Exception e)
            {
                throw new Exception(@"FormMainTrans::нет возможности установить перечень опрашиваемых/сохранямых параметров...");
            }

            if ((ОпросАдминЗначенияToolStripMenuItem.Checked == false) &&
                (ОпросППБРToolStripMenuItem.Checked == false))
            {
                throw new Exception(@"FormMainTrans::не определен перечень опрашиваемых/сохранямых параметров...");
            }
            else
                ;

            // m_statusStripMain
            this.m_statusStripMain.Location = new System.Drawing.Point(0, 546);
            this.m_statusStripMain.Size = new System.Drawing.Size(841, 22);
            // m_lblMainState
            this.m_lblMainState.Size = new System.Drawing.Size(166, 17);
            // m_lblDateError
            this.m_lblDateMessage.Size = new System.Drawing.Size(166, 17);
            // m_lblDescError
            this.m_lblDescMessage.Size = new System.Drawing.Size(463, 17);

            notifyIconMain.Click += new EventHandler(notifyIconMain_Click);

            //this.Deactivate += new EventHandler(FormMainTrans_Deactivate);

            this.m_checkboxModeMashine = new CheckBox();
            this.m_checkboxModeMashine.Name = "m_checkboxModeMashine";
            this.m_checkboxModeMashine.Text = "Фоновый режим";
            this.m_checkboxModeMashine.Location = new System.Drawing.Point(13, 516);
            this.m_checkboxModeMashine.Size = new System.Drawing.Size(123, 23);
            //this.m_checkboxModeMashine.CheckAlign = ContentAlignment.;
            this.m_checkboxModeMashine.TextAlign = ContentAlignment.MiddleLeft;
            this.m_checkboxModeMashine.CheckedChanged += new EventHandler(checkboxModeMashine_CheckedChanged);
            this.Controls.Add(this.m_checkboxModeMashine);
            //Пока переходить из режима в режим пользователь НЕ может (нестабильная работа trans_tg.exe) ???
            this.m_checkboxModeMashine.Enabled = false; ;
            //labelTime
            m_labelTime = new Label();
            m_labelTime.Name = "m_labelTime";
            m_labelTime.Location = new System.Drawing.Point(150, 520);
            m_labelTime.Size = new System.Drawing.Size(580, 15);
            m_labelTime.Text = "";
            Controls.Add(m_labelTime);
            m_labelTime.Visible = true;

            Logging.LinkId (Logging.INDEX_MESSAGE.D_001, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGPBRNUMBER);
            Logging.LinkId (Logging.INDEX_MESSAGE.D_002, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGQUERY);
            Logging.UpdateMarkDebugLog ();

            //Значения аргументов 'Date', 'TimerInterval' по умолчанию в 'handlerCmd'

            setDatetimePicker (handlerCmd.Date);

            m_arGroupBox = new GroupBox[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { groupBoxSource, groupBoxDest };
            delegateEvent = new DelegateFunc(EventRaised);

            if (handlerCmd.ModeMashine == MODE_MASHINE.UNKNOWN)
                throw new Exception(handlerCmd.ThrowMessage);
            else
                if (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_TO_DATE)
                    enabledUIControl(false);
                else
                {
                    enabledUIControl(true);

                    if ((handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_PERIOD)
                        || (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_ON_EVENT)) {
                        this.WindowState = FormWindowState.Minimized;
                        this.ShowInTaskbar = false;
                        this.notifyIconMain.Visible = true;
                    } else
                        ;
                }
        }

        /// <summary>
        /// Создание объекта-обработчика аргументов командной строки
        /// </summary>
        /// <param name="id_app">Идентификатор приложения из файла конфигурации</param>
        /// <param name="args">Массив аргументов командной строки</param>
        /// <returns>Объект-обработчик аргументов командной строки</returns>
        protected override HCmd_Arg createHCmdArg(ProgramBase.ID_APP id_app, string [] args)
        {
            return new handlerCmd(id_app, args);
        }

        private void InitializeComponentTrans()
        {
            int i = -1;

            i = (Int16)INDX_UICONTROLS.PORT;
            ((System.ComponentModel.ISupportInitialize)(m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i])).BeginInit();

            for (i = 0; i < 2 * (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS; i++)
            {
                this.groupBoxDest.Controls.Add(m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i]);
            }

            i = (Int16)INDX_UICONTROLS.PORT + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelDestPort
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(11, 57);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "labelDestPort";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(32, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 31;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Text = "Порт";

            i = (Int16)INDX_UICONTROLS.PORT;
            // 
            // nudnDestPort
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(128, 55);
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i]).Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "nudnDestPort";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(69, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 26;
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i]).TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i]).Value = new decimal(new int[] {
            3306,
            0,
            0,
            0});

            i = (Int16)INDX_UICONTROLS.PASS + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelDestPass
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(10, 136);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "labelDestPass";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(45, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 34;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Text = "Пароль";

            i = (Int16)INDX_UICONTROLS.USER_ID + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelDestUserID
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(10, 110);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "labelDestUserID";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(103, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 33;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Text = "Имя пользователя";

            i = (Int16)INDX_UICONTROLS.NAME_DATABASE + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelDestNameDatabase
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(10, 84);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "labelDestNameDatabase";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(98, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 32;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Text = "Имя базы данных";

            i = (Int16)INDX_UICONTROLS.SERVER_IP + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelDestServerIP
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(10, 30);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "labelDestServerIP";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(95, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 30;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Text = "IP адрес сервера";

            i = (Int16)INDX_UICONTROLS.PASS;
            // 
            // mtbxDestPass
            //
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(128, 133);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "mtbxDestPass";
            ((MaskedTextBox)m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i]).PasswordChar = '#';
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 29;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TextChanged += new System.EventHandler(this.component_Changed);

            i = (Int16)INDX_UICONTROLS.USER_ID;
            // 
            // tbxDestUserId
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(128, 107);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "tbxDestUserId";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 28;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TextChanged += new System.EventHandler(this.component_Changed);

            i = (Int16)INDX_UICONTROLS.NAME_DATABASE;
            // 
            // tbxDestNameDatabase
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(128, 81);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "tbxDestNameDatabase";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 27;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TextChanged += new System.EventHandler(this.component_Changed);

            i = (Int16)INDX_UICONTROLS.SERVER_IP;
            // 
            // tbxDestServerIP
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Location = new System.Drawing.Point(128, 27);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Name = "tbxDestServerIP";
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TabIndex = 25;
            m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i].TextChanged += new System.EventHandler(this.component_Changed);

            i = (Int16)INDX_UICONTROLS.PORT;
            ((System.ComponentModel.ISupportInitialize)(m_arUIControls[(Int16)CONN_SETT_TYPE.DEST, i])).EndInit();

            this.Deactivate += new EventHandler(onDeactivate);
        }

        protected void InitializeComponentTransDB()
        {
            int i = -1;

            //m_arUIControls = new System.Windows.Forms.Control[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE, 2 * (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROLS]
            m_arUIControls = new System.Windows.Forms.Control[,]
            { { new System.Windows.Forms.TextBox(), new System.Windows.Forms.NumericUpDown(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.MaskedTextBox(),
                new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label() },
            { new System.Windows.Forms.TextBox(), new System.Windows.Forms.NumericUpDown(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.MaskedTextBox(),
                new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label() } };

            InitializeComponentTrans();

            i = (Int16)INDX_UICONTROLS.PORT;
            ((System.ComponentModel.ISupportInitialize)(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i])).BeginInit();

            for (i = 0; i < 2 * (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS; i++)
            {
                this.groupBoxSource.Controls.Add(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]);
            }

            i = (Int16)INDX_UICONTROLS.PORT + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourcePort
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(12, 55);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourcePort";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(32, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 21;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "Порт";

            i = (Int16)INDX_UICONTROLS.PORT;
            // 
            // nudnSourcePort
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(129, 53);
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "nudnSourcePort";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(69, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 16;
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            ((NumericUpDown)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).Value = new decimal(new int[] {
            3306,
            0,
            0,
            0});

            i = (Int16)INDX_UICONTROLS.PASS + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourcePass
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 134);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourcePass";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(45, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 24;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "Пароль";

            i = (Int16)INDX_UICONTROLS.USER_ID + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourceUserId
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 108);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourceUserId";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(103, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 23;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "Имя пользователя";

            i = (Int16)INDX_UICONTROLS.NAME_DATABASE + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourceNameDatabase
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 82);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourceNameDatabase";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(98, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 22;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "Имя базы данных";

            i = (Int16)INDX_UICONTROLS.SERVER_IP + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourceServerIP
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 28);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourceServerIP";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(95, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 20;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "IP адрес сервера";

            i = (Int16)INDX_UICONTROLS.PASS;
            // 
            // mtbxSourcePass
            //
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(129, 131);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "mtbxSourcePass";
            ((MaskedTextBox)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).PasswordChar = '#';
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 19;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TextChanged += new System.EventHandler(component_Changed);

            i = (Int16)INDX_UICONTROLS.USER_ID;
            // 
            // tbxSourceUserId
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(129, 105);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "tbxSourceUserId";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 18;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TextChanged += new System.EventHandler(component_Changed);

            i = (Int16)INDX_UICONTROLS.NAME_DATABASE;
            // 
            // tbxSourceNameDatabase
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(129, 79);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "tbxSourceNameDatabase";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 17;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TextChanged += new System.EventHandler(this.component_Changed);

            i = (Int16)INDX_UICONTROLS.SERVER_IP;
            // 
            // tbxSourceServerIP
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(129, 25);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "tbxSourceServerIP";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(160, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 15;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TextChanged += new System.EventHandler(this.component_Changed);

            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();
            i = (Int16)INDX_UICONTROLS.PORT;
            ((System.ComponentModel.ISupportInitialize)(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i])).EndInit();
        }

        protected abstract void buttonSaveSourceSett_Click(object sender, EventArgs e);

        //protected override void UpdateActiveGui(int type) { }
        //protected override void HideGraphicsSettings() { }

        protected void InitializeComponentTransSrc(string text)
        {
            int i = -1;

            m_arUIControls = new System.Windows.Forms.Control[,]
            { { new System.Windows.Forms.TextBox(), new Button (), null, null, null,
                new System.Windows.Forms.Label(), null, null, null, null },
            { new System.Windows.Forms.TextBox(), new System.Windows.Forms.NumericUpDown(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.TextBox(), new System.Windows.Forms.MaskedTextBox(),
                new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label(), new System.Windows.Forms.Label() } };

            InitializeComponentTrans();

            for (i = 0; i < 2 * (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS; i++)
            {
                if (!(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i] == null))
                    this.groupBoxSource.Controls.Add(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]);
            }

            i = (Int16)INDX_UICONTROLS.SERVER_IP + (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS;
            // 
            // labelSourcePathExcel
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].AutoSize = true;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 28);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "labelSourceSett";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(95, 13);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 20;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = text;

            i = (Int16)INDX_UICONTROLS.SERVER_IP;
            // 
            // tbxSourcePathExcel
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(11, 55);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "tbxSourceSett";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(243, 20);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 15;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TextChanged += new System.EventHandler(this.component_Changed);
            ((TextBox)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).ReadOnly = true;

            i = (Int16)INDX_UICONTROLS.PORT;
            // 
            // buttonPathExcel
            // 
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Location = new System.Drawing.Point(257, 53);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Name = "buttonSaveSourceSett";
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Size = new System.Drawing.Size(29, 23);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].TabIndex = 2;
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Text = "...";
            ((Button)m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i]).UseVisualStyleBackColor = true;
            //m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Click += new System.EventHandler(...);
            m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, i].Enabled = false;

            //Идентичный код с панелью Modes-Centre
            buttonSourceExport.Location = new System.Drawing.Point(8, 86);

            buttonSourceSave.Location = new System.Drawing.Point(151, 86);
            buttonSourceSave.Click -= buttonSourceSave_Click;
            buttonSourceSave.Click += new EventHandler(this.buttonSaveSourceSett_Click);
            buttonSourceSave.Enabled = false;

            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();

            this.groupBoxSource.Size = new System.Drawing.Size(300, 120);

            this.groupBoxDest.Location = new System.Drawing.Point(3, 196);

            panelMain.Size = new System.Drawing.Size(822, 404);

            //base.buttonClose.Anchor = AnchorStyles.Left;
            buttonClose.Location = new System.Drawing.Point(733, 434);

            this.Size = new System.Drawing.Size(849, 514);

            this.m_checkboxModeMashine.Location = new System.Drawing.Point(13, 434);
            m_labelTime.Location = new System.Drawing.Point(150, 437);
        }

        protected List<int> m_listID_TECNotUse;

        protected abstract void start();

        protected override void Start()
        {
            initTableHourRows();

            base.Start();
        }

        protected void RemoveTEC(HAdmin admin)
        {
            foreach (int id in m_listID_TECNotUse)
            {
                admin.RemoveTEC(id);
            }
        }

        private void enabledUIControl(bool enabled)
        {
            for (int i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                //m_arGroupBox[i].Enabled = enabled;
                if (!(m_arGroupBox[i].Enabled == enabled)) m_arGroupBox[i].Enabled = enabled; else ;
            }

            if (!(dateTimePickerMain.Enabled == enabled)) dateTimePickerMain.Enabled = enabled; else ;
            if (!(comboBoxTECComponent.Enabled == enabled)) comboBoxTECComponent.Enabled = enabled; else ;
            //Пока переходить из режима в режимпользователь НЕ может (нестабильная работа trans_tg.exe) ???
            //if (!(m_checkboxModeMashine.Enabled == enabled)) m_checkboxModeMashine.Enabled = enabled; else ;

            if (enabled)
            {
                comboBoxTECComponent.SelectedIndexChanged += new EventHandler(comboBoxTECComponent_SelectedIndexChanged);
                dateTimePickerMain.ValueChanged += new EventHandler(dateTimePickerMain_Changed);
            }
            else
            {
                comboBoxTECComponent.SelectedIndexChanged -= comboBoxTECComponent_SelectedIndexChanged;
                dateTimePickerMain.ValueChanged -= dateTimePickerMain_Changed;
            }

            m_bEnabledUIControl = enabled;
        }

        protected void setUIControlConnectionSettings(int i)
        {
            if (!(comboBoxTECComponent.SelectedIndex < 0)
                && (SelectedItemKey.Id > 0))
            {
                ConnectionSettings connSett = m_arAdmin[i].FindTECComponent(SelectedItemKey).tec.connSetts[(int)StatisticCommon.CONN_SETT_TYPE.PBR];
                for (int j = 0; j < (Int16)INDX_UICONTROLS.COUNT_INDX_UICONTROLS; j++)
                {
                    switch (j)
                    {
                        case (Int16)FormMainTrans.INDX_UICONTROLS.SERVER_IP:
                            ((TextBox)m_arUIControls[i, j]).Text = connSett.server;
                            break;
                        case (Int16)INDX_UICONTROLS.PORT:
                            //if (m_arUIControlDB[i, j].Enabled == true)
                            ((NumericUpDown)m_arUIControls[i, j]).Text = connSett.port.ToString();
                            //else
                            //    ;
                            break;
                        case (Int16)INDX_UICONTROLS.NAME_DATABASE:
                            ((TextBox)m_arUIControls[i, j]).Text = connSett.dbName;
                            break;
                        case (Int16)INDX_UICONTROLS.USER_ID:
                            ((TextBox)m_arUIControls[i, j]).Text = connSett.userName;
                            break;
                        case (Int16)INDX_UICONTROLS.PASS:
                            ((MaskedTextBox)m_arUIControls[i, j]).Text = connSett.password;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        protected abstract void setUIControlSourceState();

        protected void enabledButtonSourceExport(bool enabled)
        {
            buttonSourceExport.Enabled = enabled;
        }

        /// <summary>
        /// Создание формы для редактирования параметров соединения с БД конфигурации
        /// </summary>
        /// <param name="connSettFileName">наименование файла с параметрами соединения</param>
        /// <param name="bCheckAdminLength">признак проверки кол-ва параметров соединения по кол-ву объекьлв 'HAdmin'</param>
        protected void EditFormConnectionSettings(string connSettFileName, bool bCheckAdminLength)
        {
            CreatefileConnSett(connSettFileName);

            bool bShowFormConnSett = false;

            if (s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
                ;
            else
                bShowFormConnSett = true;

            if (bCheckAdminLength == true)
            {
                if (s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].Count < m_arAdmin.Length)
                {
                    while (s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].Count < m_arAdmin.Length)
                        s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].addConnSett(new ConnectionSettings());

                    if (bShowFormConnSett == false) bShowFormConnSett = true; else ;
                }
                else
                    ;
            }
            else
            {
            }

            if (bShowFormConnSett == true)
                конфигурацияБДToolStripMenuItem.PerformClick();
            else
                new DbTSQLConfigDatabase (s_listFormConnectionSettings [(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].getConnSett (0));
            ;
        }

        /// <summary>
        /// Заполнение списка компонентов
        /// </summary>
        protected virtual void FillComboBoxTECComponent(FormChangeMode.MODE_TECCOMPONENT mode, bool bWithNameTECOwner)
        {
            List<FormChangeMode.KeyDevice> listKey;

            comboBoxTECComponent.Items.Clear();

            listKey = ((AdminTS)m_arAdmin [(Int16)CONN_SETT_TYPE.DEST]).GetListKeyTECComponent (mode, true);

            listKey.ForEach(key =>
                comboBoxTECComponent.Items.Add(new ComboBoxItem () { Tag = key, Text = ((AdminTS)m_arAdmin [(Int16)CONN_SETT_TYPE.DEST]).GetNameTECComponent(key, bWithNameTECOwner) }));

            if (comboBoxTECComponent.Items.Count > 0)
                comboBoxTECComponent.SelectedIndex = 0;
            else
                ;
        }

        /// <summary>
        /// Заполнение таблицы полученными данными (при [авто]режиме экспорт данных + переход к следующему элементу списка компонентов)
        /// </summary>
        /// <param name="date">дата</param>
        /// <param name="bNewValues">Признак наличия новых значений, иначе требуется изменить оформление представления</param>
        protected virtual void setDataGridViewAdmin(DateTime date, bool bNewValues)
        {
            if ((IsService == true)
                && (m_bEnabledUIControl == false))
            {
                try
                {
                    //Копирование данных из массива одного объекта (SOURCE) в массив другого объекта (DEST)
                    m_arAdmin[(int)CONN_SETT_TYPE.DEST].getCurRDGValues(m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]);
                    //((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).m_bSavePPBRValues = true;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"FormMainTrans::setDataGridView (CurrentKey={0}) - ...", m_arAdmin[(int)CONN_SETT_TYPE.SOURCE].CurrentKey.ToString()), Logging.INDEX_MESSAGE.NOT_SET);
                }

                //SaveRDGValues (false);
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateBoolFunc(SaveRDGValues), false);
                else
                    Logging.Logg().Error(@"FormMainTrans::setDataGridViewAdmin () - ... BeginInvoke (SaveRDGValues) - ...", Logging.INDEX_MESSAGE.D_001);

                //this.BeginInvoke(new DelegateFunc(trans_auto_next));
            }
            else
            {
                updateDataGridViewAdmin(date, bNewValues);
            }
        }

        ///// <summary>
        ///// отчет об итерациях
        ///// </summary>
        //private void CounterSuccessfulDownload()
        //{
        //    m_labelTime.Invoke(new Action(() => m_labelTime.Text = "Время крайнего опроса: " + DateTime.Now.ToString() + ";" + " Успешных итераций: " + CT.currentIter + " из " + CT.GetNum() + ""));
        //    m_labelTime.Invoke(new Action(() => m_labelTime.Update()));
        //}

        /// <summary>
        /// При [авто]режиме переход к следующему элементу списка компонентов
        /// </summary>
        protected virtual void errorDataGridViewAdmin(int state)
        {
            if ((IsService == true)
                && (m_bEnabledUIControl == false))
            {
                m_arAdmin [(int)CONN_SETT_TYPE.SOURCE].TECComponentComplete (state, false);

                CT.ErrorIter();

                IAsyncResult asyncRes;
                if (IsService == true)
                    if (InvokeRequired == true)
                        if (IsHandleCreated == true)
                            asyncRes = this.BeginInvoke (new DelegateFunc (trans_auto_next));
                        else
                            Logging.Logg ().Error (@"FormMainTrans::errorDataGridViewAdmin () - ... BeginInvoke (trans_auto_next) - ...", Logging.INDEX_MESSAGE.D_001);
                    else
                        trans_auto_next ();
                else
                    ;
            }
            else ;
        }

        protected abstract void updateDataGridViewAdmin(DateTime date, bool bNewValues);

        protected void initTableHourRows(/*int indx = индекс для m_arAdmin*/)
        {
            for (CONN_SETT_TYPE type = (CONN_SETT_TYPE)0; type < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; type++)
                m_arAdmin[(int)type].m_curDate = dateTimePickerMain.Value;

            if (dateTimePickerMain.Value.Date.Equals(HAdmin.SeasonDateTime.Date) == false)
            {
                m_dgwAdminTable.InitRows(24, false);
            }
            else
            {
                m_dgwAdminTable.InitRows(25, true);
            }
        }

        /// <summary>
        /// При [авто]режиме переход к следующему элементу списка компонентов
        /// </summary>
        protected virtual void saveDataGridViewAdminComplete(int state)
        {
            //Logging.Logg().Debug(@"FormMainTrans::saveDataGridViewAdminComplete () - m_bTransAuto=" + m_bTransAuto + @", m_modeMashine=" + m_modeMashine.ToString () + @", - вХод...", Logging.INDEX_MESSAGE.NOT_SET);

            if ((IsService == true)
                && (m_bEnabledUIControl == false))
            {
                m_arAdmin[(int)CONN_SETT_TYPE.SOURCE].TECComponentComplete (state, true);

                CT.SuccessIter();

                IAsyncResult asyncRes;
                //if (IsService == true) проверка уже выполнена выше
                    if (InvokeRequired == true)
                        asyncRes = this.BeginInvoke (new DelegateFunc (trans_auto_next));
                    else
                        trans_auto_next ();
                //else
                //    ;
            }
            else
                ;

            //Logging.Logg().Debug(@"FormMainTrans::saveDataGridViewAdminComplete () - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected void setDatetimePicker(DateTime date)
        {
            IAsyncResult iar;
            object result;
            string mesToLog = string.Empty;

            Action<DateTime, string> fSetter = delegate (DateTime dateValue, string mes) {
                dateTimePickerMain.Value = dateValue;

                Logging.Logg ().Action (mes, Logging.INDEX_MESSAGE.D_001);
            };

            try {
                mesToLog = $"FormMainTrans::setDatetimePicker (Date={date.ToShortDateString ()}) - условия: IsHandleCreated={IsHandleCreated}, InvokeRequired={InvokeRequired}...";

                if (IsHandleCreated/*InvokeRequired*/ == true) {
                    if (InvokeRequired == true)
                        //iar =
                            BeginInvoke (fSetter, date, mesToLog);

                        //result = EndInvoke (iar);
                    else
                        fSetter (date, mesToLog);
                } else {
                    fSetter (date, mesToLog);
                }
            } catch (Exception e) {
                Logging.Logg ().Exception (e, $"FormMainTrans::setDatetimePicker (Date={date.ToShortDateString ()}) - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        protected override void Stop()
        {
            ClearTables();

            comboBoxTECComponent.Items.Clear();

            for (int i = 0; (i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE) && (!(m_arAdmin == null)); i++)
            {
                if (!(m_arAdmin[i] == null))
                {
                    m_arAdmin[i].Stop();
                    m_arAdmin[i] = null;
                }
                else
                    ;
            }

            base.Stop();
        }

        protected virtual void buttonClose_Click(object sender, EventArgs e)
        {
            Stop();

            Close();
        }

        private void groupBoxFocus(GroupBox groupBox)
        {
            GroupBox groupBoxOther = null;
            bool bBackColorChange = false;
            if (!(groupBox.BackColor == SystemColors.Info))
            {
                groupBox.BackColor = SystemColors.Info;

                UpdateStatusString();

                bBackColorChange = true;
            }
            else
                ;

            switch (groupBox.Name)
            {
                case "groupBoxSource":
                    groupBoxOther = groupBoxDest;
                    break;
                case "groupBoxDest":
                    groupBoxOther = groupBoxSource;
                    break;
                default:
                    break;
            }

            if (bBackColorChange)
            {
                groupBoxOther.BackColor = SystemColors.Control;

                if (s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].Count > 1)
                    s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].SelectedIndex = m_IndexDB;
                else
                    ;

                comboBoxTECComponent_SelectedIndexChanged(null, EventArgs.Empty);
            }
            else
                ;
        }

        private void groupBox_MouseClick(object sender, MouseEventArgs e)
        {
            groupBoxFocus(((GroupBox)sender));
        }

        private void groupBox_Enter(object sender, EventArgs e)
        {
            groupBoxFocus(((GroupBox)sender));
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //buttonClose_Click (null, null);
            buttonClose.PerformClick();
        }

        private void конфигурацияБДToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //m_formConnectionSettings.StartPosition = FormStartPosition.CenterParent;
            s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].ShowDialog(this);

            //Эмуляция нажатия кнопки "Ок"
            /*
            m_formConnectionSettings.btnOk_Click(null, null);
            */

            DialogResult dlgRes = s_listFormConnectionSettings[(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].DialogResult;
            if (dlgRes == System.Windows.Forms.DialogResult.Yes)
            {
                Stop();

                Start();
            }
            else
                ;
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormAbout formAbout = new FormAbout(this.Icon.ToBitmap() as Image))
            {
                formAbout.ShowDialog(this);
            }
        }

        //protected override void ErrorReport (string msg) {
        //    Logging.Logg().Error(@"FormMainTrans::ErrorReport () - сообщение: " + msg);

        //    m_statusStripMain.BeginInvoke(delegateEvent);

        //    m_arAdmin[(int)CONN_SETT_TYPE.SOURCE].AbortRDGExcelValues();

        //    this.BeginInvoke(new DelegateBoolFunc(enabledButtonSourceExport), false);

        //    if ((m_bTransAuto == true || m_modeMashine == MODE_MASHINE.SERVICE) && (m_bEnabledUIControl == false))
        //        this.BeginInvoke(new DelegateFunc(trans_auto_next));
        //    else
        //        ;
        //}

        //protected override void ActionReport(string msg)
        //{
        //    m_statusStripMain.BeginInvoke(delegateEvent);

        //    this.BeginInvoke(new DelegateBoolFunc(enabledButtonSourceExport), true);
        //}

        /// <summary>
        /// Обновление статусной строки
        /// </summary>
        /// <returns>Признак наличия сообщения в строке с сообщениями</returns>
        protected override int UpdateStatusString()
        {
            int have_msg = -1;

            if ((!(m_arAdmin == null)) && (!(m_arAdmin[m_IndexDB] == null)))
            {
                have_msg = base.UpdateStatusString();
            }
            else
                ;

            return have_msg;
        }

        protected void trans_auto_start()
        {
            ////Таймер больше не нужен (сообщения в "строке статуса")
            //timerMain.Stop();
            //timerMain.Interval = TIMER_START_INTERVAL;
            ////timerMain.Enabled = false;

            m_arAdmin [(int)CONN_SETT_TYPE.SOURCE].PrepareActionRDGValues ();

            if (!(comboBoxTECComponent.SelectedIndex < 0)) {
                comboBoxTECComponent.SelectedIndex = -1;

                trans_auto_next();
            } else if (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_TO_DATE)
                buttonClose.PerformClick();
            else
                enabledUIControl (true);
        }

        protected void trans_auto_next()
        {
            FormChangeMode.KeyDevice  currentTECComponentKey
                , nextTECComponentKey;
            ComboBoxItem nextItem;
            int iNextIndex = -1;

            currentTECComponentKey = !(comboBoxTECComponent.SelectedIndex < 0)
                ? ((ComboBoxItem)comboBoxTECComponent.SelectedItem).Tag
                    : FormChangeMode.KeyDevice.Empty;

            nextTECComponentKey = m_arAdmin [(int)CONN_SETT_TYPE.SOURCE].FirstTECComponentKey;
            if (nextTECComponentKey.Equals (FormChangeMode.KeyDevice.Empty) == false) {
                nextItem = comboBoxTECComponent.Items.Cast<ComboBoxItem> ().First (item => {
                    return item.Tag.Equals (nextTECComponentKey) == true;
                });
                iNextIndex = comboBoxTECComponent.Items.IndexOf (nextItem);
            } else
                ;

            Logging.Logg ().Debug ($@"FormMainTrans::trans_auto_next () - SelectedItem=[Index: {comboBoxTECComponent.SelectedIndex}, Key: {currentTECComponentKey.ToString ()}], NextItem=[Index: {iNextIndex}, Key: {nextTECComponentKey.ToString()}]..."
                , Logging.INDEX_MESSAGE.NOT_SET);

            if ((!(iNextIndex < 0))
                && (iNextIndex < comboBoxTECComponent.Items.Count))
            {
                comboBoxTECComponent.SelectedIndex = iNextIndex;

                CT.AttemptIter(((ComboBoxItem)comboBoxTECComponent.SelectedItem).Tag);

                //Обработчик отключен - вызов "программно"
                comboBoxTECComponent_SelectedIndexChanged(null, EventArgs.Empty);
            }
            else
                if (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_TO_DATE)
                    buttonClose.PerformClick();
                else
                // режим работы "сервис" (??? по таймеру | событиям)
                    if (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_PERIOD)
                        if (isTomorrow() == true)
                        {
                            setDatetimePicker (dateTimePickerMain.Value.AddDays (1));
                            new Thread (new ParameterizedThreadStart (delegate (object obj) {
                                    trans_auto_start ();
                                })) {
                                IsBackground = true
                                , Name = string.Format ("FormMainTrans::Tomorrow")
                            }.Start ();
                        }
                        else
                        {//??? завершение выполнения очередной итерации
                            trans_auto_stop ();
                        }
                    else if (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_ON_EVENT)
                        trans_auto_stop();
                    else
                        throw new Exception($"FormMainTrans::trans_auto_next () - неизвестный сервисный режим <{handlerCmd.ModeMashine.ToString ()}> работы...");
        }

        protected virtual void trans_auto_stop ()
        {
            Logging.Logg ().Debug ($"FormMainTrans::trans_auto_stop () IsService={handlerCmd.ModeMashine.ToString()}, SetDate({DateTime.Now.Date.ToShortDateString()})..."
                , Logging.INDEX_MESSAGE.NOT_SET);

            setDatetimePicker (DateTime.Now);
            //enabledUIControl(true);
        }

        protected virtual bool isTomorrow()
        {
            return IsTomorrow(dateTimePickerMain.Value, DateTime.Now, FileAppSettings.This ().OverDate ());
        }

        public static bool IsTomorrow (DateTime picker, DateTime now, TimeSpan timeSpan)
        {
            DateTime newDateApp = picker.Date.AddDays (1);
            double diff = 0F;

            diff = (now.Add (timeSpan) - newDateApp).TotalDays;

            return (diff > 0F)
                && (diff < timeSpan.TotalDays) ;
        }

        protected override void timer_Start()
        {
            HAdmin.SeasonDateTime = DateTime.Parse(FileAppSettings.This ().GetValueOfMainIndexParameter(FormParameters.PARAMETR_SETUP.SEASON_DATETIME));
            HAdmin.SeasonAction = Int32.Parse(FileAppSettings.This ().GetValueOfMainIndexParameter (FormParameters.PARAMETR_SETUP.SEASON_ACTION));

            FormChangeMode.MODE_TECCOMPONENT mode = FormChangeMode.MODE_TECCOMPONENT.GTP;

            if (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_TO_DATE) {
                FillComboBoxTECComponent(mode, true);
                CT = new ComponentTesting(comboBoxTECComponent.Items.Count);
                trans_auto_start();
            } else if ((handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_PERIOD)
                || (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_ON_EVENT))
                m_checkboxModeMashine.Checked = true;
            else {
                FillComboBoxTECComponent(mode, true);
                CT = new ComponentTesting(comboBoxTECComponent.Items.Count);
            }
        }

        //protected bool isFirstTimerTick
        //{
        //    get
        //    {
        //        return timerService.Interval == ProgramBase.TIMER_START_INTERVAL;
        //    }
        //}

        protected virtual void timerService_Tick(object sender, EventArgs e)
        {
            FormChangeMode.MODE_TECCOMPONENT mode = FormChangeMode.MODE_TECCOMPONENT.GTP;

            switch (handlerCmd.ModeMashine)
            {
                case MODE_MASHINE.SERVICE_TO_DATE:
                // никаких действий не предпринимается
                    break;
                case MODE_MASHINE.SERVICE_PERIOD:
                    if (timerService.Interval == ProgramBase.TIMER_START_INTERVAL) // isFirstTimerTick == true
                    {
                        //Первый запуск
                        if (handlerCmd.TimerInterval == timerService.Interval)
                            handlerCmd.IncTimerInterval();
                        else ; //??? случайное совпадение...
                        timerService.Interval = handlerCmd.TimerInterval;

                        FillComboBoxTECComponent (mode, true);
                        CT = new ComponentTesting (comboBoxTECComponent.Items.Count);
                    }
                    else
                        ;

                    setDatetimePicker (DateTime.Now);

                    trans_auto_start();
                    break;
                //// только 'trans_mc.exe' может выполняться в таком режиме - см. 'FormMainTransMC::timerService_Tick'
                //case MODE_MASHINE.SERVICE_ON_EVENT:
                //    break;
                default:
                    break;
            }
        }

        protected virtual void buttonClear_Click(object sender, EventArgs e)
        {
            //m_IndexDB = только DEST
            ((AdminTS)m_arAdmin[m_IndexDB]).ClearRDGValues(dateTimePickerMain.Value.Date);
        }

        protected /*virtual*/ void buttonDestSave_Click(object sender, EventArgs e) { throw new NotImplementedException(); }

        protected /*virtual*/ void buttonSourceSave_Click(object sender, EventArgs e) { throw new NotImplementedException(); }

        protected virtual void component_Changed(object sender, EventArgs e)
        {
            //Не передавать значения в форму с параметрами соединения с БД конфигурации
            //Раньше эти настройки изменялись на самой форме...
            /*
            uint indxDB = (uint)m_IndexDB;
            ConnectionSettings connSett = new ConnectionSettings();

            connSett.server = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.SERVER_IP].Text;
            connSett.port = (Int32)((NumericUpDown)m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.PORT]).Value;
            connSett.dbName = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.NAME_DATABASE].Text;
            connSett.userName = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.USER_ID].Text;
            connSett.password = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.PASS].Text;
            connSett.ignore = false;

            m_formConnectionSettings.ConnectionSettingsEdit = connSett;
            */
        }

        protected bool IsCanSelectedIndexChanged
        {
            get
            {
                bool bRes = false;

                if (((!(comboBoxTECComponent.SelectedIndex < 0))
                        && (((ComboBoxItem)comboBoxTECComponent.SelectedItem).Tag.Id > 0))
                    && (!(m_arAdmin == null))
                        && (!(m_arAdmin [m_IndexDB] == null)))
                    bRes = true;
                else
                    ;

                return bRes;
            }
        }

        protected abstract void comboBoxTECComponent_SelectedIndexChanged(object cbx, EventArgs ev);

        private void dateTimePickerMain_Changed(object sender, EventArgs e)
        {
            initTableHourRows();
            comboBoxTECComponent_SelectedIndexChanged(null, EventArgs.Empty);
        }

        public void ClearTables()
        {
            m_dgwAdminTable.ClearTables();
        }

        protected abstract void getDataGridViewAdmin(int indxDB);

        /// <summary>
        /// Экспорт данных ихз источника
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">Аргумент события</param>
        private void buttonSourceExport_Click(object sender, EventArgs e)
        {
            if (!(comboBoxTECComponent.SelectedIndex < 0))
            {
                //Взять значения "с окна" в таблицу
                getDataGridViewAdmin((int)(Int16)CONN_SETT_TYPE.DEST);
                //ClearTables();
                DateTime time = DateTime.Now;
                m_labelTime.Text = "Последний экспорт данных в " + time;
                SaveRDGValues(true);
            }
            else
                ;
        }

        protected void  stopTimerService ()
        {
            timerService.Stop ();
            //timerService.Interval = TIMER_START_INTERVAL;
            timerService = null;
        }

        private void checkboxModeMashine_CheckedChanged(object sender, EventArgs e)
        {
            if (!(handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_TO_DATE))
                if (m_checkboxModeMashine.Checked == true)
                {
                    //if (m_modeMashine == MODE_MASHINE.INTERACTIVE) m_modeMashine = MODE_MASHINE.SERVICE; else ;
                    //То же самое
                    if ((!(handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_PERIOD))
                        && (!(handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_ON_EVENT)))
                        handlerCmd.SetModeServiceMashineDefault();
                    else ;

                    enabledUIControl(false);
                    m_dgwAdminTable.Enabled = false;
                    InitializeTimerService();
                    WinApi.SendMessage(this.Handle, 0x112, 0xF020, 0);
                    timerService.Start();
                    //timerService.Change (0, ;
                }
                else
                {
                    if (!(handlerCmd.ModeMashine == MODE_MASHINE.INTERACTIVE))
                        handlerCmd.SetModeMashine(MODE_MASHINE.INTERACTIVE);
                    else ;

                    stopTimerService ();
                }
            else
            {
                InitializeTimerService();
                timerService.Start();
            }
        }

        private void InitializeTimerService()
        {
            if (timerService == null)
            {
                timerService =
                    new System.Windows.Forms.Timer()
                    //new System.Threading.Timer(this.timerService_Tick)
                    ;
                timerService.Interval = ProgramBase.TIMER_START_INTERVAL; //Первый запуск
                timerService.Tick += new System.EventHandler(this.timerService_Tick);
            }
            else
                ;
        }

        private struct PARAMToSaveRDGValues
        {
            public FormChangeMode.KeyDevice key;
            public DateTime date;
            public bool bCallback;

            public PARAMToSaveRDGValues(FormChangeMode.KeyDevice k, DateTime dt, bool cb)
            {
                key = k;
                date = dt;
                bCallback = cb;
            }
        };

        private void saveRDGValues(object bCallback)
        {
            try
            {
                ((AdminTS)m_arAdmin[(int)(Int16)CONN_SETT_TYPE.DEST]).SaveRDGValues(((PARAMToSaveRDGValues)bCallback).key, ((PARAMToSaveRDGValues)bCallback).date, ((PARAMToSaveRDGValues)bCallback).bCallback);
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"FormMainTrans::saveRDGValues () - ", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Оброботчик сообщений формы
        /// </summary>
        /// <param name="m">Сообщение ОС</param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WinApi.SW_RESTORE:
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                    notifyIconMain.Visible = false;
                    break;

                case WinApi.WM_CLOSE:
                    //MessageBox.Show("WM_CLOSE");
                    break;
            }
            base.WndProc(ref m);
        }

        public FormChangeMode.KeyDevice SelectedItemKey
        {
            get
            {
                return ((ComboBoxItem)comboBoxTECComponent.SelectedItem).Tag;
            }
        }

        protected virtual void SaveRDGValues(bool bCallback)
        {
            //((AdminTS)m_arAdmin[(int)(Int16)CONN_SETT_TYPE.DEST]).SaveRDGValues(m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value, bCallback);
            PARAMToSaveRDGValues paramToSaveRDGValues = new PARAMToSaveRDGValues(SelectedItemKey, dateTimePickerMain.Value, bCallback);
            new Thread(new ParameterizedThreadStart(saveRDGValues)).Start(paramToSaveRDGValues);
        }

        private void notifyIconMain_Click(object sender, EventArgs e)
        {
            развернутьToolStripMenuItem.PerformClick();
        }

        /// <summary>
        /// обработчи события клика по иконке в трее
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">Аргумент события</param>
        private void развернутьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (notifyIconMain.Visible == true)
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                notifyIconMain.Visible = false;
            }
            else
                ;
        }

        /// <summary>
        /// Развертывание из трея приложения
        /// </summary>
        public void ExplandApp()
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIconMain.Visible = false;
            this.Show();
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonClose.PerformClick();
        }

        private void onDeactivate(object sender, EventArgs ev)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIconMain.Visible = true;

                try { Application.DoEvents(); }
                catch (Exception e) { Logging.Logg().Exception(e, @"Application.DoEvents ()", Logging.INDEX_MESSAGE.NOT_SET); }
            }
            else
                ;

            //Logging.Logg().Debug(@"FormMainTrans::onDeactivate () - WindowState=" + WindowState, Logging.INDEX_MESSAGE.NOT_SET);
        }

        ///// <summary>
        ///// Перехват нажатия на кнопку свернуть
        ///// </summary>
        ///// <param name="m">Сообщение ОС</param>
        //protected override void WndProc(ref Message m)
        //{
        //    if (m.Msg == 0x112)
        //    {
        //        if (m.WParam.ToInt32() == 0xF020)
        //        {
        //            this.WindowState = FormWindowState.Minimized;
        //            this.ShowInTaskbar = false;
        //            notifyIconMain.Visible = true;

        //            return;
        //        }
        //    }
        //    else
        //        ;

        //    base.WndProc(ref m);
        //}
    }
}
