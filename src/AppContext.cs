﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Reflection;
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

            ThemeManager.importPaths = args.Where(System.IO.File.Exists).ToList();
            HandleMultiInstance(args);

            InitializeTrayIcon();
            LocationManager.Initialize();
            ThemeManager.Initialize();
            ScriptManager.Initialize();

            scheduler.RunAndUpdateLocation();
            LaunchSequence.NextStep();
            UpdateChecker.Initialize();
        }

        private void HandleMultiInstance(string[] args)
        {
            ipcManager = new IpcManager();

            if (ipcManager.isFirstInstance)
            {
                ipcManager.ListenForArgs(OnArgumentsReceived);
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

        private void OnArgumentsReceived(string[] args)
        {
            if (JsonConfig.settings.hideTrayIcon)
            {
                MainForm.BeginInvoke(ToggleTrayIcon);
            }

            foreach (string arg in args)
            {
                if (arg.StartsWith('/'))
                {
                    switch (arg.ToLower())
                    {
                        case "/refresh":
                            scheduler.RunAndUpdateLocation(true);
                            break;
                        case "/theme:auto":
                            MainForm.BeginInvoke(SolarScheduler.SetAppearanceMode, AppearanceMode.Automatic);
                            break;
                        case "/theme:light":
                            MainForm.BeginInvoke(SolarScheduler.SetAppearanceMode, AppearanceMode.Light); 
                            break;
                        case "/theme:dark":
                            MainForm.BeginInvoke(SolarScheduler.SetAppearanceMode, AppearanceMode.Dark);
                            break;
                        default:
                            Console.WriteLine("Unrecognized command line option: " + arg);
                            break;
                    }
                }
                else
                {
                    ThemeManager.importPaths.Add(arg);
                }
            }

            if (ThemeManager.importPaths.Count > 0 && !ThemeManager.importMode)
            {
                MainForm.BeginInvoke(ThemeManager.SelectTheme);
            }
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
