// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace WinDynamicDesktop
{
    [AddINotifyPropertyChangedInterface]
    public class AppConfig : INotifyPropertyChanged
    {
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        public string location { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public bool darkMode { get; set; }
        public bool hideTrayIcon { get; set; }
        public bool disableAutoUpdate { get; set; }
        public string lastUpdateCheck { get; set; }
        public string themeName { get; set; }
        public bool useWindowsLocation { get; set; }
        public string language { get; set; }
        public bool usePoeditorLanguage { get; set; }
        public bool enableShuffle { get; set; }
        public string lastShuffleDate { get; set; }
        public string[] shuffleHistory { get; set; }
        public bool dontUseLocation { get; set; }
        public string sunriseTime { get; set; }
        public string sunsetTime { get; set; }
        public int sunriseSunsetDuration { get; set; }
        public bool fullScreenPause { get; set; }
        public bool enableScripts { get; set; }
        public string[] favoriteThemes { get; set; }
    }

    class JsonConfig
    {
        private static Timer autoSaveTimer;
        private static bool restartPending = false;
        private static bool unsavedChanges;

        public static AppConfig settings = new AppConfig();
        public static bool firstRun = !File.Exists("settings.json");

        public static void LoadConfig()
        {
            ConfigMigrator.UpdateToVersion4();

            if (autoSaveTimer != null)
            {
                autoSaveTimer.Stop();
            }

            if (!firstRun)
            {
                try
                {
                    string jsonText = File.ReadAllText("settings.json");
                    settings = JsonConvert.DeserializeObject<AppConfig>(jsonText);
                }
                catch (JsonReaderException)
                {
                    MessageDialog.ShowWarning("Your WinDynamicDesktop configuration file was corrupt and has been " +
                        "reset to the default settings.", "Warning");
                    firstRun = true;
                }
            }

            unsavedChanges = false;
            autoSaveTimer = new Timer();
            autoSaveTimer.AutoReset = false;
            autoSaveTimer.Interval = 1000;

            settings.PropertyChanged += OnSettingsPropertyChanged;
            autoSaveTimer.Elapsed += OnAutoSaveTimerElapsed;
        }

        public static void ReloadConfig()
        {
            restartPending = true;
            autoSaveTimer.Start();
        }

        private static void OnSettingsPropertyChanged(object sender, EventArgs e)
        {
            unsavedChanges = true;
            autoSaveTimer.Start();
        }

        private static async void OnAutoSaveTimerElapsed(object sender, EventArgs e)
        {
            if (!restartPending && !unsavedChanges)
            {
                return;
            }

            if (unsavedChanges)
            {
                unsavedChanges = false;
                autoSaveTimer.Elapsed -= OnAutoSaveTimerElapsed;

                await Task.Run(() =>
                {
                    string jsonText = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText("settings.json", jsonText);
                });
            }

            if (restartPending)
            {
                restartPending = false;
                System.Windows.Forms.Application.Restart();
            }
            else
            {
                autoSaveTimer.Elapsed += OnAutoSaveTimerElapsed;
                autoSaveTimer.Start();
            }
        }
    }
}
