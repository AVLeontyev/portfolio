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
        /// Класс для отображения значений в табличной форме
        /// </summary>
        private class HDataGridView : DataGridView
        {
            /// <summary>
            /// Конструктор - основной (без аргументов)
            /// </summary>
            public HDataGridView()
                : base()
            {
                initializeComponent();
            }

            private void initializeComponent()
            {
                TimeSpan tsRow = TimeSpan.Zero;

                Columns.Add("Unknown", string.Empty);
                Columns[0].Visible = false;
                Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;

                for (int i = 0; i < 49; i++) {
                    Rows.Add();

                    Rows[i].Tag = i;
                    if (i < 48) {
                        Rows[i].HeaderCell.Value =
                        Rows[i].HeaderCell.ToolTipText =
                            string.Format("{0}", new DateTime((tsRow = tsRow.Add(TimeSpan.FromMinutes(30))).Ticks).ToString(@"HH:mm"));
                    } else
                        Rows[i].HeaderCell.Value =
                        Rows[i].HeaderCell.ToolTipText =
                            string.Format("{0}", @"Итог:");
                }

                AllowUserToAddRows = false;
                AllowUserToDeleteRows = false;
                AllowUserToResizeColumns = false;

                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders | DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                MultiSelect = false;
                AutoSizeColumnsMode = /*DataGridViewAutoSizeColumnsMode.ColumnHeader |*/ DataGridViewAutoSizeColumnsMode.AllCells;
                SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
            }

            /// <summary>
            /// Действие со столбцом: при наличии - удалить, при отсутствии - добавить
            /// </summary>
            /// <param name="name">Идентификатор столбца</param>
            /// <param name="headerText">Заголовок столбца</param>
            /// <returns>Признак удаления/добавления столбца</returns>
            public bool ActionColumn(string name, string headerText)
            {
                bool bRes = !Columns.Contains(name);

                if (bRes == false)
                    // столбец найден
                    Columns.Remove(name);
                else {
                    // столбец не найден - добавить
                    SelectionMode = DataGridViewSelectionMode.CellSelect;

                    Columns.Add(name, headerText);
                    Columns [ColumnCount - 1].DefaultCellStyle.BackColor = BackColor == SystemColors.Control ? SystemColors.Window : BackColor;
                    Columns [ColumnCount - 1].DefaultCellStyle.ForeColor = ForeColor;
                    Columns [ColumnCount - 1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    Columns[ColumnCount - 1].SortMode = DataGridViewColumnSortMode.NotSortable;

                    SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
                }

                return bRes;
            }

            /// <summary>
            /// Заполнить значениями один из столбцов по указанному индексу
            /// </summary>
            /// <param name="iColumn">Индекс столбца</param>
            /// <param name="values">Значения для столбца</param>
            private void fill(int iColumn, IEnumerable<HandlerSignalQueue.VALUE> values)
            {
                float fValue = -1F;

                if (iColumn > 0)
                    if (values.Count () > 0)
                        foreach (DataGridViewRow row in Rows)
                            if (row.Index < values.Count())
                                try {
                                    fValue = (from value in values where value.index_stamp == (int)row.Tag select value.value).ElementAt (0);
                                    row.Cells[iColumn].Value = fValue;
                                    row.Cells [iColumn].ToolTipText = fValue.ToString ();
                                } catch (Exception e) {
                                    row.Cells [iColumn].Value = @"-";

                                    Logging.Logg().Warning(string.Format(@"PanelSOTIASSODay.HDataGridView::Fill (iColumn={0}) - не найдено значение для строки Index={1}, Tag={2}", iColumn, row.Index, row.Tag), Logging.INDEX_MESSAGE.NOT_SET);
                                }
                            else
                                row.Cells[iColumn].Value = values.Sum(v => v.value);
                    else
                        Logging.Logg ().Error ($"PanelSOTIASSODay.HDataGridView::fill (iColumn={iColumn}) - нет ни одного значения для ...", Logging.INDEX_MESSAGE.NOT_SET);
                else
                    Logging.Logg().Error(string.Format(@"PanelSOTIASSODay.HDataGridView::fill (iColumn={0}) - номер столбца для значений не корректен...", iColumn), Logging.INDEX_MESSAGE.NOT_SET);
            }

            /// <summary>
            /// Заполнить значениями один(всегда крайний) из столбцов
            /// </summary>
            /// <param name="values">Значения для столбца</param>
            public void Fill(IEnumerable<HandlerSignalQueue.VALUE> values)
            {
                fill(ColumnCount - 1, values);
            }

            ///// <summary>
            ///// Заполнить значениями один из столбцов: поиск по индексу
            ///// </summary>
            ///// <param name="values">Значения для столбца</param>
            //public void Fill(int iColumn, IEnumerable<HandlerSignalQueue.VALUE> values)
            //{
            //    fill(iColumn, values);
            //}
            ///// <summary>
            ///// Заполнить значениями один из столбцов: поиск по наименованию
            ///// </summary>
            ///// <param name="nameColumn">Наименование столбца</param>
            ///// <param name="values">Значения для столбца</param>
            //public void Fill(string nameColumn, IEnumerable<HandlerSignalQueue.VALUE> values)
            //{
            //    fill(Columns.Cast<DataGridViewColumn>().FirstOrDefault(col => { return false; }).Index, values);
            //}

            /// <summary>
            /// Очистить элемент (удалить столбцы
            ///  , за исключением 1-го, служебного, невидимого, чтобы сохранить строки и их идентификаторы - tag-и)
            /// </summary>
            public void Clear()
            {
                while (ColumnCount > 1) // т.к. необходимо оставить "Unknown", чтобы сохранить строки и их tag-и
                    Columns.RemoveAt(ColumnCount - 1);
            }

            /// <summary>
            /// Извлечь данные для экспорта
            /// </summary>
            /// <returns>Таблица со значениями</returns>
            public DataTable GetValues()
            {
                DataTable tableRes = new DataTable();

                object[] rowValues;
                float value = -1F;

                if (ColumnCount > 1) {
                    foreach (DataGridViewColumn column in Columns) {
                        if (column.Index > 0)
                            tableRes.Columns.Add(column.HeaderText, typeof(string));
                        else
                        // служебный, скрытый столбец
                            ;
                    }

                    rowValues = new object[ColumnCount - 1];

                    foreach (DataGridViewRow row in Rows) {
                        if (row.Index < (Rows.Count - 1)) {
                            foreach (DataGridViewColumn column in Columns)
                                if (column.Index > 0)
                                    //if (float.TryParse((string)row.Cells[column.Index].Value, out value) == true)
                                        rowValues[column.Index - 1] =
                                            //value
                                            row.Cells[column.Index].Value.ToString()
                                                ;
                                    //else
                                    //    rowValues[column.Index - 1] = 0F;
                                else
                                // служебный, скрытый столбец
                                    ;

                            tableRes.Rows.Add(rowValues);
                        } else
                        // строка с ИТОГО - не требуется
                            ;
                    }
                } else
                    ;

                return tableRes;
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

                    for (int j = 0; j < ColumnCount; j++)
                        //for (int i = 0; i < RowCount; i++)
                            Columns [j].DefaultCellStyle.ForeColor = value;
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

                    for (int j = 0; j < ColumnCount; j++)
                        //for (int i = 0; i < RowCount; i++)
                            Columns[j].DefaultCellStyle.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
                }
            }
        }
    }
}