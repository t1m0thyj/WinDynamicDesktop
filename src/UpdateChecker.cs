// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class UpdateChecker
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static ToolStripMenuItem menuItem;

        public static void Initialize()
        {
            if (UwpDesktop.IsRunningAsUwp())
            {
                return;
            }

            AppContext.notifyIcon.BalloonTipClicked += OnBalloonTipClicked;

            TryCheckAuto();
        }

        public static List<ToolStripItem> GetMenuItems()
        {
            if (!UwpDesktop.IsRunningAsUwp())
            {
                menuItem = new ToolStripMenuItem(_("Check for &updates automatically once a week"), null,
                    OnAutoUpdateItemClick);
                menuItem.Checked = JsonConfig.settings.autoUpdateCheck != false;

                return new List<ToolStripItem>() {
                    new ToolStripSeparator(),
                    menuItem
                };
            }
            else
            {
                return new List<ToolStripItem>();
            }
        }

        private static async Task<string> GetLatestVersion()
        {
            var client = new RestClient("https://api.github.com");
            var request = new RestRequest("repos/t1m0thyj/WinDynamicDesktop/releases/latest");
            var response = await client.ExecuteAsync<GitHubApiData>(request);

            return response.IsSuccessful ? response.Data.tag_name.Substring(1) : null;
        }

        private static string GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private static bool IsUpdateAvailable(string currentVersion, string latestVersion)
        {
            Version current = new Version(currentVersion);
            Version latest = new Version(latestVersion);

            return (latest > current);
        }

        public static async Task CheckManual()
        {
            string currentVersion = GetCurrentVersion();
            string latestVersion = await GetLatestVersion();

            if (latestVersion == null)
            {
                MessageDialog.ShowWarning(_("WinDynamicDesktop could not connect to the Internet to check for " +
                    "updates."), _("Error"));
            }
            else if (IsUpdateAvailable(currentVersion, latestVersion))
            {
                DialogResult result = MessageDialog.ShowQuestion(string.Format(_("There is a newer version of " +
                    "WinDynamicDesktop available. Do you want to download the update now?\n\nCurrent Version: {0}\n" +
                    "Latest Version: {1}"), currentVersion, latestVersion), _("Update Available"));

                if (result == DialogResult.Yes)
                {
                    UwpDesktop.GetHelper().OpenUpdateLink();
                }
            }
            else
            {
                MessageDialog.ShowInfo(_("You already have the latest version of WinDynamicDesktop installed."),
                    _("Up To Date"));
            }
        }

        private static async Task CheckAuto()
        {
            string currentVersion = GetCurrentVersion();
            string latestVersion = await GetLatestVersion();

            if (latestVersion == null)
            {
                return;
            }
            else if (IsUpdateAvailable(currentVersion, latestVersion))
            {
                AppContext.ShowPopup(string.Format(_("WinDynamicDesktop {0} is available. Click here to download it."),
                    latestVersion), _("Update Available"));
            }

            JsonConfig.settings.lastUpdateCheckTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        public static void TryCheckAuto(bool forceIfEnabled = false)
        {
            if (UwpDesktop.IsRunningAsUwp() || JsonConfig.settings.autoUpdateCheck == false)
            {
                return;
            }

            if (JsonConfig.settings.lastUpdateCheckTime != null && !forceIfEnabled)
            {
                DateTime lastUpdateCheck = DateTime.Parse(JsonConfig.settings.lastUpdateCheckTime,
                    CultureInfo.InvariantCulture);
                TimeSpan timeDiff = new TimeSpan(DateTime.Now.Ticks - lastUpdateCheck.Ticks);

                if (timeDiff.Days < 7)
                {
                    return;
                }
            }

            Task.Run(async () => await CheckAuto());
        }

        private static void OnAutoUpdateItemClick(object sender, EventArgs e)
        {
            bool isEnabled = JsonConfig.settings.autoUpdateCheck ^ true;
            JsonConfig.settings.autoUpdateCheck = isEnabled;
            menuItem.Checked = isEnabled;

            TryCheckAuto(true);
        }

        private static void OnBalloonTipClicked(object sender, EventArgs e)
        {
            if (AppContext.notifyIcon.BalloonTipTitle == _("Update Available"))
            {
                UwpDesktop.GetHelper().OpenUpdateLink();
            }
        }
    }
}
