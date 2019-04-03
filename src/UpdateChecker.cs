// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;
using RestSharp;

namespace WinDynamicDesktop
{
    public class GitHubApiData
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public string assets_url { get; set; }
        public string upload_url { get; set; }
        public string tarball_url { get; set; }
        public string zipball_url { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string tag_name { get; set; }
        public string target_commitish { get; set; }
        public string name { get; set; }
        public string body { get; set; }
        public bool draft { get; set; }
        public bool prerelease { get; set; }
        public DateTime created_at { get; set; }
        public DateTime published_at { get; set; }
    }

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
                menuItem = new ToolStripMenuItem(_("Check for &updates automatically once a week"),
                    null, OnAutoUpdateItemClick);
                menuItem.Checked = !JsonConfig.settings.disableAutoUpdate;

                return new List<ToolStripItem>() { menuItem };
            }
            else
            {
                return new List<ToolStripItem>();
            }
        }

        private static string GetLatestVersion()
        {
            var client = new RestClient("https://api.github.com");
            var request = new RestRequest("/repos/t1m0thyj/WinDynamicDesktop/releases/latest");

            var response = client.Execute<GitHubApiData>(request);
            if (!response.IsSuccessful)
            {
                return null;
            }

            return response.Data.tag_name.Substring(1);
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

        public static void CheckManual()
        {
            string currentVersion = GetCurrentVersion();
            string latestVersion = GetLatestVersion();

            if (latestVersion == null)
            {
                MessageBox.Show(_("WinDynamicDesktop could not connect to the Internet to check " +
                    "for updates."), _("Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (IsUpdateAvailable(currentVersion, latestVersion))
            {
                DialogResult result = MessageBox.Show(string.Format(_("There is a newer version " +
                    "of WinDynamicDesktop available. Do you want to download the update now?\n\n" +
                    "Current Version: {0}\nLatest Version: {1}"), currentVersion, latestVersion),
                    _("Update Available"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    UwpDesktop.GetHelper().OpenUpdateLink();
                }
            }
            else
            {
                MessageBox.Show(_("You already have the latest version of WinDynamicDesktop " +
                    "installed."), _("Up To Date"), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private static void CheckAuto()
        {
            string currentVersion = GetCurrentVersion();
            string latestVersion = GetLatestVersion();

            if (latestVersion == null)
            {
                return;
            }
            else if (IsUpdateAvailable(currentVersion, latestVersion))
            {
                AppContext.ShowPopup(string.Format(_("WinDynamicDesktop {0} is available. Click " +
                    "here to download it."), latestVersion), _("Update Available"));
            }

            JsonConfig.settings.lastUpdateCheck = DateTime.Now.ToString();
        }

        public static void TryCheckAuto(bool forceIfEnabled = false)
        {
            if (UwpDesktop.IsRunningAsUwp() || JsonConfig.settings.disableAutoUpdate)
            {
                return;
            }

            if (JsonConfig.settings.lastUpdateCheck != null && !forceIfEnabled)
            {
                DateTime lastUpdateCheck = DateTime.Parse(JsonConfig.settings.lastUpdateCheck);
                TimeSpan timeDiff = new TimeSpan(DateTime.Now.Ticks - lastUpdateCheck.Ticks);

                if (timeDiff.Days < 7)
                {
                    return;
                }
            }

            Task.Run(() => CheckAuto());
        }

        private static void OnAutoUpdateItemClick(object sender, EventArgs e)
        {
            bool isEnabled = JsonConfig.settings.disableAutoUpdate ^ true;
            JsonConfig.settings.disableAutoUpdate = isEnabled;
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
