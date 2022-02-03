using System;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

//using HClassLibrary;
using StatisticCommon;
using ASUTP;
using ASUTP.Core;

namespace Statistic
{
    /// <summary>
    /// Частичный класс  "Панель ТЭЦ основная"
    /// </summary>
    partial class PanelTecViewBase
    {
        /// <summary>
        /// Защищенный частичный класс "Макет таблицы"
        /// </summary>
        protected partial class HPanelTableLayout : TableLayoutPanel
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public HPanelTableLayout(Color foreColor, Color backColor)
            {
                //Свойство возвращает или задает язык и региональные параметры для текущего потока
                Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                    ASUTP.Helper.ProgramBase.ss_MainCultureInfo;
                
                BackColor = backColor;
                ForeColor = foreColor;

                InitializeComponent ();
            }

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="container">контейнер</param>
            public HPanelTableLayout(IContainer container)
            {
                container.Add(this);

                InitializeComponent();
            }

            /// <summary>
            /// Возвратить объект "шрифт" для подписи, указанного в аргументе типа
            /// </summary>
            /// <param name="typeLabel">Тип подписи</param>
            /// <returns>Объект "шрифт" для подписи указанного типа</returns>
            public Font GetFontHLabel(ASUTP.Control.HLabel.TYPE_HLABEL typeLabel)
            {
                Font fontRes = null;
                //Максимальная длина текста
                string textOfMaxLengths = string.Empty;
                string strValue = string.Empty;
                SizeF szLabelOfMinSizes = new SizeF(); //(float.MaxValue, float.MaxValue);

                Graphics g = this.CreateGraphics();

                textOfMaxLengths = string.Empty;
                szLabelOfMinSizes.Height =
                szLabelOfMinSizes.Width =
                    float.MaxValue;
                //Для каждого элемента управления
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is ASUTP.Control.HLabel)
                    {
                        if (typeLabel == (ctrl as ASUTP.Control.HLabel).m_type)
                        {
                            if (textOfMaxLengths.Length < ctrl.Text.Length)
                            {
                                switch (typeLabel)
                                {
                                    case ASUTP.Control.HLabel.TYPE_HLABEL.TG:
                                        if (ctrl.Text.LongCount(delegate (char ch) { return ch == '-'; }) > 1)
                                            strValue = new string('8', (int)ctrl.Text.LongCount(delegate (char ch) { return ch == '-'; }));
                                        else
                                            strValue = ctrl.Text;
                                        break;
                                    case ASUTP.Control.HLabel.TYPE_HLABEL.TOTAL:
                                        if (ctrl.Text.LongCount(delegate (char ch) { return ch == '-'; }) > 1)
                                            strValue = new string('8', ctrl.Text.Length + 3);
                                        else
                                            strValue = ctrl.Text;
                                        break;
                                    case ASUTP.Control.HLabel.TYPE_HLABEL.TOTAL_ZOOM:
                                        if (ctrl.Text.LongCount(delegate (char ch) { return ch == '-'; }) > 1)
                                            strValue = new string('8', ctrl.Text.Length + 6);
                                        else
                                            strValue = ctrl.Text;
                                        break;
                                    default:
                                        strValue = ctrl.Text;
                                        break;
                                }

                                if (textOfMaxLengths.Length < strValue.Length)
                                    textOfMaxLengths = strValue;
                                else
                                    ;
                            }
                            else
                                ;

                            if ((szLabelOfMinSizes.Height > ctrl.Height)
                                || (szLabelOfMinSizes.Width > ctrl.Width))
                            {
                                szLabelOfMinSizes.Height = ctrl.Height;
                                szLabelOfMinSizes.Width = ctrl.Width;
                            }
                            else
                                ;
                        }
                        else
                        {
                            Logging.Logg().Error(@"HPanelTableLayout::GetFontHLabel () - type=UNKNOWN, ctrl.Text=" + ctrl.Text, Logging.INDEX_MESSAGE.NOT_SET);

                            break;
                        }
                    }
                    else
                    {
                    }
                }

                if ((szLabelOfMinSizes.Height < float.MaxValue) && (szLabelOfMinSizes.Width < float.MaxValue))
                {
                    fontRes = ASUTP.Control.HLabel.FitFont(g, textOfMaxLengths, szLabelOfMinSizes, new SizeF(0.85F, 0.85F), 0.1F);
                }
                else
                    ;

                return fontRes;
            }

            public Font[] GetFontHLabel()
            {
                string[] textOfMaxLengths = new string[(int)ASUTP.Control.HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL] { string.Empty, string.Empty, string.Empty };
                string strValue = string.Empty;
                SizeF[] szLabelOfMinSizes = new SizeF[(int)ASUTP.Control.HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL]; //(float.MaxValue, float.MaxValue);

                Font[] arFontRes = null;

                Graphics g = this.CreateGraphics();

                int indx = -1, i = -1;

                for (i = (int)ASUTP.Control.HLabel.TYPE_HLABEL.TG; i < (int)ASUTP.Control.HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL; i++)
                {
                    textOfMaxLengths[i] = string.Empty;
                    szLabelOfMinSizes[i].Height =
                    szLabelOfMinSizes[i].Width =
                        float.MaxValue;
                }

                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is ASUTP.Control.HLabel)
                    {
                        indx = (int)((ASUTP.Control.HLabel)ctrl).m_type;
                        if (!(indx == (int)ASUTP.Control.HLabel.TYPE_HLABEL.UNKNOWN))
                        {
                            if (textOfMaxLengths[indx].Length < ctrl.Text.Length)
                            {
                                switch (indx)
                                {
                                    case (int)ASUTP.Control.HLabel.TYPE_HLABEL.TG:
                                        if (ctrl.Text.LongCount(delegate (char ch) { return ch == '-'; }) > 1)
                                            strValue = new string('8', (int)ctrl.Text.LongCount(delegate (char ch) { return ch == '-'; }));
                                        else
                                            strValue = ctrl.Text;
                                        break;
                                    case (int)ASUTP.Control.HLabel.TYPE_HLABEL.TOTAL:
                                        if (ctrl.Text.LongCount(delegate (char ch) { return ch == '-'; }) > 1)
                                            strValue = new string('8', ctrl.Text.Length + 3);
                                        else
                                            strValue = ctrl.Text;
                                        break;
                                    case (int)ASUTP.Control.HLabel.TYPE_HLABEL.TOTAL_ZOOM:
                                        if (ctrl.Text.LongCount(delegate (char ch) { return ch == '-'; }) > 1)
                                            strValue = new string('8', ctrl.Text.Length + 6);
                                        else
                                            strValue = ctrl.Text;
                                        break;
                                    default:
                                        strValue = ctrl.Text;
                                        break;
                                }

                                if (textOfMaxLengths[indx].Length < strValue.Length)
                                    textOfMaxLengths[indx] = strValue;
                                else
                                    ;
                            }
                            else
                                ;
                            if ((szLabelOfMinSizes[indx].Height > ctrl.Height) || (szLabelOfMinSizes[indx].Width > ctrl.Width)) { szLabelOfMinSizes[indx].Height = ctrl.Height; szLabelOfMinSizes[indx].Width = ctrl.Width; } else;
                        }
                        else
                        {
                            Logging.Logg().Error(@"HPanelTableLayout::GetFontHLabel () - type=UNKNOWN, ctrl.Text=" + ctrl.Text, Logging.INDEX_MESSAGE.NOT_SET);

                            break;
                        }
                    }
                    else
                    {
                    }
                }

                if (!(indx == (int)ASUTP.Control.HLabel.TYPE_HLABEL.UNKNOWN))
                {
                    arFontRes = new Font[(int)ASUTP.Control.HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL];

                    for (i = (int)ASUTP.Control.HLabel.TYPE_HLABEL.TG; i < (int)ASUTP.Control.HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL; i++)
                        if ((szLabelOfMinSizes[i].Height < float.MaxValue) && (szLabelOfMinSizes[i].Width < float.MaxValue))
                        {
                            arFontRes[i] = ASUTP.Control.HLabel.FitFont(g, textOfMaxLengths[i], szLabelOfMinSizes[i], new SizeF(0.85F, 0.85F), 0.1F);
                        }
                        else
                            ;
                }
                else
                    ; //Logging.Logg ().Error (@"HPanelTableLayout::GetFontHLabel () - type=UNKNOWN");

                return arFontRes;
            }

            /// <summary>
            /// Обработчик события изменеия размера панели
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            public void OnSizeChanged(object obj, EventArgs ev)
            {
                Font[] fonts = GetFontHLabel();

                if (obj == null)
                    obj = this;
                else
                    ;

                if (!(fonts == null))
                    foreach (Control ctrl in ((TableLayoutPanel)obj).Controls)
                        if (ctrl is ASUTP.Control.HLabel)
                            if (!(fonts [(int)((ASUTP.Control.HLabel)ctrl).m_type] == null)) {
                                ctrl.Font = fonts [(int)((ASUTP.Control.HLabel)ctrl).m_type];
                                //ctrl.ForeColor = ForeColor;
                            } else
                                Logging.Logg ().Error (@"HPanelTableLayout::OnSizeChanged () - fonts[" + ((ASUTP.Control.HLabel)ctrl).m_type.ToString () + @"]=null", Logging.INDEX_MESSAGE.NOT_SET);
                        else
                            ;
                else
                    Logging.Logg ().Error (@"HPanelTableLayout::OnSizeChanged () - fonts=null", Logging.INDEX_MESSAGE.NOT_SET);
            }

            /// <summary>
            /// Требуется переменная конструктора.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary> 
            /// Освободить все используемые ресурсы.
            /// </summary>
            /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
            protected override void Dispose (bool disposing)
            {
                if (disposing && (components != null)) {
                    components.Dispose ();
                }
                base.Dispose (disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponent ()
            {
                components = new System.ComponentModel.Container ();

                this.SizeChanged += new EventHandler (OnSizeChanged);
            }

            #endregion
        }
        /// <summary>
        /// Защищенный абстрактный класс "Панель быстрые данные"
        /// </summary>
        protected abstract class HPanelQuickData : HPanelTableLayout
        {
            /// <summary>
            /// Количество элементов управления-подписей по оси X
            /// </summary>
            protected int COUNT_LABEL;
            /// <summary>
            /// Количество элементов управления-подписей по оси Y для ТГ(параметров ВЫВОДов)
            /// </summary>
            protected int COUNT_TG_IN_COLUMN;
            /// <summary>
            /// Индекс столбца с которого начинается отображение ТГ(параметров ВЫВОДов)
            /// </summary>
            protected int COL_TG_START;
            /// <summary>
            /// Количество виртуальных строк на которые разбита панель
            /// </summary>
            protected int COUNT_ROWS = -1;
            /// <summary>
            /// Количество виртуальных строк на которых размещается любая из ОБЩих подписей (до ТГ, параметров ВЫВОДов)
            /// </summary>
            protected int COUNT_ROWSPAN_LABELCOMMON;
            protected float SZ_COLUMN_LABEL, SZ_COLUMN_LABEL_VALUE
                , SZ_COLUMN_TG_LABEL, SZ_COLUMN_TG_LABEL_VALUE;
            /// <summary>
            /// Кнопка для установки режима "Текущий час"
            /// </summary>
            public System.Windows.Forms.Button btnSetNow;
            /// <summary>
            /// Элемент графического интерфейса - календарь
            /// </summary>
            public DateTimePicker dtprDate;
            /// <summary>
            /// Подпись для отображения времени сервера
            /// </summary>
            public System.Windows.Forms.Label lblServerTime;
            /// <summary>
            /// Пустая(вспомогательная) панель для обеспечения корректного выравнивания
            /// </summary>
            protected Panel m_panelEmpty;
            /// <summary>
            /// Изменить структуру размещения дочерних элементов управления
            /// </summary>
            public abstract void RestructControl();

            protected abstract TableLayoutPanelCellPosition getPositionCell(int indx);
            /// <summary>
            /// Отобразить фактические значения
            /// </summary>
            public abstract void ShowFactValues();
            /// <summary>
            /// Отобразить значения телемеханики
            /// </summary>
            public abstract void ShowTMValues();

            protected virtual PanelTecViewBase m_parent { get { return (PanelTecViewBase)Parent; } }

            public /*static*/ int /*s*/m_indxStartCommonFirstValueSeries
                , /*s*/m_indxStartCommonSecondValueSeries;
            protected int m_iCountCommonLabels;
            /// <summary>
            /// Словарь массивов подписей для обобщенных значений компонента(ГТП, ЩУ, станция)
            /// </summary>
            public System.Windows.Forms.Label[] m_arLabelCommon;
            /// <summary>
            /// Словарь массивов подписей для ТГ; ключ - идентификатор компонента
            ///  , массив - все подписи для ТГ: описание, факт, телемеханика
            /// </summary>
            protected Dictionary<int, System.Windows.Forms.Label[]> m_tgLabels;
            /// <summary>
            /// СЛоварь массивоов всплывающих подсказок для подписей со значениями телемеханики ТГ
            /// </summary>
            protected Dictionary<int, System.Windows.Forms.ToolTip[]> m_tgToolTips;
            /// <summary>
            /// Конструктор - основной (с аргументами)
            /// </summary>
            /// <param name="foreColor">Цвет шрифта</param>
            /// <param name="backColor">Цвет фона</param>
            public HPanelQuickData(Color foreColor, Color backColor)
                : base (foreColor, backColor)
            {
                InitializeComponent();
            }
            /// <summary>
            /// Конструктор - дополнительный (с аргументами)
            /// </summary>
            /// <param name="container">Объект родительского контейнера</param>
            /// <param name="foreColor">Цвет шрифта</param>
            /// <param name="backColor">Цвет фона</param>
            public HPanelQuickData (IContainer container, Color foreColor, Color backColor)
                : base (foreColor, backColor)
            {
                container.Add(this);

                InitializeComponent();
            }

            private void InitializeComponent()
            {
                /*SZ_COLUMN_LABEL = 48F;*/
                SZ_COLUMN_LABEL_VALUE = 88F;
                SZ_COLUMN_TG_LABEL = 40F; SZ_COLUMN_TG_LABEL_VALUE = 75F;

                this.btnSetNow = new System.Windows.Forms.Button();
                this.dtprDate = new System.Windows.Forms.DateTimePicker();
                this.lblServerTime = new System.Windows.Forms.Label();

                m_panelEmpty = new Panel();

                this.SuspendLayout();

                //
                // btnSetNow (Текущий час)
                //
                //this.btnSetNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                this.btnSetNow.Dock = DockStyle.Fill;
                //this.btnSetNow.Location = arPlacement[(int)CONTROLS.btnSetNow].pt;
                this.btnSetNow.Name = "btnSetNow";
                //this.btnSetNow.Size = arPlacement[(int)CONTROLS.btnSetNow].sz;
                this.btnSetNow.TabIndex = 2;
                this.btnSetNow.Text = "Текущий час";
                this.btnSetNow.UseVisualStyleBackColor = true;

                // 
                // dtprDate (Дата-Календарь)
                // 
                //this.dtprDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                this.dtprDate.Dock = DockStyle.Fill;
                //this.dtprDate.Location = arPlacement[(int)CONTROLS.dtprDate].pt;
                this.dtprDate.Name = "dtprDate";
                //this.dtprDate.Size = arPlacement[(int)CONTROLS.dtprDate].sz;
                this.dtprDate.TabIndex = 4;

                // 
                // lblServerTime (Время сервера)
                // 
                //this.lblServerTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                this.lblServerTime.Dock = DockStyle.Fill;
                this.lblServerTime.AutoSize = false;
                //this.lblServerTime.Location = arPlacement[(int)CONTROLS.lblServerTime].pt;
                this.lblServerTime.Name = "lblServerTime";
                //this.lblServerTime.Size = arPlacement[(int)CONTROLS.lblServerTime].sz;
                this.lblServerTime.TabIndex = 5;
                this.lblServerTime.Text = "--:--:--";
                this.lblServerTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                this.lblServerTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                this.lblServerTime.TextAlign = ContentAlignment.MiddleCenter;

                this.ResumeLayout(false);
                //this.PerformLayout();

                m_tgLabels = new Dictionary<int, System.Windows.Forms.Label[]>();
                m_tgToolTips = new Dictionary<int, ToolTip[]>();
            }

            /// <summary>
            /// Цвет шрифта панели
            /// </summary>
            public override Color ForeColor
            {
                get
                {
                    return base.ForeColor;
                }

                set
                {
                    base.ForeColor = value;

                    if ((Equals (btnSetNow, null) == false)
                        && (Equals (lblServerTime, null) == false))
                        btnSetNow.ForeColor =
                        lblServerTime.ForeColor =
                            value;
                    else
                        ;

                    if (Equals (m_arLabelCommon, null) == false)
                        foreach (Label label in m_arLabelCommon)
                            if ((Equals (label) == false)
                                && (label.GetType ().Equals (typeof (ASUTP.Control.HLabel)) == false))
                                label.ForeColor = value;
                            else
                                ;
                    else
                        ;

                    if (Equals (m_tgLabels, null) == false)
                        foreach (KeyValuePair<int, Label []> pair in m_tgLabels)
                            foreach (Label label in pair.Value)
                                if ((Equals (label) == false)
                                    && (label.GetType ().Equals (typeof (ASUTP.Control.HLabel)) == false))
                                    label.ForeColor = value;
                                else
                                    ;
                    else
                        ;
                }
            }

            /// <summary>
            /// Цвет фона панели
            /// </summary>
            public override Color BackColor
            {
                get
                {
                    return base.BackColor;
                }

                set
                {
                    base.BackColor = value;

                    if (Equals (m_arLabelCommon, null) == false)
                        foreach (Label label in m_arLabelCommon)
                            if ((Equals (label) == false)
                                && (label.GetType ().Equals (typeof (ASUTP.Control.HLabel)) == false))
                                label.BackColor = value;
                            else
                                ;
                    else
                        ;

                    if (Equals (m_tgLabels, null) == false)
                        foreach (KeyValuePair<int, Label []> pair in m_tgLabels)
                            foreach (Label label in pair.Value)
                                if ((Equals (label) == false)
                                    && (label.GetType ().Equals (typeof (ASUTP.Control.HLabel)) == false))
                                    label.BackColor = value;
                                else
                                    ;
                    else
                        ;
                }
            }            

            //public void addTGView(ref string name_shr, /*ref float val,*/ ref int positionXName, ref int positionYName, ref int positionXValue, ref int positionYValue)
            public virtual void AddTGView(TECComponentBase comp)
            {
                int cnt = -1
                    , id = -1;
                ASUTP.Control.HLabel hlblValue;

                id = comp.m_id;

                m_tgLabels.Add(id, new Label[(int)TG.INDEX_VALUE.COUNT_INDEX_VALUE]);
                m_tgToolTips.Add(id, new ToolTip[(int)TG.INDEX_VALUE.COUNT_INDEX_VALUE]);
                cnt = m_tgLabels.Count;

                m_tgLabels[id][(int)TG.INDEX_VALUE.LABEL_DESC] = ASUTP.Control.HLabel.createLabel(comp.name_shr.Trim()
                    , new ASUTP.Control.HLabelStyles (/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1)
                    , FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor
                    , 8F
                    , ContentAlignment.MiddleRight));

                hlblValue = new ASUTP.Control.HLabel (new ASUTP.Control.HLabelStyles (new Point(-1, -1), new Size(-1, -1), Color.LimeGreen, Color.Black, 13F, ContentAlignment.MiddleCenter));
                hlblValue.Text = @"---.--"; //name_shr + @"_Fact";
                hlblValue.m_type = ASUTP.Control.HLabel.TYPE_HLABEL.TG;
                //m_tgToolTips[id][(int)TG.INDEX_VALUE.FACT].SetToolTip(hlblValue, tg.name_shr + @"[" + tg.m_SensorsStrings_ASKUE[0] + @"]: " + (tg.m_TurnOnOff == TG.INDEX_TURNOnOff.ON ? @"вкл." : @"выкл."));
                m_tgLabels[id][(int)TG.INDEX_VALUE.FACT] = (Label)hlblValue;
                m_tgToolTips[id][(int)TG.INDEX_VALUE.FACT] = new ToolTip();

                hlblValue = new ASUTP.Control.HLabel (new ASUTP.Control.HLabelStyles (new Point(-1, -1), new Size(-1, -1), Color.Green, Color.Black, 13F, ContentAlignment.MiddleCenter));
                hlblValue.Text = @"---.--"; //name_shr + @"_TM";
                hlblValue.m_type = ASUTP.Control.HLabel.TYPE_HLABEL.TG;
                m_tgLabels[id][(int)TG.INDEX_VALUE.TM] = (Label)hlblValue;

                m_tgToolTips[id][(int)TG.INDEX_VALUE.TM] = new ToolTip();
            }

            /// <summary>
            /// Создать элемент интерфейса - подпись
            /// </summary>
            /// <param name="indx">Индекс подписи в массиве</param>
            /// <param name="strLabelText">Текст для отображения</param>
            /// <param name="clrLabelFore">Цвет текста на подписи</param>
            /// <param name="clrLabelBackground">Цвет фона подписи</param>
            /// <param name="fSzLabelFont">Размер шрифта теста подписи</param>
            /// <param name="alignLabel">Признак выравнивания текста подписи</param>
            protected void createLabel(int indx, string strLabelText, Color clrLabelFore, Color clrLabelBackground, float fSzLabelFont, ContentAlignment alignLabel)
            {
                if (strLabelText.Equals(string.Empty) == false)
                // итоговые(TOTAL) подписи для значений за компонент
                    if (m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries] == null)
                        m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries] = ASUTP.Control.HLabel.createLabel(strLabelText
                            , new ASUTP.Control.HLabelStyles (/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1)
                            , clrLabelFore, clrLabelBackground
                            , fSzLabelFont, alignLabel));
                    else
                        ;
                else
                // итоговые(TOTAL) значения за компонент
                    if (m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries] == null) {
                        m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries] = new ASUTP.Control.HLabel (/*i.ToString(); @"---",*/
                            new ASUTP.Control.HLabelStyles (/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1)
                            , clrLabelFore, clrLabelBackground
                            , fSzLabelFont, alignLabel));
                        m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries].Text = @"---";
                        ((ASUTP.Control.HLabel)m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries]).m_type = ASUTP.Control.HLabel.TYPE_HLABEL.TOTAL;
                    } else
                        ;
            }

            /// <summary>
            /// Удалить 1-ую (по порядку) общую подпись
            /// </summary>
            /// <param name="limit">Индекс (подписи) до которого следует удалять подписи</param>
            protected void removeFirstCommonLabels(int limit)
            {
                removeSecondCommonLabels(m_indxStartCommonFirstValueSeries, limit);
            }

            protected void removeSecondCommonLabels(int limit)
            {
                removeSecondCommonLabels(m_indxStartCommonSecondValueSeries, limit);
            }

            private void removeSecondCommonLabels(int indxStart, int limit)
            {
                for (int i = indxStart; i < limit + 1; i++)
                {
                    if (!(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries] == null))
                        if (!(this.Controls.IndexOf(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]) < 0))
                            this.Controls.Remove(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]);
                        else;
                    else
                        ;
                }
            }

            /// <summary>
            /// Добавить подписи для ТГ
            /// </summary>
            /// <param name="bIsTM">Признак необходимости добавления подписи для значений телемеханики</param>
            protected void addTGLabels(bool bIsTM)
            {
                int r = -1, c = -1
                    , i = 0;

                foreach (int key in m_tgLabels.Keys)
                {
                    i++;

                    this.Controls.Add(m_tgLabels[key][(int)TG.INDEX_VALUE.LABEL_DESC]);
                    c = (i - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 0); r = (i - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN);
                    this.SetCellPosition(m_tgLabels[key][(int)TG.INDEX_VALUE.LABEL_DESC], new TableLayoutPanelCellPosition(c, r));
                    this.SetRowSpan(m_tgLabels[key][(int)TG.INDEX_VALUE.LABEL_DESC], (COUNT_ROWS / COUNT_TG_IN_COLUMN));

                    this.Controls.Add(m_tgLabels[key][(int)TG.INDEX_VALUE.FACT]);
                    c = (i - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 1); r = (i - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN);
                    this.SetCellPosition(m_tgLabels[key][(int)TG.INDEX_VALUE.FACT], new TableLayoutPanelCellPosition(c, r));
                    this.SetRowSpan(m_tgLabels[key][(int)TG.INDEX_VALUE.FACT], (COUNT_ROWS / COUNT_TG_IN_COLUMN));

                    if (bIsTM == true)
                    {
                        this.Controls.Add(m_tgLabels[key][(int)TG.INDEX_VALUE.TM]);
                        c = (i - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 2); r = (i - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN);
                        this.SetCellPosition(m_tgLabels[key][(int)TG.INDEX_VALUE.TM], new TableLayoutPanelCellPosition(c, r));
                        this.SetRowSpan(m_tgLabels[key][(int)TG.INDEX_VALUE.TM], (COUNT_ROWS / COUNT_TG_IN_COLUMN));
                    }
                    else
                        ;
                }
            }

            /// <summary>
            /// Удалить подписи для ТГ
            /// </summary>
            protected void removeTGLabels()
            {
                if (m_tgLabels.Count > 0)
                    foreach (int key in m_tgLabels.Keys)
                        for (int j = 0; j < (int)TG.INDEX_VALUE.COUNT_INDEX_VALUE; j++)
                            if ((!(m_tgLabels[key][j] == null)) && (!(this.Controls.IndexOf(m_tgLabels[key][j]) < 0)))
                                this.Controls.Remove(m_tgLabels[key][j]);
                            else
                                ;
                else
                    ;
            }

            /// <summary>
            /// Отобразить значение для элемента управления с условиями
            /// </summary>
            /// <param name="lbl">элемент управления, как ссылка</param>
            /// <param name="val">значение для отображения</param>
            /// <param name="bCheckVal">признак проверки значения</param>
            /// <param name="bPower">признак отображения значения мощности</param>
            /// <param name="adding">дополнительная для отображения строка</param>
            protected static void showValue(ref System.Windows.Forms.Label lbl, double val, int round, bool bCheckVal, bool bPower, string adding)
            {
                if (!(lbl == null))
                    if (val == double.NegativeInfinity)
                        lbl.Text = adding;
                    else
                        if (bPower == true)
                        if ((val > 1)
                            || (bCheckVal == false))
                            lbl.Text = val.ToString("F" + round.ToString()) + adding;
                        else
                            lbl.Text = 0.ToString("F0");
                    else
                        lbl.Text = val.ToString("F" + round.ToString()) + adding;
                else
                    ;
            }

            /// <summary>
            /// Отобразить значение для элемента управления с условиями
            /// </summary>
            /// <param name="lbl">элемент управления</param>
            /// <param name="val">значение для отображения</param>
            protected static void showValue(System.Windows.Forms.Label lbl, double val, int round, bool bCheckVal)
            {
                if ((val > 1)
                    || (bCheckVal == false))
                    lbl.Text = Math.Round(val, round, MidpointRounding.ToEven).ToString("F" + round.ToString());
                else
                    lbl.Text = 0.ToString("F0");
            }

            /// <summary>
            /// Возвратить цвет для отображения фактических значений
            /// </summary>
            /// <returns>Цвет для фактических значений</returns>
            protected Color getColorValues(TG.INDEX_VALUE indx)
            {
                Color clrRes = Color.Empty;
                // 0 - ретро-значения, 1 - текущие значения
                Color [,] arColorValues = new Color[,] {
                    { Color.OrangeRed, Color.Orange }
                    , { Color.LimeGreen, Color.Green }
                };

                if (!(m_parent == null))
                    //Определить цвет
                    switch (indx) {
                        case TG.INDEX_VALUE.FACT: // для фактических значений или для значений в 1-ом столбце детализации
                            if (m_parent.m_tecView.currHour == true)
                                if (m_parent.m_tecView.m_markWarning.IsMarked((int)TecView.INDEX_WARNING.LAST_MIN) == true)
                                    clrRes = arColorValues[0, (int)indx];
                                else
                                    clrRes = arColorValues[1, (int)indx];
                            else
                                clrRes = arColorValues[0, (int)indx];
                            break;
                        case TG.INDEX_VALUE.TM: // для значений телемеханики или значений во 2-ом столбце детализации
                            clrRes = (m_parent.m_tecView.currHour == true) ?
                                arColorValues[1, (int)indx] :
                                    arColorValues[0, (int)indx];
                            break;
                        default:
                            break;
                    }
                else
                    ;

                return clrRes;
            }
        }
        /// <summary>
        /// Защищенный частичный класс "Стандартная панель Быстрые данные"
        /// </summary>
        protected partial class PanelQuickDataStandard : HPanelQuickData
        {
            /// <summary>
            /// Перечисление "Индексы контестного меню:прогноз ЭЭ, значение телеметрии"
            /// </summary>
            private enum INDEX_CONTEXTMENUITEM { FORECASTEE, TM };

            /// <summary>
            /// Требуется переменная конструктора.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary> 
            /// Освободить все используемые ресурсы.
            /// </summary>
            /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            protected override TableLayoutPanelCellPosition getPositionCell(int indx)
            {
                int row = -1,
                    col = -1;

                switch ((CONTROLS)indx)
                {
                    case CONTROLS.lblCommonP:
                        row = 0; col = 1;
                        break;
                    case CONTROLS.lblCommonPVal_Fact:
                        row = 0; col = 2;
                        break;
                    case CONTROLS.lblCommonPVal_TM:
                        row = 0; col = 3;
                        break;
                    case CONTROLS.lblAverP:
                        row = 4; col = 1;
                        break;
                    case CONTROLS.lblAverPVal:
                        row = 4; col = 2;
                        break;
                    case CONTROLS.lblPBRrec:
                        row = 8; col = 0;
                        break;
                    case CONTROLS.lblPBRrecVal:
                        row = 8; col = 1;
                        break;
                    case CONTROLS.lblCurrentE:
                        row = 0; col = 4;
                        break;
                    case CONTROLS.lblCurrentEVal:
                        row = 0; col = 5;
                        break;
                    case CONTROLS.lblHourE:
                        row = 4; col = 4;
                        break;
                    case CONTROLS.lblHourEVal:
                        row = 4; col = 5;
                        break;
                    case CONTROLS.lblDevE:
                        row = 8; col = 4;
                        break;
                    case CONTROLS.lblDevEVal:
                        row = 8; col = 5;
                        break;
                    default:
                        break;
                }

                if ((((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == false) && (col > 2))
                    col--;
                else
                    ;

                //Console.WriteLine(@"PanelQuickData::getPositionCell () - " + indx.ToString () + @"[row=" + row + @", col=" + col + @"]");

                return new TableLayoutPanelCellPosition(col, row);
            }

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponent()
            {
                COUNT_ROWS = 12;
                SZ_COLUMN_LABEL = 54F;

                m_indxStartCommonFirstValueSeries = (int)CONTROLS.lblCommonP;
                m_indxStartCommonSecondValueSeries = (int)CONTROLS.lblCurrentE;
                m_iCountCommonLabels = (int)CONTROLS.lblDevEVal - (int)CONTROLS.lblCommonP + 1;

                components = new System.ComponentModel.Container();

                bool bEnabled = true
                    //, bChecked = ! HStatisticUsers.RoleIsOperationPersonal
                    ;

                this.ContextMenuStrip = new ContextMenuStrip();
                this.ContextMenuStrip.Items.AddRange(new ToolStripMenuItem[] {
                    new ToolStripMenuItem (@"Прогноз ЭЭ"),
                    new ToolStripMenuItem (@"Знач. телеметрии") });
                //Checked = bChecked;
                this.ContextMenuStrip.Items[0].Enabled = bEnabled; ((ToolStripMenuItem)this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.FORECASTEE]).Checked = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUCONTEXTITEM_PANELQUICKDATA_FORECASTEE); this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.FORECASTEE].Click += OnItemClick;
                this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM].Enabled = bEnabled; ((ToolStripMenuItem)this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUCONTEXTITEM_PANELQUICKDATA_TMVALUES); this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM].Click += OnItemClick;

                this.RowCount = COUNT_ROWS;

                for (int i = 0; i < this.RowCount + 1; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((float)100 / this.RowCount, 1)));
                // Создание лейбла для номера ПБР
                this.lblPBRNumber = new System.Windows.Forms.Label();

                this.m_arLabelCommon = new System.Windows.Forms.Label[m_iCountCommonLabels];

                //
                // btnSetNow
                //
                this.Controls.Add(this.btnSetNow, 0, 0);
                this.SetRowSpan(this.btnSetNow, 3);
                // 
                // dtprDate
                // 
                this.Controls.Add(this.dtprDate, 0, 3);
                this.SetRowSpan(this.dtprDate, 3);
                // 
                // lblServerTime
                // 
                this.Controls.Add(this.lblServerTime, 0, 6);
                this.SetRowSpan(this.lblServerTime, 3);
                // 
                // lblPBRNumber
                // 
                //this.lblPBRNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                this.lblPBRNumber.Dock = DockStyle.Fill;
                this.lblPBRNumber.AutoSize = false;
                //this.lblPBRNumber.Location = arPlacement[(int)CONTROLS.lblPBRNumber].pt;
                this.lblPBRNumber.Name = "lblPBRNumber";
                //this.lblPBRNumber.Size = arPlacement[(int)CONTROLS.lblPBRNumber].sz;
                this.lblPBRNumber.TabIndex = 5;
                this.lblPBRNumber.Text = "---";
                this.lblPBRNumber.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                this.lblPBRNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                this.lblPBRNumber.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(this.lblPBRNumber, 0, 9);
                this.SetRowSpan(this.lblPBRNumber, 3);
                

                //Ширина столбца группы "Элементы управления"
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));

                //Создание ОБЩих элементов управления
                Color foreColor, backColor;
                float szFont;
                ContentAlignment align;
                //Size sz;
                string text = string.Empty;
                //int row = -1, col = -1;

                #region добавить поля для значений МОЩНОСТИ и их подписи
                for (CONTROLS i = (CONTROLS)m_indxStartCommonFirstValueSeries; i < CONTROLS.lblPBRrecVal + 1; i++)
                {
                    //szFont = 6F;

                    switch (i)
                    {
                        case CONTROLS.lblCommonP:
                        case CONTROLS.lblPBRrec:
                        case CONTROLS.lblAverP:
                            backColor = FormMain.formGraphicsSettings.BackgroundColor;
                            foreColor = FormMain.formGraphicsSettings.FontColor;
                            szFont = 8F;
                            align = ContentAlignment.MiddleLeft;
                            //sz = new Size(-1, -1);
                            //col = 1;
                            break;
                        case CONTROLS.lblCommonPVal_Fact:
                        case CONTROLS.lblAverPVal:
                            backColor = Color.Black;
                            foreColor = Color.LimeGreen;
                            szFont = 15F;
                            align = ContentAlignment.MiddleCenter;
                            //sz = arPlacement[(int)i].sz;
                            //col = 2;
                            break;
                        case CONTROLS.lblPBRrecVal:
                            backColor = Color.Black;
                            foreColor = Color.Yellow;
                            szFont = 15F;
                            align = ContentAlignment.MiddleCenter;
                            break;
                        case CONTROLS.lblCommonPVal_TM:
                            backColor = Color.Black;
                            foreColor = Color.Green;
                            szFont = 15F;
                            align = ContentAlignment.MiddleCenter;
                            //sz = arPlacement[(int)i].sz;
                            //col = 3;
                            break;
                        default:
                            backColor = Color.Red;
                            foreColor = Color.Yellow;
                            szFont = 6F;
                            align = ContentAlignment.MiddleCenter;
                            //sz = new Size(-1, -1);
                            break;
                    }

                    switch (i)
                    {
                        case CONTROLS.lblCommonP:
                            text = @"Pтек"; //@"P тек";
                            break;
                        case CONTROLS.lblPBRrec:
                            text = @"Pрек";
                            break;
                        case CONTROLS.lblAverP:
                            text = @"Pср";
                            break;
                        default:
                            text = string.Empty;
                            break;
                    }

                    createLabel((int)i, text, foreColor, backColor, szFont, align);
                }
                #endregion

                #region добавить поля для значений ЭНЕРГИИ и их подписи
                for (CONTROLS i = (CONTROLS)m_indxStartCommonSecondValueSeries; i < CONTROLS.lblDevEVal + 1; i++)
                {
                    switch (i)
                    {
                        case CONTROLS.lblCurrentE:
                        case CONTROLS.lblHourE:
                        case CONTROLS.lblDevE:
                            backColor = FormMain.formGraphicsSettings.BackgroundColor;
                            foreColor = FormMain.formGraphicsSettings.FontColor;
                            szFont = 8F;
                            align = ContentAlignment.MiddleRight;
                            //sz = new Size(-1, -1);
                            //col = 4;
                            break;
                        case CONTROLS.lblCurrentEVal:
                        case CONTROLS.lblHourEVal:
                            backColor = Color.Black;
                            foreColor = Color.LimeGreen;
                            szFont = 15F;
                            align = ContentAlignment.MiddleCenter;
                            //sz = arPlacement[(int)i].sz;
                            //col = 5;
                            break;
                        case CONTROLS.lblDevEVal:
                            backColor = Color.Black;
                            foreColor = Color.Yellow;
                            szFont = 15F;
                            align = ContentAlignment.MiddleCenter;
                            //sz = arPlacement[(int)i].sz;
                            //col = 5;
                            break;
                        default:
                            backColor = Color.Yellow;
                            foreColor = Color.Red;
                            szFont = 6F;
                            align = ContentAlignment.MiddleCenter;
                            //sz = new Size(-1, -1);
                            break;
                    }

                    switch (i)
                    {
                        case CONTROLS.lblCurrentE:
                            text = @"Етек"; //@"P тек";
                            break;
                        case CONTROLS.lblHourE:
                            text = @"Ечас";
                            break;
                        case CONTROLS.lblDevE:
                            text = @"Откл";
                            break;
                        default:
                            text = string.Empty;
                            //text = @"---";
                            break;
                    }

                    createLabel((int)i, text, foreColor, backColor, szFont, align);
                }
                #endregion

                //Создание пассивного эл./упр. "надпись" для увеличенного дублирования знач. Pтек
                m_lblPowerFactZoom = new ASUTP.Control.HLabel (new Point(-1, -1), new Size(-1, -1), Color.LimeGreen, FormMain.formGraphicsSettings.BackgroundColor, 12F, ContentAlignment.MiddleCenter);
                m_lblPowerFactZoom.m_type = ASUTP.Control.HLabel.TYPE_HLABEL.TOTAL_ZOOM;
                m_lblPowerFactZoom.Text = @"Pтек=----.--";

                //OnSizeChanged(this, EventArgs.Empty);
            }

            #endregion

            public enum CONTROLS : uint
            {
                lblCommonP, lblCommonPVal_Fact,
                lblCommonPVal_TM,
                lblAverP, lblAverPVal,
                lblPBRrec, lblPBRrecVal,
                lblCurrentE, lblCurrentEVal,
                lblHourE, lblHourEVal,
                lblDevE, lblDevEVal,
                dtprDate,
                lblServerTime,
                btnSetNow,
                lblPBRNumber,
                COUNT_CONTROLS
            };

            private ASUTP.Control.HLabel m_lblPowerFactZoom;
            private System.Windows.Forms.Label lblPBRNumber;

            public override void RestructControl()
            {
                COUNT_LABEL = (int)TG.INDEX_VALUE.COUNT_INDEX_VALUE; COUNT_TG_IN_COLUMN = 4; COL_TG_START = 6;
                COUNT_ROWSPAN_LABELCOMMON = 4;

                bool bPowerFactZoom = false;
                int cntCols = -1;

                //Console.WriteLine(@"PanelQuickData::RestructControl () - вХод...");

                //Удаление ОБЩих элементов управления
                // рекомендация
                removeFirstCommonLabels((int)CONTROLS.lblPBRrecVal);
                // отклонение
                removeSecondCommonLabels((int)CONTROLS.lblDevEVal);

                //Удаление ТГ
                removeTGLabels();

                //Удаление ДУБЛирующей подписи
                if (!(this.Controls.IndexOf(m_lblPowerFactZoom) < 0)) this.Controls.Remove(m_lblPowerFactZoom); else;

                //Удаление ПУСТой панели
                if (!(this.Controls.IndexOf(m_panelEmpty) < 0)) this.Controls.Remove(m_panelEmpty); else;

                //Удаление стилей столбцов
                while (this.ColumnStyles.Count > 1)
                    this.ColumnStyles.RemoveAt(this.ColumnStyles.Count - 1);

                if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.FORECASTEE]).Checked == false)
                {
                    COL_TG_START -= 2;
                }
                else
                    ;

                if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == false)
                {
                    COUNT_LABEL--;
                    COL_TG_START--;
                }
                else
                    ;

                #region Добавить столбцы группы "Рекомендация"
                for (CONTROLS i = (CONTROLS)m_indxStartCommonFirstValueSeries; i < CONTROLS.lblPBRrecVal + 1; i++)
                {
                    bool bAddItem = false;
                    if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == false)
                    {
                        if (i == CONTROLS.lblCommonPVal_TM)
                            ; //continue;
                        else
                        {
                            bAddItem = true;
                        }
                    }
                    else
                    {
                        bAddItem = true;
                    }

                    if (bAddItem == true)
                    {
                        //this.Controls.Add(m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
                        this.Controls.Add(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]);
                        this.SetCellPosition(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], getPositionCell((int)i));
                        this.SetRowSpan(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], COUNT_ROWSPAN_LABELCOMMON);
                    }
                    else
                        ;
                }

                //Ширина столбцов группы "Рекомендация"
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL_VALUE));
                if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == true)
                    //Телеметрия для объекта отображения
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
                else
                    ;
                #endregion

                #region Добавить столбцы группы "Отклонение" (в ~ от пользовательской настройки)
                if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.FORECASTEE]).Checked == true)
                {
                    for (CONTROLS i = (CONTROLS)m_indxStartCommonSecondValueSeries; i < CONTROLS.lblDevEVal + 1; i++)
                    {
                        //this.Controls.Add(m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
                        this.Controls.Add(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]);
                        this.SetCellPosition(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], getPositionCell((int)i));
                        this.SetRowSpan(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], COUNT_ROWSPAN_LABELCOMMON);
                    }

                    //Ширина столбцов группы "Отклонение"
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL));
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL_VALUE));
                }
                else
                    ;
                #endregion

                bPowerFactZoom = false;
                cntCols = ((m_tgLabels.Count / COUNT_TG_IN_COLUMN) + ((m_tgLabels.Count % COUNT_TG_IN_COLUMN == 0) ? 0 : 1));

                for (int i = 0; i < cntCols; i++)
                {
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL));
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL_VALUE));
                    if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == true)
                        //Телеметрия ТГ
                        this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL_VALUE));
                    else
                        ;
                }

                if (m_tgLabels.Count > 0)
                    addTGLabels(((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked);
                else
                    ;

                //if ((Users.Role == (int)Users.ID_ROLES.NSS) || (Users.Role == (int)Users.ID_ROLES.MAJOR_MASHINIST) || (Users.Role == (int)Users.ID_ROLES.MASHINIST))
                if ((((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.FORECASTEE]).Checked == false)
                    && (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == false))
                {
                    this.Controls.Add(m_lblPowerFactZoom, COL_TG_START + cntCols * COUNT_LABEL + 0, 0);
                    this.SetRowSpan(m_lblPowerFactZoom, COUNT_ROWS);
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88 * 3));

                    //Font fontLabelZoom = GetFontHLabel(HLabel.TYPE_HLABEL.TOTAL_ZOOM);
                    //m_lblPowerFactZoom.Font = new Font (fontLabelZoom, FontStyle.Bold);

                    bPowerFactZoom = true;
                }
                else
                    ;

                this.Controls.Add(m_panelEmpty, COL_TG_START + cntCols * COUNT_LABEL + (bPowerFactZoom == true ? 1 : 0), 0);
                this.SetRowSpan(m_panelEmpty, COUNT_ROWS);
                this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }
        }
        /// Защищенный частичный класс "Стандартная панель Оперативные данные"
        partial class PanelQuickDataStandard : HPanelQuickData
        {
            public PanelQuickDataStandard(Color foreColor, Color backColor)
                : base(foreColor, backColor)
            {
                InitializeComponent();
            }

            public PanelQuickDataStandard(IContainer container, Color foreColor, Color backColor)
                : base (container, foreColor, backColor)
            {
                container.Add(this);

                InitializeComponent();
            }

            private void showTMValue(TG tg, ref double val, int min)
            {
                showTMValue(ref m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.TM]
                    , tg.m_SensorsString_SOTIASSO
                    , m_parent.m_tecView.m_dictValuesLowPointDev[tg.m_id].m_powerMinutes[min]
                    , m_parent.m_tecView.m_dictValuesLowPointDev[tg.m_id].m_powerCurrent_TM
                    , m_parent.m_tecView.m_dictValuesLowPointDev[tg.m_id].m_dtCurrent_TM
                    , m_parent.m_tecView.serverTime
                    , m_tgToolTips[tg.m_id][(int)TG.INDEX_VALUE.TM]
                    , ref val);
            }

            private void showTMValue(ref Label lbl, string tg_kksname, double tg_val_fact, double tg_val, DateTime dt_val, DateTime dt_srv, ToolTip toolTip, ref double val)
            {
                string text = string.Empty
                    , textNotValue = @"---";
                bool bValidateDateTime = false;

                if (tg_kksname.Length > 0)
                {
                    if (!(tg_val < 0))
                    {// больше ИЛИ равно 0
                        bValidateDateTime = TecView.ValidateDatetimeTMValue(dt_srv, dt_val);

                        if (tg_val > 1)
                        {// больше 1                        
                            text = tg_val.ToString("F2");
                            if (bValidateDateTime == true)
                                if (!(val < 0))
                                    val += tg_val;
                                else
                                    ;
                            else
                                val = -1F;
                        }
                        else
                        {// в диапазоне [0; 1] включительно
                            text = 0.ToString("F0");
                        }
                    }
                    else
                    {// меньше 0
                        // в зависимости от значения АИИС КУЭ
                        if (!(tg_val_fact > 1))
                        {// факт. меньше ИЛИ равно 1
                            text = 0.ToString("F0");
                            bValidateDateTime = true;
                        }
                        else
                            text = textNotValue;

                        //val = -1F;
                    }
                }
                else
                {
                    text = textNotValue;
                    val = -1F;
                }

                //if (text.Equals (textNotValue) == true)
                if (bValidateDateTime == false)
                    lbl.ForeColor = Color.Orange;
                else
                    lbl.ForeColor = getColorValues(TG.INDEX_VALUE.TM);

                lbl.Text = text;
                toolTip.SetToolTip(lbl, dt_val.ToString("dd.MM.yyyy HH:mm:ss"));
            }

            /// <summary>
            /// Отобразить текущие значения
            /// </summary>
            public override void ShowTMValues()
            {
                double value_TM = 0.0;

                if ((!(m_parent == null)) && ((m_parent.m_tecView.serverTime - DateTime.MinValue).TotalSeconds > 1))
                {
                    //if (m_parent.m_tecView.currHour == true)
                    //{
                    //    if (m_parent.m_tecView.currentMinuteTM_GenWarning == true)
                    //    {
                    //        ; //m_parent.m_tecView.WarningReport("Значение телемеханики для одного из ТГ не найдено!");
                    //    }
                    //    else
                    //        ;
                    //}
                    //else
                    //    ;

                    int min = m_parent.m_tecView.lastMin < m_parent.m_tecView.m_valuesMins.Length ? m_parent.m_tecView.lastMin : m_parent.m_tecView.m_valuesMins.Length - 1;

                    if (m_parent.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC) // значит этот view будет суммарным для всех ГТП
                    {
                        foreach (TECComponent g in m_parent.m_tecView.m_tec.ListTECComponents)
                        {
                            if (g.IsGTP == true)
                                //Только ГТП
                                foreach (TG tg in g.ListLowPointDev)
                                    showTMValue(tg, ref value_TM, min);
                            else
                                ;
                        }
                    }
                    else
                    {
                        foreach (TG tg in m_parent.m_tecView.m_tec.ListTECComponents.Find(comp => comp.m_id == m_parent.TecViewKey.Id).ListLowPointDev)
                            showTMValue(tg, ref value_TM, min);
                    }
                }
                else
                    ;

                if (value_TM < 1)
                    value_TM = 0.0;
                else
                    ;

                showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_TM - m_indxStartCommonFirstValueSeries]
                    , value_TM
                    , 2 //round
                    , true
                    , true
                    , string.Empty);
                m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_TM - m_indxStartCommonFirstValueSeries].ForeColor = getColorValues(TG.INDEX_VALUE.TM);
            }

            /// <summary>
            /// Отобразить фактические значение
            /// </summary>
            public override void ShowFactValues()
            {
                int indxStartCommonPVal = m_indxStartCommonFirstValueSeries;
                int id = -1
                    , i = -1, j = -1
                    , min = -1;

                bool bPrevValueValidate = false
                        , bMinValuesReceived = true;
                double prevValue = 0F, value = 0F
                    , valueEBefore = 0F
                    , valueECur = 0F
                    , valueEFuture = 0F;

                if (!(m_parent == null))
                {
                    indxStartCommonPVal = m_indxStartCommonFirstValueSeries;
                    min = m_parent.m_tecView.lastMin;

                    if (!(min == 0)) min--; else;

                    bPrevValueValidate = double.TryParse(m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].Text, out prevValue);

                    value = m_parent.m_tecView.GetSummaFactValues();

                    //14.04.2015 ???
                    if ((bMinValuesReceived == true) && (value < 1F) && (bPrevValueValidate == true) && (!(prevValue < 1F)))
                    {
                        //Logging.Logg().Debug(@"PanelQuickData::ShowFactValues () - value < 1F", Logging.INDEX_MESSAGE.NOT_SET);
                        Logging.Logg().Warning(@"PanelQuickData::ShowFactValues () - value < 1F", Logging.INDEX_MESSAGE.NOT_SET);

                        //return;
                    }
                    else
                        ;

                    for (i = 0; i < m_parent.m_tecView.ListLowPointDev.Count; i++)
                        if (m_parent.m_tecView.m_dictValuesLowPointDev[m_parent.m_tecView.ListLowPointDev[i].m_id].m_bPowerMinutesRecieved == true)
                            for (j = 0; j < min; j++) {
                                id = m_parent.m_tecView.ListLowPointDev[i].m_id;
                                if ((!(m_parent.m_tecView.m_dictValuesLowPointDev[id].m_powerMinutes[j] < 0))
                                    && (m_parent.m_tecView.m_dictValuesLowPointDev[m_parent.m_tecView.ListLowPointDev[i].m_id].m_powerMinutes[j] > 1))
                                    valueEBefore += m_parent.m_tecView.m_dictValuesLowPointDev[id].m_powerMinutes[j] / 20;
                                else
                                    ;
                            }
                        else
                            ;

                    showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal]
                        , value
                        , 2 //round
                        , true
                        , true
                        , string.Empty);
                    //if (this.ContextMenuStrip.Items [(int)INDEX_CONTEXTMENU_ITEM_FUTURE_EE] = false)
                    //if ((!(Users.Role == (int)Users.ID_ROLES.KOM_DISP)) && (!(Users.Role == (int)Users.ID_ROLES.ADMIN)))
                    m_lblPowerFactZoom.Text = @"Pтек=" + value.ToString(@"F2");
                    //else ;

                    valueECur = value / 20;
                    showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCurrentEVal - indxStartCommonPVal]
                        , valueECur
                        , 2 //round
                        , true
                        , true
                        , string.Empty);

                    valueEFuture = valueECur * (20 - min - 0);
                    showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblHourEVal - indxStartCommonPVal]
                        , valueEBefore + valueECur + valueEFuture
                        , 2 //round
                        , true
                        , true
                        , string.Empty);

                    if ((m_parent.m_tecView.adminValuesReceived == true)
                        && (m_parent.m_tecView.currHour == true))
                        showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblPBRrecVal - indxStartCommonPVal]
                            , m_parent.m_tecView.recomendation
                            , 2 //round
                            , true
                            , true
                            , string.Empty);
                    else
                        m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblPBRrecVal - indxStartCommonPVal].Text = "---";

                    double summ = 0;
                    //Для возможности восстановления значения
                    bool bPrevRecalcAver = m_parent.m_tecView.recalcAver;

                    if ((m_parent.m_tecView.currHour == true) && (min == 0))
                        m_parent.m_tecView.recalcAver = false;
                    else
                        ;

                    if (m_parent.m_tecView.recalcAver == true)
                    {
                        int hour = m_parent.m_tecView.lastHour;
                        if (hour == 24)
                            hour = 23;
                        else
                            ;

                        if (m_parent.m_tecView.currHour == true)
                        {
                            for (i = 1; i < m_parent.m_tecView.lastMin; i++)
                                summ += m_parent.m_tecView.m_valuesMins[i].valuesFact;
                            if (!(min == 0))
                                showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblAverPVal - indxStartCommonPVal]
                                    , summ / min
                                    , 2 //round
                                    , true
                                    , true
                                    , string.Empty);
                            else
                                m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblAverPVal - indxStartCommonPVal].Text = 0.ToString("F0");
                        }
                        else
                        {
                            //if ((m_parent.m_tecView.m_valuesHours.addonValues == true) && (hour == m_parent.m_tecView.m_valuesHours.hourAddon))
                            //    summ = m_parent.m_tecView.m_valuesHours.valuesFactAddon;
                            //else
                            summ = m_parent.m_tecView.m_valuesHours[hour].valuesFact;

                            showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblAverPVal - indxStartCommonPVal]
                                , summ
                                , 2 //round
                                , true
                                , true
                                , string.Empty);
                        }

                        //if (! ([lastHour] == 0))
                        double dblDevEVal = -1.0;
                        bool bDevEVal = true;
                        if ((m_parent.m_tecView.lastHour < m_parent.m_tecView.m_valuesHours.Length) &&
                            (!(m_parent.m_tecView.m_valuesHours[m_parent.m_tecView.lastHour].valuesUDGe == 0)))
                        {
                            dblDevEVal = ((((valueEBefore + valueECur + valueEFuture) -
                                        m_parent.m_tecView.m_valuesHours[m_parent.m_tecView.lastHour].valuesUDGe) / m_parent.m_tecView.m_valuesHours[m_parent.m_tecView.lastHour].valuesUDGe) * 100);
                            if (Math.Abs(dblDevEVal) < 100) ; else bDevEVal = false;

                            Logging.Logg().Debug(@"dblDevEVal=" + dblDevEVal.ToString(@"F3")
                                                + @" (valueEBefore=" + valueEBefore.ToString(@"F3")
                                                + @"; valueECur=" + valueECur.ToString(@"F3")
                                                + @"; valueEFuture=" + valueEFuture.ToString(@"F3")
                                                + @"; valuesUDGe=" + m_parent.m_tecView.m_valuesHours[m_parent.m_tecView.lastHour].valuesUDGe
                                                + @") [" + hour + @", " + m_parent.m_tecView.lastMin + @"]"
                                                , Logging.INDEX_MESSAGE.D_004);
                        }
                        else
                            bDevEVal = false;

                        if (bDevEVal == true)
                            showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblDevEVal - indxStartCommonPVal]
                                , dblDevEVal
                                , 2 //round
                                , true
                                , false
                                , @"%");
                        else
                            showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblDevEVal - indxStartCommonPVal]
                                , double.NegativeInfinity
                                , 2 //round
                                , true
                                , false
                                , @"---");
                    }

                    if ((m_parent.m_tecView.currHour == true) && (min == 0))
                        m_parent.m_tecView.recalcAver = bPrevRecalcAver;
                    else
                        ;

                    if (m_parent.m_tecView.currHour == true)
                    {
                        //if (m_parent.m_tecView.lastHourError == true)
                        //{
                        //    m_parent.m_tecView.ErrorReport("По текущему часу значений не найдено!");
                        //}
                        //else
                        //{
                        //    if (m_parent.m_tecView.lastHourHalfError == true)
                        //    {
                        //        m_parent.m_tecView.ErrorReport("За текущий час не получены некоторые получасовые значения!");
                        //    }
                        //    else
                        //    {
                        m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblAverPVal - indxStartCommonPVal].ForeColor =
                        m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].ForeColor =
                            m_lblPowerFactZoom.ForeColor = getColorValues(TG.INDEX_VALUE.FACT);

                        if (m_parent.m_tecView.m_markWarning.IsMarked((int)TecView.INDEX_WARNING.LAST_MIN) == true)
                        {
                            string strErrMsg = @"По текущему ";
                            if ((m_parent.m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                                || (m_parent.m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO)
                                || (m_parent.m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN))
                                strErrMsg += @"3-минутному";
                            else
                                if (m_parent.m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN)
                                strErrMsg += @"1-минутному";
                            else
                                ;
                            strErrMsg += @" отрезку значений не найдено!";
                            m_parent.m_tecView.ErrorReport(strErrMsg);
                        }
                        else
                        {
                            if (!(m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCurrentEVal - indxStartCommonPVal] == null))
                                m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCurrentEVal - indxStartCommonPVal].ForeColor =
                                    System.Drawing.Color.LimeGreen;
                            else;

                            if (!(m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblHourEVal - indxStartCommonPVal] == null))
                                m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblHourEVal - indxStartCommonPVal].ForeColor =
                                    System.Drawing.Color.Yellow;
                            else;
                        }
                        //    }
                        //}
                    }
                    else
                    {
                        m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblAverPVal - indxStartCommonPVal].ForeColor =
                        m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].ForeColor =
                            m_lblPowerFactZoom.ForeColor = System.Drawing.Color.OrangeRed;

                        if ((!(m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCurrentEVal - indxStartCommonPVal] == null)) && (!(m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblHourEVal - indxStartCommonPVal] == null)))
                            m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCurrentEVal - indxStartCommonPVal].ForeColor =
                            m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblHourEVal - indxStartCommonPVal].ForeColor =
                                System.Drawing.Color.OrangeRed;
                        else
                            ;
                    }
                    lblPBRNumber.Text = m_parent.m_tecView.lastLayout;
                    lblPBRNumber.BackColor = PBRState == TecViewStandard.PBR_STATE.NORM
                        ? Color.Empty
                            : HDataGridViewTables.s_dgvCellStyles[(int)HDataGridViewTables.INDEX_CELL_STYLE.ERROR].BackColor;

                    //ShowTGValue
                    i = 0;
                    if (m_parent.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC) // значит этот view будет суммарным для всех ГТП
                    {
                        foreach (TECComponent g in m_parent.m_tecView.LocalTECComponents)
                        {
                            if (g.IsGTP == true)
                                //Только ГТП
                                foreach (TG tg in g.ListLowPointDev)
                                {//Цикл по списку с ТГ
                                    //Отобразить значение
                                    if ((!(m_parent.m_tecView.m_dictValuesLowPointDev[tg.m_id].m_powerMinutes == null))
                                        //Ошибка при запуске 'CustomTecView' с оперативной панелью (Исправлено: 13.05.2015)
                                        // не "успевает" new ???
                                        && (!(m_parent.m_tecView.m_dictValuesLowPointDev[tg.m_id].m_powerMinutes[min] < 0)))
                                        showValue(m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.FACT]
                                            , m_parent.m_tecView.m_dictValuesLowPointDev[tg.m_id].m_powerMinutes[min]
                                            , 2 //round
                                            , true);
                                    else
                                    {
                                        m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.FACT].Text = "---";
                                        //m_tgToolTips[tg.m_id][(int)TG.INDEX_VALUE.FACT].SetToolTip (m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.FACT], @"Знач. недостоверно");
                                    }

                                    m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.FACT].ForeColor = getColorValues(TG.INDEX_VALUE.FACT);

                                    i++;
                                }
                            else
                                ;
                        }
                    }
                    else
                    {
                        foreach (TECComponent comp in m_parent.m_tecView.LocalTECComponents)
                        {
                            if ((!(m_parent.m_tecView.m_dictValuesLowPointDev[comp.ListLowPointDev[0].m_id].m_powerMinutes == null))
                                && (!(m_parent.m_tecView.m_dictValuesLowPointDev[comp.ListLowPointDev[0].m_id].m_powerMinutes[min] < 0)))
                                showValue(m_tgLabels[comp.ListLowPointDev[0].m_id][(int)TG.INDEX_VALUE.FACT]
                                    , m_parent.m_tecView.m_dictValuesLowPointDev[comp.ListLowPointDev[0].m_id].m_powerMinutes[min]
                                    , 2 //round
                                    , true);
                            else
                                m_tgLabels[comp.ListLowPointDev[0].m_id][(int)TG.INDEX_VALUE.FACT].Text = "---";

                            m_tgLabels[comp.ListLowPointDev[0].m_id][(int)TG.INDEX_VALUE.FACT].ForeColor = getColorValues(TG.INDEX_VALUE.FACT);

                            i++;
                        }
                    }

                    //Logging.Logg().Debug(@"PanelQuickData::ShowFactValues () - вЫход...");
                }
                else
                    ;
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

                    if (Equals (ContextMenuStrip, null) == false)
                        ContextMenuStrip.BackColor = value;
                    else
                        ;

                    if (Equals (m_lblPowerFactZoom, null) == false)
                        m_lblPowerFactZoom.BackColor = value;
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

            private void OnItemClick(object obj, EventArgs ev)
            {
                ((ToolStripMenuItem)obj).Checked = !((ToolStripMenuItem)obj).Checked;

                if (ContextMenuStrip.Items.IndexOf((ToolStripMenuItem)obj) == (int)INDEX_CONTEXTMENUITEM.TM)
                    m_parent.m_tecView.m_bLastValue_TM_Gen = ((ToolStripMenuItem)obj).Checked;
                else
                    ;

                RestructControl();
            }

            private TecViewStandard.PBR_STATE PBRState
            {
                get
                {
                    return ((Equals(m_parent, null) == false)
                            && (Equals (m_parent.m_tecView, null) == false)
                            && (Equals (m_parent.m_tecView.GetType(), typeof(TecViewStandard)) == true))
                        ? (m_parent.m_tecView as TecViewStandard).PBRState
                            : TecViewStandard.PBR_STATE.NORM;
                }
            }

            public void UpdateColorPbr()
            {
                lblPBRNumber.BackColor = PBRState == TecViewStandard.PBR_STATE.NORM
                    ? Color.Empty
                        : HDataGridViewTables.s_dgvCellStyles [(int)HDataGridViewTables.INDEX_CELL_STYLE.ERROR].BackColor;
            }
        }
    }
}

