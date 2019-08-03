namespace WinDynamicDesktop
{
    partial class BrightnessDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrightnessDialog));
            this.panel1 = new System.Windows.Forms.Panel();
            this.applyButton = new System.Windows.Forms.Button();
            this.ddcErrorTextBox = new System.Windows.Forms.TextBox();
            this.setBrightnessGroupBox = new System.Windows.Forms.GroupBox();
            this.allNightComboBox = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.allDayComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.showBrightnessNotificationToastcheckBox = new System.Windows.Forms.CheckBox();
            this.enableAutoBrightnesscheckBox = new System.Windows.Forms.CheckBox();
            this.nightComboBox = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.sunsetComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dayComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.sunriseComboBox = new System.Windows.Forms.ComboBox();
            this.setBrightnessCustomcheckBox = new System.Windows.Forms.CheckBox();
            this.setBrightnessAutocheckBox = new System.Windows.Forms.CheckBox();
            this.currentDisplayBrightnessValueLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ddcSupportLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.setBrightnessGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Controls.Add(this.ddcErrorTextBox);
            this.panel1.Controls.Add(this.setBrightnessGroupBox);
            this.panel1.Controls.Add(this.currentDisplayBrightnessValueLabel);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.ddcSupportLabel);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.okButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(782, 333);
            this.panel1.TabIndex = 0;
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(669, 283);
            this.applyButton.Margin = new System.Windows.Forms.Padding(4);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(100, 28);
            this.applyButton.TabIndex = 15;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // ddcErrorTextBox
            // 
            this.ddcErrorTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.ddcErrorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ddcErrorTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ddcErrorTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ddcErrorTextBox.Location = new System.Drawing.Point(362, 15);
            this.ddcErrorTextBox.MaxLength = 64;
            this.ddcErrorTextBox.Multiline = true;
            this.ddcErrorTextBox.Name = "ddcErrorTextBox";
            this.ddcErrorTextBox.ReadOnly = true;
            this.ddcErrorTextBox.Size = new System.Drawing.Size(407, 59);
            this.ddcErrorTextBox.TabIndex = 14;
            this.ddcErrorTextBox.Text = "Your display does not support DDC/CI. Check with your manufacturer if this featur" +
    "e is available.";
            this.ddcErrorTextBox.Visible = false;
            // 
            // setBrightnessGroupBox
            // 
            this.setBrightnessGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.setBrightnessGroupBox.Controls.Add(this.allNightComboBox);
            this.setBrightnessGroupBox.Controls.Add(this.label9);
            this.setBrightnessGroupBox.Controls.Add(this.allDayComboBox);
            this.setBrightnessGroupBox.Controls.Add(this.label8);
            this.setBrightnessGroupBox.Controls.Add(this.showBrightnessNotificationToastcheckBox);
            this.setBrightnessGroupBox.Controls.Add(this.enableAutoBrightnesscheckBox);
            this.setBrightnessGroupBox.Controls.Add(this.nightComboBox);
            this.setBrightnessGroupBox.Controls.Add(this.label7);
            this.setBrightnessGroupBox.Controls.Add(this.sunsetComboBox);
            this.setBrightnessGroupBox.Controls.Add(this.label6);
            this.setBrightnessGroupBox.Controls.Add(this.dayComboBox);
            this.setBrightnessGroupBox.Controls.Add(this.label5);
            this.setBrightnessGroupBox.Controls.Add(this.label3);
            this.setBrightnessGroupBox.Controls.Add(this.sunriseComboBox);
            this.setBrightnessGroupBox.Controls.Add(this.setBrightnessCustomcheckBox);
            this.setBrightnessGroupBox.Controls.Add(this.setBrightnessAutocheckBox);
            this.setBrightnessGroupBox.Location = new System.Drawing.Point(14, 80);
            this.setBrightnessGroupBox.Name = "setBrightnessGroupBox";
            this.setBrightnessGroupBox.Size = new System.Drawing.Size(755, 170);
            this.setBrightnessGroupBox.TabIndex = 13;
            this.setBrightnessGroupBox.TabStop = false;
            this.setBrightnessGroupBox.Text = "Auto Brightness Options";
            // 
            // allNightComboBox
            // 
            this.allNightComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.allNightComboBox.FormattingEnabled = true;
            this.allNightComboBox.Location = new System.Drawing.Point(585, 120);
            this.allNightComboBox.Name = "allNightComboBox";
            this.allNightComboBox.Size = new System.Drawing.Size(114, 24);
            this.allNightComboBox.TabIndex = 26;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(585, 95);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(58, 17);
            this.label9.TabIndex = 25;
            this.label9.Text = "All night";
            // 
            // allDayComboBox
            // 
            this.allDayComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.allDayComboBox.FormattingEnabled = true;
            this.allDayComboBox.Location = new System.Drawing.Point(585, 60);
            this.allDayComboBox.Name = "allDayComboBox";
            this.allDayComboBox.Size = new System.Drawing.Size(114, 24);
            this.allDayComboBox.TabIndex = 24;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(585, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 17);
            this.label8.TabIndex = 23;
            this.label8.Text = "All day";
            // 
            // showBrightnessNotificationToastcheckBox
            // 
            this.showBrightnessNotificationToastcheckBox.AutoSize = true;
            this.showBrightnessNotificationToastcheckBox.Location = new System.Drawing.Point(20, 125);
            this.showBrightnessNotificationToastcheckBox.Name = "showBrightnessNotificationToastcheckBox";
            this.showBrightnessNotificationToastcheckBox.Size = new System.Drawing.Size(178, 21);
            this.showBrightnessNotificationToastcheckBox.TabIndex = 22;
            this.showBrightnessNotificationToastcheckBox.Text = "Show Notification Toast";
            this.showBrightnessNotificationToastcheckBox.UseVisualStyleBackColor = true;
            // 
            // enableAutoBrightnesscheckBox
            // 
            this.enableAutoBrightnesscheckBox.AutoSize = true;
            this.enableAutoBrightnesscheckBox.Location = new System.Drawing.Point(20, 35);
            this.enableAutoBrightnesscheckBox.Name = "enableAutoBrightnesscheckBox";
            this.enableAutoBrightnesscheckBox.Size = new System.Drawing.Size(74, 21);
            this.enableAutoBrightnesscheckBox.TabIndex = 21;
            this.enableAutoBrightnesscheckBox.Text = "Enable";
            this.enableAutoBrightnesscheckBox.UseVisualStyleBackColor = true;
            this.enableAutoBrightnesscheckBox.CheckedChanged += new System.EventHandler(this.EnableAutoBrightnesscheckBox_CheckedChanged);
            // 
            // nightComboBox
            // 
            this.nightComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nightComboBox.FormattingEnabled = true;
            this.nightComboBox.Location = new System.Drawing.Point(443, 120);
            this.nightComboBox.Name = "nightComboBox";
            this.nightComboBox.Size = new System.Drawing.Size(114, 24);
            this.nightComboBox.TabIndex = 20;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(440, 95);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 17);
            this.label7.TabIndex = 19;
            this.label7.Text = "Night";
            // 
            // sunsetComboBox
            // 
            this.sunsetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sunsetComboBox.FormattingEnabled = true;
            this.sunsetComboBox.Location = new System.Drawing.Point(443, 60);
            this.sunsetComboBox.Name = "sunsetComboBox";
            this.sunsetComboBox.Size = new System.Drawing.Size(114, 24);
            this.sunsetComboBox.TabIndex = 18;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(440, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 17);
            this.label6.TabIndex = 17;
            this.label6.Text = "Sunset";
            // 
            // dayComboBox
            // 
            this.dayComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dayComboBox.FormattingEnabled = true;
            this.dayComboBox.Location = new System.Drawing.Point(300, 120);
            this.dayComboBox.Name = "dayComboBox";
            this.dayComboBox.Size = new System.Drawing.Size(114, 24);
            this.dayComboBox.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(300, 95);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 17);
            this.label5.TabIndex = 15;
            this.label5.Text = "Day";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(300, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 17);
            this.label3.TabIndex = 14;
            this.label3.Text = "Sunrise";
            // 
            // sunriseComboBox
            // 
            this.sunriseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sunriseComboBox.FormattingEnabled = true;
            this.sunriseComboBox.Location = new System.Drawing.Point(300, 60);
            this.sunriseComboBox.Name = "sunriseComboBox";
            this.sunriseComboBox.Size = new System.Drawing.Size(114, 24);
            this.sunriseComboBox.TabIndex = 2;
            // 
            // setBrightnessCustomcheckBox
            // 
            this.setBrightnessCustomcheckBox.AutoSize = true;
            this.setBrightnessCustomcheckBox.Location = new System.Drawing.Point(20, 95);
            this.setBrightnessCustomcheckBox.Name = "setBrightnessCustomcheckBox";
            this.setBrightnessCustomcheckBox.Size = new System.Drawing.Size(77, 21);
            this.setBrightnessCustomcheckBox.TabIndex = 1;
            this.setBrightnessCustomcheckBox.Text = "Custom";
            this.setBrightnessCustomcheckBox.UseVisualStyleBackColor = true;
            this.setBrightnessCustomcheckBox.CheckedChanged += new System.EventHandler(this.SetBrightnessCustomcheckBox_CheckedChanged);
            // 
            // setBrightnessAutocheckBox
            // 
            this.setBrightnessAutocheckBox.AutoSize = true;
            this.setBrightnessAutocheckBox.Location = new System.Drawing.Point(20, 65);
            this.setBrightnessAutocheckBox.Name = "setBrightnessAutocheckBox";
            this.setBrightnessAutocheckBox.Size = new System.Drawing.Size(59, 21);
            this.setBrightnessAutocheckBox.TabIndex = 0;
            this.setBrightnessAutocheckBox.Text = "Auto";
            this.setBrightnessAutocheckBox.UseVisualStyleBackColor = true;
            this.setBrightnessAutocheckBox.CheckedChanged += new System.EventHandler(this.SetBrightnessAutocheckBox_CheckedChanged);
            // 
            // currentDisplayBrightnessValueLabel
            // 
            this.currentDisplayBrightnessValueLabel.AutoSize = true;
            this.currentDisplayBrightnessValueLabel.Location = new System.Drawing.Point(220, 40);
            this.currentDisplayBrightnessValueLabel.Name = "currentDisplayBrightnessValueLabel";
            this.currentDisplayBrightnessValueLabel.Size = new System.Drawing.Size(44, 17);
            this.currentDisplayBrightnessValueLabel.TabIndex = 11;
            this.currentDisplayBrightnessValueLabel.Text = "100%";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(180, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "Current Display Brightness:";
            // 
            // ddcSupportLabel
            // 
            this.ddcSupportLabel.AutoSize = true;
            this.ddcSupportLabel.Location = new System.Drawing.Point(220, 15);
            this.ddcSupportLabel.Name = "ddcSupportLabel";
            this.ddcSupportLabel.Size = new System.Drawing.Size(74, 17);
            this.ddcSupportLabel.TabIndex = 9;
            this.ddcSupportLabel.Text = "Supported";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "DDC/CI Support:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(14, 260);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(755, 2);
            this.label2.TabIndex = 6;
            this.label2.Text = "label2";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(561, 283);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 28);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(453, 283);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 28);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // BrightnessDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(782, 333);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BrightnessDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Set Auto Brightness";
            this.Load += new System.EventHandler(this.BrightnessDialog_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.setBrightnessGroupBox.ResumeLayout(false);
            this.setBrightnessGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox sunriseComboBox;
        private System.Windows.Forms.CheckBox setBrightnessCustomcheckBox;
        private System.Windows.Forms.CheckBox setBrightnessAutocheckBox;
        private System.Windows.Forms.Label currentDisplayBrightnessValueLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label ddcSupportLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox sunsetComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox dayComboBox;
        private System.Windows.Forms.ComboBox nightComboBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox enableAutoBrightnesscheckBox;
        private System.Windows.Forms.TextBox ddcErrorTextBox;
        private System.Windows.Forms.CheckBox showBrightnessNotificationToastcheckBox;
        private System.Windows.Forms.ComboBox allNightComboBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox allDayComboBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.GroupBox setBrightnessGroupBox;
    }
}