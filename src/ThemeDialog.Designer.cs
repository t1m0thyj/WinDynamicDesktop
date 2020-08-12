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
            this.applyButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.themeLinkLabel = new System.Windows.Forms.LinkLabel();
            this.importButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.themeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.creditsLabel = new System.Windows.Forms.Label();
            this.imageListView1 = new Manina.Windows.Forms.ImageListView();
            this.previewLinkLabel = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.downloadLabel = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(586, 321);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 5;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(691, 321);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 6;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // themeLinkLabel
            // 
            this.themeLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.themeLinkLabel.AutoSize = true;
            this.themeLinkLabel.Location = new System.Drawing.Point(136, 331);
            this.themeLinkLabel.Name = "themeLinkLabel";
            this.themeLinkLabel.Size = new System.Drawing.Size(118, 13);
            this.themeLinkLabel.TabIndex = 3;
            this.themeLinkLabel.TabStop = true;
            this.themeLinkLabel.Text = "Get more themes online";
            this.themeLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.themeLinkLabel_LinkClicked);
            // 
            // importButton
            // 
            this.importButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.importButton.AutoSize = true;
            this.importButton.Location = new System.Drawing.Point(12, 326);
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
            this.themeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 26);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // themeToolStripMenuItem
            // 
            this.themeToolStripMenuItem.Name = "themeToolStripMenuItem";
            this.themeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.themeToolStripMenuItem.Text = "toolStripMenuItem1";
            this.themeToolStripMenuItem.Click += new System.EventHandler(this.themeToolStripMenuItem_Click);
            // 
            // creditsLabel
            // 
            this.creditsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.creditsLabel.Enabled = false;
            this.creditsLabel.Location = new System.Drawing.Point(543, 208);
            this.creditsLabel.Name = "creditsLabel";
            this.creditsLabel.Size = new System.Drawing.Size(256, 13);
            this.creditsLabel.TabIndex = 14;
            this.creditsLabel.Text = "label2";
            this.creditsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // imageListView1
            // 
            this.imageListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imageListView1.Location = new System.Drawing.Point(10, 10);
            this.imageListView1.MultiSelect = false;
            this.imageListView1.Name = "imageListView1";
            this.imageListView1.PersistentCacheDirectory = "";
            this.imageListView1.PersistentCacheSize = ((long)(100));
            this.imageListView1.Size = new System.Drawing.Size(504, 300);
            this.imageListView1.TabIndex = 15;
            this.imageListView1.UseWIC = true;
            this.imageListView1.SelectionChanged += new System.EventHandler(this.imageListView1_SelectionChanged);
            // 
            // previewLinkLabel
            // 
            this.previewLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.previewLinkLabel.AutoSize = true;
            this.previewLinkLabel.Location = new System.Drawing.Point(625, 237);
            this.previewLinkLabel.Name = "previewLinkLabel";
            this.previewLinkLabel.Size = new System.Drawing.Size(96, 13);
            this.previewLinkLabel.TabIndex = 17;
            this.previewLinkLabel.TabStop = true;
            this.previewLinkLabel.Text = "Preview in browser";
            this.previewLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.previewLinkLabel_LinkClicked);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(530, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(2, 300);
            this.label2.TabIndex = 18;
            this.label2.Text = "label2";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(547, 46);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(256, 144);
            this.pictureBox1.TabIndex = 19;
            this.pictureBox1.TabStop = false;
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameLabel.Location = new System.Drawing.Point(547, 15);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(256, 23);
            this.nameLabel.TabIndex = 20;
            this.nameLabel.Text = "Big Sur";
            this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // downloadLabel
            // 
            this.downloadLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.downloadLabel.AutoSize = true;
            this.downloadLabel.Location = new System.Drawing.Point(557, 237);
            this.downloadLabel.Name = "downloadLabel";
            this.downloadLabel.Size = new System.Drawing.Size(236, 13);
            this.downloadLabel.TabIndex = 21;
            this.downloadLabel.Text = "Theme will be downloaded when you click Apply";
            // 
            // ThemeDialog
            // 
            this.AcceptButton = this.applyButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(824, 361);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.previewLinkLabel);
            this.Controls.Add(this.creditsLabel);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.themeLinkLabel);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.imageListView1);
            this.Controls.Add(this.downloadLabel);
            this.Icon = global::WinDynamicDesktop.Properties.Resources.AppIcon;
            this.MinimumSize = new System.Drawing.Size(640, 400);
            this.Name = "ThemeDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Theme";
            this.Load += new System.EventHandler(this.ThemeDialog_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.LinkLabel themeLinkLabel;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem themeToolStripMenuItem;
        private System.Windows.Forms.Label creditsLabel;
        private Manina.Windows.Forms.ImageListView imageListView1;
        private System.Windows.Forms.LinkLabel previewLinkLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label downloadLabel;
    }
}