using System;

using StatisticCommon;
using StatisticTrans;
using System.Drawing;
using ASUTP;
using ASUTP.Database;

namespace trans_gtp
{
    public partial class FormMainTransGTP : FormMainTrans
    {
        public FormMainTransGTP()
            : base(FileAppSettings.This().GetIdApplication
                    , new System.Collections.Generic.KeyValuePair<string, string> [] {
                        new System.Collections.Generic.KeyValuePair<string, string> (@"ИгнорДатаВремя-techsite", false.ToString ())
                        , new System.Collections.Generic.KeyValuePair<string, string> (@"ТЭЦПараметрыНазначение", @"{}")
                    }
            )
        {
            InitializeComponentTransDB();

            this.m_dgwAdminTable = new StatisticCommon.DataGridViewAdminKomDisp(SystemColors.ControlText, new Color [] { SystemColors.Window, Color.Yellow, Color.Red });
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).BeginInit();
            this.SuspendLayout();
            // 
            // m_dgwAdminTable
            // 
            this.m_dgwAdminTable.Location = new System.Drawing.Point(319, 5);
            this.m_dgwAdminTable.Name = "m_dgwAdminTable";
            this.m_dgwAdminTable.RowHeadersVisible = false;
            this.m_dgwAdminTable.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_dgwAdminTable.Size = new System.Drawing.Size(498, 471);
            this.m_dgwAdminTable.TabIndex = 27;
            this.panelMain.Controls.Add(this.m_dgwAdminTable);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).EndInit();
            this.ResumeLayout(false);

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTrans));
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic4"))); //$this.Icon
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic4"))); //$this.Icon

            m_modeTECComponent = FormChangeMode.MODE_TECCOMPONENT.GTP;

            //Созжание массива для объектов получения данных
            m_arAdmin = new AdminTS[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            Start();
        }

        protected override void start () {
            base.Start ();
        }
        
        protected override void Start()
        {
            int i = -1;

            EditFormConnectionSettings("connsett_gtp.ini", true);

            bool bIgnoreDateTime = false;
            if (Boolean.TryParse(FileAppSettings.This().GetValue(@"ИгнорДатаВремя-techsite"), out bIgnoreDateTime) == false)
                bIgnoreDateTime = false;
            else
                ;

            ASUTP.Core.HMark markQueries = new ASUTP.Core.HMark (0);
            markQueries.Set((int)StatisticCommon.CONN_SETT_TYPE.PBR, ОпросППБРToolStripMenuItem.Checked);
            markQueries.Set((int)StatisticCommon.CONN_SETT_TYPE.ADMIN, ОпросАдминЗначенияToolStripMenuItem.Checked);

            // определить пользователя по 1-ой БД конфигурации
            DbTSQLConfigDatabase.DbConfig().Register ();
            try {
                using (HStatisticUsers users = new HStatisticUsers(DbTSQLConfigDatabase.DbConfig ().ListenerId, ASUTP.Helper.HUsers.MODE_REGISTRATION.MIXED)) {; }
            } catch (Exception e) {
                Logging.Logg().Exception(e, "FormMainTransGTP::Start () - HStatisticUser initialize...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            //Инициализация объектов получения данных
            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                bool bPPBRSavedValues = false;
                if (i == (Int16)CONN_SETT_TYPE.DEST)
                    bPPBRSavedValues = СохранППБРToolStripMenuItem.Checked;
                else
                    ;
                m_arAdmin[i] = new AdminTS_KomDisp(new bool[] { false, bPPBRSavedValues });
                
                try
                {
                    //((AdminTS_KomDisp)m_arAdmin[i]).InitTEC(m_formConnectionSettingsConfigDB.getConnSett((Int16)CONN_SETT_TYPE.DEST), m_modeTECComponent, true, false);
                    m_arAdmin[i].InitTEC(m_modeTECComponent, /*arTypeConfigDB[i], */markQueries, true, new int[] { 0, (int)TECComponent.ID.GTP });
                    RemoveTEC(m_arAdmin[i]);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "FormMainTransGTP::FormMainTransGTP ()", Logging.INDEX_MESSAGE.NOT_SET);
                    //ErrorReport("Ошибка соединения. Перехож в ожидание.");
                    //setUIControlConnectionSettings(i);
                    break;
                }

                ////Для переназначения идентификаторов источников данных БийскТЭЦ
                //int j = -1;
                //string val = m_fileINI.GetValueOfKey (@"ID_БДНазначение_PPBR_PBR");
                //val = val.Split (',')[0];
                //for (j = 0; j < m_arAdmin[i].m_list_tec.Count; j ++) {
                //    if (m_arAdmin[i].m_list_tec[j].m_id == Int32.Parse (val))
                //        break;
                //    else
                //        ;
                //}

                //if (j < m_arAdmin[i].m_list_tec.Count) {
                //} else {
                //}

                if ((i == (int)CONN_SETT_TYPE.DEST)
                    /*&& (arTypeConfigDB[(int)CONN_SETT_TYPE.DEST] == TYPE_DATABASE_CFG.CFG_190)*/
                    )
                {
                    string strTECParametersDest = FileAppSettings.This ().GetValue(@"ТЭЦПараметрыНазначение");
                    if (strTECParametersDest.Equals (string.Empty) == false) {
                        ////if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == Convert.ToInt32 (list_tec.Rows[i]["ID"]))) {
                        //    int err = -1
                        //        , indx = -1
                        //        , indx_tec = -1;

                        //    foreach (TEC t in m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec)
                        //        if (t.m_id == 6) //Идентификатор БиТЭЦ
                        //        {
                        //            indx_tec = m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec.IndexOf(t);
                        //            break;
                        //        }
                        //        else
                        //            ;

                        //    if (!(indx_tec < 0))
                        //    {
                        //        m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].m_arNameTableAdminValues[(int)((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).m_typeFields] = @"";
                        //        m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].m_arNameTableUsedPPBRvsPBR[(int)((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).m_typeFields] = @"BiPPBRvsPBR"; //???

                        //        m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].SetNamesField(@"", //ADMIN_DATETIME
                        //                            @"", //ADMIN_REC
                        //                            @"", //ADMIN_IS_PER
                        //                            @"", //ADMIN_DIVIAT
                        //                            @"Date_time", //PBR_DATETIME
                        //                            @"PBR", //PPBRvsPBR
                        //                            @"PBR_number");

                        //        m_arAdmin[(int)CONN_SETT_TYPE.DEST].m_list_tec[indx_tec].connSettings(ConnectionSettingsSource.GetConnectionSettings(TYPE_DATABASE_CFG.CFG_190, idListener, 103, -1, out err), (int)StatisticCommon.CONN_SETT_TYPE.PBR);
                        //    }
                        //    else ;
                        ////}
                        ////else ;
                    }
                    else ;
                }
                else {
                }

                //for (AdminTS.TYPE_FIELDS tf = AdminTS.TYPE_FIELDS.STATIC; i < (int)AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS; tf++)
                //    if (arStrTypeField[i].Equals(tf.ToString()) == true)
                //    {
                //        ((AdminTS)m_arAdmin[i]).m_typeFields = tf;
                //        break;
                //    }
                //    else
                //        ;

                m_arAdmin[i].m_ignore_date = bIgnoreDateTime;
                //m_arAdmin[i].m_ignore_connsett_data = true; //-> в конструктор

                setUIControlConnectionSettings(i);

                m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                //m_arAdmin[i].SetDelegateWait(new DelegateFunc(StartWait), new DelegateFunc (StopWait), delegateEvent);
                m_arAdmin[i].SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);

                m_arAdmin[i].SetDelegateData(setDataGridViewAdmin, errorDataGridViewAdmin);
                m_arAdmin[i].SetDelegateSaveComplete(saveDataGridViewAdminComplete);

                m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                m_arAdmin[i].Start();

                DbTSQLConfigDatabase.DbConfig ().UnRegister ();
                switch ((CONN_SETT_TYPE)i) {
                    case CONN_SETT_TYPE.SOURCE:
                        // 1-ый источник инициализировали, подключаем БД конфигурации 2-го источника(назначение)
                        DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings (s_listFormConnectionSettings [(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].getConnSett (i + 1));
                        DbTSQLConfigDatabase.DbConfig ().Register ();
                        break;
                    case CONN_SETT_TYPE.DEST:
                        //??? восстановить исходный источник данных
                        break;
                }
            }

            if (!(i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE))
            {
                start ();
            }
            else
                ;
        }

        protected override void getDataGridViewAdmin(int indxDB)
        {
            //int indxDB = m_IndexDB;

            double value;
            bool valid;

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < (int)DataGridViewAdminKomDisp.COLUMN_INDEX.TO_ALL; j++)
                {
                    switch (j)
                    {
                        case (int)DataGridViewAdminKomDisp.COLUMN_INDEX.PLAN: // План
                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.PLAN].Value, out value);
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr = value;
                            //((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pbr;
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pmin = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pmin;
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pmax = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pmax;

                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr_number = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pbr_number;
                            break;
                        case (int)DataGridViewAdminKomDisp.COLUMN_INDEX.RECOMENDATION: // Рекомендация
                            //cellValidated(e.RowIndex, (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION);

                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.RECOMENDATION].Value, out value);
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].recomendation = value;

                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].dtRecUpdate = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].dtRecUpdate;

                            break;
                        case (int)DataGridViewAdminKomDisp.COLUMN_INDEX.FOREIGN_CMD:
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].fc = bool.Parse(this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.FOREIGN_CMD].Value.ToString());
                            break;
                        case (int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION_TYPE:
                            {
                                ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviationPercent = bool.Parse(this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION_TYPE].Value.ToString());
                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION: // Максимальное отклонение
                            {
                                valid = double.TryParse((string)this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION].Value, out value);
                                ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviation = value;

                                break;
                            }
                        default:
                            break;
                    }
                }
            }

            m_arAdmin[indxDB].CopyCurToPrevRDGValues();
        }

        /// <summary>
        /// Обновить значения в представлении
        /// </summary>
        /// <param name="date">Дата за которую требуется обновить значения</param>
        /// <param name="bNewValues">Признак наличия новых значений (false - обновление оформления представления со старыми значениями при изменении цветовой схемы)</param>
        protected override void updateDataGridViewAdmin(DateTime date, bool bNewValues)
        {
            int indxDB = m_IndexDB;

            for (int i = 0; i < 24; i++)
            {
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DATE_HOUR].Value = date.AddHours(i + 1).ToString("dd-MM-yyyy HH:00");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.PLAN].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr.ToString("F2");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.PLAN].ToolTipText = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr_number;
                if (i > 0)
                    this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.UDGe].Value = (((m_arAdmin[indxDB].m_curRDGValues[i].pbr + m_arAdmin[indxDB].m_curRDGValues[i - 1].pbr) / 2) + m_arAdmin[indxDB].m_curRDGValues[i].recomendation).ToString("F2");
                else
                    this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.UDGe].Value = (((m_arAdmin[indxDB].m_curRDGValues[i].pbr + ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues_PBR_0) / 2) + m_arAdmin[indxDB].m_curRDGValues[i].recomendation).ToString("F2");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.RECOMENDATION].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].recomendation.ToString("F2");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.RECOMENDATION].ToolTipText = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].dtRecUpdate.ToString();
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.FOREIGN_CMD].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].fc.ToString();
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION_TYPE].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviationPercent.ToString();
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviation.ToString("F2");
            }

            //m_arAdmin[indxDB].CopyCurToPrevRDGValues ();

            //this.m_dgwAdminTable.Invalidate();
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object cbx, EventArgs ev)
        {
            if (IsCanSelectedIndexChanged == true)
            {
                ClearTables();

                short indexDB = m_IndexDB;

                switch (m_modeTECComponent)
                {
                    case FormChangeMode.MODE_TECCOMPONENT.GTP:
                        ((AdminTS)m_arAdmin[indexDB]).GetRDGValues(SelectedItemKey, dateTimePickerMain.Value.Date);
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TG:
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TEC:
                        break;
                    default:
                        break;
                }

                setUIControlConnectionSettings((int)CONN_SETT_TYPE.SOURCE);
                setUIControlConnectionSettings((int)CONN_SETT_TYPE.DEST);
            }
            else
                ;
        }

        protected override void timer_Start()
        {
            base.timer_Start();
        }

        protected override void buttonSaveSourceSett_Click(object sender, EventArgs e) 
        {

        }

        protected override void setUIControlSourceState()
        {

        }
    }
}
