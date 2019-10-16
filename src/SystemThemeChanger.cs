// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WinDynamicDesktop
{
    class SystemThemeChanger
    {
        private const string registryThemeLocation =
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private static ToolStripMenuItem automaticSystemThemeItem;
        private static ToolStripMenuItem automaticAppThemeItem;

        public static List<ToolStripItem> GetMenuItems()
        {
            RegistryKey themeKey = Registry.CurrentUser.OpenSubKey(registryThemeLocation);

            if (UwpDesktop.IsRunningAsUwp() || themeKey == null)
            {
                JsonConfig.settings.changeSystemTheme = false;
                JsonConfig.settings.changeAppTheme = false;

                return new List<ToolStripItem>();
            }

            themeKey.Close();

            automaticSystemThemeItem = new ToolStripMenuItem(
                Localization.GetTranslation("Change Windows 10 &system theme automatically"),
                null, OnSystemThemeItemClick);
            automaticSystemThemeItem.Checked = JsonConfig.settings.changeSystemTheme;

            automaticAppThemeItem = new ToolStripMenuItem(
                Localization.GetTranslation("Change Windows 10 &app theme automatically"),
                null, OnAppThemeItemClick);
            automaticAppThemeItem.Checked = JsonConfig.settings.changeAppTheme;

            return new List<ToolStripItem>() {
                new ToolStripSeparator(),
                automaticSystemThemeItem,
                automaticAppThemeItem
            };
        }

        public static void TryUpdateSystemTheme()
        {
            bool changeSystemTheme = JsonConfig.settings.changeSystemTheme;
            bool changeAppTheme = JsonConfig.settings.changeAppTheme;

            if (!changeSystemTheme && !changeAppTheme)
            {
                return;
            }

            bool isNight = !WallpaperChangeScheduler.isSunUp;
            RegistryKey themeKey = Registry.CurrentUser.OpenSubKey(registryThemeLocation, true);

            if (changeSystemTheme)
            {
                if (isNight)
                {
                    themeKey.SetValue("SystemUsesLightTheme", 0);   // Dark system theme
                }
                else
                {
                    themeKey.SetValue("SystemUsesLightTheme", 1);   // Light system theme
                }
            }

            if (changeAppTheme)
            {
                if (isNight)
                {
                    themeKey.SetValue("AppsUseLightTheme", 0);      // Dark app theme
                }
                else
                {
                    themeKey.SetValue("AppsUseLightTheme", 1);      // Light app theme
                }
            }

            themeKey.Close();
        }

        private static void OnSystemThemeItemClick(object sender, EventArgs e)
        {
            bool isEnabled = JsonConfig.settings.changeSystemTheme ^ true;
            JsonConfig.settings.changeSystemTheme = isEnabled;
            automaticSystemThemeItem.Checked = isEnabled;

            TryUpdateSystemTheme();
        }

        private static void OnAppThemeItemClick(object sender, EventArgs e)
        {
            bool isEnabled = JsonConfig.settings.changeAppTheme ^ true;
            JsonConfig.settings.changeAppTheme = isEnabled;
            automaticAppThemeItem.Checked = isEnabled;

            TryUpdateSystemTheme();
        }
    }
}
