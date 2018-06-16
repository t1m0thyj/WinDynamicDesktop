using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace WinDynamicDesktop
{
    public partial class ProgressDialog : Form
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        public void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        public void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ZipFile.ExtractToDirectory("images.zip", "images");
            File.Delete("images.zip");

            this.Close();
        }
    }
}
