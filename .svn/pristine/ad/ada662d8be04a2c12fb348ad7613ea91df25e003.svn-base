using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using StatisticCommon;
using StatisticTrans;
using ASUTP.Core;
using ASUTP.Database;

namespace trans_tg
{
    public partial class FormMainTransTG : FormMainTrans
    {
        public FormMainTransTG()
            : base(ASUTP.Helper.ProgramBase.ID_APP.TRANS_TG
            , new KeyValuePair<string, string>[] { })
        {
            InitializeComponentTransSrc(@"Путь РДГ (Excel)");

            //???
            this.m_dgwAdminTable = new StatisticCommon.DataGridViewAdminNSS(SystemColors.ControlText, new Color [] { SystemColors.Window, Color.Yellow, Color.Red });
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).BeginInit();
            // 
            // m_dgwAdminTable
            // 
            this.SuspendLayout();
            this.m_dgwAdminTable.Location = new System.Drawing.Point(319, 5);
            this.m_dgwAdminTable.Name = "m_dgwAdminTable";
            this.m_dgwAdminTable.RowHeadersVisible = false;
            this.m_dgwAdminTable.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //this.m_dgwAdminTable.Size = new System.Drawing.Size(498, 401);
            this.m_dgwAdminTable.TabIndex = 27;
            this.panelMain.Controls.Add(this.m_dgwAdminTable);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).EndInit();
            this.ResumeLayout(false);

            //m_listIsDataTECComponents = new List<bool> ();

            m_dgwAdminTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left));
            m_dgwAdminTable.Size = new System.Drawing.Size(498, 391);

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTrans));
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic3"))); //$this.Icon
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic3"))); //$this.Icon

            m_modeTECComponent = FormChangeMode.MODE_TECCOMPONENT.TEC;

            m_arAdmin = new AdminTS[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            Start();
        }

        private void watcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
        }

        protected override void start()
        {
            base.Start();
        }

        protected override void Start()
        {
            int i = -1;

            EditFormConnectionSettings("connsett_tg.ini", true);

            bool bIgnoreTECInUse = false;

            HMark markQueries = new HMark(new int[] { (int)StatisticCommon.CONN_SETT_TYPE.ADMIN, (int)StatisticCommon.CONN_SETT_TYPE.PBR });
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.ADMIN);
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.PBR);

            DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings (s_listFormConnectionSettings [(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].getConnSett ());
            DbTSQLConfigDatabase.DbConfig ().Register();

            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                if (i == (Int16)CONN_SETT_TYPE.SOURCE)
                    m_arAdmin[i] = new AdminTransTG(new bool [] {false, false});
                else
                    if (i == (Int16)CONN_SETT_TYPE.DEST)
                        m_arAdmin[i] = new AdminTransTG(new bool[] { false, true });
                    else
                        ;

                try {
                    ((AdminTS)m_arAdmin[i]).InitTEC(FormChangeMode.MODE_TECCOMPONENT.ANY, /*iTypeConfigDB, */markQueries, bIgnoreTECInUse, new int[] { 0, (int)TECComponent.ID.LK });
                    RemoveTEC(m_arAdmin[i]);
                }
                catch (Exception e)
                {
                    ASUTP.Logging.Logg().Exception(e, "FormMainTransTG::FormMainTransTG ()"
                        , ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                    //ErrorReport("Ошибка соединения. Перехож в ожидание.");
                    //setUIControlConnectionSettings(i);
                    break;
                }
                //((AdminTS)m_arAdmin[i]).connSettConfigDB = m_formConnectionSettings.getConnSett();
                //((AdminTS)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
                if (i == (Int16)CONN_SETT_TYPE.SOURCE)
                    m_arAdmin[i].m_ignore_date = false;
                else
                    if (i == (Int16)CONN_SETT_TYPE.DEST)
                        m_arAdmin[i].m_ignore_date = true;
                    else
                        ;

                //m_arAdmin[i].m_ignore_connsett_data = true; //-> в конструктор
            }

            DbTSQLConfigDatabase.DbConfig ().UnRegister();

            if (!(i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE))
            {
                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);

                for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                {
                    m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                    //m_arAdmin[i].SetDelegateWait(new DelegateFunc(StartWait), new DelegateFunc(StopWait), delegateEvent);
                    m_arAdmin[i].SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);

                    m_arAdmin[i].SetDelegateData(setDataGridViewAdmin, errorDataGridViewAdmin);
                    m_arAdmin[i].SetDelegateSaveComplete(saveDataGridViewAdminComplete);

                    m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                    //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                    m_arAdmin[i].Start();
                }

                /*
                // This needs to be declared in a place where it will not go out of scope.
                // For example, it would be a class variable in a form class.
                System.IO.FileSystemWatcher watcher = new System.IO.FileSystemWatcher();
                // This code would go in one of the initialization methods of the class.
                watcher.Path = "c:\\";
                // Watch only for changes to *.txt files.
                watcher.Filter = "*.txt";
                watcher.IncludeSubdirectories = false;
                // Enable the component to begin watching for changes.
                watcher.EnableRaisingEvents = true;
                // Filter for Last Write changes.
                watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
                // Example of watching more than one type of change.
                watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.Size;

                //Асинхронно
                watcher.Changed += new System.IO.FileSystemEventHandler(this.watcher_Changed);

                //Сихронно
                watcher.WaitForChanged(System.IO.WatcherChangeTypes.All);
                */

                //panelMain.Visible = false;

                start ();
            }
            else
                ;
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object cbx, EventArgs ev)
        {
            if (IsCanSelectedIndexChanged == true)
            {
                ClearTables();

                switch (m_modeTECComponent)
                {
                    case FormChangeMode.MODE_TECCOMPONENT.GTP:
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TG:
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TEC:
                        switch (m_IndexDB)
                        {
                            case (int)CONN_SETT_TYPE.SOURCE:
                                m_arAdmin[(int)CONN_SETT_TYPE.SOURCE].ResetRDGExcelValues();

                                int countComp = ((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_listKeyTECComponentDetail.Count;

                                //if (m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].allTECComponents[m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex]].tec.m_path_rdg_excel.Length > 0)
                                //{
                                ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).ImpRDGExcelValues(SelectedItemKey, dateTimePickerMain.Value.Date);
                                //}
                                //else
                                //    ;
                                break;
                            case (int)CONN_SETT_TYPE.DEST:
                                ((AdminTS)m_arAdmin[m_IndexDB]).GetRDGValues(SelectedItemKey, dateTimePickerMain.Value.Date);
                                break;
                            default:
                                break;
                        }

                        setUIControlSourceState();
                        setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);
                        break;
                    default:
                        break;
                }
            }
            else
                ;
        }

        protected override void component_Changed(object sender, EventArgs e)
        {
            if (m_IndexDB == (short)CONN_SETT_TYPE.DEST)
            {
                /*
                uint indxDB = (uint)m_IndexDB;
                ConnectionSettings connSett = new ConnectionSettings();

                connSett.server = tbxDestServerIP.Text;
                connSett.port = (int)nudnDestPort.Value;
                connSett.dbName = tbxDestNameDatabase.Text;
                connSett.userName = tbxDestUserId.Text;
                connSett.password = tbxDestPassword.Text;
                connSett.ignore = false;

                m_formConnectionSettings.ConnectionSettingsEdit = connSett;
                */

                base.component_Changed(sender, e);
            }
            else ;
        }

        protected override void setUIControlSourceState()
        {
            // tec получить у любого, в т.ч. "0"-го элемента
            TEC tec = ((AdminTS)m_arAdmin [(Int16)CONN_SETT_TYPE.DEST]).FindTECComponent (((AdminTS_NSS)m_arAdmin [(Int16)CONN_SETT_TYPE.DEST]).m_listKeyTECComponentDetail [0]).tec;

            m_arUIControls [(Int16)CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text =
                tec.GetAddingParameter(TEC.ADDING_PARAM_KEY.PATH_RDG_EXCEL).ToString();
            enabledButtonSourceExport(m_arUIControls[(Int16)CONN_SETT_TYPE.SOURCE, (Int16)INDX_UICONTROLS.SERVER_IP].Text.Length > 0 ? true : false);
        }

        private int getIndexGTPOwner(int indx_tg)
        {
            int indxDB = m_IndexDB,
                id_gtp_owner = ((DataGridViewAdminNSS)m_dgwAdminTable).GetIdGTPOwner(indx_tg);

            foreach (FormChangeMode.KeyDevice key in ((AdminTransTG)m_arAdmin[indxDB]).m_listKeyTECComponentDetail)
                if (((AdminTransTG)m_arAdmin[indxDB]).FindTECComponent(key).m_id == id_gtp_owner)
                    return ((AdminTransTG)m_arAdmin[indxDB]).m_listKeyTECComponentDetail.IndexOf(key);
                else
                    ;

            return -1;
        }

        protected override void getDataGridViewAdmin(int indxDB) //indxDB = DEST (ВСЕГДА)
        {
            double value;
            bool valid;

            ((AdminTS_TG)m_arAdmin[indxDB]).ClearListRDGValues();

            foreach (FormChangeMode.KeyDevice key in ((AdminTransTG)m_arAdmin[indxDB]).m_listKeyTECComponentDetail)
            {
                int indx_comp = ((AdminTransTG)m_arAdmin[indxDB]).m_listKeyTECComponentDetail.IndexOf(key),
                    indx_owner = getIndexGTPOwner(indx_comp);

                if (!(indx_comp < 0))
                {
                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues.Add(new AdminTS.RDGStruct[24]);

                    if (key.Mode == FormChangeMode.MODE_TECCOMPONENT.GTP)
                    {
                        ((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_listCurRDGValues[indx_comp].CopyTo(((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_comp], 0);
                    }
                    else
                        if (key.Mode == FormChangeMode.MODE_TECCOMPONENT.TG)
                        {
                            if (!(indx_owner < 0))
                                for (int i = 0; i < 24; i++)
                                {
                                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_comp][i].pbr = Convert.ToDouble(m_dgwAdminTable.Rows[i].Cells[indx_comp + 1].Value); // '+ 1' за счет DateTime

                                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_comp][i].recomendation = 0.0;
                                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_comp][i].deviationPercent = ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_owner][i].deviationPercent;
                                    ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_comp][i].deviation = ((AdminTransTG)m_arAdmin[indxDB]).m_listCurRDGValues[indx_owner][i].deviation;
                                }
                            else
                                ;
                        }
                        else
                            ;
                }
                else
                    ;
            }

            //((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).getDataGridViewAdmin(((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]));
            ((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).getCurTimezoneOffsetRDGExcelValues(((AdminTransTG)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]));
        }

        /// <summary>
        /// Добавить столбец в DataGridView + заполнение значениями ячеек
        /// </summary>
        /// <param name="date">Дата отображаемых значений</param>
        /// <param name="bNewValues">Признак наличия новых значений (false - обновление оформления представления при изменении цветовой схемы)</param>
        /// <param name="bSyncReq">Признак необходимости синхронизации по окончании выполнения метода</param>
        private void addTextBoxColumn (DateTime date, bool bNewValues, bool bSyncReq)
        {
            int indxDB = m_IndexDB;
            FormChangeMode.KeyDevice key = ((AdminTS_NSS)m_arAdmin[indxDB]).m_listKeyTECComponentDetail[m_dgwAdminTable.Columns.Count - 2];
            ((DataGridViewAdminNSS)m_dgwAdminTable).addTextBoxColumn(((AdminTS_NSS)m_arAdmin[indxDB]).GetNameTECComponent(key, false)
                , key
                , ((AdminTS_NSS)m_arAdmin[indxDB]).GetIdGTPOwnerTECComponent(key));

            DataGridViewCellEventArgs ev;

            for (int i = 0; i < 24; i++)
            {
                if (m_dgwAdminTable.Columns.Count == 3) //Только при добавлении 1-го столбца
                    m_dgwAdminTable.Rows[i].Cells[0].Value = date.AddHours(i + 1).ToString("dd-MM-yyyy HH:00");
                else
                    ;

                m_dgwAdminTable.Rows[i].Cells[m_dgwAdminTable.Columns.Count - 2].Value = ((AdminTS_NSS)m_arAdmin[indxDB]).m_listCurRDGValues[m_dgwAdminTable.Columns.Count - 3][i].pbr.ToString("F2");
                ev = new DataGridViewCellEventArgs(m_dgwAdminTable.Columns.Count - 2, i);
                ((DataGridViewAdminNSS)m_dgwAdminTable).DataGridViewAdminNSS_CellValueChanged(null, ev);
            }

            if (bNewValues == true)
                m_arAdmin [indxDB].CopyCurToPrevRDGValues ();
            else
                ;
        }

        private void SaveChanges ()
        {
            ((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).SaveChanges();
        }

        protected override void SaveRDGValues(bool bCallback)
        {
            ((AdminTS)m_arAdmin[(int)(Int16)CONN_SETT_TYPE.DEST]).SaveRDGValues(SelectedItemKey, dateTimePickerMain.Value, bCallback);
            //((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).SaveChanges();
        }

        protected override void updateDataGridViewAdmin(DateTime date, bool bNewValues)
        {
            if (IsHandleCreated == true)
                if (InvokeRequired == true)
                    this.BeginInvoke (new Action<DateTime, bool, bool> (addTextBoxColumn), date, true, InvokeRequired);
                else
                    addTextBoxColumn(date, true, InvokeRequired);
            else
                ASUTP.Logging.Logg ().Error (@"FormMainTransTG::updateDataGridViewAdmin () - ... BeginInvoke (addTextBoxColumn) - ..."
                , ASUTP.Logging.INDEX_MESSAGE.D_001);
        }

        protected override void buttonClear_Click(object sender, EventArgs e)
        {
            //m_IndexDB = только DEST
            ((AdminTS)m_arAdmin[m_IndexDB]).ClearRDGValues(dateTimePickerMain.Value.Date);
            
            m_dgwAdminTable.ClearTables ();

            ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).GetRDGValues(SelectedItemKey, dateTimePickerMain.Value.Date);
        }

        protected override void saveDataGridViewAdminComplete(int state)
        {
            bool bCompletedSaveChanges = false;

            //Вариант №1
            //bCompletedSaveChanges = ((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).CompletedSaveChanges;

            //Вариант №2
            bCompletedSaveChanges = WaitHandle.WaitAny(new WaitHandle[] { ((AdminTS_NSS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).m_evSaveChangesComplete }, 0) == 0;

            ASUTP.Logging.Logg().Debug(@"FormMainTransTG::saveDataGridViewAdminComplete () - CompletedSaveChanges=" + bCompletedSaveChanges.ToString()
                , ASUTP.Logging.INDEX_MESSAGE.NOT_SET);

            if (bCompletedSaveChanges == true)
            {
                //Logging.Logg().Debug(@"FormMainTransTG::saveDataGridViewAdminComplete () - SuccessSaveChanges=" + ((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).SuccessSaveChanges.ToString());
                //if (((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).SuccessSaveChanges == true) {
                    base.saveDataGridViewAdminComplete(state);
                //} else ;
            }
            else
                ;
                
        }

        /// <summary>
        /// Отобразить значения в представлении
        /// </summary>
        /// <param name="date">Дата, за которую получены значения для отображения</param>
        /// <param name="bNewValues">Признак наличия новых значений, иначе требуется изменить оформление представления</param>
        protected override void setDataGridViewAdmin(DateTime date, bool bNewValues)
        {
            //if (WindowState == FormWindowState.Minimized)
            //if (m_bTransAuto == true)
            //if (m_modeMashine == MODE_MASHINE.AUTO || m_modeMashine == MODE_MASHINE.SERVICE)
            if ((IsService == true)
                && (m_bEnabledUIControl == false))
            {
                if (((AdminTS_NSS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).CompletedGetRDGValues == true)
                {
                    m_arAdmin[(int)CONN_SETT_TYPE.DEST].getCurRDGValues(m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]);

                    if (IsHandleCreated/*InvokeRequired*/ == true)
                        this.BeginInvoke(new ASUTP.Core.DelegateBoolFunc (SaveRDGValues), false);
                    else
                        ASUTP.Logging.Logg().Error(@"FormMainTransTG::setDataGridViewAdmin () - ... BeginInvoke (SaveRDGValues) - ..."
                            , ASUTP.Logging.INDEX_MESSAGE.D_001);

                    //this.BeginInvoke(new DelegateFunc(trans_auto_next));
                }
                else
                    ;
            }
            else
            {
                //this.BeginInvoke(new DelegateDateFunction(addTextBoxColumn), date);
                updateDataGridViewAdmin(date, bNewValues);
            }
        }

        protected override void buttonSaveSourceSett_Click(object sender, EventArgs e)
        {
        }

        protected override void timer_Start()
        {
            base.timer_Start();
        }
    }
}
