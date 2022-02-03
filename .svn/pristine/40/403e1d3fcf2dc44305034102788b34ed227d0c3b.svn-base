using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.Common;
//using System.Data.Odbc;
using MySql.Data.MySqlClient;

using StatisticCommon;
using ASUTP.Database;

namespace trans_mc_cmd
{
    /// <summary>
    /// Класс работы с базой techsite на сервере MySQL
    /// </summary>
    class MySQLtechsite
    {
        public enum CONN_SETT_TYPE { CONFIG, PPBR, COUNT_CONN_SETT_TYPE };

        private struct KeyTECComponent
        {
            public FormChangeMode.KeyDevice Key;

            public List<int> MCentreId;
        }

        AdminTS m_admin;

        List<KeyTECComponent> m_listKeyTECComponent;

        public int Initialized
        {
            get
            {
                int iRes = 0;

                //if (!(m_MySQLConnections == null))
                //for (CONN_SETT_TYPE i = CONN_SETT_TYPE.CONFIG; i < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                //{
                //if (DbTSQLInterface.IsConnected (m_MySQLConnections[(int)i]) == true)
                if (DbTSQLInterface.IsConnected(ref m_connection) == false)
                    iRes = 1;
                else
                    ;

                //    if (!(iRes == 0))
                //        break;
                //    else
                //        ;
                //}
                //else
                //    ;

                if (iRes == 0)
                    if (!(m_listKeyTECComponent.Count > 0))
                        iRes = -1;
                    else
                        ;
                else
                    ;

                return iRes;
            }
        }

        public bool m_bCalculatedHalfHourValues;

        //public MySqlConnection [] m_MySQLConnections;
        public DbConnection m_connection;
        public string m_strTableNamePPBR;
        private int m_idListener;
        //public static string m_strNameSectionINI = "Параметры соединения с БД (trans_mc_cmd.exe)";
        public enum Params : int { PBR = 0, Pmin = 1, Pmax = 2, COUNT_PARAMS };

        //public enum TEClist : int { BTEC = ?, TEC2 = 17, TEC3 = сумма104_201_200_14, TEC3_110 = суммма201_200, TEC3_110_2 = 104, TEC3_220 = 14, TEC4 = сумма194и195?, TEC5 = сумма147и146и148, TEC5_110 = 146, TEC5_220 = 147, TEC5_110_2 = 148 };

        /// <summary>
        /// Коллекция почасовых величин генерации. После опроса ModesCentre должны получить 24 почасовых элемента в коллекции. Каждый элемент соответствует одной записи в базе данных techsite.
        /// </summary>
        //SortedList<DateTime, OneRecord> HourlyValuesCollection;
        SortedList<DateTime, HTECComponentsRecord> HourlyValuesCollection;
        //SortedList<DateTime, OneField> HourlyFieldValues;

        /// <summary>
        /// Конструктор открывает коннект к базе. Закрывает деструктор.
        /// </summary>
        public MySQLtechsite(bool bCalculatedHalfHourValues)
        {
            int iRes = -1;

            m_bCalculatedHalfHourValues = bCalculatedHalfHourValues;

            TECComponent comp;
            int iConfigDB = -1;
            ConnectionSettings connSett = Program.ReadConnSettFromFileINI(out iConfigDB);
            connSett.id = ConnectionSettings.UN_ENUMERABLE_ID;

            Console.WriteLine("DB parametrs: IP=" + connSett.server + ", port=" + connSett.port + ", DBName=" + connSett.dbName + ", UID=" + connSett.userName + Environment.NewLine);

            //Соединение дл я БД конфигурации
            new DbTSQLConfigDatabase (connSett);
            DbTSQLConfigDatabase.DbConfig().Register();
            m_connection = DbSources.Sources ().GetConnection (m_idListener = DbTSQLConfigDatabase.DbConfig ().ListenerId, out iRes);

            //TYPE_DATABASE_CFG typeConfigDB = TYPE_DATABASE_CFG.UNKNOWN;
            //for (TYPE_DATABASE_CFG t = TYPE_DATABASE_CFG.CFG_190; t < TYPE_DATABASE_CFG.UNKNOWN; t ++) {
            //    if (t.ToString().Contains(iConfigDB.ToString()) == true)
            //    {
            //        typeConfigDB = t;
            //        break;
            //    }
            //    else
            //        ;
            //}

            if (iRes == 0)
            {
                m_admin = new AdminTS(new bool [] {false, true}, TECComponentBase.TYPE.ELECTRO);

                ASUTP.Core.HMark markQueries = new ASUTP.Core.HMark (new int[] { (int)StatisticCommon.CONN_SETT_TYPE.ADMIN, (int)StatisticCommon.CONN_SETT_TYPE.PBR });
                //markQueries.Marked ((int)StatisticCommon.CONN_SETT_TYPE.ADMIN);
                //markQueries.Marked((int)StatisticCommon.CONN_SETT_TYPE.PBR);

                m_admin.InitTEC(FormChangeMode.MODE_TECCOMPONENT.GTP, /*typeConfigDB, */markQueries, true, new int [] { 0, (int)TECComponent.ID.LK });
                m_admin.GetListKeyTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP, true).ForEach(key =>
                    m_listKeyTECComponent.Add(new KeyTECComponent () { Key = key, MCentreId = new List<int>((m_admin.FindTECComponent(key) as TECComponent).m_listMCentreId) }));

                //Пересоединение для таблиц ПБР
                if ((DbTSQLInterface.IsConnected(ref m_connection) == true)
                    && (m_listKeyTECComponent.Count > 0))
                {
                    DbSources.Sources ().UnRegister (m_idListener);

                    comp = m_admin.FindTECComponent (m_listKeyTECComponent [0].Key) as TECComponent;
                    m_idListener = DbSources.Sources().Register(comp.tec.connSetts[(int)StatisticCommon.CONN_SETT_TYPE.PBR], false, @"PBR");
                    m_connection = DbSources.Sources().GetConnection (m_idListener, out iRes);
                    m_strTableNamePPBR = comp.tec.m_strNameTableUsedPPBRvsPBR/*[(int)AdminTS.TYPE_FIELDS.STATIC]*/;
                }
                else
                {
                }
            }
            else
            {
                //itssAUX.PrintErrorMessage("Ошибка! MySQLtechsite::MySQLtechsite () - чтение файла с шифрованными параметрами соединения (" + m_strFileNameConnSett + ")...");
                itssAUX.PrintErrorMessage("Проверте параметры соединения (" + Program.m_fileINI.m_NameFileINI + "). Затем запустите программу с аргументом /setmysqlpassword..." + Environment.NewLine);
            }

            DbTSQLConfigDatabase.DbConfig ().UnRegister ();
        }

        public string TestRead()
        {
            int err = -1;
            DataTable dataTest;
            //dataTest = DbTSQLInterface.Select(m_MySQLConnections[(int)CONN_SETT_TYPE.PPBR], "SELECT page FROM settings", null, null, out err);
            dataTest = DbTSQLInterface.Select(ref m_connection, "SELECT page FROM settings", null, null, out err);

            if (err == 0)
                return (dataTest.Rows[0][0].ToString());
            else
                return string.Empty;
        }

        /// <summary>
        /// Пишем в коллекцию с экземплярами класса OneRecord. А потом за раз коллекцию превратим в апдейты таблицы базы и выполним.
        /// Привязка к id станции! Если в API id поменяют - будет неверно работать!
        /// </summary>
        public void WritePlanValue(int iStationId, DateTime DT, string sPBRnum, Params PAR, double dGenValue)
        {
            KeyTECComponent key = m_listKeyTECComponent.Find (k => !(k.MCentreId.IndexOf (iStationId) < 0));

            if (Equals(key, null) == false)
            {
                //OneRecord HVCrecord = null;
                HTECComponentsRecord HVCrecord = null;

                if (HourlyValuesCollection == null)
                    //HourlyValuesCollection = new SortedList<DateTime, OneRecord>(50);
                    HourlyValuesCollection = new SortedList<DateTime, HTECComponentsRecord>(50);
                else
                    ;
                //А может здесь из базы читать предыдущие значения в коллекцию? Нет. Данные из базы здесь не нужны.

                /*OneField OFthis = HourlyFieldValues.First(item => item.Key == DT && item.Value.sFieldName == PAR.ToString()).Value;     //Не верно, но тема такая
                if (OFthis == null) OFthis = new OneField();*/

                if (HourlyValuesCollection.ContainsKey(DT))
                    HVCrecord = HourlyValuesCollection.First(item => item.Key == DT).Value;
                else
                    ;

                if (HVCrecord == null)
                {
                    //HVCrecord = new OneRecord();
                    HVCrecord = new HTECComponentsRecord(key.MCentreId);
                    HVCrecord.date_time = DT;
                    HVCrecord.parent = this;
                    HourlyValuesCollection.Add(DT, HVCrecord);      //После добавления можно продолжать модифицировать экземпляр класса - в коллекции та же самая ссылка хранится.
                }
                else
                    ;

                HVCrecord.wr_date_time = DateTime.Now;
                HVCrecord.PBR_number = sPBRnum;

                HVCrecord.SetValues(iStationId, PAR, dGenValue);
            }
            else
                ; //Такого ГТП (с ном. 'iStationId' не найдено)
        }

        private void AddCalculatedOwnerValues()
        {
            foreach (HTECComponentsRecord rec in HourlyValuesCollection.Values)
                rec.GenerateOwnerValues();
        }

        /// <summary>
        /// Добавляет в коллекцию HourlyValuesCollection получасовки, расчитанные как среднее арифметическое между показателями соседних часов.
        /// Получасовки только по будущим показателям считаем. По прошлым запрашиваемые через API значения могут расходиться с сохранёнными ранее в базе!
        /// Кроме того, не всегда получасовки окажутся средним арифметическим: после вычисления получасовки до начала следующего часа данные следующего часа могут измениться. В 02:40, например. Данные на 02:30 уже не изменишь, а на 03:00 будут другие.
        /// </summary>
        private void AddCalculatedHalfHourValues(DateTime dtMskNow)
        {
            //DateTime dtMskNow = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            //List<OneRecord> li = HourlyValuesCollection.Values.Where(item => item.date_time > dtMskNow.AddMinutes(30)).ToList();
            List<HTECComponentsRecord> li = HourlyValuesCollection.Values.Where(item => item.date_time > dtMskNow.AddMinutes(30)).ToList();

            //прочесть: http://msmvps.com/blogs/deborahk/archive/2010/10/30/finding-in-a-child-list.aspx

            //foreach (OneRecord rec in HourlyValuesCollection.Values.Where(item => item.date_time > DateTime.Now.AddMinutes(30)).Select(item => item))
            //foreach (OneRecord rec in li.OrderBy(item => item.date_time))     //сортировка по дате
            foreach (HTECComponentsRecord rec in li.OrderBy(item => item.date_time))
                GenerateHalfHourValues(rec, dtMskNow);

        }

        /// <summary>
        /// Создаёт "получасовку" по среднему арифметическому между часовыми показателями.
        /// </summary>
        private void GenerateHalfHourValues(HTECComponentsRecord HVCrec, DateTime dtMskNow)
        {
            List<int> listIds = HVCrec.m_srtlist_ppbr.Keys.ToList();

            //OneRecord HVClast_rec = HourlyValuesCollection.Last().Value;
            HTECComponentsRecord HVCprev_rec;
            HTECComponentsRecord HVChalf_hour = new HTECComponentsRecord(listIds);
            //DateTime dtMskNow = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));

            if (HVCrec.date_time.AddHours(-1) <= dtMskNow)
            {
                //Показания в прошлом читаем из базы, из API игнорируем (могут отличаться)
                HVCprev_rec = new HTECComponentsRecord(listIds);
                HVCprev_rec.parent = this;
                HVCprev_rec.ReadFromDatabase(HVCrec.date_time.AddHours(-1));
            }
            else
            {
                HVCprev_rec = HourlyValuesCollection.First(item => item.Key == HVCrec.date_time.AddHours(-1)).Value;                            //Вынимаем из коллекции показания предыдущего (по отношению к выбранному) часа
            }

            if (HVCrec.date_time > new DateTime() && HVCprev_rec.date_time > new DateTime())        //Если время в экземплярах классов проставлено - считаем, что данные есть.
            {
                HVChalf_hour.date_time = HVCrec.date_time.AddMinutes(-30);
                HVChalf_hour.parent = this;
                HVChalf_hour.PBR_number = HVCrec.PBR_number;
                HVChalf_hour.wr_date_time = DateTime.Now;

                int i = -1;
                Params j;
                for (i = 0; i < listIds.Count; i++)
                {
                    for (j = Params.PBR; j < Params.COUNT_PARAMS; j++)
                        //Интерполируем средними арифметическими значениями от соседних часов
                        HVChalf_hour.m_srtlist_ppbr[listIds[i]][(int)j] = (HVCrec.m_srtlist_ppbr[listIds[i]][(int)j].GetValueOrDefault(0) + HVCprev_rec.m_srtlist_ppbr[listIds[i]][(int)j].GetValueOrDefault(0)) / 2;
                }

                HourlyValuesCollection.Add(HVChalf_hour.date_time, HVChalf_hour);
            }
            else
                ;
        }

        private int? GetIdNextRec(DateTime DT, out int er)
        {
            int? iRes = null;
            er = -1;
            string errMsg = string.Empty;

            DataTable data;
            //data = DbTSQLInterface.Select(m_MySQLConnections[(int)CONN_SETT_TYPE.PPBR], "SELECT id FROM PPBRvsPBRnew where date_time = ?", new DbType[] { DbType.DateTime }, new object[] { DT }, out er);
            data = DbTSQLInterface.Select(ref m_connection, "SELECT id FROM " + m_strTableNamePPBR + " where date_time = @0", new DbType[] { DbType.DateTime }, new object[] { DT }, out er);
            //data = DbTSQLInterface.Select(m_connection, "SELECT id FROM PPBRvsPBRnew where date_time = ?", new DbType[] { DbType.DateTime }, new object[] { DT }, out er);
            if (!(er == 0))
            {
                errMsg = "Не получен идентификатор для новой записи в '" + m_strTableNamePPBR + "'";
                itssAUX.PrintErrorMessage(errMsg);
                throw new Exception(errMsg);  //Чтобы остановить дальнейшее выполнение
            }
            else
            {
                if (data.Rows.Count == 1)
                    iRes = (int?)data.Rows[0][0];
                else
                {
                    if (data.Rows.Count > 1)
                    {
                        errMsg = "Не получен идентификатор для новой записи в '" + m_strTableNamePPBR + "'";
                        itssAUX.PrintErrorMessage(errMsg);
                        throw new Exception(errMsg);  //Чтобы остановить дальнейшее выполнение
                    }
                    else
                        ; //Нет строк вообще - нормально
                }
            }

            return iRes;
        }

        /// <summary>
        /// Несмотря на название, сейчас при необходимости вставляет только одну запись, соответствующую указанному часу или получасу.
        /// </summary>
        public int? Insert48HalfHoursIfNeedAndGetId(DateTime DT)
        {
            int err = 0;
            int? iId = GetIdNextRec(DT, out err);

            if (!iId.HasValue && (err == 0))
            {//Запись (ID) не была найдена - INSERT
                //NOW() на этом сервере не совпадает с Новосибирским временем
                //OdbcCommand cmdi = new OdbcCommand("INSERT INTO PPBRvsPBR_Test (date_time, wr_date_time, Is_Comdisp) VALUES('" + DateToSQL(DT) + "', '" + DateToSQL(DateTime.Now) + "', 0)", mysqlConn);
                //DbTSQLInterface.ExecNonQuery(m_MySQLConnections[(int)CONN_SETT_TYPE.PPBR], "INSERT INTO PPBRvsPBRnew (date_time, wr_date_time, Is_Comdisp) VALUES( ?, ?, 0)", new DbType[] { DbType.DateTime, DbType.DateTime }, new object[] { DT, DateTime.Now }, out err);
                DbTSQLInterface.ExecNonQuery(ref m_connection, "INSERT INTO " + m_strTableNamePPBR + " (date_time, wr_date_time, Is_Comdisp) VALUES( @0, @1, 0)", new DbType[] { DbType.DateTime, DbType.DateTime }, new object[] { DT, DateTime.Now }, out err);
                Console.WriteLine("INSERT INTO " + m_strTableNamePPBR + " (date_time, wr_date_time, Is_Comdisp) VALUES( @0, @1, 0)" + "; @0 = " + DT.ToString() + ", @1 = " + DateTime.Now.ToString() + "; Рез-т = " + err);
                if (!(err == 0))
                    itssAUX.PrintErrorMessage("Ошибка записи в базу MySQL на INSERT!");
                else
                    ;

                //Возвратим ID новой записи
                iId = GetIdNextRec(DT, out err);
            }
            else
                ;

            return iId;
        }

        /// <summary>
        /// Запись данных из коллекции HourlyValuesCollection в базу. Очистка коллекции. Запускается один раз в конце сеанса связи с API Modes-Centre.
        /// </summary>
        public void FlushDataToDatabase()
        {
            int err = -1;
            string sUpdate;

            DateTime dtMskNow = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));

            Console.WriteLine("Сохраняем данные в БД ППБР..." + Environment.NewLine);

            if (!(HourlyValuesCollection == null))
            {
                AddCalculatedOwnerValues();

                if (m_bCalculatedHalfHourValues == true)
                    AddCalculatedHalfHourValues(dtMskNow);
                else
                    ;

                foreach (HTECComponentsRecord rec in HourlyValuesCollection.Values)
                {
                    sUpdate = rec.GenUpdateStatement(dtMskNow);
                    if (!(sUpdate == ""))
                    {
                        Console.WriteLine(sUpdate + Environment.NewLine);

                        //Запуск апдейта одной часовой записи
                        //DbTSQLInterface.ExecNonQuery(m_MySQLConnections[(int)CONN_SETT_TYPE.PPBR], sUpdate, null, null, out err);
                        DbTSQLInterface.ExecNonQuery(ref m_connection, sUpdate, null, null, out err);

                        if (!(err == 0))
                            itssAUX.PrintErrorMessage("Ошибка! MySQLtechsite::FlushDataToDatabase () - Updated...");
                        else
                            ;
                    }
                    else
                        itssAUX.PrintErrorMessage("Для дата/время: " + rec.date_time.ToString() + " запрос пуст..." + Environment.NewLine);
                }

                HourlyValuesCollection.Clear();

                m_connection.Close();
                m_connection = null;
            }
            else
                ; //HourlyValuesCollection пуста
        }

        private string DateToSQL(DateTime DT)
        {
            return DT.ToString("u").Replace("Z", "");
        }

        ~MySQLtechsite()
        {
            //if(mysqlConn.State != ConnectionState.Closed) 
            //    mysqlConn.Close();
            //else
            //    ;
        }

        /// <summary>
        /// Возвратить компонент ТЭЦ с указанным в аргументе ИГО
        /// </summary>
        /// <param name="id_mc">Идентификатор ГО</param>
        /// <returns>Компонент ТЭЦ</returns>
        public TECComponent GetTECComponentOfIdMC (int id_mc)
        {
            return m_admin.GetTECComponentOfIdMC(id_mc);
        }
    }

    static class itssAUX
    {
        /// <summary>
        /// Выводит сообщение об ошибке на консоль красным шрифтом и (или) в журнал Windows.
        /// </summary>
        public static void PrintErrorMessage(string sErrMess, bool bConsoleOutput = true, bool bWriteToWinEventLog = true)
        {
            if (bConsoleOutput)
            {
                ConsoleColor cc = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(sErrMess);
                Console.ForegroundColor = cc;
            }
            else
                ;

            if ((Program.WriteToWinEventLog == true) && (bWriteToWinEventLog == true) && (Environment.OSVersion.Version.Major < 6))     //На Windows Vista и выше в журнал таким способом записать прав не хватит
            {
                //Для Win7 надо палочкой махнуть, но не кашерно: Try giving the following registry key Read permission for NETWORK SERVICE: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog\Security
                string sAppName = string.Empty;
                sAppName = ASUTP.Helper.ProgramBase.AppName + ".exe";
                //sAppName = "trans_mc_cmd.exe";
                System.Diagnostics.EventLog.WriteEntry(sAppName, sErrMess, System.Diagnostics.EventLogEntryType.Error);
            }
            else
                ;
        }
    }
}
