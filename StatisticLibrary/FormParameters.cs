using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Windows.Forms; //Application.ProductVersion


using ASUTP.Database;
using ASUTP.Forms;
using ASUTP.Helper;
using System.Drawing;

namespace StatisticCommon
{
    public abstract partial class FormParameters : FormParametersBase
    {
        public enum PARAMETR_SETUP {
            UNKNOWN = -1
            , POLL_TIME, ERROR_DELAY, MAX_ATTEMPT, WAITING_TIME, WAITING_COUNT,
            MAIN_DATASOURCE, MAIN_PRIORITY,
            /*ALARM_USE, */ ALARM_TIMER_UPDATE, ALARM_EVENT_RETRY, ALARM_TIMER_BEEP, ALARM_SYSTEMMEDIA_TIMERBEEP
            , USERS_DOMAIN_NAME, USERS_ID_TEC, USERS_ID_ROLE
            , SEASON_DATETIME, SEASON_ACTION
            //, GRINVICH_OFFSET_DATETIME
            , APP_VERSION, APP_VERSION_QUERY_INTERVAL
            , KOMDISP_FOLDER_CSV
            , KOMDISP_SHEDULE_START_EXPORT_PBR, KOMDISP_SHEDULE_PERIOD_EXPORT_PBR /* 19/09/2017 ChrjapinAN */
            , LK_FOLDER_CSV /* 19/09/2017 ChrjapinAN */
            //????????????
            , MAINFORMBASE_CONTROLHANDLE_LOGERRORCREATE
            , MAINFORMBASE_SETPBRQUERY_LOGPBRNUMBER, MAINFORMBASE_SETPBRQUERY_LOGQUERY
            // KryapinAN 2018-02-14
            , MAINFORMBASE_SETADMINQUERY_LOGQUERY
            , TECVIEW_LOGRECOMENDATIONVAL, TECVIEW_GETCURRENTTMGEN_LOGWARNING
            , PANELQUICKDATA_LOGDEVIATIONEVAL
            //??????????? ??????????...
            , VALIDATE_TM_VALUE , VALIDATE_ASKUE_VALUE
            , DIAGNOSTIC_TIMER_UPDATE
            ////??? ? ??? ?? ???????????????
            //, ID_SOURCE_SOTIASSO_BTEC, ID_SOURCE_SOTIASSO_TEC2, ID_SOURCE_SOTIASSO_TEC3, ID_SOURCE_SOTIASSO_TEC4, ID_SOURCE_SOTIASSO_TEC5, ID_SOURCE_SOTIASSO_BiTEC
            , IGO_VERSION
            , MODE_REGISTRATION
            , COMMON_AUX_PATH
            , COUNT_PARAMETR_SETUP
        };

        protected static string[] NAME_PARAMETR_SETUP = {
            "Polling period", "Error delay", "Max attempts count", @"Waiting time", @"Waiting count"
            , @"Main DataSource", @"Main Priority"
            /*@"Alarm Use", */, @"Alarm Timer Update" , @"Alarm Event Retry", @"Alarm Timer Beep", @"Alarm SytemMedia FileNam"
            , @"Users DomainName", @"Users ID_TEC", @"Users ID_ROLE"
            , @"Season DateTime", @"Season Action"
            //, @"Grinvich OffsetDateTime"
            , @"App Version", @"App Version Query Interval"
            , @"KomDisp Folder CSV"
            , @"KomDisp Shedule-Start Export PBR", @"KomDisp Shedule-Period Export PBR" /* 19/09/2017 ChrjapinAN */
            , @"LK Folder CSV" /* 19/09/2017 ChrjapinAN */
            //????????????
            , @"ControlHandle LogErrorCreate"
            , @"SetPBRQuery LogPBRNumber", @"SetPBRQuery LogQuery"
            // KryapinAN 2018-02-14
            , @"SetAdminQuery LogQuery"
            , @"TecView LogRecomendation", @"GetCurrentTMGenResponse LogWarning"
            , @"ShowFactValues LogDevEVal"
            //??????????? ??????????...
            , @"Validate TM Value"
            , @"Validate ASKUE Value"
            , @"Diagnostic Timer Update" 
            ////?????????????? ?????????????? ???????? ?????????? ????????
            //, @"ID_SOURCE_SOTIASSO_BTEC", @"ID_SOURCE_SOTIASSO_TEC2", @"ID_SOURCE_SOTIASSO_TEC3", @"ID_SOURCE_SOTIASSO_TEC4", @"ID_SOURCE_SOTIASSO_TEC5", @"ID_SOURCE_SOTIASSO_BiTEC"
            , @"IGO Version"
            , @"Mode Registration"
            , @"COMMON_AUX_PATH"
        };

        protected static string[] NAMESI_PARAMETR_SETUP = {
            "???", "???", "??.", @"????", @"????"
            , @"???", @"???",
            /*@"???", */"???", "???", "???", @"???"
            , @"???", @"???", @"???"
            , @"????/?????", @"???"
            //, "???"
            , @"???", @"????"
            , @"???"
            , @"???", @"???" /* 19/09/2017 ChrjapinAN */
            , @"???" /* 19/09/2017 ChrjapinAN */
            //????????????
            , @"???-???"
            , @"???-???", @"???-???"
            // KryapinAN 2018-02-14
            , @"???-???"
            , @"???-???", @"???-???"
            , @"???-???"
            //??????????? ??????????...
            , @"???"
            , @"???"
            , @"???"
            //?????????????? ?????????????? ???????? ?????????? ????????
            //, @"???", @"???", @"???", @"???", @"???", @"???"
            , @"???"
            , @"???"
            , @"???"
        };

        protected Dictionary<int, string> m_arParametrSetupDefault;
        public Dictionary<int, string> m_arParametrSetup;

        public FormParameters()
            : base()
        {
            InitializeComponent();

            m_arParametrSetup = new Dictionary<int, string>();
            m_arParametrSetup.Add((int)PARAMETR_SETUP.POLL_TIME, @"30");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ERROR_DELAY, @"60");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAX_ATTEMPT, @"3");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.WAITING_TIME, @"106");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.WAITING_COUNT, @"39");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAIN_DATASOURCE, @"671");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAIN_PRIORITY, @"????????");

            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_USE, @"True");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_TIMER_UPDATE, @"300");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_EVENT_RETRY, @"900");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_TIMER_BEEP, @"16");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.ALARM_SYSTEMMEDIA_TIMERBEEP, @"16");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.USERS_DOMAIN_NAME, @"");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.USERS_ID_TEC, @"-1");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.USERS_ID_ROLE, @"-1");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.SEASON_DATETIME, @"26.10.2014 02:00");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.SEASON_ACTION, @"-1");

            ////?? ?? ?? ??????????? ???? 30.10.2014
            //m_arParametrSetup.Add((int)PARAMETR_SETUP.GRINVICH_OFFSET_DATETIME, @"3"); 

            //m_arParametrSetup.Add((int)PARAMETR_SETUP.ID_APP, ((int)ProgramBase.ID_APP.STATISTIC).ToString ());

            m_arParametrSetup.Add((int)PARAMETR_SETUP.APP_VERSION, Application.ProductVersion/*StatisticCommon.Properties.Resources.TradeMarkVersion*/);
            m_arParametrSetup.Add((int)PARAMETR_SETUP.APP_VERSION_QUERY_INTERVAL, @"66666");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.KOMDISP_FOLDER_CSV, @"\\ne2844\2.X.X\???-csv");
            m_arParametrSetup.Add ((int)PARAMETR_SETUP.KOMDISP_SHEDULE_START_EXPORT_PBR, @"2760");
            m_arParametrSetup.Add ((int)PARAMETR_SETUP.KOMDISP_SHEDULE_PERIOD_EXPORT_PBR, @"3600");
            m_arParametrSetup.Add ((int)PARAMETR_SETUP.LK_FOLDER_CSV, @"\\ne2844\2.X.X\???-csv");
            //????????????
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAINFORMBASE_CONTROLHANDLE_LOGERRORCREATE, false.ToString());
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGPBRNUMBER, false.ToString());
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGQUERY, false.ToString());
            // KryapinAN 2018-02-14
            m_arParametrSetup.Add ((int)PARAMETR_SETUP.MAINFORMBASE_SETADMINQUERY_LOGQUERY, false.ToString ());
            m_arParametrSetup.Add((int)PARAMETR_SETUP.TECVIEW_LOGRECOMENDATIONVAL, false.ToString());
            m_arParametrSetup.Add((int)PARAMETR_SETUP.TECVIEW_GETCURRENTTMGEN_LOGWARNING, false.ToString());
            m_arParametrSetup.Add((int)PARAMETR_SETUP.PANELQUICKDATA_LOGDEVIATIONEVAL, false.ToString());

            m_arParametrSetup.Add((int)PARAMETR_SETUP.VALIDATE_TM_VALUE, @"86");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.VALIDATE_ASKUE_VALUE, @"86");
            m_arParametrSetup.Add((int)PARAMETR_SETUP.DIAGNOSTIC_TIMER_UPDATE, @"30000");

            m_arParametrSetup.Add((int)PARAMETR_SETUP.IGO_VERSION, 0.ToString());
            m_arParametrSetup.Add((int)PARAMETR_SETUP.MODE_REGISTRATION, HStatisticUsers.MODE_REGISTRATION.MIXED.ToString());
            m_arParametrSetup.Add((int)PARAMETR_SETUP.COMMON_AUX_PATH, @"\\ne22\lnk,Tepmlate.xls,Sheet1,1,5,25,1.1");

            m_arParametrSetupDefault = new Dictionary<int, string>(m_arParametrSetup);

            this.btnCancel.Location = new System.Drawing.Point(8, 290);
            this.btnOk.Location = new System.Drawing.Point(89, 290);
            this.btnReset.Location = new System.Drawing.Point(170, 290);

            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            mayClose = false;
        }

        public override void Update (out int err)
        {
            throw new NotImplementedException ();
        }

        protected void setDataGUI(bool bInit)
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                if (bInit == true)
                {
                    m_dgvData.Rows.Insert((int)i, new object[] { NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i], NAMESI_PARAMETR_SETUP[(int)i] });

                    m_dgvData.Rows[(int)i].Height = 19;
                    m_dgvData.Rows[(int)i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                    m_dgvData.Rows[(int)i].HeaderCell.Value = ((int)i).ToString();
                }
                else
                    m_dgvData.Rows[(int)i].Cells[1].Value = m_arParametrSetup[(int)i];
            }
        }

        public void ShowDialog (Form parent, Color foreColor, Color backColor)
        {
            BackColor = backColor;
            m_dgvData.DefaultCellStyle.BackColor = backColor == SystemColors.Control ? SystemColors.Window : backColor;
            ForeColor =
            m_dgvData.DefaultCellStyle.ForeColor =
                foreColor;

            ShowDialog (parent);
        }

        //protected override void btnOk_Click(object sender, EventArgs e)
        protected void btnOk_Click(object sender, EventArgs e)
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                m_arParametrSetup[(int)i] = m_dgvData.Rows[(int)i + 0].Cells[1].Value.ToString();
            }

            saveParam();
            mayClose = true;
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                m_arParametrSetup[(int)i] = m_arParametrSetupDefault[(int)i];
            }
        }

        public abstract void SaveParamKey(string column, string value);

        public abstract void IncIGOVersion();

        public static string GetNameParametersOfIndex(int indx)
        {
            return NAME_PARAMETR_SETUP[indx];
        }

        public string GetINIParametersOfID (PARAMETR_SETUP id)
        {
            return GetINIParametersOfID((int)id);
        }

        public string GetINIParametersOfID (int id)
        {
            return m_arParametrSetup [id];
        }
    }

    public partial class FormParameters_FIleINI : FormParameters
    {
        private static string NAME_SECTION_MAIN = "Main settings (" + ProgramBase.AppName + ")";

        private FileINI m_FileINI;

        public FormParameters_FIleINI(string nameSetupFileINI)
            : base()
        {
            m_FileINI = new FileINI(nameSetupFileINI, false);
            //ProgramBase.s_iAppID = (int)ProgramBase.ID_APP.STATISTIC;
            //ProgramBase.s_iAppID = Int32.Parse(m_arParametrSetup[(int)PARAMETR_SETUP.ID_APP]);
            //ProgramBase.s_iAppID = Properties.s

            loadParam(true);
        }

        protected override void loadParam(bool bInit)
        {
            string strDefault = string.Empty;

            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
            {
                m_arParametrSetup[(int)i] = m_FileINI.ReadString(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)i], strDefault);
                if (m_arParametrSetup[(int)i].Equals(strDefault) == true)
                {
                    m_arParametrSetup[(int)i] = m_arParametrSetupDefault[(int)i];
                    m_FileINI.WriteString(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i]);
                }
                else
                    ;
            }

            setDataGUI(bInit);
        }

        protected override void saveParam()
        {
            for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
                m_FileINI.WriteString(NAME_SECTION_MAIN, NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i]);
        }

        public override void SaveParamKey(string key, string value)
        {
            throw new NotImplementedException();
        }

        public override void IncIGOVersion()
        {
            throw new NotImplementedException();
        }
    }

    public partial class FormParameters_DB : FormParameters
    {
        //private ConnectionSettings m_connSett;
        //private DbConnection m_dbConn;

        //public FormParameters_DB(int idListener)
        public FormParameters_DB()
            : base()
        {
            int err = -1;

            loadParam(true, out err);
        }

        public void Update(int idListener, out int err)
        {
            loadParam(false, out err);

            //???????????? ??????????? ????????? ??? ???

            //???????????? ??????????? ????????? ...

            //??????? ????????? ??????????? ?????????
        }

        protected override void loadParam (bool bInit)
        {
            int err = -1;

            loadParam (bInit, out err);

            if (!(err == 0))
                throw new Exception (@"???????? ?????????? ????????????...");
            else
                ;
        }

        private void loadParam(bool bInit, out int err)
        {
            err = -1;

            string query = string.Empty;
            DataTable table = DbTSQLConfigDatabase.DbConfig().GetDataTableSetupParameters(out err);
            DataRow[] rowRes;

            if (err == (int)DbTSQLInterface.Error.NO_ERROR)
                if (!(table == null))
                {
                    query = string.Empty;

                    foreach (PARAMETR_SETUP indx in Enum.GetValues (typeof (PARAMETR_SETUP))) {
                        if ((indx == PARAMETR_SETUP.UNKNOWN)
                            || (indx == PARAMETR_SETUP.COUNT_PARAMETR_SETUP))
                            continue;
                        else
                            ;

                        rowRes = table.Select (@"KEY='" + NAME_PARAMETR_SETUP [(int)indx].ToString () + @"'");
                        switch (rowRes.Length) {
                            case 1:
                                m_arParametrSetup [(int)indx] =
                                m_arParametrSetupDefault [(int)indx] =
                                    rowRes [0] [@"VALUE"].ToString ().Trim ();
                                break;
                            case 0:
                                m_arParametrSetup [(int)indx] = m_arParametrSetupDefault [(int)indx];
                                query += DbTSQLConfigDatabase.DbConfig ().GetSetupParameterQuery (NAME_PARAMETR_SETUP [(int)indx], m_arParametrSetup [(int)indx], DbTSQLInterface.QUERY_TYPE.INSERT) + @";";
                                break;
                            default:
                                break;
                        }
                    }

                    // ????????? ???????? ????? ??????????? ? ???? ? ??????? [setup] ? ?? ????????????
                    if (query.Equals(string.Empty) == false)
                    // ????????? ??????? ? ?? ???????????? ?????????????? ??????????? ? ?? ?????????? ?? ?????????
                        DbTSQLConfigDatabase.DataSource().ExecNonQuery(query, out err);
                    else
                        ;
                }
                else
                    err = (int)DbTSQLInterface.Error.TABLE_NULL;
            else
                ;

            setDataGUI(bInit);
        }

        protected override void saveParam()
        {
            int err = -1;
            string query = string.Empty;

            if (err == 0)
                for (PARAMETR_SETUP i = PARAMETR_SETUP.POLL_TIME; i < PARAMETR_SETUP.COUNT_PARAMETR_SETUP; i++)
                    query += DbTSQLConfigDatabase.DbConfig ().GetSetupParameterQuery (NAME_PARAMETR_SETUP[(int)i], m_arParametrSetup[(int)i], DbTSQLInterface.QUERY_TYPE.UPDATE) + @";";
            else
                ;

            if (query.Equals(string.Empty) == false)
                DbTSQLConfigDatabase.DataSource().ExecNonQuery(query, out err);
            else
                ;
        }

        public override void IncIGOVersion()
        {
            int version = Convert.ToInt32(m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.IGO_VERSION].Trim());
            version++;
            m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.IGO_VERSION] = version.ToString();
            SaveParamKey("IGO Version",version.ToString());
        }

        public override void SaveParamKey(string key, string value)
        {
            int err = -1;
            string query = string.Empty;

            query += DbTSQLConfigDatabase.DbConfig ().GetSetupParameterQuery (key, value, DbTSQLInterface.QUERY_TYPE.UPDATE) + @";";

            if (query.Equals(string.Empty) == false)
                DbTSQLConfigDatabase.DataSource().ExecNonQuery (query, out err);
            else
                ;
        }

        private string readString (string key, string valDef, out int err) {
            return ReadString (key, valDef, out err);
        }

        public static string ReadString(int key, string valDef, out int err)
        {
            return ReadString(NAME_PARAMETR_SETUP[key], valDef, out err);
        }

        public static string ReadString(string key, string valDef, out int err)
        {
            return DbTSQLConfigDatabase.DbConfig ().ReadSetupParameter (key, valDef, out err);
        }
    }
}