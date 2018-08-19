using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class MainMenu
    {
        public static MenuItem themeItem;
        public static MenuItem darkModeItem;
        public static MenuItem startOnBootItem;

        private static StartupManager startupManager;

        public static ContextMenu GetMenu()
        {
            List<MenuItem> menuItems = GetMenuItems();

            startupManager = UwpDesktop.GetStartupManager();

            return new ContextMenu(menuItems.ToArray());
        }

        private static List<MenuItem> GetMenuItems()
        {
            List<MenuItem> items = new List<MenuItem>();

            themeItem = new MenuItem("&Select Theme...", OnThemeItemClick);
            items.AddRange(new List<MenuItem>()
            {
                new MenuItem("WinDynamicDesktop"),
                new MenuItem("-"),
                themeItem,
                new MenuItem("&Update Location...", OnLocationItemClick),
                new MenuItem("&Refresh Wallpaper", OnRefreshItemClick),
                new MenuItem("-"),
            });
            items[0].Enabled = false;

            darkModeItem = new MenuItem("&Dark Mode", OnDarkModeClick);
            darkModeItem.Checked = JsonConfig.settings.darkMode;
            items.Add(darkModeItem);

            items.AddRange(SystemThemeChanger.GetMenuItems());

            startOnBootItem = new MenuItem("S&tart on Boot", OnStartOnBootClick);
            items.AddRange(new List<MenuItem>()
            {
                startOnBootItem,
                new MenuItem("-"),
            });

            items.AddRange(UpdateChecker.GetMenuItems());

            items.AddRange(new List<MenuItem>()
            {
                new MenuItem("&About", OnAboutItemClick),
                new MenuItem("-"),
                new MenuItem("E&xit", OnExitItemClick)
            });

            return items;
        }

        private static void ToggleDarkMode()
        {
            JsonConfig.settings.darkMode ^= true;
            darkModeItem.Checked = JsonConfig.settings.darkMode;

            AppContext.wcsService.LoadImageLists();
            AppContext.wcsService.RunScheduler();

            JsonConfig.SaveConfig();
        }

        private static void OnThemeItemClick(object sender, EventArgs e)
        {
            ThemeManager.SelectTheme();
        }

        private static void OnLocationItemClick(object sender, EventArgs e)
        {
            LocationManager.UpdateLocation();
        }

        private static void OnRefreshItemClick(object sender, EventArgs e)
        {
            AppContext.wcsService.RunScheduler(true);
        }

        private static void OnDarkModeClick(object sender, EventArgs e)
        {
            ToggleDarkMode();
        }

        private static void OnStartOnBootClick(object sender, EventArgs e)
        {
            startupManager.ToggleStartOnBoot();
        }

        private static void OnAboutItemClick(object sender, EventArgs e)
        {
            (new AboutDialog()).Show();
        }

        private static void OnExitItemClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
