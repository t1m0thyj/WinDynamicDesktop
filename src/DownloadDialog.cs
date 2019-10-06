﻿// This Source Code Form is subject to the terms of the Mozilla Public
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
    public partial class DownloadDialog : Form
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private string imagesZipDest;
        private List<Uri> themeUris;
        private int themeUriIndex;

        private Stopwatch stopwatch = new Stopwatch();
        private WebClient wc = new WebClient();

        public DownloadDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);

            this.Font = SystemFonts.MessageBoxFont;
            this.FormClosing += OnFormClosing;
            ThemeLoader.taskbarHandle = this.Handle;

            wc.DownloadProgressChanged += OnDownloadProgressChanged;
            wc.DownloadFileCompleted += OnDownloadFileCompleted;
        }

        public void InitDownload(ThemeConfig theme)
        {
            ThemeManager.downloadMode = true;
            this.Invoke(new Action(() =>
                label1.Text = string.Format(_("Downloading images for '{0}'..."),
                ThemeManager.GetThemeName(theme))));

            imagesZipDest = theme.themeId + "_images.zip";
            themeUris = CustomAppConfig.GetThemeUriList(theme.themeId).ToList();
            themeUriIndex = 0;
            DownloadNext(theme);
        }

        private void DownloadNext(ThemeConfig theme)
        {
            this.Invoke(new Action(() => UpdatePercentage(0)));
            stopwatch.Start();
            wc.DownloadFileAsync(themeUris[themeUriIndex], imagesZipDest, theme);
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
            return (File.Exists(imagesZipDest) &&
                ((new FileInfo(imagesZipDest)).Length > 1024 * 1024));
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UpdatePercentage(e.ProgressPercentage);

            var currentBytesReceived = e.BytesReceived / 1024d / stopwatch.Elapsed.TotalSeconds;

            if (currentBytesReceived < 1024d)
            {
                fileTransferSpeedLabel.Text = string.Format(_("{0} KB/s"),
                    (e.BytesReceived / 1024d / stopwatch.Elapsed.TotalSeconds).ToString("0.#"));
            }
            else
            {
                fileTransferSpeedLabel.Text = string.Format(_("{0} MB/s"),
                    (e.BytesReceived / 1024d / 1024d / stopwatch.Elapsed.TotalSeconds).ToString("0.#"));
            }

            fileSizeProgressLabel.Text = string.Format(_("{0} MB of {1} MB"),
                (e.BytesReceived / 1024d / 1024d).ToString("0.#"),
                (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.#"));
        }

        public async void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            stopwatch.Stop();
            ThemeConfig theme = (ThemeConfig)e.UserState;

            if ((e.Error == null) && EnsureZipNotHtml())
            {
                cancelButton.Enabled = false;
                ThemeResult result = await Task.Run(
                    () => ThemeLoader.ExtractTheme(imagesZipDest, theme.themeId));
                result.Match(ThemeLoader.HandleError, newTheme =>
                {
                    int themeIndex = ThemeManager.themeSettings.FindIndex(t => t.themeId == newTheme.themeId);
                    ThemeManager.themeSettings[themeIndex] = newTheme;
                });
                this.Close();
            }
            else if (themeUriIndex >= themeUris.Count)
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
                }
            }
            else
            {
                themeUriIndex++;
                DownloadNext(theme);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            wc.DownloadProgressChanged -= OnDownloadProgressChanged;
            wc.DownloadFileCompleted -= OnDownloadFileCompleted;
            wc.CancelAsync();
            this.Close();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            ThemeLoader.taskbarHandle = IntPtr.Zero;
            ThemeManager.downloadMode = false;
            wc?.Dispose();

            Task.Run(() =>
            {
                try
                {
                    File.Delete(imagesZipDest);
                }
                catch { }
            });
        }
    }
}
