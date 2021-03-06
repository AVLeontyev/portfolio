using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;


using StatisticCommon;
using ASUTP.Core;
using ASUTP.Control;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using ASUTP;

namespace Statistic
{
    partial class PanelCustomTecView
    {
        const Char CHAR_DELIM_PROP = '+'
                , CHAR_DELIM_LABEL = '&'
                , CHAR_DELIM_ARRAYITEM = ',';

        public class HLabelCustomTecView : Label
        {
            public static string s_ViewEmptyText = @"Добавить выбором пункта контекстного меню...";

            private PanelTecViewBase.LabelViewProperties m_propView;
           
            /// <summary>
            /// Событие - инициирует измекнение структуры элемента управления
            /// </summary>
            public event Action<PanelTecViewBase.LabelViewProperties> EventRestruct;

            /// <summary>
            /// Метод инициирующий возникновение события из-вне
            /// </summary>
            /// <param name="obj">Аргумент события</param>
            public void PerformRestruct(PanelTecViewBase.BANK_DEFAULT bankName)
            {
                EventRestruct (new PanelTecViewBase.LabelViewProperties (bankName));
            }

            /// <summary>
            /// Метод инициирующий возникновение события из-вне
            /// </summary>
            public void PerformRestruct()
            {
                EventRestruct (m_propView);
            }

            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public HLabelCustomTecView(PanelTecViewBase.BANK_DEFAULT bankName)
            {
                this.Dock = DockStyle.Fill;
                this.Text = s_ViewEmptyText;
                this.BorderStyle = BorderStyle.Fixed3D;
                this.TextAlign = ContentAlignment.MiddleCenter;
                //this.AutoSize = false; // по умолчанию 'false'
                this.SetAutoSizeMode(AutoSizeMode.GrowOnly);
                _fontDefault =
                _fontActual =
                    this.Font;

                //m_listIdContextMenuItems = new List<int>();

                m_propView = new PanelTecViewBase.LabelViewProperties (bankName);

                ForeColorChanged += new EventHandler (onForeColorChanged);
                BackColorChanged += new EventHandler (onBackColorChanged);

                //this.SizeChanged += new EventHandler (onSizeChanged);
            }

            /// <summary>
            /// Обработчик события - выбор п. меню
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void OnMenuItem_Content(object obj, EventArgs ev)
            {
                MenuItem mi;
                PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW indx = PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.UNKNOWN;

                mi = (MenuItem)obj;

                if (Equals(mi.Tag, null) == false)
                    if (typeof (PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW).IsAssignableFrom (mi.Tag.GetType ()) == true)
                        indx = (PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW)mi.Tag;
                    else if (typeof (Orientation).IsAssignableFrom (mi.Tag.GetType ()) == true)
                        indx = PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION;
                    else
                        ;
                else
                    indx = (PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW)((MenuItem)obj).Index;

                if (!(indx == PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION))
                    m_propView.SetProperty(indx, (PanelTecViewBase.VALUE)(((MenuItem)obj).Checked == true ? 0 : 1));
                else
                // ориентация размещения таблиц/гистограмм
                    m_propView.SetProperty(indx, (PanelTecViewBase.VALUE)mi.Index);

                EventRestruct (m_propView);

                ContentMenuStateChange ();

                ((PanelCustomTecView)Parent.Parent).EventContentChanged ();
            }
            
            /// <summary>
            /// Создать массив п. меню, управляющего содержанием элемента управления
            /// </summary>
            /// <returns>Массив п.п. контекстного меню</returns>
            private MenuItem[] createContentMenuItems()
            {
                PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW indx = PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.UNKNOWN;
                MenuItem[] arMenuItems = new MenuItem[(int)PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW];

                for (indx = 0; indx < PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW; indx++) {
                    arMenuItems [(int)indx] = new MenuItem (PanelTecViewBase.LabelViewProperties.GetText (indx), this.OnMenuItem_Content);
                    arMenuItems [(int)indx].Tag = indx;

                    if (indx == PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION) {
                        arMenuItems [(int)indx] = new MenuItem (PanelTecViewBase.LabelViewProperties.GetText (indx), new MenuItem [] {
                            new MenuItem (@"Вертикально", this.OnMenuItem_Content)
                            , new MenuItem (@"Горизонтально", this.OnMenuItem_Content)
                        });

                        arMenuItems [(int)indx].MenuItems[0].Tag = Orientation.Vertical;
                        arMenuItems [(int)indx].MenuItems [1].Tag = Orientation.Horizontal;
                    } else
                        ;
                }

                return arMenuItems;
            }

            /// <summary>
            /// Изменить состояние меню
            /// </summary>
            private void ContentMenuStateChange () {
                Menu.MenuItemCollection arMenuItems;

                arMenuItems =
                    ContextMenu.MenuItems [ContextMenu.MenuItems.Count - (COUNT_FIXED_CONTEXT_MENUITEM - INDEX_START_CONTEXT_MENUITEM)].MenuItems;

                for (PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW i = 0; i < PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.COUNT_PROPERTIES_VIEW; i ++) {
                    arMenuItems[(int)i].Enabled = m_propView.GetValue(i) == PanelTecViewBase.VALUE.DISABLED ? false : true;

                    if (i == PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW.ORIENTATION) {
                        if (arMenuItems[(int)i].Enabled == true) {
                            for (int j = 0; j < 2; j ++) {
                                arMenuItems[(int)i].MenuItems[j].RadioCheck = true;
                                arMenuItems[(int)i].MenuItems[j].Checked = j == (int)m_propView.GetValue (i);
                            }
                        }
                        else
                            ;
                    }
                    else {
                        arMenuItems[(int)i].Checked = m_propView.GetValue(i) == PanelTecViewBase.VALUE.ON;
                    }
                }
                
            }

            /// <summary>
            /// Добавить "постоянные" элементы в контекстное меню (Содержание, Очистить)
            /// </summary>
            /// <param name="indx">индекс 1-го из добавляемых пунктов (Содержание)</param>
            /// <param name="f">функция-обработчик выбора пункта очистить</param>
            public void AddContextMenuFixedMenuItems (int indx, EventHandler fClear) {
                this.ContextMenu.MenuItems.Add(@"-");
                this.ContextMenu.MenuItems.Add(@"Содержание", createContentMenuItems());
                this.ContextMenu.MenuItems[indx].Enabled = false;
                this.ContextMenu.MenuItems.Add(@"Очистить");
                this.ContextMenu.MenuItems[ContextMenu.MenuItems.Count - 1].Click += fClear;

                ContentMenuStateChange ();
            }

            //TODO:
            public void onForeColorChanged(object obj, EventArgs ev)
            {
            //    if (Equals (ContextMenuStrip, null) == false)
            //        ContextMenu.ForeColor = (obj as Control).ForeColor;
            //    else
            //        ;
            }

            //TODO:
            public void onBackColorChanged(object obj, EventArgs ev)
            {
            //    if (Equals (ContextMenuStrip, null) == false)
            //        ContextMenu.BackColor = (obj as Control).BackColor;
            //    else
            //        ;
            }

            /// <summary>
            /// Состояние элемента управления
            ///  0/1 - нет/есть объекта отображения
            /// </summary>
            protected bool _state;
            /// <summary>
            /// Шрифт для подписи элемента управления
            /// </summary>
            private System.Drawing.Font _fontDefault
                , _fontActual;
            /// <summary>
            /// Цвет шрифта для подписи элемента управления
            /// </summary>
            Color _color { get { return _state == true ? Color.Red : Color.Black; } }

            /// <summary>
            /// Установить признак "Доступность" для п. меню
            /// </summary>
            /// <param name="indx">Индекс п. меню</param>
            /// <param name="bEnabled">Признпк "Доступность"</param>
            public void EnableContextMenuItem(int indx, bool bEnabled)
            {
                ContextMenu.MenuItems[indx].Enabled =
                _state =
                    bEnabled;
                // доработка по просьбе Заказчика 15.01.2016 г.
                //  увеличение читабельности (заметности) подписи
                if (bEnabled == true)
                {
                    FitFont();
                }
                else
                // только при наличии предыдущего шрифта
                    if (!(_fontDefault == null))
                    {
                        setFont(_fontDefault, _color);
                    }
                    else
                        ;
            }

            /// <summary>
            /// Установить шрифт и цвет шрифта для подписи
            /// </summary>
            /// <param name="font">Устанавливаемый шрифт</param>
            /// <param name="color">Устанавливаемый цвет шрифта</param>
            private void setFont(Font font, Color color)
            {
                this.Font = new System.Drawing.Font(
                    font.FontFamily
                    , font.Size
                    , font.Style
                    , font.Unit
                    , font.GdiCharSet
                );

                this.ForeColor = color;
            }

            /// <summary>
            /// Применить актуальный размер шрифта
            /// </summary>
            public void FitFont()
            {
                if (_state == true)
                {
                    _fontActual = HLabel.FitFont(this.CreateGraphics(), Text, ClientSize, new SizeF(0.95F, 0.95F), 0.05F);

                    setFont((!(_fontActual == null)) ? _fontActual : _fontDefault, _color);
                }
                else
                    ;
            }

            public FormChangeMode.KeyDevice CurrentKeyDevice
            {
                get
                {
                    return getKeyDeviceMenuItemChecked ();
                }
            } 

            /// <summary>
            /// Возвратить идентификатор п. меню с установленным признаком "Использовать"
            /// </summary>
            /// <returns>Идентификатор п. меню</returns>
            private FormChangeMode.KeyDevice getKeyDeviceMenuItemChecked()
            {
                FormChangeMode.KeyDevice keyRes = FormChangeMode.KeyDevice.Empty;
                int indxMenuItem = -1;

                // найти индекс п. меню
                foreach (MenuItem mi in ContextMenu.MenuItems)
                {
                    indxMenuItem = ContextMenu.MenuItems.IndexOf(mi);
                    if (indxMenuItem < (ContextMenu.MenuItems.Count - COUNT_FIXED_CONTEXT_MENUITEM))
                        if (mi.Checked == true)
                            break;
                        else
                            ;
                    else
                        ;
                }

                if (indxMenuItem < (ContextMenu.MenuItems.Count - COUNT_FIXED_CONTEXT_MENUITEM))
                // присвоить значение идентификатора
                    keyRes = (FormChangeMode.KeyDevice)ContextMenu.MenuItems [indxMenuItem].Tag;

                else
                // идентификатор для этого п. меню нет
                    ;

                return keyRes;
            }

            /// <summary>
            /// Изменить содержимое ячейки для объекта отображения 
            /// </summary>
            /// <param name="profile">Массив изменяемых парметров объекта отображения</param>
            public void LoadProfile (FormChangeMode.KeyDevice key, PanelTecViewBase.LabelViewProperties properties)
            {
                loadProfile (key, properties);
            }

            /// <summary>
            /// Изменить содержимое ячейки для объекта отображения 
            /// </summary>
            /// <param name="arProp">Массив изменяемых парметров объекта отображения</param>
            public void LoadProfile(string []arProp)
            {
                Func<string, PanelTecViewBase.VALUE> parseValue = delegate (string put) {
                    PanelTecViewBase.VALUE valRes = PanelTecViewBase.VALUE.DISABLED;

                    try {
                        valRes = Enum.IsDefined (typeof (PanelTecViewBase.VALUE), put) == true
                            ? (PanelTecViewBase.VALUE)Enum.Parse (typeof (PanelTecViewBase.VALUE), put)
                                : (PanelTecViewBase.VALUE)Int32.Parse (put);
                    } catch {
                    }

                    return valRes;
                };

                loadProfile (new FormChangeMode.KeyDevice { Id = Int32.Parse (arProp [1]), Mode = FormChangeMode.MODE_TECCOMPONENT.GTP }
                    , new PanelTecViewBase.LabelViewProperties (arProp [2].Split (CHAR_DELIM_ARRAYITEM).Select<string, PanelTecViewBase.VALUE> (parseValue).ToList()));
            }

            private void loadProfile (FormChangeMode.KeyDevice key, PanelTecViewBase.LabelViewProperties properties)
            {
                MenuItem mItem;
                int indxItem = -1
                    , shift = -1;

                Func<PanelTecViewBase.VALUE, string> valueToString = delegate (PanelTecViewBase.VALUE v) {
                    return v.ToString ();
                };

                //Очистить
                ContextMenu.MenuItems [ContextMenu.MenuItems.Count - 1].PerformClick ();
                //Установить параметры содержания отображения
                if (!(properties.Count < m_propView.Length))
                    if (properties.Count == m_propView.Length)
                        for (int i = 0; i < m_propView.Length; i++)
                            try {
                                m_propView = new PanelTecViewBase.LabelViewProperties (properties);
                            } catch (Exception e) {
                            }
                    else if (properties.Count % m_propView.Length == 0) {
                        shift = (properties.Count / m_propView.Length - 1) * m_propView.Length;
                        for (int i = 0; i < m_propView.Length; i++)
                            try {
                                m_propView.SetValue ((PanelTecViewBase.LabelViewProperties.INDEX_PROPERTIES_VIEW)i, properties[i + shift]);
                            } catch (Exception e) {
                            }
                    } else
                        ;
                else
                    ; //Ошибка ...

                try {
                    //Назначить объект
                    //int indx = m_listIdContextMenuItems.IndexOf(Int32.Parse(arProp[1]))
                    mItem = ContextMenu.MenuItems.Cast<MenuItem> ().First (mi => ((FormChangeMode.KeyDevice)mi.Tag).Id == key.Id);
                    indxItem = ContextMenu.MenuItems.IndexOf (mItem);
                    if ((!(indxItem < 0))
                        && (indxItem < ContextMenu.MenuItems.Count - COUNT_FIXED_CONTEXT_MENUITEM)) {
                        // инициировать операции по выбору п. меню
                        mItem.PerformClick ();
                        // изменить состояние п. меню
                        ContentMenuStateChange ();
                    } else
                        ; //??? Ошибка: не найден
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, $"HLabelCustomTecView::loadProfile () - Key={key.ToString()}, properties={string.Join (", ", properties.Select<PanelTecViewBase.VALUE, string>(valueToString).ToArray())}"
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Возвратить строку с закодированными настройками объекта отображения
            /// </summary>
            /// <returns>Строка для восстановления внешнего вида объекта при очередном вызове на отображение</returns>
            public object SaveProfile()
            {
                object objRes = null;
                FormChangeMode.KeyDevice key_device;

                key_device = getKeyDeviceMenuItemChecked ();

                if (!(key_device.Id < 0))
                    switch (PanelCustomTecView._modeProfile) {
                        case MODE_PROFILE.STRING:
                            //Идентификатор объекта...
                            objRes += key_device.Id.ToString ();
                            objRes = $"{objRes}{CHAR_DELIM_PROP}";
                            //Параметры объекта...
                            objRes = $"{objRes}{string.Join ($"{CHAR_DELIM_ARRAYITEM}", m_propView.ToArray ().Select (prop => prop.ToString ()).ToArray())}";
                            break;
                        case MODE_PROFILE.OBJECT_XML:
                        case MODE_PROFILE.OBJECT_BIN:
                        case MODE_PROFILE.OBJECT_JSON_NET:
                        case MODE_PROFILE.OBJECT_JSON_NEWTON:
                            //Идентификатор объекта... и параметры объекта...
                            objRes = m_propView;
                            break;
                        default:
                            break;
                    }
                else
                    ;

                return objRes;
            }
        }

        int m_indxContentMenuItem;
        /// <summary>
        /// Количество фиксированных п.п.  контекстного меню
        /// </summary>
        static int COUNT_FIXED_CONTEXT_MENUITEM = 3;
        static int INDEX_START_CONTEXT_MENUITEM = 1;
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

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            MenuItem mItem;

            components = new System.ComponentModel.Container();

            PanelTecViewBase [] arPanelTecViewTable = new PanelTecViewBase [this.RowCount * this.ColumnCount];

            m_arLabelEmpty = new HLabelCustomTecView[this.RowCount * this.ColumnCount];
            //m_arControls = new Controls[this.RowCount * this.ColumnCount];

            m_indxContentMenuItem = m_formChangeMode.m_MainFormContextMenuStripListTecViews.Items.Count + INDEX_START_CONTEXT_MENUITEM;

            for (int i = 0; i < arPanelTecViewTable.Length; i ++) {
                m_arLabelEmpty[i] = new HLabelCustomTecView(PanelTecViewBase.BANK_DEFAULT.HOUR_TABLE_GRAPH);

                m_arLabelEmpty [i].ContextMenu = new ContextMenu ();
                //foreach (ToolStripItem tsi in m_formChangeMode.m_MainFormContextMenuStripListTecViews.Items)
                foreach (FormChangeMode.ListBoxItem item in m_formChangeMode.m_listItems)
                {
                    if ((item.bVisibled == true)
                        && (item.key.Id < FormChangeMode.ID_ADMIN_TABS [(int)FormChangeMode.MANAGER.DISP]))
                    {
                        mItem = createMenuItem (item.name_shr);
                        m_arLabelEmpty [i].ContextMenu.MenuItems.Add(mItem);
                        mItem.Tag = item.key;
                        //m_arLabelEmpty[i].m_listIdContextMenuItems.Add(item.id);
                    }
                    else
                        ;
                }

                m_arLabelEmpty[i].AddContextMenuFixedMenuItems(m_indxContentMenuItem, MenuItem_OnClick);

                this.Controls.Add(m_arLabelEmpty [i], getAddress (i).Y, getAddress (i).X);

                //m_arControls [i] = m_arLabelEmpty [i];
            }

            m_formChangeMode.EventMenuItemsClear += new DelegateFunc(OnMenuItemsClear);
            m_formChangeMode.EventMenuItemAdd += new DelegateStringFunc (OnMenuItemAdd);

            initializeLayoutStyle ();

            this.Dock = DockStyle.Fill;

            this.SizeChanged += new EventHandler(onSizeChanged);
        }

        #endregion

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly ();
        }
    }

    public partial class PanelCustomTecView : PanelStatistic
    {
        public event DelegateFunc EventContentChanged;

        private HLabelCustomTecView[] m_arLabelEmpty;
        //private Control[] m_arControls;

        //public bool m_bIsActive;

        private FormChangeMode m_formChangeMode;
        DelegateStringFunc m_fErrorReport, m_fWarningReport, m_fActionReport;
        DelegateBoolFunc m_fReportClear;

        private Point getAddress (int indx) {
            Point ptRes = new Point(indx % this.RowCount, indx / this.ColumnCount);

            return ptRes;
        }

        public PanelCustomTecView(FormChangeMode formCM, Size sz/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fREpClr*/)
            : base (MODE_UPDATE_VALUES.AUTO, FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor)
        {
            m_formChangeMode = formCM;

            this.RowCount = sz.Height;
            this.ColumnCount = sz.Width;

            //SetDelegateReport (fErrRep, fWarRep, fActRep, fREpClr);

            InitializeComponent();
        }

        public PanelCustomTecView(IContainer container, FormChangeMode formCM, Size sz/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : this(formCM, sz/*, fErrRep, fWarRep, fActRep, fRepClr*/)
        {
            container.Add(this);
        }

        public override void Start()
        {
            base.Start ();
            
            foreach (Control panel in this.Controls)
            {
                if (panel is PanelTecViewBase) ((PanelTecViewBase)panel).Start(); else ;
            }
        }

        public override void Stop () {
            foreach (Control panel in this.Controls)
            {
                if (panel is PanelTecViewBase) ((PanelTecViewBase)panel).Stop(); else ;
            }

            base.Stop ();
        }

        public override bool Activate (bool active) {
            bool bRes = base.Activate(active);
            
            foreach (Control panel in this.Controls)
            {
                if (panel is PanelTecViewBase) ((PanelTecViewBase)panel).Activate(active); else ;
            }

            return bRes;
        }

        //protected override void initTableHourRows()
        //{
        //    //Ничего не делаем, т.к. составные элементы самостоятельно настраивают кол-во строк в таблицах
        //}

        protected void Clear () {
        }

        public override void SetDelegateReport (DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr) {
            m_fErrorReport = fErrRep;
            m_fWarningReport = fWarRep;
            m_fActionReport = fActRep;
            m_fReportClear = fRepClr;

            foreach (var child in Controls) {
                if (child is PanelTecViewBase) {
                    (child as PanelTecViewBase).m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);
                } else {
                }
            }
        }

        /// <summary>
        /// Метод непосредственного применения параметров графического представления данных
        /// </summary>
        /// <param name="type">Тип изменившихся параметров</param>
        public override void UpdateGraphicsCurrent(int type)
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecViewBase)
                {
                    ((PanelTecViewBase)ctrl).UpdateGraphicsCurrent (type);
                }
                else
                    ;
            }
        }

        private MenuItem createMenuItem (string nameItem) {
            MenuItem menuItemRes = new MenuItem (nameItem);
            menuItemRes.RadioCheck = true;
            menuItemRes.Click += new EventHandler(MenuItem_OnClick);

            return menuItemRes;
        }

        /// <summary>
        /// Обработчик события - изменение
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (панель)</param>
        /// <param name="ev">Аргумент события</param>
        private void onSizeChanged(object obj, EventArgs ev)
        {
            foreach (HLabelCustomTecView label in m_arLabelEmpty)
                label.FitFont();
        }

        private void OnMenuItemsClear  () {
            foreach (HLabelCustomTecView le in m_arLabelEmpty)
            {
                while (le.ContextMenu.MenuItems.Count > COUNT_FIXED_CONTEXT_MENUITEM) {
                    le.ContextMenu.MenuItems.RemoveAt (0);
                }

                //le.m_listIdContextMenuItems.Clear();
            }

            m_indxContentMenuItem = INDEX_START_CONTEXT_MENUITEM;
        }
        /// <summary>
        /// Обработчик события - разместить на панели объект отображения
        /// </summary>
        /// <param name="item">Строка с параметрами (идентификатор, наименование) объекта отображения</param>
        private void OnMenuItemAdd (string item) {
            int indx = -1
                , id = Int32.Parse (item.Split (';')[0]);
            MenuItem mItem;
            string nameItem = item.Split(';')[1];
            foreach (HLabelCustomTecView le in m_arLabelEmpty)
            {
                indx = le.ContextMenu.MenuItems.Count - COUNT_FIXED_CONTEXT_MENUITEM;
                if (indx < 0) indx = 0; else ;
                mItem = createMenuItem (nameItem);
                le.ContextMenu.MenuItems.Add(indx, mItem);
                //le.m_listIdContextMenuItems.Add(id);
                mItem.Tag = new FormChangeMode.KeyDevice { Id = id, Mode = TECComponent.GetMode (id) };
            }

            m_indxContentMenuItem ++;
        }
        /// <summary>
        /// Обработчик событияе - ывбор п. контекстного меню
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (п. меню)</param>
        /// <param name="ev">Аргумент события</param>
        private void MenuItem_OnClick(object obj, EventArgs ev)
        {
            int indxLabel = -1
                , indx = ((MenuItem)obj).Index;
            FormChangeMode.KeyDevice key_device = FormChangeMode.KeyDevice.Empty;
            TEC tec = null;
            TECComponent tec_comp = null;
            Point ptAddress;
            PanelTecViewBase panelTecView = null;

            foreach (HLabelCustomTecView le in m_arLabelEmpty)
                if (le.ContextMenu == ((ContextMenu)((MenuItem)obj).Parent)) {
                    indxLabel++;
                    break;
                } else
                    indxLabel++;

            if ((indxLabel < 0)
                || (!(indxLabel < m_arLabelEmpty.Length)))
                return;
            else
                ;

            if (indx == ((ContextMenu)((MenuItem)obj).Parent).MenuItems.Count - 1) {
                //Не устанавливать признак "выбран" для крайнего пункта
                ((MenuItem)obj).Checked = false;
                //Снять с отображения
                foreach (MenuItem mi in ((ContextMenu)((MenuItem)obj).Parent).MenuItems) {
                    if (mi.Checked == true) {
                        mi.Checked = false;
                    } else
                        ;
                }
                clearAddress (indxLabel);

                EnableLabelContextMenuItem (indxLabel, false);
            } else {
                if (((MenuItem)obj).Checked == true) {
                    //Снять с отображения
                    ((MenuItem)obj).Checked = false;
                    clearAddress (indxLabel);

                    EnableLabelContextMenuItem (indxLabel, false);  //.ContextMenu.MenuItems [m_indxContentMenuItem].Enabled = false;
                } else {
                    //Снять с отображения
                    foreach (MenuItem mi in ((ContextMenu)((MenuItem)obj).Parent).MenuItems) {
                        if ((mi.Checked == true) && (!(mi.Index == indx))) {
                            mi.Checked = false;
                        } else
                            ;
                    }
                    clearAddress (indxLabel);

                    //Вызвать на отображение
                    ((MenuItem)obj).Checked = true;
                    // отображаем вкладки ТЭЦ - аналог FormMain::сменитьРежим...
                    key_device = (FormChangeMode.KeyDevice)(obj as MenuItem).Tag;
                    switch (key_device.Mode) {
                        case FormChangeMode.MODE_TECCOMPONENT.TEC:
                            tec = m_formChangeMode.m_list_tec.Find (t => t.m_id == key_device.Id);
                            break;
                        case FormChangeMode.MODE_TECCOMPONENT.GTP:
                            foreach (TEC t in m_formChangeMode.m_list_tec) {
                            }
                            tec = m_formChangeMode.m_list_tec.Find (t => {
                                return !(t.ListTECComponents.FindIndex(comp => comp.m_id == key_device.Id) < 0);
                            });
                            tec_comp = tec.ListTECComponents.Find (comp => comp.m_id == key_device.Id);
                            break;
                        default:
                            break;
                    }
                    ptAddress = getAddress (indxLabel);

                    panelTecView = null;
                    try {
                        if (tec.m_id > (int)TECComponent.ID.LK)
                            panelTecView = new PanelLKView (tec, key_device, m_arLabelEmpty [indxLabel]);
                        else
                            panelTecView = new PanelTecView (tec, key_device, m_arLabelEmpty [indxLabel]);
                        //        new PanelTecView(m_formChangeMode.m_list_tec[tec_index], tec_index, TECComponent_index, m_arLabelEmpty[indxLabel]/*, m_fErrorReport, m_fWarningReport, m_fActionReport, m_fReportClear*/);
                        panelTecView.SetDelegateReport (m_fErrorReport, m_fWarningReport, m_fActionReport, m_fReportClear);
                        this.Controls.Add (panelTecView, ptAddress.Y, ptAddress.X);
                        this.Controls.SetChildIndex (panelTecView, indxLabel);
                        indxLabel = this.Controls.GetChildIndex (panelTecView);
                        ((PanelTecViewBase)this.Controls [indxLabel]).Start ();
                        // 2018-04-10 KhryapinAN 'Activate' только в случае активности родительской панели
                        if (Actived == true)
                            ((PanelTecViewBase)this.Controls [indxLabel]).Activate (true);
                        else
                            ;
                    } catch (Exception e) {
                        ASUTP.Logging.Logg ().Exception (e, $"PanelCustomTecView::MenuItem_OnClick () - Индекс={indxLabel}, KeyDevice={key_device.ToString()}..."
                            , ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    EnableLabelContextMenuItem (indxLabel, true);
                }
            }

            EventContentChanged ();
        }
        /// <summary>
        /// Включить/отключить п. контекстного меню
        /// </summary>
        /// <param name="indxLabel">Индекс панели</param>
        /// <param name="bEnabled">Признак включения/отключения</param>
        private void EnableLabelContextMenuItem(int indxLabel, bool bEnabled)
        {
            m_arLabelEmpty[indxLabel].EnableContextMenuItem(m_indxContentMenuItem, bEnabled);
        }
        /// <summary>
        /// Очистить панель (снять с отображения объект) по указанному индексу
        /// </summary>
        /// <param name="indx">Индекс панели</param>
        private void clearAddress (int indx) {
            PanelTecViewBase pnlTecView = null;
            // найти панель по индексу
            foreach (Control panel in this.Controls)
            {
                if ((panel is PanelTecViewBase) && (this.Controls.IndexOf (panel) == indx)) {
                    pnlTecView = (PanelTecViewBase)panel;

                    break;
                }
                else
                    ;
            }
            // остановить панель, удалить
            if (! (pnlTecView == null)) {
                pnlTecView.Activate(false);
                pnlTecView.Stop();

                this.Controls.Remove(pnlTecView);
                
                pnlTecView = null;
            }
            else
                ;
            // добавить пустую панель
            Point ptAddress = getAddress (indx);
            m_arLabelEmpty[indx].Text = HLabelCustomTecView.s_ViewEmptyText;
            this.Controls.Add (m_arLabelEmpty [indx], ptAddress.Y, ptAddress.X);
            this.Controls.SetChildIndex(m_arLabelEmpty[indx], indx);
        }
        /// <summary>
        /// Разбор строки с настройками всех панелей (отображаемые объекты, состав отображаемой информации)
        ///  для восстановления
        /// </summary>
        /// <param name="profile">Строка с настройками всех панелей</param>
        public void LoadProfile(string profile)
        {
            MODE_PROFILE modeAutoDetected = MODE_PROFILE.UNKNOWN;

            string [] arLabel = null;

            object ser;
            List<LabelProfile> listLabelProfiles = null;

            //TODO: какое-то преобразование для 'OBJECT_JSON_NET'
            Func<object, LabelProfile> parseLabelProfile = delegate (object obj) {
                return new LabelProfile ();
            };

            // определить режим с помощью клторого выполнено сохранение профиля
            foreach (MODE_PROFILE mode in Enum.GetValues (typeof (MODE_PROFILE))) {
                try {
                    //Debug.WriteLine ($"::LoadProfile (MODE={mode.ToString()}, profile={profile}) - ...");

                    switch (mode) {
                        case MODE_PROFILE.OBJECT_XML:
                            listLabelProfiles = new List<LabelProfile> ();
                            ser = new XmlSerializer (listLabelProfiles.GetType());
                            using (MemoryStream buffer = new MemoryStream (Convert.FromBase64String (profile))) {
                                listLabelProfiles = (List<LabelProfile>)((XmlSerializer)ser).Deserialize (buffer);
                            }

                            modeAutoDetected = mode;
                            break;
                        case MODE_PROFILE.OBJECT_BIN:
                            listLabelProfiles = new List<LabelProfile> ();
                            ser = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            using (MemoryStream buffer = new MemoryStream (Convert.FromBase64String (profile))) {
                                listLabelProfiles = (List<LabelProfile>)((System.Runtime.Serialization.Formatters.Binary.BinaryFormatter)ser).Deserialize (buffer);
                            }

                            modeAutoDetected = mode;
                            break;
                        case MODE_PROFILE.OBJECT_JSON_NET:
                            listLabelProfiles = new List<LabelProfile> ();
                            ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                            listLabelProfiles = ((Array)((System.Web.Script.Serialization.JavaScriptSerializer)ser).DeserializeObject(profile)).Cast<object>().Select<object, LabelProfile>(parseLabelProfile).ToList();

                            modeAutoDetected = mode;
                            break;
                        case MODE_PROFILE.OBJECT_JSON_NEWTON:
                            listLabelProfiles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LabelProfile>> (profile);

                            modeAutoDetected = mode;
                            break;
                        case MODE_PROFILE.STRING:
                            arLabel = profile.Split (new char [] { CHAR_DELIM_LABEL }, StringSplitOptions.RemoveEmptyEntries);

                            if (arLabel.Length > 0)
                                modeAutoDetected = mode;
                            else
                                ;
                            break;
                        default:
                            break;
                    }
                } catch (Exception e) {
                    ASUTP.Logging.Logg ().Warning ($"PanelCustomTecView::LoadProfile () - распознавание профиля, Mode={mode}...", ASUTP.Logging.INDEX_MESSAGE.NOT_SET);
                }

                if (!(modeAutoDetected == MODE_PROFILE.UNKNOWN))
                    break;
                else
                    ;
            }

            // применить определуннвй режим для распознавания значений профиля
            switch (modeAutoDetected) {
                case MODE_PROFILE.OBJECT_XML:
                case MODE_PROFILE.OBJECT_BIN:
                case MODE_PROFILE.OBJECT_JSON_NET:
                case MODE_PROFILE.OBJECT_JSON_NEWTON:
                    if (Equals(listLabelProfiles, null) == false)
                        foreach (LabelProfile label in listLabelProfiles) {
                            if ((Equals(label.Key, FormChangeMode.KeyDevice.Empty) == false)
                                && (Equals (label.Properties, null) == false))
                                m_arLabelEmpty [label.Coordinate].LoadProfile (label.Key, label.Properties);
                            else
                                ;
                        }
                    else
                        ;
                    break;
                case MODE_PROFILE.STRING:
                    foreach (string label in arLabel) {
                        string [] arProp = label.Split (CHAR_DELIM_PROP);

                        if (arProp.Length == 0)
                            ; //Ошибка...
                        else
                            if (arProp.Length == 1)
                                ; //"Пустая"...
                            else
                                if (arProp.Length > 1)
                                if (!(arProp.Length == 3))
                                    ; //Ошибка...
                                else
                                    m_arLabelEmpty [Int32.Parse (arProp [0])].LoadProfile (arProp);
                    }
                    break;
                default:
                    break;
            }
        }

        private enum MODE_PROFILE
        {
            UNKNOWN = -1, OBJECT_XML, OBJECT_BIN, OBJECT_JSON_NEWTON, OBJECT_JSON_NET, STRING
        }

        private static MODE_PROFILE _modeProfile = MODE_PROFILE.OBJECT_JSON_NEWTON;

        [Serializable]
        public class LabelProfile
        {
            public int Coordinate;

            //public PanelTecViewBase.LabelCustomTecViewProfile Value;

            public FormChangeMode.KeyDevice Key;

            public PanelTecViewBase.LabelViewProperties Properties; // { get; }
        }

        //[Serializable]
        //public class ListLabelProfiles : List<LabelProfile>
        //{
        //}

        /// <summary>
        /// Возвратить строку с настройками всех панелей (отображаемые объекты, состав отображаемой информации)
        ///  для их автоматического восстановления при очередном запуске на выполнение приложения
        /// </summary>
        /// <returns>Строка с настройками, подготовленная к записи в БД</returns>
        public string SaveProfile()
        {
            string strRes = string.Empty;
            object ser;
            List<LabelProfile> listlabelProfiles;
            LabelProfile labelProfile;
            int i = -1;

            switch (_modeProfile) {
                case MODE_PROFILE.STRING:
                    for (i = 0; i < m_arLabelEmpty.Length; i ++)
                    {
                        //Координаты...
                        strRes += i.ToString(); strRes += CHAR_DELIM_PROP;
                        //Содержание
                        strRes += m_arLabelEmpty[i].SaveProfile();
                        //Разделитель
                        strRes += CHAR_DELIM_LABEL;
                    }

                    //Обрезать лишний символ-разделитель 'CHAR_DELIM_LABEL'
                    strRes = strRes.Substring(0, strRes.Length - 1);
                    break;
                case MODE_PROFILE.OBJECT_XML:
                case MODE_PROFILE.OBJECT_BIN:
                case MODE_PROFILE.OBJECT_JSON_NET:
                case MODE_PROFILE.OBJECT_JSON_NEWTON:
                    listlabelProfiles = new List<LabelProfile> ();
                    for (i = 0; i < m_arLabelEmpty.Length; i++) {
                        labelProfile = new LabelProfile ();
                        labelProfile.Coordinate = i;
                        labelProfile.Key = m_arLabelEmpty [i].CurrentKeyDevice;
                        labelProfile.Properties = (PanelTecViewBase.LabelViewProperties)m_arLabelEmpty [i].SaveProfile ();

                        listlabelProfiles.Add (labelProfile);
                    }

                    if (_modeProfile == MODE_PROFILE.OBJECT_XML) {
                        ser = new XmlSerializer (listlabelProfiles.GetType ());

                        using (StringWriter buffer = new StringWriter ()) {
                            ((XmlSerializer)ser).Serialize (buffer, listlabelProfiles);
                            strRes = buffer.ToString ();
                        }
                    } else if (_modeProfile == MODE_PROFILE.OBJECT_BIN) {
                        ser = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();

                        using (MemoryStream stream = new MemoryStream ()) {
                            ((System.Runtime.Serialization.Formatters.Binary.BinaryFormatter)ser).Serialize (stream, listlabelProfiles);
                            strRes = Convert.ToBase64String (stream.ToArray ());
                        }
                    } else if (_modeProfile == MODE_PROFILE.OBJECT_JSON_NET) {
                        ser = new System.Web.Script.Serialization.JavaScriptSerializer ();
                        strRes = ((System.Web.Script.Serialization.JavaScriptSerializer)ser).Serialize (listlabelProfiles);
                    } else if (_modeProfile == MODE_PROFILE.OBJECT_JSON_NEWTON) {
                        strRes = Newtonsoft.Json.JsonConvert.SerializeObject (listlabelProfiles);
                    } else
                        ;

                    //Debug.WriteLine ($"::SaveProfile (MODE={_modeProfile.ToString()}, profile={strRes}) - ...");
                    break;
                default:
                    break;
            }

            return strRes;
        }
    }
}
