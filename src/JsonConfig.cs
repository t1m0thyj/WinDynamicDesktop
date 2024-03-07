// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    [AddINotifyPropertyChangedInterface]
    public class AppConfig : INotifyPropertyChanged
    {
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        // Schedule settings
        public int locationMode { get; set; }
        public string location { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string sunriseTime { get; set; }
        public string sunsetTime { get; set; }
        public int sunriseSunsetDuration { get; set; }

        // Theme settings
        public string[] activeThemes { get; set; }
        [Obsolete("Will be removed in v6")]
        public bool darkMode { get; set; }
        public int appearanceMode { get; set; }
        [Obsolete("Will be removed in v6")]
        public bool changeLockScreen { get; set; }
        public int lockScreenDisplayIndex { get; set; } = -1;
        public string lockScreenTheme { get; set; }
        [Obsolete("Will be removed in v6")]
        public bool enableShuffle { get; set; }
        public int themeShuffleMode { get; set; } = (int)ShufflePeriod.EveryDay;
        [Obsolete("Will be removed in v6")]
        public string lastShuffleDate { get; set; }
        public string lastShuffleTime { get; set; }
        public string[] shuffleHistory { get; set; }
        public string[] favoriteThemes { get; set; }

        // General settings
        public string language { get; set; }
        public bool autoUpdateCheck { get; set; } = true;
        public string lastUpdateCheckTime { get; set; }
        public bool hideTrayIcon { get; set; }
        public bool fullScreenPause { get; set; }
        public bool enableScripts { get; set; }
        public bool debugLogging { get; set; }
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

            if (!firstRun)
            {
                try
                {
                    string jsonText = File.ReadAllText("settings.json");
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

            unsavedChanges = false;
            autoSaveTimer = new System.Timers.Timer(1000);
            autoSaveTimer.AutoReset = false;
            ConfigMigrator.UpdateObsoleteSettings();

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
