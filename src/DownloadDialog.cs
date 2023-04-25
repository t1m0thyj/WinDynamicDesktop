// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using HttpProgress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinDynamicDesktop.COM;

namespace WinDynamicDesktop
{
    public partial class DownloadDialog : Form
    {
        public bool applyPending;

        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private ThemeConfig downloadTheme;
        private List<Uri> themeUris;
        private int themeUriIndex;
        private string themeZipDest;
        private double downloadTime;

        private HttpClient httpClient = new HttpClient();
        private System.Timers.Timer inactiveTimer;
        private Progress<ICopyProgress> progressReport;
        private CancellationTokenSource tokenSource;

        public DownloadDialog()
        {
            InitializeComponent();
            AcrylicUI.ThemeForm(this);
            Localization.TranslateForm(this);

            this.FormClosing += OnFormClosing;
            ThemeLoader.taskbarHandle = this.Handle;

            inactiveTimer = new System.Timers.Timer();
            inactiveTimer.AutoReset = false;
            inactiveTimer.Interval = 10000;
            inactiveTimer.Elapsed += (sender, e) => this.Invoke(new Action(() => OnDownloadFileCompleted(false)));
            progressReport = new Progress<ICopyProgress>(OnDownloadProgressChanged);
        }

        public void InitDownload(ThemeConfig theme)
        {
            ThemeManager.downloadMode = true;
            this.Invoke(new Action(() =>
                label1.Text = string.Format(_("Downloading images for '{0}'..."), ThemeManager.GetThemeName(theme))));

            themeUris = DefaultThemes.GetThemeUriList(theme.themeId).ToList();
            if (themeUris.Count > 1)
            {
                this.Invoke(new Action(() =>
                {
                    themeUriList.Items.Clear();
                    themeUriList.Items.AddRange(themeUris.Select(themeUri => themeUri.Host).ToArray());
                }));
            }
            else
            {
                themeUriList.Hide();
            }

            downloadTheme = theme;
            themeUriIndex = 0;
            themeZipDest = Path.Combine("themes", theme.themeId + ".zip");
            DownloadNext();
        }

        private void DownloadNext()
        {
            this.Invoke(new Action(() =>
            {
                UpdatePercentage(0);
                themeUriList.SelectedIndex = themeUriIndex;
            }));
            downloadTime = 0;
            inactiveTimer.Start();
            tokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                HttpResponseMessage response;
                using (var downloadStream = File.OpenWrite(themeZipDest))
                using (tokenSource.Token.Register(downloadStream.Close))
                {
                    response = await httpClient.GetAsync(themeUris[themeUriIndex].ToString(),
                        downloadStream, progressReport, tokenSource.Token);
                }
                this.Invoke(new Action(() => OnDownloadFileCompleted(response.IsSuccessStatusCode)));
            });
        }

        private void UpdatePercentage(int percentage)
        {
            progressBar1.Value = percentage;
            progressBar1.Refresh();
            TaskbarProgress.SetValue(this.Handle, percentage, 100);
        }

        private void OnDownloadProgressChanged(ICopyProgress e)
        {
            if (tokenSource.IsCancellationRequested || e.TransferTime.TotalMilliseconds < (downloadTime + 200))
            {
                return;
            }

            UpdatePercentage((int)(e.PercentComplete * 100));
            downloadTime = e.TransferTime.TotalMilliseconds;
            inactiveTimer.Interval = 10000;

            if (e.BytesPerSecond < 1024d * 1024d)
            {
                fileTransferSpeedLabel.Text = string.Format(_("{0} KB/s"), (e.BytesPerSecond / 1024d).ToString("0.#"));
            }
            else
            {
                fileTransferSpeedLabel.Text = string.Format(_("{0} MB/s"),
                    (e.BytesPerSecond / 1024d / 1024d).ToString("0.#"));
            }

            fileSizeProgressLabel.Text = string.Format(_("{0} MB of {1} MB"),
                (e.BytesTransferred / 1024d / 1024d).ToString("0.#"),
                (e.ExpectedBytes / 1024d / 1024d).ToString("0.#"));
        }

        private async void OnDownloadFileCompleted(bool success)
        {
            // Handle case where HTML page gets downloaded instead of ZIP
            if (success && File.Exists(themeZipDest) && new FileInfo(themeZipDest).Length > 1e6)
            {
                cancelButton.Enabled = false;
                ThemeResult result = await Task.Run(() =>
                    ThemeLoader.ExtractTheme(themeZipDest, downloadTheme.themeId));
                result.Match(ThemeLoader.HandleError, newTheme =>
                {
                    int themeIndex = ThemeManager.themeSettings.FindIndex(t => t.themeId == newTheme.themeId);
                    ThemeManager.themeSettings[themeIndex] = newTheme;
                });
                this.Close();
            }
            else
            {
                tokenSource.Cancel();
                themeUriIndex++;

                if (themeUriIndex >= themeUris.Count)
                {
                    bool shouldRetry = ThemeLoader.PromptDialog(string.Format(_("Failed to " +
                        "download images for the '{0}' theme. Do you want to try again?"),
                        downloadTheme.themeId));

                    if (shouldRetry)
                    {
                        InitDownload(downloadTheme);
                    }
                    else
                    {
                        ThemeLoader.HandleError(new FailedToDownloadImages(downloadTheme.themeId));
                        this.Close();
                    }
                }
                else
                {
                    DownloadNext();
                }
            }
        }

        private void themeUriList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tokenSource == null || tokenSource.IsCancellationRequested)
            {
                return;
            }
            tokenSource.Cancel();
            themeUriIndex = themeUriList.SelectedIndex;
            DownloadNext();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
            this.Close();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            ThemeLoader.taskbarHandle = IntPtr.Zero;
            ThemeManager.downloadMode = false;
            inactiveTimer?.Stop();
            httpClient?.Dispose();

            Task.Run(() =>
            {
                try
                {
                    System.Threading.Thread.Sleep(100);  // Wait for file to free up
                    File.Delete(themeZipDest);
                }
                catch { /* Do nothing */ }
            });
        }
    }
}
