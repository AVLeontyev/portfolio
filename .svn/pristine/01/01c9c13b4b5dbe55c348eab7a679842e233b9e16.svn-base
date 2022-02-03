using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Statistic
{
    partial class PanelTecViewBase
    {
        //[Serializable]
        //public class LabelCustomTecViewProfile
        //{
        //    private LabelCustomTecViewProfile ()
        //    {
        //    }

        //    public LabelCustomTecViewProfile (FormChangeMode.KeyDevice key, LabelViewProperties properties)
        //        : this ()
        //    {
        //        Key = key;

        //        Properties = properties;
        //    }
        //}

        //[Serializable]
        //public class ListValue : List<LabelViewProperties.VALUE>
        //{
        //    public ListValue ()
        //        : base ()
        //    {
        //    }

        //    public ListValue (List<LabelViewProperties.VALUE> src)
        //        : base (src)
        //    {
        //    }
        //}

        [Serializable]
        public enum VALUE
        {
            DISABLED = -1, OFF, ON
        }

        public enum BANK_DEFAULT
        {
            DISABLED = 0
                , HOUR_TABLE_GRAPH
                , HOUR_TABLE_GRAPH_OPER
                , HOUR_TABLE_OPER
        }

        public static string ValueToString (VALUE value)
        {
            return value.ToString();
        }

        private static Dictionary<BANK_DEFAULT, List<VALUE>> s_defaultValues = new Dictionary<BANK_DEFAULT, List<VALUE>> () {
                { BANK_DEFAULT.DISABLED // все отключено
                    , new List<VALUE>() { VALUE.DISABLED // TABLE_MINS
                        , VALUE.DISABLED // TABLE_HOURS
                        , VALUE.DISABLED // GRAPH_MINS
                        , VALUE.DISABLED // GRAPH_HOURS
                        , VALUE.DISABLED // ORIENTATION
                        , VALUE.DISABLED // QUICK_PANEL
                        , VALUE.DISABLED // TABLE_AND_GRAPH
                    }
                }
                , { BANK_DEFAULT.HOUR_TABLE_GRAPH // отобразить часовые таблицу/гистограмму: 0, 1, 0, 1, 0, 0, -1
                    , new List<VALUE> { VALUE.OFF // TABLE_MINS
                        , VALUE.ON // TABLE_HOURS
                        , VALUE.OFF // GRAPH_MINS
                        , VALUE.ON // GRAPH_HOURS
                        , VALUE.OFF // ORIENTATION
                        , VALUE.ON // QUICK_PANEL
                        , VALUE.DISABLED // TABLE_AND_GRAPH
                    }
                }
                , { BANK_DEFAULT.HOUR_TABLE_GRAPH_OPER // отобразить часовые таблицу/гистограмму/панель с оперативными данными: 0, 1, 0, 1, 0, 1, -1
                    , new List<VALUE> { VALUE.OFF // TABLE_MINS
                        , VALUE.ON // TABLE_HOURS
                        , VALUE.OFF // GRAPH_MINS
                        , VALUE.ON // GRAPH_HOURS
                        , VALUE.OFF // ORIENTATION
                        , VALUE.ON // QUICK_PANEL
                        , VALUE.DISABLED // TABLE_AND_GRAPH
                    }
                }
                , { BANK_DEFAULT.HOUR_TABLE_OPER // отобразить часовые таблицу/панель с оперативными данными: 0, 1, 0, 0, -1, 1, -1
                    , new List<VALUE> { VALUE.OFF // TABLE_MINS
                        , VALUE.ON // TABLE_HOURS
                        , VALUE.OFF // GRAPH_MINS
                        , VALUE.OFF // GRAPH_HOURS
                        , VALUE.DISABLED // ORIENTATION
                        , VALUE.ON // QUICK_PANEL
                        , VALUE.DISABLED // TABLE_AND_GRAPH
                    }
                }
            };

        [Serializable]
        public class LabelViewProperties : List<VALUE>
        {
            public enum INDEX_PROPERTIES_VIEW { UNKNOWN = -1, TABLE_MINS, TABLE_HOURS, GRAPH_MINS, GRAPH_HOURS, ORIENTATION, QUICK_PANEL, TABLE_AND_GRAPH, COUNT_PROPERTIES_VIEW };

            //public List<VALUE> Values { get { return _values.ToList(); } } 

            /// <summary>
            /// Значение признака ориентации размещения таблиц, графиков
            /// </summary>
            public VALUE PreviousOrientation;

            public LabelViewProperties (List<VALUE> values)
            {
                Clear ();
                AddRange (values);

                PreviousOrientation = GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION);
            }

            public LabelViewProperties ()
                : this (BANK_DEFAULT.HOUR_TABLE_GRAPH)
            {
            }

            public LabelViewProperties (BANK_DEFAULT nameBank)
                : this (s_defaultValues [nameBank])
            {
            }

            public static string GetText (INDEX_PROPERTIES_VIEW indx)
            {
                string [] array = { @"Таблица(мин)", @"Таблица(час)", @"График(мин)", @"График(час)", @"Ориентация", @"Оперативные значения", @"Таблица+Гистограмма" };

                return array [(int)indx];
            }

            public VALUE GetValue (INDEX_PROPERTIES_VIEW indx)
            {
                return this [(int)indx];
            }

            public void SetValue (INDEX_PROPERTIES_VIEW indx, VALUE value)
            {
                this [(int)indx] = value;
            }

            /// <summary>
            /// Установить новое значение для свойства
            /// </summary>
            /// <param name="indx">Индекс свойства</param>
            /// <param name="newVal">Новое значение свойства</param>
            public void SetProperty (LabelViewProperties.INDEX_PROPERTIES_VIEW indx, VALUE newVal)
            {
                SetValue (indx, newVal);

                int cnt = 0;
                LabelViewProperties.INDEX_PROPERTIES_VIEW i = LabelViewProperties.INDEX_PROPERTIES_VIEW.UNKNOWN;
                // сколько таблиц/гистограмм отображается
                cnt = GetCountOn (0, LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_HOURS);

                if (cnt > 1) {
                    if (cnt > 2) {
                        //if (cnt > 3) {
                        if (indx < LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS)
                            //3-й установленный признак - таблица: снять с отображения графики
                            SetGraphOff ();
                        else
                            //3-й установленный признак - график: снять с отображения таблицы
                            SetTableOff ();
                        // был снят с отображения один из элементов - уменьшаем кол-во
                        cnt--; 
                        //} else ;
                    } else
                        ;

                    if (cnt > 1)
                        if (IsOrientationDisabled == true)
                            if (PreviousOrientation == VALUE.DISABLED)
                                //Вертикально - по умолчанию
                                SetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION
                                    , VALUE.OFF);
                            else
                                OrientationRecovery ();
                        else
                            ; //Оставить "как есть"
                    else
                        OrientationDisabled ();
                } else {
                    OrientationDisabled ();
                }
            }

            public void SetTableOff ()
            {
                SetValue (INDEX_PROPERTIES_VIEW.TABLE_MINS, VALUE.OFF); //Снять с отображения
                SetValue (INDEX_PROPERTIES_VIEW.TABLE_HOURS, VALUE.OFF); //Снять с отображения
            }

            public void SetGraphOff ()
            {
                SetValue (INDEX_PROPERTIES_VIEW.GRAPH_MINS, VALUE.OFF); //Снять с отображения
                SetValue (INDEX_PROPERTIES_VIEW.GRAPH_HOURS, VALUE.OFF); //Снять с отображения
            }

            public bool IsOrientationDisabled
            {
                get
                {
                    return this [(int)INDEX_PROPERTIES_VIEW.ORIENTATION] == VALUE.DISABLED;
                }
            }

            //public VALUE PreviousOrientation
            //{
            //    get
            //    {
            //        return PreviousOrientation;
            //    }
            //}

            /// <summary>
            /// Блокировать возможность выбора "ориентация сплиттера"
            /// </summary>
            public void OrientationDisabled ()
            {
                //Запомнить предыдущее стостояние "ориентация сплиттера"
                PreviousOrientation = this [(int)INDEX_PROPERTIES_VIEW.ORIENTATION];
                //Блокировать возможность выбора "ориентация сплиттера"
                this [(int)INDEX_PROPERTIES_VIEW.ORIENTATION] = VALUE.DISABLED;
            }

            /// <summary>
            /// Восстановить значение "ориентация сплиттера"
            /// </summary>
            public void OrientationRecovery ()
            {
                this [(int)INDEX_PROPERTIES_VIEW.ORIENTATION] = PreviousOrientation;
                
            }

            public void CopyTo (LabelViewProperties dest)
            {

            }

            public int Length
            {
                get
                {
                    return (int)INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW;
                }
            }

            public bool IsOn(INDEX_PROPERTIES_VIEW indx)
            {
                return this [(int)indx] == VALUE.ON;
            }

            public bool IsOff (INDEX_PROPERTIES_VIEW indx)
            {
                return this [(int)indx] == VALUE.OFF;
            }

            public int CountOn
            {
                get
                {
                    return this.Count (v => v == VALUE.ON);
                }
            }

            public int GetCountOn(INDEX_PROPERTIES_VIEW start, INDEX_PROPERTIES_VIEW end)
            {
                int iRes = 0;

                for(INDEX_PROPERTIES_VIEW indx = start; !(indx > end); indx ++)
                    iRes += this [(int)indx] == VALUE.ON ? 1 : 0;

                return iRes;
            }

            //public int [] ToArray ()
            //{
            //    return this.Select (v => (int)v).ToArray ();
            //}
        }
    }
}
