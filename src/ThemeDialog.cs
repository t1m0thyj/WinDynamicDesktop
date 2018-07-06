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
    }
}
