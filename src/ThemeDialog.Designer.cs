namespace WinDynamicDesktop
{
    partial class ThemeDialog
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
            this.previewBox = new System.Windows.Forms.GroupBox();
            this.downloadLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.firstButton = new System.Windows.Forms.Button();
            this.previousButton = new System.Windows.Forms.Button();
            this.imageNumberLabel = new System.Windows.Forms.Label();
            this.nextButton = new System.Windows.Forms.Button();
            this.lastButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.themeLinkLabel = new System.Windows.Forms.LinkLabel();
            this.importButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.creditsLabel = new System.Windows.Forms.Label();
            this.imageListView1 = new Manina.Windows.Forms.ImageListView();
            this.previewBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // previewBox
            // 
            this.previewBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.previewBox.Controls.Add(this.downloadLabel);
            this.previewBox.Controls.Add(this.pictureBox1);
            this.previewBox.Controls.Add(this.firstButton);
            this.previewBox.Controls.Add(this.previousButton);
            this.previewBox.Controls.Add(this.imageNumberLabel);
            this.previewBox.Controls.Add(this.nextButton);
            this.previewBox.Controls.Add(this.lastButton);
            this.previewBox.Location = new System.Drawing.Point(441, 10);
            this.previewBox.Name = "previewBox";
            this.previewBox.Size = new System.Drawing.Size(346, 248);
            this.previewBox.TabIndex = 1;
            this.previewBox.TabStop = false;
            this.previewBox.Text = "Preview";
            // 
            // downloadLabel
            // 
            this.downloadLabel.ForeColor = System.Drawing.Color.DarkGreen;
            this.downloadLabel.Location = new System.Drawing.Point(9, 223);
            this.downloadLabel.Name = "downloadLabel";
            this.downloadLabel.Size = new System.Drawing.Size(329, 13);
            this.downloadLabel.TabIndex = 16;
            this.downloadLabel.Text = "Theme will be downloaded when you click Apply";
            this.downloadLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.downloadLabel.Visible = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(9, 18);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(329, 187);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // firstButton
            // 
            this.firstButton.Location = new System.Drawing.Point(13, 213);
            this.firstButton.Name = "firstButton";
            this.firstButton.Size = new System.Drawing.Size(43, 23);
            this.firstButton.TabIndex = 0;
            this.firstButton.Text = "<<";
            this.firstButton.UseVisualStyleBackColor = true;
            this.firstButton.Click += new System.EventHandler(this.firstButton_Click);
            // 
            // previousButton
            // 
            this.previousButton.Location = new System.Drawing.Point(69, 213);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(43, 23);
            this.previousButton.TabIndex = 1;
            this.previousButton.Text = "<";
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.previousButton_Click);
            // 
            // imageNumberLabel
            // 
            this.imageNumberLabel.Location = new System.Drawing.Point(112, 217);
            this.imageNumberLabel.Name = "imageNumberLabel";
            this.imageNumberLabel.Size = new System.Drawing.Size(123, 13);
            this.imageNumberLabel.TabIndex = 10;
            this.imageNumberLabel.Text = "label1";
            this.imageNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(235, 213);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(43, 23);
            this.nextButton.TabIndex = 2;
            this.nextButton.Text = ">";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // lastButton
            // 
            this.lastButton.Location = new System.Drawing.Point(291, 213);
            this.lastButton.Name = "lastButton";
            this.lastButton.Size = new System.Drawing.Size(43, 23);
            this.lastButton.TabIndex = 3;
            this.lastButton.Text = ">>";
            this.lastButton.UseVisualStyleBackColor = true;
            this.lastButton.Click += new System.EventHandler(this.lastButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(523, 296);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 5;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(628, 296);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 6;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // themeLinkLabel
            // 
            this.themeLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.themeLinkLabel.AutoSize = true;
            this.themeLinkLabel.Location = new System.Drawing.Point(314, 300);
            this.themeLinkLabel.Name = "themeLinkLabel";
            this.themeLinkLabel.Size = new System.Drawing.Size(118, 13);
            this.themeLinkLabel.TabIndex = 3;
            this.themeLinkLabel.TabStop = true;
            this.themeLinkLabel.Text = "Get more themes online";
            this.themeLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.themeLinkLabel_LinkClicked);
            // 
            // importButton
            // 
            this.importButton.AutoSize = true;
            this.importButton.Location = new System.Drawing.Point(15, 296);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(107, 23);
            this.importButton.TabIndex = 2;
            this.importButton.Text = "Import from file...";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Theme files|*.ddw;*.zip;*.json|All files|*.*";
            this.openFileDialog1.InitialDirectory = "shell:Downloads";
            this.openFileDialog1.Multiselect = true;
            this.openFileDialog1.Title = "Import Theme";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(118, 26);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // creditsLabel
            // 
            this.creditsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.creditsLabel.Enabled = false;
            this.creditsLabel.Location = new System.Drawing.Point(441, 269);
            this.creditsLabel.Name = "creditsLabel";
            this.creditsLabel.Size = new System.Drawing.Size(346, 13);
            this.creditsLabel.TabIndex = 14;
            this.creditsLabel.Text = "label2";
            this.creditsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // imageListView1
            // 
            this.imageListView1.Location = new System.Drawing.Point(10, 10);
            this.imageListView1.MultiSelect = false;
            this.imageListView1.Name = "imageListView1";
            this.imageListView1.PersistentCacheDirectory = "";
            this.imageListView1.PersistentCacheSize = ((long)(100));
            this.imageListView1.Size = new System.Drawing.Size(422, 275);
            this.imageListView1.TabIndex = 15;
            this.imageListView1.UseWIC = true;
            this.imageListView1.SelectionChanged += new System.EventHandler(this.imageListView1_SelectionChanged);
            // 
            // ThemeDialog
            // 
            this.AcceptButton = this.applyButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(796, 332);
            this.Controls.Add(this.imageListView1);
            this.Controls.Add(this.creditsLabel);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.themeLinkLabel);
            this.Controls.Add(this.previewBox);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::WinDynamicDesktop.Properties.Resources.AppIcon;
            this.MaximizeBox = false;
            this.Name = "ThemeDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Theme";
            this.Load += new System.EventHandler(this.ThemeDialog_Load);
            this.previewBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox previewBox;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button firstButton;
        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.Label imageNumberLabel;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button lastButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.LinkLabel themeLinkLabel;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.Label creditsLabel;
        private Manina.Windows.Forms.ImageListView imageListView1;
        private System.Windows.Forms.Label downloadLabel;
    }
}