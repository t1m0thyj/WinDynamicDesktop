// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class TrayMenu : ContextMenuStrip
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;

        public static ToolStripMenuItem darkModeItem;
        public static ToolStripMenuItem startOnBootItem;
        public static ToolStripMenuItem fullScreenItem;
        public static ToolStripMenuItem hideTrayItem;
        public static ToolStripMenuItem enableScriptsItem;

        public TrayMenu()
        {
            this.Items.AddRange(GetMenuItems());
            UwpDesktop.GetHelper().CheckStartOnBoot();
        }

        private ToolStripItem[] GetMenuItems()
        {
            darkModeItem = new ToolStripMenuItem(_("Enable &Night Mode"), null, OnDarkModeClick);
            darkModeItem.Checked = JsonConfig.settings.darkMode;
            startOnBootItem = new ToolStripMenuItem(_("Start on &Boot"), null, OnStartOnBootClick);
            ToolStripMenuItem optionsItem = new ToolStripMenuItem(_("More &Options"));
            optionsItem.DropDownItems.AddRange(GetOptionsMenuItems());

            return new ToolStripItem[]
            {
                new ToolStripMenuItem("WinDynamicDesktop") { Enabled = false },
                new ToolStripSeparator(),
                new ToolStripMenuItem(_("&Configure Schedule..."), null, OnScheduleItemClick),
                new ToolStripMenuItem(_("&Select Theme..."), null, OnThemeItemClick),
                new ToolStripSeparator(),
                darkModeItem,
                startOnBootItem,
                optionsItem,
                new ToolStripSeparator(),
                new ToolStripMenuItem(_("&Check for Updates"), null, OnUpdateItemClick),
                new ToolStripMenuItem(_("&About"), null, OnAboutItemClick),
                new ToolStripSeparator(),
                new ToolStripMenuItem(_("E&xit"), null, OnExitItemClick)
            };
        }

        private ToolStripItem[] GetOptionsMenuItems()
        {
            List<ToolStripItem> items = new List<ToolStripItem>()
            {
                new ToolStripMenuItem(_("Select &Language..."), null, OnLanguageItemClick),
                new ToolStripMenuItem(_("&Refresh Wallpaper"), null, OnRefreshItemClick),
                new ToolStripSeparator()
            };

            fullScreenItem = new ToolStripMenuItem(_("Pause when fullscreen apps running"), null,
                OnFullScreenItemClick);
            fullScreenItem.Checked = JsonConfig.settings.fullScreenPause;
            hideTrayItem = new ToolStripMenuItem(_("Hide system tray icon"), null, OnHideTrayItemClick);
            hideTrayItem.Checked = JsonConfig.settings.hideTrayIcon;
            items.AddRange(new ToolStripItem[]
            {
                fullScreenItem,
                hideTrayItem,
                new ToolStripSeparator()
            });

            items.AddRange(UpdateChecker.GetMenuItems());

            items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem(_("Edit configuration file"), null, OnEditConfigFileClick),
                new ToolStripMenuItem(_("Reload configuration file"), null, OnReloadConfigFileClick),
                new ToolStripSeparator()
            });

            enableScriptsItem = new ToolStripMenuItem(_("Enable PowerShell scripts"), null, OnEnableScriptsClick);
            enableScriptsItem.Checked = JsonConfig.settings.enableScripts;
            items.AddRange(new ToolStripItem[]
            {
                enableScriptsItem,
                new ToolStripMenuItem(_("Manage installed scripts"), null, OnManageScriptsClick)
            });
            return items.ToArray();
        }

        private void OnScheduleItemClick(object sender, EventArgs e)
        {
            LocationManager.ChangeLocation();
        }

        private void OnThemeItemClick(object sender, EventArgs e)
        {
            ThemeManager.SelectTheme();
        }

        private void OnDarkModeClick(object sender, EventArgs e)
        {
            SolarScheduler.ToggleDarkMode();
        }

        private void OnStartOnBootClick(object sender, EventArgs e)
        {
            UwpDesktop.GetHelper().ToggleStartOnBoot();
        }

        private async void OnUpdateItemClick(object sender, EventArgs e)
        {
            await UpdateChecker.CheckManual();
        }

        private void OnAboutItemClick(object sender, EventArgs e)
        {
            (new AboutDialog()).Show();
        }

        private void OnExitItemClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnLanguageItemClick(object sender, EventArgs e)
        {
            Localization.SelectLanguage(false);
        }

        private void OnRefreshItemClick(object sender, EventArgs e)
        {
            AppContext.scheduler.Run(true);
        }

        private void OnFullScreenItemClick(object sender, EventArgs e)
        {
            AppContext.scheduler.fullScreenChecker.ToggleFullScreenPause();
        }

        private void OnHideTrayItemClick(object sender, EventArgs e)
        {
            AppContext.ToggleTrayIcon();
        }

        private void OnEditConfigFileClick(object sender, EventArgs e)
        {
            Process.Start("explorer", "settings.json");
        }

        private void OnReloadConfigFileClick(object sender, EventArgs e)
        {
            JsonConfig.ReloadConfig();
        }

        private void OnEnableScriptsClick(object sender, EventArgs e)
        {
            ScriptManager.ToggleEnableScripts();
        }

        private void OnManageScriptsClick(object sender, EventArgs e)
        {
            Process.Start("explorer", "scripts");
        }
    }
}
