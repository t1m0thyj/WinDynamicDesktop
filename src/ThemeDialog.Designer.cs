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
            components = new System.ComponentModel.Container();
            applyButton = new System.Windows.Forms.Button();
            closeButton = new System.Windows.Forms.Button();
            themeLinkLabel = new System.Windows.Forms.LinkLabel();
            importButton = new System.Windows.Forms.Button();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            favoriteThemeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteThemeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            previewerHost = new System.Windows.Forms.Integration.ElementHost();
            listView1 = new System.Windows.Forms.ListView();
            displayComboBox = new System.Windows.Forms.ComboBox();
            optionsButton = new System.Windows.Forms.Button();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // applyButton
            // 
            applyButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            applyButton.Location = new System.Drawing.Point(464, 488);
            applyButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            applyButton.Name = "applyButton";
            applyButton.Size = new System.Drawing.Size(88, 27);
            applyButton.TabIndex = 5;
            applyButton.Text = "Apply";
            applyButton.UseVisualStyleBackColor = true;
            applyButton.Click += applyButton_Click;
            // 
            // closeButton
            // 
            closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            closeButton.Location = new System.Drawing.Point(580, 488);
            closeButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(88, 27);
            closeButton.TabIndex = 6;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // themeLinkLabel
            // 
            themeLinkLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            themeLinkLabel.AutoSize = true;
            themeLinkLabel.Location = new System.Drawing.Point(158, 494);
            themeLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            themeLinkLabel.Name = "themeLinkLabel";
            themeLinkLabel.Size = new System.Drawing.Size(134, 15);
            themeLinkLabel.TabIndex = 3;
            themeLinkLabel.TabStop = true;
            themeLinkLabel.Text = "Get more themes online";
            themeLinkLabel.LinkClicked += themeLinkLabel_LinkClicked;
            // 
            // importButton
            // 
            importButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            importButton.AutoSize = true;
            importButton.Location = new System.Drawing.Point(14, 488);
            importButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            importButton.Name = "importButton";
            importButton.Size = new System.Drawing.Size(125, 27);
            importButton.TabIndex = 2;
            importButton.Text = "Import from file...";
            importButton.UseVisualStyleBackColor = true;
            importButton.Click += importButton_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.InitialDirectory = "shell:Downloads";
            openFileDialog1.Multiselect = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { favoriteThemeMenuItem, deleteThemeMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(181, 48);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // favoriteThemeMenuItem
            // 
            favoriteThemeMenuItem.Name = "favoriteThemeMenuItem";
            favoriteThemeMenuItem.Size = new System.Drawing.Size(180, 22);
            favoriteThemeMenuItem.Text = "toolStripMenuItem1";
            favoriteThemeMenuItem.Click += favoriteThemeMenuItem_Click;
            // 
            // deleteThemeMenuItem
            // 
            deleteThemeMenuItem.Name = "deleteThemeMenuItem";
            deleteThemeMenuItem.Size = new System.Drawing.Size(180, 22);
            deleteThemeMenuItem.Text = "toolStripMenuItem1";
            deleteThemeMenuItem.Click += deleteThemeMenuItem_Click;
            // 
            // previewerHost
            // 
            previewerHost.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            previewerHost.Location = new System.Drawing.Point(255, 12);
            previewerHost.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            previewerHost.Name = "previewerHost";
            previewerHost.Size = new System.Drawing.Size(648, 462);
            previewerHost.TabIndex = 1;
            // 
            // listView1
            // 
            listView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            listView1.Location = new System.Drawing.Point(12, 41);
            listView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new System.Drawing.Size(233, 431);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
            // 
            // displayComboBox
            // 
            displayComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            displayComboBox.FormattingEnabled = true;
            displayComboBox.Items.AddRange(new object[] { "All Displays" });
            displayComboBox.Location = new System.Drawing.Point(12, 12);
            displayComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            displayComboBox.Name = "displayComboBox";
            displayComboBox.Size = new System.Drawing.Size(203, 23);
            displayComboBox.TabIndex = 7;
            displayComboBox.SelectedIndexChanged += displayComboBox_SelectedIndexChanged;
            // 
            // optionsButton
            // 
            optionsButton.Location = new System.Drawing.Point(222, 11);
            optionsButton.Name = "optionsButton";
            optionsButton.Size = new System.Drawing.Size(23, 23);
            optionsButton.TabIndex = 9;
            optionsButton.Text = "…";
            optionsButton.UseVisualStyleBackColor = true;
            // 
            // ThemeDialog
            // 
            AcceptButton = applyButton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = closeButton;
            ClientSize = new System.Drawing.Size(915, 532);
            Controls.Add(optionsButton);
            Controls.Add(displayComboBox);
            Controls.Add(listView1);
            Controls.Add(importButton);
            Controls.Add(themeLinkLabel);
            Controls.Add(applyButton);
            Controls.Add(closeButton);
            Controls.Add(previewerHost);
            Icon = Properties.Resources.AppIcon;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(861, 398);
            Name = "ThemeDialog";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Select Theme";
            Load += ThemeDialog_Load;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.LinkLabel themeLinkLabel;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem favoriteThemeMenuItem;
        private System.Windows.Forms.Integration.ElementHost previewerHost;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ToolStripMenuItem deleteThemeMenuItem;
        private System.Windows.Forms.ComboBox displayComboBox;
        private System.Windows.Forms.Button optionsButton;
    }
}