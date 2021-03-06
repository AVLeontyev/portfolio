using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

using StatisticCommon;
using StatisticTrans;
using StatisticTransModes;
using ASUTP;
using StatisticTrans.Contract.ModesCentre;
using StatisticTrans.Contract;
using System.ServiceModel;
using System.Threading.Tasks;

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
                    , new System.Collections.Generic.KeyValuePair<string, string> ("JEventListener", Newtonsoft.Json.JsonConvert.SerializeObject (new Newtonsoft.Json.Linq.JObject {
                        { EVENT.OnData53500Modified.ToString(), false }
                        , { EVENT.OnMaket53500Changed.ToString(), false }
                        , { EVENT.OnPlanDataChanged.ToString(), true }
                        , { EVENT.OnModesEvent.ToString(), false }
                    }))
                    , new System.Collections.Generic.KeyValuePair<string, string> (@"FetchWaking", $"HH:mm:ss;{StatisticTrans.Contract.Default.FetchWaking}")
                    , new KeyValuePair<string, string> ("NameEndPointService", "EndPointServiceTransModesCentre")
                  })
        {
            InitializeComponent ();

            this.notifyIconMain.Icon =
            this.Icon = trans_mc.Properties.Resources.statistic5;
            InitializeComponentTransSrc (@"Сервер Модес-Центр");

            m_dgwAdminTable.Size = new System.Drawing.Size(498, 391);
        }

        protected override IServiceTransModes createChannelService (InstanceContext context)
        {
            return new DuplexChannelFactory<StatisticTrans.Contract.ModesCentre.IServiceModesCentre> (context, FileAppSettings.This ().GetValue (@"NameEndPointService"))
                .CreateChannel ();
        }

        protected override void Start()
        {
            bool bInitialize = false;

            bInitialize = EditFormConnectionSettings ("connsett_mc.ini", false);

            if (bInitialize == true)
            {
                bInitialize = ((StatisticTrans.Contract.ModesCentre.IServiceModesCentre)_client).Initialize (s_listFormConnectionSettings [(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].getConnSett ()
                    , s_iMainSourceData
                    , FileAppSettings.This ().GetValue (@"MCServiceHost")
                    , handlerCmd.ModeMashine
                    , FileAppSettings.This ().FetchWaking(StatisticTrans.Contract.Default.FetchWaking)
                    , StatisticTrans.FileAppSettings.This ().GetValue ("JEventListener")
                    , m_modeTECComponent,
                    m_listID_TECNotUse);

                if (bInitialize == true) {
                    _client.SetIgnoreDate (StatisticTrans.CONN_SETT_TYPE.SOURCE, bool.Parse (FileAppSettings.This ().GetValue (@"ИгнорДатаВремя-ModesCentre")));
                    _client.SetIgnoreDate (StatisticTrans.CONN_SETT_TYPE.DEST, bool.Parse (FileAppSettings.This ().GetValue (@"ИгнорДатаВремя-techsite")));

                    ////??? никогда не выполняется, т.к. не заполнен ComboBox с перечнем ГТП
                    //if (IsCanSelectedIndexChanged == true)
                    //    setUIControlConnectionSettings (StatisticTrans.CONN_SETT_TYPE.DEST, _client.GetConnectionSettingsByKeyDeviceAndType (StatisticTrans.CONN_SETT_TYPE.DEST, SelectedItemKey, StatisticCommon.CONN_SETT_TYPE.PBR));
                    //else
                    //    ;

                    _client.Start ();

                    ////??? только после события 'Started'
                    //base.Start ();
                } else
                    ;
            }
            else
                ;
        }

        protected override void _callback_EventRaise (ServiceCallbackResultEventArgs arg1)
        {
            DateTime date;
            bool bResult = false;
            HAdmin.RDGStruct [] values;

            switch (arg1.Id) {
                case IdPseudoDelegate.ModesCentre_PlanDataChanged:
                    if (IsHandleCreated == true)
                        BeginInvoke (new Action<ServiceCallbackResultEventArgs> (delegate (ServiceCallbackResultEventArgs arg2) {
                            trans_auto_start ();
                        }), arg1);
                    else
                        ;
                    break;
                default:
                    base._callback_EventRaise (arg1);
                    break;
            }

            //Logging.Logg ().Debug ($"::_callback_EventRaise () - Id=<{arg1.Id}>...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override void Stop ()
        {
            switch (handlerCmd.ModeMashine) {
                case MODE_MASHINE.SERVICE_ON_EVENT:
                    _client.Activate (StatisticTrans.CONN_SETT_TYPE.SOURCE, false);
                    break;
                default:
                    break;
            }

            base.Stop ();
        }

        protected override void setUIControlSourceState(bool bEnabled)
        {
            m_arUIControls[(Int16)StatisticTrans.CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text = bEnabled == true
                ? FileAppSettings.This ().GetValue(@"MCServiceHost")
                    : string.Empty;

            enabledButtonSourceExport(bEnabled);
        }

        protected override void buttonSaveSourceSett_Click(object sender, EventArgs e)
        {
            //base.buttonSaveSourceSett_Click (sender, e);
        }

        protected override void trans_auto_stop ()
        {
            Task<bool>.Factory.StartNew (() => {
                return ((StatisticTrans.Contract.ModesCentre.IServiceModesCentre)_client).IsServiceOnEvent;
            }).ContinueWith ((taskCommpleted) => {
                Logging.Logg ().Debug ($"FormMainTransMC::trans_auto_stop () IsServiceOnEvent={taskCommpleted.Result}..."
                    , Logging.INDEX_MESSAGE.NOT_SET);

                if (taskCommpleted.Result == true)
                    ((StatisticTrans.Contract.ModesCentre.IServiceModesCentre)_client).FetchEvent (true);
                else
                    BeginInvoke ((MethodInvoker)delegate () {
                        base.trans_auto_stop ();
                    });
            });
        }

        private delegate bool BoolDelegateObjectFunc (object obj);

        protected override void comboBoxTECComponent_SelectedIndexChanged(object sender, EventArgs ev)
        {
            //IDevice comp;
            bool bUIControlSourceEnabled = false;
            ASUTP.Database.ConnectionSettings connSett = null;
            FormChangeMode.KeyDevice selectedItemKey;
            IAsyncResult iar;

            Func<FormChangeMode.KeyDevice> fGetSelectedItemKey = delegate () {
                return SelectedItemKey;
            };

            Task<FormChangeMode.KeyDevice>.Factory.StartNew (() => {
                return _client.GetCurrentKey (m_IndexDB);
            }).ContinueWith ((taskCompleted) => {
                BeginInvoke (new ASUTP.Core.DelegateObjectFunc ((object obj) => {
                    Logging.Logg ().Debug (string.Format (@"FormMainTransMC::comboBoxTECComponent_SelectedIndexChanged () - IsCanSelectedIndexChanged={0}, IndexDB={1}, <AdminMC.CurrentKey.Id={2} >> SelectedIndex={3}, SelectedKey.Id={4}> ..."
                            , IsCanSelectedIndexChanged
                            , m_IndexDB
                            , ((FormChangeMode.KeyDevice)obj).Id
                            , comboBoxTECComponent.SelectedIndex
                            , !(comboBoxTECComponent.SelectedIndex < 0) ? ((ComboBoxItem)comboBoxTECComponent.SelectedItem).Tag.Id : -1)
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }), taskCompleted.Result);
            });

            Task<Tuple<bool, ASUTP.Database.ConnectionSettings>>.Factory.StartNew (() => {
                try {
                    iar = BeginInvoke (fGetSelectedItemKey);
                    selectedItemKey = (FormChangeMode.KeyDevice)EndInvoke (iar);

                    if (!(selectedItemKey == FormChangeMode.KeyDevice.Empty)) {
                        // _client.GetCurrentDevice (StatisticTrans.CONN_SETT_TYPE.SOURCE); //.DEST
                        bUIControlSourceEnabled = _client.GetAllowRequested (StatisticTrans.CONN_SETT_TYPE.SOURCE);
                        connSett = _client.GetConnectionSettingsByKeyDeviceAndType (StatisticTrans.CONN_SETT_TYPE.DEST, selectedItemKey, StatisticCommon.CONN_SETT_TYPE.PBR);
                    } else
                        Logging.Logg ().Error ($@"FormMainTransMC::comboBoxTECComponent_SelectedIndexChanged () - ::_client method calling: SelectedItemKey={selectedItemKey.ToString()}...", Logging.INDEX_MESSAGE.NOT_SET);
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, $@"FormMainTransMC::comboBoxTECComponent_SelectedIndexChanged () - ::_client method calling...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return Tuple.Create (
                    bUIControlSourceEnabled
                    , connSett);
            })
            .ContinueWith<bool>((taskCompleted) => {
                bool bTaskResult = false;

                try {
                    iar = BeginInvoke(new BoolDelegateObjectFunc ((obj) => {
                        bool bInvokeResult = false;

                        try {
                            if (IsCanSelectedIndexChanged == true)
                            {
                                base.comboBoxTECComponent_SelectedIndexChanged(sender, ev);

                                setUIControlSourceState ((obj as Tuple<bool, ASUTP.Database.ConnectionSettings>).Item1);
                                setUIControlConnectionSettings (StatisticTrans.CONN_SETT_TYPE.DEST, (obj as Tuple<bool, ASUTP.Database.ConnectionSettings>).Item2);
                            }
                            else {
                                bInvokeResult = true;

                                //??? как переходить к следующей итерации
                                //TODO:
                            }
                        } catch (Exception e) {
                            Logging.Logg ().Exception (e, $@"FormMainTransMC::comboBoxTECComponent_SelectedIndexChanged () - ::BeginInvoke () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }

                        return bInvokeResult;
                    }), taskCompleted.Result);
                    bTaskResult = (bool)EndInvoke (iar);
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, $@"FormMainTransMC::comboBoxTECComponent_SelectedIndexChanged () - ::ContinueWith () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                return bTaskResult;
            }).ContinueWith((taskCompleted) => {
                if (taskCompleted.Result == true) {
                    _client.TECComponentComplete (m_IndexDB, -1, false);

                    //??? как переходить к следующей итерации
                    //TODO:
                } else
                    ;
            });
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

                    _client.Activate (StatisticTrans.CONN_SETT_TYPE.SOURCE, true);

                    if ((handlerCmd.DebugTurn == true)
                        && (handlerCmd.ModeMashine == MODE_MASHINE.SERVICE_ON_EVENT))
                    // отладка переопубликации плана
                        ((StatisticTrans.Contract.ModesCentre.IServiceModesCentre)_client).DebugEventReloadPlanValues ();
                    else
                        ;
                    // обязательный запрос актуального плана для всех подразделений
                    ((StatisticTrans.Contract.ModesCentre.IServiceModesCentre)_client).ToDateRequest (ASUTP.Core.HDateTime.ToMoscowTimeZone ().Date);
                    break;
                default:
                    base.timerService_Tick (sender, e);
                    break;
            }
        }

        private void FormMainTransMC_EventPlanDataChanged (object sender, EventArgs e)
        {
            setDatetimePicker ((e as IEventArgs).m_Date.Date);

            if (InvokeRequired == true)
                Invoke ((MethodInvoker)delegate () {
                    trans_auto_start ();
                });
            else
                trans_auto_start ();

            Logging.Logg ().Action (@"::FormMainTransMC_EventPlanDataChanged () - ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        private void FormMainTransMC_EventMaketChanged (object sender, EventArgs e)
        {
            IAsyncResult iar;
            object res;

            iar = BeginInvoke ((MethodInvoker)delegate () {
                ((StatisticTrans.Contract.ModesCentre.IServiceModesCentre)_client).GetMaketEquipment (FormChangeMode.KeyDevice.Service, e as EventArgs<Guid>, (e as IEventArgs).m_Date);
            });

            //iar.AsyncWaitHandle.WaitOne ();
            //res = EndInvoke (iar);

            Logging.Logg ().Action (@"::FormMainTransMC_EventMaketChanged () - ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        #region Код, автоматически созданный конструктором форм Windows

        private void FormMainTransMC_EventHandlerConnect (object obj, EventArgs ev)
        {
            Action checkStateChanged = delegate () {
                EventArgs<bool> arg = ev as EventArgs<bool>;

                ((ToolStripMenuItem)this.СобытияМодесЦентрToolStripMenuItem.DropDownItems.Find (getNameSubToolStripMenuItem (arg.TranslateEvent (arg.m_id)), true) [0])
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

        private static string getNameSubToolStripMenuItem (EVENT nameEvent)
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
            List<Tuple<EVENT, string>> listTextToolStripMenuItem;
            //JObject jsonEventListener;

            listTextToolStripMenuItem = new List<Tuple<EVENT, string>> {
                Tuple.Create (EVENT.OnData53500Modified, "Оборудование")
                , Tuple.Create (EVENT.OnMaket53500Changed, "Макет")
                , Tuple.Create (EVENT.OnPlanDataChanged, "План")
                , Tuple.Create (EVENT.OnModesEvent, "Служебное")
            };

            СобытияМодесЦентрToolStripMenuItem = new ToolStripMenuItem ();
            m_listSubEventModesCentreToolStripMenuItem = new List<ToolStripMenuItem> ();
            foreach (EVENT nameEvent in Enum.GetValues (typeof (EVENT))) {
                if (nameEvent == EVENT.Unknown)
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

            foreach (EVENT nameEvent in Enum.GetValues (typeof (EVENT))) {
                if (nameEvent == EVENT.Unknown)
                    continue;
                else
                    ;

                subToolStripMenuItem = m_listSubEventModesCentreToolStripMenuItem.Single (item => (EVENT)item.Tag == nameEvent);
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
