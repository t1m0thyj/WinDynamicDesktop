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
            progressBar1 = new System.Windows.Forms.ProgressBar();
            fileSizeProgressLabel = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            cancelButton = new System.Windows.Forms.Button();
            fileTransferSpeedLabel = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            themeUriList = new System.Windows.Forms.ComboBox();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // progressBar1
            // 
            progressBar1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            progressBar1.Location = new System.Drawing.Point(13, 37);
            progressBar1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(349, 15);
            progressBar1.TabIndex = 5;
            // 
            // fileSizeProgressLabel
            // 
            fileSizeProgressLabel.AutoSize = true;
            fileSizeProgressLabel.Location = new System.Drawing.Point(13, 61);
            fileSizeProgressLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            fileSizeProgressLabel.Name = "fileSizeProgressLabel";
            fileSizeProgressLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            fileSizeProgressLabel.Size = new System.Drawing.Size(78, 15);
            fileSizeProgressLabel.TabIndex = 4;
            fileSizeProgressLabel.Text = "0 MB of 0 MB";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 14);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(192, 15);
            label1.TabIndex = 3;
            label1.Text = "Downloading images, please wait...";
            // 
            // cancelButton
            // 
            cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Location = new System.Drawing.Point(275, 98);
            cancelButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(88, 27);
            cancelButton.TabIndex = 0;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // fileTransferSpeedLabel
            // 
            fileTransferSpeedLabel.Location = new System.Drawing.Point(231, 61);
            fileTransferSpeedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            fileTransferSpeedLabel.Name = "fileTransferSpeedLabel";
            fileTransferSpeedLabel.Size = new System.Drawing.Size(131, 16);
            fileTransferSpeedLabel.TabIndex = 1;
            fileTransferSpeedLabel.Text = "0 KB/s";
            fileTransferSpeedLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            label2.Location = new System.Drawing.Point(12, 84);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(352, 2);
            label2.TabIndex = 0;
            label2.Text = "label2";
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.Controls.Add(themeUriList);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(fileTransferSpeedLabel);
            panel1.Controls.Add(cancelButton);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(fileSizeProgressLabel);
            panel1.Controls.Add(progressBar1);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Margin = new System.Windows.Forms.Padding(2);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(378, 138);
            panel1.TabIndex = 0;
            // 
            // themeUriList
            // 
            themeUriList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            themeUriList.FormattingEnabled = true;
            themeUriList.Location = new System.Drawing.Point(13, 100);
            themeUriList.Name = "themeUriList";
            themeUriList.Size = new System.Drawing.Size(121, 23);
            themeUriList.TabIndex = 6;
            themeUriList.SelectedIndexChanged += themeUriList_SelectedIndexChanged;
            // 
            // DownloadDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSize = true;
            CancelButton = cancelButton;
            ClientSize = new System.Drawing.Size(378, 138);
            ControlBox = false;
            Controls.Add(panel1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = Properties.Resources.AppIcon;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "DownloadDialog";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "WinDynamicDesktop";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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