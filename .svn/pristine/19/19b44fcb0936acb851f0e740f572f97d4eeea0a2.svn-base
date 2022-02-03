using System;
using System.Drawing;
using System.Windows.Forms;
/// <summary>
/// Пространство имен Statistic 
/// </summary>
namespace Statistic {
    /// <summary>
    /// Частичный класс FormGraphicsSettings (настройка графиков) 
    /// </summary>
    partial class FormGraphicsSettings {

        /// <summary>
        /// Структура LABEL_COLOR
        /// </summary>
        struct LABEL_COLOR {

            /// <summary>
            ///  Поля структуры
            /// </summary>
            public Color color;
            public string name, text;
            public System.Drawing.Point pos;

            /// <summary>
            ///  Параметризованный конструктор LABEL_COLOR инициализирует поля структуры
            /// </summary>
            /// <param name="col">Цвет</param>
            /// <param name="name">Имя</param>
            /// <param name="text">Надпись</param>
            /// <param name="pt">Позиция</param>
            public LABEL_COLOR (Color col, string name, string text, System.Drawing.Point pt)
            {
                this.color = col;
                this.name = name;
                this.text = text;
                this.pos = pt;
            }
        }

        /// <summary>                                                        //Автоматически генерируемый код при создании файла Designer.cs. 
        /// Required designer variable.                                      //components отслеживает все элементы управления, помещенные в форму.   
        /// </summary>                                                       //Код в файле конструктора гарантирует, что эти компоненты будут удалены, когда форма будет удалена. 
        private System.ComponentModel.IContainer components = null;          //Если вы не добавили такие компоненты в форму во время разработки, компоненты будут нулевыми.         

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose ();
            }
            base.Dispose (disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// 
        /// Метод InitializeComponent() инициализирует все компоненты,расположенные на форме.
        /// InitializeComponent() вызывает метод LoadComponent().
        /// LoadComponent() извлекает скомпилированный XAML из сборки и использует его для построения пользовательского интерфейса.
        /// </summary>

        private void InitializeComponent ()
        {
            int heightElementColor = 25 //Высота подписей "Цвет" по оси ординат
                , indx = -1
                , yPos = -1 //Позиция по оси ординат
                , heightElementGui = 22 //Высота элементов управления по оси ординат
                , yMargin = 3;

            // Создание массива лейблов  (УДГэ,Отклонение и т.д.)
            this.m_arlblColorValues = new System.Windows.Forms.Label [(int)INDEX_COLOR_VAUES.COUNT_INDEX_COLOR];
            this.gbxColorShema = new System.Windows.Forms.GroupBox ();
            //this.m_arRbtnColorShema = new System.Windows.Forms.RadioButton [] {
            //    new System.Windows.Forms.RadioButton()
            //    , new System.Windows.Forms.RadioButton()
            //};
            this.m_cbUseSystemColors = new System.Windows.Forms.CheckBox ();
            this.m_arlblColorShema = new System.Windows.Forms.Label [Enum.GetValues(typeof(INDEX_COLOR_SHEMA)).Length];
            // Создание GroupBox (ящик: тип графиков)
            this.gbxTypeGraph = new System.Windows.Forms.GroupBox ();
            // Создание CheckBox (флажка)
            this.m_cbxPBRHourToHour = new System.Windows.Forms.CheckBox ();
            // Создание CheckBox (флажка)
            this.cbxScale = new System.Windows.Forms.CheckBox ();
            // Создание RadioButton (переключатель:гистограмма, линейный)
            this.m_arRbtnTypeGraph = new System.Windows.Forms.RadioButton [] {
                new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
            };
            // Создание GroupBox (ящик: типы значений графиков)
            this.gbxSourceData = new System.Windows.Forms.GroupBox ();
            // Создание массива переключателей "Типы значений графиков"
            this.m_arRbtnSourceData = new System.Windows.Forms.RadioButton [] {
                new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
            };
            // Метод SuspendLayout() приостанавливает компоновку в ящиках "цветовая схема", "тип графиков" и "типы значений графиков"
            this.gbxColorShema.SuspendLayout ();
            this.gbxTypeGraph.SuspendLayout ();
            this.gbxSourceData.SuspendLayout ();
            // SuspendLayout останавливает работу менеджера выравнивания (layout logic)
            this.SuspendLayout ();

            #region Подписи для выбора цвета элементов
            // Массив подписей, состоящий из 10 элементов (УДГэ,Отклонение и т.д.)
            LABEL_COLOR [] arLabelColor = new LABEL_COLOR [(int)INDEX_COLOR_VAUES.COUNT_INDEX_COLOR]
            {
                // LABEL1: Цвет черный,имя "lblUDGcolor",надпись "УДГэ, УДГт",координаты положения (12, 11)
                  new LABEL_COLOR (Color.FromArgb(0, 0, 0), "lblUDGcolor", "УДГэ, УДГт", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.UDG * heightElementColor) + 11))
                , new LABEL_COLOR (Color.FromArgb(0, 0, 255), "lblPBRHourToHour", "ПБР от часа к часу(мин.)", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.PBR_HOUR_TO_HOUR * heightElementColor) + 11))
                , new LABEL_COLOR (Color.FromArgb(255, 0, 0), "lblDIVcolor", "Отклонение", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.DIVIATION * heightElementColor) + 11))
                , new LABEL_COLOR (Color.FromArgb(0, 128, 0), "lblP_ASKUEcolor", "Мощность (АИИСКУЭЭ)", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.ASKUE * heightElementColor) + 11))
                , new LABEL_COLOR (Color.FromArgb(255, 255, 255), "lblP_ASKUE_normHourscolor", "Мощность (АИИСКУЭЭ, обычн.ч.)", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.ASKUE_LK_REGULAR * heightElementColor) + 11))
                , new LABEL_COLOR (Color.FromArgb(0, 128, 192), "lblP_SOTIASSOcolor", "Мощность (СОТИАССО)", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.SOTIASSO * heightElementColor) + 11))
                , new LABEL_COLOR (Color.FromArgb(255, 255, 0), "lblRECcolor", "Рекомендация", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.REC * heightElementColor) + 11))
                , new LABEL_COLOR (Color.FromArgb(128, 000, 128), "lblT_ASKUTEcolor", "Температура (АИИСКУТЭ)", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.TEMP_ASKUTE * heightElementColor) + 11))
                , new LABEL_COLOR (Color.FromArgb(231, 231, 238 /*230, 230, 230*/), "lblBG_ASKUE_color", "Фон (АИИСКУЭЭ)", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.BG_ASKUE * heightElementColor) + 11))
                , new LABEL_COLOR (Color.FromArgb(231, 238, 231), "lblBG_SOTIASSO_color", "Фон (СОТИАССО)", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.BG_SOTIASSO * heightElementColor) + 11))
                , new LABEL_COLOR (Color.FromArgb(231, 231, 238), "lblBG_ASKUTE_color", "Фон (АИИСКУТЭ)", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.BG_ASKUTE * heightElementColor) + 11))
                , new LABEL_COLOR (Color.FromArgb(200, 200, 200), "lblGRIDcolor", "Сетка", new System.Drawing.Point(12, ((int)INDEX_COLOR_VAUES.GRID * heightElementColor) + 11))
            };

            // Для каждого параметра (УДГ,Отклонение и т.д.)
            for (int i = 0; i < (int)INDEX_COLOR_VAUES.COUNT_INDEX_COLOR; i++) {
                // Cоздать подпись 
                this.m_arlblColorValues [i] = new System.Windows.Forms.Label ();

                this.m_arlblColorValues [i].Tag = (INDEX_COLOR_VAUES)i;
                // Цвет заднего плана (подписи)
                this.m_arlblColorValues [i].BackColor = arLabelColor [i].color;
                // Цвет переднего плана (подписи)
                this.m_arlblColorValues [i].ForeColor = getForeColor (arLabelColor [i].color);
                // Стиль рамки 
                this.m_arlblColorValues [i].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                // Положение
                this.m_arlblColorValues [i].Location = arLabelColor [i].pos;
                // Имя
                this.m_arlblColorValues [i].Name = arLabelColor [i].name;
                // Размер
                this.m_arlblColorValues [i].Size = new System.Drawing.Size (195, 26);
                // Текс надписи
                this.m_arlblColorValues [i].Text = arLabelColor [i].text;
                // Выравнивание текста
                this.m_arlblColorValues [i].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                // При CLick на лейбл обработчику событий передается событие нажатия 
                this.m_arlblColorValues [i].Click += new System.EventHandler (this.lbl_color_Click);
            }
            #endregion

            #region Цветовая схема            
            // Координаты расположения ящика
            this.gbxColorShema.Location = new System.Drawing.Point (222, 6);
            // Имя ящика
            this.gbxColorShema.Name = "gbxColorShema";
            // Размер 
            this.gbxColorShema.Size = new System.Drawing.Size (173, 60);
            // Последовательность перехода между ссылками при нажатии на кнопку Tab
            this.gbxColorShema.TabIndex = 5;
            // gbxType не получает фокус с помощью клавиши TAB. 
            this.gbxColorShema.TabStop = false;
            // Текст надписи
            this.gbxColorShema.Text = "Цветовая схема";
            // 
            // cbUseSystemColors Элемент "Система"
            // 
            indx = (int)ColorShemas.System;
            this.m_cbUseSystemColors.AutoSize = true;
            this.m_cbUseSystemColors.Tag = (ColorShemas)indx;
            // Включена проверка нажатия
            this.m_cbUseSystemColors.Checked = true;
            this.m_cbUseSystemColors.Location = new System.Drawing.Point (6, yPos = 16);
            this.m_cbUseSystemColors.Name = "cbUseSystemColors";
            this.m_cbUseSystemColors.Size = new System.Drawing.Size (92, 17);
            this.m_cbUseSystemColors.TabIndex = 1;
            this.m_cbUseSystemColors.TabStop = true;
            this.m_cbUseSystemColors.Text = "Система";
            this.m_cbUseSystemColors.UseVisualStyleBackColor = true;
            this.m_cbUseSystemColors.CheckedChanged += new System.EventHandler (this.cbUseSystemColors_CheckedChanged);
            this.m_cbUseSystemColors.Enabled = _allowedChangeShema;
            // 
            // labelColorShema Элемент "Фон"
            // 
            int wLabelColorShema = ((gbxColorShema.ClientSize.Width - (2 * 6)) / 2) - 1; // "1" - расстояние между подписями по горизонтали
            indx = (int)INDEX_COLOR_SHEMA.BACKGROUND;
            this.m_arlblColorShema [indx] = new System.Windows.Forms.Label ();
            this.m_arlblColorShema[indx].Tag = (INDEX_COLOR_SHEMA)indx;
            //this.m_arlblColorShema [indx].AutoSize = true;
            this.m_arlblColorShema [indx].Location = new System.Drawing.Point (6, yPos += (heightElementGui - 2));
            this.m_arlblColorShema [indx].Name = "labelColorShema";
            this.m_arlblColorShema [indx].Size = new System.Drawing.Size (wLabelColorShema, 17 + 2);
            this.m_arlblColorShema [indx].TabIndex = 0;
            this.m_arlblColorShema [indx].Text = "Фон";
            this.m_arlblColorShema [indx].TextAlign = ContentAlignment.MiddleLeft;
            this.m_arlblColorShema [indx].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_arlblColorShema [indx].BackColor = CustomColorTable.BackColor;
            this.m_arlblColorShema [indx].ForeColor = CustomColorTable.ForeColor;
            //this.m_arlblColorShema [indx].Enabled = false;
            // Обработка события "двойной щелчок" - изменение цвета "темной" схемы
            this.m_arlblColorShema [indx].Click += new System.EventHandler (lbl_color_Click);
            this.m_arlblColorShema [indx].BackColorChanged += new System.EventHandler (labelColorShema_ValueChanged);
            // 
            // labelColorFont Элемент "Шрифт"
            // 
            indx = (int)INDEX_COLOR_SHEMA.FONT;
            this.m_arlblColorShema [indx] = new System.Windows.Forms.Label ();
            this.m_arlblColorShema [indx].Tag = (INDEX_COLOR_SHEMA)indx;
            //this.m_arlblColorShema [indx].AutoSize = true;
            this.m_arlblColorShema [indx].Location = new System.Drawing.Point (6 + wLabelColorShema + 2 * 1, yPos); // "2 * 1" - расстояние между подписями по горизонтали
            this.m_arlblColorShema [indx].Name = "labelColorShema";
            this.m_arlblColorShema [indx].Size = new System.Drawing.Size (wLabelColorShema, 17 + 2);
            this.m_arlblColorShema [indx].TabIndex = 0;
            this.m_arlblColorShema [indx].Text = "Шрифт";
            this.m_arlblColorShema [indx].TextAlign = ContentAlignment.MiddleLeft;
            this.m_arlblColorShema [indx].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //TODO:
            this.m_arlblColorShema [indx].BackColor = CustomColorTable.BackColor;
            this.m_arlblColorShema [indx].ForeColor = CustomColorTable.ForeColor;
            //this.m_arlblColorShema [indx].Enabled = false;
            // Обработка события "двойной щелчок" - изменение цвета "темной" схемы
            this.m_arlblColorShema [indx].Click += new System.EventHandler (lbl_color_Click);
            this.m_arlblColorShema [indx].ForeColorChanged += new System.EventHandler (labelColorShema_ValueChanged);
            #endregion

            #region Масштабирование, типы графиков
            // 
            // gbxType Элемент "Тип графиков"
            // 
            // Добавление в ящик  (коллекцию) элемента управления масштабирования
            this.gbxTypeGraph.Controls.Add (this.m_cbxPBRHourToHour);
            // Добавление в ящик  (коллекцию) элемента управления масштабирования
            this.gbxTypeGraph.Controls.Add (this.cbxScale);
            // Добавление в ящик  (коллекцию) элементов "Гистограмма" и "Линейный"
            this.gbxTypeGraph.Controls.AddRange (m_arRbtnTypeGraph);
            // Координаты расположения ящика
            this.gbxTypeGraph.Location = new System.Drawing.Point (222, gbxColorShema.Location.Y + gbxColorShema.Size.Height + yMargin);
            // Имя ящика
            this.gbxTypeGraph.Name = "gbxTypeGraph";
            // Размер 
            this.gbxTypeGraph.Size = new System.Drawing.Size (173, 4 * heightElementGui + heightElementGui);
            // Последовательность перехода между ссылками при нажатии на кнопку Tab
            this.gbxTypeGraph.TabIndex = 5;
            // gbxType не получает фокус с помощью клавиши TAB. 
            this.gbxTypeGraph.TabStop = false;
            // Текст надписи
            this.gbxTypeGraph.Text = "Тип графиков";
            // 
            // cbxScale Элемент "Масштабирование графиков"
            // 
            // Размер элемента автоматически изменятся в соответствии с размером его содержимого
            this.m_cbxPBRHourToHour.AutoSize = true;
            //Координаты левого верхнего угла элемента относительно левого верхнего угла его контейнера 
            this.m_cbxPBRHourToHour.Location = new System.Drawing.Point (6, yPos = 19);
            //Идентификатор
            this.m_cbxPBRHourToHour.Tag = TYPE_UPDATEGUI.PBR_HOUR_TO_HOUR;
            // Имя элемента
            this.m_cbxPBRHourToHour.Name = "cbxPBRHourToHour";
            // Размер элемента
            this.m_cbxPBRHourToHour.Size = new System.Drawing.Size (159, 17);
            // Последовательность перехода между ссылками при нажатии на кнопку Tab
            this.m_cbxPBRHourToHour.TabIndex = 0;
            // Подпись элемента
            this.m_cbxPBRHourToHour.Text = "ПБР от часа к часу(мин.)";
            // Отрисовка фона с помощью визуальных стилей
            this.m_cbxPBRHourToHour.UseVisualStyleBackColor = true;
            // !!! значение по умолчанию, установить перед вызовом 'InitializeComponent'
            this.m_cbxPBRHourToHour.Checked = pbrHourToHour;
            // Обработка нажатия флажка обработчиком события
            this.m_cbxPBRHourToHour.CheckedChanged += new System.EventHandler (this.cbxPBRHourToHour_CheckedChanged);
            // 
            // cbxScale Элемент "Масштабирование графиков"
            // 
            // Размер элемента автоматически изменятся в соответствии с размером его содержимого
            this.cbxScale.AutoSize = true;
            //Координаты левого верхнего угла элемента относительно левого верхнего угла его контейнера 
            this.cbxScale.Location = new System.Drawing.Point (6, yPos += heightElementGui);
            //Идентификатор
            this.cbxScale.Tag = TYPE_UPDATEGUI.SCALE;
            // Имя элемента
            this.cbxScale.Name = "cbxScale";
            // Размер элемента
            this.cbxScale.Size = new System.Drawing.Size (159, 17);
            // Последовательность перехода между ссылками при нажатии на кнопку Tab
            this.cbxScale.TabIndex = 0;
            // Подпись элемента
            this.cbxScale.Text = "Масштабировать графики";
            // Отрисовка фона с помощью визуальных стилей
            this.cbxScale.UseVisualStyleBackColor = true;
            // Обработка нажатия флажка обработчиком события
            this.cbxScale.CheckedChanged += new System.EventHandler (this.cbxScale_CheckedChanged);
            // 
            // rbtnBar Элемент "Гистограмма"
            // 
            indx = (int)GraphTypes.Bar;
            this.m_arRbtnTypeGraph [indx].AutoSize = true;
            // Включена проверка нажатия
            this.m_arRbtnTypeGraph [indx].Tag = (GraphTypes)indx;
            this.m_arRbtnTypeGraph [indx].Checked = true;
            this.m_arRbtnTypeGraph [indx].Location = new System.Drawing.Point (6, yPos += heightElementGui);
            this.m_arRbtnTypeGraph [indx].Name = "rbtnBar";
            this.m_arRbtnTypeGraph [indx].Size = new System.Drawing.Size (92, 17);
            this.m_arRbtnTypeGraph [indx].TabIndex = 1;
            this.m_arRbtnTypeGraph [indx].TabStop = true;
            this.m_arRbtnTypeGraph [indx].Text = "гистограмма";
            this.m_arRbtnTypeGraph [indx].UseVisualStyleBackColor = true;
            // 
            // rbtnLine Элемент "Линейный"
            // 
            indx = (int)GraphTypes.Linear;
            this.m_arRbtnTypeGraph [indx].Tag = (GraphTypes)indx;
            this.m_arRbtnTypeGraph [indx].AutoSize = true;
            this.m_arRbtnTypeGraph [indx].Location = new System.Drawing.Point (6, yPos += heightElementGui);
            this.m_arRbtnTypeGraph [indx].Name = "rbtnLine";
            this.m_arRbtnTypeGraph [indx].Size = new System.Drawing.Size (75, 17);
            this.m_arRbtnTypeGraph [indx].TabIndex = 0;
            this.m_arRbtnTypeGraph [indx].Text = "линейный";
            this.m_arRbtnTypeGraph [indx].UseVisualStyleBackColor = true;
            // Обработка нажатия флажка обработчиком события
            this.m_arRbtnTypeGraph [indx].CheckedChanged += new System.EventHandler (this.rbtnTypeGraph_CheckedChanged);
            #endregion

            #region Типы значений графиков
            // 
            // groupBoxSourceData Элемент "Типы значений графиков"   
            // 
            // Добавление массива кнопок в ящик (коллекцию)
            this.gbxSourceData.Controls.AddRange (m_arRbtnSourceData);
            this.gbxSourceData.Location = new System.Drawing.Point (222, gbxTypeGraph.Location.Y + gbxTypeGraph.Size.Height + yMargin);
            this.gbxSourceData.Name = "groupBoxSourceData";
            this.gbxSourceData.Size = new System.Drawing.Size (173, m_arRbtnSourceData.Length * heightElementGui + heightElementGui);
            this.gbxSourceData.TabIndex = 8;
            this.gbxSourceData.TabStop = false;
            this.gbxSourceData.Text = "Типы значений графиков";

            // 
            // rbtnSourceData_AISKUE_PLUS_SOTIASSO
            // 
            // Переменной indx присвоен 0, yPos присвоен 16;
            indx = (int)MODE_VIEW.AISKUE_PLUS_SOTIASSO;
            yPos = 16;
            // Группа элементов управления RadioButton не будет действовать как взаимоисключающая группа,
            // а свойство Checked должно быть обновлено в коде
            this.m_arRbtnSourceData [(int)indx].Tag = indx;
            this.m_arRbtnSourceData [(int)indx].AutoCheck = false;
            this.m_arRbtnSourceData [(int)indx].AutoSize = true;
            this.m_arRbtnSourceData [(int)indx].Location = new System.Drawing.Point (6, yPos);
            this.m_arRbtnSourceData [(int)indx].Name = "rbtnSourceData_ASKUE_PLUS_SOTIASSO";
            this.m_arRbtnSourceData [(int)indx].Size = new System.Drawing.Size (134, 17);
            this.m_arRbtnSourceData [(int)indx].TabIndex = 3;
            this.m_arRbtnSourceData [(int)indx].Text = "АИСКУЭ+СОТИАССО";
            this.m_arRbtnSourceData [(int)indx].UseVisualStyleBackColor = true;
            this.m_arRbtnSourceData [(int)indx].Click += new System.EventHandler (this.rbtnSourceData_Click);
            // 
            // rbtnSourceData_ASKUE
            // Переменной indx присвоена 1, yPos присвоено 16+19=35;
            indx = (int)MODE_VIEW.AISKUE_3_MIN;
            yPos += heightElementGui;
            this.m_arRbtnSourceData [(int)indx].Tag = indx;
            this.m_arRbtnSourceData [(int)indx].AutoCheck = false;
            this.m_arRbtnSourceData [(int)indx].AutoSize = true;
            this.m_arRbtnSourceData [(int)indx].Checked = true;
            this.m_arRbtnSourceData [(int)indx].Location = new System.Drawing.Point (6, yPos);
            this.m_arRbtnSourceData [(int)indx].Name = "rbtnSourceData_ASKUE";
            this.m_arRbtnSourceData [(int)indx].Size = new System.Drawing.Size (69, 17);
            this.m_arRbtnSourceData [(int)indx].TabIndex = 1;
            this.m_arRbtnSourceData [(int)indx].Text = "АИСКУЭ";
            this.m_arRbtnSourceData [(int)indx].UseVisualStyleBackColor = true;
            this.m_arRbtnSourceData [(int)indx].Click += new System.EventHandler (this.rbtnSourceData_Click);
            // 
            // rbtnSourceData_SOTIASSO_3_min
            // Переменной indx присвоена 2, yPos присвоено 35+19=54;
            indx = (int)MODE_VIEW.SOTIASSO_3_MIN;
            yPos += heightElementGui;
            this.m_arRbtnSourceData [(int)indx].Tag = indx;
            this.m_arRbtnSourceData [(int)indx].AutoCheck = false;
            this.m_arRbtnSourceData [(int)indx].AutoSize = true;
            this.m_arRbtnSourceData [(int)indx].Location = new System.Drawing.Point (6, yPos);
            this.m_arRbtnSourceData [(int)indx].Name = "rbtnSourceData_SOTIASSO_3_min";
            this.m_arRbtnSourceData [(int)indx].Size = new System.Drawing.Size (84, 17);
            this.m_arRbtnSourceData [(int)indx].TabIndex = 0;
            this.m_arRbtnSourceData [(int)indx].Text = "СОТИАССО(3 мин)";
            this.m_arRbtnSourceData [(int)indx].UseVisualStyleBackColor = true;
            this.m_arRbtnSourceData [(int)indx].Click += new System.EventHandler (this.rbtnSourceData_Click);
            // 
            // rbtnSourceData_SOTIASSO_1_min
            // Переменной indx присвоена 3, yPos присвоено 54+19=73;
            indx = (int)MODE_VIEW.SOTIASSO_1_MIN;
            yPos += heightElementGui;
            this.m_arRbtnSourceData [(int)indx].Tag = indx;
            this.m_arRbtnSourceData [(int)indx].AutoCheck = false;
            this.m_arRbtnSourceData [(int)indx].AutoSize = true;
            this.m_arRbtnSourceData [(int)indx].Location = new System.Drawing.Point (6, yPos);
            this.m_arRbtnSourceData [(int)indx].Name = "rbtnSourceData_SOTIASSO_1_min";
            this.m_arRbtnSourceData [(int)indx].Size = new System.Drawing.Size (84, 17);
            this.m_arRbtnSourceData [(int)indx].TabIndex = 0;
            this.m_arRbtnSourceData [(int)indx].Text = "СОТИАССО(1 мин)";
            this.m_arRbtnSourceData [(int)indx].UseVisualStyleBackColor = true;
            this.m_arRbtnSourceData [(int)indx].Click += new System.EventHandler (this.rbtnSourceData_Click);
            // 
            // rbtnSourceData_COSTUMIZE
            // Переменной indx присвоена 4, yPos присвоено 73+19=92;
            indx = (int)MODE_VIEW.COSTUMIZE;
            yPos += heightElementGui;
            this.m_arRbtnSourceData [(int)indx].Tag = indx;
            this.m_arRbtnSourceData [(int)indx].AutoCheck = false;
            this.m_arRbtnSourceData [(int)indx].AutoSize = true;
            this.m_arRbtnSourceData [(int)indx].Location = new System.Drawing.Point (6, yPos);
            this.m_arRbtnSourceData [(int)indx].Name = "rbtnSourceData_COSTUMIZE";
            this.m_arRbtnSourceData [(int)indx].Size = new System.Drawing.Size (80, 17);
            this.m_arRbtnSourceData [(int)indx].TabIndex = 2;
            this.m_arRbtnSourceData [(int)indx].TabStop = true;
            this.m_arRbtnSourceData [(int)indx].Text = "выборочно";
            this.m_arRbtnSourceData [(int)indx].UseVisualStyleBackColor = true;
            this.m_arRbtnSourceData [(int)indx].Click += new System.EventHandler (this.rbtnSourceData_Click);
            #endregion

            // 
            // FormGraphicsSettings
            // 
            // Проектирование в  96 DPI (мера разрешения изображения)
            this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
            // Автомасштабирование шрифта
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            // Размер окна 407 * 325
            this.ClientSize = new System.Drawing.Size (407, 325);
            // Добавление элементов управления в окно
            for (int i = 0; i < (int)INDEX_COLOR_VAUES.COUNT_INDEX_COLOR; i++)
                this.Controls.Add (this.m_arlblColorValues [i]);
            // Добавление в ящик  (коллекцию) элементов "Фон" и "Шрифт"
            this.gbxColorShema.Controls.AddRange (
                new System.Windows.Forms.Control [] { m_cbUseSystemColors, m_arlblColorShema [(int)INDEX_COLOR_SHEMA.BACKGROUND], m_arlblColorShema [(int)INDEX_COLOR_SHEMA.FONT] }
            );
            this.Controls.Add (this.gbxColorShema);
            this.Controls.Add (this.gbxTypeGraph);
            this.Controls.Add (this.gbxSourceData);
            //this.Controls.Add(this.cbxScale);
            // Стиль рамки 
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            // Окно не разворачивается и не сворачивается
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            // Минимальный размер, в который может быть изменен размер формы
            //this.MinimumSize = new System.Drawing.Size(170, 25);
            this.Name = "FormGraphicsSettings";
            // Не отображать значок в строке заголовка формы
            this.ShowIcon = false;
            // Форма не  отображается в панели задач Windows во время выполнения
            this.ShowInTaskbar = false;
            // Центрировать форму при запуске
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройки графического оформления";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler (this.GraphicsSettings_FormClosing);
            // Метод ResumeLayout возобновляет работу менеджера выравнивания 
            this.gbxColorShema.ResumeLayout (false);
            this.gbxColorShema.PerformLayout ();
            this.gbxTypeGraph.ResumeLayout (false);
            this.gbxTypeGraph.PerformLayout ();
            this.gbxSourceData.ResumeLayout (false);
            this.gbxSourceData.PerformLayout ();
            this.ResumeLayout (false);
            //Выполнить компоновку
            this.PerformLayout ();

        }

        #endregion

        private System.Windows.Forms.Label [] m_arlblColorValues;
        private System.Windows.Forms.GroupBox gbxColorShema;
        //private System.Windows.Forms.RadioButton [] m_arRbtnColorShema;
        private System.Windows.Forms.CheckBox m_cbUseSystemColors;
        private System.Windows.Forms.Label [] m_arlblColorShema;
        private System.Windows.Forms.GroupBox gbxTypeGraph;
        private System.Windows.Forms.CheckBox cbxScale;
        private System.Windows.Forms.CheckBox m_cbxPBRHourToHour;
        private System.Windows.Forms.RadioButton [] m_arRbtnTypeGraph;
        private System.Windows.Forms.GroupBox gbxSourceData;
        private System.Windows.Forms.RadioButton [] m_arRbtnSourceData;
    }
}