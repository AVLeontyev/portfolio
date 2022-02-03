using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using ASUTP.Database;
using ASUTP.Core;
using System.Runtime.Serialization;

namespace StatisticCommon
{
    public interface IDevice
    {
        string name_shr { get; set; }

        int m_id { get; set; }

        TEC tec { get; }

        List<TECComponentBase> ListLowPointDev { get; }

        List<int> ListMCentreId { get; }

        List<int> ListMTermId { get; }
    }
    /// <summary>
    /// Класс для описания основного компонента ТЭЦ
    /// </summary>
    [DataContract]
    public class TECComponentBase
    {
        /// <summary>
        /// Перечисление - типы компонентов в отношении его принадлежности к электро-, или тепловой части оборудования ТЭЦ
        /// </summary>
        [DataContract]
        public enum TYPE : short { UNKNOWN = -1, TEPLO, ELECTRO, COUNT }
        /// <summary>
        /// Тип компонента в отношении его принадлежности к электро-, или тепловой части оборудования ТЭЦ
        /// </summary>
        public TYPE Type {
            get {
                TYPE typeRes = TYPE.UNKNOWN;

                if ((IsVyvod == true)
                    || (IsParamVyvod == true))
                    typeRes = TYPE.TEPLO;
                else
                    if ((IsGTP == true)
                        || (IsGTP_LK == true)
                        || (IsPC == true)
                        || (IsTG == true))
                        typeRes = TYPE.ELECTRO;
                    else
                        ;

                return typeRes;
            }
        }
        /// <summary>
        /// Признак оборудования нижнего уровня
        /// </summary>
        public bool IsLowPointDev {
            get {
                bool bRes = false;

                if ((IsTG == true)
                    || (IsParamVyvod))
                    bRes = true;
                else
                    ;

                return bRes;
            }
        }
        /// <summary>
        /// Идентификаторы для типов компонента ТЭЦ
        /// </summary>
        [DataContract]
        public enum ID : int { TEC, LK = 10, GTP = 100, GTP_LK = 200, PC = 500, VYVOD = 600, TG = 1000, PARAM_VYVOD = 2000, MAX = 10000 }
        /// <summary>
        /// Тип делегата для проверки идентификатора на принадлежность к группе
        /// </summary>
        /// <param name="verId">Идентификатор для проверки</param>
        /// <returns>Признак принадлежности к группе</returns>
        private delegate bool BoolDelegateIntFunc(int verId);
        /// <summary>
        /// Словарь с методами для проверки идентификатора компонента на проверку к принадлежности к группе
        /// </summary>
        private static Dictionary<ID, BoolDelegateIntFunc> _dictVerifyIDDelegate = new Dictionary<ID, BoolDelegateIntFunc> {
            { ID.TEC, VerifyTEC }
            , { ID.LK, VerifyLK }
            , { ID.GTP, VerifyGTP }
            , { ID.GTP_LK, VerifyGTP_LK }
            , { ID.PC, VerifyPC }
            , { ID.TG, VerifyTG }
            , { ID.VYVOD, VerifyVyvod }
            , { ID.PARAM_VYVOD, VerifyParamVyvod }
        };
        /// <summary>
        /// Идентификаторы "владельцев" для ТГ (ГТП, Б(Гр)ЩУ)
        /// </summary>
        [DataMember]
        public List<FormChangeMode.KeyDevice> m_keys_owner;
        /// <summary>
        /// Краткое наименовнаие компонента
        /// </summary>
        [DataMember]
        public string name_shr { get; set; }
        /// <summary>
        /// Нименование для использования в будущем (при расширении)
        /// </summary>
        [DataMember]
        public string name_future;
        /// <summary>
        /// Идентификатор компонента (из БД конфигурации)
        /// </summary>
        [DataMember]
        public int m_id { get; set; }
        /// <summary>
        /// Индекс столбца в книге Excel со значениями ПБР для экспорта значений (подзадача сравнения ПБР-значений с аналогичными значениями из-вне, только для ГТП)
        /// </summary>
        [DataMember]
        public int m_indx_col_export_pbr_excel;
        /// <summary>
        /// Индекс столбца в книге Excel со значениями РДГ для компонента (в настоящее время только для НТЭЦ-5)
        /// </summary>
        [DataMember]
        public int m_indx_col_rdg_excel;
        /// <summary>
        /// Коэффициент для тонкой настройки алгоритма сигнализации
        /// </summary>
        [DataMember]
        public decimal m_dcKoeffAlarmPcur;

        //Копия из 'class PanelStatisticView : PanelStatistic' - из 'PanelStatisticView' класса требуется исключть???
        [DataMember]
        public volatile string[] m_SensorsStrings_ASKUE = { string.Empty, string.Empty }; //Только для особенной ТЭЦ (Бийск) - 3-х, 30-ти мин идентификаторы
        [DataMember]
        public volatile string m_SensorsString_SOTIASSO = string.Empty;
        [DataMember]
        public volatile string m_SensorsString_VZLET = string.Empty;
        /// <summary>
        /// Коструктор - основной (без параметров)
        /// </summary>
        public TECComponentBase()
        {
            m_dcKoeffAlarmPcur = -1;

            m_keys_owner =
            //Нет ни одного владельца
                new List<FormChangeMode.KeyDevice> ();
        }

        public FormChangeMode.MODE_TECCOMPONENT Mode
        {
            get
            {
                return GetMode (m_id);
            }
        }
        /// <summary>
        /// Возвратить тип (режим) компонента по указанному идентификатору
        /// </summary>
        /// <param name="id">Идентификатор компонента</param>
        /// <returns>Тип (режим) компонента</returns>
        public static FormChangeMode.MODE_TECCOMPONENT GetMode(int id)
        {
            return (id < (int)ID.GTP) == true ? FormChangeMode.MODE_TECCOMPONENT.TEC :
                ((id > (int)ID.GTP) && (id < (int)ID.PC)) == true ? FormChangeMode.MODE_TECCOMPONENT.GTP :
                ((id > (int)ID.PC) && (id < (int)ID.VYVOD)) == true ? FormChangeMode.MODE_TECCOMPONENT.PC :
                ((id > (int)ID.VYVOD) && (id < (int)ID.TG)) == true ? FormChangeMode.MODE_TECCOMPONENT.ANY : // для ВЫВОДа нет идентификатора типа
                    ((id > (int)ID.TG) && (id < (int)ID.PARAM_VYVOD)) == true ? FormChangeMode.MODE_TECCOMPONENT.TG :
                    ((id > (int)ID.PARAM_VYVOD) && (id < (int)ID.MAX)) == true ? FormChangeMode.MODE_TECCOMPONENT.TG : //??? (эмуляция для 'SaveChanges') для параметра ВЫВОДа нет идентификатора типа
                        FormChangeMode.MODE_TECCOMPONENT.ANY;
        }

        public bool IsLK { get { return VerifyLK(m_id); } }
        /// <summary>
        /// Признак принадлежности компонента к группе ГТП
        /// </summary>
        public bool IsGTP { get { return VerifyGTP(m_id); } }
        /// <summary>
        /// Признак принадлежности компонента к группе ГТП(ЛК)
        /// </summary>
        public bool IsGTP_LK { get { return VerifyGTP_LK(m_id); } }
        /// <summary>
        /// Признак принадлежности компонента к группе щиты управления
        ///  (блочные, групповые)
        /// </summary>
        public bool IsPC { get { return VerifyPC(m_id); } }
        /// <summary>
        /// Признак принадлежности компонента к группе ТГ
        /// </summary>
        public bool IsTG { get { return VerifyTG(m_id); } }
        /// <summary>
        /// Признак принадлежности компонента к группе выводов
        /// </summary>
        public bool IsVyvod { get { return VerifyVyvod(m_id); } }
        /// <summary>
        /// Признак принадлежности компонента к группе параметры вывода
        /// </summary>
        public bool IsParamVyvod { get { return VerifyParamVyvod(m_id); } }
        /// <summary>
        /// Проверить идентификатор на принадлежность к одной из групп
        /// </summary>
        /// <param name="verId">Идентификатор для проверки</param>
        /// <param name="arId">Идентификаторы групп компонентов</param>
        /// <returns>Признак принадлежности к одной из групп, перечисленных в аргументе 'arId'</returns>
        public static bool VerifyID(int verId, params ID[] arId)
        {
            bool bRes = false;

            foreach (ID id in arId)
                if (bRes = _dictVerifyIDDelegate[id](verId) == true)
                    break;
                else
                    ;

            return bRes;
        }

        public static bool VerifyTEC(int id) { return (id > 0) && (id < (int)ID.LK); }

        public static bool VerifyLK(int id) { return (id > (int)ID.LK) && (id < (int)ID.GTP); }

        public static bool VerifyGTP(int id) { return (id > (int)ID.GTP) && (id < (int)ID.GTP_LK); }

        public static bool VerifyGTP_LK(int id) { return (id > (int)ID.GTP_LK) && (id < (int)ID.PC); }
        /// <summary>
        /// Признак принадлежности компонента к группе щиты управления
        ///  (блочные, групповые)
        /// </summary>
        public static bool VerifyPC(int id) { return (id > (int)ID.PC) && (id < (int)ID.VYVOD); }
        /// <summary>
        /// Признак принадлежности компонента к группе ТГ
        /// </summary>
        public static bool VerifyTG(int id) { return (id > (int)ID.TG) && (id < (int)ID.PARAM_VYVOD); }

        public static bool VerifyVyvod(int id) { return (id > (int)ID.VYVOD) && (id < (int)ID.TG); }

        public static bool VerifyParamVyvod(int id) { return (id > (int)ID.PARAM_VYVOD) && (id < (int)ID.MAX); }

        public static TYPE GetType (int id)
        {
            return (VerifyGTP (id) == true) || (VerifyLK (id) == true) || (VerifyGTP_LK (id) == true) || (VerifyPC (id) == true) || (VerifyTG (id) == true)
                ? TYPE.ELECTRO
                    : (VerifyVyvod (id) == true) || (VerifyParamVyvod (id) == true)
                        ? TYPE.TEPLO
                            : TYPE.UNKNOWN;
        }
    }

    //public partial class TEC {
    /// <summary>
    /// Класс для описания компонента ТЭЦ - ТГ
    /// </summary>
    [DataContract]
    public class TG : TECComponentBase
    {
        [DataContract]
        public struct AISKUE_KEY : IEquatable<AISKUE_KEY>
        {
            public int IdObject;

            public int IdItem;

            public bool Equals (AISKUE_KEY other)
            {
                return this == other;
            }

            public static bool operator==(AISKUE_KEY pKey1, AISKUE_KEY pKey2)
            {
                return (pKey1.IdObject == pKey2.IdObject)
                    && (pKey1.IdItem == pKey2.IdItem);
            }

            public static bool operator != (AISKUE_KEY pKey1, AISKUE_KEY pKey2)
            {
                return (!(pKey1.IdObject == pKey2.IdObject))
                    || (!(pKey1.IdItem == pKey2.IdItem));
            }

            public override bool Equals (object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode ()
            {
                return base.GetHashCode ();
            }

            public override string ToString ()
            {
                return $"([OBJECT]={IdObject} AND [ITEM]={IdItem})";
            }
        }
        /// <summary>
        /// Перечисление - индексы элементов интерфейса для отображения значений ТГ
        /// </summary>
        [DataContract]
        public enum INDEX_VALUE : int
        {
            FACT //факт.
            , TM //телемеханика
            , LABEL_DESC //описание (краткое наименование) ТГ
                , COUNT_INDEX_VALUE
        }; //Количество индексов
        /// <summary>
        /// Перечисление - возможные состояния ТГ
        /// </summary>
        [DataContract]
        public enum INDEX_TURNOnOff : int { OFF = -1, UNKNOWN, ON };
        /// <summary>
        /// Массив идентификаторов ТГ в АИИС КУЭ (размерность по 'ID_TIME')
        ///  для особенной ТЭЦ (Бийск) различаются 3-х и 30-ти мин идентификаторы
        ///  для остальных - совпадают
        /// </summary>
        [DataMember]
        public AISKUE_KEY[] m_aiskue_keys;
        /// <summary>
        /// Признак состояния ТГ
        /// </summary>
        [DataMember]
        public INDEX_TURNOnOff m_TurnOnOff; //Состояние -1 - выкл., 0 - неизвестно, 1 - вкл. (только для AdminAlarm)
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public TG()
            : base()
        {
            m_aiskue_keys = new AISKUE_KEY[(int)HDateTime.INTERVAL.COUNT_ID_TIME];

            m_TurnOnOff = INDEX_TURNOnOff.UNKNOWN; //Неизвестное состояние
        }

        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// <param name="row_param_tg">Строка из набора - результата запроса к представлению БД [ALL_PARAM_TG]</param>
        /// </summary>
        public TG (DataRow row_param_tg)
            : this ()
        {
            initTG (row_param_tg);
        }

        ///// <summary>
        ///// Конструктор - дополнительный (с параметрами)
        ///// </summary>
        //public TG(DataRow row_tg, DataRow row_param_tg)
        //    // передавать только строку из представления
        //    : this (row_param_tg)
        //{
        //    //initTG(row_tg, row_param_tg);
        //}

        //private void initTG(DataRow row_tg, DataRow row_param_tg)
        //{
        //    #region Здесь столбцы из старой таблицы [LIST_TG]
        //    // row_tg: NAME_SHR, NAME_FUTURE, ID, INDX_COL_RDG_EXCEL
        //    // , теперь используется строки из представления [ALL_PARAM_TG]
        //    // , к наименованиям полей добавлен префикс "_TG"
        //    name_shr = row_param_tg ["NAME_SHR_TG"].ToString();
        //    if (DbTSQLInterface.IsNameField(row_param_tg, "NAME_FUTURE_TG") == true) name_future = row_param_tg ["NAME_FUTURE_TG"].ToString(); else ;
        //    m_id = Convert.ToInt32(row_param_tg ["ID_TG"]);
        //    if (!(row_param_tg ["INDX_COL_RDG_EXCEL_TG"] is System.DBNull))
        //        m_indx_col_rdg_excel = Convert.ToInt32(row_param_tg ["INDX_COL_RDG_EXCEL_TG"]);
        //    else
        //        ;
        //    #endregion

        //    #region Здесь столбцы из нового представления [ALL_PARAM_TG]
        //    m_strKKS_NAME_TM = row_param_tg[@"KKS_NAME"].ToString();
        //    m_arIds_fact[(int)HDateTime.INTERVAL.MINUTES] =
        //        //Int32.Parse(row_param_tg[@"ID_IN_ASKUE_3"].ToString())
        //        // ChrjapinAN, 26.12.2017 переход на составной ключ "OBJECT/ITEM"
        //        new AISKUE_KEY () { IdObject = Int32.Parse (row_param_tg [@"PIRAMIDA_OBJECT"].ToString ()), IdItem = Int32.Parse (row_param_tg [@"PIRAMIDA_ITEM"].ToString ()) }
        //        ;
        //    m_arIds_fact[(int)HDateTime.INTERVAL.HOURS] =
        //        //Int32.Parse(row_param_tg[@"ID_IN_ASKUE_30"].ToString())
        //        // ChrjapinAN, 26.12.2017 переход на составной ключ "OBJECT/ITEM"
        //        new AISKUE_KEY () { IdObject = Int32.Parse (row_param_tg [@"PIRAMIDA_OBJECT"].ToString ()), IdItem = Int32.Parse (row_param_tg [@"PIRAMIDA_ITEM"].ToString ()) }
        //        ;
        //    #endregion
        //}

        /// <summary>
        /// Инициализировать значения полей свойств объекта из строки представления БД
        /// </summary>
        /// <param name="row_param_tg">Строка из набора - результата запроса к представлению БД [ALL_PARAM_TG]</param>
        private void initTG (DataRow row_param_tg)
        {
            // , теперь используется строки из представления [ALL_PARAM_TG]
            // , к наименованиям полей добавлен префикс "_TG"
            name_shr = row_param_tg ["NAME_SHR_TG"].ToString ();
            if (DbTSQLInterface.IsNameField (row_param_tg, "NAME_FUTURE_TG") == true)
                name_future = row_param_tg ["NAME_FUTURE_TG"].ToString ();
            else
                ;
            m_id = Convert.ToInt32 (row_param_tg ["ID_TG"]);
            if (!(row_param_tg ["INDX_COL_RDG_EXCEL_TG"] is System.DBNull))
                m_indx_col_rdg_excel = Convert.ToInt32 (row_param_tg ["INDX_COL_RDG_EXCEL_TG"]);
            else
                ;

            m_SensorsString_SOTIASSO = row_param_tg [@"KKS_NAME"].ToString ();
            m_aiskue_keys [(int)HDateTime.INTERVAL.MINUTES] =
                //Int32.Parse(row_param_tg[@"ID_IN_ASKUE_3"].ToString())
                // ChrjapinAN, 26.12.2017 переход на составной ключ "OBJECT/ITEM"
                new AISKUE_KEY () { IdObject = Int32.Parse (row_param_tg [@"PIRAMIDA_OBJECT"].ToString ()), IdItem = Int32.Parse (row_param_tg [@"PIRAMIDA_ITEM"].ToString ()) }
                ;
            m_SensorsStrings_ASKUE [(int)HDateTime.INTERVAL.MINUTES] = m_aiskue_keys [(int)HDateTime.INTERVAL.MINUTES].ToString ();
            m_aiskue_keys [(int)HDateTime.INTERVAL.HOURS] =
                //Int32.Parse(row_param_tg[@"ID_IN_ASKUE_30"].ToString())
                // ChrjapinAN, 26.12.2017 переход на составной ключ "OBJECT/ITEM"
                new AISKUE_KEY () { IdObject = Int32.Parse (row_param_tg [@"PIRAMIDA_OBJECT"].ToString ()), IdItem = Int32.Parse (row_param_tg [@"PIRAMIDA_ITEM"].ToString ()) }
                ;
            m_SensorsStrings_ASKUE [(int)HDateTime.INTERVAL.HOURS] = m_aiskue_keys [(int)HDateTime.INTERVAL.HOURS].ToString ();
        }
    }
    //} partial class TEC
    /// <summary>
    /// Класс для описания компонента ТЭЦ (ГТП, Б(Гр)ЩУ)
    /// </summary>
    [DataContract]
    public class TECComponent : TECComponentBase,  IDevice
    {
        /// <summary>
        /// Список идентификаторов в Модес-Центр
        /// </summary>
        [DataMember]
        public List<int> m_listMCentreId;
        /// <summary>
        /// Список идентификаторов в Модес-Терминале
        /// </summary>
        [DataMember]
        public List<int> m_listMTermId;
        /// <summary>
        /// Список ТГ
        /// </summary>
        [DataMember]
        private List<TECComponentBase> m_listLowPointDev;

        public List<TECComponentBase> ListLowPointDev
        {
            get
            {
                return m_listLowPointDev;
            }
        }
        /// <summary>
        /// Объект ТЭЦ - "владелец" компонента
        /// </summary>
        [DataMember]
        public TEC tec { get; }

        public List<int> ListMCentreId
        {
            get
            {
                return m_listMCentreId;
            }
        }

        public List<int> ListMTermId
        {
            get
            {
                return m_listMTermId;
            }
        }

        [DataMember]
        public bool m_bKomUchet;
        /// <summary>
        /// Конструктор - дополнительный
        /// </summary>
        public TECComponent(TEC tec, DataRow rComp)
            : this (tec)
        {
            name_shr = rComp["NAME_SHR"].ToString(); //rComp["NAME_GNOVOS"]
            if (DbTSQLInterface.IsNameField(rComp, "NAME_FUTURE") == true) this.name_future = rComp["NAME_FUTURE"].ToString(); else ;
            m_id = Convert.ToInt32(rComp["ID"]);
            m_listMCentreId = getMCentreId(rComp);
            m_listMTermId = getMTermId(rComp);
            m_indx_col_export_pbr_excel = ((DbTSQLInterface.IsNameField(rComp, "INDX_COL_EXPORT_PBR_EXCEL") == true) && (!(rComp["INDX_COL_EXPORT_PBR_EXCEL"] is System.DBNull)))
                ? Convert.ToInt32(rComp["INDX_COL_EXPORT_PBR_EXCEL"])
                    : -1; // значение по умолчанию "-1" - не установлено
            if ((DbTSQLInterface.IsNameField (rComp, "INDX_COL_RDG_EXCEL") == true) && (!(rComp["INDX_COL_RDG_EXCEL"] is System.DBNull)))
                m_indx_col_rdg_excel = Convert.ToInt32(rComp["INDX_COL_RDG_EXCEL"]);
            else
                ;
            if ((DbTSQLInterface.IsNameField (rComp, "KoeffAlarmPcur") == true) && (!(rComp["KoeffAlarmPcur"] is System.DBNull)))
                m_dcKoeffAlarmPcur = Convert.ToInt32(rComp["KoeffAlarmPcur"]);
            else
                ;

            if ((DbTSQLInterface.IsNameField(rComp, @"KOM_UCHET") == true) && (!(rComp[@"KOM_UCHET"] is System.DBNull)))
                m_bKomUchet = Convert.ToByte(rComp[@"KOM_UCHET"]) == 1 ? true : false;
            else
                m_bKomUchet = true;
        }

        public TECComponent (TEC tec, TG tg)
            : this (tec)
        {
            name_shr = tg.name_shr;
            m_id = tg.m_id;

            name_future = tg.name_future;

            m_indx_col_export_pbr_excel = tg.m_indx_col_export_pbr_excel;
            m_indx_col_rdg_excel = tg.m_indx_col_rdg_excel;

            m_dcKoeffAlarmPcur = tg.m_dcKoeffAlarmPcur;
        }

        public TECComponent (TEC tec, Vyvod.ParamVyvod pv)
            : this (tec)
        {
            name_shr = pv.name_shr;
            m_id = pv.m_id;

            name_future = pv.name_future;

            m_indx_col_export_pbr_excel = pv.m_indx_col_export_pbr_excel;
            m_indx_col_rdg_excel = pv.m_indx_col_rdg_excel;

            m_dcKoeffAlarmPcur = pv.m_dcKoeffAlarmPcur;
        }
        /// <summary>
        /// Конструктор - дополнительный
        /// </summary>
        private TECComponent(TEC tec)
        {
            this.tec = tec;

            m_listLowPointDev = new List<TECComponentBase>();
            m_listMCentreId =
            m_listMTermId = null;
        }

        protected List<int> getModesId(DataRow r, string col)
        {
            int i = -1
                , id = -1;
            List<int> listRes = new List<int>();

            if ((DbTSQLInterface.IsNameField (r, col) == true) && (!(r[col] is DBNull)))
            {
                string[] ids = r[col].ToString().Split(',');
                for (i = 0; i < ids.Length; i++)
                    if (ids[i].Length > 0)
                        if (Int32.TryParse(ids[i], out id) == true)
                            listRes.Add(id);
                        else
                            listRes.Add(-1);
                    else
                        listRes.Add(-1);
            }
            else
                ;

            return listRes;
        }

        protected List<int> getMCentreId(DataRow r)
        {
            return getModesId(r, @"ID_MC");
        }

        protected List<int> getMTermId(DataRow r)
        {
            return getModesId(r, @"ID_MT");
        }

        public int AddLowPointDev (TECComponentBase comp)
        {
            int iRes = 0;

            try {
                m_listLowPointDev.Add (comp);

                iRes = m_listLowPointDev.Count;
            } catch {
                iRes = -1;
            }

            return iRes;
        }
    }

    [Serializable]
    public class Vyvod : TECComponent
    {//??? как класс не требутся совсем - нет отличий от 'TECComponent'
        /// <summary>
        /// Идентификаторы параметров нижнего уровня
        ///  следует добавить ТГ-активная/реактивная мощность
        /// </summary>
        public enum ID_PARAM : short { UNKNOWN = -1
            , G_PV = 1, G_OV
            , T_PV, T_OV
            , P_PV, P_OV
            , G2_PV, G2_OV
            , T2_PV, T2_OV
            , }

        public class ParamVyvod : TECComponentBase
        {
            public enum INDEX_VALUE : short {
                FACT //факт. (основной источник данных)
                , DEVIAT //телемеханика (альтерн./источник данных)
                , LABEL_DESC //описание (краткое наименование) параметра ВЫВОДА
                    , COUNT
            }; //Количество индексов

            //public int m_owner_vyvod;

            public ID_PARAM m_id_param;
            public string m_Symbol;
            public int m_typeAgregate;

            public ParamVyvod(DataRow r)
                : base ()
            {
                m_id = Convert.ToInt32(r[@"ID"]);

                Initialize(r);
            }

            public void Initialize(DataRow r)
            {
                int iVzletGrafa = -1;

                m_keys_owner.Add(new FormChangeMode.KeyDevice () { Id = Convert.ToInt32 (r [@"ID_VYVOD"]), Mode = FormChangeMode.MODE_TECCOMPONENT.VYVOD });

                m_id_param = (ID_PARAM)Convert.ToInt16(r[@"ID_PARAM"]);
                iVzletGrafa = (int)r[@"VZLET_GRAFA"];
                m_SensorsString_VZLET = TEC.TypeDbVzlet == TEC.TYPE_DBVZLET.KKS_NAME ? ((string)r[@"KKS_NAME"]).Trim() :
                        ((TEC.TypeDbVzlet == TEC.TYPE_DBVZLET.GRAFA) && (iVzletGrafa > 0)) ? @"Графа_" + iVzletGrafa.ToString() : string.Empty
                    ;

                name_shr = ((string)r[@"NAME_SHR"]).Trim();
                m_Symbol = ((string)r[@"SYMBOL"]).Trim();
                m_typeAgregate = Convert.ToByte(r[@"TYPE_AGREGATE"]);
            }
        }

        public Vyvod()
            : this(null, new DataRow[] {  })
        {
        }

        public Vyvod(TEC tec, DataRow row_param)
            : this(tec, new DataRow[] { row_param })
        {
        }
        /// <summary>
        /// Конструктор - основной (c параметрами)
        /// </summary>
        /// <param name="tec">Объект-ТЭЦ - родительский по отношению к создаваемому объекту</param>
        /// <param name="r">Строка со значениями свойств создаваемого объекта-ВЫВОДа</param>
        public Vyvod(TEC tec, DataRow[] rows_param)
            : base(tec, rows_param[0])
        {
        }
    }
}
