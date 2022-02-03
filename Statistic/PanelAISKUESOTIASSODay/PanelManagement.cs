using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel; //IContainer
using System.Threading; //ManualResetEvent
using System.Drawing; //Color
using System.Data;

using ZedGraph;


using StatisticCommon;
using System.Linq;
using System.Data.Common;
using ASUTP.Control;
using ASUTP.Core;
using ASUTP;

namespace Statistic
{
    /// <summary>
    /// Панель для отображения значений СОТИАССО (телеметрия)
    ///  для контроля
    /// </summary>
    partial class PanelAISKUESOTIASSODay
    {
        /// <summary>
        /// Класс для размещения активных элементов управления
        /// </summary>
        private class PanelManagement : ASUTP.Control.HPanelCommon
        {
            public event DelegateFunc EvtExportDo;

            public event Action<ActionDateTime> EvtDateTimeChanged;
            /// <summary>
            /// Событие изменения текущего индекса ГТП
            /// </summary>
            public event Action<int> EvtTECListSelectionIndexChanged;
            /// <summary>
            /// Событие выбора сигнала (АИИСКУЭ/СОТИАССО) для отображения И экспорта
            /// </summary>
            public event Action<CONN_SETT_TYPE, ActionSignal, int> EvtActionSignalItem;
            /// <summary>
            /// Словарь с идентификаторами выбранных сигналов
            /// 2 элемента; список использовать нельзя, т.к. значения перечисления 'CONN_SETT_TYPE' м.б. отрицательными
            /// </summary>
            private Dictionary<CONN_SETT_TYPE, int> m_dictPreviousSignalItemSelected;

            /// <summary>
            /// Конструктор - дополнительный (без параметров)
            /// </summary>
            private PanelManagement()
                : base(6, 24)
            {
            }

            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец объекта</param>
            /// <param name="foreColor">Цвет шрифта</param>
            /// <param name="backColor">Цвет фона</param>
            public PanelManagement (IContainer container, Color foreColor, Color backColor)
                : this(foreColor, backColor)
            {
                container.Add(this);
            }

            /// <summary>
            /// Конструктор - основной (c параметрами)
            /// </summary>
            /// <param name="foreColor">Цвет шрифта</param>
            /// <param name="backColor">Цвет фона</param>
            public PanelManagement (Color foreColor, Color backColor)
                : base (6, 24)
            {
                //Инициализировать равномерные высоту/ширину столбцов/строк
                initializeLayoutStyleEvenly ();

                m_dictPreviousSignalItemSelected = new Dictionary<CONN_SETT_TYPE, int> () { { CONN_SETT_TYPE.DATA_AISKUE, -1 }, { CONN_SETT_TYPE.DATA_SOTIASSO, -1 } };

                BackColor = backColor;
                ForeColor = foreColor;

                initializeComponent ();

                ComboBox ctrl = findControl (KEY_CONTROLS.CBX_TIMEZONE.ToString ()) as ComboBox;
                ctrl.Items.AddRange (new object [] { "UTC", "Москва", "Новосибирск" });
                ctrl.SelectedIndex = 1;
            }

            /// <summary>
            /// Инициализация панели с установкой кол-ва столбцов, строк
            /// </summary>
            /// <param name="cols">Количество столбцов</param>
            /// <param name="rows">Количество строк</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// Инициализация, размещения собственных элементов управления
            /// </summary>
            private void initializeComponent()
            {
                Control ctrl = null;
                SplitContainer stctrSignals;

                //Приостановить прорисовку текущей панели
                // ??? корректней приостановить прорисовку после создания всех дочерних элементов
                // ??? при этом потребуется объявить переменные для каждого из элементов управления
                this.SuspendLayout();

                //Создать дочерние элементы управления
                // календарь для установки текущих даты, номера часа
                ctrl = new DateTimePicker();
                ctrl.Name = KEY_CONTROLS.DTP_CUR_DATE.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as DateTimePicker).DropDownAlign = LeftRightAlignment.Right;
                (ctrl as DateTimePicker).Format = DateTimePickerFormat.Custom;
                (ctrl as DateTimePicker).CustomFormat = "dd MMM, yyyy";
                (ctrl as DateTimePicker).Value = DateTime.Now.Date.AddDays(-1);
                //Добавить к текущей панели календарь
                this.Controls.Add(ctrl, 0, 0);
                this.SetColumnSpan(ctrl, 3);
                this.SetRowSpan(ctrl, 1);
                // Обработчики событий
                (ctrl as DateTimePicker).ValueChanged += new EventHandler(curDatetime_OnValueChanged);

                // список для выбора ТЭЦ
                ctrl = new ComboBox();
                ctrl.Name = KEY_CONTROLS.CBX_TEC_LIST.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                //Добавить к текущей панели список выбра ТЭЦ
                this.Controls.Add(ctrl, 3, 0);
                this.SetColumnSpan(ctrl, 3);
                this.SetRowSpan(ctrl, 1);
                // Обработчики событий
                (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTECList_OnSelectionIndexChanged);

                // список для часовых поясов
                ctrl = new ComboBox();
                ctrl.Name = KEY_CONTROLS.CBX_TIMEZONE.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                ctrl.Enabled = false;
                //Добавить к текущей панели список для часовых поясов
                this.Controls.Add(ctrl, 0, 1);
                this.SetColumnSpan(ctrl, 3);
                this.SetRowSpan(ctrl, 1);
                //// Обработчики событий
                (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_OnSelectedIndexChanged);

                // кнопка для инициирования экспорта
                ctrl = new Button();
                ctrl.Name = KEY_CONTROLS.BTN_EXPORT.ToString();
                ctrl.Dock = DockStyle.Fill;
                ctrl.Text = @"Экспорт";
                //Добавить к текущей панели кнопку "Экспорт"
                this.Controls.Add(ctrl, 3, 1);
                this.SetColumnSpan(ctrl, 3);
                this.SetRowSpan(ctrl, 1);
                // Обработчики событий
                (ctrl as Button).Click += new EventHandler(btnExport_OnClick);

                // панель для управления размером списков с сигналами
                stctrSignals = new SplitContainer();
                stctrSignals.Dock = DockStyle.Fill;
                stctrSignals.Orientation = Orientation.Horizontal;
                //stctrSignals.Panel1MinSize = -1;
                //stctrSignals.Panel2MinSize = -1;
                stctrSignals.SplitterDistance = 46;
                //Добавить сплитер на панель управления
                this.Controls.Add(stctrSignals, 0, 2);
                this.SetColumnSpan(stctrSignals, 6);
                this.SetRowSpan(stctrSignals, 22);

                // список сигналов АИИСКУЭ
                ctrl = new CheckedListBox();
                ctrl.Name = KEY_CONTROLS.CLB_AIISKUE_SIGNAL.ToString();
                ctrl.Dock = DockStyle.Fill;
                ctrl.BackColor = BackColor == SystemColors.Control ? SystemColors.Window : BackColor;
                ctrl.ForeColor = ForeColor;
                ////Добавить к текущей панели список сигналов АИИСКУЭ
                //this.Controls.Add(ctrl, 0, 2);
                //this.SetColumnSpan(ctrl, 6);
                //this.SetRowSpan(ctrl, 10);
                //Добавить с сплиттеру
                stctrSignals.Panel1.Controls.Add(ctrl);
                // Обработчики событий
                (ctrl as CheckedListBox).SelectedIndexChanged += new EventHandler(clbAIISKUESignal_OnSelectedIndexChanged);
                (ctrl as CheckedListBox).ItemCheck += new ItemCheckEventHandler(clbAIISKUESignal_OnItemChecked);

                // список сигналов СОТИАССО
                ctrl = new CheckedListBox();
                ctrl.Name = KEY_CONTROLS.CLB_SOTIASSO_SIGNAL.ToString();
                ctrl.Dock = DockStyle.Fill;
                ctrl.BackColor = BackColor == SystemColors.Control ? SystemColors.Window : BackColor;
                ctrl.ForeColor = ForeColor;
                ////Добавить к текущей панели список сигналов СОТИАССО
                //this.Controls.Add(ctrl, 0, 12);
                //this.SetColumnSpan(ctrl, 6);
                //this.SetRowSpan(ctrl, 12);
                //Добавить с сплиттеру
                stctrSignals.Panel2.Controls.Add(ctrl);
                // Обработчики событий
                (ctrl as CheckedListBox).SelectedIndexChanged += new EventHandler(clbSOTIASSOSignal_OnSelectedIndexChanged);
                (ctrl as CheckedListBox).ItemCheck += new ItemCheckEventHandler(clbSOTIASSOSignal_OnItemChecked);

                //Возобновить прорисовку текущей панели
                this.ResumeLayout(false);
                //Принудительное применение логики макета
                this.PerformLayout();
            }

            /// <summary>
            /// Обработчик события - нажатие кнопки экспорт
            /// </summary>
            /// <param name="sender">Объект - инициатор события(кнопка)</param>
            /// <param name="e">Аргумент события</param>
            private void btnExport_OnClick(object sender, EventArgs e)
            {
                EvtExportDo?.Invoke();
            }

            /// <summary>
            /// Обработчик события - изменение состояния "отмечено" элемента списка
            /// </summary>
            /// <param name="sender">Объект - инициатор события(список)</param>
            /// <param name="e">Аргумент события</param>
            private void clbAIISKUESignal_OnItemChecked(object sender, ItemCheckEventArgs e)
            {
                EvtActionSignalItem?.Invoke(CONN_SETT_TYPE.DATA_AISKUE, ActionSignal.CHECK, (sender as CheckedListBox).SelectedIndex);
            }

            /// <summary>
            /// Обработчик события - изменение выбранного элемента списка
            /// </summary>
            /// <param name="sender">Объект - инициатор события(список)</param>
            /// <param name="e">Аргумент события</param>
            private void clbAIISKUESignal_OnSelectedIndexChanged(object sender, EventArgs e)
            {
                CONN_SETT_TYPE type = CONN_SETT_TYPE.DATA_AISKUE;

                if (!(m_dictPreviousSignalItemSelected[type] == (sender as CheckedListBox).SelectedIndex)) {
                    EvtActionSignalItem?.Invoke(type, ActionSignal.SELECT, (sender as CheckedListBox).SelectedIndex);

                    m_dictPreviousSignalItemSelected[type] = (sender as CheckedListBox).SelectedIndex;
                } else
                    ;
            }

            private void clbSOTIASSOSignal_OnItemChecked(object sender, ItemCheckEventArgs e)
            {
                EvtActionSignalItem?.Invoke(CONN_SETT_TYPE.DATA_SOTIASSO, ActionSignal.CHECK, (sender as CheckedListBox).SelectedIndex);
            }

            private void clbSOTIASSOSignal_OnSelectedIndexChanged(object sender, EventArgs e)
            {
                CONN_SETT_TYPE type = CONN_SETT_TYPE.DATA_SOTIASSO;

                if (!(m_dictPreviousSignalItemSelected[type] == (sender as CheckedListBox).SelectedIndex)) {
                    EvtActionSignalItem?.Invoke(type, ActionSignal.SELECT, (sender as CheckedListBox).SelectedIndex);

                    m_dictPreviousSignalItemSelected[type] = (sender as CheckedListBox).SelectedIndex;
                } else
                    ;
            }

            /// <summary>
            /// Обработчик события - дескриптор элемента управления создан
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            public void Parent_OnHandleCreated(object obj, EventArgs ev)
            {
            }

            /// <summary>
            /// Текущее (указанное пользователем) дата/время
            /// ??? учитывать часовой пояс
            /// </summary>
            public DateTime CurDateTime
            {
                get
                {
                    return (findControl(KEY_CONTROLS.DTP_CUR_DATE.ToString()) as DateTimePicker).Value;
                }
            }

            public int CurUtcOffset
            {
                get {
                    int iRes = 0;

                    switch ((findControl(KEY_CONTROLS.CBX_TIMEZONE.ToString()) as ComboBox).SelectedIndex) {
                        case 0:
                            // UTC
                            break;
                        case 1:
                            iRes = 3; // Москва
                            break;
                        case 2:
                            iRes = 7; // Новосибирск
                            break;
                        default:
                            break;
                    }

                    return iRes;
                }
            }

            private void enableSelectedIndexchanged(CheckedListBox clb, CONN_SETT_TYPE key, bool bEnabled)
            {
                if (bEnabled == true) {
                    if (key == CONN_SETT_TYPE.DATA_AISKUE)
                        clb.SelectedIndexChanged += /*new EventHandler(*/clbAIISKUESignal_OnSelectedIndexChanged/*)*/;
                    else if (key == CONN_SETT_TYPE.DATA_SOTIASSO)
                        clb.SelectedIndexChanged += /*new EventHandler(*/clbSOTIASSOSignal_OnSelectedIndexChanged/*)*/;
                    else
                        ;
                } else if (bEnabled == false) {
                    if (key == CONN_SETT_TYPE.DATA_AISKUE)
                        clb.SelectedIndexChanged -= /*new EventHandler(*/clbAIISKUESignal_OnSelectedIndexChanged/*)*/;
                    else if (key == CONN_SETT_TYPE.DATA_SOTIASSO)
                        clb.SelectedIndexChanged -= /*new EventHandler(*/clbSOTIASSOSignal_OnSelectedIndexChanged/*)*/;
                    else
                        ;
                } else
                    ;
            }

            public void ClearSignalList(CONN_SETT_TYPE key)
            {
                KEY_CONTROLS keyCtrl = KEY_CONTROLS.UNKNOWN;
                CheckedListBox clb;

                switch (key) {
                    case CONN_SETT_TYPE.DATA_AISKUE:
                        keyCtrl = KEY_CONTROLS.CLB_AIISKUE_SIGNAL;
                        break;
                    case CONN_SETT_TYPE.DATA_SOTIASSO:
                        keyCtrl = KEY_CONTROLS.CLB_SOTIASSO_SIGNAL;
                        break;
                    default:
                        break;
                }

                if (!(keyCtrl == KEY_CONTROLS.UNKNOWN)) {
                    clb = (findControl(keyCtrl.ToString())) as CheckedListBox;

                    // отменить регистрацию обработчика
                    enableSelectedIndexchanged(clb, key, false);

                    m_dictPreviousSignalItemSelected[key] = -1;
                    clb.Items.Clear();

                    // восстановить регистрацию обработчика
                    enableSelectedIndexchanged(clb, key, true);
                } else
                    Logging.Logg().Error(string.Format(@"PanelSOTIASSODay.PanelManagement::InitializeSignalList (key={0}) - ", key.ToString())
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }

            public void InitializeSignalList(CONN_SETT_TYPE key, IEnumerable<string> listSignalNameShr)
            {
                KEY_CONTROLS keyCtrl = KEY_CONTROLS.UNKNOWN;
                CheckedListBox clb;

                switch (key) {
                    case CONN_SETT_TYPE.DATA_AISKUE:
                        keyCtrl = KEY_CONTROLS.CLB_AIISKUE_SIGNAL;
                        break;
                    case CONN_SETT_TYPE.DATA_SOTIASSO:
                        keyCtrl = KEY_CONTROLS.CLB_SOTIASSO_SIGNAL;
                        break;
                    default:
                        break;
                }

                if (!(keyCtrl == KEY_CONTROLS.UNKNOWN)) {
                    clb = (findControl(keyCtrl.ToString())) as CheckedListBox;

                    // отменить регистрацию обработчика
                    enableSelectedIndexchanged(clb, key, false);

                    clb.Items.AddRange(listSignalNameShr.ToArray());

                    // восстановить регистрацию обработчика
                    enableSelectedIndexchanged(clb, key, true);

                    if (clb.Items.Count > 0)
                        clb.SelectedIndex = 0;
                    else
                        ;
                } else
                    Logging.Logg().Error(string.Format(@"PanelSOTIASSODay.PanelManagement::InitializeSignalList (key={0}) - ", key.ToString())
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }

            private void curDatetime_OnValueChanged(object obj, EventArgs ev)
            {
                EvtDateTimeChanged?.Invoke(ActionDateTime.VALUE);
            }

            public void SetTECList(IEnumerable<TEC> listTEC)
            {
                ComboBox ctrl;

                ctrl = findControl(KEY_CONTROLS.CBX_TEC_LIST.ToString()) as ComboBox;

                foreach (TEC t in listTEC)
                    ctrl.Items.Add(t.name_shr);
            }
            /// <summary>
            /// Обработчик события - изменение выбранного элемента 'ComboBox' - текущая ТЭЦ
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void cbxTECList_OnSelectionIndexChanged(object obj, EventArgs ev)
            {
                EvtTECListSelectionIndexChanged?.Invoke(Convert.ToInt32(((this.Controls.Find(KEY_CONTROLS.CBX_TEC_LIST.ToString(), true))[0] as ComboBox).SelectedIndex));
            }

            private void cbxTimezone_OnSelectedIndexChanged(object obj, EventArgs ev)
            {
                EvtDateTimeChanged?.Invoke(ActionDateTime.TIMEZONE);
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

                    findControl (KEY_CONTROLS.CLB_AIISKUE_SIGNAL.ToString ()).ForeColor =
                    findControl (KEY_CONTROLS.CLB_SOTIASSO_SIGNAL.ToString ()).ForeColor =
                        value;
                }
            }

            public override Color BackColor
            {
                get
                {
                    return base.BackColor;
                }

                set
                {
                    base.BackColor = value;

                    findControl (KEY_CONTROLS.CLB_AIISKUE_SIGNAL.ToString ()).BackColor =
                    findControl (KEY_CONTROLS.CLB_SOTIASSO_SIGNAL.ToString ()).BackColor =
                        value == SystemColors.Control ? SystemColors.Window : value;
                }
            }
        }
    }
}