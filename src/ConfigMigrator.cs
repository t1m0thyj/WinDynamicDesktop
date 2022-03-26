// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;

namespace WinDynamicDesktop
{
    public class OldAppConfigV3
    {
        public bool changeSystemTheme { get; set; }
        public bool changeAppTheme { get; set; }
        public bool changeLockScreen { get; set; }
        public bool useAutoBrightness { get; set; }
        public bool useCustomAutoBrightness { get; set; }
    }

    public class OldAppConfigV4
    {
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

    class ConfigMigrator
    {
        public static void Run()
        {
            UpdateToVersion4();
            UpdateToVersion5();
        }

        private static DateTime SafeParse(string dateTime)  // Added 2020-05-21
        {
            try
            {
                return DateTime.Parse(dateTime, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return DateTime.Parse(dateTime);
            }
        }

        private static void UpdateToVersion4()  // Added 2020-01-01
        {
            if (!File.Exists("settings.json"))
            {
                // Updated 2020-10-25 for settings.conf -> settings.json
                if (File.Exists("settings.conf"))
                {
                    File.Move("settings.conf", "settings.json");
                    JsonConfig.firstRun = false;
                }
                else
                {
                    return;
                }
            }
            string jsonText = File.ReadAllText("settings.json");
            OldAppConfigV3 settings = JsonConvert.DeserializeObject<OldAppConfigV3>(jsonText);
            bool legacySettingsEnabled = (settings.changeSystemTheme || settings.changeAppTheme ||
                settings.useAutoBrightness || settings.useCustomAutoBrightness);
            if (legacySettingsEnabled)
            {
                jsonText = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<AppConfig>(jsonText),
                    Formatting.Indented);
                File.WriteAllText("settings.json", jsonText);
                MessageDialog.ShowInfo("Updated to WinDynamicDesktop 4.0 successfully. Some features you were using " +
                    "have been disabled because they were removed from the core app. You were using one or more of " +
                    "the following features:\n\n* Change Windows 10 app/system theme\n* Change screen brightness\n\n" +
                    "To re-enable these features, install scripts for them from here: https://windd.info/scripts/");
            }
        }

        private static void UpdateToVersion5()  // Added 2021-11-30
        {
            if (!File.Exists("settings.json"))
            {
                return;
            }
            string jsonText = File.ReadAllText("settings.json");
            if (jsonText.IndexOf("\"themeName\"") == -1)
            {
                return;
            }

            OldAppConfigV4 oldSettings = null;
            try
            {
                oldSettings = JsonConvert.DeserializeObject<OldAppConfigV4>(jsonText);
            }
            catch { /* Do nothing */ }

            int locationMode = 0;
            double? newLatitude = null;
            double? newLongitude = null;
            if (oldSettings.useWindowsLocation)
            {
                locationMode = 1;
            }
            else if (oldSettings.dontUseLocation)
            {
                locationMode = -1;
            }
            if (oldSettings.latitude != null)
            {
                newLatitude = double.Parse(oldSettings.latitude, CultureInfo.InvariantCulture);
            }
            if (oldSettings.longitude != null)
            {
                newLongitude = double.Parse(oldSettings.longitude, CultureInfo.InvariantCulture);
            }

            AppConfig newSettings = new AppConfig
            {
                locationMode = locationMode,
                location = oldSettings.location,
                latitude = newLatitude,
                longitude = newLongitude,
                sunriseTime = oldSettings.sunriseTime,
                sunsetTime = oldSettings.sunsetTime,
                sunriseSunsetDuration = oldSettings.sunriseSunsetDuration,
                activeThemes = new string[] { oldSettings.themeName },
                darkMode = oldSettings.darkMode,
                enableShuffle = oldSettings.enableShuffle,
                lastShuffleDate = oldSettings.lastShuffleDate != null ? SafeParse(
                    oldSettings.lastShuffleDate).ToString(CultureInfo.InvariantCulture) : null,
                shuffleHistory = oldSettings.shuffleHistory,
                favoriteThemes = oldSettings.favoriteThemes,
                language = oldSettings.language,
                autoUpdateCheck = !oldSettings.disableAutoUpdate,
                lastUpdateCheckTime = oldSettings.lastUpdateCheck != null ? SafeParse(
                    oldSettings.lastUpdateCheck).ToString(CultureInfo.InvariantCulture) : null,
                hideTrayIcon = oldSettings.hideTrayIcon,
                fullScreenPause = oldSettings.fullScreenPause,
                enableScripts = oldSettings.enableScripts
            };

            jsonText = JsonConvert.SerializeObject(newSettings, Formatting.Indented);
            File.WriteAllText("settings.json", jsonText);
        }
    }
}
