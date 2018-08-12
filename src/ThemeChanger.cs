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
        private static string registryThemeLocation =
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private static MenuItem menuItem;

        public static void Initialize(NotifyIcon notifyIcon)
        {
            RegistryKey themeKey = Registry.CurrentUser.OpenSubKey(registryThemeLocation);

            if (themeKey != null)
            {
                themeKey.Close();

                notifyIcon.ContextMenu.MenuItems.Add(6, new MenuItem("&Change Windows 10 Theme",
                    OnThemeItemClick));
                menuItem = notifyIcon.ContextMenu.MenuItems[6];
                notifyIcon.ContextMenu.MenuItems[6].Checked =
                    JsonConfig.settings.changeSystemTheme;
            }
            else
            {
                JsonConfig.settings.changeSystemTheme = false;
            }
        }

        public static void TryUpdateSystemTheme()
        {
            if (!JsonConfig.settings.changeSystemTheme)
            {
                return;
            }

            RegistryKey themeKey = Registry.CurrentUser.OpenSubKey(registryThemeLocation, true);

            if (!WallpaperChangeScheduler.isDayNow || JsonConfig.settings.darkMode)
            {
                themeKey.SetValue("AppsUseLightTheme", 0);  // Dark theme
            }
            else
            {
                themeKey.SetValue("AppsUseLightTheme", 1);  // Light theme
            }

            themeKey.Close();
        }

        public static void ToggleChangeTheme()
        {
            JsonConfig.settings.changeSystemTheme ^= true;
            menuItem.Checked = JsonConfig.settings.changeSystemTheme;

            TryUpdateSystemTheme();
            JsonConfig.SaveConfig();
        }

        private static void OnThemeItemClick(object sender, EventArgs e)
        {
            ToggleChangeTheme();
        }
    }
}
