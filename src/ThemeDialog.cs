using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;

namespace WinDynamicDesktop
{
    public partial class ThemeDialog : Form
    {
        public ThemeDialog()
        {
            InitializeComponent();

            this.FormClosing += OnFormClosing;
        }

        private void themesLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(themesLinkLabel.Text);
        }

        private async void importButton_Click(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                await Task.Run(() => ZipFile.ExtractToDirectory(fileDialog.FileName,
                    Path.Combine(Environment.CurrentDirectory, "images")));

                this.Close();
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Directory.Exists("images"))
            {
                DialogResult result = MessageBox.Show("This app cannot display wallpapers until you " +
                    "have downloaded images for it to use. Are you sure you want to cancel and quit the " +
                    "program?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    Environment.Exit(0);
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
