using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace WinDynamicDesktop
{
    public partial class ProgressDialog : Form
    {
        public int numDownloads = 0;
        public List<string> downloadedThemes = new List<string>();

        public ProgressDialog()
        {
            InitializeComponent();
        }

        public void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        public async void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            numDownloads--;
            downloadedThemes.Add(e.UserState.ToString());
            
            if (numDownloads == 0)
            {
                progressBar1.Style = ProgressBarStyle.Marquee;

                await Task.Run(() => ThemeManager.ExtractThemes(downloadedThemes));

                downloadedThemes.Clear();
                this.Close();
            }
        }
    }
}
