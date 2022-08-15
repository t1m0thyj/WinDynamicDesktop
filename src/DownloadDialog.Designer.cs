namespace WinDynamicDesktop
{
    partial class DownloadDialog
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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.fileSizeProgressLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.fileTransferSpeedLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.themeUriList = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(13, 37);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(349, 15);
            this.progressBar1.TabIndex = 5;
            // 
            // fileSizeProgressLabel
            // 
            this.fileSizeProgressLabel.AutoSize = true;
            this.fileSizeProgressLabel.Location = new System.Drawing.Point(13, 61);
            this.fileSizeProgressLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.fileSizeProgressLabel.Name = "fileSizeProgressLabel";
            this.fileSizeProgressLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.fileSizeProgressLabel.Size = new System.Drawing.Size(78, 15);
            this.fileSizeProgressLabel.TabIndex = 4;
            this.fileSizeProgressLabel.Text = "0 MB of 0 MB";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(192, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Downloading images, please wait...";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(275, 98);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(88, 27);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // fileTransferSpeedLabel
            // 
            this.fileTransferSpeedLabel.Location = new System.Drawing.Point(231, 61);
            this.fileTransferSpeedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.fileTransferSpeedLabel.Name = "fileTransferSpeedLabel";
            this.fileTransferSpeedLabel.Size = new System.Drawing.Size(131, 16);
            this.fileTransferSpeedLabel.TabIndex = 1;
            this.fileTransferSpeedLabel.Text = "0 KB/s";
            this.fileTransferSpeedLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(12, 84);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(352, 2);
            this.label2.TabIndex = 0;
            this.label2.Text = "label2";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.themeUriList);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.fileTransferSpeedLabel);
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.fileSizeProgressLabel);
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(378, 138);
            this.panel1.TabIndex = 0;
            // 
            // themeUriList
            // 
            this.themeUriList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.themeUriList.FormattingEnabled = true;
            this.themeUriList.Location = new System.Drawing.Point(13, 100);
            this.themeUriList.Name = "themeUriList";
            this.themeUriList.Size = new System.Drawing.Size(121, 23);
            this.themeUriList.TabIndex = 6;
            this.themeUriList.SelectedIndexChanged += new System.EventHandler(this.themeUriList_SelectedIndexChanged);
            // 
            // DownloadDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(378, 138);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::WinDynamicDesktop.Properties.Resources.AppIcon;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "DownloadDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinDynamicDesktop";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label fileSizeProgressLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label fileTransferSpeedLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox themeUriList;
    }
}