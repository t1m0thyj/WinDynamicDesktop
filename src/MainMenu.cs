// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class MainMenu
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        public static ToolStripMenuItem themeItem;
        public static ToolStripMenuItem darkModeItem;
        public static ToolStripMenuItem startOnBootItem;
        public static ToolStripMenuItem enableScriptsItem;
        public static ToolStripMenuItem shuffleItem;
        public static ToolStripMenuItem fullScreenItem;

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
                new ToolStripMenuItem(_("&Configure Schedule..."), null, OnScheduleItemClick),
                themeItem
            });
            items[0].Enabled = false;

            darkModeItem = new ToolStripMenuItem(_("&Night Mode"), null, OnDarkModeClick);
            darkModeItem.Checked = JsonConfig.settings.darkMode;
            startOnBootItem = new ToolStripMenuItem(_("Start on &Boot"), null, OnStartOnBootClick);

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
            items.Add(new ToolStripSeparator());

            shuffleItem = new ToolStripMenuItem(_("Shuffle theme daily"), null, OnShuffleItemClick);
            shuffleItem.Checked = JsonConfig.settings.enableShuffle;
            items.Add(shuffleItem);

            fullScreenItem = new ToolStripMenuItem(_("Pause while fullscreen apps running"), null, OnFullScreenItemClick);
            fullScreenItem.Checked = JsonConfig.settings.fullScreenPause;
            items.Add(fullScreenItem);

            items.AddRange(UpdateChecker.GetMenuItems());
            items.Add(new ToolStripSeparator());

            items.Add(new ToolStripMenuItem(_("Edit configuration file"), null, OnEditConfigFileClick));
            items.Add(new ToolStripMenuItem(_("Reload configuration file"), null, OnReloadConfigFileClick));
            items.Add(new ToolStripSeparator());

            enableScriptsItem = new ToolStripMenuItem(_("Enable PowerShell scripts"), null, OnEnableScriptsClick);
            enableScriptsItem.Checked = JsonConfig.settings.enableScripts;
            items.Add(enableScriptsItem);
            items.Add(new ToolStripMenuItem(_("Manage installed scripts"), null, OnManageScriptsClick));

            return items;
        }

        private static void OnScheduleItemClick(object sender, EventArgs e)
        {
            LocationManager.ChangeLocation();
        }

        private static void OnThemeItemClick(object sender, EventArgs e)
        {
            ThemeManager.SelectTheme();
        }

        private static void OnRefreshItemClick(object sender, EventArgs e)
        {
            AppContext.wpEngine.RunScheduler(true);
        }

        private static void OnDarkModeClick(object sender, EventArgs e)
        {
            AppContext.wpEngine.ToggleDarkMode();
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
            Localization.SelectLanguage(false);
        }

        private static void OnShuffleItemClick(object sender, EventArgs e)
        {
            WallpaperShuffler.ToggleShuffle();
        }

        private static void OnFullScreenItemClick(object sender, EventArgs e)
        {
            AppContext.wpEngine.fullScreenChecker.ToggleFullScreenPause();
        }

        private static void OnEditConfigFileClick(object sender, EventArgs e)
        {
            Process.Start("explorer", "settings.json");
        }

        private static void OnReloadConfigFileClick(object sender, EventArgs e)
        {
            JsonConfig.ReloadConfig();
        }

        private static void OnEnableScriptsClick(object sender, EventArgs e)
        {
            ScriptManager.ToggleEnableScripts();
        }

        private static void OnManageScriptsClick(object sender, EventArgs e)
        {
            Process.Start("explorer", "scripts");
        }
    }
}
