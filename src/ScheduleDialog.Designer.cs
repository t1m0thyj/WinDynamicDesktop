namespace WinDynamicDesktop
{
    partial class ScheduleDialog
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
            locationLabel = new System.Windows.Forms.Label();
            locationBox = new System.Windows.Forms.TextBox();
            okButton = new System.Windows.Forms.Button();
            cancelButton = new System.Windows.Forms.Button();
            radioButton1 = new System.Windows.Forms.RadioButton();
            radioButton2 = new System.Windows.Forms.RadioButton();
            radioButton3 = new System.Windows.Forms.RadioButton();
            locationPermissionLabel = new System.Windows.Forms.Label();
            sunriseTimeLabel = new System.Windows.Forms.Label();
            sunsetTimeLabel = new System.Windows.Forms.Label();
            sunriseTimePicker = new System.Windows.Forms.DateTimePicker();
            sunsetTimePicker = new System.Windows.Forms.DateTimePicker();
            sunriseSunsetDurationLabel = new System.Windows.Forms.Label();
            grantPermissionButton = new System.Windows.Forms.Button();
            sunriseSunsetDurationBox = new System.Windows.Forms.NumericUpDown();
            checkPermissionButton = new System.Windows.Forms.Button();
            sunriseSunsetDurationUnitLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)sunriseSunsetDurationBox).BeginInit();
            SuspendLayout();
            // 
            // locationLabel
            // 
            locationLabel.AutoSize = true;
            locationLabel.Location = new System.Drawing.Point(31, 46);
            locationLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            locationLabel.Name = "locationLabel";
            locationLabel.Size = new System.Drawing.Size(215, 15);
            locationLabel.TabIndex = 13;
            locationLabel.Text = "Enter your location (e.g., New York NY):";
            // 
            // locationBox
            // 
            locationBox.Location = new System.Drawing.Point(35, 65);
            locationBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            locationBox.Name = "locationBox";
            locationBox.Size = new System.Drawing.Size(341, 23);
            locationBox.TabIndex = 1;
            locationBox.TextChanged += OnInputValueChanged;
            // 
            // okButton
            // 
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Location = new System.Drawing.Point(194, 300);
            okButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(88, 27);
            okButton.TabIndex = 9;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Location = new System.Drawing.Point(289, 300);
            cancelButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(88, 27);
            cancelButton.TabIndex = 10;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Checked = true;
            radioButton1.Location = new System.Drawing.Point(14, 16);
            radioButton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new System.Drawing.Size(293, 19);
            radioButton1.TabIndex = 0;
            radioButton1.TabStop = true;
            radioButton1.Text = "Use location to determine sunrise and sunset times";
            radioButton1.UseVisualStyleBackColor = true;
            radioButton1.CheckedChanged += OnInputValueChanged;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Enabled = false;
            radioButton2.Location = new System.Drawing.Point(14, 106);
            radioButton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new System.Drawing.Size(284, 19);
            radioButton2.TabIndex = 2;
            radioButton2.TabStop = true;
            radioButton2.Text = "Use Windows location service to determine times";
            radioButton2.UseVisualStyleBackColor = true;
            radioButton2.CheckedChanged += OnInputValueChanged;
            // 
            // radioButton3
            // 
            radioButton3.AutoSize = true;
            radioButton3.Location = new System.Drawing.Point(14, 202);
            radioButton3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton3.Name = "radioButton3";
            radioButton3.Size = new System.Drawing.Size(219, 19);
            radioButton3.TabIndex = 5;
            radioButton3.TabStop = true;
            radioButton3.Text = "Use specific sunrise and sunset times";
            radioButton3.UseVisualStyleBackColor = true;
            radioButton3.CheckedChanged += OnInputValueChanged;
            // 
            // locationPermissionLabel
            // 
            locationPermissionLabel.AutoSize = true;
            locationPermissionLabel.Enabled = false;
            locationPermissionLabel.Location = new System.Drawing.Point(33, 136);
            locationPermissionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            locationPermissionLabel.Name = "locationPermissionLabel";
            locationPermissionLabel.Size = new System.Drawing.Size(161, 15);
            locationPermissionLabel.TabIndex = 8;
            locationPermissionLabel.Text = "Only available in Windows 10";
            // 
            // sunriseTimeLabel
            // 
            sunriseTimeLabel.AutoSize = true;
            sunriseTimeLabel.Enabled = false;
            sunriseTimeLabel.Location = new System.Drawing.Point(31, 233);
            sunriseTimeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            sunriseTimeLabel.Name = "sunriseTimeLabel";
            sunriseTimeLabel.Size = new System.Drawing.Size(48, 15);
            sunriseTimeLabel.TabIndex = 7;
            sunriseTimeLabel.Text = "Sunrise:";
            // 
            // sunsetTimeLabel
            // 
            sunsetTimeLabel.AutoSize = true;
            sunsetTimeLabel.Enabled = false;
            sunsetTimeLabel.Location = new System.Drawing.Point(34, 263);
            sunsetTimeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            sunsetTimeLabel.Name = "sunsetTimeLabel";
            sunsetTimeLabel.Size = new System.Drawing.Size(45, 15);
            sunsetTimeLabel.TabIndex = 6;
            sunsetTimeLabel.Text = "Sunset:";
            // 
            // sunriseTimePicker
            // 
            sunriseTimePicker.Enabled = false;
            sunriseTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            sunriseTimePicker.Location = new System.Drawing.Point(91, 228);
            sunriseTimePicker.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            sunriseTimePicker.Name = "sunriseTimePicker";
            sunriseTimePicker.ShowUpDown = true;
            sunriseTimePicker.Size = new System.Drawing.Size(109, 23);
            sunriseTimePicker.TabIndex = 6;
            sunriseTimePicker.Value = new System.DateTime(1970, 1, 1, 6, 0, 0, 0);
            sunriseTimePicker.ValueChanged += OnInputValueChanged;
            // 
            // sunsetTimePicker
            // 
            sunsetTimePicker.Enabled = false;
            sunsetTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            sunsetTimePicker.Location = new System.Drawing.Point(91, 260);
            sunsetTimePicker.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            sunsetTimePicker.Name = "sunsetTimePicker";
            sunsetTimePicker.ShowUpDown = true;
            sunsetTimePicker.Size = new System.Drawing.Size(109, 23);
            sunsetTimePicker.TabIndex = 7;
            sunsetTimePicker.Value = new System.DateTime(1970, 1, 1, 18, 0, 0, 0);
            sunsetTimePicker.ValueChanged += OnInputValueChanged;
            // 
            // sunriseSunsetDurationLabel
            // 
            sunriseSunsetDurationLabel.AutoSize = true;
            sunriseSunsetDurationLabel.Enabled = false;
            sunriseSunsetDurationLabel.Location = new System.Drawing.Point(226, 233);
            sunriseSunsetDurationLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            sunriseSunsetDurationLabel.Name = "sunriseSunsetDurationLabel";
            sunriseSunsetDurationLabel.Size = new System.Drawing.Size(137, 15);
            sunriseSunsetDurationLabel.TabIndex = 3;
            sunriseSunsetDurationLabel.Text = "Sunrise/Sunset Duration:";
            // 
            // grantPermissionButton
            // 
            grantPermissionButton.Enabled = false;
            grantPermissionButton.Location = new System.Drawing.Point(36, 160);
            grantPermissionButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            grantPermissionButton.Name = "grantPermissionButton";
            grantPermissionButton.Size = new System.Drawing.Size(164, 27);
            grantPermissionButton.TabIndex = 3;
            grantPermissionButton.Text = "Grant Permission";
            grantPermissionButton.UseVisualStyleBackColor = true;
            grantPermissionButton.Click += grantPermissionButton_Click;
            // 
            // sunriseSunsetDurationBox
            // 
            sunriseSunsetDurationBox.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            sunriseSunsetDurationBox.Location = new System.Drawing.Point(230, 255);
            sunriseSunsetDurationBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            sunriseSunsetDurationBox.Maximum = new decimal(new int[] { 1800, 0, 0, 0 });
            sunriseSunsetDurationBox.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            sunriseSunsetDurationBox.Name = "sunriseSunsetDurationBox";
            sunriseSunsetDurationBox.Size = new System.Drawing.Size(75, 23);
            sunriseSunsetDurationBox.TabIndex = 8;
            sunriseSunsetDurationBox.Value = new decimal(new int[] { 120, 0, 0, 0 });
            // 
            // checkPermissionButton
            // 
            checkPermissionButton.Enabled = false;
            checkPermissionButton.Location = new System.Drawing.Point(212, 160);
            checkPermissionButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkPermissionButton.Name = "checkPermissionButton";
            checkPermissionButton.Size = new System.Drawing.Size(164, 27);
            checkPermissionButton.TabIndex = 4;
            checkPermissionButton.Text = "Check for Permission";
            checkPermissionButton.UseVisualStyleBackColor = true;
            checkPermissionButton.Click += checkPermissionButton_Click;
            // 
            // sunriseSunsetDurationUnitLabel
            // 
            sunriseSunsetDurationUnitLabel.AutoSize = true;
            sunriseSunsetDurationUnitLabel.Location = new System.Drawing.Point(309, 258);
            sunriseSunsetDurationUnitLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            sunriseSunsetDurationUnitLabel.Name = "sunriseSunsetDurationUnitLabel";
            sunriseSunsetDurationUnitLabel.Size = new System.Drawing.Size(50, 15);
            sunriseSunsetDurationUnitLabel.TabIndex = 14;
            sunriseSunsetDurationUnitLabel.Text = "minutes";
            // 
            // ScheduleDialog
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new System.Drawing.Size(413, 347);
            Controls.Add(sunriseSunsetDurationUnitLabel);
            Controls.Add(checkPermissionButton);
            Controls.Add(sunriseSunsetDurationBox);
            Controls.Add(grantPermissionButton);
            Controls.Add(sunriseSunsetDurationLabel);
            Controls.Add(sunsetTimePicker);
            Controls.Add(sunriseTimePicker);
            Controls.Add(sunsetTimeLabel);
            Controls.Add(sunriseTimeLabel);
            Controls.Add(locationPermissionLabel);
            Controls.Add(radioButton3);
            Controls.Add(radioButton2);
            Controls.Add(radioButton1);
            Controls.Add(cancelButton);
            Controls.Add(locationLabel);
            Controls.Add(okButton);
            Controls.Add(locationBox);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = Properties.Resources.AppIcon;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "ScheduleDialog";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Configure Schedule";
            Load += InputDialog_Load;
            ((System.ComponentModel.ISupportInitialize)sunriseSunsetDurationBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label locationLabel;
        private System.Windows.Forms.TextBox locationBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.Label locationPermissionLabel;
        private System.Windows.Forms.Label sunriseTimeLabel;
        private System.Windows.Forms.Label sunsetTimeLabel;
        private System.Windows.Forms.DateTimePicker sunriseTimePicker;
        private System.Windows.Forms.DateTimePicker sunsetTimePicker;
        private System.Windows.Forms.Label sunriseSunsetDurationLabel;
        private System.Windows.Forms.Button grantPermissionButton;
        private System.Windows.Forms.NumericUpDown sunriseSunsetDurationBox;
        private System.Windows.Forms.Button checkPermissionButton;
        private System.Windows.Forms.Label sunriseSunsetDurationUnitLabel;
    }
}

