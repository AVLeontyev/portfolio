using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StatisticCommon;
using System.Collections.Generic;
using Statistic;
using ASUTP.Core;
using System.Threading.Tasks;
using ASUTP;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using ASUTP.Forms;

namespace UnitTest {
    [TestClass]
    public class StatisticCommonAdminTest
    {
        [Flags]
        private enum FormRequired {
            No
            , GraphicsSettings
            , Parameters
            ,
        }

        private class Counter
        {
            public Counter(int cnt, int cnt_max, IComparable comparator)
            {
                
            }
        }

        private static PanelAdminKomDisp panel;

        private static FormRequired _formRequired = FormRequired.GraphicsSettings;

        #region Дополнительные атрибуты тестирования

        //При написании тестов можно использовать следующие дополнительные атрибуты:

        //ClassInitialize используется для выполнения кода до запуска первого теста в классе
        [ClassInitialize ()]
        public static void StatisticCommonAdminTestInitialize (TestContext testContext)
        {
            if (!(testContext.TestName.IndexOf ("Export_PBR") < 0))
                _formRequired |= FormRequired.Parameters;
            else
                ;

            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            FormMainBase.s_iMainSourceData = 671;

            Logging.SetMode (Logging.LOG_MODE.FILE_EXE);

            Logging.Logg().PostStart(ASUTP.Helper.ProgramBase.MessageWellcome);

            int errCode = -1;
            string errMess = string.Empty;
            ASUTP.Database.FIleConnSett fileCS;
            List<ASUTP.Database.ConnectionSettings> listConnSett;

            listConnSett = new List<ASUTP.Database.ConnectionSettings> ();
            fileCS = new ASUTP.Database.FIleConnSett ("connsett.ini", ASUTP.Database.FIleConnSett.MODE.FILE);
            fileCS.ReadSettingsFile (-1, out listConnSett, out errCode, out errMess);
            new DbTSQLConfigDatabase (
                //new ASUTP.Database.ConnectionSettings("CONFIG_DB", "10.100.104.18", "", 1433, "techsite_cfg-2.X.X", "Stat_user", "5tat_u%ser")
                listConnSett [0]
                );
            DbTSQLConfigDatabase.DbConfig ().Register ();

            using (new HStatisticUsers (DbTSQLConfigDatabase.DbConfig ().ListenerId, ASUTP.Helper.HUsers.MODE_REGISTRATION.USER_DOMAINNAME)) {
                ;
            }

            foreach (FormRequired req in Enum.GetValues((typeof(FormRequired))))
                if ((_formRequired & req) == req)
                    switch (req) {
                        case FormRequired.No:
                            continue;
                        case FormRequired.GraphicsSettings:
                            FormMain.formGraphicsSettings = new FormGraphicsSettings (delegate (int arg) {
                                }
                                , delegate () {
                                }
                                , false
                            );
                            break;
                        case FormRequired.Parameters:
                            FormMain.formParameters = new FormParameters_DB ();
                            break;
                        default:
                            Assert.Fail($"{req} не обрабатывается...");
                            break;
                    }
                else
                    ;

            panel = new PanelAdminKomDisp (new HMark (new int [] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR }));
            panel.SetDelegateReport (delegate (string mes) {
            // error
                 System.Diagnostics.Debug.WriteLine ($"Error: {mes}");
            }, delegate (string mes) {
            // warning
                System.Diagnostics.Debug.WriteLine ($"Warning: {mes}");
            }, delegate (string mes) {
            // action
                System.Diagnostics.Debug.WriteLine ($"Action: {mes}");
            }, delegate (bool bClear) {
                // clear
            });

            DbTSQLConfigDatabase.DbConfig ().UnRegister ();

            Assert.IsNotNull (panel);

            panel.Start ();
            panel.InitializeComboBoxTecComponent (FormChangeMode.MODE_TECCOMPONENT.GTP, true, true);
            panel.Activate (true);
            Assert.IsTrue (panel.Actived);

            //Task.Factory.StartNew (() => {
            //    System.Threading.Thread.Sleep (42000);
            //});
        }

        //ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        [ClassCleanup ()]
        public static void StatisticCommonAdminTestCleanup ()
        {
            try {
                panel.Activate (false);
                panel.Stop ();

                Logging.Logg().PostStop(ASUTP.Helper.ProgramBase.MessageExit);
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            Assert.IsFalse (panel.Actived);
        }

        ////TestInitialize используется для выполнения кода перед запуском каждого теста
        //[TestInitialize ()]
        //public void MyTestInitialize ()
        //{
        //}

        ////TestCleanup используется для выполнения кода после завершения каждого теста
        //[TestCleanup ()]
        //public void MyTestCleanup ()
        //{
        //}

        #endregion

        [TestMethod]
        public void Test_Export_PBRValues ()
        {
            panel.ModeGetRDGValues |= AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST;
            panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.DISPLAY;
            Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
            Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);
            Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
            // проверка наличия необходимых "статических" форм
            Assert.IsNotNull (FormMain.formGraphicsSettings);
            Assert.IsNotNull (FormMain.formParameters);

            string mesDebug = string.Empty;

            FormChangeMode.KeyDevice prevKey = FormChangeMode.KeyDevice.Empty
                , nextKey = FormChangeMode.KeyDevice.Empty;
            Action onEventUnitTestSetDataGridViewAdminCompleted;
            AdminTS_KomDisp.DelegateUnitTestExportPBRValuesRequest delegateExportPBRValuesRequest;
            Task taskPerformButtonExportPBRValuesClick;
            TaskStatus taskStatusPerformButtonExportPBRValuesClick;
            CancellationTokenSource cancelTokenSource;

            onEventUnitTestSetDataGridViewAdminCompleted = null;

            taskPerformButtonExportPBRValuesClick =
                null;

            cancelTokenSource = new CancellationTokenSource ();
            // вызывается при ретрансляции панелью события имитации отправления запроса на обновление значений
            delegateExportPBRValuesRequest = delegate (FormChangeMode.KeyDevice next_key, DateTime date, FormChangeMode.KeyDevice current_key, IEnumerable<FormChangeMode.KeyDevice> listTECComponentKey) {
                Assert.IsNotNull (listTECComponentKey);

                mesDebug = string.Format ("Handler On 'EventUnitTestExportPBRValuesRequest': NextKey={0}, Date={1}, CurrentKey={2}, ListKey=<Count={3}, List={4}>..."
                    , next_key
                    , date.ToShortDateString()
                    , current_key
                    , listTECComponentKey.Count ()
                    , string.Join(",", listTECComponentKey));

                Logging.Logg ().Debug (mesDebug, Logging.INDEX_MESSAGE.NOT_SET);
                System.Diagnostics.Debug.WriteLine (mesDebug);

                prevKey = nextKey;
                nextKey = next_key;

                //TODO: проверка значений аргументов на истинность
                if (nextKey.Id > 0) {
                    Assert.AreNotEqual (DateTime.MinValue, date);
                    Assert.IsTrue (listTECComponentKey.Count () > 0);
                    Assert.AreEqual (nextKey, listTECComponentKey.ToArray () [0]);
                } else
                // ожидать (в 'onEventUnitTestSetDataGridViewAdminCompleted') штатное завершение
                    ;
            };
            // вызывается при завершении заполнения 'DatagridView' значениями
            onEventUnitTestSetDataGridViewAdminCompleted = delegate () {
                mesDebug = string.Format("Handler On 'EventUnitTestSetDataGridViewAdminCompleted': PrevIndex={0}, NextIndex={1}..."
                    , prevKey, nextKey);

                if (prevKey.Equals (nextKey) == true) {
                    // старт задачи сохранения значений
                    taskPerformButtonExportPBRValuesClick = Task.Factory.StartNew (delegate () {
                        panel.PerformButtonExportPBRValuesClick (delegateExportPBRValuesRequest);
                    });
                } else if (nextKey.Id == 0) {
                    Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
                    Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
                    // штатное завершение
                    nextKey.Id = -1;
                } else
                    ;

                Logging.Logg ().Debug (mesDebug, Logging.INDEX_MESSAGE.NOT_SET);
                System.Diagnostics.Debug.WriteLine (mesDebug);
            };

            try {
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);

                panel.EventUnitTestSetDataGridViewAdminCompleted += onEventUnitTestSetDataGridViewAdminCompleted;

                // исходные состояния задач
                taskStatusPerformButtonExportPBRValuesClick =
                    Equals (taskPerformButtonExportPBRValuesClick, null) == false ? taskPerformButtonExportPBRValuesClick.Status : TaskStatus.WaitingForActivation;

                int cnt = 0
                    , cnt_max = 26;
                while ((cnt++ < cnt_max)
                    && (!(nextKey.Id < 0))) {
                    // ожидать
                    Thread.Sleep (1000);
                    // сообщение для индикации ожидания
                    System.Diagnostics.Debug.WriteLine ($"Ожидание: счетчик <{cnt}> из <{cnt_max}>...");
                }

                // состояния задач по завершению цикла
                taskStatusPerformButtonExportPBRValuesClick =
                    Equals (taskPerformButtonExportPBRValuesClick, null) == false ? taskPerformButtonExportPBRValuesClick.Status : taskStatusPerformButtonExportPBRValuesClick;
                System.Diagnostics.Debug.WriteLine (string.Format ("Окончание ожидания <{0}>, задача-Click is <{1}>, nextIndex={2}..."
                    , cnt
                    , Equals (taskPerformButtonExportPBRValuesClick, null) == false ? taskPerformButtonExportPBRValuesClick.Status.ToString () : "не создана"                    
                    , nextKey));

                Assert.IsFalse (cnt > cnt_max);
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine (e.Message);

                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void Test_SetAdminValues ()
        {
            #region Проверка переключения режимов работы
            try {
                panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.DISPLAY | AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);

                panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.EXPORT;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);

                panel.ModeGetRDGValues |= AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);

                //??? на практике исключение этого флага не потребуется
                //panel.ModeGetRDGValues &= ~AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST;
                //Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
                //Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);

                panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.EXPORT;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);

                panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.DISPLAY;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);

                panel.ModeGetRDGValues |= AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST;
                panel.ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.DISPLAY;
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);
                Assert.IsFalse ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.EXPORT) == AdminTS.MODE_GET_RDG_VALUES.EXPORT);
            } catch (InvalidOperationException ioe) {
                Assert.IsTrue (string.Equals (ioe.Message, "PanelAdmin.ModeGetRDGValues::set - взаимоисключающие значения..."));
            }
            #endregion

            // проверка наличия необходимых "статических" форм
            Assert.IsNotNull (FormMain.formGraphicsSettings);

            string mesDebug = string.Empty;

            FormChangeMode.KeyDevice prevKey = FormChangeMode.KeyDevice.Empty
                , nextKey = FormChangeMode.KeyDevice.Empty;
            Action onEventUnitTestSetDataGridViewAdminCompleted;
            PanelAdmin.DelegateUnitTestNextIndexSetValuesRequest delegateNextIndexSetValuesRequest;
            Task taskPerformButtonSetClick
                , taskPerformComboBoxTECComponentSelectedIndex;
            TaskStatus taskStatusPerformButtonSetClick
                , taskStatusPerformComboBoxTECComponentSelectedIndex;
            CancellationTokenSource cancelTokenSource;

            onEventUnitTestSetDataGridViewAdminCompleted = null;

            taskPerformButtonSetClick =
            taskPerformComboBoxTECComponentSelectedIndex =
                null;

            cancelTokenSource = new CancellationTokenSource ();
            // вызывается при ретрансляции панелью события имитации отправления запроса на обновление значений
            delegateNextIndexSetValuesRequest = delegate (FormChangeMode.KeyDevice next_key, TECComponent comp, DateTime date, CONN_SETT_TYPE type, IEnumerable<int> list_id_rec, string [] queries) {
                Assert.IsNotNull(list_id_rec);
                Assert.IsFalse(list_id_rec.ToArray().Length < 24);
                //TODO: проверка значений массива на истинность (сравнить с идентификаторами из таблицы БД)

                nextKey = next_key;

                mesDebug = $"ТЭЦ={comp.tec.name_shr}; комп.={comp.name_shr};{Environment.NewLine}([{ASUTP.Database.DbTSQLInterface.QUERY_TYPE.INSERT.ToString()}]: [{queries[(int)ASUTP.Database.DbTSQLInterface.QUERY_TYPE.INSERT]}])"
                    + $"{Environment.NewLine}([{ASUTP.Database.DbTSQLInterface.QUERY_TYPE.UPDATE.ToString()}]: [{queries[(int)ASUTP.Database.DbTSQLInterface.QUERY_TYPE.UPDATE]}]"
                    + $"{Environment.NewLine}(идентификаторы: [{string.Join(";", list_id_rec.Select(id => id.ToString()).ToArray())}]";

                Logging.Logg().Debug(mesDebug, Logging.INDEX_MESSAGE.NOT_SET);
                System.Diagnostics.Debug.WriteLine(mesDebug);
            };
            // вызывается при завершении заполнения 'DatagridView' значениями
            onEventUnitTestSetDataGridViewAdminCompleted = delegate () {
                mesDebug = "Handler On 'EventUnitTestSetDataGridViewAdminCompleted'...";

                Logging.Logg().Debug(mesDebug, Logging.INDEX_MESSAGE.NOT_SET);
                System.Diagnostics.Debug.WriteLine (mesDebug);

                if (prevKey.Equals(nextKey) == true)
                // старт задачи сохранения значений
                    taskPerformButtonSetClick = Task.Factory.StartNew (delegate () {
                        panel.PerformButtonSetClick(delegateNextIndexSetValuesRequest);
                    });
                else
                    if (!(nextKey.Id < 0))
                        taskPerformComboBoxTECComponentSelectedIndex = Task.Factory.StartNew(delegate () {
                            // установить новый индекс (назначить новый компонент-объект)
                            panel.PerformComboBoxTECComponentSelectedKey(prevKey = nextKey);
                        });
                    else
                        ;
            };

            try {
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY);
                Assert.IsTrue ((panel.ModeGetRDGValues & AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST) == AdminTS.MODE_GET_RDG_VALUES.UNIT_TEST);

                panel.EventUnitTestSetDataGridViewAdminCompleted += onEventUnitTestSetDataGridViewAdminCompleted;

                //task.ContinueWith (t => {
                //}
                //, cancelTokenSource.Token);

                int cnt = 0
                    , cnt_max = 26;
                // исходные состояния задач
                taskStatusPerformButtonSetClick =
                    Equals (taskPerformButtonSetClick, null) == false ? taskPerformButtonSetClick.Status : TaskStatus.WaitingForActivation;
                taskStatusPerformComboBoxTECComponentSelectedIndex =
                    Equals(taskPerformComboBoxTECComponentSelectedIndex, null) == false ? taskPerformComboBoxTECComponentSelectedIndex.Status : TaskStatus.WaitingForActivation;

                while ((cnt++ < cnt_max)
                    && (!(nextKey == FormChangeMode.KeyDevice.Empty))) {
                    // ожидать
                    Thread.Sleep (1000);
                    // сообщение для индикации ожидания
                    System.Diagnostics.Debug.WriteLine ($"Ожидание: счетчик <{cnt}> из <{cnt_max}>...");
                }

                // состояния задач по завершению цикла
                taskStatusPerformButtonSetClick =
                    Equals(taskPerformButtonSetClick, null) == false ? taskPerformButtonSetClick.Status : taskStatusPerformButtonSetClick;
                taskStatusPerformComboBoxTECComponentSelectedIndex =
                    Equals(taskPerformComboBoxTECComponentSelectedIndex, null) == false ? taskPerformComboBoxTECComponentSelectedIndex.Status : TaskStatus.WaitingForActivation;
                System.Diagnostics.Debug.WriteLine (string.Format("Окончание ожидания <{0}>, задача-Click is <{1}>, задача-Selected is <{2}>, nextIndex={3}..."
                    , cnt
                    , Equals (taskPerformButtonSetClick, null) == false ? taskPerformButtonSetClick.Status.ToString () : "не создана"
                    , Equals(taskPerformComboBoxTECComponentSelectedIndex, null) == false ? taskPerformComboBoxTECComponentSelectedIndex.Status.ToString() : "не создана"
                    , nextKey));
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine (e.Message);

                Assert.Fail (e.Message);
            }
        }

        /// <summary>
        /// Оброботчик исключений в потоках и запись их в лог
        /// </summary>
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // here you can log the exception ...
            Logging.Logg().Exception(e.Exception, "::Application_ThreadException () - ...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Оборботчик не перехваченного исключения в текущем домене и запись их в лог
        /// </summary>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // here you can log the exception ...
            Logging.Logg().Exception((Exception)e.ExceptionObject, "::AppDomain_UnhandledException () - ...", Logging.INDEX_MESSAGE.NOT_SET);
        }
    }
}
