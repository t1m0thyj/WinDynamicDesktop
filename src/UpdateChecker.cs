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
        private static string updateLink = "https://github.com/t1m0thyj/WinDynamicDesktop/releases";
        
        private static MenuItem menuItem;

        public static void Initialize()
        {
            if (UwpDesktop.IsRunningAsUwp())
            {
                return;
            }

            AppContext.notifyIcon.BalloonTipClicked += OnBalloonTipClicked;

            TryCheckAuto(true);
        }

        public static List<MenuItem> GetMenuItems()
        {
            if (!UwpDesktop.IsRunningAsUwp())
            {
                menuItem = new MenuItem("Check for &updates automatically once a week",
                    OnAutoUpdateItemClick);
                menuItem.Checked = !JsonConfig.settings.disableAutoUpdate;

                return new List<MenuItem>() { menuItem };
            }
            else
            {
                return new List<MenuItem>();
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
                MessageBox.Show("WinDynamicDesktop could not connect to the Internet to check " +
                    "for updates.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (IsUpdateAvailable(currentVersion, latestVersion))
            {
                DialogResult result = MessageBox.Show("There is a newer version of " +
                    "WinDynamicDesktop available. Would you like to visit the download page?\n\n" +
                    "Current Version: " + currentVersion + "\nLatest Version: " + latestVersion,
                    "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(updateLink);
                }
            }
            else
            {
                MessageBox.Show("You already have the latest version of WinDynamicDesktop " +
                    "installed.", "Up To Date", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                NotifyIcon _notifyIcon = AppContext.notifyIcon;
                _notifyIcon.BalloonTipTitle = "Update Available";
                _notifyIcon.BalloonTipText = "WinDynamicDesktop " + latestVersion +
                    " is available. Click here to download it.";
                _notifyIcon.ShowBalloonTip(10000);
            }

            JsonConfig.settings.lastUpdateCheck = DateTime.Now.ToString();
            JsonConfig.SaveConfig();
        }

        public static async void TryCheckAuto(bool forceIfEnabled = false)
        {
            if (UwpDesktop.IsRunningAsUwp() || JsonConfig.settings.disableAutoUpdate)
            {
                return;
            }

            if (JsonConfig.settings.lastUpdateCheck != null && !forceIfEnabled)
            {
                DateTime lastUpdateCheck = DateTime.Parse(JsonConfig.settings.lastUpdateCheck);
                int dayDiff = (new TimeSpan(DateTime.Now.Ticks - lastUpdateCheck.Ticks)).Days;

                if (dayDiff < 7)
                {
                    return;
                }
            }

            await Task.Run(() => CheckAuto());
        }

        private static void ToggleAutoUpdate()
        {
            JsonConfig.settings.disableAutoUpdate ^= true;
            menuItem.Checked = !JsonConfig.settings.disableAutoUpdate;

            TryCheckAuto(true);
            JsonConfig.SaveConfig();
        }

        private static void OnAutoUpdateItemClick(object sender, EventArgs e)
        {
            ToggleAutoUpdate();
        }

        private static void OnBalloonTipClicked(object sender, EventArgs e)
        {
            if (AppContext.notifyIcon.BalloonTipTitle == "Update Available")
            {
                System.Diagnostics.Process.Start(updateLink);
            }
        }
    }
}
