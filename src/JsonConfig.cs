// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;
using PropertyChanged.SourceGenerator;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public partial class AppConfig : INotifyPropertyChanged
    {
        // Schedule settings
        [Notify] private int _locationMode { get; set; }
        [Notify] private string _location { get; set; }
        [Notify] private double? _latitude { get; set; }
        [Notify] private double? _longitude { get; set; }
        [Notify] private string _sunriseTime { get; set; }
        [Notify] private string _sunsetTime { get; set; }
        [Notify] private int _sunriseSunsetDuration { get; set; }

        // Theme settings
        [Notify] private string[] _activeThemes { get; set; }
        [Obsolete("Deprecated")]
        public bool darkMode { get; set; }
        [Notify] private int _appearanceMode { get; set; }
        [Obsolete("Deprecated")]
        public bool changeLockScreen { get; set; }
        [Notify] private int _lockScreenDisplayIndex { get; set; } = -1;
        [Notify] private string _lockScreenTheme { get; set; }
        [Obsolete("Deprecated")]
        public bool enableShuffle { get; set; }
        [Notify] private int _themeShuffleMode { get; set; } = (int)ShufflePeriod.EveryDay;
        [Obsolete("Deprecated")]
        public string lastShuffleDate { get; set; }
        [Notify] private string _lastShuffleTime { get; set; }
        [Notify] private string[] _shuffleHistory { get; set; }
        [Notify] private string[] _favoriteThemes { get; set; }
        [Notify] private bool _showInstalledOnly { get; set; }

        // General settings
        [Notify] private string _language { get; set; }
        [Notify] private bool _autoUpdateCheck { get; set; } = true;
        [Notify] private string _lastUpdateCheckTime { get; set; }
        [Notify] private bool _hideTrayIcon { get; set; }
        [Notify] private bool _fullScreenPause { get; set; }
        [Notify] private bool _enableScripts { get; set; }
        [Notify] private bool _debugLogging { get; set; }
    }

    class JsonConfig
    {
        private static System.Timers.Timer autoSaveTimer;
        private static bool restartPending = false;
        private static bool unsavedChanges;
        private static readonly object settingsFileLock = new object();

        public static AppConfig settings = new AppConfig();
        public static bool firstRun = !File.Exists("settings.json");

        public static void LoadConfig()
        {
            autoSaveTimer?.Stop();
            ConfigMigrator.RenameSettingsFile();
            string jsonText = null;

            if (!firstRun)
            {
                try
                {
                    jsonText = File.ReadAllText("settings.json");
                    ConfigMigrator.UpdateConfig(jsonText);
                    settings = JsonConvert.DeserializeObject<AppConfig>(jsonText);
                }
                catch (JsonReaderException)
                {
                    DialogResult result = MessageDialog.ShowQuestion("The WinDynamicDesktop configuration file is " +
                        "corrupt and could not be loaded. Do you want to reset to default settings?", "Error",
                        MessageBoxIcon.Error);
                    if (result == DialogResult.Yes)
                    {
                        firstRun = true;
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
            }

            unsavedChanges = ConfigMigrator.UpdateObsoleteSettings(jsonText);
            autoSaveTimer = new System.Timers.Timer(1000);
            autoSaveTimer.AutoReset = false;
            autoSaveTimer.Enabled = unsavedChanges;

            settings.PropertyChanged += OnSettingsPropertyChanged;
            autoSaveTimer.Elapsed += OnAutoSaveTimerElapsed;
        }

        public static void ReloadConfig()
        {
            restartPending = true;
            autoSaveTimer.Start();
        }

        public static void SaveConfig()
        {
            if (!unsavedChanges)
            {
                return;
            }

            unsavedChanges = autoSaveTimer.Enabled = false;
            string jsonText = JsonConvert.SerializeObject(settings, Formatting.Indented);
            lock (settingsFileLock)
            {
                File.WriteAllText("settings.json.tmp", jsonText);
                File.Move("settings.json.tmp", "settings.json", true);
            }
        }

        public static bool IsNullOrEmpty(Array array)
        {
            return (array == null || array.Length == 0);
        }

        private static void OnSettingsPropertyChanged(object sender, EventArgs e)
        {
            unsavedChanges = autoSaveTimer.Enabled = true;
        }

        private static void OnAutoSaveTimerElapsed(object sender, EventArgs e)
        {
            if (!restartPending && !unsavedChanges)
            {
                return;
            }

            SaveConfig();

            if (restartPending)
            {
                restartPending = false;
                System.Diagnostics.Process.Start(Application.ExecutablePath);
                Application.Exit();
            }
        }
    }
}
