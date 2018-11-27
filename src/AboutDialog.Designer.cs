namespace WinDynamicDesktop
{
    partial class AboutDialog
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
            this.iconBox = new System.Windows.Forms.PictureBox();
            this.nameLabel = new DarkUI.Controls.DarkLabel();
            this.copyrightLabel = new DarkUI.Controls.DarkLabel();
            this.descriptionLabel = new DarkUI.Controls.DarkLabel();
            this.websiteLabel = new System.Windows.Forms.LinkLabel();
            this.closeButton = new DarkUI.Controls.DarkButton();
            this.creditsButton = new DarkUI.Controls.DarkButton();
            this.donateButton = new DarkUI.Controls.DarkButton();
            this.creditsBox = new DarkUI.Controls.DarkTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
            this.SuspendLayout();
            // 
            // iconBox
            // 
            this.iconBox.Location = new System.Drawing.Point(5, 5);
            this.iconBox.Name = "iconBox";
            this.iconBox.Size = new System.Drawing.Size(48, 48);
            this.iconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.iconBox.TabIndex = 0;
            this.iconBox.TabStop = false;
            // 
            // nameLabel
            // 
            this.nameLabel.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nameLabel.Location = new System.Drawing.Point(12, 6);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(428, 29);
            this.nameLabel.TabIndex = 1;
            this.nameLabel.Text = "WinDynamicDesktop";
            this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // copyrightLabel
            // 
            this.copyrightLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.copyrightLabel.Location = new System.Drawing.Point(12, 38);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.Size = new System.Drawing.Size(428, 20);
            this.copyrightLabel.TabIndex = 7;
            this.copyrightLabel.Text = "Copyright © 2018 Timothy Johnson";
            this.copyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.descriptionLabel.Location = new System.Drawing.Point(11, 59);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(432, 20);
            this.descriptionLabel.TabIndex = 2;
            this.descriptionLabel.Text = "Port of macOS Mojave Dynamic Desktop feature to Windows 10";
            this.descriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // websiteLabel
            // 
            this.websiteLabel.LinkColor = System.Drawing.Color.Gainsboro;
            this.websiteLabel.Location = new System.Drawing.Point(13, 80);
            this.websiteLabel.Name = "websiteLabel";
            this.websiteLabel.Size = new System.Drawing.Size(428, 20);
            this.websiteLabel.TabIndex = 1;
            this.websiteLabel.TabStop = true;
            this.websiteLabel.Text = "https://github.com/t1m0thyj/WinDynamicDesktop";
            this.websiteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.websiteLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.websiteLabel_LinkClicked);
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(302, 112);
            this.closeButton.Name = "closeButton";
            this.closeButton.Padding = new System.Windows.Forms.Padding(5);
            this.closeButton.Size = new System.Drawing.Size(88, 26);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "Close";
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // creditsButton
            // 
            this.creditsButton.ButtonStyle = DarkUI.Controls.DarkButtonStyle.Toggle;
            this.creditsButton.Location = new System.Drawing.Point(9, 112);
            this.creditsButton.Name = "creditsButton";
            this.creditsButton.Padding = new System.Windows.Forms.Padding(5);
            this.creditsButton.Size = new System.Drawing.Size(75, 26);
            this.creditsButton.TabIndex = 2;
            this.creditsButton.Text = "Credits";
            this.creditsButton.Click += new System.EventHandler(this.creditsButton_Click);
            // 
            // donateButton
            // 
            this.donateButton.Location = new System.Drawing.Point(93, 112);
            this.donateButton.Name = "donateButton";
            this.donateButton.Padding = new System.Windows.Forms.Padding(5);
            this.donateButton.Size = new System.Drawing.Size(75, 26);
            this.donateButton.TabIndex = 3;
            this.donateButton.Text = "Donate";
            this.donateButton.Click += new System.EventHandler(this.donateButton_Click);
            // 
            // creditsBox
            // 
            this.creditsBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.creditsBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.creditsBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.creditsBox.Location = new System.Drawing.Point(5, 5);
            this.creditsBox.Multiline = true;
            this.creditsBox.Name = "creditsBox";
            this.creditsBox.ReadOnly = true;
            this.creditsBox.Size = new System.Drawing.Size(390, 97);
            this.creditsBox.TabIndex = 8;
            this.creditsBox.Visible = false;
            // 
            // AboutDialog
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 144);
            this.Controls.Add(this.creditsBox);
            this.Controls.Add(this.donateButton);
            this.Controls.Add(this.creditsButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.iconBox);
            this.Controls.Add(this.websiteLabel);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.copyrightLabel);
            this.Controls.Add(this.nameLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About WinDynamicDesktop";
            this.Load += new System.EventHandler(this.AboutDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.iconBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox iconBox;
        private DarkUI.Controls.DarkLabel nameLabel;
        private DarkUI.Controls.DarkLabel copyrightLabel;
        private DarkUI.Controls.DarkLabel descriptionLabel;
        private System.Windows.Forms.LinkLabel websiteLabel;
        private DarkUI.Controls.DarkButton closeButton;
        private DarkUI.Controls.DarkButton creditsButton;
        private DarkUI.Controls.DarkButton donateButton;
        private DarkUI.Controls.DarkTextBox creditsBox;
    }
}