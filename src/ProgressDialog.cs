// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

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
using System.Diagnostics;

namespace WinDynamicDesktop
{
    public partial class ProgressDialog : Form
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private Queue<ThemeConfig> downloadQueue;
        private Queue<string> importQueue;
        private int numJobs;
        private IntPtr taskbarHandle;

        private WebClient wc = new WebClient();
        private Stopwatch stopwatch = new Stopwatch();

        public bool isUriEmpty = false;

        public ProgressDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);

            this.Font = SystemFonts.MessageBoxFont;
            this.FormClosing += OnFormClosing;
            taskbarHandle = this.Handle;

            wc.DownloadProgressChanged += OnDownloadProgressChanged;
            wc.DownloadFileCompleted += OnDownloadFileCompleted;
        }

        public void InitDownload(List<ThemeConfig> themeList)
        {
            downloadQueue = new Queue<ThemeConfig>(themeList);
            numJobs = downloadQueue.Count;

            DownloadNext();
        }

        public void InitImport(List<string> themePaths)
        {
            ThemeManager.importMode = true;
            label1.Text = _("Importing themes, please wait...");
            importQueue = new Queue<string>(themePaths);
            numJobs = importQueue.Count;

            Task.Run(() => ImportNext());

            // Hide download progress labels when importing directly from file system
            fileSizeProgressLabel.Visible = false;
            fileTransferSpeedLabel.Visible = false;
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

                    if (!ThemeManager.importMode)
                    {
                        DownloadNext();
                    }
                }
                else
                {
                    List<string> imagesZipUris = theme.imagesZipUri.Split('|').ToList();

                    try
                    {
                        wc.DownloadFileAsync(new Uri(imagesZipUris.First()),
                        theme.themeId + "_images.zip", imagesZipUris.Skip(1).ToList());
                    }
                    catch (Exception e) {
                        isUriEmpty = true;
                    }

                }
            }
            else if (!ThemeManager.importMode)
            {
                this.Close();
            }
        }

        private void ImportNext()
        {
            if (ThemeManager.importPaths.Count > 0)
            {
                foreach (string themePath in ThemeManager.importPaths)
                {
                    importQueue.Enqueue(themePath);
                }

                numJobs += ThemeManager.importPaths.Count;
                ThemeManager.importPaths.Clear();
            }

            this.Invoke(new Action(() => UpdateTotalPercentage(0)));

            if (importQueue.Count > 0)
            {
                string themePath = importQueue.Peek();

                ThemeConfig theme = ThemeManager.ImportTheme(themePath, taskbarHandle);

                if (theme != null)
                {
                    if (Path.GetExtension(themePath) == ".json")
                    {
                        downloadQueue = new Queue<ThemeConfig>(new List<ThemeConfig>() { theme });
                        DownloadNext();
                    }

                    ThemeManager.importedThemes.Add(theme);
                }

                importQueue.Dequeue();
                ImportNext();
            }
            else
            {
                ThemeManager.importMode = false;
                this.Invoke(new Action(() => this.Close()));
            }
        }

        private void UpdateTotalPercentage(int themePercentage)
        {
            int numRemaining = ThemeManager.importMode ? importQueue.Count : downloadQueue.Count;
            int percentage = ((numJobs - numRemaining) * 100 + themePercentage) / numJobs;

            stopwatch.Start();
            this.progressBar1.BeginInvoke((MethodInvoker)delegate ()
            {
                progressBar1.Value = percentage;
                progressBar1.Refresh();
                TaskbarProgress.SetValue(this.Handle, percentage, 100);
            }); ;
            //progressBar1.Value = percentage;


        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdateTotalPercentage(e.ProgressPercentage);
            
            fileTransferSpeedLabel.Text = string.Format("{0} MB/s", 
                (e.BytesReceived / 1024f / 1024f / stopwatch.Elapsed.TotalSeconds).ToString("0.#"));

            fileSizeProgressLabel.Text = string.Format("{0} MB of {1} MB",
                (e.BytesReceived / 1024d / 1024d).ToString("0.#"),
                (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.#"));
        }

        public void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            stopwatch.Stop();
            
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
