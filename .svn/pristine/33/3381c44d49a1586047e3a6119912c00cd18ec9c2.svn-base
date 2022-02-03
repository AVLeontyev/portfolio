using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.Common;
//using System.Data.Odbc;


using StatisticCommon;

namespace trans_mc_cmd
{
    abstract class PPBR_Record
    {
        /// <summary>
        /// Целевое время, которому соответствуют все показатели. Московское.
        /// </summary>
        public DateTime date_time;
        /// <summary>
        /// Дата-время записи (обновления). Новосибирское.
        /// </summary>
        public DateTime wr_date_time;
        public string PBR_number;
        /// <summary>
        /// Указатель на рабочий экземпляр класса, из которого будут использоваться экземпляры этого класса. Для обращения к методу того класса.
        /// </summary>
        public MySQLtechsite parent;

        public abstract string GenUpdateStatement(DateTime dtMskNow);

        /// <summary>
        /// Читает данные в поля экземпляра класса из базы на указанное время.
        /// </summary>
        public abstract bool ReadFromDatabase(DateTime DT);

        /// <summary>
        /// Сохраняет данные в переменных класса.
        /// </summary>
        public abstract bool SetValues(int id, MySQLtechsite.Params PAR, double val);
    }

    class HTECComponentsRecord : PPBR_Record
    {
        public SortedList<int, double?[]> m_srtlist_ppbr;

        public HTECComponentsRecord(List<int> ids)
        {
            int i = -1;
            m_srtlist_ppbr = new SortedList<int, double?[]>();

            for (i = 0; i < ids.Count; i++)
            {
                if (ids[i] > 0)
                    m_srtlist_ppbr.Add(ids[i], new double?[(int)MySQLtechsite.Params.COUNT_PARAMS]);
                else
                    ;
            }
        }

        public override bool ReadFromDatabase(DateTime DT)
        {
            bool bRes = true;

            int err = -1
                /*, i = -1*/;

            DataTable rec;
            //rec = DbTSQLInterface.Select(parent.m_MySQLConnections[(int)MySQLtechsite.CONN_SETT_TYPE.PPBR], "SELECT * FROM " + parent.m_strTableNamePPBR + " WHERE date_time = ?", new DbType[] { DbType.DateTime }, new object[] { DT }, out err);
            rec = ASUTP.Database.DbTSQLInterface.Select(ref parent.m_connection, "SELECT * FROM " + parent.m_strTableNamePPBR + " WHERE date_time = @0", new DbType[] { DbType.DateTime }, new object[] { DT }, out err);

            if ((rec.Rows.Count == 1) && (err == 0))
            {
                throw new NotImplementedException();

                string prefix_comp = string.Empty;

                foreach (KeyValuePair<int, double?[]> pair in m_srtlist_ppbr)
                {
                    //NotImplementedException
                    //prefix_comp = parent.GetPrefixOfId(pair.Key, false);

                    for (MySQLtechsite.Params par = 0; par < MySQLtechsite.Params.COUNT_PARAMS; par++)
                    {
                        pair.Value[(int)par] = Convert.ToDouble(rec.Rows[0][prefix_comp + @"_" + par.ToString()]);
                    }
                }
            }
            else
            {
                bRes = false;

                if (rec.Rows.Count > 1)
                {
                    //Вообще ошибка! 2-х строк за одну дату НЕ может быть
                }
                else
                    ; //err тоже при анализе следовало бы учитывать...
            }

            return bRes;
        }

        public override string GenUpdateStatement(DateTime dtMskNow)
        {
            string strRes = string.Empty;

            string prefix = string.Empty;
            int? iId = -1;
            int i = -1;

            if (date_time > dtMskNow)
            {
                throw new NotImplementedException ();

                iId = parent.Insert48HalfHoursIfNeedAndGetId(date_time);

                strRes = "UPDATE " + parent.m_strTableNamePPBR + " SET wr_date_time = '@wr_date_time', PBR_number = '@PBR_number'";

                bool bPrefixOwnerOnly = false;
                string strNameField = string.Empty;

                foreach (KeyValuePair<int, double?[]> pair in m_srtlist_ppbr)
                {
                    if ((pair.Key > 0) && (pair.Key < 100))
                        bPrefixOwnerOnly = true;
                    else
                        if ((pair.Key > 100) && (pair.Key < 500))
                            bPrefixOwnerOnly = false;
                        else
                            ;
                    //NotImplementedException
                    //prefix = parent.GetPrefixOfId(pair.Key, bPrefixOwnerOnly);

                    for (MySQLtechsite.Params par = 0; par < MySQLtechsite.Params.COUNT_PARAMS; par++)
                    {
                        strNameField = prefix + @"_" + par.ToString();
                        strRes += @", " + strNameField + @" = '" + ((double?)pair.Value[(int)par]).ToString() + "'";
                    }
                }

                strRes += " WHERE id = @id";

                strRes = strRes.Replace("@wr_date_time", wr_date_time.ToString("u").Replace("Z", ""));
                strRes = strRes.Replace("@PBR_number", PBR_number);
                strRes = strRes.Replace("@id", iId.ToString());
            }
            else
                ;

            return strRes;
        }

        private void AddValues(int id, KeyValuePair<int, double?[]> pair_src)
        {
            int indx = -1;
            MySQLtechsite.Params par;

            indx = m_srtlist_ppbr.IndexOfKey(id);

            if (indx < 0)
            {
                m_srtlist_ppbr.Add(id, new double?[(int)MySQLtechsite.Params.COUNT_PARAMS]);
                indx = m_srtlist_ppbr.Keys.IndexOf(id);
                for (par = MySQLtechsite.Params.PBR; par < MySQLtechsite.Params.COUNT_PARAMS; par++)
                    m_srtlist_ppbr.Values[indx][(int)par] = 0.0;
            }
            else
                ;

            for (par = MySQLtechsite.Params.PBR; par < MySQLtechsite.Params.COUNT_PARAMS; par++)
                m_srtlist_ppbr.Values[indx][(int)par] += pair_src.Value[(int)par];
        }

        public void GenerateOwnerValues()
        {
            int i = -1
                , idOwner = -1
                , indxOwner = -1;
            MySQLtechsite.Params par;
            TECComponent comp;
            List<int> list_idTECComponents = new List<int>();

            SortedList<int, double?[]> srtlist_ppbrMC = new SortedList<int, double?[]>();
            double?[] val_dest;

            //Копия словаря с ID_MC
            foreach (KeyValuePair<int, double?[]> pair_src in m_srtlist_ppbr)
            {
                val_dest = new double?[(int)MySQLtechsite.Params.COUNT_PARAMS];
                for (par = MySQLtechsite.Params.PBR; par < MySQLtechsite.Params.COUNT_PARAMS; par++)
                    val_dest[(int)par] = pair_src.Value[(int)par];

                srtlist_ppbrMC.Add(pair_src.Key, val_dest);
            }

            //Обнуление; подготовка к конвертации ID_MC -> ID
            m_srtlist_ppbr.Clear();

            for (i = 0; i < srtlist_ppbrMC.Count; i++)
            {
                comp = parent.GetTECComponentOfIdMC(srtlist_ppbrMC.Keys[i]);

                //Добавление значений за объект
                AddValues(comp.tec.m_id, srtlist_ppbrMC.ElementAt(i));
                //Добавление значений за компонент
                AddValues(comp.m_id, srtlist_ppbrMC.ElementAt(i));
            }
        }

        public override bool SetValues(int id, MySQLtechsite.Params PAR, double val)
        {
            bool bRes = false;

            m_srtlist_ppbr[id][(int)PAR] = val;

            return bRes;
        }
    }

    /// <summary>
    /// Представляет одну запись в таблице PPBRvsPBR базы techsite на MySQL, соответствующую получасовому интервалу.
    /// </summary>
    class OneRecord : PPBR_Record
    {
        public double? TEC2_PBR;
        public double? TEC2_Pmax;
        public double? TEC2_Pmin;

        public double? BTEC_PBR;
        public double? BTEC_Pmax;
        public double? BTEC_Pmin;
        public double? BTEC_TG1_PBR;
        public double? BTEC_TG1_Pmax;
        public double? BTEC_TG1_Pmin;
        public double? BTEC_TG2_PBR;
        public double? BTEC_TG2_Pmax;
        public double? BTEC_TG2_Pmin;
        public double? BTEC_TG4_PBR;
        public double? BTEC_TG4_Pmax;
        public double? BTEC_TG4_Pmin;
        public double? BTEC_TG35_PBR;
        public double? BTEC_TG35_Pmax;
        public double? BTEC_TG35_Pmin;

        public double? TEC4_PBR;
        public double? TEC4_Pmax;
        public double? TEC4_Pmin;
        public double? TEC4_TG3_PBR;
        public double? TEC4_TG3_Pmax;
        public double? TEC4_TG3_Pmin;
        public double? TEC4_TG48_PBR;
        public double? TEC4_TG48_Pmax;
        public double? TEC4_TG48_Pmin;

        public double? TEC3_PBR;
        public double? TEC3_Pmax;
        public double? TEC3_Pmin;
        public double? TEC3_TG1_PBR;
        public double? TEC3_TG1_Pmax;
        public double? TEC3_TG1_Pmin;
        public double? TEC3_TG5_PBR;
        public double? TEC3_TG5_Pmax;
        public double? TEC3_TG5_Pmin;
        public double? TEC3_TG712_PBR;
        public double? TEC3_TG712_Pmax;
        public double? TEC3_TG712_Pmin;
        public double? TEC3_TG1314_PBR;
        public double? TEC3_TG1314_Pmax;
        public double? TEC3_TG1314_Pmin;

        public double? TEC5_PBR;
        public double? TEC5_Pmax;
        public double? TEC5_Pmin;
        public double? TEC5_TG12_PBR;
        public double? TEC5_TG12_Pmax;
        public double? TEC5_TG12_Pmin;
        public double? TEC5_TG36_PBR;
        public double? TEC5_TG36_Pmax;
        public double? TEC5_TG36_Pmin;

        public override bool ReadFromDatabase(DateTime DT)
        {
            int err = -1;

            date_time = DT;

            DataTable rdr;
            //rdr = DbTSQLInterface.Select(parent.m_MySQLConnections [(int)MySQLtechsite.CONN_SETT_TYPE.PPBR], "SELECT * FROM PPBRvsPBRnew where date_time = ?", new DbType[] { DbType.DateTime }, new object[] { DT }, out err);
            rdr = ASUTP.Database.DbTSQLInterface.Select(ref parent.m_connection, "SELECT * FROM PPBRvsPBRnew where date_time = @0", new DbType[] { DbType.DateTime }, new object[] { DT }, out err);
            if (rdr.Rows.Count > 0)
            {
                BTEC_PBR = (double?)rdr.Rows[0]["BTEC_PBR"];
                BTEC_Pmax = (double?)rdr.Rows[0]["BTEC_Pmax"];
                BTEC_Pmin = (double?)rdr.Rows[0]["BTEC_Pmin"];
                BTEC_TG1_PBR = (double?)rdr.Rows[0]["BTEC_TG1_PBR"];
                BTEC_TG1_Pmax = (double?)rdr.Rows[0]["BTEC_TG1_Pmax"];
                BTEC_TG1_Pmin = (double?)rdr.Rows[0]["BTEC_TG1_Pmin"];
                BTEC_TG2_PBR = (double?)rdr.Rows[0]["BTEC_TG2_PBR"];
                BTEC_TG2_Pmax = (double?)rdr.Rows[0]["BTEC_TG2_Pmax"];
                BTEC_TG2_Pmin = (double?)rdr.Rows[0]["BTEC_TG2_Pmin"];
                BTEC_TG35_PBR = (double?)rdr.Rows[0]["BTEC_TG35_PBR"];
                BTEC_TG35_Pmax = (double?)rdr.Rows[0]["BTEC_TG35_Pmax"];
                BTEC_TG35_Pmin = (double?)rdr.Rows[0]["BTEC_TG35_Pmin"];
                BTEC_TG4_PBR = (double?)rdr.Rows[0]["BTEC_TG4_PBR"];
                BTEC_TG4_Pmax = (double?)rdr.Rows[0]["BTEC_TG4_Pmax"];
                BTEC_TG4_Pmin = (double?)rdr.Rows[0]["BTEC_TG4_Pmin"];
                TEC2_PBR = (double?)rdr.Rows[0]["TEC2_PBR"];
                TEC2_Pmax = (double?)rdr.Rows[0]["TEC2_Pmax"];
                TEC2_Pmin = (double?)rdr.Rows[0]["TEC2_Pmin"];
                TEC3_PBR = (double?)rdr.Rows[0]["TEC3_PBR"];
                TEC3_Pmax = (double?)rdr.Rows[0]["TEC3_Pmax"];
                TEC3_Pmin = (double?)rdr.Rows[0]["TEC3_Pmin"];
                TEC3_TG1_PBR = (double?)rdr.Rows[0]["TEC3_TG1_PBR"];
                TEC3_TG1_Pmax = (double?)rdr.Rows[0]["TEC3_TG1_Pmax"];
                TEC3_TG1_Pmin = (double?)rdr.Rows[0]["TEC3_TG1_Pmin"];
                TEC3_TG1314_PBR = (double?)rdr.Rows[0]["TEC3_TG1314_PBR"];
                TEC3_TG1314_Pmax = (double?)rdr.Rows[0]["TEC3_TG1314_Pmax"];
                TEC3_TG1314_Pmin = (double?)rdr.Rows[0]["TEC3_TG1314_Pmin"];
                TEC3_TG5_PBR = (double?)rdr.Rows[0]["TEC3_TG5_PBR"];
                TEC3_TG5_Pmax = (double?)rdr.Rows[0]["TEC3_TG5_Pmax"];
                TEC3_TG5_Pmin = (double?)rdr.Rows[0]["TEC3_TG5_Pmin"];
                TEC3_TG712_PBR = (double?)rdr.Rows[0]["TEC3_TG712_PBR"];
                TEC3_TG712_Pmax = (double?)rdr.Rows[0]["TEC3_TG712_Pmax"];
                TEC3_TG712_Pmin = (double?)rdr.Rows[0]["TEC3_TG712_Pmin"];
                TEC4_PBR = (double?)rdr.Rows[0]["TEC4_PBR"];
                TEC4_Pmax = (double?)rdr.Rows[0]["TEC4_Pmax"];
                TEC4_Pmin = (double?)rdr.Rows[0]["TEC4_Pmin"];
                TEC4_TG3_PBR = (double?)rdr.Rows[0]["TEC4_TG3_PBR"];
                TEC4_TG3_Pmax = (double?)rdr.Rows[0]["TEC4_TG3_Pmax"];
                TEC4_TG3_Pmin = (double?)rdr.Rows[0]["TEC4_TG3_Pmin"];
                TEC4_TG48_PBR = (double?)rdr.Rows[0]["TEC4_TG48_PBR"];
                TEC4_TG48_Pmax = (double?)rdr.Rows[0]["TEC4_TG48_Pmax"];
                TEC4_TG48_Pmin = (double?)rdr.Rows[0]["TEC4_TG48_Pmin"];
                TEC5_PBR = (double?)rdr.Rows[0]["TEC5_PBR"];
                TEC5_Pmax = (double?)rdr.Rows[0]["TEC5_Pmax"];
                TEC5_Pmin = (double?)rdr.Rows[0]["TEC5_Pmin"];
                TEC5_TG12_PBR = (double?)rdr.Rows[0]["TEC5_TG12_PBR"];
                TEC5_TG12_Pmax = (double?)rdr.Rows[0]["TEC5_TG12_Pmax"];
                TEC5_TG12_Pmin = (double?)rdr.Rows[0]["TEC5_TG12_Pmin"];
                TEC5_TG36_PBR = (double?)rdr.Rows[0]["TEC5_TG36_PBR"];
                TEC5_TG36_Pmax = (double?)rdr.Rows[0]["TEC5_TG36_Pmax"];
                TEC5_TG36_Pmin = (double?)rdr.Rows[0]["TEC5_TG36_Pmin"];
            }

            return rdr.Rows.Count > 0 ? true : false;
        }

        public override string GenUpdateStatement(DateTime dtMskNow)
        {
            //апдейтить только те, у которых ПБР в будущем (не с текущей датой лучше сравнивать, а с номером в индексе ПБР
            /* При очередном UPDATE надо знать, какой график пришёл с ОДУ - ПБР1, ПБР4, ПБР7, ПБР10, ПБР13, ПБР16, ПБР19 или ПБР22.
             * Каждый последующий обновляет только актуальные часы, ретроспектива же остаётся без изменений, то есть ПБР4 не обновит первые часы ПБР1, а ПБР22 обновит только часы, начиная с 21:30.  */
            string sUpdate = "";
            int? iId;
            //byte PBRHour;

            //PBRHour = byte.Parse(PBR_number.Replace(HAdmin.PBR_PREFIX, ""));

            //if (PBRHour >= date_time.Hour || (PBRHour == 21 && date_time.Hour == 0))
            if (date_time > dtMskNow)       //Московское время
            {
                iId = parent.Insert48HalfHoursIfNeedAndGetId(date_time);
                //sUpdate = string.Format("UPDATE PPBRvsPBR_Test SET wr_date_time = '{0}', PBR_number = '{1}' WHERE id = {2}", wr_date_time.ToString("u").Replace("Z", ""), PBR_number, iId.ToString());
                sUpdate = "UPDATE PPBRvsPBRnew SET wr_date_time = '@wr_date_time', PBR_number = '@PBR_number', BTEC_PBR = @BTEC_PBR, BTEC_Pmax = @BTEC_Pmax, BTEC_Pmin = @BTEC_Pmin, BTEC_TG1_PBR = @BTEC_TG1_PBR, ";
                sUpdate += "BTEC_TG1_Pmax = @BTEC_TG1_Pmax, BTEC_TG1_Pmin = @BTEC_TG1_Pmin, BTEC_TG2_PBR = @BTEC_TG2_PBR, BTEC_TG2_Pmax = @BTEC_TG2_Pmax, BTEC_TG2_Pmin = @BTEC_TG2_Pmin, ";
                sUpdate += "BTEC_TG4_PBR = @BTEC_TG4_PBR, BTEC_TG4_Pmax = @BTEC_TG4_Pmax, BTEC_TG4_Pmin = @BTEC_TG4_Pmin, BTEC_TG35_PBR = @BTEC_TG35_PBR, BTEC_TG35_Pmax = @BTEC_TG35_Pmax, BTEC_TG35_Pmin = @BTEC_TG35_Pmin, ";
                sUpdate += "TEC2_PBR = @TEC2_PBR, TEC2_Pmax = @TEC2_Pmax, TEC2_Pmin = @TEC2_Pmin, ";
                sUpdate += "TEC3_PBR = @TEC3_PBR, TEC3_Pmax = @TEC3_Pmax, TEC3_Pmin = @TEC3_Pmin, TEC3_TG1_PBR = @TEC3_TG1_PBR, TEC3_TG1_Pmax = @TEC3_TG1_Pmax, TEC3_TG1_Pmin = @TEC3_TG1_Pmin, ";
                sUpdate += "TEC3_TG5_PBR = @TEC3_TG5_PBR, TEC3_TG5_Pmax = @TEC3_TG5_Pmax, TEC3_TG5_Pmin = @TEC3_TG5_Pmin, TEC3_TG712_PBR = @TEC3_TG712_PBR, TEC3_TG712_Pmax = @TEC3_TG712_Pmax, ";
                sUpdate += "TEC3_TG712_Pmin = @TEC3_TG712_Pmin, TEC3_TG1314_PBR = @TEC3_TG1314_PBR, TEC3_TG1314_Pmax = @TEC3_TG1314_Pmax, TEC3_TG1314_Pmin = @TEC3_TG1314_Pmin, ";
                sUpdate += "TEC4_PBR = @TEC4_PBR, TEC4_Pmax = @TEC4_Pmax, TEC4_Pmin = @TEC4_Pmin, TEC4_TG3_PBR = @TEC4_TG3_PBR, TEC4_TG3_Pmax = @TEC4_TG3_Pmax, TEC4_TG3_Pmin = @TEC4_TG3_Pmin, ";
                sUpdate += "TEC4_TG48_PBR = @TEC4_TG48_PBR, TEC4_TG48_Pmax = @TEC4_TG48_Pmax, TEC4_TG48_Pmin = @TEC4_TG48_Pmin, ";
                sUpdate += "TEC5_PBR = @TEC5_PBR, TEC5_Pmax = @TEC5_Pmax, TEC5_Pmin = @TEC5_Pmin, TEC5_TG12_PBR = @TEC5_TG12_PBR, TEC5_TG12_Pmax = @TEC5_TG12_Pmax, TEC5_TG12_Pmin = @TEC5_TG12_Pmin, ";
                sUpdate += "TEC5_TG36_PBR = @TEC5_TG36_PBR, TEC5_TG36_Pmax = @TEC5_TG36_Pmax, TEC5_TG36_Pmin = @TEC5_TG36_Pmin ";
                sUpdate += "WHERE id = @id";
                sUpdate = sUpdate.Replace("@wr_date_time", wr_date_time.ToString("u").Replace("Z", ""));
                sUpdate = sUpdate.Replace("@PBR_number", PBR_number);
                sUpdate = sUpdate.Replace("@BTEC_PBR", BTEC_PBR.ToString());
                sUpdate = sUpdate.Replace("@BTEC_Pmax", BTEC_Pmax.ToString());
                sUpdate = sUpdate.Replace("@BTEC_Pmin", BTEC_Pmin.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG1_PBR", BTEC_TG1_PBR.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG1_Pmax", BTEC_TG1_Pmax.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG1_Pmin", BTEC_TG1_Pmin.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG2_PBR", BTEC_TG2_PBR.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG2_Pmax", BTEC_TG2_Pmax.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG2_Pmin", BTEC_TG2_Pmin.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG4_PBR", BTEC_TG4_PBR.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG4_Pmax", BTEC_TG4_Pmax.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG4_Pmin", BTEC_TG4_Pmin.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG35_PBR", BTEC_TG35_PBR.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG35_Pmax", BTEC_TG35_Pmax.ToString());
                sUpdate = sUpdate.Replace("@BTEC_TG35_Pmin", BTEC_TG35_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC2_PBR", TEC2_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC2_Pmax", TEC2_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC2_Pmin", TEC2_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC3_PBR", TEC3_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC3_Pmax", TEC3_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC3_Pmin", TEC3_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1_PBR", TEC3_TG1_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1_Pmax", TEC3_TG1_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1_Pmin", TEC3_TG1_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG5_PBR", TEC3_TG5_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG5_Pmax", TEC3_TG5_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG5_Pmin", TEC3_TG5_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG712_PBR", TEC3_TG712_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG712_Pmax", TEC3_TG712_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG712_Pmin", TEC3_TG712_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1314_PBR", TEC3_TG1314_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1314_Pmax", TEC3_TG1314_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC3_TG1314_Pmin", TEC3_TG1314_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC4_PBR", TEC4_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC4_Pmax", TEC4_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC4_Pmin", TEC4_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG3_PBR", TEC4_TG3_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG3_Pmax", TEC4_TG3_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG3_Pmin", TEC4_TG3_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG48_PBR", TEC4_TG48_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG48_Pmax", TEC4_TG48_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC4_TG48_Pmin", TEC4_TG48_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC5_PBR", TEC5_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC5_Pmax", TEC5_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC5_Pmin", TEC5_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG12_PBR", TEC5_TG12_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG12_Pmax", TEC5_TG12_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG12_Pmin", TEC5_TG12_Pmin.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG36_PBR", TEC5_TG36_PBR.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG36_Pmax", TEC5_TG36_Pmax.ToString());
                sUpdate = sUpdate.Replace("@TEC5_TG36_Pmin", TEC5_TG36_Pmin.ToString());
                sUpdate = sUpdate.Replace("@id", iId.ToString());
            }

            //Можно экземпляр объекта OdbcCommand возвращать, и использовать здесь параметризованную команду апдейта. Но строку выводить на экран удобнее.
            return sUpdate;
        }

        public override bool SetValues(int id, MySQLtechsite.Params PAR, double val)
        {
            bool bRes = false;

            switch (id)
            {
                case 209:                                                                                           //Барабинская ТЭЦ, ТГ-1 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddBTECsumValues(PAR, val);
                    if (PAR == MySQLtechsite.Params.PBR) BTEC_TG1_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) BTEC_TG1_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) BTEC_TG1_Pmin = val;
                    break;
                case 210:                                                                                           //Барабинская ТЭЦ, ТГ-2 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddBTECsumValues(PAR, val);
                    if (PAR == MySQLtechsite.Params.PBR) BTEC_TG2_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) BTEC_TG2_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) BTEC_TG2_Pmin = val;
                    break;
                case 211:                                                                                           //Барабинская ТЭЦ, ТГ-4 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddBTECsumValues(PAR, val);
                    if (PAR == MySQLtechsite.Params.PBR) BTEC_TG4_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) BTEC_TG4_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) BTEC_TG4_Pmin = val;
                    break;
                case 28:                                                                                            //Барабинская ТЭЦ, ТГ-3,5 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddBTECsumValues(PAR, val);
                    if (PAR == MySQLtechsite.Params.PBR) BTEC_TG35_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) BTEC_TG35_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) BTEC_TG35_Pmin = val;
                    break;
                case 17:                                                                                            //Новосибирская ТЭЦ-2 [РГЕ ТЭЦ, КЭС, АЭС]
                    if (PAR == MySQLtechsite.Params.PBR) TEC2_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) TEC2_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) TEC2_Pmin = val;
                    break;
                case 65:                                                                                            //Новосибирская ТЭЦ-3, ТГ-1 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC3sumValues(PAR, val);
                    if (PAR == MySQLtechsite.Params.PBR) TEC3_TG1_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) TEC3_TG1_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) TEC3_TG1_Pmin = val;
                    break;
                case 201:                                                                                           //Новосибирская ТЭЦ-3, ТГ-5 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC3sumValues(PAR, val);
                    if (PAR == MySQLtechsite.Params.PBR) TEC3_TG5_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) TEC3_TG5_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) TEC3_TG5_Pmin = val;
                    break;
                case 13:                                                                                            //Новосибирская ТЭЦ-3, ТГ 7-12 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC3sumValues(PAR, val);
                    if (PAR == MySQLtechsite.Params.PBR) TEC3_TG712_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) TEC3_TG712_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) TEC3_TG712_Pmin = val;
                    break;
                case 14:                                                                                            //Новосибирская ТЭЦ-3, ТГ-13,14 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC3sumValues(PAR, val);
                    if (PAR == MySQLtechsite.Params.PBR) TEC3_TG1314_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) TEC3_TG1314_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) TEC3_TG1314_Pmin = val;
                    break;
                case 195:                                                                                           //Новосибирская ТЭЦ-4, ТГ-3 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC4sumValues(PAR, val);
                    if (PAR == MySQLtechsite.Params.PBR) TEC4_TG3_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) TEC4_TG3_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) TEC4_TG3_Pmin = val;
                    break;
                case 8:                                                                                             //Новосибирская ТЭЦ-4, ТГ 4-8 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC4sumValues(PAR, val);
                    if (PAR == MySQLtechsite.Params.PBR) TEC4_TG48_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) TEC4_TG48_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) TEC4_TG48_Pmin = val;
                    break;
                case 23:                                                                                            //Новосибирская ТЭЦ-5, ТГ-3,4 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC5sumValues(PAR, val);
                    AddTEC5TG36Values(PAR, val);
                    break;
                case 24:                                                                                            //Новосибирская ТЭЦ-5, ТГ-5,6 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC5sumValues(PAR, val);
                    AddTEC5TG36Values(PAR, val);
                    break;
                case 25:                                                                                            //Новосибирская ТЭЦ-5, ТГ-1,2 [РГЕ ТЭЦ, КЭС, АЭС]
                    AddTEC5sumValues(PAR, val);
                    if (PAR == MySQLtechsite.Params.PBR) TEC5_TG12_PBR = val;
                    if (PAR == MySQLtechsite.Params.Pmax) TEC5_TG12_Pmax = val;
                    if (PAR == MySQLtechsite.Params.Pmin) TEC5_TG12_Pmin = val;
                    break;
                #region Comment
                /*      Это прежняя, устаревшая в августе 2013 кодировка:
                 157 	-Барабинская ТЭЦ [Электростанция]  P:10
                 209 	--Барабинская ТЭЦ, ТГ-1 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 210	--Барабинская ТЭЦ, ТГ-2 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 211	--Барабинская ТЭЦ, ТГ-4 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 208	--Барабинская ТЭЦ, ТГ-3,5 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 155	-Новосибирская ТЭЦ-2 [Электростанция]  P:10
                 17 	--Новосибирская ТЭЦ-2 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 154	-Новосибирская ТЭЦ-3 [Электростанция]  P:10
                 104	--Новосибирская ТЭЦ-3, ТГ-1 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 201	--Новосибирская ТЭЦ-3, ТГ-5 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 200	--Новосибирская ТЭЦ-3, ТГ 7-12 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 14 	--Новосибирская ТЭЦ-3, ТГ-13,14 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 153	-Новосибирская ТЭЦ-4 [Электростанция]  P:10
                 195	--Новосибирская ТЭЦ-4, ТГ-3 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 194	--Новосибирская ТЭЦ-4, ТГ 4-8 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 156	-Новосибирская ТЭЦ-5 [Электростанция]  P:10
                 147	--Новосибирская ТЭЦ-5, ТГ-1,2 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 146	--Новосибирская ТЭЦ-5, ТГ-3,4 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                 148	--Новосибирская ТЭЦ-5, ТГ-5,6 [РГЕ ТЭЦ, КЭС, АЭС]  P:22
                */
                #endregion
            }

            return bRes;
        }

        /// <summary>
        /// Т.к. поле TEC3_PBR - это сумма по [ТГ-1] + [ТГ-5] + [ТГ 7-12] + [ТГ-13,14].
        /// Аналогично TEC3_Pmax и TEC3_Pmin.
        /// </summary>
        private void AddTEC3sumValues(MySQLtechsite.Params PAR, double dGenValue)
        {
            if (PAR == MySQLtechsite.Params.PBR) TEC3_PBR = TEC3_PBR.GetValueOrDefault(0) + dGenValue;
            if (PAR == MySQLtechsite.Params.Pmax) TEC3_Pmax = TEC3_Pmax.GetValueOrDefault(0) + dGenValue;
            if (PAR == MySQLtechsite.Params.Pmin) TEC3_Pmin = TEC3_Pmin.GetValueOrDefault(0) + dGenValue;
        }

        private void AddTEC4sumValues(MySQLtechsite.Params PAR, double dGenValue)
        {
            if (PAR == MySQLtechsite.Params.PBR) TEC4_PBR = TEC4_PBR.GetValueOrDefault(0) + dGenValue;
            if (PAR == MySQLtechsite.Params.Pmax) TEC4_Pmax = TEC4_Pmax.GetValueOrDefault(0) + dGenValue;
            if (PAR == MySQLtechsite.Params.Pmin) TEC4_Pmin = TEC4_Pmin.GetValueOrDefault(0) + dGenValue;
        }

        /// <summary>
        /// Т.к. поле TEC5_PBR - это сумма по [ТГ-1,2] + [ТГ-3,4] + [ТГ-5,6].
        /// Аналогично TEC5_Pmax и TEC5_Pmin.
        /// </summary>
        private void AddTEC5sumValues(MySQLtechsite.Params PAR, double dGenValue)
        {
            if (PAR == MySQLtechsite.Params.PBR) TEC5_PBR = TEC5_PBR.GetValueOrDefault(0) + dGenValue;
            if (PAR == MySQLtechsite.Params.Pmax) TEC5_Pmax = TEC5_Pmax.GetValueOrDefault(0) + dGenValue;
            if (PAR == MySQLtechsite.Params.Pmin) TEC5_Pmin = TEC5_Pmin.GetValueOrDefault(0) + dGenValue;
        }

        private void AddTEC5TG36Values(MySQLtechsite.Params PAR, double dGenValue)
        {
            if (PAR == MySQLtechsite.Params.PBR) TEC5_TG36_PBR = TEC5_TG36_PBR.GetValueOrDefault(0) + dGenValue;
            if (PAR == MySQLtechsite.Params.Pmax) TEC5_TG36_Pmax = TEC5_TG36_Pmax.GetValueOrDefault(0) + dGenValue;
            if (PAR == MySQLtechsite.Params.Pmin) TEC5_TG36_Pmin = TEC5_TG36_Pmin.GetValueOrDefault(0) + dGenValue;
        }

        private void AddBTECsumValues(MySQLtechsite.Params PAR, double dGenValue)
        {
            if (PAR == MySQLtechsite.Params.PBR) BTEC_PBR = BTEC_PBR.GetValueOrDefault(0) + dGenValue;
            if (PAR == MySQLtechsite.Params.Pmax) BTEC_Pmax = BTEC_Pmax.GetValueOrDefault(0) + dGenValue;
            if (PAR == MySQLtechsite.Params.Pmin) BTEC_Pmin = BTEC_Pmin.GetValueOrDefault(0) + dGenValue;
        }
    }

    /*class OneField
    {
        public DateTime date_time;
        public string sFieldName;
        public double dValue;
    }*/
}
