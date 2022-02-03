using System;
using System.Drawing;
using System.Windows.Forms;



namespace CommonAux
{
    partial class FormMain
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        PanelCommonAux m_panel;

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
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));

            this.SuspendLayout();

            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 650);
            this.MinimumSize = new System.Drawing.Size(1000, 650);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("IconApp")));

            this.MaximizeBox = true;
            this.Name = "FormMainCommonAux";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Собственные нужды - АИИСКУЭ";
            this.MainMenuStrip = new MenuStrip();
            this.MainMenuStrip.Items.AddRange(
              new ToolStripMenuItem[] {
                    new ToolStripMenuItem (@"Файл")
                    , new ToolStripMenuItem (@"Настройка")
                    , new ToolStripMenuItem (@"О программе")
                }
            );
            (this.MainMenuStrip.Items[0] as ToolStripMenuItem).DropDownItems.Add(new ToolStripMenuItem(@"Выход"));
            (this.MainMenuStrip.Items[0] as ToolStripMenuItem).DropDownItems[0].Click += new EventHandler(fMenuItemExit_Click);
            (this.MainMenuStrip.Items[1] as ToolStripMenuItem).DropDownItems.Add(new ToolStripMenuItem(@"БД конфигурации"));
            (this.MainMenuStrip.Items[1] as ToolStripMenuItem).DropDownItems[0].Click += new EventHandler(fMenuItemDBConfig_Click);
            (this.MainMenuStrip.Items[2] as ToolStripMenuItem).Click += new EventHandler(fMenuItemAbout_Click);

            this.Controls.Add(MainMenuStrip);

            #region Добавить панель (родительскую для рабочей панели) на форму
            _panelMain = new Panel ();
            _panelMain.Location = new Point (0, this.MainMenuStrip.Height);
            _panelMain.Size = new System.Drawing.Size (this.ClientSize.Width, this.ClientSize.Height - this.MainMenuStrip.Height - this.m_statusStripMain.Height);
            _panelMain.Anchor = (AnchorStyles)(((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right) | AnchorStyles.Bottom);
            //_panelMain.Controls.Add (this.m_panel); //!!! добавить в OnForm_Load
            this.Controls.Add (_panelMain);
            #endregion

            this.ResumeLayout(false);
            this.PerformLayout();

            this.Load += new System.EventHandler(FormMain_Load);
            this.Activated += new System.EventHandler(FormMain_Activate);
            this.Deactivate += new System.EventHandler(FormMain_Deactivate);
        }

        #endregion
    }
}

