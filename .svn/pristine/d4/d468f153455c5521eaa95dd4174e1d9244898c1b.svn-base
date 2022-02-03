using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using StatisticCommon;
using StatisticTrans;
using StatisticTransModes;
using ASUTP;

namespace trans_mc
{
    public partial class FormMainTransMC : FormMainTransModes
    {
        public FormMainTransMC()
            : base(ASUTP.Helper.ProgramBase.ID_APP.TRANS_MODES_CENTRE
                , new KeyValuePair<string, string> [] {
                    new System.Collections.Generic.KeyValuePair<string, string> ("MCServiceHost", "ne1843.ne.ru")
                    , new System.Collections.Generic.KeyValuePair<string, string> (@"ИгнорДатаВремя-ModesCentre", false.ToString())
                    //, new System.Collections.Generic.KeyValuePair<string, string> ("service", "on_event") перенесено в 'FormMainTrans'
                    , new System.Collections.Generic.KeyValuePair<string, string> ("JEventListener", JsonConvert.SerializeObject (new JObject {
                        { DbMCInterface.EVENT.OnData53500Modified.ToString(), false }
                        , { DbMCInterface.EVENT.OnMaket53500Changed.ToString(), false }
                        , { DbMCInterface.EVENT.OnPlanDataChanged.ToString(), true }
                        , { DbMCInterface.EVENT.OnModesEvent.ToString(), false }
                    }))
                    , new System.Collections.Generic.KeyValuePair<string, string> (@"FetchWaking", $"HH:mm:ss;{AdminMC.s_FetchWaking}")
                  })
        {
            InitializeComponent ();

            this.notifyIconMain.Icon =
            this.Icon = trans_mc.Properties.Resources.statistic5;
            InitializeComponentTransSrc (@"Сервер Модес-Центр");

            m_dgwAdminTable.Size = new System.Drawing.Size(498, 391);
        }

        protected override void Start()
        {
            int i = -1;

            EditFormConnectionSettings("connsett_mc.ini", false);

            bool bIgnoreTECInUse = false;

            //??? для создания статического 'DbMCSources' = 'DbSources'
            DbMCSources.Sources();
            DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
            DbTSQLConfigDatabase.DbConfig ().Register();

            ASUTP.Core.HMark markQueries = new ASUTP.Core.HMark (new int [] {(int)StatisticCommon.CONN_SETT_TYPE.ADMIN, (int)StatisticCommon.CONN_SETT_TYPE.PBR});

            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                switch (i)
                {
                    case (Int16)CONN_SETT_TYPE.SOURCE:
                        m_arAdmin[i] = new AdminMC(FileAppSettings.This ().GetValue(@"MCServiceHost"));
                        if (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_ON_EVENT) {
                            (m_arAdmin [i] as AdminMC).AddEventHandler (DbMCInterface.ID_EVENT.HANDLER_CONNECT, FormMainTransMC_EventHandlerConnect);

                            (m_arAdmin [i] as AdminMC).AddEventHandler(DbMCInterface.ID_EVENT.RELOAD_PLAN_VALUES, FormMainTransMC_EventMaketChanged);
                            //!!! дубликат для отладки
                            (m_arAdmin [i] as AdminMC).AddEventHandler (DbMCInterface.ID_EVENT.PHANTOM_RELOAD_PLAN_VALUES, FormMainTransMC_EventMaketChanged);
                            (m_arAdmin [i] as AdminMC).AddEventHandler (DbMCInterface.ID_EVENT.NEW_PLAN_VALUES, FormMainTransMC_EventPlanDataChanged);
                            //!!! дубликат для выполнения внеочередного запроса (например, при запуске)
                            (m_arAdmin [i] as AdminMC).AddEventHandler (DbMCInterface.ID_EVENT.REQUEST_PLAN_VALUES, FormMainTransMC_EventPlanDataChanged);
                        } else
                            ;
                        break;
                    case (Int16)CONN_SETT_TYPE.DEST:
                        m_arAdmin[i] = new AdminTS_Modes(new bool[] { false, true });
                        break;
                    default:
                        break;
                }
                try
                {
                    m_arAdmin[i].InitTEC(m_modeTECComponent, /*typeConfigDB, */markQueries, bIgnoreTECInUse, new int[] { 0, (int)TECComponent.ID.LK });
                    RemoveTEC(m_arAdmin[i]);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "FormMainTransMC::FormMainTransMC ()", Logging.INDEX_MESSAGE.NOT_SET);
                    //ErrorReport("Ошибка соединения. Переход в ожидание.");
                    //setUIControlConnectionSettings(i);
                    break;
                }
                switch (i)
                {
                    case (Int16)CONN_SETT_TYPE.SOURCE:
                        m_arAdmin[i].m_ignore_date = bool.Parse (FileAppSettings.This ().GetValue(@"ИгнорДатаВремя-ModesCentre"));
                        break;
                    case (Int16)CONN_SETT_TYPE.DEST:
                        m_arAdmin[i].m_ignore_date = bool.Parse (FileAppSettings.This ().GetValue(@"ИгнорДатаВремя-techsite"));
                        break;
                    default:
                        break;
                }
            }

            DbTSQLConfigDatabase.DbConfig().UnRegister();

            if (!(i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE))
            {
                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);

                for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                {
                    //setUIControlConnectionSettings(i); //??? Перенос ДО цикла

                    m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                    //m_arAdmin[i].SetDelegateWait(new DelegateFunc (StartWait), new DelegateFunc(StopWait), delegateEvent);
                    m_arAdmin[i].SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);

                    m_arAdmin[i].SetDelegateData(setDataGridViewAdmin, errorDataGridViewAdmin);
                    m_arAdmin[i].SetDelegateSaveComplete(saveDataGridViewAdminComplete);

                    m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                    //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                    //??? Перенос ПОСЛЕ цикла
                    //if (i == (int)(Int16)CONN_SETT_TYPE.DEST)
                    //    (Int16)CONN_SETT_TYPE.DEST
                    m_arAdmin[i].Start();
                    //else
                    //    ;
                }

                //Перенес обратно...
                //((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).StartDbInterface();

                //panelMain.Visible = false;

                base.Start();
            }
            else
                ;
        }

        private void FormMainTransMC_EventMaketChanged (object sender, EventArgs e)
        {
            IAsyncResult iar;
            object res;

            iar = BeginInvoke ((MethodInvoker)delegate () {
                AdminMC adminMC = m_arAdmin [(int)CONN_SETT_TYPE.SOURCE] as AdminMC;
                
                adminMC.GetMaketEquipment (FormChangeMode.KeyDevice.Service, e as AdminMC.EventArgs<Guid>, (e as AdminMC.IEventArgs).m_Date);
            });

            //iar.AsyncWaitHandle.WaitOne ();
            //res = EndInvoke (iar);

            Logging.Logg ().Action (@"::FormMainTransMC_EventMaketChanged () - ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        private void FormMainTransMC_EventPlanDataChanged (object sender, EventArgs e)
        {
            setDatetimePicker ((e as AdminMC.IEventArgs).m_Date.Date);

            if (InvokeRequired == true)
                Invoke ((MethodInvoker)delegate () { trans_auto_start (); });
            else
                trans_auto_start ();

            Logging.Logg ().Action (@"::FormMainTransMC_EventPlanDataChanged () - ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override void setUIControlSourceState()
        {
            IDevice comp = ((AdminTS)m_arAdmin [(Int16)CONN_SETT_TYPE.DEST]).CurrentDevice;

            if (comp.ListMCentreId.Count > 0)
            {
                //Properties.Settings sett = new Properties.Settings();
                //tbxSourceServerMC.Text = sett.Modes_Centre_Service_Host_Name;

                m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text = FileAppSettings.This ().GetValue(@"MCServiceHost");
            }
            else
                m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text = string.Empty;

            enabledButtonSourceExport(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text.Length > 0 ? true : false);
        }

        protected override void buttonSaveSourceSett_Click(object sender, EventArgs e)
        {
            //base.buttonSaveSourceSett_Click (sender, e);
        }

        protected override void trans_auto_stop ()
        {
            AdminMC adminMC = m_arAdmin [(int)CONN_SETT_TYPE.SOURCE] as AdminMC;

            Logging.Logg ().Debug ($"FormMainTransMC::trans_auto_stop () IsServiceOnEvent={adminMC.IsServiceOnEvent}..."
                , Logging.INDEX_MESSAGE.NOT_SET);

            if (adminMC.IsServiceOnEvent == true)
                adminMC.FetchEvent (true);
            else
                base.trans_auto_stop ();
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object sender, EventArgs ev)
        {
            Logging.Logg ().Debug (string.Format(@"FormMainTransMC::comboBoxTECComponent_SelectedIndexChanged () - IsCanSelectedIndexChanged={0}, IndexDB={1}, <AdminMC.CurrentKey.Id={2} >> SelectedIndex={3}, SelectedKey.Id={4}> ..."
                    , IsCanSelectedIndexChanged
                    , m_IndexDB
                    , m_arAdmin [m_IndexDB].CurrentKey.Id
                    , comboBoxTECComponent.SelectedIndex
                    , !(comboBoxTECComponent.SelectedIndex < 0) ? ((ComboBoxItem)comboBoxTECComponent.SelectedItem).Tag.Id : -1)
                , Logging.INDEX_MESSAGE.NOT_SET);

            if (IsCanSelectedIndexChanged == true)
            {
                base.comboBoxTECComponent_SelectedIndexChanged(sender, ev);

                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);
                setUIControlSourceState();
            }
            else {
                m_arAdmin [m_IndexDB].TECComponentComplete (-1, false);

                //??? как переходить к следующей итерации
                //??? 
            }
        }

        protected override void timerService_Tick (object sender, EventArgs e)
        {
            FormChangeMode.MODE_TECCOMPONENT mode = FormChangeMode.MODE_TECCOMPONENT.GTP;

            switch (handlerCmd.ModeMashine) {
                case MODE_MASHINE.SERVICE_ON_EVENT:
                    // остановить таймер; это первый  вызов (можно обрабытывать также в 'timer_Start')
                    stopTimerService ();

                    FillComboBoxTECComponent (mode, true);
                    CT = new ComponentTesting (comboBoxTECComponent.Items.Count);

                    setDatetimePicker (DateTime.Now);

                    m_arAdmin [(int)CONN_SETT_TYPE.SOURCE].Activate (true);


                    if ((handlerCmd.DebugTurn == true)
                        && (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_ON_EVENT))
                    // отладка переопубликации плана
                        (m_arAdmin [(int)CONN_SETT_TYPE.SOURCE] as AdminMC).DebugEventReloadPlanValues ();
                    else
                        ;
                    // обязательный запрос актуального плана для всех подразделений
                    (m_arAdmin [(int)CONN_SETT_TYPE.SOURCE] as AdminMC).ToDateRequest (ASUTP.Core.HDateTime.ToMoscowTimeZone ().Date);
                    break;
                default:
                    base.timerService_Tick (sender, e);
                    break;
            }
        }

        #region Код, автоматически созданный конструктором форм Windows

        private void FormMainTransMC_EventHandlerConnect (object obj, EventArgs ev)
        {
            Action checkStateChanged = delegate () {
                trans_mc.AdminMC.EventArgs<bool> arg = ev as trans_mc.AdminMC.EventArgs<bool>;

                ((ToolStripMenuItem)this.СобытияМодесЦентрToolStripMenuItem.DropDownItems.Find (getNameSubToolStripMenuItem (DbMCInterface.TranslateEvent (arg.m_id)), true) [0])
                    .Checked = arg.m_listParameters [0];

                this.СобытияМодесЦентрToolStripMenuItem.Enabled = this.СобытияМодесЦентрToolStripMenuItem.DropDownItems.Cast<ToolStripMenuItem> ().Any (item => item.Checked == true);
            };

            try {
                if (InvokeRequired == true)
                    Invoke (checkStateChanged);
                else
                    checkStateChanged ();
                
            } catch (Exception e) {
                Logging.Logg ().Exception (e, $"::FormMainTransMC_EventHandlerConnect () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        private static string getNameSubToolStripMenuItem (DbMCInterface.EVENT nameEvent)
        {
            return $"{nameEvent.ToString ()}СобытияМодесЦентрToolStripMenuItem";
        }

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent ()
        {
            ToolStripMenuItem subToolStripMenuItem;
            List<Tuple<DbMCInterface.EVENT, string>> listTextToolStripMenuItem;
            //JObject jsonEventListener;

            listTextToolStripMenuItem = new List<Tuple<DbMCInterface.EVENT, string>> {
                Tuple.Create (DbMCInterface.EVENT.OnData53500Modified, "Оборудование")
                , Tuple.Create (DbMCInterface.EVENT.OnMaket53500Changed, "Макет")
                , Tuple.Create (DbMCInterface.EVENT.OnPlanDataChanged, "План")
                , Tuple.Create (DbMCInterface.EVENT.OnModesEvent, "Служебное")
            };

            СобытияМодесЦентрToolStripMenuItem = new ToolStripMenuItem ();
            m_listSubEventModesCentreToolStripMenuItem = new List<ToolStripMenuItem> ();
            foreach (DbMCInterface.EVENT nameEvent in Enum.GetValues (typeof (DbMCInterface.EVENT))) {
                if (nameEvent == DbMCInterface.EVENT.Unknown)
                    continue;
                else
                    ;

                m_listSubEventModesCentreToolStripMenuItem.Add (new ToolStripMenuItem ());
                m_listSubEventModesCentreToolStripMenuItem [m_listSubEventModesCentreToolStripMenuItem.Count - 1].Tag = nameEvent;
            }

            // 
            // СобытияМодесЦентрToolStripMenuItem
            // 
            this.СобытияМодесЦентрToolStripMenuItem.Name = "СобытияМодесЦентрToolStripMenuItem";
            this.СобытияМодесЦентрToolStripMenuItem.Size = new System.Drawing.Size (118, 22);
            this.СобытияМодесЦентрToolStripMenuItem.Text = "События Модес-Центр";
            this.СобытияМодесЦентрToolStripMenuItem.Enabled = false;

            //jsonEventListener = JsonConvert.DeserializeObject<JObject> (StatisticTrans.FileAppSettings.This ().GetValue ("JEventListener"));

            foreach (DbMCInterface.EVENT nameEvent in Enum.GetValues (typeof (DbMCInterface.EVENT))) {
                if (nameEvent == DbMCInterface.EVENT.Unknown)
                    continue;
                else
                    ;

                subToolStripMenuItem = m_listSubEventModesCentreToolStripMenuItem.Single (item => (DbMCInterface.EVENT)item.Tag == nameEvent);
                // 
                // подпункт для СобытияМодесЦентрToolStripMenuItem
                // 
                subToolStripMenuItem.Tag = nameEvent;
                subToolStripMenuItem.Name = getNameSubToolStripMenuItem (nameEvent);
                subToolStripMenuItem.Size = new System.Drawing.Size (118, 22);
                subToolStripMenuItem.Text = listTextToolStripMenuItem.Single (desc => desc.Item1 == nameEvent).Item2;
                subToolStripMenuItem.Enabled =
                    //bool.Parse(jsonEventListener.Value<string>(eventName.ToString()))
                    false
                    ;

                this.СобытияМодесЦентрToolStripMenuItem.DropDownItems.Add (subToolStripMenuItem);
            }

            //this.СобытияМодесЦентрToolStripMenuItem.Enabled = handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_ON_EVENT;

            this.настройкиToolStripMenuItem.DropDownItems.Add (this.СобытияМодесЦентрToolStripMenuItem);
        }

        protected System.Windows.Forms.ToolStripMenuItem СобытияМодесЦентрToolStripMenuItem;
        protected IList<System.Windows.Forms.ToolStripMenuItem> m_listSubEventModesCentreToolStripMenuItem;

        #endregion
    }
}
