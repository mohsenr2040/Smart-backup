namespace AutoSmartBackup
{
    partial class Frm_StartBackup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Btn_Cancel = new System.Windows.Forms.Button();
            this.Btn_Start = new System.Windows.Forms.Button();
            this.Btn_Report = new System.Windows.Forms.Button();
            this.Lstb_Report = new System.Windows.Forms.ListBox();
            this.Pgb_Backup = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // Btn_Cancel
            // 
            this.Btn_Cancel.Location = new System.Drawing.Point(95, 186);
            this.Btn_Cancel.Name = "Btn_Cancel";
            this.Btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.Btn_Cancel.TabIndex = 10;
            this.Btn_Cancel.Text = "لغو";
            this.Btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // Btn_Start
            // 
            this.Btn_Start.Location = new System.Drawing.Point(14, 186);
            this.Btn_Start.Name = "Btn_Start";
            this.Btn_Start.Size = new System.Drawing.Size(75, 23);
            this.Btn_Start.TabIndex = 9;
            this.Btn_Start.Text = "شروع";
            this.Btn_Start.UseVisualStyleBackColor = true;
            this.Btn_Start.Click += new System.EventHandler(this.Btn_Start_Click);
            // 
            // Btn_Report
            // 
            this.Btn_Report.BackColor = System.Drawing.Color.White;
            this.Btn_Report.Location = new System.Drawing.Point(358, 186);
            this.Btn_Report.Name = "Btn_Report";
            this.Btn_Report.Size = new System.Drawing.Size(75, 23);
            this.Btn_Report.TabIndex = 8;
            this.Btn_Report.Text = "گزارش";
            this.Btn_Report.UseVisualStyleBackColor = false;
            // 
            // Lstb_Report
            // 
            this.Lstb_Report.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Lstb_Report.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lstb_Report.ForeColor = System.Drawing.Color.Teal;
            this.Lstb_Report.FormattingEnabled = true;
            this.Lstb_Report.Location = new System.Drawing.Point(14, 59);
            this.Lstb_Report.Name = "Lstb_Report";
            this.Lstb_Report.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.Lstb_Report.Size = new System.Drawing.Size(419, 121);
            this.Lstb_Report.TabIndex = 7;
            // 
            // Pgb_Backup
            // 
            this.Pgb_Backup.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.Pgb_Backup.Location = new System.Drawing.Point(14, 21);
            this.Pgb_Backup.Name = "Pgb_Backup";
            this.Pgb_Backup.Size = new System.Drawing.Size(419, 23);
            this.Pgb_Backup.Step = 1;
            this.Pgb_Backup.TabIndex = 6;
            // 
            // Frm_StartBackup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Menu;
            this.ClientSize = new System.Drawing.Size(447, 231);
            this.Controls.Add(this.Btn_Cancel);
            this.Controls.Add(this.Btn_Start);
            this.Controls.Add(this.Btn_Report);
            this.Controls.Add(this.Lstb_Report);
            this.Controls.Add(this.Pgb_Backup);
            this.Name = "Frm_StartBackup";
            this.Text = "SmartBackup";
            this.Load += new System.EventHandler(this.Frm_StartBackup_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Btn_Cancel;
        private System.Windows.Forms.Button Btn_Start;
        private System.Windows.Forms.Button Btn_Report;
        public System.Windows.Forms.ListBox Lstb_Report;
        private System.Windows.Forms.ProgressBar Pgb_Backup;
    }
}