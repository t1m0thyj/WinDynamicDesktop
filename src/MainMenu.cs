// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class CustomContextMenu : ContextMenuStrip
    {
        private const int ENDSESSION_CLOSEAPP = 0x1;
        private const int WM_QUERYENDSESSION = 0x11;
        private const int WM_ENDSESSION = 0x16;

        public CustomContextMenu(ToolStripItem[] menuItems)
        {
            this.Items.AddRange(menuItems);
            IntPtr _handle = this.Handle;  // Handle needed for BeginInvoke to work
        }

        protected override void WndProc(ref Message m)
        {
            // https://github.com/rocksdanister/lively/blob/9142f6a4cfc222cd494f205a5daaa1a0238282e3/src/Lively/Lively/Views/WindowMsg/WndProcMsgWindow.xaml.cs#L41
            switch (m.Msg)
            {
                case WM_QUERYENDSESSION:
                    if (m.LParam.ToInt32() == ENDSESSION_CLOSEAPP)
                    {
                        UpdateChecker.RegisterApplicationRestart(null, (int)RestartFlags.RESTART_NO_CRASH |
                            (int)RestartFlags.RESTART_NO_HANG | (int)RestartFlags.RESTART_NO_REBOOT);
                    }
                    m.Result = new IntPtr(1);
                    break;
                case WM_ENDSESSION:
                    Application.Exit();
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }

    class MainMenu
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        public static ToolStripMenuItem themeItem;
        public static ToolStripMenuItem darkModeItem;
        public static ToolStripMenuItem startOnBootItem;
        public static ToolStripMenuItem enableScriptsItem;
        public static ToolStripMenuItem shuffleItem;
        public static ToolStripMenuItem fullScreenItem;
        public static ToolStripMenuItem hideTrayItem;

        public static ContextMenuStrip GetMenu()
        {
            List<ToolStripItem> menuItems = GetMenuItems();
            UwpDesktop.GetHelper().CheckStartOnBoot();
            return new CustomContextMenu(menuItems.ToArray());
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
                themeItem,
                new ToolStripSeparator()
            });
            items[0].Enabled = false;

            items.AddRange(LockScreenChanger.GetMenuItems());
            darkModeItem = new ToolStripMenuItem(_("Enable &Night Mode"), null, OnDarkModeClick);
            darkModeItem.Checked = JsonConfig.settings.darkMode;
            startOnBootItem = new ToolStripMenuItem(_("Start on &Boot"), null, OnStartOnBootClick);

            ToolStripMenuItem optionsItem = new ToolStripMenuItem(_("More &Options"));
            optionsItem.DropDownItems.AddRange(GetOptionsMenuItems().ToArray());

            items.AddRange(new List<ToolStripItem>()
            {
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
            items.Add(new ToolStripMenuItem(_("&Refresh Wallpaper"), null, OnRefreshItemClick));
            items.Add(new ToolStripSeparator());

            shuffleItem = new ToolStripMenuItem(_("Shuffle theme daily"), null, OnShuffleItemClick);
            shuffleItem.Checked = JsonConfig.settings.enableShuffle;
            items.Add(shuffleItem);

            fullScreenItem = new ToolStripMenuItem(_("Pause when fullscreen apps running"), null, OnFullScreenItemClick);
            fullScreenItem.Checked = JsonConfig.settings.fullScreenPause;
            items.Add(fullScreenItem);

            hideTrayItem = new ToolStripMenuItem(_("Hide system tray icon"), null, OnHideTrayItemClick);
            hideTrayItem.Checked = JsonConfig.settings.hideTrayIcon;
            items.Add(hideTrayItem);

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

        private static void OnDarkModeClick(object sender, EventArgs e)
        {
            AppContext.wpEngine.ToggleDarkMode();
        }

        private static void OnStartOnBootClick(object sender, EventArgs e)
        {
            UwpDesktop.GetHelper().ToggleStartOnBoot();
        }

        private static async void OnUpdateItemClick(object sender, EventArgs e)
        {
            await UpdateChecker.CheckManual();
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

        private static void OnRefreshItemClick(object sender, EventArgs e)
        {
            AppContext.wpEngine.RunScheduler(true);
        }

        private static void OnShuffleItemClick(object sender, EventArgs e)
        {
            ThemeShuffler.ToggleShuffle();
        }

        private static void OnFullScreenItemClick(object sender, EventArgs e)
        {
            AppContext.wpEngine.fullScreenChecker.ToggleFullScreenPause();
        }

        private static void OnHideTrayItemClick(object sender, EventArgs e)
        {
            AppContext.ToggleTrayIcon();
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
