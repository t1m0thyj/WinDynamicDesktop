// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class AppContext : ApplicationContext
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private IpcManager ipcManager;

        public static NotifyIcon notifyIcon;
        public static EventScheduler scheduler = new EventScheduler();

        public AppContext(string[] args) : base(new HiddenForm())
        {
            JsonConfig.LoadConfig();
            Localization.Initialize();
            LoggingHandler.RotateDebugLog();

            CheckSingleInstance(args);
            ipcManager.ProcessArgs(args);

            InitializeTrayIcon();
            ThemeManager.Initialize();
            ScriptManager.Initialize();

            Task.Run(() =>
            {
                scheduler.RunAndUpdateLocation(false, LocationManager.Initialize);
                MainForm.Invoke(() => LaunchSequence.NextStep());
                UpdateChecker.Initialize();
            });
        }

        private void CheckSingleInstance(string[] args)
        {
            ipcManager = new IpcManager();

            if (ipcManager.isFirstInstance)
            {
                ipcManager.ListenForArgs(this);
            }
            else
            {
                if (args.Length > 0 || JsonConfig.settings.hideTrayIcon)
                {
                    ipcManager.SendArgsToFirstInstance(args);
                }
                else
                {
                    MessageDialog.ShowWarning(_("Another instance of WinDynamicDesktop is already running. You can " +
                        "access it by clicking on the icon in the system tray."), _("Error"));
                }

                Environment.Exit(0);
            }
        }

        private void InitializeTrayIcon()
        {
            Application.ApplicationExit += OnApplicationExit;
            Dark.Net.DarkNet.Instance.UserDefaultAppThemeIsDarkChanged += DarkUI.UserDefaultAppThemeIsDarkChanged;

            notifyIcon = new NotifyIcon
            {
                Visible = !JsonConfig.settings.hideTrayIcon,
                Icon = Properties.Resources.AppIcon,
                Text = "WinDynamicDesktop",
            };
            notifyIcon.ContextMenuStrip = new TrayMenu();
            notifyIcon.MouseUp += OnNotifyIconMouseUp;

            Localization.NotifyIfTestMode();
        }

        public static void ShowPopup(string message, string title = null)
        {
            notifyIcon.BalloonTipTitle = title ?? "WinDynamicDesktop";
            notifyIcon.BalloonTipText = message;
            notifyIcon.ShowBalloonTip(10000);
        }

        public static void ToggleTrayIcon()
        {
            bool isHidden = JsonConfig.settings.hideTrayIcon ^ true;
            JsonConfig.settings.hideTrayIcon = isHidden;
            TrayMenu.hideTrayItem.Checked = isHidden;
            notifyIcon.Visible = !isHidden;
        }

        private void OnNotifyIconMouseUp(object sender, MouseEventArgs e)
        {
            // Show context menu when taskbar icon is left clicked
            // Code from https://stackoverflow.com/a/2208910/5504760
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon, null);
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
            }

            ipcManager?.Dispose();
            JsonConfig.SaveConfig();
        }
    }
}
