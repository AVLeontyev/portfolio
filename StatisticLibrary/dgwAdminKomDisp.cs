using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Globalization;


using System.Drawing;
using ASUTP;

namespace StatisticCommon
{
    public class DataGridViewAdminKomDisp : DataGridViewAdmin
    {
        public enum COLUMN_INDEX : ushort { DATE_HOUR, PLAN, UDGe, RECOMENDATION, FOREIGN_CMD, DEVIATION_TYPE, DEVIATION, TO_ALL, COUNT_COLUMN };

        /// <summary>
        /// ???????? ???????? ??????? ????????????? ??? ????????????? ?? ????????
        /// </summary>
        struct COLUMN_PROPERTIES {
            /// <summary>
            /// ??? ???????
            /// </summary>
            public Type _type;
            /// <summary>
            /// ????????????? ???????????? ????????
            /// </summary>
            public string _name;
            /// <summary>
            /// ????????????? ???????????? ????????
            /// </summary>
            public string _headerText;
            /// <summary>
            /// ???????? ?? ?????????
            /// </summary>
            public string DefaultValue;
            /// <summary>
            /// ????????????? ???????????? ????????
            /// </summary>
            public bool _frozen;
            /// <summary>
            /// ????????????? ???????????? ????????
            /// </summary>
            public bool _readOnly;
            /// <summary>
            /// ????????????? ???????????? ????????
            /// </summary>
            public DataGridViewTriState _resizable;
            /// <summary>
            /// ????????????? ???????????? ????????
            /// </summary>
            public int _width;
            /// <summary>
            /// ????????????? ???????????? ????????
            /// </summary>
            public System.Windows.Forms.DataGridViewColumnSortMode _sortMode;
        }

        protected override int INDEX_COLUMN_BUTTON_TO_ALL
        {
            get
            {
                return (int)COLUMN_INDEX.TO_ALL;
            }
        }

        public double m_PBR_0;

        public DataGridViewAdminKomDisp(System.Drawing.Color foreColor, System.Drawing.Color []backgroudColors) : base(foreColor, backgroudColors)
        {
            this.CellMouseMove += new DataGridViewCellMouseEventHandler (dgwAdminTable_CellMouseMove);
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            Columns.AddRange (new DataGridViewColumn [] {
                new DataGridViewTextBoxColumn () { // DATE_HOUR
                Name = COLUMN_INDEX.DATE_HOUR.ToString()
                , HeaderText = "????, ???"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = true
                , ReadOnly = true
                , Resizable = DataGridViewTriState.False
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewTextBoxColumn () { // PLAN
                Name = COLUMN_INDEX.PLAN.ToString()
                , HeaderText = "????"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = false
                , ReadOnly = true
                , Resizable = DataGridViewTriState.False
                , Width = 56
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewTextBoxColumn() { // UDGe
                Name = COLUMN_INDEX.UDGe.ToString()
                , HeaderText = "????"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = false
                , ReadOnly = true
                , Resizable = DataGridViewTriState.False
                , Width = 56
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewTextBoxColumn() { // RECOMENDATION
                Name = COLUMN_INDEX.RECOMENDATION.ToString()
                , HeaderText = "????????????"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = false
                , ReadOnly = false
                , Resizable = DataGridViewTriState.False
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewCheckBoxColumn() { // FOREIGN_CMD
                Name = COLUMN_INDEX.FOREIGN_CMD.ToString()
                , HeaderText = "?????. ???-??"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = false }
                , Frozen = false
                , ReadOnly = false
                , Resizable = DataGridViewTriState.False                
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewCheckBoxColumn() { // DEVIATION_TYPE
                Name = COLUMN_INDEX.DEVIATION_TYPE.ToString()
                , HeaderText = "?????????? ? ?????????"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = false }
                , Frozen = false
                , ReadOnly = false
                , Resizable = DataGridViewTriState.False
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewTextBoxColumn() { // DEVIATION
                Name = COLUMN_INDEX.DEVIATION.ToString()
                , HeaderText = "???????? ????????????? ??????????"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = false
                , ReadOnly = false
                , Resizable = DataGridViewTriState.True
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }
            , new DataGridViewButtonColumn() { // TO_ALL
                Name = COLUMN_INDEX.TO_ALL.ToString()
                , HeaderText = "???????????"
                , DefaultCellStyle = new DataGridViewCellStyle() { NullValue = string.Empty }
                , Frozen = false
                , ReadOnly = true
                , Resizable = DataGridViewTriState.False
                //, Width = -1
                , SortMode = DataGridViewColumnSortMode.NotSortable
            }});

            //AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
            
            Columns [(int)COLUMN_INDEX.FOREIGN_CMD].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Columns [(int)COLUMN_INDEX.DEVIATION_TYPE].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Columns [(int)COLUMN_INDEX.TO_ALL].DefaultCellStyle.BackColor = s_dgvCellStyles[(int)INDEX_CELL_STYLE.COMMON].BackColor;
            Columns [(int)COLUMN_INDEX.TO_ALL].DefaultCellStyle.ForeColor = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON].ForeColor;

            //BackColor = s_dgvCellStyles[(int)INDEX_CELL_STYLE.COMMON].BackColor;
            InitializeColumnToAll ();
        }

        protected override void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs ev)
        {
            double value;
            bool valid;

            switch (ev.ColumnIndex)
            {
                case (int)COLUMN_INDEX.PLAN: // ????
                    valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.PLAN].Value, out value);
                    if ((valid == false) || (value > maxRecomendationValue))
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.PLAN].Value = 0.ToString("F2");
                    else
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.PLAN].Value = value.ToString("F2");
                    break;
                case (int)COLUMN_INDEX.UDGe: //?? ?????????????
                    break;
                case (int)COLUMN_INDEX.RECOMENDATION: // ????????????
                    valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.RECOMENDATION].Value, out value);
                    if ((valid == false) || (value > maxRecomendationValue))
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.RECOMENDATION].Value = 0.ToString("F2");
                    else {
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.RECOMENDATION].Value = value.ToString("F2");

                        double prevPbr
                            , Pbr = double.Parse(Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.PLAN].Value.ToString ());
                        if (ev.RowIndex > 0)
                            prevPbr = double.Parse(Rows[ev.RowIndex - 1].Cells[(int)COLUMN_INDEX.PLAN].Value.ToString());
                        else
                            prevPbr = m_PBR_0;

                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.UDGe].Value = (((Pbr + prevPbr) / 2) + value).ToString("F2");
                    }
                    break;
                case (int)COLUMN_INDEX.FOREIGN_CMD:
                    bool fCmd = false;
                    try {
                        fCmd = bool.Parse(Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.FOREIGN_CMD].Value.ToString());
                    }
                    catch (Exception e) {
                        Logging.Logg().Exception(e, @"DataGridViewAdminKomDisp::CellValidate () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.RECOMENDATION].Value, out value);
                    if ((valid == false) /*|| (value == 0F)*/ || (value > maxRecomendationValue))
                        fCmd = false;
                    else
                        ;
                    Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.FOREIGN_CMD].Value = fCmd;
                    break;
                case (int)COLUMN_INDEX.DEVIATION_TYPE:
                    break;
                case (int)COLUMN_INDEX.DEVIATION: // ???????????? ??????????
                    valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.DEVIATION].Value, out value);
                    bool isPercent = bool.Parse(Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.DEVIATION_TYPE].Value.ToString());
                    double maxValue = -1F;

                    if (isPercent == true)
                        maxValue = maxDeviationPercentValue;
                    else
                        maxValue = maxDeviationValue; // ?????? ??? ???????? ?? ???????????, ?? ??? ???????????? ??????? ?????????

                    if ((valid == false) || (value < 0) || (value > maxValue))
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.DEVIATION].Value = 0.ToString("F2");
                    else
                        Rows[ev.RowIndex].Cells[(int)COLUMN_INDEX.DEVIATION].Value = value.ToString("F2");
                    break;
                default:
                    break;
            }
        }

        public override void ClearTables()
        {
            ////for (int i = 0; i < Rows.Count; i++)
            //foreach (DataGridViewRow r in Rows)
            //    for (int j = (int)COLUMN_INDEX.DATE_HOUR; j < ((int)COLUMN_INDEX.TO_ALL + 0); j++)
            //        r.Cells[j].Value = _columnProperties[j].DefaultValue;
        }

        private void dgwAdminTable_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case (int)COLUMN_INDEX.DATE_HOUR:
                case (int)COLUMN_INDEX.PLAN:
                case (int)COLUMN_INDEX.UDGe:
                    Cursor = Cursors.Help;
                    break;
                case (int)COLUMN_INDEX.RECOMENDATION:
                case (int)COLUMN_INDEX.DEVIATION:
                    Cursor = Cursors.IBeam;
                    break;
                case (int)COLUMN_INDEX.FOREIGN_CMD:
                case (int)COLUMN_INDEX.DEVIATION_TYPE:
                case (int)COLUMN_INDEX.TO_ALL:
                    Cursor = Cursors.Hand;
                    break;
                default:
                    Cursor = Cursors.Default;
                    break;
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

                //??? ??????? ?1 - ???????????
                DefaultCellStyle.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
                ////??? ??????? ?2 - ??????????(?? ?????????????)
                //if ((INDEX_COLUMN_BUTTON_TO_ALL > 0)
                //    && (RowCount > 0))
                //    for (int col = 0; col < (int)INDEX_COLUMN_BUTTON_TO_ALL; col++)
                //        for (int i = 0; i < 24; i++) {
                //            if ((Rows [i].Cells [col].Style.BackColor.Equals (s_dgvCellStyles[(int)INDEX_CELL_STYLE.ERROR].BackColor) == false)
                //                && (Rows [i].Cells [col].Style.BackColor.Equals (s_dgvCellStyles [(int)INDEX_CELL_STYLE.ERROR].BackColor) == false))
                //            // ??????? ??????????? ??? ?????????? ???????? ????? ??? ?????
                //                Rows [i].Cells [col].Style.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
                //            else
                //                ;
                //        }
                //else
                //// ??? ????????/????? - ??? ???????? ?? ????????? ????? ???? ?????
                //    ;
            }
        }

        public void Fill (DateTime date, HAdmin.RDGStruct [] values, Func<int, string> fGetFmtDatetime, Func<int, int> fGetSeasonHourOffset, int exact = 2)
        {
            string strFmtDatetime = string.Empty
                , strFmtValue = string.Empty;
            int offset = -1;

            strFmtValue = $"F{exact}";

            for (int i = 0; i < values.Length; i++) {
                strFmtDatetime = fGetFmtDatetime (i);
                offset = fGetSeasonHourOffset (i + 1);

                Rows [i].Cells [(int)COLUMN_INDEX.DATE_HOUR].Value = date.AddHours (i + 1 - offset).ToString (strFmtDatetime);
                //this.dgwAdminTable.Rows [i].Cells [(int)COLUMN_INDEX.DATE_HOUR].Style.BackColor = this.dgwAdminTable.BackColor;

                Rows [i].Cells [(int)COLUMN_INDEX.PLAN].Value = values [i].pbr.ToString (strFmtValue);
                Rows [i].Cells [(int)COLUMN_INDEX.PLAN].ToolTipText = values [i].pbr_number;
                //Rows [i].Cells [(int)COLUMN_INDEX.PLAN].Style.BackColor = this.dgwAdminTable.BackColor;
                if (i > 0)
                    Rows [i].Cells [(int)COLUMN_INDEX.UDGe].Value = (((values [i].pbr + values [i - 1].pbr) / 2) + values [i].recomendation).ToString (strFmtValue);
                else
                    Rows [i].Cells [(int)COLUMN_INDEX.UDGe].Value = (((values [i].pbr + m_PBR_0) / 2) + values [i].recomendation).ToString (strFmtValue);
                //Rows [i].Cells [(int)COLUMN_INDEX.UDGe].Style.BackColor = this.dgwAdminTable.BackColor;
                Rows [i].Cells [(int)COLUMN_INDEX.RECOMENDATION].Value = values [i].recomendation.ToString (strFmtValue);
                Rows [i].Cells [(int)COLUMN_INDEX.RECOMENDATION].ToolTipText = values [i].dtRecUpdate.ToString ();
                //Rows [i].Cells [(int)COLUMN_INDEX.RECOMENDATION].Style.BackColor = this.dgwAdminTable.BackColor;
                Rows [i].Cells [(int)COLUMN_INDEX.FOREIGN_CMD].Value = values [i].fc.ToString ();
                //Rows [i].Cells [(int)COLUMN_INDEX.FOREIGN_CMD].Style.BackColor = this.dgwAdminTable.BackColor;
                Rows [i].Cells [(int)COLUMN_INDEX.DEVIATION_TYPE].Value = values [i].deviationPercent.ToString ();
                //Rows [i].Cells [(int)COLUMN_INDEX.DEVIATION_TYPE].Style.BackColor = this.dgwAdminTable.BackColor;
                Rows [i].Cells [(int)COLUMN_INDEX.DEVIATION].Value = values [i].deviation.ToString (strFmtValue);
                //Rows [i].Cells [(int)COLUMN_INDEX.DEVIATION].Style.BackColor = this.dgwAdminTable.BackColor;
            }
        }
    }
}
