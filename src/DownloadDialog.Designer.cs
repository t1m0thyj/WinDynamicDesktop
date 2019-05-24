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
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.fileSizeProgressLabel = new System.Windows.Forms.Label();
            this.fileTransferSpeedLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 15);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(228, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Downloading images, please wait...";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(13, 45);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(326, 23);
            this.progressBar1.TabIndex = 0;
            // 
            // fileSizeProgressLabel
            // 
            this.fileSizeProgressLabel.Location = new System.Drawing.Point(15, 80);
            this.fileSizeProgressLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.fileSizeProgressLabel.Name = "fileSizeProgressLabel";
            this.fileSizeProgressLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.fileSizeProgressLabel.Size = new System.Drawing.Size(183, 16);
            this.fileSizeProgressLabel.TabIndex = 3;
            this.fileSizeProgressLabel.Text = "0 MB of 0 MB";
            this.fileSizeProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // fileTransferSpeedLabel
            // 
            this.fileTransferSpeedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fileTransferSpeedLabel.Location = new System.Drawing.Point(250, 80);
            this.fileTransferSpeedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.fileTransferSpeedLabel.Name = "fileTransferSpeedLabel";
            this.fileTransferSpeedLabel.Size = new System.Drawing.Size(92, 16);
            this.fileTransferSpeedLabel.TabIndex = 4;
            this.fileTransferSpeedLabel.Text = "0 MB/s";
            this.fileTransferSpeedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(200, 105);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(142, 28);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.fileSizeProgressLabel);
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.fileTransferSpeedLabel);
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(352, 148);
            this.panel1.TabIndex = 6;
            // 
            // DownloadDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(352, 148);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::WinDynamicDesktop.Properties.Resources.AppIcon;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "DownloadDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinDynamicDesktop";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label fileSizeProgressLabel;
        private System.Windows.Forms.Label fileTransferSpeedLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel panel1;
    }
}