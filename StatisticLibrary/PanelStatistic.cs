using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Drawing; //Color..


using System.Linq;
using ASUTP.Control;
using ASUTP.Core;
using ASUTP.Helper;

namespace StatisticCommon
{
    public abstract class PanelStatistic : HPanelCommon
    {
        /// <summary>
        /// Перечисление - возможные значения для режима обновления значений
        /// </summary>
        public enum MODE_UPDATE_VALUES { NOT_SET, AUTO, ACTION }
        /// <summary>
        /// Режим обновления значений
        /// </summary>
        protected MODE_UPDATE_VALUES _modeUpdateValues;
        /// <summary>
        /// Делегаты для отображения/скрытия элемента управления
        ///  , визуализирующего длительный процесс обработки запроса
        /// </summary>
        protected DelegateFunc delegateStartWait
            , delegateStopWait;
        /// <summary>
        /// Делегат при обработке события обновления (??? чего)
        /// </summary>
        protected DelegateFunc delegateEventUpdate;
        /// <summary>
        /// Фильтр для диалогового окна открыть шаблон/макет книги MS Excel  для импорта/экспорта
        /// </summary>
        protected static string s_DialogMSExcelBrowseFilter = @"Книга MS Excel 2003|*.xls|Книга MS Excel 2010|*.xlsx";
        /// <summary>
        /// Коструктор - основной (с аогументами)
        /// </summary>
        /// <param name="cCols">Количество столбцов в макете для размещения элементов управления</param>
        /// <param name="cRows">Количество строк в макете для размещения элементов управления</param>
        public PanelStatistic (MODE_UPDATE_VALUES modeUpdateValues, Color foreColor, Color backColor, int cCols = -1, int cRows = -1)
            : base(cCols, cRows)
        {
            Control formMain = null;

            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo;

            while ((!(formMain is Form))
                && (Equals(Parent, null) == false))
                formMain = Equals(formMain, null) == true ? Parent.Parent : formMain.Parent;

            if (Equals (formMain, null) == false) {
                formMain.BackColorChanged += formMain_BackColorChanged;
                formMain.ForeColorChanged += formMain_ForeColorChanged;
            } else if (Application.OpenForms.Count > 0) {
                Application.OpenForms [0].BackColorChanged += formMain_BackColorChanged;
                Application.OpenForms [0].ForeColorChanged += formMain_ForeColorChanged;
            } else
                ;

            BackColor = backColor;
            ForeColor = foreColor;
            _modeUpdateValues = modeUpdateValues;
        }

        /// <summary>
        /// Найти все дочерние для 'ctrl' объекты с типами(наследованными от них), указанных в аргументе
        /// </summary>
        /// <param name="ctrl">Элемент интерфйеса</param>
        /// <param name="types">Последовательность типов, объекты которых необходимо искать</param>
        /// <returns>Последовательность элементов</returns>
        protected IEnumerable<Control> getTypedControls (Control ctrl, IEnumerable<Type> types)
        {
            List<Control> listRes = new List<Control>();

            types.ToList ().ForEach (type => listRes.AddRange (from child in ctrl.Controls.Cast<Control> ()
                                                                where ((child.GetType ().IsSubclassOf (type)) || (child.GetType ().Equals(type) == true))
                                                                select child));

            ctrl.Controls.Cast<Control> ().ToList ().ForEach (child => listRes.AddRange (getTypedControls(child, types)));

            return listRes;
        }

        protected virtual void formMain_ForeColorChanged (object sender, EventArgs e)
        {
            List<Control> listCtrlDoChangeForeColor;

            ForeColor = (sender as Form).ForeColor;

            listCtrlDoChangeForeColor = getTypedControls (this, new List<Type> () { typeof (DataGridView), typeof (ZedGraph.ZedGraphControl) }).ToList ();

            listCtrlDoChangeForeColor.ForEach (ctrl => ctrl.ForeColor = ForeColor.Equals (SystemColors.ControlText) == false ? ForeColor : SystemColors.ControlText);
        }

        /// <summary>
        /// Обработчик события - изменение цветовой схемы отображения
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие(форма)</param>
        /// <param name="e">Аргумент события</param>
        protected virtual void formMain_BackColorChanged (object sender, EventArgs e)
        {
            List<Control> listCtrlDoChangeBackColor;

            BackColor = (sender as Form).BackColor;

            listCtrlDoChangeBackColor = getTypedControls (this, new List<Type> () { typeof(DataGridView), typeof(ZedGraph.ZedGraphControl) }).ToList();

            listCtrlDoChangeBackColor.ForEach (ctrl => ctrl.BackColor = BackColor.Equals (SystemColors.Control) == false ? BackColor : SystemColors.Control ); //??? SystemColors.Window
        }

        public static volatile int POOL_TIME = -1
            , ERROR_DELAY = -1;

        public virtual void SetDelegateWait(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }

        public abstract void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr);

        public virtual int MayToClose()
        {
            return 0;
        }

        public abstract void UpdateGraphicsCurrent (int type);
    }

    public abstract class PanelContainerStatistic : PanelStatistic
    {
        private Type _controlType;

        public PanelContainerStatistic(MODE_UPDATE_VALUES modeUpdateValues, Color foreColor, Color backColor, Type type)
            : base(modeUpdateValues, foreColor, backColor)
        {
            registredControlType(type);
        }

        private void registredControlType(Type type)
        {
            _controlType = type;
        }
        /// <summary>
        /// Старт опроса
        /// </summary>
        public override void Start()
        {
            base.Start();

            foreach (Control ctrl in this.Controls)
                if (ctrl.GetType().Equals(_controlType) == true)
                    ((PanelStatistic)ctrl).Start();
                else
                    ;
        }
        /// <summary>
        /// Остановка опроса
        /// </summary>
        public override void Stop()
        {
            foreach (Control ctrl in this.Controls)
                if (ctrl.GetType().Equals(_controlType) == true)
                    ((PanelStatistic)ctrl).Stop();
                else
                    ;

            base.Stop();
        }
        /// <summary>
        /// Активация панели
        /// </summary>
        /// <param name="active">Установка состояния панели</param>
        /// <returns>Возвращает состояние после выполнения операции</returns>
        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (bRes == false)
                return bRes;
            else
                ;

            int i = 0;
            foreach (Control ctrl in this.Controls)
                if (ctrl.GetType().Equals(_controlType) == true)
                    ((PanelStatistic)ctrl).Activate(active);
                else
                    ;

            return bRes;
        }
        /// <summary>
        /// Инициализация характеристик, стилей макета для размещения дочерних элементов интерфейса
        ///  (должна быть вызвана явно)
        /// </summary>
        /// <param name="col">Количество столбцов в макете</param>
        /// <param name="row">Количество строк в макете</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            this.ColumnCount = cols;
            if (this.ColumnCount == 0) this.ColumnCount++; else ;
            this.RowCount = rows / this.ColumnCount;

            initializeLayoutStyleEvenly();
        }
        /// <summary>
        /// Назначить делегаты по отображению сообщений в строке статуса
        /// </summary>
        /// <param name="ferr">Делегат для отображения в строке статуса ошибки</param>
        /// <param name="fwar">Делегат для отображения в строке статуса предупреждения</param>
        /// <param name="fact">Делегат для отображения в строке статуса описания действия</param>
        /// <param name="fclr">Делегат для удаления из строки статуса сообщений</param>
        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            foreach (Control ctrl in this.Controls)
                if (ctrl.GetType().Equals(_controlType) == true)
                    ((PanelStatistic)ctrl).SetDelegateReport(ferr, fwar, fact, fclr);
                else
                    ;
        }
    }

    public abstract class PanelStatisticWithTableHourRows : PanelStatistic
    {
        public PanelStatisticWithTableHourRows (MODE_UPDATE_VALUES modeUpdateValues, Color foreColor, Color backColor)
            : base (modeUpdateValues, foreColor, backColor)
        {
        }

        protected abstract void initTableHourRows();
    }
}
