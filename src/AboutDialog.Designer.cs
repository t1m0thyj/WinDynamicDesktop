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
            this.nameLabel = new System.Windows.Forms.Label();
            this.copyrightLabel = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.websiteLabel = new System.Windows.Forms.LinkLabel();
            this.closeButton = new System.Windows.Forms.Button();
            this.creditsButton = new System.Windows.Forms.CheckBox();
            this.donateButton = new System.Windows.Forms.Button();
            this.creditsBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
            this.SuspendLayout();
            // 
            // iconBox
            // 
            this.iconBox.Location = new System.Drawing.Point(4, 4);
            this.iconBox.Name = "iconBox";
            this.iconBox.Size = new System.Drawing.Size(41, 42);
            this.iconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconBox.TabIndex = 0;
            this.iconBox.TabStop = false;
            // 
            // nameLabel
            // 
            this.nameLabel.Location = new System.Drawing.Point(10, 5);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(367, 25);
            this.nameLabel.TabIndex = 1;
            this.nameLabel.Text = "WinDynamicDesktop";
            this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // copyrightLabel
            // 
            this.copyrightLabel.Location = new System.Drawing.Point(10, 33);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.Size = new System.Drawing.Size(367, 17);
            this.copyrightLabel.TabIndex = 7;
            this.copyrightLabel.Text = "Copyright © 2019 Timothy Johnson";
            this.copyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Location = new System.Drawing.Point(9, 51);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(370, 17);
            this.descriptionLabel.TabIndex = 2;
            this.descriptionLabel.Text = "Port of macOS Mojave Dynamic Desktop feature to Windows 10";
            this.descriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // websiteLabel
            // 
            this.websiteLabel.Location = new System.Drawing.Point(11, 69);
            this.websiteLabel.Name = "websiteLabel";
            this.websiteLabel.Size = new System.Drawing.Size(367, 17);
            this.websiteLabel.TabIndex = 1;
            this.websiteLabel.TabStop = true;
            this.websiteLabel.Text = "https://github.com/t1m0thyj/WinDynamicDesktop";
            this.websiteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.websiteLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.websiteLabel_LinkClicked);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(259, 97);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // creditsButton
            // 
            this.creditsButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.creditsButton.Location = new System.Drawing.Point(8, 97);
            this.creditsButton.Name = "creditsButton";
            this.creditsButton.Size = new System.Drawing.Size(64, 23);
            this.creditsButton.TabIndex = 2;
            this.creditsButton.Text = "Credits";
            this.creditsButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.creditsButton.UseVisualStyleBackColor = true;
            this.creditsButton.CheckedChanged += new System.EventHandler(this.creditsButton_CheckedChanged);
            // 
            // donateButton
            // 
            this.donateButton.AutoSize = true;
            this.donateButton.Location = new System.Drawing.Point(80, 97);
            this.donateButton.Name = "donateButton";
            this.donateButton.Size = new System.Drawing.Size(64, 23);
            this.donateButton.TabIndex = 3;
            this.donateButton.Text = "Donate";
            this.donateButton.UseVisualStyleBackColor = true;
            this.donateButton.Click += new System.EventHandler(this.donateButton_Click);
            // 
            // creditsBox
            // 
            this.creditsBox.Location = new System.Drawing.Point(4, 4);
            this.creditsBox.Multiline = true;
            this.creditsBox.Name = "creditsBox";
            this.creditsBox.ReadOnly = true;
            this.creditsBox.Size = new System.Drawing.Size(335, 85);
            this.creditsBox.TabIndex = 8;
            this.creditsBox.Visible = false;
            // 
            // AboutDialog
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 125);
            this.Controls.Add(this.creditsBox);
            this.Controls.Add(this.donateButton);
            this.Controls.Add(this.creditsButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.iconBox);
            this.Controls.Add(this.websiteLabel);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.copyrightLabel);
            this.Controls.Add(this.nameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
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
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label copyrightLabel;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.LinkLabel websiteLabel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.CheckBox creditsButton;
        private System.Windows.Forms.Button donateButton;
        private System.Windows.Forms.TextBox creditsBox;
    }
}