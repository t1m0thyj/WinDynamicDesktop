// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using HttpProgress;
using WinDynamicDesktop.COM;

namespace WinDynamicDesktop
{
    public partial class DownloadDialog : Form
    {
        public bool applyPending;

        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private List<Uri> themeUris;
        private int themeUriIndex;
        private string themeZipDest;
        private double downloadTime;

        private HttpClient httpClient = new HttpClient();
        private Progress<ICopyProgress> progress;

        public DownloadDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);

            this.FormClosing += OnFormClosing;
            ThemeLoader.taskbarHandle = this.Handle;

            progress = new Progress<ICopyProgress>(OnDownloadProgressChanged);
        }

        public void InitDownload(ThemeConfig theme)
        {
            ThemeManager.downloadMode = true;
            this.Invoke(new Action(() =>
                label1.Text = string.Format(_("Downloading images for '{0}'..."), ThemeManager.GetThemeName(theme))));

            themeUris = DefaultThemes.GetThemeUriList(theme.themeId).ToList();
            themeUriIndex = 0;
            themeZipDest = Path.Combine("themes", theme.themeId + ".zip");
            DownloadNext(theme);
        }

        private void DownloadNext(ThemeConfig theme)
        {
            this.Invoke(new Action(() => UpdatePercentage(0)));
            downloadTime = 0;
            Task.Run(async () =>
            {
                using (var downloadStream = File.OpenWrite(themeZipDest))
                {
                    var response = await httpClient.GetAsync(themeUris[themeUriIndex].ToString(),
                        downloadStream, progress);
                    this.Invoke(new Action(() => OnDownloadFileCompleted(response, theme)));
                }
            });
        }

        private void UpdatePercentage(int percentage)
        {
            progressBar1.Value = percentage;
            progressBar1.Refresh();
            TaskbarProgress.SetValue(this.Handle, percentage, 100);
        }

        private bool EnsureZipNotHtml()
        {
            // Handle case where HTML page gets downloaded instead of ZIP
            return (File.Exists(themeZipDest) && ((new FileInfo(themeZipDest)).Length > 1048576));
        }

        private void OnDownloadProgressChanged(ICopyProgress e)
        {
            if (e.TransferTime.TotalMilliseconds < (downloadTime + 200))
            {
                return;
            }

            UpdatePercentage((int)(e.PercentComplete * 100));
            downloadTime = e.TransferTime.TotalMilliseconds;

            if (e.BytesPerSecond < 1024d)
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

        public async void OnDownloadFileCompleted(HttpResponseMessage response, ThemeConfig theme)
        {
            if (response.IsSuccessStatusCode && EnsureZipNotHtml())
            {
                cancelButton.Enabled = false;
                ThemeResult result = await Task.Run(() => ThemeLoader.ExtractTheme(themeZipDest, theme.themeId));
                result.Match(ThemeLoader.HandleError, newTheme =>
                {
                    int themeIndex = ThemeManager.themeSettings.FindIndex(t => t.themeId == newTheme.themeId);
                    ThemeManager.themeSettings[themeIndex] = newTheme;
                });
                this.Close();
            }
            else
            {
                themeUriIndex++;

                if (themeUriIndex >= themeUris.Count)
                {
                    bool shouldRetry = ThemeLoader.PromptDialog(string.Format(_("Failed to " +
                        "download images for the '{0}' theme. Do you want to try again?"),
                        theme.themeId));

                    if (shouldRetry)
                    {
                        InitDownload(theme);
                    }
                    else
                    {
                        ThemeLoader.HandleError(new FailedToDownloadImages(theme.themeId));
                        this.Close();
                    }
                }
                else
                {
                    DownloadNext(theme);
                }
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            httpClient.CancelPendingRequests();
            this.Close();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            ThemeLoader.taskbarHandle = IntPtr.Zero;
            ThemeManager.downloadMode = false;
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
