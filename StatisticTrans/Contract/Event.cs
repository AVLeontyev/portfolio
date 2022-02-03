using System;
using System.Collections.ObjectModel;

namespace StatisticTrans
{
    [System.Serializable]
    public enum EVENT
    {
        Unknown = -1
            , OnData53500Modified, OnMaket53500Changed, OnPlanDataChanged
            , OnModesEvent
    }

    [System.Serializable]
    public enum ID_EVENT : short
    {
        Unknown = -1, HANDLER_CONNECT
        , GENOBJECT_MODIFIED
        , RELOAD_PLAN_VALUES, NEW_PLAN_VALUES
        , PHANTOM_RELOAD_PLAN_VALUES, REQUEST_PLAN_VALUES
        , MC_SERVICE
    }
}

namespace StatisticTrans.Contract
{
    public static class EventArgExtensions
    {
        public static EVENT TranslateEvent (this EventArgs arg, ID_EVENT ev)
        {
            EVENT evRes = EVENT.Unknown;

            arg.TranslateEvent (ev, out evRes);

            return evRes;
        }

        public static void TranslateEvent (this EventArgs arg, EVENT ev, out ID_EVENT evRes)
        {
            evRes = ID_EVENT.Unknown;

            switch (ev) {
                case EVENT.OnData53500Modified:
                    evRes = ID_EVENT.GENOBJECT_MODIFIED;
                    break;
                case EVENT.OnMaket53500Changed:
                    evRes = ID_EVENT.RELOAD_PLAN_VALUES;
                    break;
                case EVENT.OnPlanDataChanged:
                    evRes = ID_EVENT.NEW_PLAN_VALUES;
                    break;
                case EVENT.OnModesEvent:
                    evRes = ID_EVENT.MC_SERVICE;
                    break;
                default:
                    break;
            }
        }

        public static void TranslateEvent (this EventArgs arg, ID_EVENT ev, out EVENT evRes)
        {
            evRes = EVENT.Unknown;

            switch (ev) {
                case ID_EVENT.GENOBJECT_MODIFIED:
                    evRes = EVENT.OnData53500Modified;
                    break;
                case ID_EVENT.NEW_PLAN_VALUES:
                case ID_EVENT.REQUEST_PLAN_VALUES:
                    evRes = EVENT.OnPlanDataChanged;
                    break;
                case ID_EVENT.RELOAD_PLAN_VALUES:
                case ID_EVENT.PHANTOM_RELOAD_PLAN_VALUES:
                    evRes = EVENT.OnMaket53500Changed;
                    break;
                case ID_EVENT.MC_SERVICE:
                    evRes = EVENT.OnModesEvent;
                    break;
                default:
                    break;
            }
        }

        public static ID_EVENT TranslateEvent (this EventArgs arg, EVENT ev)
        {
            ID_EVENT evRes = ID_EVENT.Unknown;

            arg.TranslateEvent (ev, out evRes);

            return evRes;
        }
    }

    /// <summary>
    /// Интерфейс аргумента события
    /// </summary>
    public interface IEventArgs
    {
        /// <summary>
        /// Идентификатор события, внутренний
        /// </summary>
        ID_EVENT m_id
        {
            get;
        }
        /// <summary>
        /// Целевая дата/время
        /// </summary>
        DateTime m_Date
        {
            get;
        }
        /// <summary>
        /// Целевой тип аргумента события, ~ от идентификатора события
        /// </summary>
        Type m_type
        {
            get;
        }
    }

    /// <summary>
    /// Аргумент события, полученного от Модес-Центра для помещения в очередь обработки
    /// </summary>
    /// <typeparam name="T">Тип целевого объекта в аргументе</typeparam>
    [Serializable]
    public class EventArgs<T> : System.EventArgs, IEventArgs
    {
        /// <summary>
        /// Идентификатор события, внутренний
        /// </summary>
        public ID_EVENT m_id
        {
            get;
        }
        /// <summary>
        /// Целевая дата/время
        /// </summary>
        public DateTime m_Date
        {
            get;
        }
        /// <summary>
        /// Целевой тип аргумента события, ~ от идентификатора события
        /// </summary>
        public Type m_type
        {
            get
            {
                return typeof (T);
            }
        }

        public ReadOnlyCollection<T> m_listParameters;

        public EventArgs (ID_EVENT id, DateTime date, ReadOnlyCollection<T> listParameters)
            : base ()
        {
            m_id = id;

            m_Date = date;

            m_listParameters = new ReadOnlyCollection<T> (listParameters);
        }
    }

    namespace ModesCentre
    {
        [Serializable]
        public enum ID_GEN_OBJECT_TYPE
        {
            GOU = 15, TEC = 1, RGE = 3
        }

        [Serializable]
        public enum Operation : short
        {
            Unknown = -1, InitIGO, MaketEquipment, PPBR
        }
    }
}