using ASUTP;
using ASUTP.Core;
using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Statistic
{
    partial class PanelAdminKomDisp
    {
        /// <summary>
        /// Обработчик события - изменение рассчтываемого значения
        /// </summary>
        /// <param name="obj">Объект - инициатор события</param>
        /// <param name="ev">Аргумент события с новым значением</param>
        private void dgvInitiativeAuxCalculate_ValueChanged (object obj, EventArgs ev)
        {
            EventValueChangedArgs arg = ev as EventValueChangedArgs;

            this.buttonApplyInitiativeAux.Enabled = Equals (arg, null) == false;
            this.buttonApplyInitiativeAux.Tag = arg;
        }

        private class DateTimeRange : ASUTP.Core.DateTimeRange
        {
            public DateTimeRange ()
                : base (HDateTime.ToMoscowTimeZone (), HDateTime.ToMoscowTimeZone ())
            {
                DateTime begin = Begin.AddSeconds (-Begin.Second).AddMilliseconds (-Begin.Millisecond);

                Set (begin
                    , begin.Date.AddHours (begin.Hour + 1));
            }

            /// <summary>
            /// Установить для обоих значений новую дату
            ///  ; при этом значения Hour, Minute - оставить прежние
            /// </summary>
            public void SetDate (DateTime newDate)
            {
                // все то же самое (часы, мин., сек., мсек.), но с другой базовой датой
                Set (newDate.Date.AddHours(Begin.Hour).AddMinutes(Begin.Minute).AddSeconds(Begin.Second).AddMilliseconds(Begin.Millisecond)
                    , newDate.Date.AddHours (End.Hour).AddMinutes (End.Minute).AddSeconds (End.Second).AddMilliseconds (End.Millisecond));
            }

            /// <summary>
            /// Проверить корректность дипазона: начало раньше окончания
            /// </summary>
            public bool IsValidate
            {
                get
                {
                    return (End - Begin).TotalMinutes > 0;
                }
            }
        }

        /// <summary>
        /// Интерфейс для хранения/расчета значений инициатив собственных
        /// </summary>
        public interface IInitiativeAux
        {
            /// <summary>
            /// Метка дфты/времени для рекомендации
            /// </summary>
            DateTime Stamp  { get; set; }

            /// <summary>
            /// Значение инициативы собственной(рекомендации)
            /// </summary>
            double Recomendation { get; set; }
        }

        /// <summary>
        /// Структура для хранения результата при передаче значений родительской панели
        /// </summary>
        public struct InitiativeAux : IInitiativeAux
        {
            public DateTime Stamp   { get; set; }

            public double Recomendation { get; set; }

            public InitiativeAux (IInitiativeAux src)
            {
                Stamp = src.Stamp;

                Recomendation = src.Recomendation;
            }
        }

        public class InitiativeAuxCalculate : IInitiativeAux, IDisposable
        {
            private int _index;

            private double _new_pbr;
            private double _PBR_0;

            private double _koef_current;
            private double _koef_new_pbr;

            private enum SIGN { Route, Ascent, CountStep, Cross }
            private HMark _markSign;

            /// <summary>
            /// Значения начало-окончание для текущего часа
            /// </summary>
            private double P1, P2;
            /// <summary>
            /// Начало набора-снижения нагрузки по команде
            /// </summary>
            private double P0;
            /// <summary>
            /// Окончание набора-снижения нагрузки в текущем часе (пересечение с границей часа)
            /// ; в случае окончания команды до завершения текщего часа = _new_pbr
            /// </summary>
            private double Pm;

            private ASUTP.Core.DateTimeRange _range;

            List<int> _hours;
            List<double> _hourDurations; // длительность (часы) каждого из интервалов в 'range.Hours'

            public DateTime Stamp   { get; set; }

            public double Recomendation { get; set; }

            public bool Ready
            {
                get
                {
                    return (Count > 0)
                        && (Count - _hourDurations.Count == 0)
                        && (Count - _index > 0);
                }
            }

            public int Count
            {
                get
                {
                    return _hours.Count;
                }
            }

            /// <summary>
            /// Возвратить площадь треугольника по 3-м сторонам (ф. Герона)
            /// </summary>
            /// <param name="a">Сторона 1</param>
            /// <param name="b">Сторона 2</param>
            /// <param name="c">Сторона 3</param>
            /// <returns>Площадь треугольника</returns>
            public double SquareTreangle (double a, double b, double c)
            {
                double dblRes = -1F;

                double p = 1F;

                p = (a + b + c) / 2;
                dblRes = Math.Sqrt (p * (p - a) * (p - b) * (p - c));

                return dblRes;
            }

            /// <summary>
            /// Возвратить площадь прямоугольного треугольника
            /// </summary>
            /// <param name="a">Катет 1</param>
            /// <param name="b">Катет 2</param>
            /// <returns>Площадь треугольника</returns>
            public double SquareTreangle (double a, double b)
            {
                return a * b / 2;
            }

            private double DurationCross
            {
                get
                {
                    return (Pm - P0) / _koef_current;
                }
            }

            /// <summary>
            /// Возвратить признак наличия пересечения
            /// </summary>
            private bool IsCross
            {
                get
                {
                    bool bRes = false;

                    double t = -1F; // время за которое нагрузка примет значение внешней команды без воздействия

                    if ((_markSign.IsMarked ((int)SIGN.Route) == true)
                        && (_markSign.IsMarked ((int)SIGN.Ascent) == true)
                        && (_markSign.IsMarked ((int)SIGN.CountStep) == true))
                        t = DurationCross;
                    else
                        ;
                    // д.б. положительным и произойти до начала следующего часа
                    bRes = (t > 0)
                        && t < (1 - _range.Begin.Minute / 60);

                    return bRes;
                }
            }

            private int IndexVariant
            {
                get
                {
                    int iRes = -1;

                    #region Возможные варианты
                    // 5 - набор-набор; 1; >
                    // 6 - набор-набор; 1; <
                    // 7 - набор-набор; 2; >
                    // 8 - набор-набор; 2; <
                    // 9 - набор-набор; 2; >; пересечение

                    // 10 - набор-снижение; 1
                    // 11 - набор-снижение; 2

                    // 12 - снижение-снижение; 1; <
                    // 13 - снижение-снижение; 1; >
                    // 14 - снижение-снижение; 2; <
                    // 15 - снижение-снижение; 2; >
                    // 16 - снижение-снижение; 2; <; пересечение

                    // 17 - снижение-набор; 1
                    // 18 - снижение-набор; 2
                    #endregion

                    if (_koef_current == 0)
                    // вар. №№1-4
                        if (_markSign.IsMarked ((int)SIGN.CountStep) == false)
                            iRes = 1; //??? 3; не важно
                        else if (_markSign.IsMarked ((int)SIGN.CountStep) == true)
                            iRes = 2; //??? 4; не важно
                        else
                            ;
                    else if ((_markSign.IsMarked ((int)SIGN.Route) == true)
                        && (_markSign.IsMarked ((int)SIGN.Ascent) == true)
                        && (_markSign.IsMarked ((int)SIGN.CountStep) == false)
                        && (_markSign.IsMarked ((int)SIGN.Cross) == false)) // всегда 'false'
                    // вар. №№5, 12
                        iRes = 5;
                    else if ((_markSign.IsMarked ((int)SIGN.Route) == true)
                        && (_markSign.IsMarked ((int)SIGN.Ascent) == false)
                        && (_markSign.IsMarked ((int)SIGN.CountStep) == false)
                        && (_markSign.IsMarked ((int)SIGN.Cross) == false)) // всегда 'false'
                    // вар. №№6, 13
                        iRes = 6;
                    else if ((_markSign.IsMarked ((int)SIGN.Route) == true)
                        && (_markSign.IsMarked ((int)SIGN.Ascent) == true)
                        && (_markSign.IsMarked ((int)SIGN.CountStep) == true)
                        && (_markSign.IsMarked ((int)SIGN.Cross) == false)) // вариант
                    // вар. №№7, 14
                        iRes = 7;
                    else if ((_markSign.IsMarked ((int)SIGN.Route) == true)
                        && (_markSign.IsMarked ((int)SIGN.Ascent) == false)
                        && (_markSign.IsMarked ((int)SIGN.CountStep) == true)
                        && (_markSign.IsMarked ((int)SIGN.Cross) == false)) // всегда 'false'
                    // вар. №№8, 15
                        iRes = 8;
                    else if ((_markSign.IsMarked ((int)SIGN.Route) == true)
                        && (_markSign.IsMarked ((int)SIGN.Ascent) == true)
                        && (_markSign.IsMarked ((int)SIGN.CountStep) == true)
                        && (_markSign.IsMarked ((int)SIGN.Cross) == true)) // вариант
                    // вар. №№9, 16
                        iRes = 9;
                    else if ((_markSign.IsMarked ((int)SIGN.Route) == false)
                        //&& (_markSign.IsMarked ((int)SIGN.Koef) == false) не важно
                        && (_markSign.IsMarked ((int)SIGN.CountStep) == false)
                        && (_markSign.IsMarked ((int)SIGN.Cross) == false)) // всегда 'false'
                    // вар. №№10, 17
                        iRes = 10;
                    else if ((_markSign.IsMarked ((int)SIGN.Route) == false)
                        //&& (_markSign.IsMarked ((int)SIGN.Koef) == false) не важно
                        && (_markSign.IsMarked ((int)SIGN.CountStep) == true)
                        && (_markSign.IsMarked ((int)SIGN.Cross) == false)) // всегда 'false'
                    // вар. №№11, 18
                        iRes = 11;
                    else;

                    // добавить 7(кол-во вариантов для набора или снижения)
                    iRes += (_koef_current < 0) ? 7 : 0;

                    return iRes;
                }
            }

            public InitiativeAuxCalculate (HAdmin.RDGStruct []values, double new_pbr, double pbr_0, ASUTP.Core.DateTimeRange range)
            {
                _index = 0;

                int iHour = -1
                    , iDuration = -1; // при "iDuration > 0" команда завершается до начала следующего часа, т.е. кол-во шагов = 2
                double dx = -1F 
                    , sTreangle = -1F
                    , sQuadrate = -1F; 

                _new_pbr = new_pbr;
                _PBR_0 = pbr_0;

                _markSign = new HMark (0);

                // номера часов в течение которых выполняется команда
                _hours = range.Hours.ToList ();

                // найти длительности всех интервалов в диапазоне: 1-ый и крайний могут быть не полными (60 мин), в середине всегда полные
                _hourDurations = range.GetHourDurations ().ToList ();

                if (Ready == true) {
                    _range = range;

                    // для текущего часа (индекс = 0)
                    iHour = _hours [_index];
                    iDuration = (60 - range.Begin.Minute) - (int)(range.End - range.Begin).TotalMinutes;

                    Stamp = range.Begin.Date.AddHours (iHour + 1);

                    P1 = iHour > 0 ? values [iHour - 1].pbr : _PBR_0;
                    P2 = values [iHour].pbr;

                    _koef_current = P2 - P1;
                    // начальное знач. с которого производится набор/снижение нагрузки по команде
                    P0 = _koef_current * (range.Begin.Minute / 60) + P1;

                    _koef_new_pbr = (_new_pbr - P0) / (range.End - range.Begin).TotalHours;
                    //??? команда может завершиться до начала следующего часа
                    Pm = _koef_new_pbr * _hourDurations [_index] + P0;

                    _markSign.Set ((int)SIGN.Route, ((_koef_current < 0) && (_koef_new_pbr < 0))
                        || ((_koef_current > 0) && (_koef_new_pbr > 0)));
                    _markSign.Set ((int)SIGN.Ascent, Math.Abs(_koef_new_pbr) > Math.Abs (_koef_current));
                    _markSign.Set ((int)SIGN.CountStep, iDuration > 0);
                    _markSign.Set ((int)SIGN.Cross, IsCross);

                    switch (IndexVariant) {
                        #region Обычный (непереходной) час
                        case 1:
                        case 3:
                            //; площадь прямоугольного треугольника
                            Recomendation = SquareTreangle (_hourDurations [_index], Pm - P0);
                            // вар. №№1, 3; оставить только площадь треугольника
                            break;
                        case 2:
                        case 4:
                            //; площадь прямоугольного треугольника
                            Recomendation = SquareTreangle (_hourDurations [_index], Pm - P0);
                            // вар. №№2, 4; площадь прямоугольного треугольника + прямоугольник(квадрат)
                            Recomendation += iDuration * (_new_pbr - P0);
                            break;
                        #endregion

                        #region Переходной час (1 шаг)
                        case 5: // набор-набор; 1; >
                        case 13: // снижение-снижение; 1; >
                        case 17: // снижение-набор; 1
                            // площадь треугольника [+]
                            Recomendation = SquareTreangle (Math.Sqrt (Math.Pow (Pm - P0, 2) + Math.Pow (_hourDurations [_index], 2))
                                , Math.Sqrt (Math.Pow (P2 - P0, 2) + Math.Pow (_hourDurations [_index], 2))
                                , Math.Abs (P2 - Pm));
                            break;
                        case 6: // набор-набор; 1; <
                        case 12: // снижение-снижение; 1; <
                        case 10: // набор-снижение; 1
                            // площадь треугольника [-]
                            Recomendation = -1 * SquareTreangle (Math.Sqrt (Math.Pow (Pm - P0, 2) + Math.Pow (_hourDurations [_index], 2))
                                , Math.Sqrt (Math.Pow (P2 - P0, 2) + Math.Pow (_hourDurations [_index], 2))
                                , Math.Abs (P2 - Pm));
                            break;
                        #endregion

                        #region Переходной час (2 шага, без пересечения)
                        case 8: // набор-набор; 2; <
                        case 11: // набор-снижение; 2
                        case 14: // снижение-снижение; 2; <
                            // площадь треугольника [-]
                            Recomendation = -1 * SquareTreangle (Math.Sqrt (Math.Pow (Pm - P0, 2) + Math.Pow (_hourDurations [_index], 2))
                                , Math.Sqrt (Math.Pow (P2 - P0, 2) + Math.Pow (1 - _range.Begin.Minute / 60, 2))
                                , Math.Sqrt (Math.Pow (P2 - Pm, 2) + Math.Pow (1 - _range.End.Minute / 60, 2)));
                            // дополнительно площадь треугольника
                            Recomendation -= SquareTreangle(1 - _range.End.Minute / 60, Math.Abs (P2 - Pm));
                            break;
                        case 7: // набор-набор; 2; >
                        case 15: // снижение-снижение; 2; >
                        case 18: // снижение-набор; 2
                            // площадь треугольника [+]
                            Recomendation = SquareTreangle (Math.Sqrt (Math.Pow (Pm - P0, 2) + Math.Pow (_hourDurations [_index], 2))
                                , Math.Sqrt (Math.Pow (P2 - P0, 2) + Math.Pow (1 - _range.Begin.Minute / 60, 2))
                                , Math.Sqrt (Math.Pow (P2 - Pm, 2) + Math.Pow (1 - _range.End.Minute / 60, 2)));
                            // дополнительно площадь прямоугольного треугольника
                            Recomendation += SquareTreangle (1 - _range.End.Minute / 60, Math.Abs (P2 - Pm));
                            break;
                        #endregion

                        #region Переходной час (2 шага, с пересечением)
                        case 9: // набор-набор; 2; >; пересечение
                            // площадь треугольника [+]
                            Recomendation = SquareTreangle (Math.Sqrt (Math.Pow (Pm - P0, 2) + Math.Pow (_hourDurations [_index], 2))
                                , Math.Sqrt (0 + Math.Pow (DurationCross - _hourDurations[_index], 2))
                                , Math.Sqrt (Math.Pow (Pm - P0, 2) + Math.Pow (DurationCross, 2)));
                            // дополнительно [-] площадь прямоугольного треугольника
                            Recomendation -= SquareTreangle (1 - _range.Begin.Minute / 60 - DurationCross, Math.Abs (P2 - Pm));
                            break;
                        case 16: // снижение-снижение; 2; <; пересечение
                            // площадь треугольника [-]
                            Recomendation = -1 * SquareTreangle (Math.Sqrt (Math.Pow (Pm - P0, 2) + Math.Pow (_hourDurations [_index], 2))
                                , Math.Sqrt (0 + Math.Pow (DurationCross - _hourDurations [_index], 2))
                                , Math.Sqrt (Math.Pow (Pm - P0, 2) + Math.Pow (DurationCross, 2)));
                            // дополнительно [+] площадь прямоугольного треугольника
                            Recomendation += SquareTreangle (1 - _range.Begin.Minute / 60 - DurationCross, Math.Abs (P2 - Pm));
                            break;
                        #endregion

                        default:
                            break;
                    }
                } else
                    fail();
            }

            public void Next ()
            {
                int iHour = -1;

                _index++;

                if (Ready == true) {
                    iHour = _hours [_index];
                    Stamp = _range.Begin.Date.AddHours (iHour + 1);

                    // hourDurations [i] всегда == 1; на "2" разделить т.к. площадь прямоугольного треугольника
                    Recomendation = _hourDurations [_index] * (Pm - _new_pbr) / 2;
                } else
                    fail ();
            }

            public int IndexHourEnd
            {
                get
                {
                    return (Ready == false) && (_index > 0)
                        ? _hours [_index - 1]
                            : -1;
                }
            }

            private void fail ()
            {
                Stamp = DateTime.MaxValue;
                Recomendation = double.NegativeInfinity;
            }

            public void Dispose ()
            {
            }
        }

        /// <summary>
        /// Аргумент для события 'ValueChanged' для оповещения родительского элемента управления
        /// </summary>
        public class EventValueChangedArgs : EventArgs
        {
            private EventValueChangedArgs ()
                : base ()
            {
                Value = new List<InitiativeAux> ();
            }

            public EventValueChangedArgs (List<InitiativeAux> values)
                : this ()
            {
                values.ForEach (v => Value.Add(new InitiativeAux () { Stamp = v.Stamp, Recomendation = v.Recomendation }));
            }
            /// <summary>
            /// Текущие(новые) значения
            /// </summary>
            public List<InitiativeAux> Value;
        }

        /// <summary>
        /// Элемент графического интерфейса - представление для расчета значений инициатив собственных на интервале 1 ч
        ///  и отображения исходных для расчета данных
        /// </summary>
        private class DataGridViewInitiativeAuxCalculate : HDataGridViewTables
        {
            private enum Error
            {
                PBRValue = -3
                , DatetimeRange = -2
                , DatetimeDeny
                , Ok
            }

            /// <summary>
            /// Индекс свойства строки
            ///  , для индексирования элементов массива свойств 'RowProperties.Mark'
            /// </summary>
            private enum INDEX_ROW_PROPERTY
            {
                REQUIRED
                , TURN
                , EDIT
            }

            /// <summary>
            /// Свойства строки в представлении (значения по умолчанию) для их установки при инициализации
            /// </summary>
            private struct RowProperties
            {
                /// <summary>
                /// Совокупность признаков для устновки свойств строки-ячейки
                ///  ??? проще установить [Flags] для 'enum'
                /// </summary>
                public HMark Mark;

                /// <summary>
                /// Заголовок строки
                /// </summary>
                public string Text;
            }

            /// <summary>
            /// Индекс(идентификтор, тег) строк в представлении
            /// </summary>
            private enum INDEX_ROW
            {
                TIME_BEGIN, TIME_END
                , PBR, PBRmin, PBRmax, UDGe, newPBR
                , VALUE
                    , COUNT
            }

            private Error _state;

            private Error m_State
            {
                get
                {
                    return _state;
                }

                set
                {
                    INDEX_CELL_STYLE indxCellStyle;

                    if (!(_state == value)) {
                        switch (value) {
                            case Error.Ok:
                                indxCellStyle = INDEX_CELL_STYLE.COMMON;
                                break;
                            case Error.DatetimeRange:
                                indxCellStyle = INDEX_CELL_STYLE.ERROR;
                                break;
                            case Error.DatetimeDeny:
                                indxCellStyle = INDEX_CELL_STYLE.WARNING;
                                break;
                            default:
                                indxCellStyle = INDEX_CELL_STYLE.ERROR;
                                break;
                        }

                        Rows [(int)INDEX_ROW.TIME_BEGIN].Cells [0].Style.BackColor =
                        Rows [(int)INDEX_ROW.TIME_END].Cells [0].Style.BackColor =
                            s_dgvCellStyles [(int)indxCellStyle].BackColor;

                        _state = value;
                    } else
                        ;

                    newPBRReadOnly = !(m_State == Error.Ok);
                }
            }

            private DateTimeRange m_datetimeRange;

            private string _cellValueEmpty = new string ('-', 3);

            /// <summary>
            /// Массив значений по умолчанию для строк при инициализации
            /// </summary>
            private RowProperties [] _arRowPropertiesdefault;
            ///// <summary>
            ///// Значение для строки (тег: MINUTE) по умолчанию
            ///// </summary>
            //private string _cellMinuteValueDefault;
            /// <summary>
            /// Строка для форматирования/отображения нередактируемых значений (ПБР, ПБРмин, ПБРмакс)
            /// </summary>
            private string _formatValue;
            /// <summary>
            /// ПБР-значения для указанной ГТП за дату
            /// </summary>
            private HAdmin.RDGStruct [] _values;
            /// <summary>
            /// Делегат получения текущего часа
            ///  , в ~ от времени сервера
            /// </summary>
            private Func<DateTime, CONN_SETT_TYPE, int> delegateGetCurrentHour;
            /// <summary>
            /// Редактирование начало/окончание команды генерации
            /// </summary>
            private DateTimePicker m_datetimePicker;
            /// <summary>
            /// Значение ПБР при переходе через границу суток
            ///  , для определения УДГэ 1-го часа в сутках
            ///  по аналогии с 'DataGridViewKomDisp'
            /// </summary>
            public double m_PBR_0;
            /// <summary>
            /// Событие для оповещения родительской панели об изменении целевого значения
            /// </summary>
            public event EventHandler ValueChanged;

            /// <summary>
            /// Конструктор - основной (с аргументами)
            /// </summary>
            /// <param name="foreColor">Цвет текста для элемента управления</param>
            /// <param name="backgroundColors">Цвет фона</param>
            public DataGridViewInitiativeAuxCalculate (Color foreColor, Color [] backgroundColors)
                : base (foreColor, backgroundColors, false)
            {
                HMark markReqTurnOnEdit
                    , markReqTurnOnNotEdit
                    , markReqTurnOffNotEdit
                    , markNotReqTurnOnNotEdit;

                markReqTurnOnEdit = new HMark (0); markReqTurnOnEdit.Marked ((int)INDEX_ROW_PROPERTY.REQUIRED); markReqTurnOnEdit.Marked ((int)INDEX_ROW_PROPERTY.TURN); markReqTurnOnEdit.Marked ((int)INDEX_ROW_PROPERTY.EDIT);
                markReqTurnOnNotEdit = new HMark (0); markReqTurnOnNotEdit.Marked ((int)INDEX_ROW_PROPERTY.REQUIRED); markReqTurnOnNotEdit.Marked ((int)INDEX_ROW_PROPERTY.TURN);
                markReqTurnOffNotEdit = new HMark (0); markReqTurnOffNotEdit.Marked ((int)INDEX_ROW_PROPERTY.REQUIRED);
                markNotReqTurnOnNotEdit = new HMark (0); markNotReqTurnOnNotEdit.Marked ((int)INDEX_ROW_PROPERTY.TURN);
                
                // в соответствии с 'INDEX_ROW'
                _arRowPropertiesdefault = new RowProperties [] {
                    new RowProperties { Text = "Начало", Mark = new HMark(markReqTurnOnEdit.Value) }
                    , new RowProperties { Text = "Оконч.", Mark = new HMark(markReqTurnOnEdit.Value) }
                    , new RowProperties { Text = "ПБР", Mark = new HMark(markReqTurnOnNotEdit.Value) }
                    , new RowProperties { Text = "ПБРмин", Mark = new HMark(0) }
                    , new RowProperties { Text = "ПБРмакс", Mark = new HMark(0) }
                    , new RowProperties { Text = "УДГэ", Mark = new HMark(markReqTurnOnNotEdit.Value) } // markReqTurnOffNotEdit
                    , new RowProperties { Text = "Команда", Mark = new HMark(markReqTurnOnEdit.Value) }
                    , new RowProperties { Text = "Знач.", Mark = new HMark(markReqTurnOnNotEdit.Value) }
                };

                m_datetimeRange = new DateTimeRange();

                InitializeComponents ();

                DataError += onDataError;
                SizeChanged += onSizeChanged;
                CellValidating += onCellValidating;
                EditingControlShowing += onEditingControlShowing;
                CellEnter += onCellEnter;
                CellLeave += onCellLeave;
            }

            /// <summary>
            /// Метод инициализации свойств, дочерних элементов, вызывается из конструктора
            /// </summary>
            private void InitializeComponents ()
            {
                DataGridViewCell cell;

                this.ContextMenuStrip = new ContextMenuStrip ();
                this.ContextMenuStrip.ForeColor = ForeColor;
                this.ContextMenuStrip.BackColor = BackColor;

                MultiSelect = false;
                //SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                //ReadOnly = true;
                AllowUserToResizeRows = false;
                AllowUserToResizeColumns = false;
                AllowUserToDeleteRows = false;
                AllowUserToAddRows = false;

                ColumnHeadersVisible = false;
                RowHeadersVisible = true;

                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                Columns.Add (string.Empty, string.Empty);

                Enum.GetValues (typeof (INDEX_ROW)).Cast<INDEX_ROW> ().ToList ().ForEach (indx => {
                    try {
                        if (indx < INDEX_ROW.COUNT) {
                            switch (indx) {
                                default:
                                    cell = new DataGridViewTextBoxCell ();
                                    break;
                            }

                            Rows.Add ();
                            Rows [RowCount - 1].Cells [0] = cell;
                            Rows [RowCount - 1].Cells [0].ReadOnly = !_arRowPropertiesdefault [(int)indx].Mark.IsMarked ((int)INDEX_ROW_PROPERTY.EDIT);
                            Rows [RowCount - 1].Tag = indx;
                            Rows [RowCount - 1].Visible = _arRowPropertiesdefault [(int)indx].Mark.IsMarked ((int)INDEX_ROW_PROPERTY.TURN);
                            Rows [RowCount - 1].HeaderCell.Value = _arRowPropertiesdefault [(int)indx].Text;

                            this.ContextMenuStrip.Items.Add (_arRowPropertiesdefault [(int)indx].Text);
                            this.ContextMenuStrip.Items [(int)this.ContextMenuStrip.Items.Count - 1].Enabled = !_arRowPropertiesdefault [(int)indx].Mark.IsMarked ((int)INDEX_ROW_PROPERTY.REQUIRED);
                            this.ContextMenuStrip.Items [(int)this.ContextMenuStrip.Items.Count - 1].Tag = indx;
                            ((ToolStripMenuItem)this.ContextMenuStrip.Items [(int)this.ContextMenuStrip.Items.Count - 1]).CheckState = _arRowPropertiesdefault [(int)indx].Mark.IsMarked ((int)INDEX_ROW_PROPERTY.TURN) == true
                                ? CheckState.Checked
                                    : CheckState.Unchecked;
                            ((ToolStripMenuItem)this.ContextMenuStrip.Items [(int)this.ContextMenuStrip.Items.Count - 1]).CheckOnClick = true;
                            ((ToolStripMenuItem)this.ContextMenuStrip.Items [(int)this.ContextMenuStrip.Items.Count - 1]).CheckStateChanged += contextMenuStrip_CheckStateChanged;
                        } else
                            ;
                    } catch (Exception e) {
                        Logging.Logg ().Exception (e, "PanelAdminKomDisp.DataGridViewIniativeAuxCalculate::InitializeComponents () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                });

                Columns [0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            #region Цветовая схема
            public override Color BackColor
            {
                get
                {
                    return base.BackColor;
                }

                set
                {
                    base.BackColor = value == SystemColors.Control ? SystemColors.Window : value;

                    //DefaultCellStyle.BackColor =
                    if (Equals (ContextMenuStrip, null) == false)
                        ContextMenuStrip.BackColor =
                            value == SystemColors.Control ? SystemColors.Window : value;
                    else
                        ;
                }
            }

            public override Color ForeColor
            {
                get
                {
                    return base.ForeColor;
                }

                set
                {
                    base.ForeColor = value;

                    if (Equals (ContextMenuStrip, null) == false)
                        ContextMenuStrip.ForeColor = value;
                    else
                        ;
                }
            }
            #endregion

            /// <summary>
            /// Установить значение "только для чтения" для строки "новое значение ПБР"
            ///  , т.к. за уже прошедшие часы инициативы собственные не могут быть установлены/измнены
            /// </summary>
            private bool newPBRReadOnly
            {
                set
                {
                    Rows [(int)INDEX_ROW.newPBR].ReadOnly = value;
                }
            }

            /// <summary>
            /// Установить значение "только для чтения" для строки "минута в часе для установки нового ПБР и инициативы"
            /// </summary>
            private bool timeReadOnly
            {
                set
                {
                    Rows [(int)INDEX_ROW.TIME_BEGIN].ReadOnly =
                    Rows [(int)INDEX_ROW.TIME_END].ReadOnly =
                        value;
                }
            }

            private void onCellEnter (object sender, DataGridViewCellEventArgs e)
            {
                Rectangle displayRect;

                if (ColumnCount > 0)
                    switch ((INDEX_ROW)e.RowIndex) {
                        case INDEX_ROW.TIME_BEGIN:
                        case INDEX_ROW.TIME_END:
                            if (Equals (m_datetimePicker, null) == true) {
                                //Initialized a new DateTimePicker Control  
                                m_datetimePicker = new DateTimePicker ();

                                // Setting the format (i.e. 2014-10-10)  
                                m_datetimePicker.Format = DateTimePickerFormat.Custom;
                                m_datetimePicker.CustomFormat = "HH:mm";
                                m_datetimePicker.ShowUpDown = true;
                                //??? alignment
                                m_datetimePicker.Visible = false;

                                //Adding DateTimePicker control into DataGridView   
                                Controls.Add (m_datetimePicker);

                                // An event attached to dateTimePicker Control which is fired when any date is selected
                                m_datetimePicker.ValueChanged += datetimePicker_ValueChanged;
                            } else
                                ;

                            // It returns the retangular area that represents the Display area for a cell  
                            displayRect = GetCellDisplayRectangle (e.ColumnIndex, e.RowIndex, true);

                            //Setting area for DateTimePicker Control  
                            m_datetimePicker.Size = new Size (displayRect.Width, displayRect.Height);

                            // Setting Location  
                            m_datetimePicker.Location = new Point (displayRect.X, displayRect.Y);

                            // Now make it visible  
                            m_datetimePicker.Visible = true;

                            if ((INDEX_ROW)e.RowIndex == INDEX_ROW.TIME_BEGIN)
                                m_datetimePicker.Value = m_datetimeRange.Begin;
                            else if ((INDEX_ROW)e.RowIndex == INDEX_ROW.TIME_END)
                                m_datetimePicker.Value = m_datetimeRange.End;
                            else
                                ;
                            break;
                        default:
                            break;
                    }
                else
                    ;
            }

            /// <summary>
            /// Обработчик события - изменение значения в элементе управления "Календарь"
            /// </summary>
            /// <param name="sender">Иницитор события</param>
            /// <param name="e">Аргумент события</param>
            private void datetimePicker_ValueChanged (object sender, EventArgs e)
            {
                INDEX_ROW indxRowSelected = INDEX_ROW.COUNT;

                if (SelectedCells.Count > 0)
                    if (SelectedCells.Count == 1) {
                        indxRowSelected = (INDEX_ROW)SelectedCells [0].RowIndex;
                        if (indxRowSelected == INDEX_ROW.TIME_BEGIN) {
                            m_datetimeRange.Set (m_datetimePicker.Value, m_datetimeRange.End);
                        } else if (indxRowSelected == INDEX_ROW.TIME_END) {
                            m_datetimeRange.Set (m_datetimeRange.Begin, m_datetimePicker.Value);
                        } else
                            ;

                        m_State = m_datetimeRange.IsValidate == true
                            ? !(m_datetimeRange.Begin.Hour < delegateGetCurrentHour(m_datetimeRange.Begin.Date, CONN_SETT_TYPE.ADMIN))
                                ? Error.Ok
                                    : Error.DatetimeDeny
                                        : Error.DatetimeRange;

                        fill ();
                    } else
                        ;
                else
                    ;
            }

            /// <summary>
            /// Обработчик события - потеря ячейкой фокуса ввода
            /// </summary>
            /// <param name="sender">Иницитор события (??? представление)</param>
            /// <param name="e">Аргумент события</param>
            private void onCellLeave (object sender, DataGridViewCellEventArgs e)
            {
                switch ((INDEX_ROW)e.RowIndex) {
                    case INDEX_ROW.TIME_BEGIN:
                    case INDEX_ROW.TIME_END:
                        Rows [e.RowIndex].Cells [0].Value = time_format ((INDEX_ROW)e.RowIndex);

                        // Now make it unvisible  
                        m_datetimePicker.Visible = false;
                        break;
                    default:
                        break;
                }
            }

            /// <summary>
            /// Обработчик события - активация пользователем механизма редактирования ячейки
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="e">Аргумент события</param>
            private void onEditingControlShowing (object sender, DataGridViewEditingControlShowingEventArgs e)
            {
                ComboBox cbx;
                TextBox tbx;

                /*if (e.Control is ComboBox) {
                    cbx = e.Control as ComboBox;
                    cbx.SelectedIndexChanged -= cellComboBox_SelectionChanged;
                    cbx.SelectedIndexChanged += cellComboBox_SelectionChanged;
                } else*/ if (e.Control is TextBox) {
                    tbx = e.Control as TextBox;
                    tbx.TextChanged -= cellTextBox_TextChanged;
                    tbx.TextChanged += cellTextBox_TextChanged;
                } else
                    ;
            }

            /// <summary>
            /// Обработчик события - измнение значения в ячейке, интегрированный элемент управления которой - поле ввода
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие(поле ввода)</param>
            /// <param name="e">Аргумент события</param>
            private void cellTextBox_TextChanged (object sender, EventArgs e)
            {
                if (onNewPbrTextChanged ((sender as TextBox).Text.Trim ()) == false) {
                    (sender as TextBox).SelectAll ();

                    ValueChanged?.Invoke (this, EventArgs.Empty);
                } else
                    ;
            }

            /// <summary>
            /// Проверить допустимость введенного пользователем нового значения
            /// </summary>
            /// <param name="newPbrText">Новое значение</param>
            /// <returns></returns>
            private bool onNewPbrTextChanged (string newPbrText)
            {
                bool bRes = false;

                double newPbr = -1F;
                List<InitiativeAux> listRec;

                try {
                    if ((string.IsNullOrEmpty (newPbrText) == false)
                        && (newPbrText.Equals(_cellValueEmpty) == false)
                        && (double.TryParse (newPbrText, out newPbr) == true)) {
                        listRec = _values.CalculateInitiativeAux (m_PBR_0, m_datetimeRange, newPbr).ToList ();

                        Rows [(int)INDEX_ROW.VALUE].Cells [0].Value = value_format (from rec in listRec select rec.Recomendation);

                        if (listRec.Count (value => {
                            return !(value.Recomendation == 0);
                        }) > 0)
                        // только если хотя бы одна инициатива отлична от "0"
                            ValueChanged?.Invoke (this, new EventValueChangedArgs (listRec));
                        else
                            ;

                        bRes = true;
                    } else {
                        Rows [(int)INDEX_ROW.VALUE].Cells [0].Value = 0.ToString (_formatValue);
                    }
                } catch (Exception ex) {
                    Logging.Logg ().Exception (ex, $@"DataGridViewInitiativeAuxCalculate::cellTextBox_TextChanged () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                } finally {
                }

                return bRes;
            }

            /// <summary>
            /// Обработчик события - изменение размера, для регулирования ширины столбцов
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="e">Аргумент события</param>
            private void onSizeChanged (object sender, EventArgs e)
            {
                RowHeadersWidth = 4 * Width / 8;
            }

            /// <summary>
            /// Обработчик события - ошибка при размещения значения в ячейке представления
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие(this)</param>
            /// <param name="e">Аргумент события</param>
            private void onDataError (object sender, DataGridViewDataErrorEventArgs e)
            {
                Logging.Logg ().Error ($@"PanelAdminKomDisp.DataGridViewIniativeAuxCalculate::DataError [Column={e.ColumnIndex}, Row={e.RowIndex}]{Environment.NewLine}"
                        + $@"Исключение: {e.Exception.Message}..."
                    , Logging.INDEX_MESSAGE.NOT_SET);
                e.Cancel = true;
                e.ThrowException = false;
            }

            /// <summary>
            /// Обработчик события - изменение состояния пункта контекстного меню
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="e">Аргумент события</param>
            private void contextMenuStrip_CheckStateChanged (object sender, EventArgs e)
            {
                DataGridViewRow row;

                row = Rows.Cast<DataGridViewRow> ().FirstOrDefault (r => {
                    return (INDEX_ROW)r.Tag == (INDEX_ROW)(sender as ToolStripMenuItem).Tag;
                });

                if (Equals (row, null) == false)
                    row.Visible = (sender as ToolStripMenuItem).CheckState == CheckState.Checked;
                else
                    ;
            }

            /// <summary>
            /// Обработчик события - начало проверки значения перед его размещением в ячейке
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="e">Аргумент события</param>
            private void onCellValidating (object sender, DataGridViewCellValidatingEventArgs e)
            {
                switch ((INDEX_ROW)e.RowIndex) {
                    case INDEX_ROW.newPBR:
                        break;
                    case INDEX_ROW.PBR:
                        m_State = Error.PBRValue;
                        break;
                    default:
                        // не реагировать - изменение автоматически
                        break;
                }
            }

            private string time_format (INDEX_ROW indx)
            {
                string strRes = "--:--";

                DateTime stamp;

                if (indx == INDEX_ROW.TIME_BEGIN) {
                    stamp = m_datetimeRange.Begin;
                } else if (indx == INDEX_ROW.TIME_END) {
                    stamp = m_datetimeRange.End;
                } else
                    throw new Exception($@"DateGridViewInitiativeAuxCalculate::time_format (INDEX_ROW={indx}) - некорректный индекс строки...");

                return string.Format ("{0:00}:{1:00}", stamp.Hour, stamp.Minute);
            }

            private string value_format (IEnumerable<double> values)
            {
                string delimeter = ";";

                return
                    //string.Format ("[{0}]", string.Join (delimeter, (from v in values select v.ToString (_formatValue)).ToArray ()))
                    $"[{string.Join (delimeter, (from v in values select v.ToString (_formatValue)).ToArray ())}]"
                    ;
            }

            /// <summary>
            /// Очистить значения перед размещением в представлении новых значений
            ///  ; после смены даты, ГТП на родительской панели
            /// </summary>
            private void clear ()
            {
                Rows [(int)INDEX_ROW.newPBR].Cells [0].Value = _cellValueEmpty;
            }

            /// <summary>
            /// Заполнить значениями ячейки предстваления
            /// </summary>
            private void fill ()
            {
                string newPbrText = string.Empty;
                int iHour = -1;
                List<int> hours;

                if (!(m_State == Error.DatetimeRange)) {
                    hours = m_datetimeRange.Hours.ToList();
                    iHour = m_datetimeRange.Begin.Hour;

                    try {
                        Rows [(int)INDEX_ROW.TIME_BEGIN].Cells [0].Value = time_format (INDEX_ROW.TIME_BEGIN);
                        Rows [(int)INDEX_ROW.TIME_END].Cells [0].Value = time_format (INDEX_ROW.TIME_END);
                        Rows [(int)INDEX_ROW.PBR].Cells [0].Value = value_format (from h in hours select _values [h].pbr);
                        Rows [(int)INDEX_ROW.PBRmin].Cells [0].Value = value_format (from h in hours select _values [h].pmin);
                        Rows [(int)INDEX_ROW.PBRmax].Cells [0].Value = value_format (from h in hours select _values [h].pmax);
                        Rows [(int)INDEX_ROW.UDGe].Cells [0].Value = value_format(from h in hours select _values.UDGe (m_PBR_0, h));

                        if (onNewPbrTextChanged (Rows [(int)INDEX_ROW.newPBR].Cells [0].Value.ToString ().Trim ()) == false)
                            Rows [(int)INDEX_ROW.VALUE].Cells [0].Value = value_format (from h in hours select _values [h].recomendation);
                        else
                            ;
                    } catch (Exception e) {
                        Logging.Logg ().Exception (e, "PanelAdminKomDisp.DataGridViewIniativeAuxCalculate::Fill () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                } else {
                    Rows [(int)INDEX_ROW.PBR].Cells [0].Value =
                    Rows [(int)INDEX_ROW.PBRmin].Cells [0].Value =
                    Rows [(int)INDEX_ROW.PBRmax].Cells [0].Value =
                    Rows [(int)INDEX_ROW.UDGe].Cells [0].Value =
                    Rows [(int)INDEX_ROW.newPBR].Cells [0].Value =
                    Rows [(int)INDEX_ROW.VALUE].Cells [0].Value =
                        _cellValueEmpty;
                }
            }

            /// <summary>
            /// Заполнить значениями ячейки предстваления
            /// </summary>
            /// <param name="date">Дата, указанная пользователем на родительской панели</param>
            /// <param name="values">Значения за дату для ГТП</param>
            /// <param name="fGetCurrentHour">Метод получения текущего часа</param>
            /// <param name="exact">Точность(кол-во знаков после запятой) при отображении</param>
            public void Fill (DateTime date, HAdmin.RDGStruct [] values, Func<DateTime, CONN_SETT_TYPE, int> fGetCurrentHour, int exact = 2)
            {
                clear ();

                m_datetimeRange.SetDate(date.Date);
                _values = new HAdmin.RDGStruct [values.Length];
                for (int i = 0; i < values.Length; i++) {
                    _values [i] = new HAdmin.RDGStruct ();
                    _values [i].From (values [i]);
                }

                delegateGetCurrentHour = fGetCurrentHour;
                _formatValue = $"F{exact}";

                if (InvokeRequired == true)
                    BeginInvoke ((MethodInvoker)delegate () {
                        fill ();
                    });
                else
                    fill ();
            }
        }
    }

    public static partial class RDGStructExtensions
    {
        /// <summary>
        /// Расчет значения инициативы собственной на указанном интервале
        /// </summary>
        /// <param name="values">This</param>
        /// <param name="PBR_0">ПБР-значение на границе с предыдущими сутками</param>
        /// <param name="iHour">Номер(индекс) часа на который требуется расчет инициативы собственной</param>
        /// <param name="newPbr">Новое ПБР-значение</param>
        /// <param name="iMinute">Номер интервала на котором должна произойти замена ПБР-значений</param>
        /// <param name="interval">Интервал интегрирования</param>
        /// <returns>Значение инициативы собствееной</returns>
        public static List<PanelAdminKomDisp.InitiativeAux> CalculateInitiativeAux (this HAdmin.RDGStruct [] values, double PBR_0, ASUTP.Core.DateTimeRange range, double newPbr, int interval = 3)
        {
            List<PanelAdminKomDisp.InitiativeAux> arrayRes = new List<PanelAdminKomDisp.InitiativeAux> ();

            int iHour = -1;
            double udge = -1F;

            using (PanelAdminKomDisp.InitiativeAuxCalculate calc = new PanelAdminKomDisp.InitiativeAuxCalculate (values, newPbr, PBR_0, range)) {
                // проверить соответствие размеров коллекций
                if (calc.Ready == true) {
                    while (calc.Ready == true) {
                        arrayRes.Add (new PanelAdminKomDisp.InitiativeAux (calc));

                        calc.Next ();
                    }

                    iHour = calc.IndexHourEnd;

                    // проверить соответствует ли значение по команде значениям источника ПБР (Модес-Центр, -Терминал) в последующих часах
                    while (++iHour < values.Length) {
                        udge = values [iHour].UDGe (values [iHour - 1].pbr);
                        if (!(newPbr == udge)) {
                            arrayRes.Add (new PanelAdminKomDisp.InitiativeAux () { Stamp = range.Begin.Date.AddHours (iHour + 1), Recomendation = newPbr - udge });
                        } else
                            ;
                    }
                } else
                    // размеры коллекций не совпадают
                    throw new Exception ("::CalculateInitiativeAux () - размеры коллекций не совпадают...");
            }

            return arrayRes;
        }
    }
}
