using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;
using StatisticTrans;
using StatisticTransModes;

namespace trans_mt
{
    public partial class FormMainTransMT : FormMainTransModes
    {
        public FormMainTransMT()
            : base(ASUTP.Helper.ProgramBase.ID_APP.TRANS_MODES_TERMINALE, new KeyValuePair<string, string> [] {
                new KeyValuePair<string, string> (@"ИгнорДатаВремя-ModesTerminale", false.ToString())
            })
        {
            this.notifyIconMain.Icon =
            this.Icon = trans_mt.Properties.Resources.statistic6;

            InitializeComponentTransDB ();

            m_dgwAdminTable.Size = new System.Drawing.Size(498, 471);
        }

        protected override void Start()
        {
            int i = -1;

            EditFormConnectionSettings("connsett_mt.ini", true);

            bool bIgnoreTECInUse = false;

            ASUTP.Core.HMark markQueries = new ASUTP.Core.HMark (new int[] { (int)StatisticCommon.CONN_SETT_TYPE.ADMIN, (int)StatisticCommon.CONN_SETT_TYPE.PBR, (int)StatisticCommon.CONN_SETT_TYPE.MTERM });
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.ADMIN);
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.PBR);
            //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.MTERM);

            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings (s_listFormConnectionSettings [(int)StatisticCommon.CONN_SETT_TYPE.CONFIG_DB].getConnSett (i));
                DbTSQLConfigDatabase.DbConfig ().Register();

                if (! (DbTSQLConfigDatabase.DbConfig ().ListenerId < 0))
                {
                    switch (i)
                    {
                        case (Int16)CONN_SETT_TYPE.SOURCE:
                            m_arAdmin[i] = new AdminMT();
                            break;
                        case (Int16)CONN_SETT_TYPE.DEST:
                            m_arAdmin[i] = new AdminTS_Modes(new bool[] { false, true });
                            break;
                        default:
                            break;
                    }
                    try
                    {
                        m_arAdmin[i].InitTEC(m_modeTECComponent, /*arTypeConfigDB [i], */markQueries, bIgnoreTECInUse, new int[] { 0, (int)TECComponent.ID.LK });
                        RemoveTEC(m_arAdmin[i]);
                    }
                    catch (Exception e)
                    {
                        ASUTP.Logging.Logg().Exception(e, "FormMainTransMT::FormMainTransMT ()", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                        //ErrorReport("Ошибка соединения. Переход в ожидание.");
                        //setUIControlConnectionSettings(i);
                        break;
                    }
                    switch (i)
                    {
                        case (Int16)CONN_SETT_TYPE.SOURCE:
                            m_arAdmin[i].m_ignore_date = bool.Parse (FileAppSettings.This ().GetValue(@"ИгнорДатаВремя-ModesTerminale"));
                            break;
                        case (Int16)CONN_SETT_TYPE.DEST:
                            //if (strTypeField.Equals(AdminTS.TYPE_FIELDS.DYNAMIC.ToString()) == true)
                            //    ((AdminTS)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
                            //else if (strTypeField.Equals(AdminTS.TYPE_FIELDS.STATIC.ToString()) == true)
                            //    ((AdminTS)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.STATIC;
                            //else
                            //    ;
                            m_arAdmin[i].m_ignore_date = bool.Parse (FileAppSettings.This ().GetValue(@"ИгнорДатаВремя-techsite"));
                            break;
                        default:
                            break;
                    }

                    //m_arAdmin[i].m_ignore_connsett_data = true; //-> в конструктор

                    DbTSQLConfigDatabase.DbConfig ().UnRegister();
                }
                else
                    ;
            }

            if (!(i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE))
            {
                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);

                for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                {
                    //setUIControlConnectionSettings(i); //??? Перенос ДО цикла

                    m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                    //m_arAdmin[i].SetDelegateWait(new DelegateFunc(StartWait), new DelegateFunc(StopWait), delegateEvent);
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

        protected override void buttonSaveSourceSett_Click(object sender, EventArgs e)
        {
        }

        protected override void setUIControlSourceState()
        {
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object sender, EventArgs ev)
        {
            if (IsCanSelectedIndexChanged == true)
            {
                base.comboBoxTECComponent_SelectedIndexChanged(sender, ev);

                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.DEST);
                setUIControlConnectionSettings((Int16)CONN_SETT_TYPE.SOURCE);
            }
            else
                ;
        }

        protected override void timer_Start()
        {
            base.timer_Start();
        }
    }
}
