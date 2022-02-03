using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Globalization;

using StatisticCommon;
using System.Drawing;

namespace StatisticTransModes
{
    public class DataGridViewAdminModes : DataGridViewAdmin
    {
        public enum DESC_INDEX : ushort { DATE_HOUR, PBR, PMIN, PMAX/*, TO_ALL*/, COUNT_COLUMN };
        private static string[] arDescStringIndex = { "DateHour", "Plan", "Pmin", "Pmax"/*, "ToAll"*/ };
        private static string[] arDescRusStringIndex = { "Дата, час", "План", "Минимум", "Максимум"/*, "Дозаполнить"*/ };

        public DataGridViewAdminModes()
            : base(SystemColors.ControlText
                  , new System.Drawing.Color [] {
                      SystemColors.Window
                      , System.Drawing.Color.Yellow
                      , System.Drawing.Color.Red
                  })
        {
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            Columns.AddRange(new DataGridViewColumn[(int)DESC_INDEX.COUNT_COLUMN] {new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewTextBoxColumn ()/*,
                                                                                    new DataGridViewButtonColumn ()*/});
            // 
            // DateHour
            // 
            Columns[(int)DESC_INDEX.DATE_HOUR].Frozen = true;
            Columns[(int)DESC_INDEX.DATE_HOUR].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.DATE_HOUR];
            Columns[(int)DESC_INDEX.DATE_HOUR].Name = arDescStringIndex[(int)DESC_INDEX.DATE_HOUR];
            Columns[(int)DESC_INDEX.DATE_HOUR].ReadOnly = true;
            Columns[(int)DESC_INDEX.DATE_HOUR].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Plan
            // 
            //Columns[(int)DESC_INDEX.PBR].Frozen = true;
            Columns[(int)DESC_INDEX.PBR].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.PBR];
            Columns[(int)DESC_INDEX.PBR].Name = arDescStringIndex[(int)DESC_INDEX.PBR];
            Columns[(int)DESC_INDEX.PBR].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Columns[(int)DESC_INDEX.PBR].Width = 126;
            Columns[(int)DESC_INDEX.PBR].ReadOnly = true;
            // 
            // Pmin
            // 
            Columns[(int)DESC_INDEX.PMIN].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.PMIN];
            Columns[(int)DESC_INDEX.PMIN].Name = arDescStringIndex[(int)DESC_INDEX.PMIN];
            Columns[(int)DESC_INDEX.PMIN].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Columns[(int)DESC_INDEX.PMIN].Width = 126;
            Columns[(int)DESC_INDEX.PMIN].ReadOnly = true;
            // 
            // Pmax
            // 
            Columns[(int)DESC_INDEX.PMAX].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.PMAX];
            Columns[(int)DESC_INDEX.PMAX].Name = arDescStringIndex[(int)DESC_INDEX.PMAX];
            Columns[(int)DESC_INDEX.PMAX].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Columns[(int)DESC_INDEX.PMAX].Width = 126;
            Columns[(int)DESC_INDEX.PMAX].ReadOnly = true;
            // 
            // ToAll
            // 
            /*Columns[(int)DESC_INDEX.TO_ALL].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.TO_ALL];
            Columns[(int)DESC_INDEX.TO_ALL].Name = arDescStringIndex[(int)DESC_INDEX.TO_ALL];*/
        }

        protected override int INDEX_COLUMN_BUTTON_TO_ALL
        {
            get
            {
                return -1;
            }
        }

        protected override void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            double value;
            bool valid;

            if ((e.ColumnIndex > 0) && (e.ColumnIndex < Columns.Count - 1))
            {
                valid = double.TryParse((string)Rows[e.RowIndex].Cells[e.ColumnIndex].Value, out value);
                if ((valid == false) || (value > DataGridViewAdmin.maxRecomendationValue))
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0.ToString("F2");
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value.ToString("F2");
                }
            }
            else
                ;
        }

        public override void ClearTables()
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                Rows[i].Cells[(int)DESC_INDEX.DATE_HOUR].Value = 
                Rows[i].Cells[(int)DESC_INDEX.PBR].Value =
                Rows[i].Cells[(int)DESC_INDEX.PMIN].Value =
                Rows[i].Cells[(int)DESC_INDEX.PMAX].Value = string.Empty;
            }
        }
    }
}
