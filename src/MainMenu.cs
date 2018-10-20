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

        public static ContextMenu GetMenu()
        {
            List<MenuItem> menuItems = GetMenuItems();
            UwpDesktop.GetHelper().CheckStartOnBoot();

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
                new MenuItem("&Update Location...", OnLocationItemClick)
            });
            items[0].Enabled = false;

            darkModeItem = new MenuItem("Enable &Dark Mode", OnDarkModeClick);
            darkModeItem.Checked = JsonConfig.settings.darkMode;
            startOnBootItem = new MenuItem("&Start on Boot", OnStartOnBootClick);

            MenuItem optionsItem = new MenuItem("More &Options");
            optionsItem.MenuItems.AddRange(GetOptionsMenuItems().ToArray());

            items.AddRange(new List<MenuItem>()
            {
                new MenuItem("&Refresh Wallpaper", OnRefreshItemClick),
                new MenuItem("-"),
                darkModeItem,
                startOnBootItem,
                optionsItem,
                new MenuItem("-"),
                new MenuItem("&Check for Updates", OnUpdateItemClick),
                new MenuItem("&About", OnAboutItemClick),
                new MenuItem("-"),
                new MenuItem("E&xit", OnExitItemClick)
            });

            return items;
        }

        private static List<MenuItem> GetOptionsMenuItems()
        {
            List<MenuItem> items = new List<MenuItem>();

            items.AddRange(SystemThemeChanger.GetMenuItems());
            items.AddRange(UpdateChecker.GetMenuItems());

            return items;
        }

        private static void ToggleDarkMode()
        {
            bool isEnabled = JsonConfig.settings.darkMode ^ true;
            JsonConfig.settings.darkMode = isEnabled;
            darkModeItem.Checked = isEnabled;

            AppContext.wcsService.LoadImageLists();
            AppContext.wcsService.RunScheduler();
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
            AppContext.wcsService.RunScheduler();
        }

        private static void OnDarkModeClick(object sender, EventArgs e)
        {
            ToggleDarkMode();
        }

        private static void OnStartOnBootClick(object sender, EventArgs e)
        {
            UwpDesktop.GetHelper().ToggleStartOnBoot();
        }

        private static void OnUpdateItemClick(object sender, EventArgs e)
        {
            UpdateChecker.CheckManual();
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
