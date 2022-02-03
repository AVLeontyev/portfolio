namespace StatisticTrans
{
    partial class FormMainTrans
    {
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

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            //System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTrans));
            //this.m_dgwAdminTable = new StatisticCommon.DataGridViewAdminKomDisp();
            this.buttonClose = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.comboBoxTECComponent = new System.Windows.Forms.ComboBox();
            this.labelTECComponent = new System.Windows.Forms.Label();
            this.labelDate = new System.Windows.Forms.Label();
            this.dateTimePickerMain = new System.Windows.Forms.DateTimePicker();
            this.groupBoxDest = new System.Windows.Forms.GroupBox();
            this.buttonDestClear = new System.Windows.Forms.Button();
            this.buttonDestSave = new System.Windows.Forms.Button();
            this.groupBoxSource = new System.Windows.Forms.GroupBox();
            this.buttonSourceExport = new System.Windows.Forms.Button();
            this.buttonSourceSave = new System.Windows.Forms.Button();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.конфигурацияБДToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ППБРToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ОпросППБРToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ОпросАдминЗначенияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.АдминЗначенияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.СохранППБРToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.СохранАдминЗначенияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.помощьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIconMain = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStripNotifyIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.развернутьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorNotifyIcon = new System.Windows.Forms.ToolStripSeparator();
            this.закрытьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            //((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).BeginInit();
            this.panelMain.SuspendLayout();
            this.groupBoxDest.SuspendLayout();
            //((System.ComponentModel.ISupportInitialize)(this.nudnDestPort)).BeginInit();
            this.groupBoxSource.SuspendLayout();
            
            this.menuStripMain.SuspendLayout();
            this.contextMenuStripNotifyIcon.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(733, 516);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(100, 23);
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Text = "Закрыть";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.comboBoxTECComponent);
            this.panelMain.Controls.Add(this.labelTECComponent);
            this.panelMain.Controls.Add(this.labelDate);
            //this.panelMain.Controls.Add(this.m_dgwAdminTable);
            this.panelMain.Controls.Add(this.dateTimePickerMain);
            this.panelMain.Controls.Add(this.groupBoxDest);
            this.panelMain.Controls.Add(this.groupBoxSource);
            this.panelMain.Location = new System.Drawing.Point(12, 28);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(822, 484);
            this.panelMain.TabIndex = 3;
            // 
            // comboBoxTECComponent
            // 
            this.comboBoxTECComponent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTECComponent.FormattingEnabled = true;
            this.comboBoxTECComponent.Location = new System.Drawing.Point(132, 37);
            this.comboBoxTECComponent.Name = "comboBoxTECComponent";
            this.comboBoxTECComponent.Size = new System.Drawing.Size(171, 21);
            this.comboBoxTECComponent.TabIndex = 33;
            // 
            // labelTECComponent
            // 
            this.labelTECComponent.AutoSize = true;
            this.labelTECComponent.Location = new System.Drawing.Point(11, 39);
            this.labelTECComponent.Name = "labelTECComponent";
            this.labelTECComponent.Size = new System.Drawing.Size(63, 13);
            this.labelTECComponent.TabIndex = 31;
            this.labelTECComponent.Text = "Компонент";
            // 
            // labelDate
            // 
            this.labelDate.AutoSize = true;
            this.labelDate.Location = new System.Drawing.Point(11, 12);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(33, 13);
            this.labelDate.TabIndex = 29;
            this.labelDate.Text = "Дата";
            // 
            // dateTimePickerMain
            // 
            this.dateTimePickerMain.Location = new System.Drawing.Point(132, 7);
            this.dateTimePickerMain.Name = "dateTimePickerMain";
            this.dateTimePickerMain.Size = new System.Drawing.Size(170, 20);
            this.dateTimePickerMain.TabIndex = 28;
            // 
            // groupBoxDest
            // 
            this.groupBoxDest.BackColor = System.Drawing.SystemColors.Control;
            //...Controls...
            this.groupBoxDest.Controls.Add(this.buttonDestClear);
            this.groupBoxDest.Controls.Add(this.buttonDestSave);
            //...Controls...
            this.groupBoxDest.Location = new System.Drawing.Point(3, 276);
            this.groupBoxDest.Name = "groupBoxDest";
            this.groupBoxDest.Size = new System.Drawing.Size(300, 200);
            this.groupBoxDest.TabIndex = 26;
            this.groupBoxDest.TabStop = false;
            this.groupBoxDest.Text = "Получатель";
            this.groupBoxDest.MouseClick += new System.Windows.Forms.MouseEventHandler(this.groupBox_MouseClick);
            this.groupBoxDest.Enter += new System.EventHandler(this.groupBox_Enter);
            // 
            // buttonDestClear
            // 
            this.buttonDestClear.Location = new System.Drawing.Point(10, 166);
            this.buttonDestClear.Name = "buttonDestClear";
            this.buttonDestClear.Size = new System.Drawing.Size(137, 23);
            this.buttonDestClear.TabIndex = 35;
            this.buttonDestClear.Text = "Удалить";
            this.buttonDestClear.UseVisualStyleBackColor = true;
            this.buttonDestClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonDestSave
            // 
            this.buttonDestSave.Location = new System.Drawing.Point(153, 166);
            this.buttonDestSave.Name = "buttonDestSave";
            this.buttonDestSave.Size = new System.Drawing.Size(137, 23);
            this.buttonDestSave.TabIndex = 35;
            this.buttonDestSave.Text = "Сохранить";
            this.buttonDestSave.UseVisualStyleBackColor = true;
            this.buttonDestSave.Click += new System.EventHandler(this.buttonDestSave_Click);
            
            //Source...
            // 
            // groupBoxSource
            // 
            this.groupBoxSource.BackColor = System.Drawing.SystemColors.Info;
            this.groupBoxSource.Controls.Add(this.buttonSourceExport);
            this.groupBoxSource.Controls.Add(this.buttonSourceSave);
            //Source...
            this.groupBoxSource.Location = new System.Drawing.Point(3, 65);
            this.groupBoxSource.Name = "groupBoxSource";
            this.groupBoxSource.Size = new System.Drawing.Size(300, 200);
            this.groupBoxSource.TabIndex = 25;
            this.groupBoxSource.TabStop = false;
            this.groupBoxSource.Text = "Источник";
            this.groupBoxSource.MouseClick += new System.Windows.Forms.MouseEventHandler(this.groupBox_MouseClick);
            this.groupBoxSource.Enter += new System.EventHandler(this.groupBox_Enter);
            // 
            // buttonSourceExport
            // 
            this.buttonSourceExport.Location = new System.Drawing.Point(8, 163);
            this.buttonSourceExport.Name = "buttonSourceExport";
            this.buttonSourceExport.Size = new System.Drawing.Size(137, 23);
            this.buttonSourceExport.TabIndex = 37;
            this.buttonSourceExport.Text = "Экспорт данных";
            this.buttonSourceExport.UseVisualStyleBackColor = true;
            this.buttonSourceExport.Click += new System.EventHandler(this.buttonSourceExport_Click);
            // 
            // buttonSourceSave
            // 
            this.buttonSourceSave.Location = new System.Drawing.Point(151, 163);
            this.buttonSourceSave.Name = "buttonSourceSave";
            this.buttonSourceSave.Size = new System.Drawing.Size(137, 23);
            this.buttonSourceSave.TabIndex = 36;
            this.buttonSourceSave.Text = "Сохранить";
            this.buttonSourceSave.UseVisualStyleBackColor = true;
            this.buttonSourceSave.Click += new System.EventHandler(this.buttonSourceSave_Click);

            // 
            // menuStripMain
            // 
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.настройкиToolStripMenuItem,
            this.помощьToolStripMenuItem});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Size = new System.Drawing.Size(841, 24);
            this.menuStripMain.TabIndex = 4;
            this.menuStripMain.Text = "menuStripMain";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.выходToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // настройкиToolStripMenuItem
            // 
            this.настройкиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.конфигурацияБДToolStripMenuItem
                , this.ППБРToolStripMenuItem
                , this.АдминЗначенияToolStripMenuItem
            });
            this.настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
            this.настройкиToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.настройкиToolStripMenuItem.Text = "Настройки";
            // 
            // конфигурацияБДToolStripMenuItem
            // 
            this.конфигурацияБДToolStripMenuItem.Name = "конфигурацияБДToolStripMenuItem";
            this.конфигурацияБДToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.конфигурацияБДToolStripMenuItem.Text = "БД конфигурации";
            this.конфигурацияБДToolStripMenuItem.Click += new System.EventHandler(this.конфигурацияБДToolStripMenuItem_Click);
            // 
            // ППБРToolStripMenuItem
            // 
            this.ППБРToolStripMenuItem.Name = "ППБРToolStripMenuItem";
            this.ППБРToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.ППБРToolStripMenuItem.Text = "ППБР";
            this.ППБРToolStripMenuItem.Enabled = true;
            // 
            // АдминЗначенияToolStripMenuItem
            // 
            this.АдминЗначенияToolStripMenuItem.Name = "Админ./знач.ToolStripMenuItem";
            this.АдминЗначенияToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.АдминЗначенияToolStripMenuItem.Text = "Админ./знач.";
            this.АдминЗначенияToolStripMenuItem.Enabled = true;
            // 
            // ОпросППБРToolStripMenuItem
            // 
            this.ОпросППБРToolStripMenuItem.Name = "ОпросППБРToolStripMenuItem";
            this.ОпросППБРToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.ОпросППБРToolStripMenuItem.Text = "Опрос ППБР";
            this.ОпросППБРToolStripMenuItem.Enabled = false;
            // 
            // СохранАдминЗначенияToolStripMenuItem
            // 
            this.СохранАдминЗначенияToolStripMenuItem.Name = "СохранАдминЗначенияToolStripMenuItem";
            this.СохранАдминЗначенияToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.СохранАдминЗначенияToolStripMenuItem.Text = "Сохранение админ./знач.";
            this.СохранАдминЗначенияToolStripMenuItem.Enabled = false;
            // 
            // СохранППБРToolStripMenuItem
            // 
            this.СохранППБРToolStripMenuItem.Name = "СохранППБРToolStripMenuItem";
            this.СохранППБРToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.СохранППБРToolStripMenuItem.Text = "Сохранение ППБР";
            this.СохранППБРToolStripMenuItem.Enabled = false;
            // 
            // ОпросАдминЗначенияToolStripMenuItem
            // 
            this.ОпросАдминЗначенияToolStripMenuItem.Name = "ОпросАдминЗначенияToolStripMenuItem";
            this.ОпросАдминЗначенияToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.ОпросАдминЗначенияToolStripMenuItem.Text = "Опрос админ./знач.";
            this.ОпросАдминЗначенияToolStripMenuItem.Enabled = false;

            this.АдминЗначенияToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { this.ОпросАдминЗначенияToolStripMenuItem, this.СохранАдминЗначенияToolStripMenuItem });
            this.ППБРToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { this.ОпросППБРToolStripMenuItem, this.СохранППБРToolStripMenuItem });

            // 
            // помощьToolStripMenuItem
            // 
            this.помощьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.оПрограммеToolStripMenuItem});
            this.помощьToolStripMenuItem.Name = "помощьToolStripMenuItem";
            this.помощьToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.помощьToolStripMenuItem.Text = "Помощь";
            // 
            // оПрограммеToolStripMenuItem
            // 
            this.оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.оПрограммеToolStripMenuItem.Text = "О программе";
            this.оПрограммеToolStripMenuItem.Click += new System.EventHandler(this.оПрограммеToolStripMenuItem_Click);
            // 
            // notifyIconMain
            //
            //this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            //this.notifyIconMain.Text = "Статистика: конвертер";
            // 
            // contextMenuStripNotifyIcon
            // 
            this.contextMenuStripNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.развернутьToolStripMenuItem,
            this.toolStripSeparatorNotifyIcon,
            this.закрытьToolStripMenuItem});
            this.contextMenuStripNotifyIcon.Name = "contextMenuStripNotifyIcon";
            this.contextMenuStripNotifyIcon.Size = new System.Drawing.Size(153, 76);
            // 
            // развернутьToolStripMenuItem
            // 
            this.развернутьToolStripMenuItem.Name = "развернутьToolStripMenuItem";
            this.развернутьToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.развернутьToolStripMenuItem.Text = "Развернуть";
            this.развернутьToolStripMenuItem.Click += new System.EventHandler(this.развернутьToolStripMenuItem_Click);
            // 
            // toolStripSeparatorNotifyIcon
            // 
            this.toolStripSeparatorNotifyIcon.Name = "toolStripSeparatorNotifyIcon";
            this.toolStripSeparatorNotifyIcon.Size = new System.Drawing.Size(149, 6);
            // 
            // закрытьToolStripMenuItem
            // 
            this.закрытьToolStripMenuItem.Name = "закрытьToolStripMenuItem";
            this.закрытьToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.закрытьToolStripMenuItem.Text = "Закрыть";
            this.закрытьToolStripMenuItem.Click += new System.EventHandler(this.закрытьToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(841, 568);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.menuStripMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            //this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStripMain;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            //this.Text = "Конвертер данных плана и административных данных";
            //((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).EndInit();
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.groupBoxDest.ResumeLayout(false);
            this.groupBoxDest.PerformLayout();
            //((System.ComponentModel.ISupportInitialize)(this.nudnDestPort)).EndInit();
            //Source...
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.contextMenuStripNotifyIcon.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        protected System.Windows.Forms.Button buttonClose;
        protected System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label labelTECComponent;
        private System.Windows.Forms.Label labelDate;
        protected System.Windows.Forms.DateTimePicker dateTimePickerMain;
        protected System.Windows.Forms.GroupBox groupBoxSource;
        protected System.Windows.Forms.GroupBox groupBoxDest;
        //protected System.Windows.Forms.Label labelDestPort;
        //protected System.Windows.Forms.NumericUpDown nudnDestPort;
        //protected System.Windows.Forms.Label labelDestPass;
        //protected System.Windows.Forms.Label labelDestUserID;
        //protected System.Windows.Forms.Label labelDestNameDatabase;
        //protected System.Windows.Forms.Label labelDestServerIP;
        //protected System.Windows.Forms.MaskedTextBox mtbxDestPass;
        //protected System.Windows.Forms.TextBox tbxDestUserId;
        //protected System.Windows.Forms.TextBox tbxDestNameDatabase;
        //protected System.Windows.Forms.TextBox tbxDestServerIP;
        //Source...
        protected System.Windows.Forms.ComboBox comboBoxTECComponent;
        private System.Windows.Forms.Button buttonDestClear;
        private System.Windows.Forms.Button buttonDestSave;
        protected System.Windows.Forms.Button buttonSourceSave;
        protected System.Windows.Forms.Button buttonSourceExport;
        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        protected System.Windows.Forms.ToolStripMenuItem настройкиToolStripMenuItem;
        protected System.Windows.Forms.ToolStripMenuItem конфигурацияБДToolStripMenuItem;
        protected System.Windows.Forms.ToolStripMenuItem ППБРToolStripMenuItem;
        protected System.Windows.Forms.ToolStripMenuItem ОпросППБРToolStripMenuItem;
        protected System.Windows.Forms.ToolStripMenuItem ОпросАдминЗначенияToolStripMenuItem;
        protected System.Windows.Forms.ToolStripMenuItem АдминЗначенияToolStripMenuItem;
        protected System.Windows.Forms.ToolStripMenuItem СохранППБРToolStripMenuItem;
        protected System.Windows.Forms.ToolStripMenuItem СохранАдминЗначенияToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem помощьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
        protected System.Windows.Forms.NotifyIcon notifyIconMain;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem развернутьToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem закрытьToolStripMenuItem;
    }
}

