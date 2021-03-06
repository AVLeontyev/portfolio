using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Diagnostics;
using System.Data;
//using System.Data.SqlClient;
using System.Drawing; //Color..
using System.Threading;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

//using HClassLibrary;
using StatisticCommon;
using ASUTP.Core;
using ASUTP;

namespace Statistic
{
    //Extension methods must be defined in a static class
    public static class ZedGraphControlExtension {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static void SetFontColor (this ZedGraphControl zCtrl, Color color)
        {
            if (Equals (zCtrl.GraphPane, null) == false) {
                zCtrl.GraphPane.XAxis.Title.FontSpec.FontColor =
                zCtrl.GraphPane.YAxis.Title.FontSpec.FontColor =
                    color;

                // ????????? ???? ???????? ??? ???????
                zCtrl.GraphPane.XAxis.Scale.FontSpec.FontColor =
                zCtrl.GraphPane.YAxis.Scale.FontSpec.FontColor =
                    color;

                // ????????? ???? ????????? ??? ????????
                zCtrl.GraphPane.Title.FontSpec.FontColor = color;
            } else
                ;
        }
    }
    /// <summary>
    /// ???????? ??????????? ????????? ????? PanelTecViewBase ??????????? ?? PanelStatisticWithTableHourRows
    /// </summary>
    public abstract partial  class PanelTecViewBase : PanelStatisticWithTableHourRows
    {
        /// <summary>
        /// ???? ?????? ??? ?????? "??????? ?????????? ???????"
        /// </summary>
        
        private static readonly int MS_TIMER_CURRENT_UPDATE = 1000;
        /// <summary>
        /// ??????-??????? ??? ???????? ??????? ??????
        /// </summary>
        protected PanelCustomTecView.HLabelCustomTecView m_label;

        /// <summary>
        /// ???? ??????????? ????????? ????????????
        /// </summary>
        protected uint SPLITTER_PERCENT_VERTICAL;

        /// <summary>
        /// ??????????? ????? "???????????"
        /// </summary>
        protected abstract class HZedGraphControl : ZedGraph.ZedGraphControl
        {
            /// <summary>
            /// ??????? "????????? ???????? ???????"
            /// </summary>
            public event DelegateIntFunc EventItemSelected;
            /// <summary>
            /// ??????? ??????? ????
            /// </summary>
            /// <param name="sender">ZedGraphControl</param>
            /// <param name="e">MouseEventArgs</param>
            /// <returns>??? ??????? ??????????? ????????? ???????</returns>
            public bool OnMouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                    return true;

                object obj;
                PointF p = new PointF(e.X, e.Y);
                bool found;
                int index;

                found = FindNearestObject(p, CreateGraphics(), out obj, out index);

                if ((found == true)
                    && ((!(obj == null)) && (obj is CurveItem)))
                {
                    if (((obj as CurveItem).IsBar == false)
                        && ((obj as CurveItem).IsLine == false))
                        return true;
                    else
                        ;

                    EventItemSelected(index);
                }
                else
                    ;

                return true;
            }
            /// <summary>
            /// ???????????? "?????? ???????????? ????" (??? ??????? ?? ??????? ?????? ??????? ????)
            /// </summary>
            public enum INDEX_CONTEXTMENU_ITEM
            {
                SHOW_VALUES,
                SEPARATOR_1
                    , COPY, SAVE, TO_EXCEL,
                SEPARATOR_2
                    , SETTINGS_PRINT, PRINT,
                SEPARATOR_3
                    , AISKUE_PLUS_SOTIASSO, AISKUE, SOTIASSO_3_MIN, SOTIASSO_1_MIN
                    , COUNT
            };
            /// <summary>
            /// ??????? - ????????? ??????? ?????????????????????????
            /// </summary>
            public DelegateFunc delegateSetScale;
            /// <summary>
            /// ????? ??? ???????????? ????
            /// </summary>
            protected class HContextMenuStripZedGraph : System.Windows.Forms.ContextMenuStrip
            {
                public HContextMenuStripZedGraph()
                {
                    InitializeComponent();
                }

                private void InitializeComponent()
                {
                    // 
                    // contextMenuStrip
                    // 
                    this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                        new System.Windows.Forms.ToolStripMenuItem()
                        , new System.Windows.Forms.ToolStripSeparator(),
                        new System.Windows.Forms.ToolStripMenuItem(),
                        new System.Windows.Forms.ToolStripMenuItem(),
                        new System.Windows.Forms.ToolStripMenuItem()
                        , new System.Windows.Forms.ToolStripSeparator(),
                        new System.Windows.Forms.ToolStripMenuItem(),
                        new System.Windows.Forms.ToolStripMenuItem()
                        //, new System.Windows.Forms.ToolStripSeparator(),
                        //new System.Windows.Forms.ToolStripMenuItem(),
                        //new System.Windows.Forms.ToolStripMenuItem(),
                        //new System.Windows.Forms.ToolStripMenuItem(),
                        //new System.Windows.Forms.ToolStripMenuItem()
                    });
                    this.Name = "contextMenuStripMins";
                    this.Size = new System.Drawing.Size(198, 148);

                    int indx = -1;
                    // 
                    // ??????????????????ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SHOW_VALUES; ;
                    this.Items[indx].Name = "??????????????????ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "?????????? ????????";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = true;

                    // 
                    // ??????????ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.COPY;
                    this.Items[indx].Name = "??????????ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "??????????";

                    // 
                    // ?????????ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SAVE;
                    this.Items[indx].Name = "?????????ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "????????? ??????";

                    // 
                    // ??????ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.TO_EXCEL;
                    this.Items[indx].Name = "??????ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "????????? ? MS Excel";

                    // 
                    // ???????????????ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SETTINGS_PRINT;
                    this.Items[indx].Name = "???????????????ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "????????? ??????";
                    // 
                    // ???????????ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.PRINT;
                    this.Items[indx].Name = "???????????ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "???????????";

                    initializeItemAdding();
                }

                protected virtual void initializeItemAdding()
                {
                    this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                        new System.Windows.Forms.ToolStripSeparator(),
                        new System.Windows.Forms.ToolStripMenuItem(),
                        new System.Windows.Forms.ToolStripMenuItem(),
                        new System.Windows.Forms.ToolStripMenuItem(),
                        new System.Windows.Forms.ToolStripMenuItem()
                    });

                    int indx = -1;
                    // 
                    // ???????????????????????ToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO;
                    this.Items[indx].Name = "???????????????????????ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = @"??????+????????"; //"????????: ?? ??????+???????? - 3 ???";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                    this.Items[indx].Enabled = false; //HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_ASKUE_PLUS_SOTIASSO) == true;
                    // 
                    // ??????????????ToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.AISKUE;
                    this.Items[indx].Name = "??????????????ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    //??????????? ? ???????????? "????????"
                    //this.??????????????ToolStripMenuItem.Text = "????????: ?? ?????? - 3 ???";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = true;
                    this.Items[indx].Enabled = false;
                    // 
                    // ????????????????3???ToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN;
                    this.Items[indx].Name = "????????????????3???ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = @"????????(3 ???)"; //"????????: ?? ???????? - 3 ???";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                    this.Items[indx].Enabled = false;
                    // 
                    // ????????????????1???ToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN;
                    this.Items[indx].Name = "????????????????1???ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = @"????????(1 ???)"; //"????????: ?? ???????? - 1 ???";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                    this.Items[indx].Enabled = false;
                }
            }
            /// <summary>
            /// ????  m_lockValue ???? object
            /// </summary>
            private object m_lockValue;
            /// <summary>
            /// ???????? SourceDataText ?????????? ????? ???????????? ????
            /// </summary>
            public string SourceDataText
            {
                get
                {
                    for (HZedGraphControl.INDEX_CONTEXTMENU_ITEM indx = INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx++)
                        if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)indx]).Checked == true)
                            return ((ToolStripMenuItem)ContextMenuStrip.Items[(int)indx]).Text;
                        else
                            ;

                    return string.Empty;
                }
            }
            /// <summary>
            /// ??????????? - ???????? (? ???????????)
            /// </summary>
            /// <param name="lockVal">?????? ?????????????</param>
            /// <param name="fSetScale">??????? ????????? ???????? ???????????????</param>
            public HZedGraphControl(object lockVal, DelegateFunc fSetScale)
            {
                createContextMenuStrip();

                InitializeComponent();

                m_lockValue = lockVal;

                delegateSetScale = fSetScale;
            }

            private void InitializeComponent()
            {
                // 
                // zedGraph
                // 
                this.Dock = System.Windows.Forms.DockStyle.Fill;
                //this.Location = arPlacement[(int)CONTROLS.zedGraphMins].pt;
                this.Name = "zedGraph";
                this.ScrollGrace = 0;
                this.ScrollMaxX = 0;
                this.ScrollMaxY = 0;
                this.ScrollMaxY2 = 0;
                this.ScrollMinX = 0;
                this.ScrollMinY = 0;
                this.ScrollMinY2 = 0;
                //this.Size = arPlacement[(int)CONTROLS.zedGraphMins].sz;
                this.TabIndex = 0;
                this.IsEnableHEdit = false;
                this.IsEnableHPan = false;
                this.IsEnableHZoom = false;
                this.IsEnableSelection = false;
                this.IsEnableVEdit = false;
                this.IsEnableVPan = false;
                this.IsEnableVZoom = false;
                this.IsShowPointValues = true;

                BackColor = FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control
                    ? SystemColors.Window
                        : FormMain.formGraphicsSettings.BackgroundColor;

                this.SetFontColor (FormMain.formGraphicsSettings.FontColor);

                initializeContextMenuItemStandardEventHandler ();

                this.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.OnPointValueEvent);
                this.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.OnDoubleClickEvent);
            }

            protected virtual void createContextMenuStrip()
            {
                this.ContextMenuStrip = new HContextMenuStripZedGraph();
            }
            /// <summary>
            /// ????????????? ???????????? ??????? ??? ?????? ??????? ???? (???????????)
            /// </summary>
            private void initializeContextMenuItemStandardEventHandler()
            {
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SHOW_VALUES].Click += new System.EventHandler(??????????????????ToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.COPY].Click += new System.EventHandler(??????????ToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SAVE].Click += new System.EventHandler(?????????ToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SETTINGS_PRINT].Click += new System.EventHandler(???????????????ToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.PRINT].Click += new System.EventHandler(???????????ToolStripMenuItem_Click);
            }
            /// <summary>
            /// ????????????? ???????????? ??????? ??? ?????? ??????? ???? (??????? ? MS_Excel, ????????? ???? ???????????? ??????)
            /// </summary>
            /// <param name="fToExcel">??????? ????????? ??????? - ??????? ? MS_Excel</param>
            /// <param name="fSourceData">??????? ????????? ??????? - ????????? ???? ???????????? ??????</param>
            public void InitializeContextMenuItemAddingEventHandler(EventHandler fToExcel, EventHandler fAddingHandler)
            {
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.TO_EXCEL].Click += new System.EventHandler(fToExcel);
                initializeContextMenuItemAddingEventHandler(fAddingHandler);
            }

            protected virtual void initializeContextMenuItemAddingEventHandler(EventHandler fAddingHandler)
            {
                for (int i = (int)INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; i < this.ContextMenuStrip.Items.Count; i++)
                    ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[i].Click += new System.EventHandler(fAddingHandler);
            }

            private void ??????????????????ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
                this.IsShowPointValues = ((ToolStripMenuItem)sender).Checked;
            }

            private void ??????????ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                lock (m_lockValue)
                {
                    this.Copy(false);
                }
            }

            private void ???????????????ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                PageSetupDialog pageSetupDialog = new PageSetupDialog();
                pageSetupDialog.Document = this.PrintDocument;
                pageSetupDialog.ShowDialog();
            }

            private void ???????????ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                lock (m_lockValue)
                {
                    this.PrintDocument.Print();
                }
            }

            private void ?????????ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                lock (m_lockValue)
                {
                    this.SaveAs();
                }
            }

            protected virtual string OnPointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
            {
                return curve[iPt].Y.ToString("F2");
            }

            /// <summary>
            /// ??????? "??????? ????"-???????????????
            /// </summary>
            /// <param name="sender">ZedGraphControl</param>
            /// <param name="e">MouseEventArgs</param>
            /// <returns>??????? ??????????? ????????? ???????</returns>
            private bool OnDoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
            {
                //FormMain.formGraphicsSettings.SetScale();
                delegateSetScale();

                return true;
            }

            public abstract void Draw (TecView.valuesTEC []values, params object []pars);

            //protected void getColorZedGraph(HDateTime.INTERVAL id_time, out Color colChart, out Color colP)
            //{
            //    getColorZedGraph(m_tecView.m_arTypeSourceData[(int)id_time], out colChart, out colP);
            //}

            /// <summary>
            /// ????????? ????? ???????????
            /// </summary>
            /// <param name="typeConnSett">???????? ??????</param>
            /// <param name="colChart">???? ????</param>
            /// <param name="colP">???? ???????</param>
            protected void getColorZedGraph(CONN_SETT_TYPE typeConnSett, out Color colChart, out Color colP)
            {
                //???????? ?? ?????????
                colChart = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR_VAUES.BG_ASKUE);
                colP = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR_VAUES.ASKUE);

                if ((typeConnSett == CONN_SETT_TYPE.DATA_AISKUE)
                    || (typeConnSett == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    ; // ...?? ????????? 
                else
                    if ((typeConnSett == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                        || (typeConnSett == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN))
                    {
                        colChart = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR_VAUES.BG_SOTIASSO);
                        colP = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR_VAUES.SOTIASSO);
                    }
                    else
                        ;
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

                    this.SetFontColor (value);
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
                    base.BackColor =
                    //this.GraphPane.Fill.Color =
                        value;

                    this.GraphPane.Fill = new Fill (value == SystemColors.Control ? SystemColors.Window : value);
                    this.GraphPane.Chart.Fill.Color = value == SystemColors.Control ? SystemColors.Window : value;
                }
            }

            public virtual bool FindNearestObject (PointF p, Graphics g, out object obj, out int index)
            {
                return GraphPane.FindNearestObject(p, g, out obj, out index);
            }
        }

        /// <summary>
        /// ?????? ??? ???????? ????????? ??? ?????????? ????????? ??????????: ???????, ???????/???????????, ?????? ??????????? ??????
        /// </summary>
        protected int[] m_arPercRows = null; // [0] - ??? ???????, [1] - ??? ??????/??????????, ????????? - ?????? ??????????? ??????
        
        protected HPanelQuickData _pnlQuickData;

        protected System.Windows.Forms.SplitContainer stctrView;
        protected System.Windows.Forms.SplitContainer stctrViewPanel1, stctrViewPanel2;
        /// <summary>
        /// ??????????? ? ??????? "??? - ??????"
        /// </summary>
        protected HZedGraphControl m_ZedGraphMins;
        /// <summary>
        /// ??????????? ? ??????? "????? - ???"
        /// </summary>
        protected HZedGraphControl m_ZedGraphHours;
        /// <summary>
        /// ??????? ? ??????? "????? - ???"
        /// </summary>
        protected HDataGridViewBase m_dgwHours;
        /// <summary>
        /// ??????? ? ??????? "??? - ??????"
        /// </summary>
        protected HDataGridViewBase m_dgwMins;

        private
            //System.Threading.Timer
            System.Windows.Forms.Timer
                m_timerCurrent
                ;
        /// <summary>
        /// ??????? ??? ?????????? ???????? ??????? ?? ?????? ??????????? ??????????
        /// </summary>
        private DelegateObjectFunc delegateTickTime;
        /// <summary>
        /// ?????? ??? ?????? ? ?? (????????/?????????/????????? ??????????? ????????)
        /// </summary>
        public TecView m_tecView;

        public TEC.TEC_TYPE TecViewTecType
        {
            get
            {
                return m_tecView.TecType;
            }
        }

        public string TecViewNameShr
        {
            get
            {
                return m_tecView.NameShr;
            }
        }
        /// <summary>
        /// ?????????? ?????????? ????????(?????????? ?????? = 1 ???), ???????????? ? ??????? ???????? ??????????? ??????????
        ///  , ???????????? ???
        /// </summary>
        private int _currValuesPeriod;
        ///// <summary>
        ///// ?????? ???, ? ????????? ??????? ?????? ??????? ??????, ? ?????????? ?????? ???
        ///// </summary>
        //public int indx_TEC { get { return m_tecView.m_indx_TEC; } }
        ///// <summary>
        ///// ?????? ?????????? ???, ? ????????? ???????? ?????? ??????? ??????, ? ?????????? ?????? ??????????? ???
        ///// </summary>
        //public int indx_TECComponent { get { return m_tecView.indxTECComponents; } }
        /// <summary>
        /// ????????????? ??????? (???/????????? ???), ? ????????? ???????? ?????? ??????? ??????
        /// </summary>
        public FormChangeMode.KeyDevice TecViewKey { get { return m_tecView.CurrentKey; } }

        public FormChangeMode.MODE_TECCOMPONENT Mode
        {
            get
            {
                return m_tecView.CurrentKey.Mode;
            }
        }
        /// <summary>
        /// ??????? ???????? ????????? ?????? (?????????/??_?????????)
        /// </summary>
        private bool _update;

        protected virtual void InitializeComponent()
        {
            //this.m_pnlQuickData = new PanelQuickData(); ????????? ? ????????????

            createDataGridViewHours();
            createDataGridViewMins();

            ((System.ComponentModel.ISupportInitialize)(this.m_dgwHours)).BeginInit();
            if (!(this.m_dgwMins == null))
                ((System.ComponentModel.ISupportInitialize)(this.m_dgwMins)).BeginInit();
            else
                ;

            this._pnlQuickData.RestructControl();
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.Location = arPlacement[(int)CONTROLS.THIS].pt;
            this.Name = "pnlTecView";
            //this.Size = arPlacement[(int)CONTROLS.THIS].sz;
            this.TabIndex = 0;

            this._pnlQuickData.Dock = DockStyle.Fill;
            this._pnlQuickData.btnSetNow.Click += new System.EventHandler(this.btnSetNow_Click);
            this._pnlQuickData.dtprDate.ValueChanged += new System.EventHandler(this.dtprDate_ValueChanged);

            ((System.ComponentModel.ISupportInitialize)(this.m_dgwHours)).EndInit();
            if (!(this.m_dgwMins == null))
                ((System.ComponentModel.ISupportInitialize)(this.m_dgwMins)).EndInit();
            else
                ;

            createZedGraphControlMins(m_tecView.m_lockValue);
            createZedGraphControlHours(m_tecView.m_lockValue);
            this.m_ZedGraphHours.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.m_ZedGraphHours.OnMouseUpEvent);
            this.m_ZedGraphHours.EventItemSelected += new DelegateIntFunc(zedGraphHours_MouseUpEvent);

            this.stctrViewPanel1 = new System.Windows.Forms.SplitContainer();
            this.stctrViewPanel2 = new System.Windows.Forms.SplitContainer();

            this.stctrView = new System.Windows.Forms.SplitContainer();
            //this.stctrView.IsSplitterFixed = true;

            this._pnlQuickData.SuspendLayout();

            this.stctrViewPanel1.Panel1.SuspendLayout();
            this.stctrViewPanel1.Panel2.SuspendLayout();
            this.stctrViewPanel2.Panel1.SuspendLayout();
            this.stctrViewPanel2.Panel2.SuspendLayout();
            this.stctrViewPanel1.SuspendLayout();
            this.stctrViewPanel2.SuspendLayout();
            this.stctrView.Panel1.SuspendLayout();
            this.stctrView.Panel2.SuspendLayout();
            this.stctrView.SuspendLayout();

            this.SuspendLayout();

            // 
            // stctrView
            // 
            //this.stctrView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)
            //            | System.Windows.Forms.AnchorStyles.Right)));
            //this.stctrView.Location = arPlacement[(int)CONTROLS.stctrView].pt;
            this.stctrView.Dock = DockStyle.Fill;
            this.stctrView.Name = "stctrView";
            this.stctrView.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // stctrView.Panel1
            // 
            this.stctrViewPanel1.Dock = DockStyle.Fill;
            //this.stctrViewPanel1.SplitterDistance = 301;
            this.stctrViewPanel1.SplitterMoved += new SplitterEventHandler(stctrViewPanel1_SplitterMoved);
            // 
            // stctrView.Panel2
            // 
            this.stctrViewPanel2.Dock = DockStyle.Fill;
            //this.stctrViewPanel2.SplitterDistance = 291;
            this.stctrViewPanel2.SplitterMoved += new SplitterEventHandler(stctrViewPanel2_SplitterMoved);
            //this.stctrView.Size = arPlacement[(int)CONTROLS.stctrView].sz;
            //this.stctrView.SplitterDistance = 301;
            this.stctrView.TabIndex = 7;

            this._pnlQuickData.ResumeLayout(false);
            this._pnlQuickData.PerformLayout();

            this.stctrViewPanel1.Panel1.ResumeLayout(false);
            this.stctrViewPanel1.Panel2.ResumeLayout(false);
            this.stctrViewPanel2.Panel1.ResumeLayout(false);
            this.stctrViewPanel2.Panel2.ResumeLayout(false);
            this.stctrViewPanel1.ResumeLayout(false);
            this.stctrViewPanel2.ResumeLayout(false);
            this.stctrView.Panel1.ResumeLayout(false);
            this.stctrView.Panel2.ResumeLayout(false);
            this.stctrView.ResumeLayout(false);

            this.ResumeLayout(false);

            if (!(m_label == null))
            {
                m_label.Text = m_tecView.m_tec.name_shr;
                if (!(m_tecView.CurrentKey.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC))
                    m_label.Text += @" - " + m_tecView.m_tec.ListTECComponents.Find(comp => comp.m_id == m_tecView.CurrentKey.Id).name_shr;
                else
                    ;

                m_label.EventRestruct += new Action<LabelViewProperties> (OnEventRestruct);
                //m_label.PerformRestruct (
            }
            else
                ;
        }

        protected abstract void createTecView(FormChangeMode.KeyDevice key);

        protected abstract void createDataGridViewHours();
        protected abstract void createDataGridViewMins();
        
        protected abstract void createZedGraphControlHours(object objLock);
        protected abstract void createZedGraphControlMins(object objLock);

        protected abstract void createPanelQuickData();

        public PanelTecViewBase(/*TecView.TYPE_PANEL type, */TEC tec, FormChangeMode.KeyDevice key, HMark markQueries)
            : base (MODE_UPDATE_VALUES.AUTO, FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor)
        {
            //InitializeComponent();

            SPLITTER_PERCENT_VERTICAL = 50;

            createTecView(key); //m_tecView = new TecView(type, indx_tec, indx_comp);

            //HMark markQueries = new HMark(new int []{(int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR, (int)CONN_SETT_TYPE.DATA_AISKUE, (int)CONN_SETT_TYPE.DATA_SOTIASSO});

            m_tecView.InitTEC(new List<StatisticCommon.TEC>() { tec }, markQueries);
            //m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);

            m_tecView.setDatetimeView = new DelegateFunc(setNowDate);

            m_tecView.updateGUI_Fact = new IntDelegateIntIntFunc(updateGUI_Fact);
            m_tecView.updateGUI_TM_Gen = new DelegateFunc(updateGUI_TM_Gen);

            createPanelQuickData(); //???????????? ????? 'InitializeComponent'
            if (m_tecView.ListLowPointDev == null) //m_tecView.m_tec.m_bSensorsStrings == false
                m_tecView.m_tec.InitSensorsTEC();
            else
                ;

            AddTGView();

            if (tec.Type == TEC.TEC_TYPE.BIYSK)
                ; //this.parameters = FormMain.papar;
            else
                ;

            //??? ???????? ?.?. ?????????:
            // 1) FormMain.formGraphicsSettings.m_connSettType_SourceData = 
            // 2) ? ???????????? ? ?. 1 ????????? ???????? ??????? ????
            // 3) ? ???????????? ? ?. 2 ????????? ???????? m_tecView.m_arTypeSourceData[...
            //if (FormMain.formGraphicsSettings.m_connSettType_SourceData == CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE)
                //08.12.2014 - ???????? ?? ????????? - ??? ? ?????? ????
                m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] = 
                m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] = CONN_SETT_TYPE.DATA_AISKUE;
            //else
            //    m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] =
            //        m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] = FormMain.formGraphicsSettings.m_connSettType_SourceData;

                if ((!(_pnlQuickData.ContextMenuStrip == null))
                    && (_pnlQuickData.ContextMenuStrip.Items.Count > 1))
                    m_tecView.m_bLastValue_TM_Gen = ((ToolStripMenuItem)_pnlQuickData.ContextMenuStrip.Items[1]).Checked;
                else
                    ;
            // ?????????? ??????? ?????? ?? ?????????
            _update = false;

            delegateTickTime = new DelegateObjectFunc(tickTime);
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            m_tecView.SetDelegateReport(ferr, fwar, fact, fclr);
        }

        public override void Start()
        {
            base.Start ();

            m_tecView.Start();
            // ???????? ?? ?????????
            if (!(m_dgwMins == null))
                m_dgwMins.Fill();
            else
                ;
            m_dgwHours.Fill(m_tecView.m_curDate
                , m_tecView.m_valuesHours.Length
                , m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0);

            //DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
            //TimeSpan timezone_offset = TimeSpan.FromHours (m_tecView.m_tec.m_timezone_offset_msc);
            //timezone_offset = timezone_offset.Add(m_tecView.m_tsOffsetToMoscow);
            //if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight))
            //    timezone_offset = timezone_offset.Add(TimeSpan.FromHours(1));
            //else
            //    ;

            ////????? ?.?. ??? ???
            //_pnlQuickData.dtprDate.Value = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).Add(timezone_offset);
            _pnlQuickData.dtprDate.Value = HDateTime.ToMoscowTimeZone ();

            //initTableMinRows ();
            initTableHourRows ();

            ////??? ??????? ? 'Activate'
            ////? ??????????? ?? ????????????? ????????? ? ??????????? ????
            //// , ???????????? ??????? ???? ?????????: 1-??, 2-?? ?????
            //// , ???? ?????????? ????, ?? ??????????? ???? ??????
            //setTypeSourceData(HDateTime.INTERVAL.MINUTES, ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[m_ZedGraphMins.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);
            //setTypeSourceData(HDateTime.INTERVAL.HOURS, ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[m_ZedGraphHours.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);

            m_timerCurrent =
                //new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, 0, 1000)
                new System.Windows.Forms.Timer ()
                ;
            m_timerCurrent.Interval = MS_TIMER_CURRENT_UPDATE;
            m_timerCurrent.Tick += new EventHandler(TimerCurrent_Tick);
            m_timerCurrent.Start ();

            // ?????????? ??????? - ?????? ?? ?????????
            _update = false;
        }

        public override void Stop()
        {
            m_tecView.Stop ();

            //if (! (m_evTimerCurrent == null)) m_evTimerCurrent.Reset(); else ;
            if (!(m_timerCurrent == null)) { m_timerCurrent.Dispose(); m_timerCurrent = null; } else ;

            m_tecView.ReportClear(true);

            base.Stop();
        }

        protected override void initTableHourRows()
        {
            m_tecView.m_curDate = 
            m_tecView.serverTime = 
                _pnlQuickData.dtprDate.Value.Date;

            if (m_tecView.m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == false)
            {
                m_dgwHours.InitRows(24, false);
            }
            else
            {
                m_dgwHours.InitRows(25, true);
            }
        }

        protected void initTableMinRows()
        {
            if ((m_tecView.m_arTypeSourceData [(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                || (m_tecView.m_arTypeSourceData [(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO)
                || (m_tecView.m_arTypeSourceData [(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN))
                m_dgwMins.InitRows (20, false);
            else
                if (m_tecView.m_arTypeSourceData [(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN)
                    m_dgwMins.InitRows (60, true);
                else
                    ;

            m_dgwMins.Fill ();
        }

        public virtual void AddTGView()
        {
            foreach (TECComponentBase tg in m_tecView.ListLowPointDev)
                _pnlQuickData.AddTGView(tg);
        }

        private int getHeightItem (bool bUseLabel, int iRow) { return bUseLabel == true ? m_arPercRows[iRow] : m_arPercRows[iRow] + m_arPercRows[iRow + 1]; }

        protected void OnEventRestruct(LabelViewProperties propView)
        {
            //int[] propView = pars as int[];

            this.Controls.Clear();
            this.RowStyles.Clear();
            stctrView.Panel1.Controls.Clear();
            stctrView.Panel2.Controls.Clear();
            this.stctrViewPanel1.Panel1.Controls.Clear();
            this.stctrViewPanel2.Panel1.Controls.Clear();

            int iRow = 0
                , iPercTotal = 100
                , iPercItem = -1;
            bool bUseLabel = !(m_label == null);

            if (bUseLabel == true)
            {// ?????? ??? ??????? ? ????????
                this.Controls.Add(m_label, 0, iRow);
                iPercItem = m_arPercRows[iRow];
                iPercTotal -= iPercItem;
                this.RowStyles.Add(new RowStyle(SizeType.Percent, m_arPercRows[iRow++]));
            }
            else
                ;
            //// ???????????????? ?????? ? ??????? ??? ???????? ? ?????????? ????????
            //// , ?? ~ ?? ???? ???????????? ?? 'm_label'
            //iRow++;

            if (propView.GetValue(LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION) == VALUE.DISABLED)
            {
                //?????????? ?????? ???? ???????
                bool bVisible = true;
                if (propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_MINS) == VALUE.ON)
                    this.Controls.Add(m_dgwMins, 0, iRow);
                else
                    if (propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_HOURS) == VALUE.ON)
                        this.Controls.Add(m_dgwHours, 0, iRow);
                    else
                        if (propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS) == VALUE.ON)
                            this.Controls.Add(m_ZedGraphMins, 0, iRow);
                        else
                            if (propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_HOURS) == VALUE.ON)
                                this.Controls.Add(m_ZedGraphHours, 0, iRow);
                            else
                                bVisible = false;

                if (bVisible == true)
                {
                    iPercItem = getHeightItem (bUseLabel, iRow);
                    iPercTotal -= iPercItem;
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, iPercItem));
                    iRow++;
                }
                else
                    ;
            }
            else
            { //?????????? ??? ??? ?????? ????????
                if ((propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_MINS) == VALUE.ON)
                    && (propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_HOURS) == VALUE.ON)
                    && (propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS) == VALUE.ON)
                    && (propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_HOURS) == VALUE.ON))
                { //?????????? 4 ???????? (???????(???) + ???????(???) + ??????(???) + ??????(???))
                }
                else
                { //?????????? ??? ????????
                    if (propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION) == VALUE.OFF)
                    {
                        stctrView.Orientation = Orientation.Vertical;

                        stctrView.SplitterDistance = stctrView.Width / (100 / (int)SPLITTER_PERCENT_VERTICAL);
                    }
                    else
                    {
                        stctrView.Orientation = Orientation.Horizontal;

                        stctrView.SplitterDistance = stctrView.Height / 2;
                    }

                    if ((propView.IsOn (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_MINS) == true)
                        && (propView.IsOn (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_HOURS) == true)
                        && (propView.IsOff (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS) == true)
                        && (propView.IsOff (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_HOURS) == true))
                    { //?????????? 2 ???????? (???????(???) + ???????(???))
                        stctrView.Panel1.Controls.Add(m_dgwMins);
                        stctrView.Panel2.Controls.Add(m_dgwHours);
                    }
                    else
                    {
                        if ((propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_MINS) == VALUE.OFF)
                            && (propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_HOURS) == VALUE.OFF)
                            && (propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS) == VALUE.ON)
                            && (propView.GetValue (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_HOURS) == VALUE.ON))
                        { //?????????? 2 ???????? (??????(???) + ??????(???))
                            stctrView.Panel1.Controls.Add(m_ZedGraphMins);
                            stctrView.Panel2.Controls.Add(m_ZedGraphHours);
                        }
                        else
                        {
                            if ((propView.IsOn (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_MINS) == true)
                                && (propView.IsOff (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_HOURS) == true)
                                && (propView.IsOn (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS) == true)
                                && (propView.IsOff (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_HOURS) == true))
                            { //?????????? 2 ???????? (???????(???) + ??????(???))
                                stctrView.Panel1.Controls.Add(m_dgwMins);
                                stctrView.Panel2.Controls.Add(m_ZedGraphMins);
                            }
                            else
                            {
                                if ((propView.IsOff (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_MINS) == true)
                                    && (propView.IsOn (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_HOURS) == true)
                                    && (propView.IsOff(LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS) == true)
                                    && (propView.IsOn (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_HOURS) == true))
                                { //?????????? 2 ???????? (???????(???) + ??????(???))
                                    stctrView.Panel1.Controls.Add(m_dgwHours);
                                    stctrView.Panel2.Controls.Add(m_ZedGraphHours);
                                }
                                else
                                {
                                    if ((propView.IsOff (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_MINS) == true)
                                        && (propView.IsOn (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_HOURS) == true)
                                        && (propView.IsOn (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS) == true)
                                        && (propView.IsOff (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_HOURS) == true))
                                    { //?????????? 2 ???????? (???????(???) + ??????(???))
                                        stctrView.Panel1.Controls.Add(m_dgwHours);
                                        stctrView.Panel2.Controls.Add(m_ZedGraphMins);
                                    }
                                    else
                                    {
                                        if ((propView.IsOn (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_MINS) == true)
                                            && (propView.IsOff (LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_HOURS) == true)
                                            && (propView.IsOff (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_MINS) == true)
                                            && (propView.IsOn (LabelViewProperties.INDEX_PROPERTIES_VIEW.GRAPH_HOURS) == true))
                                        { //?????????? 2 ???????? (???????(???) + ??????(???))
                                            stctrView.Panel1.Controls.Add(m_dgwMins);
                                            stctrView.Panel2.Controls.Add(m_ZedGraphHours);
                                        }
                                        else
                                        {
                                        }
                                    }
                                }
                            }
                        }
                    }

                    this.Controls.Add(this.stctrView, 0, iRow);
                    iPercItem = getHeightItem (bUseLabel, iRow);
                    iPercTotal -= iPercItem;
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, iPercItem));
                    iRow++;
                }

                switch (propView.GetValue(LabelViewProperties.INDEX_PROPERTIES_VIEW.TABLE_AND_GRAPH))
                {
                    case VALUE.DISABLED: //??????? ? ?????? ? ???????????? ??????????? ?? ????? ???? ????????? ? ????? 'SplitContainer'
                        break;
                    case VALUE.OFF:
                        break;
                    case VALUE.ON:
                        break;
                    default:
                        break;
                }
            }

            if (propView.GetValue(LabelViewProperties.INDEX_PROPERTIES_VIEW.QUICK_PANEL) == VALUE.ON)
            {
                this.Controls.Add(_pnlQuickData, 0, iRow);
                _pnlQuickData.ShowFactValues();
                _pnlQuickData.ShowTMValues();
            }
            else
            {
            }

            this.RowStyles.Add(new RowStyle(SizeType.Percent, iPercTotal));
        }

        private void updateGUI_TM_Gen()
        {
            if (IsHandleCreated/*InvokeRequired*/ == true)
                this.BeginInvoke(new DelegateFunc(showTM_Gen));
            else
                Logging.Logg().Error(@"PanelTecViewBase::updateGUI_TM_Gen () - ... BeginInvoke (UpdateGUI_TM_Gen) - ... ID = " + m_tecView.CurrentKey, Logging.INDEX_MESSAGE.D_001);
        }

        private void showTM_Gen()
        {
            lock (m_tecView.m_lockValue)
            {
                _pnlQuickData.ShowTMValues();
            }
        }

        private int updateGUI_Fact(int hour, int min)
        {
            int iRes = (int)ASUTP.Helper.HHandler.INDEX_WAITHANDLE_REASON.SUCCESS;
            
            if (IsHandleCreated/*InvokeRequired*/ == true)
                this.BeginInvoke(new DelegateIntIntFunc(show_Fact), hour, min);
            else
                Logging.Logg().Error(@"PanelTecViewBase::updateGUI_Fact () - ... BeginInvoke (UpdateGUI_Fact) - ... ID = " + m_tecView.CurrentKey, Logging.INDEX_MESSAGE.D_001);

            return iRes;
        }

        protected virtual void show_Fact(int hour, int min)
        {
            lock (m_tecView.m_lockValue)
            {
                try
                {
                    FillGridHours();

                    FillGridMins(hour);

                    _pnlQuickData.ShowFactValues();

                    DrawGraphMins(hour);
                    DrawGraphHours();
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTecViewBase::UpdateGUI_Fact () - ... ID = " + m_tecView.CurrentKey, Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }

        private void FillGridMins(int hour)
        {
            if (!(m_dgwMins == null))
                m_dgwMins.Fill(m_tecView.m_valuesMins
                    , hour, m_tecView.lastMin);
            else
                ;

            //Logging.Logg().Debug(@"PanelTecViewBase::FillGridMins () - ...");
        }

        private void FillGridHours()
        {
            // ???????? ?? ?????????
            m_dgwHours.Fill(m_tecView.m_curDate
                , m_tecView.m_valuesHours.Length
                , m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0);
            // ???????? ????????
            m_dgwHours.Fill(m_tecView.m_valuesHours
                , m_tecView.lastHour
                , m_tecView.lastReceivedHour
                , m_tecView.m_valuesHours.Length
                , m_tecView.m_tec.m_id
                , m_tecView.currHour
                , m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE
                , m_tecView.serverTime);

            //Logging.Logg().Debug(@"PanelTecViewBase::FillGridHours () - ...");
        }
        
        /// <summary>
        /// ??????????? ?????? ?????? ? ????????? ????? (? ???? ??????? ?????????? ???? ??????????? ?????????? ????????)
        /// </summary>
        protected void NewDateRefresh()
        {
            //Debug.WriteLine(@"PanelTecViewBase::NewDateRefresh () - m_tecView.currHour=" + m_tecView.currHour.ToString ());

            //delegateStartWait ();
            if (!(delegateStartWait == null)) delegateStartWait(); else ;
            
            //14.04.2015 ???
            if (m_tecView.currHour == true)
            {
                //// ????????? ?-?
                //changeState();
                // ????????? ???????? ?? ?-??
                m_tecView.m_curDate = _pnlQuickData.dtprDate.Value;
                m_tecView.ChangeState();
            }
            else
                m_tecView.GetRetroValues();

            //delegateStopWait ();
            if (!(delegateStopWait == null)) delegateStopWait(); else ;
        }
        
        /// <summary>
        /// ?????????? ??????? - ????????? ???? ?? ????????? ?? ?????? ??????????? ??????????
        /// </summary>
        /// <param name="sender">??????, ?????????????? ??????? (????????? ?? ?????? ??????????? ??????????)</param>
        /// <param name="e">???????? ???????</param>
        private void dtprDate_ValueChanged(object sender, EventArgs e)
        {
            //Debug.WriteLine(@"PanelTecViewBase::dtprDate_ValueChanged () - DATE_pnlQuickData=" + _pnlQuickData.dtprDate.Value.ToString() + @", update=" + update);

            if (_update == true)
            {
                //?????????? ????/????? ????
                if (!(_pnlQuickData.dtprDate.Value.Date.CompareTo (m_tecView.m_curDate.Date) == 0))
                    m_tecView.currHour = false;

                else
                    ;

                //? ???? ?????? ????/????? ???????????? ???
                initTableHourRows ();

                NewDateRefresh();

                //setRetroTickTime(m_tecView.lastHour, (m_tecView.lastMin - 1) * m_tecView.GetIntervalOfTypeSourceData (HDateTime.INTERVAL.MINUTES));
                setRetroTickTime(m_tecView.lastHour, 60);
            }
            else
                _update = true;
        }

        private void setNowDate()
        {
            //true, ?.?. ?????? ????? ??? result=true
            if (IsHandleCreated/*InvokeRequired*/ == true)
                this.BeginInvoke (new DelegateBoolFunc (setNowDate), true);
            else
                Logging.Logg().Error(@"PanelTecViewBase::setNowDate () - ... BeginInvoke (SetNowDate) - ...", Logging.INDEX_MESSAGE.D_001);
        }

        /// <summary>
        /// ?????????? ????????? ???? ?????? ??????? ????
        /// </summary>
        /// <param name="recieved">??????? ????????? ?????? ??? ???????????</param>
        protected void setNowDate(bool recieved)
        {
            m_tecView.currHour = true;

            if (recieved == true)
            {// ?????? ????????
                // ?????????? ??????? - ?????? ?? ?????????
                _update = false;
                // ???????????? ?????? ?????? ? ????? ?????
                _pnlQuickData.dtprDate.Value = m_tecView.m_curDate/*.Add(m_tecView.m_tsOffsetToMoscow)*/;
            }
            else
            {// ?????? ?? ????????
                // ????????? ?????? ??? ???????????
                NewDateRefresh();
            }
        }

        /// <summary>
        /// ?????????? ??????? - ?????? ?????? "??????? ???"
        /// </summary>
        /// <param name="sender">??????, ?????????????? ??????? (??????)</param>
        /// <param name="e">???????? ??????? (??????)</param>
        private void btnSetNow_Click(object sender, EventArgs e)
        {
            m_tecView.currHour = true;
            NewDateRefresh();
        }
        
        /// <summary>
        /// ?????????? ??????? - ????????? ??????? ????? ????? ?????? "????"
        ///  ????? ????????? (????/??????) ?? ???????????
        /// </summary>
        /// <param name="index">?????? ?????????</param>
        protected void zedGraphHours_MouseUpEvent(int index)
        {
            bool bRetroHour = false;

            if (!(m_tecView == null))
            {
                //if (m_tecView.lastReceivedHour > 0) {
                    if (!(delegateStartWait == null)) delegateStartWait(); else ;

                    bRetroHour = m_tecView.zedGraphHours_MouseUpEvent(index);

                    if (bRetroHour == true)
                        setRetroTickTime(m_tecView.lastHour, 60);
                    else
                    {
                        ////??????? ?1
                        //setNowDate(false);

                        //??????? ?2
                        m_tecView.currHour = true;
                        NewDateRefresh();
                    }

                    if (!(delegateStopWait == null)) delegateStopWait(); else ;
                //} else ;
            }
            else
                ;
        }

        protected bool timerCurrentStarted
        {
            get { return ! (m_timerCurrent == null); }
        }

        public override bool Activate(bool active)
        {
            int err = 0;
            bool bRes = false;

            if ((timerCurrentStarted == true)
                && (!(Actived == active)))
            {
                bRes = base.Activate (active);

                if (Actived == true)
                {
                    _currValuesPeriod = 0;

                    ////??? ??????? ? 'Activate'
                    ////??? ??????? ? 'enabledDataSource_...'
                    ////? ??????????? ?? ????????????? ????????? ? ??????????? ????
                    //// , ???????????? ??????? ???? ?????????: 1-??, 2-?? ?????
                    //// , ???? ?????????? ????, ?? ??????????? ???? ??????
                    //setTypeSourceData(HDateTime.INTERVAL.MINUTES, ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[m_ZedGraphMins.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);
                    //setTypeSourceData(HDateTime.INTERVAL.HOURS, ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[m_ZedGraphHours.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);

                    HMark markSourceData = enabledSourceData_ToolStripMenuItems();

                    if (m_tecView.currHour == true)
                        NewDateRefresh();
                    else
                    {
                        updateGraphicsRetro(markSourceData);
                    }

                    _pnlQuickData.OnSizeChanged(null, EventArgs.Empty);

                    m_timerCurrent.Interval = MS_TIMER_CURRENT_UPDATE;
                    m_timerCurrent.Start ();
                }
                else
                {
                    m_tecView.ClearStates();

                    if (!(m_timerCurrent == null))
                        //m_timerCurrent.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        m_timerCurrent.Stop ();
                    else
                        ;
                }
            }
            else
            {
                err = -1; //?????????

                Logging.Logg().Warning(string.Format(@"PanelTecViewBase::Activate ({0}) - ????????? ????????? ???????? ??????????... ID={1}, Started={2}, isActive={3}"
                        , active, TecViewKey, Started, Actived)
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }

            return bRes;
        }

        protected void setRetroTickTime(int hour, int min)
        {
            DateTime dt = _pnlQuickData.dtprDate.Value.Date;
            dt = dt.AddHours(hour);
            dt = dt.AddMinutes(min);

            Debug.WriteLine(string.Format (@"PanelTecViewBase::setRetroTickTime (hour={0}, minute={1}) - ...", hour, min));

            if (IsHandleCreated == true)
                if (InvokeRequired == true)
                    Invoke(delegateTickTime, dt);
                else
                    tickTime(dt/*.Add(m_tecView.m_tsOffsetToMoscow)*/);
            else
                return;
        }

        /// <summary>
        /// ??????? ?????????? ???? '????? ???????'
        /// </summary>
        /// <param name="dt">????/????? ??? ???????????</param>
        private void tickTime(object dt)
        {
            _pnlQuickData.lblServerTime.Text = ((DateTime)dt).ToString("HH:mm:ss");
        }

        /// <summary>
        /// ????? ????????? ?????? ??????? 'timerCurrent'
        /// </summary>
        /// <param name="stateInfo">????? ?????????????</param>
        //private void TimerCurrent_Tick(Object stateInfo)
        private void TimerCurrent_Tick(object obj, EventArgs ev)
        {
            if ((m_tecView.currHour == true) && (Actived == true))
            {
                m_tecView.serverTime = m_tecView.serverTime.AddSeconds(1);

                if (IsHandleCreated == true)
                    if (InvokeRequired == true)
                        Invoke(delegateTickTime, m_tecView.serverTime);
                    else
                        tickTime(m_tecView.serverTime/*.Add(m_tecView.m_tsOffsetToMoscow)*/);
                else
                    return;

                //if (!(((currValuesPeriod++) * 1000) < Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]) * 1000))
                if (!(_currValuesPeriod++ < POOL_TIME * (m_tecView.PeriodMultiplier)))
                {
                    _currValuesPeriod = 0;
                    NewDateRefresh();
                }
                else
                    ;
            }
            else
                ;

            //((ManualResetEvent)stateInfo).WaitOne();
            //try
            //{
            //    timerCurrent.Change(1000, Timeout.Infinite);
            //}
            //catch (Exception e)
            //{
            //    Logging.Logg().Exception(e, "????????? ? ?????????? 'timerCurrent'", Logging.INDEX_MESSAGE.NOT_SET);
            //}
        }

        private void DrawGraphMins(int hour)
        {
            int iForeignCommand = -1;
            double [] pbrValues;

            if (!(m_ZedGraphMins == null))
            {
                if (!(hour < m_tecView.m_valuesHours.Length))
                    hour = m_tecView.m_valuesHours.Length - 1;
                else
                    ;

                if ((!(Equals (m_tecView.m_valuesHours, null) == false))
                    && (!(m_tecView.lastHour < 0))
                    && (m_tecView.lastHour < m_tecView.m_valuesHours.Length))
                    // ??????? ?? ?? ???????(?????????) ???
                    iForeignCommand = m_tecView.m_valuesHours [m_tecView.lastHour].valuesForeignCommand == true ? 1 : 0;
                else
                    ;

                if (iForeignCommand == 0)
                    // ???????? ??? ?? ?????????? ? ??????? ????
                    pbrValues = new double [] { m_tecView.m_valuesHours[m_tecView.lastHour > 0 ? m_tecView.lastHour - 1 : m_tecView.lastHour].valuesPBR
                        , m_tecView.m_valuesHours [m_tecView.lastHour].valuesPBR };
                else
                    pbrValues = new double [] { double.MinValue, double.MinValue };

                m_ZedGraphMins.Draw(m_tecView.m_valuesMins
                    , new object[] {
                        m_tecView.currHour
                        , m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES]
                        , m_tecView.lastMin
                        , m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0
                        , (IntDelegateIntFunc)m_tecView.GetSeasonHourOffset
                        , hour
                        , m_tecView.adminValuesReceived
                        , pbrValues
                        , m_tecView.recomendation
                    }
                );
            }
            else
                ;
        }

        protected void DrawGraphHours()
        {
            m_ZedGraphHours.Draw(m_tecView.m_valuesHours
                , new object [] {
                    m_tecView.currHour
                    , m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS]
                    , m_tecView.lastHour
                    , m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0
                    , (IntDelegateIntFunc)m_tecView.GetSeasonHourOffset
                    , m_tecView.serverTime.Add (m_tecView.m_tsOffsetToMoscow)
                    , _pnlQuickData.dtprDate.Value.ToShortDateString()
                }
            );
        }

        protected abstract HMark enabledSourceData_ToolStripMenuItems();
        
        /// <summary>
        /// ?????????? ??????????? ??????? ? ????????? ????????? ????????? ??????
        /// </summary>
        /// <param name="markUpdate">????????? ?? ???????????? ????????? ??????</param>
        protected void updateGraphicsRetro (HMark markUpdate)
        {
            //if (markUpdate.IsMarked() == false)
            //    return;
            //else
            if ((markUpdate.IsMarked((int)HDateTime.INTERVAL.MINUTES) == true) && (markUpdate.IsMarked((int)HDateTime.INTERVAL.HOURS) == false))
                //????????? ????????? ?????? ??????
                m_tecView.GetRetroMins();
            else
                if ((markUpdate.IsMarked((int)HDateTime.INTERVAL.MINUTES) == false) && (markUpdate.IsMarked((int)HDateTime.INTERVAL.HOURS) == true))
                    //????????? ????????? ?????? ???
                    m_tecView.GetRetroHours();
                else
                    if ((markUpdate.IsMarked((int)HDateTime.INTERVAL.MINUTES) == true) && (markUpdate.IsMarked((int)HDateTime.INTERVAL.HOURS) == true))
                        //????????? ????????? ?????? ???, ??????
                        m_tecView.GetRetroValues();
                    else
                        ;
        }

        /// <summary>
        /// ????? ????????????????? ?????????? ?????????? ???????????? ????????????? ??????
        /// </summary>
        /// <param name="type">??? ???????????? ??????????</param>
        public override void UpdateGraphicsCurrent(int type)
        {
            lock (m_tecView.m_lockValue)
            {
                //??? ???????? 'type' TYPE_UPDATEGUI
                HMark markChanged = enabledSourceData_ToolStripMenuItems ();
                if (markChanged.IsMarked () == false) {
                    DrawGraphMins(m_tecView.lastHour);
                    DrawGraphHours();
                } else {
                    if (m_tecView.currHour == true)
                        NewDateRefresh();
                    else
                    {//m_tecView.currHour == false
                        updateGraphicsRetro(markChanged);
                    }
                }
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

                if (Equals (_pnlQuickData, null) == false)
                    _pnlQuickData.ForeColor = value;
                else
                    ;
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

                if (Equals (_pnlQuickData, null) == false) {                    
                    _pnlQuickData.BackColor = value;
                } else
                    ;
            }
        }

        private void stctrViewPanel1_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }

        private void stctrViewPanel2_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }

        private void sourceData_Click(ContextMenuStrip cms, ToolStripMenuItem sender, HDateTime.INTERVAL indx_time)
        {
            CONN_SETT_TYPE prevTypeSourceData = m_tecView.m_arTypeSourceData[(int)indx_time]
                , curTypeSourceData = prevTypeSourceData;

            if (sender.Checked == false)
            {
                HZedGraphControl.INDEX_CONTEXTMENU_ITEM indx
                    , indxChecked = HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT;
                for (indx = HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx++)
                    if (sender.Equals(cms.Items[(int)indx]) == true) {
                        indxChecked = indx;
                        ((ToolStripMenuItem)cms.Items[(int)indxChecked]).Checked = true;
                        
                        break;
                    }
                    else
                        ;

                if (! (indxChecked == HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT))
                {
                    for (indx = HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx++)
                        if (! (indx == indxChecked))
                            ((ToolStripMenuItem)cms.Items[(int)indx]).Checked = false;
                        else
                            ;

                    switch (indxChecked) {
                        case HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO:
                            curTypeSourceData = CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO;
                            break;
                        case HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE:
                            curTypeSourceData = CONN_SETT_TYPE.DATA_AISKUE;
                            break;
                        case HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN:
                            curTypeSourceData = CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN;
                            break;
                        case HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN:
                            curTypeSourceData = CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN;
                            break;
                        default:
                            indx = HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT;
                            break;
                    }

                    m_tecView.m_arTypeSourceData[(int)indx_time] = curTypeSourceData;

                    if (indx_time == HDateTime.INTERVAL.MINUTES)
                    {
                        bool bInitTableMinRows = true;

                        switch (prevTypeSourceData)
                        {
                            case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                                switch (m_tecView.m_arTypeSourceData[(int)indx_time])
                                {
                                    //case CONN_SETT_TYPE.DATA_ASKUE_PLUS_SOTIASSO:
                                    //    break;
                                    case CONN_SETT_TYPE.DATA_AISKUE:
                                        bInitTableMinRows = false;
                                        break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                                        bInitTableMinRows = false;
                                        break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                                        //bInitTableMinRows = true;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case CONN_SETT_TYPE.DATA_AISKUE:
                                switch (m_tecView.m_arTypeSourceData[(int)indx_time])
                                {
                                    case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                                        bInitTableMinRows = false;
                                        break;
                                    //case CONN_SETT_TYPE.DATA_ASKUE:
                                    //    break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                                        bInitTableMinRows = false;
                                        break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                                        //bInitTableMinRows = true;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                                switch (m_tecView.m_arTypeSourceData[(int)indx_time])
                                {
                                    case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                                        bInitTableMinRows = false;
                                        break;
                                    case CONN_SETT_TYPE.DATA_AISKUE:
                                        bInitTableMinRows = false;
                                        break;
                                    //case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                                    //    break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                                        //bInitTableMinRows = true;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                                switch (m_tecView.m_arTypeSourceData[(int)indx_time])
                                {
                                    case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                                        //bInitTableMinRows = true;
                                        break;
                                    case CONN_SETT_TYPE.DATA_AISKUE:
                                        //bInitTableMinRows = true;
                                        break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                                        //bInitTableMinRows = true;
                                        break;
                                    //case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                                    //    break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;
                        }

                        if (bInitTableMinRows == true)
                            initTableMinRows();
                        else
                            ;
                    }
                    else
                        ;

                    if (m_tecView.currHour == true)
                        NewDateRefresh();
                    else
                    {//m_tecView.currHour == false
                        if (indx_time == HDateTime.INTERVAL.MINUTES)
                            m_tecView.GetRetroMins();
                        else
                            m_tecView.GetRetroHours();
                    }
                }
                else
                    ; //?? ?????? ?? ???? ?????????? ????? ??????????? ????
            }
            else
                ; //????????? ???

            //if (enabledSourceData_ToolStripMenuItems () == true) {
            //    NewDateRefresh ();
            //}
            //else
            //    ;
        }

        protected void sourceDataMins_Click(object sender, EventArgs e)
        {
            sourceData_Click(m_ZedGraphMins.ContextMenuStrip, (ToolStripMenuItem)sender, HDateTime.INTERVAL.MINUTES);
        }

        protected void sourceDataHours_Click(object sender, EventArgs e)
        {
            sourceData_Click(m_ZedGraphHours.ContextMenuStrip, (ToolStripMenuItem)sender, HDateTime.INTERVAL.HOURS);
        }
    }
}
