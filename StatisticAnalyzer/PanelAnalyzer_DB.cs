using ASUTP;
using ASUTP.Core;
using ASUTP.Database;
using ASUTP.Helper;
using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

namespace StatisticAnalyzer
{
    public partial class PanelAnalyzer_DB : PanelAnalyzer
    {
        public PanelAnalyzer_DB (List<StatisticCommon.TEC> tec, Color foreColor, Color backColor)
            : base(tec, foreColor, backColor)
        {
        }

        private HLoggingReadHandlerDb LoggingReadHandler
        {
            get
            {
                return m_loggingReadHandler as HLoggingReadHandlerDb;
            }
        }

        #region Наследуемые методы

        /// <summary>
        /// Метод для получения лога
        /// </summary>
        /// <returns>Возвращает лог сообщений</returns>
        protected override LogParse newLogParse() { return new LogParse_DB(); }

        /// <summary>
        /// Запуск таймера обновления данных
        /// </summary>
        public override void Start()
        {
            int err = -1, 
                idMainDB = -1;

            DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
            DbTSQLConfigDatabase.DbConfig().Register();

            if (DbTSQLConfigDatabase.DbConfig ().ListenerId > 0) {
                idMainDB = Int32.Parse (DbTSQLConfigDatabase.DbConfig().Select (@"SELECT [VALUE] FROM [setup] WHERE [KEY]='" + @"Main DataSource" + @"'", null, null, out err).Rows [0] [@"VALUE"].ToString ());
                DataTable tblConnSettMainDB = ConnectionSettingsSource.GetConnectionSettings (DbTSQLConfigDatabase.DbConfig ().ListenerId, idMainDB, -1, out err);

                if ((tblConnSettMainDB.Columns.Count > 0)
                    && (tblConnSettMainDB.Rows.Count > 0)) {
                // "-1" - значит будет назначени идентификатор из БД
                    LoggingReadHandler.SetConnectionSettings(new ConnectionSettings (tblConnSettMainDB.Rows [0], -1));
                    LoggingReadHandler.SetDelegateReport(delegateErrorReport, delegateWarningReport, delegateActionReport, delegateReportClear);
                    LoggingReadHandler.SetDelegateWait(delegateStartWait, delegateStopWait, delegate () { /* ничего не делаем */ });
                    LoggingReadHandler.StartDbInterfaces();
                    LoggingReadHandler.Start ();

                    //!!! только после 'старта' handler-а
                    base.Start ();
                } else
                    throw new Exception ($"PanelAnalyzer_DB::Start () - нет параметров соединения с БД значений ID={idMainDB}...");
            } else
                throw new Exception (@"PanelAnalyzer_DB::Start () - нет соединения с БД конфигурации...");

            DbTSQLConfigDatabase.DbConfig ().UnRegister ();
        }

        /// <summary>
        /// Метод для получения из БД списка активных вкладок для выбранного пользователя
        /// </summary>
        /// <param name="role">Идентификатор роли(группы) пользователя</param>
        /// <param name="user">Идентификатор пользователя для выборки</param>
        protected override IEnumerable<int> getTabActived(int role, int user)
        {
            List<int> listRes = new List<int> ();

            int iRes = -1;
            string tabs;
            string[] idTabs;

            delegateActionReport ("Получение активных вкладок пользователя");

            #region Фомирование и выполнение запроса для получения списка открытых вкладок у пользователя

            tabs = HUsers.GetAllowed(DbTSQLConfigDatabase.DbConfig().ListenerId
                , role
                , user
                , (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE);
            //tabs = HUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS);

            if (Equals(user, null) == false) {
                if (tabs.Length > 0) {
                    //создание массива с ID вкладок
                    idTabs = tabs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < idTabs.Length; i++)
                        if (IsPunctuationContains(idTabs[i]) == true) {
                        //разбор в случае составного идентификатора
                            string[] count;
                            count = idTabs[i].Split('=');
                            //добавление идентификатора в список
                            listRes.Add(Convert.ToInt32(count[0]));
                        } else
                            //фильтрация пустых идентификаторов
                            if (string.IsNullOrEmpty(idTabs[i].Trim()) == false)
                                if (!(IsNumberContains(idTabs[i]) < 0))
                                //добавление идентификатора в список
                                    listRes.Add(Convert.ToInt32(idTabs[i]));
                                else
                                    ;
                            else
                                ;
                } else
                // нет записей в БД с информацией о вкладках, автоматически загружаемых при запуске для пользователя
                    ;
            } else
            // идентификатор пользователя не указан
                ;

            #endregion

            delegateReportClear (true);

            return listRes;
        }

        /// <summary>
        /// Разорвть соединение с БД
        /// </summary>
        protected override void disconnect()
        {
        }

        /// <summary>
        /// Получение первой строки лог-сообщений
        /// </summary>
        /// <param name="r">Строка из таблицы</param>
        /// <returns>Вызвращает строку string</returns>
        protected override string getTabLoggingTextRow(DataRow r)
        {
            string strRes = string.Empty;

            if ((!(r["DATE_TIME"] is DBNull))
                && (r["DATE_TIME"] is DateTime))
                strRes = string.Join(s_chDelimeters[(int)INDEX_DELIMETER.PART].ToString()
                    , new string[] {
                            ((DateTime)r["DATE_TIME"]).ToString (@"HH:mm:ss.fff")
                            , r["TYPE"].ToString ()
                            , r["MESSAGE"].ToString()
                        });
            else
                ;

            return strRes;
        }

        #endregion

        /// <summary>
        /// Наличие пунктуации в строке
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>true-если есть</returns>
        static bool IsPunctuationContains(string input)
        {
            bool bRes = false;

            foreach (char c in input)
                if (Char.IsPunctuation(c))
                    bRes = true;

            return bRes;
        }

        /// <summary>
        /// Наличие цифр в строке
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>-1 - нет, 0 - все цифры, 1 - есть, но не все</returns>
        static int IsNumberContains(string input)
        {
            int iRes = -1;

            int iInput = -1;

            if (int.TryParse (input, out iInput) == true)
                iRes = 0;
            else
                foreach (char c in input)
                    if (Char.IsNumber (c) == true) {
                        iRes = 1;

                        break;
                    } else
                        ;

            return iRes;
        }

        #region Обработчики событий

        protected override void LogParse_ThreadExit (LogParse.PARAM_THREAD_PROC param)
        {
            base.LogParse_ThreadExit (param);
        }

        protected override ILoggingReadHandler newLoggingRead ()
        {
            return new HLoggingReadHandlerDb();
        }

        /// <summary>
        /// Обновление статистики сообщений из лога за выбранный период
        /// </summary>
        protected override void dgvUserView_SelectionChanged (object sender, EventArgs e)
        {
            base.dgvUserView_SelectionChanged (sender, e);

            //Очистить элементы управления с данными от пред. лог-файла
            if (IsHandleCreated == true)
                if (InvokeRequired == true) {
                    BeginInvoke (new Action (clearListDateView));
                    BeginInvoke (new Action<bool> (clearMessageView), true);
                } else {
                    clearListDateView ();
                    clearMessageView (true);
                }
            else
                Logging.Logg ().Error (@"FormMainAnalyzer::dgvUserStatistic_SelectionChanged () - ... BeginInvoke (TabLoggingClearDatetimeStart, TabLoggingClearText) - ...", Logging.INDEX_MESSAGE.D_001);

            startLogParse (IdCurrentUserView.ToString());

            DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
            DbTSQLConfigDatabase.DbConfig ().Register ();
            //Заполнение ListBox'а активными вкладками
            fillListBoxTabVisible (
                getTabActived ((int)RoleCurrentUserView, IdCurrentUserView).ToList ()
            );
            DbTSQLConfigDatabase.DbConfig ().UnRegister ();
        }

        #endregion
    }
}
