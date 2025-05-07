﻿// This Source Code Form is subject to the terms of the Mozilla Public
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
        public static void RenameSettingsFile()  // Added 2020-10-25
        {
            if (File.Exists("settings.conf"))
            {
                File.Move("settings.conf", "settings.json");
                JsonConfig.firstRun = false;
            }
        }

        public static void UpdateConfig(string jsonText)
        {
            UpdateToVersion4(jsonText);
            UpdateToVersion5(jsonText);
        }

        public static bool UpdateObsoleteSettings(string jsonText)  // Added 2024-01-22
        {
            bool settingsChanged = false;
#pragma warning disable 618
            if (JsonConfig.settings.changeLockScreen)
            {
                JsonConfig.settings.lockScreenDisplayIndex = Array.FindIndex(JsonConfig.settings.activeThemes,
                    (themeId) => themeId != null);
                JsonConfig.settings.changeLockScreen = false;
                settingsChanged = true;
            }
            if (JsonConfig.settings.enableShuffle)
            {
                JsonConfig.settings.themeShuffleMode = 22;
                JsonConfig.settings.enableShuffle = false;
                settingsChanged = true;
            }
            if (JsonConfig.settings.lastShuffleDate != null)
            {
                JsonConfig.settings.lastShuffleTime = JsonConfig.settings.lastShuffleDate;
                JsonConfig.settings.lastShuffleDate = null;
                settingsChanged = true;
            }
            if (JsonConfig.settings.darkMode)
            {
                JsonConfig.settings.appearanceMode = (int)AppearanceMode.Dark;
                JsonConfig.settings.darkMode = false;
                settingsChanged = true;
            }
#pragma warning restore 618
            if (JsonConfig.settings.enableScripts && jsonText?.IndexOf("\"appearanceMode\"") == -1)
            {
                MessageDialog.ShowInfo("Updated to WinDynamicDesktop 5.5 successfully. PowerShell scripts that are " +
                    "outdated have been temporarily disabled.\n\nTo update them, download the latest scripts from " +
                    "https://windd.info/scripts/ and revise any custom scripts.\n\nAfter scripts have been updated, " +
                    "you can re-enable PowerShell scripts in the \"More Options\" menu.");
                JsonConfig.settings.enableScripts = false;
                settingsChanged = true;
            }
            return settingsChanged;
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

        private static void UpdateToVersion4(string jsonText)  // Added 2020-01-01
        {
            if (jsonText.IndexOf("\"changeSystemTheme\"") == -1)
            {
                return;
            }

            OldAppConfigV3 oldSettings;
            try
            {
                oldSettings = JsonConvert.DeserializeObject<OldAppConfigV3>(jsonText);
            }
            catch
            {
                return;
            }

            bool legacySettingsEnabled = (oldSettings.changeSystemTheme || oldSettings.changeAppTheme ||
                oldSettings.useAutoBrightness || oldSettings.useCustomAutoBrightness);
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

        private static void UpdateToVersion5(string jsonText)  // Added 2021-11-30
        {
            if (jsonText.IndexOf("\"themeName\"") == -1)
            {
                return;
            }

            OldAppConfigV4 oldSettings;
            try
            {
                oldSettings = JsonConvert.DeserializeObject<OldAppConfigV4>(jsonText);
            }
            catch
            {
                return;
            }

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
#pragma warning disable 618
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
#pragma warning restore 618
            };

            jsonText = JsonConvert.SerializeObject(newSettings, Formatting.Indented);
            File.WriteAllText("settings.json", jsonText);
        }
    }
}
