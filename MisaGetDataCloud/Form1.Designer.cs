namespace MisaGetDataCloud
{
    partial class MisaGetData
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MisaGetData));
            this.label1 = new System.Windows.Forms.Label();
            this.gbName = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.txtFileDayLai = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.cbAutoRun = new System.Windows.Forms.CheckBox();
            this.cbbDoiTuongXL = new System.Windows.Forms.ComboBox();
            this.cbStartUp = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.txtLinkSaveFile = new System.Windows.Forms.TextBox();
            this.txtToken = new System.Windows.Forms.TextBox();
            this.txtMayChu = new System.Windows.Forms.TextBox();
            this.txtMST = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.gbName.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mã số thuế :";
            // 
            // gbName
            // 
            this.gbName.Controls.Add(this.button2);
            this.gbName.Controls.Add(this.button1);
            this.gbName.Controls.Add(this.txtFileDayLai);
            this.gbName.Controls.Add(this.label6);
            this.gbName.Controls.Add(this.btnStop);
            this.gbName.Controls.Add(this.cbAutoRun);
            this.gbName.Controls.Add(this.cbbDoiTuongXL);
            this.gbName.Controls.Add(this.cbStartUp);
            this.gbName.Controls.Add(this.label5);
            this.gbName.Controls.Add(this.button3);
            this.gbName.Controls.Add(this.btnRun);
            this.gbName.Controls.Add(this.btnClose);
            this.gbName.Controls.Add(this.txtLinkSaveFile);
            this.gbName.Controls.Add(this.txtToken);
            this.gbName.Controls.Add(this.txtMayChu);
            this.gbName.Controls.Add(this.txtMST);
            this.gbName.Controls.Add(this.label4);
            this.gbName.Controls.Add(this.label3);
            this.gbName.Controls.Add(this.label2);
            this.gbName.Controls.Add(this.label1);
            this.gbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbName.Location = new System.Drawing.Point(7, 8);
            this.gbName.Name = "gbName";
            this.gbName.Size = new System.Drawing.Size(580, 313);
            this.gbName.TabIndex = 1;
            this.gbName.TabStop = false;
            this.gbName.Text = "Thông tin kết nối";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(288, 250);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(78, 30);
            this.button2.TabIndex = 23;
            this.button2.Text = "Đẩy lại";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnRetryPush_Clicked);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(534, 205);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(36, 26);
            this.button1.TabIndex = 22;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnChoseFileRetryPush_Clicked);
            // 
            // txtFileDayLai
            // 
            this.txtFileDayLai.Location = new System.Drawing.Point(134, 205);
            this.txtFileDayLai.Name = "txtFileDayLai";
            this.txtFileDayLai.Size = new System.Drawing.Size(394, 26);
            this.txtFileDayLai.TabIndex = 21;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(40, 211);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 20);
            this.label6.TabIndex = 20;
            this.label6.Text = "Đẩy lại file :";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(438, 250);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(61, 30);
            this.btnStop.TabIndex = 19;
            this.btnStop.Text = "Dừng";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Clicked);
            // 
            // cbAutoRun
            // 
            this.cbAutoRun.AutoSize = true;
            this.cbAutoRun.Checked = true;
            this.cbAutoRun.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAutoRun.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.cbAutoRun.Location = new System.Drawing.Point(12, 257);
            this.cbAutoRun.Name = "cbAutoRun";
            this.cbAutoRun.Size = new System.Drawing.Size(114, 21);
            this.cbAutoRun.TabIndex = 17;
            this.cbAutoRun.Text = "Tự động chạy";
            this.cbAutoRun.UseVisualStyleBackColor = true;
            // 
            // cbbDoiTuongXL
            // 
            this.cbbDoiTuongXL.FormattingEnabled = true;
            this.cbbDoiTuongXL.Location = new System.Drawing.Point(134, 135);
            this.cbbDoiTuongXL.Name = "cbbDoiTuongXL";
            this.cbbDoiTuongXL.Size = new System.Drawing.Size(436, 28);
            this.cbbDoiTuongXL.TabIndex = 16;
            this.cbbDoiTuongXL.SelectedIndexChanged += new System.EventHandler(this.cbbDoiTuong_SelectChanged);
            // 
            // cbStartUp
            // 
            this.cbStartUp.AutoSize = true;
            this.cbStartUp.Checked = true;
            this.cbStartUp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStartUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.cbStartUp.Location = new System.Drawing.Point(12, 286);
            this.cbStartUp.Name = "cbStartUp";
            this.cbStartUp.Size = new System.Drawing.Size(182, 21);
            this.cbStartUp.TabIndex = 15;
            this.cbStartUp.Text = "Khởi động cùng windows";
            this.cbStartUp.UseVisualStyleBackColor = true;
            this.cbStartUp.CheckedChanged += new System.EventHandler(this.StartupWithWindows);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 138);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(120, 20);
            this.label5.TabIndex = 13;
            this.label5.Text = "Đối tượng xử lý :";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(534, 173);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(36, 26);
            this.button3.TabIndex = 10;
            this.button3.Text = "...";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.btnChooseFile_Clicked);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(372, 250);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(60, 30);
            this.btnRun.TabIndex = 9;
            this.btnRun.Text = "Chạy";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Clicked);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(505, 250);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(64, 30);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "Đóng";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Clicked);
            // 
            // txtLinkSaveFile
            // 
            this.txtLinkSaveFile.Location = new System.Drawing.Point(134, 173);
            this.txtLinkSaveFile.Name = "txtLinkSaveFile";
            this.txtLinkSaveFile.Size = new System.Drawing.Size(394, 26);
            this.txtLinkSaveFile.TabIndex = 7;
            // 
            // txtToken
            // 
            this.txtToken.Location = new System.Drawing.Point(134, 99);
            this.txtToken.Name = "txtToken";
            this.txtToken.Size = new System.Drawing.Size(436, 26);
            this.txtToken.TabIndex = 6;
            // 
            // txtMayChu
            // 
            this.txtMayChu.Location = new System.Drawing.Point(134, 62);
            this.txtMayChu.Name = "txtMayChu";
            this.txtMayChu.Size = new System.Drawing.Size(436, 26);
            this.txtMayChu.TabIndex = 5;
            // 
            // txtMST
            // 
            this.txtMST.Location = new System.Drawing.Point(134, 25);
            this.txtMST.Name = "txtMST";
            this.txtMST.Size = new System.Drawing.Size(436, 26);
            this.txtMST.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 176);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 20);
            this.label4.TabIndex = 3;
            this.label4.Text = "Đường dẫn file :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(67, 105);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Token :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Máy chủ :";
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "MisaGetData";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_DoubleClicked);
            // 
            // timer1
            // 
            this.timer1.Interval = 30000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // MisaGetData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(594, 333);
            this.Controls.Add(this.gbName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MisaGetData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Misa Get Data";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Closing);
            this.Load += new System.EventHandler(this.From_Loaded);
            this.gbName.ResumeLayout(false);
            this.gbName.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox gbName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLinkSaveFile;
        private System.Windows.Forms.TextBox txtToken;
        private System.Windows.Forms.TextBox txtMayChu;
        private System.Windows.Forms.TextBox txtMST;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox cbStartUp;
        private System.Windows.Forms.ComboBox cbbDoiTuongXL;
        private System.Windows.Forms.CheckBox cbAutoRun;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtFileDayLai;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Timer timer1;
    }
}

