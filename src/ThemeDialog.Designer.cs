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
            this.listView1 = new System.Windows.Forms.ListView();
            this.previewBox = new DarkUI.Controls.DarkSectionPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.firstButton = new DarkUI.Controls.DarkButton();
            this.previousButton = new DarkUI.Controls.DarkButton();
            this.imageNumberLabel = new DarkUI.Controls.DarkLabel();
            this.nextButton = new DarkUI.Controls.DarkButton();
            this.lastButton = new DarkUI.Controls.DarkButton();
            this.darkModeCheckbox = new DarkUI.Controls.DarkCheckBox();
            this.okButton = new DarkUI.Controls.DarkButton();
            this.cancelButton = new DarkUI.Controls.DarkButton();
            this.themeLinkLabel = new System.Windows.Forms.LinkLabel();
            this.importButton = new DarkUI.Controls.DarkButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.contextMenuStrip1 = new DarkUI.Controls.DarkContextMenu();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.creditsLabel = new DarkUI.Controls.DarkLabel();
            this.previewBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.listView1.ForeColor = System.Drawing.Color.Gainsboro;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(491, 300);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // previewBox
            // 
            this.previewBox.Controls.Add(this.pictureBox1);
            this.previewBox.Controls.Add(this.firstButton);
            this.previewBox.Controls.Add(this.previousButton);
            this.previewBox.Controls.Add(this.imageNumberLabel);
            this.previewBox.Controls.Add(this.nextButton);
            this.previewBox.Controls.Add(this.lastButton);
            this.previewBox.Location = new System.Drawing.Point(519, 12);
            this.previewBox.Name = "previewBox";
            this.previewBox.SectionHeader = "Preview";
            this.previewBox.Size = new System.Drawing.Size(394, 282);
            this.previewBox.TabIndex = 10;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(5, 29);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(384, 216);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // firstButton
            // 
            this.firstButton.Location = new System.Drawing.Point(10, 250);
            this.firstButton.Name = "firstButton";
            this.firstButton.Padding = new System.Windows.Forms.Padding(5);
            this.firstButton.Size = new System.Drawing.Size(50, 26);
            this.firstButton.TabIndex = 8;
            this.firstButton.Text = "<<";
            this.firstButton.Click += new System.EventHandler(this.firstButton_Click);
            // 
            // previousButton
            // 
            this.previousButton.Location = new System.Drawing.Point(75, 250);
            this.previousButton.Name = "previousButton";
            this.previousButton.Padding = new System.Windows.Forms.Padding(5);
            this.previousButton.Size = new System.Drawing.Size(50, 26);
            this.previousButton.TabIndex = 9;
            this.previousButton.Text = "<";
            this.previousButton.Click += new System.EventHandler(this.previousButton_Click);
            // 
            // imageNumberLabel
            // 
            this.imageNumberLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.imageNumberLabel.Location = new System.Drawing.Point(140, 255);
            this.imageNumberLabel.Name = "imageNumberLabel";
            this.imageNumberLabel.Size = new System.Drawing.Size(114, 15);
            this.imageNumberLabel.TabIndex = 10;
            this.imageNumberLabel.Text = "label1";
            this.imageNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(269, 250);
            this.nextButton.Name = "nextButton";
            this.nextButton.Padding = new System.Windows.Forms.Padding(5);
            this.nextButton.Size = new System.Drawing.Size(50, 26);
            this.nextButton.TabIndex = 11;
            this.nextButton.Text = ">";
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // lastButton
            // 
            this.lastButton.Location = new System.Drawing.Point(334, 250);
            this.lastButton.Name = "lastButton";
            this.lastButton.Padding = new System.Windows.Forms.Padding(5);
            this.lastButton.Size = new System.Drawing.Size(50, 26);
            this.lastButton.TabIndex = 12;
            this.lastButton.Text = ">>";
            this.lastButton.Click += new System.EventHandler(this.lastButton_Click);
            // 
            // darkModeCheckbox
            // 
            this.darkModeCheckbox.AutoSize = true;
            this.darkModeCheckbox.Location = new System.Drawing.Point(381, 328);
            this.darkModeCheckbox.Name = "darkModeCheckbox";
            this.darkModeCheckbox.Size = new System.Drawing.Size(122, 19);
            this.darkModeCheckbox.TabIndex = 9;
            this.darkModeCheckbox.Text = "Enable Dark Mode";
            this.darkModeCheckbox.CheckedChanged += new System.EventHandler(this.darkModeCheckbox_CheckedChanged);
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(610, 325);
            this.okButton.Name = "okButton";
            this.okButton.Padding = new System.Windows.Forms.Padding(5);
            this.okButton.Size = new System.Drawing.Size(88, 26);
            this.okButton.TabIndex = 7;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(733, 325);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Padding = new System.Windows.Forms.Padding(5);
            this.cancelButton.Size = new System.Drawing.Size(88, 26);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // themeLinkLabel
            // 
            this.themeLinkLabel.AutoSize = true;
            this.themeLinkLabel.LinkColor = System.Drawing.Color.Gainsboro;
            this.themeLinkLabel.Location = new System.Drawing.Point(159, 330);
            this.themeLinkLabel.Name = "themeLinkLabel";
            this.themeLinkLabel.Size = new System.Drawing.Size(134, 15);
            this.themeLinkLabel.TabIndex = 12;
            this.themeLinkLabel.TabStop = true;
            this.themeLinkLabel.Text = "Get more themes online";
            this.themeLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.themeLinkLabel_LinkClicked);
            // 
            // importButton
            // 
            this.importButton.Location = new System.Drawing.Point(17, 325);
            this.importButton.Name = "importButton";
            this.importButton.Padding = new System.Windows.Forms.Padding(5);
            this.importButton.Size = new System.Drawing.Size(125, 26);
            this.importButton.TabIndex = 13;
            this.importButton.Text = "Import from file...";
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Theme files|*.json;*.zip|All files|*.*";
            this.openFileDialog1.InitialDirectory = "shell:Downloads";
            this.openFileDialog1.Title = "Import Theme";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.contextMenuStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(118, 26);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // creditsLabel
            // 
            this.creditsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.creditsLabel.Location = new System.Drawing.Point(519, 299);
            this.creditsLabel.Name = "creditsLabel";
            this.creditsLabel.Size = new System.Drawing.Size(394, 13);
            this.creditsLabel.TabIndex = 14;
            this.creditsLabel.Text = "label2";
            this.creditsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ThemeDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(929, 366);
            this.Controls.Add(this.creditsLabel);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.themeLinkLabel);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.previewBox);
            this.Controls.Add(this.darkModeCheckbox);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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

        private System.Windows.Forms.ListView listView1;
        private DarkUI.Controls.DarkSectionPanel previewBox;
        private System.Windows.Forms.PictureBox pictureBox1;
        private DarkUI.Controls.DarkButton firstButton;
        private DarkUI.Controls.DarkButton previousButton;
        private DarkUI.Controls.DarkLabel imageNumberLabel;
        private DarkUI.Controls.DarkButton nextButton;
        private DarkUI.Controls.DarkButton lastButton;
        private DarkUI.Controls.DarkCheckBox darkModeCheckbox;
        private DarkUI.Controls.DarkButton okButton;
        private DarkUI.Controls.DarkButton cancelButton;
        private System.Windows.Forms.LinkLabel themeLinkLabel;
        private DarkUI.Controls.DarkButton importButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private DarkUI.Controls.DarkContextMenu contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private DarkUI.Controls.DarkLabel creditsLabel;
    }
}