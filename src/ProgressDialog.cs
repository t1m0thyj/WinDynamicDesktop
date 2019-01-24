using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Net;

namespace WinDynamicDesktop
{
    public partial class ProgressDialog : Form
    {
        private WebClient client = new WebClient();
        private Queue<ThemeConfig> downloadQueue;
        private int numDownloads;

        public ProgressDialog()
        {
            InitializeComponent();

            this.Font = SystemFonts.MessageBoxFont;

            client.DownloadProgressChanged += OnDownloadProgressChanged;
            client.DownloadFileCompleted += OnDownloadFileCompleted;
        }

        public void LoadQueue(List<ThemeConfig> themeList)
        {
            downloadQueue = new Queue<ThemeConfig>(themeList);
            numDownloads = downloadQueue.Count;
        }

        public void DownloadNext()
        {
            if (downloadQueue.Count > 0)
            {
                ThemeConfig theme = downloadQueue.Peek();

                if (theme.imagesZipUri.StartsWith("file://"))
                {
                    string themePath = (new Uri(theme.imagesZipUri)).LocalPath;
                    ThemeManager.CopyLocalTheme(theme, themePath,
                        this.UpdateTotalPercentage);

                    downloadQueue.Dequeue();
                    DownloadNext();
                }
                else
                {
                    List<string> imagesZipUris = theme.imagesZipUri.Split('|').ToList();
                    client.DownloadFileAsync(new Uri(imagesZipUris.First()),
                        theme.themeId + "_images.zip", imagesZipUris.Skip(1).ToList());
                }
            }
            else
            {
                client?.Dispose();
                this.Close();
            }
        }

        private void UpdateTotalPercentage(int themePercentage)
        {
            progressBar1.Value = ((numDownloads - downloadQueue.Count) * 100 +
                themePercentage) / numDownloads;
        }

        public void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdateTotalPercentage(e.ProgressPercentage);
        }

        public async void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            List<string> imagesZipUris = (List<string>)e.UserState;

            if (e.Error != null && imagesZipUris.Count > 0)
            {
                ThemeConfig theme = downloadQueue.Peek();
                client.DownloadFileAsync(new Uri(imagesZipUris.First()),
                    theme.themeId + "_images.zip", imagesZipUris.Skip(1).ToList());
            }
            else
            {
                ThemeConfig theme = downloadQueue.Dequeue();

                if (e.Error == null)
                {
                    await Task.Run(() => ThemeManager.ExtractTheme(
                        theme.themeId + "_images.zip", theme.themeId, true));
                }

                DownloadNext();
            }
        }
    }
}
