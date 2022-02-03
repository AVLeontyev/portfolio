using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using StatisticCommon;
using StatisticTrans;
using StatisticTransModes;
using ASUTP;

namespace trans_mt
{
    public class AdminMT : AdminModes
    {
        protected enum StatesMachine
        {
            PPBRValues
            ,
        }

        public AdminMT()
            : base()
        {
        }

        protected override void getPPBRDatesRequest(DateTime date) { }

        protected override int getPPBRDatesResponse(DataTable table, DateTime date) { int iRes = 0; return iRes; }

        protected override void getPPBRValuesRequest(TEC t, IDevice comp, DateTime date/*, AdminTS.TYPE_FIELDS mode*/)
        {
            string query = string.Empty;
            DateTime dtReq = date.Date.Add(-ASUTP.Core.HDateTime.TS_MSK_OFFSET_OF_UTCTIMEZONE);
            int i = -1;

            query +=
                //@"SELECT [objName], [idFactor], [PBR_NUMBER], [Datetime],"
                //    //+ @" SUM([Value_MBT]) as VALUE"
                //    + @" [Value_MBT] as VALUE"
                //+ @" FROM [dbo].[v_ALL_PARAM_MODES_" + t.GetAddingParameter(TEC.ADDING_PARAM_KEY.PREFIX_MODES_TERMINAL).ToString() + @"]" +
                //@" WHERE [ID_Type_Data] = 3" +
                //@" AND [objName] IN (" + string.Join (@",", comp.m_listMTermId.ToArray()) + @")" +
                //@" AND [Datetime] > " + @"'" + dtReq.ToString(@"yyyyMMdd HH:00:00.000") + @"'"
                //    + @" AND [Datetime] <= " + @"'" + dtReq.AddDays(1).ToString(@"yyyyMMdd HH:00:00.000") + @"'"
                //+ @" AND [PBR_NUMBER] > 0"
                ////+ @" GROUP BY [idFactor], [PBR_NUMBER], [Datetime]"
                //+ @" ORDER BY [Datetime], [PBR_NUMBER]"
                $"EXECUTE [dbo].[sp_get_term_modes_values] {t.m_id},'{string.Join (@",", comp.ListMTermId.ToArray ())}','{dtReq.ToString (@"yyyyMMdd HH:00:00.000")}','{dtReq.AddDays (1).ToString (@"yyyyMMdd HH:00:00.000")}'"
                ;

            ASUTP.Database.DbSources.Sources().Request(m_IdListenerCurrent, query);

            //ASUTP.Logging.Logg().Debug($"AdminMT::GetPPBRValuesRequest (TEC={allTECComponents[indxTECComponents].tec.name_shr}, TECComponent={allTECComponents[indxTECComponents].name_shr}, DateTime={dtReq.AddDays(1).ToString(@"dd-MM-yyyy HH:00")}) - вЫход...: query=[{query}]"
            //    , ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override int getPPBRValuesResponse(DataTable table, DateTime date)
        {
            int iRes = 0;
            int i = -1, c = -1 //Переменаые цикла
                , MTermId = -1 //Идентификатор компонента ТЭЦ в системе Модес-Терминал
                , hour = -1 //Переменаая цикла (номер часа)
                , indxFactor = -1 //Индекс типа значения (0 - P, 1 - Pmin, 2 - Pmax)
                //, iMinPBRNumber = -1
                , iMaxPBRNumber = -1;
            INDEX_PLAN_FACTOR j = INDEX_PLAN_FACTOR.Unknown;
            //Номер ПБР для всех типов (P, Pmin, Pmax) значений
            int[] arPBRNumber = new int[(int)INDEX_PLAN_FACTOR.COUNT];
            DataRow[] hourRows;
            RDGStruct[,] arRDGValues = null;

            TECComponent comp = FindTECComponent(CurrentKey) as TECComponent;
            arRDGValues = new RDGStruct[comp.m_listMTermId.Count, m_curRDGValues.Length];

            if (CheckNameFieldsOfTable (table, new string [] { "objName", "idFactor", "Datetime", "Value_MBT" }) == true) {
                for (c = 0; c < comp.m_listMTermId.Count; c++) {
                    MTermId = comp.m_listMTermId [c];

                    for (hour = 1; hour < 25; hour++) {
                        try {
                            //Выбрать строки только для часа 'hour'
                            //hourRows = table.Select(@"Datetime='" + date.Date.AddHours(hour + 1 - ts.Hours).ToString(@"yyyyMMdd HH:00:00.000") + @"'");
                            //hourRows = table.Select(@"Datetime='" + date.Date.AddHours(hour + 1 - ts.Hours) + @"'");
                            //hourRows = table.Select(@"Datetime=#" + date.Date.AddHours(hour + 1 - ts.Hours).ToString(@"yyyyMMdd HH:00:00.000") + @"#");
                            hourRows = table.Select (string.Format (@"objName={0} AND Datetime=#{1}#", MTermId, date.Date.AddHours (hour - ASUTP.Core.HDateTime.TS_MSK_OFFSET_OF_UTCTIMEZONE.Hours).ToString (@"yyyy-MM-dd HH:00:00.000")));

                            //Присвоить исходные для часа значения
                            //PBRNumber = -1;
                            arPBRNumber [(int)INDEX_PLAN_FACTOR.PBR] =
                            arPBRNumber [(int)INDEX_PLAN_FACTOR.Pmin] =
                            arPBRNumber [(int)INDEX_PLAN_FACTOR.Pmax] =
                                -1; // номер набора
                            // значения по типам (0, 1, 2)
                            arRDGValues [c, hour - 1].pbr = -1.0F;
                            arRDGValues [c, hour - 1].pmin = -1.0F;
                            arRDGValues [c, hour - 1].pmax = -1.0F;
                            //ПБР-номер не известен
                            arRDGValues [c, hour - 1].pbr_number = string.Empty;
                            //Проверить количество строк для часа
                            if (hourRows.Length > 0)
                                //ТОлько при наличии строк для часа 'hour'
                                for (i = 0; i < hourRows.Length; i++) {
                                    //Установить тип значения в строке для часа
                                    indxFactor = Int32.Parse (hourRows [i] [@"idFactor"].ToString ());
                                    //Сравнить номер набора строки с ранее обработанным номером набора (в предыдущих строках)
                                    if (!(arPBRNumber [indxFactor] > Int32.Parse (hourRows [i] [@"PBR_NUMBER"].ToString ()))) {//Толькоо, если номер набора в текущей строке больше
                                        //??? номер ПБР назначается для всех 3-х типов значений (P, Pmin, Pmax)
                                        // , но номер ПБР индивидуален для КАЖДого из них
                                        arPBRNumber [indxFactor] = Int32.Parse (hourRows [i] [@"PBR_NUMBER"].ToString ());
                                        ////Вывод на консоль отладочной информации
                                        //for (j = 0; j < hourRows [i].Table.Columns.Count; j ++) {
                                        //    Console.Write(@"[" + hourRows[i].Table.Columns[j].ColumnName + @"] = " + hourRows[i][hourRows[i].Table.Columns[j].ColumnName] + @"; ");
                                        //}
                                        //Console.WriteLine(@"");
                                        //Присвоить значения в ~ от типа
                                        switch (indxFactor) {
                                            case 0: //'P'
                                                if (!(hourRows [i] [@"Value_MBT"] is DBNull))
                                                    arRDGValues [c, hour - 1].pbr = (double)hourRows [i] [@"Value_MBT"];
                                                else
                                                    arRDGValues [c, hour - 1].pbr = 0;
                                                break;
                                            case 1: //'Pmin'
                                                if (!(hourRows [i] [@"Value_MBT"] is DBNull))
                                                    arRDGValues [c, hour - 1].pmin = (double)hourRows [i] [@"Value_MBT"];
                                                else
                                                    arRDGValues [c, hour - 1].pmin = 0;
                                                break;
                                            case 2: //'Pmax'
                                                if (!(hourRows [i] [@"Value_MBT"] is DBNull))
                                                    arRDGValues [c, hour - 1].pmax = (double)hourRows [i] [@"Value_MBT"];
                                                else
                                                    arRDGValues [c, hour - 1].pmax = 0;
                                                break;
                                            default:
                                                break;
                                        }
                                    } else
                                        ;
                                } else
                                //Если не найдено ни одной строки для часа
                                if (hour > 1) {
                                //Если не 1-ый час - пролонгация
                                arRDGValues [c, hour - 1].pbr = arRDGValues [c, hour - 2].pbr;
                                arRDGValues [c, hour - 1].pmin = arRDGValues [c, hour - 2].pmin;
                                arRDGValues [c, hour - 1].pmax = arRDGValues [c, hour - 2].pmax;

                                for (j = INDEX_PLAN_FACTOR.PBR; j < INDEX_PLAN_FACTOR.COUNT; j++)
                                    arPBRNumber [(int)j] = Int32.Parse (arRDGValues [c, hour - 2].pbr_number.Substring (PBR_PREFIX.Length));

                                arRDGValues [c, hour - 1].pbr_number = arRDGValues [c, hour - 2].pbr_number;
                                arRDGValues [c, hour - 1].dtRecUpdate = arRDGValues [c, hour - 2].dtRecUpdate;
                            } else
                                ;

                            //iMinPBRNumber = 25;
                            iMaxPBRNumber = -1;
                            for (j = INDEX_PLAN_FACTOR.PBR; j < INDEX_PLAN_FACTOR.COUNT; j++) {
                                if (arPBRNumber [(int)j] > 0) {
                                    //???при каком индексе присваивать номер набора
                                    //arRDGValues[c, hour - 1].pbr_number = HAdmin.PBR_PREFIX + PBRNumber;
                                    //if (iMinPBRNumber > arPBRNumber[(int)j])
                                    if (iMaxPBRNumber < arPBRNumber [(int)j])
                                        //iMinPBRNumber = arPBRNumber[(int)j];
                                        iMaxPBRNumber = arPBRNumber [(int)j];
                                    else
                                        ;

                                    if (hour > 1) {
                                        switch (j) {
                                            case INDEX_PLAN_FACTOR.PBR:
                                                if (arRDGValues [c, hour - 1].pbr < 0)
                                                    arRDGValues [c, hour - 1].pbr = arRDGValues [c, hour - 2].pbr;
                                                else
                                                    ;
                                                break;
                                            case INDEX_PLAN_FACTOR.Pmin:
                                                if (arRDGValues [c, hour - 1].pmin < 0)
                                                    arRDGValues [c, hour - 1].pmin = arRDGValues [c, hour - 2].pmin;
                                                else
                                                    ;
                                                break;
                                            case INDEX_PLAN_FACTOR.Pmax:
                                                if (arRDGValues [c, hour - 1].pmax < 0)
                                                    arRDGValues [c, hour - 1].pmax = arRDGValues [c, hour - 2].pmax;
                                                else
                                                    ;
                                                break;
                                            default:
                                                break;
                                        }
                                    } else
                                        ;
                                } else
                                    ; //arRDGValues[c, hour - 1].pbr_number = GetPBRNumber (hour);

                                int hh = -1;
                                for (hh = hour; hh > 0; hh--)
                                    //??? Необходима ИНДИВИДуальная проверка номера ПБР
                                    // для каждогоо типа значений (P, Pmin, Pmax)
                                    if (arRDGValues [c, hh - 1].pbr_number.Equals (string.Empty) == false)
                                        if (arPBRNumber [(int)j] < Int32.Parse (arRDGValues [c, hh - 1].pbr_number.Substring (PBR_PREFIX.Length))) {
                                            arPBRNumber [(int)j] = Int32.Parse (arRDGValues [c, hh - 1].pbr_number.Substring (PBR_PREFIX.Length));
                                            //if (iMinPBRNumber > arPBRNumber[(int)j])
                                            if (iMaxPBRNumber < arPBRNumber [(int)j])
                                                //iMinPBRNumber = arPBRNumber[(int)j];
                                                iMaxPBRNumber = arPBRNumber [(int)j];
                                            else
                                                ;

                                            switch (j) {
                                                case INDEX_PLAN_FACTOR.PBR:
                                                    arRDGValues [c, hour - 1].pbr = arRDGValues [c, hh - 1].pbr;
                                                    break;
                                                case INDEX_PLAN_FACTOR.Pmin:
                                                    arRDGValues [c, hour - 1].pmin = arRDGValues [c, hh - 1].pmin;
                                                    break;
                                                case INDEX_PLAN_FACTOR.Pmax:
                                                    arRDGValues [c, hour - 1].pmax = arRDGValues [c, hh - 1].pmax;
                                                    break;
                                                default:
                                                    break;
                                            }

                                            //arRDGValues[c, hour - 1].pbr_number = arRDGValues[c, hh - 1].pbr_number;

                                            //break;
                                        } else
                                            ;
                                    else
                                        ;
                                // цикл-окончание hh
                            } // цикл-окончание по индексу типов значений

                            arRDGValues [c, hour - 1].pbr_number = $"{HAdmin.PBR_PREFIX}{iMaxPBRNumber}";

                            arRDGValues [c, hour - 1].dtRecUpdate = DateTime.MinValue;

                            arRDGValues [c, hour - 1].fc = false;
                            arRDGValues [c, hour - 1].recomendation = 0;
                            arRDGValues [c, hour - 1].deviationPercent = false;
                            arRDGValues [c, hour - 1].deviation = 0;
                        } catch (Exception e) {
                            ASUTP.Logging.Logg ().Exception (e, @"AdminMT::GetPPBRValuesResponse () - ...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    } // цикл-окончание по номеру часа 'hour'
                } // цикл-окончание по идентификатору составного элемента (только ГТП3-6 НТЭЦ-5)
            } else
                ASUTP.Logging.Logg ().Error ($"таблица не содержит необходимый набор полей", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);

            for (hour = 1; hour < 25; hour++)
            {
                m_curRDGValues[hour - 1].pbr = -1F;
                m_curRDGValues[hour - 1].pmin = -1F;
                m_curRDGValues[hour - 1].pmax = -1F;

                m_curRDGValues[hour - 1].pbr_number = string.Empty;

                for (c = 0; c < comp.m_listMTermId.Count; c++)
                {
                    MTermId = comp.m_listMTermId[c];

                    if (!(arRDGValues[c, hour - 1].pbr < 0)) {
                        // значение есть, необходимо суммировать, установить в "0"
                        if (m_curRDGValues[hour - 1].pbr < 0) m_curRDGValues[hour - 1].pbr = 0F; else ;
                        // суммируем
                        m_curRDGValues[hour - 1].pbr += arRDGValues[c, hour - 1].pbr;
                    } else
                        ;
                    if (!(arRDGValues[c, hour - 1].pmin < 0)) {
                        // значение есть, необходимо суммировать, установить в "0"
                        if (m_curRDGValues[hour - 1].pmin < 0) m_curRDGValues[hour - 1].pmin = 0F; else ;
                        // суммируем
                        m_curRDGValues[hour - 1].pmin += arRDGValues[c, hour - 1].pmin;
                    } else
                        ;
                    if (!(arRDGValues[c, hour - 1].pmax < 0)) {
                        // значение есть, необходимо суммировать, установить в "0"
                        if (m_curRDGValues[hour - 1].pmax < 0) m_curRDGValues[hour - 1].pmax = 0F; else ;
                        // суммируем
                        m_curRDGValues[hour - 1].pmax += arRDGValues[c, hour - 1].pmax;
                    }
                    else
                        ;
                } // цикл-окончание по идентификатору составного элемента (только ГТП3-6 НТЭЦ-5)
                //???
                m_curRDGValues[hour - 1].pbr_number = arRDGValues[0, hour - 1].pbr_number;
                //???
                m_curRDGValues[hour - 1].dtRecUpdate = DateTime.MinValue;

                m_curRDGValues[hour - 1].fc = false;
                m_curRDGValues[hour - 1].recomendation = 0;
                m_curRDGValues[hour - 1].deviationPercent = false;
                m_curRDGValues[hour - 1].deviation = 0;
            } // цикл-окончание по номеру часа 'hour'

            return iRes;
        }

        protected override bool InitDbInterfaces()
        {
            bool bRes = true;
            int i = -1;

            if (m_list_tec.Count > 0)
            {
                m_IdListenerCurrent = ASUTP.Database.DbSources.Sources().Register(m_list_tec[0].connSetts[(int)StatisticCommon.CONN_SETT_TYPE.MTERM], true, @"Modes-Terminale");

                bRes = false;
            }
            else
                ;

            //for (i = 0; i < allTECComponents.Count; i ++)
            //{
            //    if (modeTECComponent (i) == FormChangeMode.MODE_TECCOMPONENT.GTP)
            //    {
            //        m_listMCId.Add (allTECComponents [i].m_MCId.ToString ());
            //        //m_listDbInterfaces[0].ListenerRegister();
            //    }
            //    else
            //        ;
            //}

            //List <Modes.BusinessLogic.IGenObject> listIGO = (((DbInterface)m_listDbInterfaces[0]).GetListIGO(listMCId));

            return bRes;
        }

        protected override int StateRequest(int /*StatesMachine*/ state)
        {
            int result = 0;

            string msg = string.Empty;
            StatesMachine stateMachine = (StatesMachine)state;
            TECComponent comp;

            comp = FindTECComponent (CurrentKey) as TECComponent;

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                    ActionReport("Получение данных плана.");
                    if (IsCanUseTECComponents == true)
                        getPPBRValuesRequest(comp.tec, comp, m_curDate.Date/*, AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS*/);
                    else
                        result = -1;
                    break;
                default:
                    break;
            }

            //Logging.Logg().Debug(@"AdminMT::StateRequest () - state=" + state.ToString() + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);

            return result;
        }

        protected override int StateCheckResponse(int /*StatesMachine*/ state, out bool error, out object table)
        {
            int iRes = -1;

            error = true;
            table = null;

            StatesMachine stateMachine = (StatesMachine)state;

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                    //bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, false*/);
                    iRes = response(m_IdListenerCurrent, out error, out table/*, false*/);
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int /*StatesMachine*/ state, object table)
        {
            int result = -1;

            StatesMachine stateMachine = (StatesMachine)state;

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                    delegateStopWait();

                    result = getPPBRValuesResponse(table as DataTable, m_curDate);
                    if (result == 0)
                    {
                        readyData(m_curDate, true);
                    }
                    else
                        ;
                    break;
                default:
                    break;
            }

            if (result == 0)
                ReportClear(false);
            else
                ;

            //Logging.Logg().Debug(@"AdminMT::StateResponse () - state=" + state.ToString() + @", result=" + result.ToString() + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);

            return result;
        }

        protected override INDEX_WAITHANDLE_REASON StateErrors(int /*StatesMachine*/ state, int request, int result)
        {
            INDEX_WAITHANDLE_REASON reasonRes = INDEX_WAITHANDLE_REASON.SUCCESS;

            bool bClear = false;

            StatesMachine stateMachine = (StatesMachine)state;

            delegateStopWait ();

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                    if (request == 0)
                        ErrorReport("Ошибка разбора данных плана. Переход в ожидание.");
                    else
                    {
                        ErrorReport("Ошибка получения данных плана. Переход в ожидание.");

                        bClear = true;
                    }
                    break;
                default:
                    break;
            }

            if (bClear)
            {
                ClearValues();
                //ClearTables();
            }
            else
                ;

            errorData?.Invoke (state);

            return reasonRes;
        }

        protected override void StateWarnings(int state, int request, int result)
        {
        }

        public override void GetRDGValues(FormChangeMode.KeyDevice key, DateTime date)
        {
            delegateStartWait();
            lock (m_lockState)
            {
                ClearStates();

                CurrentKey = key;

                ClearValues();

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                //if (m_listIGO.Count == 0)
                //{
                //    AddState((int)StatesMachine.InitIGO);
                //}
                //else
                //    ;

                AddState((int)StatesMachine.PPBRValues);

                Run(@"AdminMC::GetRDGValues ()");
            }
        }

        /// <summary>
        /// Подготовить список идентификаторов ГТП для формирования запроса на получение данных
        ///  , ??? копия AdminTS_KomDisp
        /// </summary>
        /// <returns>Ключ 0-го оборудования из списка</returns>
        public override FormChangeMode.KeyDevice PrepareActionRDGValues ()
        {
            List<FormChangeMode.KeyDevice> listKey;

            listKey = GetListKeyTECComponent (FormChangeMode.MODE_TECCOMPONENT.GTP, true);

            if (_listTECComponentKey == null)
                _listTECComponentKey = new List<FormChangeMode.KeyDevice> ();
            else
                ;

            try {
                // проверить на наличие дубликатов
                if (listKey.Count - listKey.Distinct ().Count () == 0) {
                    _listTECComponentKey.Clear ();
                    listKey.ForEach ((key) => {
                        if (_listTECComponentKey.Contains (key) == false)
                            _listTECComponentKey.Add (key);
                        else
                            Logging.Logg ().Error (string.Format ("trans_mc.AdminMC::PrepareExportRDGValues () - добавление повторяющегося индекса {0}...", key.ToString ()), Logging.INDEX_MESSAGE.NOT_SET);
                    });

                    //TODO:
                    // аварийно прекратить выполнение предыдущей операции сохранения ПБР-значений
                } else
                    Logging.Logg ().Error (string.Format ("trans_mc.AdminMC::PrepareExportRDGValues () - в переданном списке <{0}> есть дубликаты...", string.Join (",", listKey.Select (key => key.ToString ()).ToArray ()))
                        , Logging.INDEX_MESSAGE.NOT_SET);

                Logging.Logg ().Action ($"trans_mc.AdminMC::PrepareExportRDGValues () - подготовлен список для опроса: <{string.Join (", ", _listTECComponentKey.ConvertAll<string> (key => key.Id.ToString ()).ToArray ())}>..."
                    , Logging.INDEX_MESSAGE.NOT_SET);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, string.Format ("trans_mc.AdminMC::PrepareExportRDGValues () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }

            return base.PrepareActionRDGValues ();
        }
    }
}