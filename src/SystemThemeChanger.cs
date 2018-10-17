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

        private static MenuItem menuItem;

        public static List<MenuItem> GetMenuItems()
        {
            RegistryKey themeKey = Registry.CurrentUser.OpenSubKey(registryThemeLocation);

            if (themeKey != null)
            {
                themeKey.Close();

                menuItem = new MenuItem("&Change Windows 10 theme color", OnThemeItemClick);
                menuItem.Checked = JsonConfig.settings.changeSystemTheme;

                return new List<MenuItem>() { menuItem };
            }
            else
            {
                JsonConfig.settings.changeSystemTheme = false;

                return new List<MenuItem>();
            }
        }

        public static void TryUpdateSystemTheme()
        {
            if (!JsonConfig.settings.changeSystemTheme)
            {
                return;
            }

            bool darkTheme = !WallpaperChangeScheduler.isDayNow || JsonConfig.settings.darkMode;
            RegistryKey themeKey = Registry.CurrentUser.OpenSubKey(registryThemeLocation, true);

            if (darkTheme)
            {
                themeKey.SetValue("AppsUseLightTheme", 0);  // Dark theme
            }
            else
            {
                themeKey.SetValue("AppsUseLightTheme", 1);  // Light theme
            }

            themeKey.Close();
        }

        private static void ToggleChangeSystemTheme()
        {
            bool isEnabled = JsonConfig.settings.changeSystemTheme ^ true;
            JsonConfig.settings.changeSystemTheme = isEnabled;
            menuItem.Checked = isEnabled;

            TryUpdateSystemTheme();
            JsonConfig.SaveConfig();
        }

        private static void OnThemeItemClick(object sender, EventArgs e)
        {
            ToggleChangeSystemTheme();
        }
    }
}
