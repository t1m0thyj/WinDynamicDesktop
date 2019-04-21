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

        private Stopwatch stopwatch = new Stopwatch();
        private WebClient wc = new WebClient();

        public ProgressDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);

            this.Font = SystemFonts.MessageBoxFont;
            this.FormClosing += OnFormClosing;
            ThemeLoader.taskbarHandle = this.Handle;

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
                this.Invoke(new Action(() => UpdateDownloadStatus(theme)));

                if (theme.imagesZipUri.StartsWith("file://"))
                {
                    string themePath = (new Uri(theme.imagesZipUri)).LocalPath;

                    Task.Run(() => {
                        bool success = ThemeLoader.CopyLocalTheme(theme, themePath,
                            this.UpdateTotalPercentage);

                        if (!success)
                        {
                            this.Invoke(new Action(() => ThemeLoader.HandleError(theme.themeId)));
                        }

                        downloadQueue.Dequeue();
                        this.Invoke(new Action(() => DownloadNext()));
                    });
                }
                else if (!string.IsNullOrEmpty(theme.imagesZipUri))
                {
                    List<string> imagesZipUris = theme.imagesZipUri.Split('|').ToList();
                    wc.DownloadFileAsync(new Uri(imagesZipUris.First()),
                        theme.themeId + "_images.zip", imagesZipUris.Skip(1).ToList());
                }
            }
            else if (!ThemeManager.importMode)
            {
                this.Close();
            }
        }

        private void UpdateDownloadStatus(ThemeConfig theme)
        {
            label1.Text = string.Format(_("Downloading images for '{0}'..."),
                ThemeManager.GetThemeName(theme));
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
                this.Invoke(new Action(() => UpdateImportStatus(themePath)));
                ThemeConfig theme = ThemeManager.ImportTheme(themePath);
                this.Invoke(new Action(() => ThemeLoader.HandleError(
                    Path.GetFileNameWithoutExtension(themePath))));

                if (theme != null)
                {
                    if (Path.GetExtension(themePath) == ".json")
                    {
                        downloadQueue = new Queue<ThemeConfig>(
                            new List<ThemeConfig>() { theme });
                        DownloadNext();  // TODO Test this
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

        private void UpdateImportStatus(string themePath)
        {
            label1.Text = string.Format(_("Importing theme from {0}..."),
                Path.GetFileName(themePath));
        }

        private void UpdateTotalPercentage(int themePercentage)
        {
            int numRemaining = ThemeManager.importMode ? importQueue.Count : downloadQueue.Count;
            int percentage = ((numJobs - numRemaining) * 100 + themePercentage) / numJobs;

            stopwatch.Start();
            progressBar1.Invoke(new Action(() =>
            {
                progressBar1.Value = percentage;
                progressBar1.Refresh();
                TaskbarProgress.SetValue(this.Handle, percentage, 100);
            }));
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdateTotalPercentage(e.ProgressPercentage);
            
            fileTransferSpeedLabel.Text = string.Format(_("{0} MB/s"),
                (e.BytesReceived / 1024f / 1024f / stopwatch.Elapsed.TotalSeconds).ToString("0.#"));

            fileSizeProgressLabel.Text = string.Format(_("{0} MB of {1} MB"),
                (e.BytesReceived / 1024d / 1024d).ToString("0.#"),
                (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.#"));
        }

        public async void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            stopwatch.Stop();

            if (e.Error == null)
            {
                ThemeConfig theme = downloadQueue.Dequeue();

                await Task.Run(() => ThemeLoader.ExtractTheme(theme.themeId + "_images.zip",
                    theme.themeId, true));

                ThemeLoader.HandleError(theme.themeId);
                DownloadNext();
            }
            else
            {
                List<string> imagesZipUris = (List<string>)e.UserState;
                ThemeConfig theme = downloadQueue.Peek();

                if (imagesZipUris.Count == 0)
                {
                    bool shouldRetry = ThemeLoader.PromptDialog(string.Format(_("Failed to " +
                        "download images for the '{0}' theme. Do you want to try again?"),
                        theme.themeId));

                    if (shouldRetry)
                    {
                        imagesZipUris = theme.imagesZipUri.Split('|').ToList();
                    }
                    else
                    {
                        ThemeLoader.HandleError(theme.themeId, string.Format(
                            _("Failed to download images for the '{0}' theme"), theme.themeId));
                    }
                }

                if (imagesZipUris.Count > 0)
                {
                    wc.DownloadFileAsync(new Uri(imagesZipUris.First()),
                        theme.themeId + "_images.zip", imagesZipUris.Skip(1).ToList());
                }
                else
                {
                    downloadQueue.Dequeue();
                    DownloadNext();
                }
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            ThemeLoader.taskbarHandle = IntPtr.Zero;
            wc?.Dispose();
        }
    }
}
