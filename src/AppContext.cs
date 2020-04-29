// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using NamedPipeWrapper;

namespace WinDynamicDesktop
{
    class AppContext : ApplicationContext
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private Mutex _mutex;
        private NamedPipeServer<string> _namedPipe;

        public static NotifyIcon notifyIcon;
        public static WallpaperChangeScheduler wpEngine = new WallpaperChangeScheduler();

        public AppContext(string[] args)
        {
            DebugLogger.WriteLogFile();
            JsonConfig.LoadConfig();
            Localization.Initialize();

            ThemeManager.importPaths = args.Where(System.IO.File.Exists).ToList();
            HandleMultiInstance();

            InitializeTrayIcon();
            LocationManager.Initialize();
            ThemeManager.Initialize();
            ScriptManager.Initialize();

            wpEngine.RunScheduler();
            LaunchSequence.NextStep();
            UpdateChecker.Initialize();
        }

        private void HandleMultiInstance()
        {
            _mutex = new Mutex(true, @"Global\WinDynamicDesktop", out bool isFirstInstance);
            GC.KeepAlive(_mutex);

            if (isFirstInstance)
            {
                _namedPipe = new NamedPipeServer<string>("WinDynamicDesktop");
                _namedPipe.ClientMessage += OnNamedPipeClientMessage;
                _namedPipe.Start();
            }
            else
            {
                if (ThemeManager.importPaths.Count > 0)
                {
                    var namedPipeClient = new NamedPipeClient<string>("WinDynamicDesktop");
                    namedPipeClient.Start();
                    namedPipeClient.WaitForConnection();
                    namedPipeClient.PushMessage(string.Join("|", ThemeManager.importPaths));
                    Thread.Sleep(1000);
                    namedPipeClient.Stop();
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

            notifyIcon = new NotifyIcon
            {
                Visible = !JsonConfig.settings.hideTrayIcon,
                Icon = Properties.Resources.AppIcon,
                Text = "WinDynamicDesktop",
            };
            notifyIcon.ContextMenuStrip = MainMenu.GetMenu();
            notifyIcon.MouseUp += OnNotifyIconMouseUp;
        }

        public static void ShowPopup(string message, string title = null)
        {
            notifyIcon.BalloonTipTitle = title ?? "WinDynamicDesktop";
            notifyIcon.BalloonTipText = message;
            notifyIcon.ShowBalloonTip(10000);
        }

        private void OnNamedPipeClientMessage(NamedPipeConnection<string, string> conn, string message)
        {
            ThemeManager.importPaths.AddRange(message.Split('|'));

            if (!ThemeManager.importMode)
            {
                notifyIcon.ContextMenuStrip.BeginInvoke(new Action(() => ThemeManager.SelectTheme()));
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
            foreach (Form form in Application.OpenForms)
            {
                form.Close();
            }

            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
            }

            _namedPipe?.Stop();
            _mutex?.Dispose();
        }
    }
}
