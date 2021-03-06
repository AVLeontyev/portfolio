using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using System.Data.Common; //??? DbConnection


using ASUTP.Database;
using ASUTP.Helper;
using ASUTP.Forms;
using ASUTP;
using ASUTP.Core;

namespace StatisticCommon
{
    public abstract partial class FormParametersTG : FormParametersBase
    {
        protected const int COUNT_TG = 8;
        protected int[] indexes_TG_Off = new int[]{1};//?????? ? ????????? ??????????? ??
        protected System.Windows.Forms.TextBox[,] m_array_tbxTG;
        protected System.Windows.Forms.Label[,] m_array_lblTG;

        // ChrjapinAN, 27.12.2017 ??? ??????????? ????????? OBJECT/ITEM
        // ???????? ?????? ????????? ? ?????????? ? ??
        // ???????? ??? ??????? ??? ???????????, ??????? ?? ???????? ?? ??????????
        protected TG.AISKUE_KEY [] m_tg_aiskue_key_default = {
            new TG.AISKUE_KEY() { IdObject = 6001, IdItem = 1 }
            , new TG.AISKUE_KEY() { IdObject = -1, IdItem = -1 } // ??-2 ??????? ?? ???????
            , new TG.AISKUE_KEY() { IdObject = 6002, IdItem = 1 }
            , new TG.AISKUE_KEY() { IdObject = 6002, IdItem = 2 }
            , new TG.AISKUE_KEY() { IdObject = 6002, IdItem = 3 }
            , new TG.AISKUE_KEY() { IdObject = 6002, IdItem = 4 }
            , new TG.AISKUE_KEY() { IdObject = 6002, IdItem = 5 }
            , new TG.AISKUE_KEY() { IdObject = 6002, IdItem = 6 }
        };
        protected int[,] m_tg_id_default = { { 9223, -1, 9431, 9430, 9433, 9435, 9434, 9432 }, { 8436, -1, 8878, 8674, 8980, 9150, 6974, 8266 } };
        protected int[,] m_tg_id;

        //public FormParametersTG(DelegateFunc delParApp)
        public FormParametersTG()
            : base()
        {
            InitializeComponent();

            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            //delegateParamsApply = delParApp;
            
            if ((m_tg_id_default.Length / m_tg_id_default.Rank == COUNT_TG) && (m_tg_id_default.Rank == (int)HDateTime.INTERVAL.COUNT_ID_TIME)) ;
            else ;

            m_tg_id = new int[(int)HDateTime.INTERVAL.COUNT_ID_TIME, COUNT_TG];
            for (int i = (int)HDateTime.INTERVAL.MINUTES; i < (int)HDateTime.INTERVAL.COUNT_ID_TIME; i++)
            {
                for (int j = 0; j < COUNT_TG; j++)
                {
                     m_tg_id[i, j] = m_tg_id_default[i, j];
                }
            }
            m_array_tbxTG = new System.Windows.Forms.TextBox[(int)HDateTime.INTERVAL.COUNT_ID_TIME, COUNT_TG];
            m_array_lblTG = new System.Windows.Forms.Label[(int)HDateTime.INTERVAL.COUNT_ID_TIME, COUNT_TG];

            //m_array_lblTG[(int)HDateTime.INTERVAL.MINUTES, 0] = new Label();
            //m_array_tbxTG[(int)HDateTime.INTERVAL.MINUTES, 0] = tbxTG1Mins;

            for (int i = (int)HDateTime.INTERVAL.MINUTES; i < (int)HDateTime.INTERVAL.COUNT_ID_TIME; i++)
            {
                for (int j = 0; j < COUNT_TG; j++)
                {
                    m_array_lblTG[i, j] = new System.Windows.Forms.Label();
                    m_array_lblTG[i, j].AutoSize = true;
                    m_array_lblTG[i, j].Location = new System.Drawing.Point(12 + i * 164, 9 + j * 27);
                    //m_array_lblTG[i, j].Name = "lblTG1Mins";
                    //m_array_lblTG[i, 0].Size = new System.Drawing.Size(79, 13);
                    m_array_lblTG[i, j].TabIndex = (i * COUNT_TG) + j + 6;
                    m_array_lblTG[i, j].Text = "??" + (j + 1).ToString() + " ";
                    if (i % 2 == 0)
                        m_array_lblTG[i, j].Text += "????????";
                    else
                        m_array_lblTG[i, j].Text += "???????????";

                    //m_array_lblTG[i, j].Anchor = AnchorStyles.Top;

                    this.Controls.Add(m_array_lblTG[i, j]);

                    m_array_tbxTG[i, j] = new System.Windows.Forms.TextBox();
                    m_array_tbxTG[i, j].Location = new System.Drawing.Point(97 + i * 178, 6 + j * 27);
                    //m_array_tbxTG[i, j].Name = "tbxTG1Hours";
                    m_array_tbxTG[i, j].Size = new System.Drawing.Size(70, 20);
                    m_array_tbxTG[i, j].TabIndex = (i * COUNT_TG) + j + 7;
                    this.Controls.Add(m_array_tbxTG[i, j]);
                    if (findElement(indexes_TG_Off, j) == true)
                        m_array_tbxTG[i, j].Enabled = false;
                }
            }

            int posY = m_array_tbxTG[(int)HDateTime.INTERVAL.COUNT_ID_TIME - 1, COUNT_TG - 1].Location.Y + m_array_tbxTG[(int)HDateTime.INTERVAL.COUNT_ID_TIME - 1, COUNT_TG - 1].Size.Height + (COUNT_TG + 1) * 2;
            btnCancel.Location = new System.Drawing.Point(47, posY);
            btnOk.Location = new System.Drawing.Point(148, posY);
            btnReset.Location = new System.Drawing.Point(251, posY);

            this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, btnReset.Location.Y + btnReset.Size.Height + 9);

            mayClose = false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < (int)HDateTime.INTERVAL.COUNT_ID_TIME; i++)
            {
                for (int j = 0; j < COUNT_TG; j++)
                    m_tg_id[i, j] = m_tg_id_default[i, j];
            }

            m_State++;
            
            loadParam (false);
        }

        override protected void buttonCancel_Click(object sender, EventArgs e)
        {
            m_State = 0;
            
            loadParam(false);

            base.buttonCancel_Click(sender, e);
        }

        public int ParamsGetTgId(ASUTP.Core.HDateTime.INTERVAL id_time, int sensor)
        {
            return m_tg_id[(int)id_time, sensor];
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            int tg_id;
            for (int i = 0; i < COUNT_TG; i++)
                if (findElement(indexes_TG_Off, i) == false)
                {
                    // 3-??? ??????????????
                    if (int.TryParse(m_array_tbxTG[(int)HDateTime.INTERVAL.MINUTES, i].Text, out tg_id))
                        m_tg_id[(int)HDateTime.INTERVAL.MINUTES, i] = tg_id;
                    else
                        m_array_tbxTG[(int)HDateTime.INTERVAL.MINUTES, i].Text = m_tg_id[(int)HDateTime.INTERVAL.MINUTES, i].ToString();
                    // 30-???(???????????) ??????????????
                    if (int.TryParse(m_array_tbxTG[(int)HDateTime.INTERVAL.HOURS, i].Text, out tg_id))
                        m_tg_id[(int)HDateTime.INTERVAL.HOURS, i] = tg_id;
                    else
                        m_array_tbxTG[(int)HDateTime.INTERVAL.HOURS, i].Text = m_tg_id[(int)HDateTime.INTERVAL.HOURS, i].ToString();
                }
                else
                    ;

            saveParam();

            //delegateParamsApply();

            mayClose = true;

            if (m_State > 0)
                m_State--;
            else
                ;

            Close();
        }

        protected override void loadParam(bool bInit)
        {
            try
            {
                for (int i = 0; i < (int)HDateTime.INTERVAL.COUNT_ID_TIME; i++)
                {
                    for (int j = 0; j < COUNT_TG; j++)
                    {
                        if(findElement(indexes_TG_Off,j)==false)
                            m_array_tbxTG[i, j].Text = m_tg_id[i, j].ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"FormParametersTG::loadParam () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        protected bool findElement(int[] mass, int elem)
        {
            bool b_Finded = false;
            foreach (int i in mass)
            {
                if (i == elem)
                {
                    b_Finded = true;
                    break;
                }
            }
            return b_Finded;
        }
    }

    public partial class FormParametersTG_FileINI : FormParametersTG
    {
        private enum TYPE_VALUE : int { CURRENT, PREVIOUS, COUNT_TYPE_VALUE };
        
        private const char SEP_ID_TG = ',';
        private string[] NAME_TIME = { "min", "hour" };
        private static string NAME_SECTION_TG_ID = "?????????????? ?? ????? (" + ProgramBase.AppName + ")";

        FileINI m_FileINI;

        public FormParametersTG_FileINI (string nameSetupFileINI)
        {
            m_FileINI = new FileINI (nameSetupFileINI, false);

            loadParam(true);
        }

        //TODO: ???????
        public override void Update (out int err)
        {
            throw new NotImplementedException ();
        }

        protected override void loadParam(bool bInit)
        {
            string key_value;
            string[] key_values;
            int tg_id;

            for (int i = 0; i < (int)HDateTime.INTERVAL.COUNT_ID_TIME; i++)
            {
                for (int j = 0; j < COUNT_TG; j++)
                {
                    key_value = m_FileINI.ReadString(NAME_SECTION_TG_ID, "TG" + (j + 1).ToString() + " " + NAME_TIME[i], null);
                    if (key_value.Length > 0)
                    {
                        key_values = key_value.Split(SEP_ID_TG);
                        if (int.TryParse(key_values[(int)TYPE_VALUE.CURRENT], out tg_id)) m_tg_id[i, j] = tg_id; else ;
                        if (key_values.GetLength(0) > 1)
                            if (int.TryParse(key_values[(int)TYPE_VALUE.PREVIOUS], out tg_id)) m_tg_id_default[i, j] = tg_id; else ;
                        else
                        {
                            m_tg_id_default[i, j] = tg_id;
                            WriteTGIds(i, j);
                        }
                    }
                    else
                    {
                        WriteTGIds(i, j);
                    }
                }
            }

            base.loadParam(bInit);
        }

        private void WriteTGIds(int id_time, int num_tg)
        {
            string key_value;

            key_value = m_tg_id[id_time, num_tg].ToString();
            key_value += SEP_ID_TG;
            key_value += m_tg_id_default[id_time, num_tg].ToString();
            m_FileINI.WriteString(NAME_SECTION_TG_ID, "TG" + (num_tg + 1).ToString() + " " + NAME_TIME[id_time], key_value);
        }

        protected override void saveParam()
        {
            for (int i = 0; i < (int)HDateTime.INTERVAL.COUNT_ID_TIME; i++)
            {
                for (int j = 0; j < COUNT_TG; j++)
                {
                    WriteTGIds(i, j);
                }
            }
        }
    }

    public partial class FormParametersTG_DB : FormParametersTG
    {
        private string[] NAME_FIELDS_TIME = { "ID_3", "ID_30" };
        private const string SENSORS_NAME_PREFIX = @"??"
            , SENSORS_NAME_POSTFIX = @" Pmanual"; //??????????? ? ????????
        
        TEC m_tec;

        public FormParametersTG_DB(List <TEC> list_tec)
        {            
            //????? ??????? ???
            int indx_tec = -1;
            foreach (TEC t in list_tec) {
                if (t.Type == TEC.TEC_TYPE.BIYSK)
                {
                    indx_tec = list_tec.IndexOf (t);
                } else { }
            }

            if (indx_tec < 0)
                throw new Exception(@"FormParametersTG_DB::??????????? () - ??????? ??? ? ?????? ?? ???????...");
            else
                m_tec = list_tec [indx_tec];

            loadParam(true);
        }

        private void start () {
#if MODE_STATIC_CONNECTION_LEAVING
            DbTSQLConfigDatabase.ModeStaticConnectionLeave = DbTSQLInterface.ModeStaticConnectionLeaving.Yes;
#else
#endif
        }

        private void stop () {
#if MODE_STATIC_CONNECTION_LEAVING
            DbTSQLConfigDatabase.ModeStaticConnectionLeave = DbTSQLInterface.ModeStaticConnectionLeaving.No;
#else
#endif
        }

        protected override void loadParam(bool bInit)
        {
            int j = -1
                , err = -1
                , tg_id;

            start ();

            DataTable tblTGSensors = DbTSQLConfigDatabase.DbConfig ().GetDataTableParametersBiyskTG ((int)m_State, out err);
            DataRow [] rowsRes;

            if (err == 0)
            {    
                for (j = 0; j < COUNT_TG; j++)
                {
                    if (findElement(indexes_TG_Off, j) == false)
                    {
                        rowsRes = tblTGSensors.Select(DbTSQLConfigDatabase.GetWhereParameterBiyskTG (j + 1, SENSORS_NAME_PREFIX, SENSORS_NAME_POSTFIX));
                        if (rowsRes.Length == 1)
                            for (int i = (int)HDateTime.INTERVAL.MINUTES; i < (int)HDateTime.INTERVAL.COUNT_ID_TIME; i++)
                                if (int.TryParse(rowsRes[0][NAME_FIELDS_TIME[i]].ToString(), out tg_id) == true)
                                    m_tg_id[i, j] = tg_id;
                                else ;
                        else
                            break;
                    }
                }

                if (! (j < COUNT_TG)) {
                    //tblTGSensors = DbTSQLInterface.Select(ref conn, getQueryParam((int)TYPE_VALUE.PREVIOUS), null, null, out err);
                    tblTGSensors = DbTSQLConfigDatabase.DbConfig ().GetDataTableParametersBiyskTG (m_State + 1, out err);

                    if (err == 0) {
                        if (tblTGSensors.Rows.Count < COUNT_TG)
                            err = -2;
                        else
                            ;
                    } else {
                    }

                    if (err == 0) {
                        for (j = 0; j < COUNT_TG; j++)
                        {
                            if (findElement(indexes_TG_Off, j) == false)
                            {
                                rowsRes = tblTGSensors.Select(DbTSQLConfigDatabase.GetWhereParameterBiyskTG(j + 1, SENSORS_NAME_PREFIX, SENSORS_NAME_POSTFIX));
                                if (rowsRes.Length == 1)
                                {
                                    for (int i = (int)HDateTime.INTERVAL.MINUTES; i < (int)HDateTime.INTERVAL.COUNT_ID_TIME; i++)
                                        if (int.TryParse(rowsRes[0][NAME_FIELDS_TIME[i]].ToString(), out tg_id) == true)
                                            m_tg_id_default[i, j] = tg_id;
                                        else ;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    } else {
                        for (j = 0; j < COUNT_TG; j++)
                        {
                            if (findElement(indexes_TG_Off, j) == false)
                            {
                                for (int i = (int)HDateTime.INTERVAL.MINUTES; i < (int)HDateTime.INTERVAL.COUNT_ID_TIME; i++)
                                {
                                    m_tg_id_default[i, j] = m_tg_id[i, j];
                                }
                            }
                        }
                    }

                    if (!(j < COUNT_TG))
                        base.loadParam (bInit);
                    else
                        ;
                }
                else
                    ; //??????
            }
            else
                //?????? ????????? ??????? "??????????" ? ?? ????????????
                Logging.Logg().Error(string.Format(@"FormParametrsTG::loadParam ({0}) - ??????: [{1}]..."
                        , bInit, DbTSQLConfigDatabase.GetParametersBiyskTGQuery (m_State + 1))
                    , Logging.INDEX_MESSAGE.NOT_SET);

            stop ();
        }

        //TODO: ???????
        public override void Update (out int err)
        {
            throw new NotImplementedException ();
        }

        protected override void saveParam()
        {
            int err = -1;
            string queryInsert = string.Empty;
            List<TECComponentBase> listTG;

            listTG = m_tec.GetListLowPointDev (TECComponentBase.TYPE.ELECTRO);

            for (int j = 0; j < COUNT_TG; j++)
            {
                if (findElement (indexes_TG_Off, j) == false) {
                    queryInsert += DbTSQLConfigDatabase.GetParameterBiyskTGInsertQuery (m_tec.m_id
                        , $"'{SENSORS_NAME_PREFIX}{(j + 1).ToString ()}{SENSORS_NAME_POSTFIX}',"
                        , m_tec.GetListLowPointDev (TECComponentBase.TYPE.ELECTRO).Find (x => x.name_shr == SENSORS_NAME_PREFIX + (j + 1).ToString ()).m_id
                        , int.Parse(m_array_tbxTG [(int)HDateTime.INTERVAL.MINUTES, j].Text.Trim ())
                        , int.Parse(m_array_tbxTG [(int)HDateTime.INTERVAL.HOURS, j].Text.Trim ())
                        , m_tg_aiskue_key_default [j].IdObject
                        , m_tg_aiskue_key_default [j].IdItem);
                }
            }

            DbTSQLConfigDatabase.DataSource().ExecNonQuery (queryInsert, out err);
        }
    }
}