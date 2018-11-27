using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class AppContext : ApplicationContext
    {
        private Mutex _mutex;

        public static NotifyIcon notifyIcon;
        public static WallpaperChangeScheduler wcsService;

        public AppContext()
        {
            EnforceSingleInstance();

            JsonConfig.LoadConfig();
            InitializeGui();

            ThemeManager.Initialize();
            LocationManager.Initialize();
            wcsService = new WallpaperChangeScheduler();

            if (LocationManager.isReady && ThemeManager.isReady)
            {
                wcsService.RunScheduler();
            }

            UpdateChecker.Initialize();
        }

        private void EnforceSingleInstance()
        {
            _mutex = new Mutex(true, @"Global\WinDynamicDesktop", out bool isFirstInstance);
            GC.KeepAlive(_mutex);

            if (!isFirstInstance)
            {
                MessageBox.Show("Another instance of WinDynamicDesktop is already running. " +
                    "You can access it by clicking on the icon in the system tray.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                Environment.Exit(0);
            }
        }

        private void InitializeGui()
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

        public static void RunInBackground()
        {
            if (JsonConfig.firstRun && LocationManager.isReady && ThemeManager.isReady)
            {
                notifyIcon.BalloonTipTitle = "WinDynamicDesktop";
                notifyIcon.BalloonTipText = "The app is still running in the background. " +
                    "You can access it at any time by clicking on the icon in the system tray.";
                notifyIcon.ShowBalloonTip(10000);

                JsonConfig.firstRun = false;    // Don't show this message again
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
        }
    }
}
