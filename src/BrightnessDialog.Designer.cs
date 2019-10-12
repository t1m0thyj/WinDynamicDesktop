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
            this.panel1 = new System.Windows.Forms.Panel();
            this.applyButton = new System.Windows.Forms.Button();
            this.ddcErrorTextBox = new System.Windows.Forms.TextBox();
            this.setBrightnessGroupBox = new System.Windows.Forms.GroupBox();
            this.allNightnumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.allDaynumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.showBrightnessNotificationToastcheckBox = new System.Windows.Forms.CheckBox();
            this.setCustomAutoBrightnessRadioButton = new System.Windows.Forms.RadioButton();
            this.nightNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.enableAutoBrightnessRadioButton = new System.Windows.Forms.RadioButton();
            this.disableAutoBrightnessRadioButton = new System.Windows.Forms.RadioButton();
            this.sunsetNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.dayNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.sunriseNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.currentDisplayBrightnessValueLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.setBrightnessGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.allNightnumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.allDaynumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nightNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sunsetNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dayNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sunriseNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.applyButton);
            this.panel1.Controls.Add(this.ddcErrorTextBox);
            this.panel1.Controls.Add(this.setBrightnessGroupBox);
            this.panel1.Controls.Add(this.currentDisplayBrightnessValueLabel);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.okButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(782, 303);
            this.panel1.TabIndex = 0;
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(670, 262);
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
            this.ddcErrorTextBox.Location = new System.Drawing.Point(315, 20);
            this.ddcErrorTextBox.MaxLength = 64;
            this.ddcErrorTextBox.Multiline = true;
            this.ddcErrorTextBox.Name = "ddcErrorTextBox";
            this.ddcErrorTextBox.ReadOnly = true;
            this.ddcErrorTextBox.Size = new System.Drawing.Size(454, 44);
            this.ddcErrorTextBox.TabIndex = 14;
            this.ddcErrorTextBox.Text = "Your display does not support DDC/CI. Check with your manufacturer if this featur" +
    "e is available.";
            this.ddcErrorTextBox.Visible = false;
            // 
            // setBrightnessGroupBox
            // 
            this.setBrightnessGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.setBrightnessGroupBox.Controls.Add(this.allNightnumericUpDown);
            this.setBrightnessGroupBox.Controls.Add(this.allDaynumericUpDown);
            this.setBrightnessGroupBox.Controls.Add(this.showBrightnessNotificationToastcheckBox);
            this.setBrightnessGroupBox.Controls.Add(this.setCustomAutoBrightnessRadioButton);
            this.setBrightnessGroupBox.Controls.Add(this.nightNumericUpDown);
            this.setBrightnessGroupBox.Controls.Add(this.enableAutoBrightnessRadioButton);
            this.setBrightnessGroupBox.Controls.Add(this.disableAutoBrightnessRadioButton);
            this.setBrightnessGroupBox.Controls.Add(this.sunsetNumericUpDown);
            this.setBrightnessGroupBox.Controls.Add(this.dayNumericUpDown);
            this.setBrightnessGroupBox.Controls.Add(this.sunriseNumericUpDown);
            this.setBrightnessGroupBox.Controls.Add(this.label9);
            this.setBrightnessGroupBox.Controls.Add(this.label8);
            this.setBrightnessGroupBox.Controls.Add(this.label7);
            this.setBrightnessGroupBox.Controls.Add(this.label6);
            this.setBrightnessGroupBox.Controls.Add(this.label5);
            this.setBrightnessGroupBox.Controls.Add(this.label3);
            this.setBrightnessGroupBox.Location = new System.Drawing.Point(15, 70);
            this.setBrightnessGroupBox.Name = "setBrightnessGroupBox";
            this.setBrightnessGroupBox.Size = new System.Drawing.Size(755, 170);
            this.setBrightnessGroupBox.TabIndex = 13;
            this.setBrightnessGroupBox.TabStop = false;
            this.setBrightnessGroupBox.Text = "Auto Brightness Options";
            // 
            // allNightnumericUpDown
            // 
            this.allNightnumericUpDown.Location = new System.Drawing.Point(585, 120);
            this.allNightnumericUpDown.Name = "allNightnumericUpDown";
            this.allNightnumericUpDown.Size = new System.Drawing.Size(120, 22);
            this.allNightnumericUpDown.TabIndex = 32;
            // 
            // allDaynumericUpDown
            // 
            this.allDaynumericUpDown.Location = new System.Drawing.Point(585, 60);
            this.allDaynumericUpDown.Name = "allDaynumericUpDown";
            this.allDaynumericUpDown.Size = new System.Drawing.Size(120, 22);
            this.allDaynumericUpDown.TabIndex = 31;
            // 
            // showBrightnessNotificationToastcheckBox
            // 
            this.showBrightnessNotificationToastcheckBox.AutoSize = true;
            this.showBrightnessNotificationToastcheckBox.Location = new System.Drawing.Point(19, 123);
            this.showBrightnessNotificationToastcheckBox.Name = "showBrightnessNotificationToastcheckBox";
            this.showBrightnessNotificationToastcheckBox.Size = new System.Drawing.Size(178, 21);
            this.showBrightnessNotificationToastcheckBox.TabIndex = 22;
            this.showBrightnessNotificationToastcheckBox.Text = "Show Notification Toast";
            this.showBrightnessNotificationToastcheckBox.UseVisualStyleBackColor = true;
            // 
            // setCustomAutoBrightnessRadioButton
            // 
            this.setCustomAutoBrightnessRadioButton.AutoSize = true;
            this.setCustomAutoBrightnessRadioButton.Location = new System.Drawing.Point(19, 90);
            this.setCustomAutoBrightnessRadioButton.Name = "setCustomAutoBrightnessRadioButton";
            this.setCustomAutoBrightnessRadioButton.Size = new System.Drawing.Size(172, 21);
            this.setCustomAutoBrightnessRadioButton.TabIndex = 2;
            this.setCustomAutoBrightnessRadioButton.Text = "Set Custom Brightness";
            this.setCustomAutoBrightnessRadioButton.UseVisualStyleBackColor = true;
            this.setCustomAutoBrightnessRadioButton.CheckedChanged += new System.EventHandler(this.SetCustomAutoBrightnessRadioButton_CheckedChanged);
            // 
            // nightNumericUpDown
            // 
            this.nightNumericUpDown.Location = new System.Drawing.Point(443, 120);
            this.nightNumericUpDown.Name = "nightNumericUpDown";
            this.nightNumericUpDown.Size = new System.Drawing.Size(120, 22);
            this.nightNumericUpDown.TabIndex = 30;
            // 
            // enableAutoBrightnessRadioButton
            // 
            this.enableAutoBrightnessRadioButton.AutoSize = true;
            this.enableAutoBrightnessRadioButton.Location = new System.Drawing.Point(19, 60);
            this.enableAutoBrightnessRadioButton.Name = "enableAutoBrightnessRadioButton";
            this.enableAutoBrightnessRadioButton.Size = new System.Drawing.Size(116, 21);
            this.enableAutoBrightnessRadioButton.TabIndex = 1;
            this.enableAutoBrightnessRadioButton.Text = "Enable (Auto)";
            this.enableAutoBrightnessRadioButton.UseVisualStyleBackColor = true;
            this.enableAutoBrightnessRadioButton.CheckedChanged += new System.EventHandler(this.EnableAutoBrightnessRadioButton_CheckedChanged);
            // 
            // disableAutoBrightnessRadioButton
            // 
            this.disableAutoBrightnessRadioButton.AutoSize = true;
            this.disableAutoBrightnessRadioButton.Checked = true;
            this.disableAutoBrightnessRadioButton.Location = new System.Drawing.Point(19, 30);
            this.disableAutoBrightnessRadioButton.Name = "disableAutoBrightnessRadioButton";
            this.disableAutoBrightnessRadioButton.Size = new System.Drawing.Size(76, 21);
            this.disableAutoBrightnessRadioButton.TabIndex = 0;
            this.disableAutoBrightnessRadioButton.TabStop = true;
            this.disableAutoBrightnessRadioButton.Text = "Disable";
            this.disableAutoBrightnessRadioButton.UseVisualStyleBackColor = true;
            this.disableAutoBrightnessRadioButton.CheckedChanged += new System.EventHandler(this.DisableAutoBrightnessRadioButton_CheckedChanged);
            // 
            // sunsetNumericUpDown
            // 
            this.sunsetNumericUpDown.Location = new System.Drawing.Point(443, 60);
            this.sunsetNumericUpDown.Name = "sunsetNumericUpDown";
            this.sunsetNumericUpDown.Size = new System.Drawing.Size(120, 22);
            this.sunsetNumericUpDown.TabIndex = 29;
            // 
            // dayNumericUpDown
            // 
            this.dayNumericUpDown.Location = new System.Drawing.Point(300, 120);
            this.dayNumericUpDown.Name = "dayNumericUpDown";
            this.dayNumericUpDown.Size = new System.Drawing.Size(120, 22);
            this.dayNumericUpDown.TabIndex = 28;
            // 
            // sunriseNumericUpDown
            // 
            this.sunriseNumericUpDown.Location = new System.Drawing.Point(300, 60);
            this.sunriseNumericUpDown.Name = "sunriseNumericUpDown";
            this.sunriseNumericUpDown.Size = new System.Drawing.Size(120, 22);
            this.sunriseNumericUpDown.TabIndex = 27;
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
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(585, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 17);
            this.label8.TabIndex = 23;
            this.label8.Text = "All day";
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
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(451, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 17);
            this.label6.TabIndex = 17;
            this.label6.Text = "Sunset";
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
            // currentDisplayBrightnessValueLabel
            // 
            this.currentDisplayBrightnessValueLabel.AutoSize = true;
            this.currentDisplayBrightnessValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentDisplayBrightnessValueLabel.Location = new System.Drawing.Point(15, 40);
            this.currentDisplayBrightnessValueLabel.Name = "currentDisplayBrightnessValueLabel";
            this.currentDisplayBrightnessValueLabel.Size = new System.Drawing.Size(44, 17);
            this.currentDisplayBrightnessValueLabel.TabIndex = 11;
            this.currentDisplayBrightnessValueLabel.Text = "100%";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(180, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "Current Display Brightness:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(14, 250);
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
            this.cancelButton.Location = new System.Drawing.Point(560, 262);
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
            this.okButton.Location = new System.Drawing.Point(450, 262);
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
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(782, 303);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::WinDynamicDesktop.Properties.Resources.AppIcon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BrightnessDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Set Auto Brightness";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BrightnessDialog_FormClosing);
            this.Load += new System.EventHandler(this.BrightnessDialog_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.setBrightnessGroupBox.ResumeLayout(false);
            this.setBrightnessGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.allNightnumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.allDaynumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nightNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sunsetNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dayNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sunriseNumericUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox ddcErrorTextBox;
        private System.Windows.Forms.CheckBox showBrightnessNotificationToastcheckBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.GroupBox setBrightnessGroupBox;
        private System.Windows.Forms.NumericUpDown sunriseNumericUpDown;
        private System.Windows.Forms.NumericUpDown nightNumericUpDown;
        private System.Windows.Forms.NumericUpDown sunsetNumericUpDown;
        private System.Windows.Forms.NumericUpDown dayNumericUpDown;
        private System.Windows.Forms.RadioButton setCustomAutoBrightnessRadioButton;
        private System.Windows.Forms.RadioButton enableAutoBrightnessRadioButton;
        private System.Windows.Forms.RadioButton disableAutoBrightnessRadioButton;
        private System.Windows.Forms.NumericUpDown allNightnumericUpDown;
        private System.Windows.Forms.NumericUpDown allDaynumericUpDown;
        public System.Windows.Forms.Label currentDisplayBrightnessValueLabel;
    }
}