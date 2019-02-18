using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Net;

namespace WinDynamicDesktop
{
    public partial class ProgressDialog : Form
    {
        private Queue<ThemeConfig> downloadQueue;
        private bool importMode;
        private Queue<string> importQueue;
        private int numJobs;

        private WebClient wc = new WebClient();

        public ProgressDialog()
        {
            InitializeComponent();

            this.Font = SystemFonts.MessageBoxFont;
            this.FormClosing += OnFormClosing;

            wc.DownloadProgressChanged += OnDownloadProgressChanged;
            wc.DownloadFileCompleted += OnDownloadFileCompleted;
        }

        public void InitDownload(List<ThemeConfig> themeList)
        {
            importMode = false;
            downloadQueue = new Queue<ThemeConfig>(themeList);
            numJobs = downloadQueue.Count;

            DownloadNext();
        }

        public void InitImport(List<string> themePaths)
        {
            label1.Text = "Importing themes, please wait...";
            importMode = true;
            importQueue = new Queue<string>(themePaths);
            numJobs = importQueue.Count;

            Task.Run(() => ImportNext());
        }

        private void DownloadNext()
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

                    if (!importMode)
                    {
                        DownloadNext();
                    }
                }
                else
                {
                    List<string> imagesZipUris = theme.imagesZipUri.Split('|').ToList();
                    wc.DownloadFileAsync(new Uri(imagesZipUris.First()),
                        theme.themeId + "_images.zip", imagesZipUris.Skip(1).ToList());
                }
            }
            else if (!importMode)
            {
                this.Close();
            }
        }

        private void ImportNext()
        {
            if (importQueue.Count > 0)
            {
                this.Invoke(new Action(() => UpdateTotalPercentage(0)));
                string themePath = importQueue.Peek();

                ThemeConfig theme = ThemeManager.ImportTheme(themePath);

                if (theme != null)
                {
                    if (Path.GetExtension(themePath) == ".json")
                    {
                        downloadQueue = new Queue<ThemeConfig>(new List<ThemeConfig>() { theme });
                        DownloadNext();
                    }

                    importQueue.Dequeue();
                    ThemeManager.importedThemes.Add(theme);
                    ImportNext();
                }
            }
            else
            {
                this.Invoke(new Action(() => this.Close()));
            }
        }

        private void UpdateTotalPercentage(int themePercentage)
        {
            int numRemaining = importMode ? importQueue.Count : downloadQueue.Count;
            progressBar1.Value = ((numJobs - numRemaining) * 100 + themePercentage) / numJobs;
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdateTotalPercentage(e.ProgressPercentage);
        }

        public void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            List<string> imagesZipUris = (List<string>)e.UserState;

            if (e.Error != null && imagesZipUris.Count > 0)
            {
                ThemeConfig theme = downloadQueue.Peek();
                wc.DownloadFileAsync(new Uri(imagesZipUris.First()),
                    theme.themeId + "_images.zip", imagesZipUris.Skip(1).ToList());
            }
            else
            {
                ThemeConfig theme = downloadQueue.Dequeue();

                if (e.Error == null)
                {
                    ThemeManager.ExtractTheme(theme.themeId + "_images.zip", theme.themeId, true);
                }

                DownloadNext();
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            wc?.Dispose();
        }
    }
}
