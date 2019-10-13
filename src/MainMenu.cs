// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace WinDynamicDesktop
{
    class MainMenu
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        public static ToolStripMenuItem themeItem;
        public static ToolStripMenuItem darkModeItem;
        public static ToolStripMenuItem startOnBootItem;

        public static ContextMenuStrip GetMenu()
        {
            List<ToolStripItem> menuItems = GetMenuItems();
            UwpDesktop.GetHelper().CheckStartOnBoot();

            ContextMenuStrip menuStrip = new ContextMenuStrip();
            menuStrip.Items.AddRange(menuItems.ToArray());
            IntPtr handle = menuStrip.Handle;  // Handle needed for BeginInvoke to work

            return menuStrip;
        }

        private static List<ToolStripItem> GetMenuItems()
        {
            List<ToolStripItem> items = new List<ToolStripItem>();

            themeItem = new ToolStripMenuItem(_("&Select Theme..."), null, OnThemeItemClick);

            items.AddRange(new List<ToolStripItem>()
            {
                new ToolStripMenuItem("WinDynamicDesktop"),
                new ToolStripSeparator(),
                themeItem,
                new ToolStripMenuItem(_("&Change Location..."), null, OnLocationItemClick)
            });
            items[0].Enabled = false;

            darkModeItem = new ToolStripMenuItem(_("&Night wallpaper only mode"), null, OnDarkModeClick);
            darkModeItem.Checked = JsonConfig.settings.darkMode;
            startOnBootItem = new ToolStripMenuItem(_("&Start on Boot"), null, OnStartOnBootClick);

            ToolStripMenuItem optionsItem = new ToolStripMenuItem(_("More &Options"));
            optionsItem.DropDownItems.AddRange(GetOptionsMenuItems().ToArray());

            items.AddRange(new List<ToolStripItem>()
            {
                new ToolStripMenuItem(_("&Refresh Wallpaper"), null, OnRefreshItemClick),
                new ToolStripSeparator(),
                darkModeItem,
                startOnBootItem,
                optionsItem,
                new ToolStripSeparator(),
                new ToolStripMenuItem(_("Edit Configuration File"), null, OnEditConfigFileClick),
                new ToolStripMenuItem(_("&Check for Updates"), null, OnUpdateItemClick),
                new ToolStripMenuItem(_("&About"), null, OnAboutItemClick),
                new ToolStripSeparator(),
                new ToolStripMenuItem(_("E&xit"), null, OnExitItemClick)
            });

            return items;
        }

        private static List<ToolStripItem> GetOptionsMenuItems()
        {
            List<ToolStripItem> items = new List<ToolStripItem>();

            items.Add(new ToolStripMenuItem(_("Select &Language..."), null, OnLanguageItemClick));

            if (BrightnessController.IsDDCSupported)
            {
                items.Add(new ToolStripMenuItem(_("Set Auto Brightness"), null, OnSetAutoBrightnessItemClick));
            }

            items.Add(new ToolStripSeparator());
            items.AddRange(SystemThemeChanger.GetMenuItems());
            items.AddRange(WallpaperCompressionChanger.GetMenuItems());
            items.AddRange(UpdateChecker.GetMenuItems());

            return items;
        }

        private static void ToggleDarkMode()
        {
            bool isEnabled = JsonConfig.settings.darkMode ^ true;
            JsonConfig.settings.darkMode = isEnabled;
            darkModeItem.Checked = isEnabled;

            AppContext.wpEngine.RunScheduler();
        }

        /// <summary>
        /// 
        /// Note: Starting with Windows 10 20H1, Notepad can be uninstalled from Windows.
        /// 
        /// To prevent issues going forward, the Try-Catch block will try to launch
        /// in Notepad first as all Windows PCs come shipped with it. However, if the
        /// user uninstalls Notepad, WinDynamicDesktop will ask the user to launch from another
        /// application.
        /// 
        /// </summary>
        /// 

        private static void EditConfigFile()
        {

            try
            {
                Process.Start("notepad.exe", "settings.conf");
            }
            catch (Exception e)
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "C:\\Program Files";
                    openFileDialog.Filter = "Application (*.exe)|*.exe";
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;
                        Process.Start(filePath, "settings.conf");
                    }

                }
            }

        }

        private static void OnEditConfigFileClick(object sender, EventArgs e)
        {
            EditConfigFile();
        }

        private static void OnThemeItemClick(object sender, EventArgs e)
        {
            ThemeManager.SelectTheme();
        }

        private static void OnLocationItemClick(object sender, EventArgs e)
        {
            LocationManager.ChangeLocation();
        }

        private static void OnRefreshItemClick(object sender, EventArgs e)
        {
            AppContext.wpEngine.RunScheduler(true);
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

        private static void OnLanguageItemClick(object sender, EventArgs e)
        {
            Localization.SelectLanguage();
        }

        private static void OnSetAutoBrightnessItemClick(object sender, EventArgs e)
        {
            (new BrightnessDialog()).ShowDialog();
        }
    }
}
