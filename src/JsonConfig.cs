// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Timers;
using Newtonsoft.Json;

namespace WinDynamicDesktop
{
    public class AppConfig : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string location { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public bool darkMode { get; set; }
        public bool hideTrayIcon { get; set; }
        public bool disableAutoUpdate { get; set; }
        public string lastUpdateCheck { get; set; }
        public bool changeSystemTheme { get; set; }
        public string themeName { get; set; }
        public bool useWindowsLocation { get; set; }
        public bool changeLockScreen { get; set; }
        public string language { get; set; }
    }

    public class ThemeConfig
    {
        public string themeId { get; set; }
        public string displayName { get; set; }
        public string imagesZipUri { get; set; }
        public string imageFilename { get; set; }
        public string imageCredits { get; set; }
        public int? dayHighlight { get; set; }
        public int? nightHighlight { get; set; }
        public int[] sunriseImageList { get; set; }
        public int[] dayImageList { get; set; }
        public int[] sunsetImageList { get; set; }
        public int[] nightImageList { get; set; }
    }

    class JsonConfig
    {
        private static Timer autoSaveTimer;
        private static bool restartPending = false;
        private static bool unsavedChanges;

        public static AppConfig settings = new AppConfig();
        public static bool firstRun = !File.Exists("settings.conf");

        public static void LoadConfig()
        {
            if (autoSaveTimer != null)
            {
                autoSaveTimer.Stop();
            }

            if (!firstRun)
            {
                string jsonText = File.ReadAllText("settings.conf");
                settings = JsonConvert.DeserializeObject<AppConfig>(jsonText);
            }

            unsavedChanges = false;
            autoSaveTimer = new Timer();
            autoSaveTimer.AutoReset = false;
            autoSaveTimer.Interval = 1000;

            settings.PropertyChanged += OnSettingsPropertyChanged;
            autoSaveTimer.Elapsed += OnAutoSaveTimerElapsed;
        }

        public static ThemeConfig LoadTheme(string name)
        {
            string themeJson;

            if (ThemeManager.defaultThemes.Contains(name))
            {
                themeJson = Encoding.UTF8.GetString((byte[])Properties.Resources.ResourceManager.
                    GetObject(name + "_json"));
            }
            else
            {
                themeJson = File.ReadAllText(Path.Combine("themes", name, "theme.json"));
            }

            ThemeConfig theme = JsonConvert.DeserializeObject<ThemeConfig>(themeJson);
            theme.themeId = name;

            return theme;
        }

        public static void EnablePendingRestart()
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
            if (!unsavedChanges && !restartPending)
            {
                return;
            }

            unsavedChanges = false;
            autoSaveTimer.Elapsed -= OnAutoSaveTimerElapsed;

            await Task.Run(() =>
            {
                string jsonText = JsonConvert.SerializeObject(settings);
                File.WriteAllText("settings.conf", jsonText);
            });

            if (restartPending)
            {
                restartPending = false;
                System.Windows.Forms.Application.Restart();
            }

            autoSaveTimer.Elapsed += OnAutoSaveTimerElapsed;
            autoSaveTimer.Start();
        }
    }
}
