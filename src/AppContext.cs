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
        private StartupManager startupManager;

        public static NotifyIcon notifyIcon;
        public static WallpaperChangeScheduler wcsService;

        public AppContext()
        {
            EnforceSingleInstance();

            JsonConfig.LoadConfig();
            InitializeGUI();

            startupManager = UwpDesktop.GetStartupManager(notifyIcon.ContextMenu.MenuItems[6]);
            wcsService = new WallpaperChangeScheduler();

            ThemeManager.Initialize();
            LocationManager.Initialize();

            var dialog = new ThemeDialog();
            dialog.Show();

            if (LocationManager.isReady && ThemeManager.isReady)
            {
                wcsService.RunScheduler();
            }
        }

        private void EnforceSingleInstance()
        {
            _mutex = new Mutex(true, @"Global\WinDynamicDesktop", out bool isFirstInstance);

            GC.KeepAlive(_mutex);

            if (!isFirstInstance)
            {
                MessageBox.Show("Another instance of WinDynamicDesktop is already running. " +
                    "You can access it by right-clicking the icon in the system tray.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                Environment.Exit(0);
            }
        }

        private void InitializeGUI()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);

            notifyIcon = new NotifyIcon
            {
                Visible = !JsonConfig.settings.hideTrayIcon,
                Icon = Properties.Resources.AppIcon,
                Text = "WinDynamicDesktop",
            };

            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("WinDynamicDesktop"),
                new MenuItem("-"),
                new MenuItem("&Update Location...", OnLocationItemClick),
                new MenuItem("&Refresh Wallpaper", OnRefreshItemClick),
                new MenuItem("-"),
                new MenuItem("&Dark Mode", OnDarkModeClick),
                new MenuItem("&Start on Boot", OnStartOnBootClick),
                new MenuItem("-"),
                new MenuItem("&About", OnAboutItemClick),
                new MenuItem("-"),
                new MenuItem("E&xit", OnExitItemClick)
            });

            notifyIcon.ContextMenu.MenuItems[0].Enabled = false;
            notifyIcon.ContextMenu.MenuItems[5].Checked = JsonConfig.settings.darkMode;
            notifyIcon.MouseUp += new MouseEventHandler(OnNotifyIconMouseUp);

            if (!UwpDesktop.IsRunningAsUwp())
            {
                UpdateChecker.Initialize();
            }

            SystemThemeChanger.Initialize();
        }

        public static void BackgroundNotify()
        {
            if (!JsonConfig.firstRun || !LocationManager.isReady || !ThemeManager.isReady)
            {
                return;
            }

            notifyIcon.BalloonTipTitle = "WinDynamicDesktop";
            notifyIcon.BalloonTipText = "The app is still running in the background. " +
                "You can access it at any time by right-clicking on this icon.";
            notifyIcon.ShowBalloonTip(10000);

            JsonConfig.firstRun = false;    // Don't show this message again
        }

        private void ToggleDarkMode()
        {
            JsonConfig.settings.darkMode ^= true;
            notifyIcon.ContextMenu.MenuItems[5].Checked = JsonConfig.settings.darkMode;

            wcsService.LoadImageLists();
            wcsService.RunScheduler();

            JsonConfig.SaveConfig();
        }

        private void OnLocationItemClick(object sender, EventArgs e)
        {
            LocationManager.UpdateLocation();
        }

        private void OnRefreshItemClick(object sender, EventArgs e)
        {
            wcsService.RunScheduler(true);
        }

        private void OnDarkModeClick(object sender, EventArgs e)
        {
            ToggleDarkMode();
        }

        private void OnStartOnBootClick(object sender, EventArgs e)
        {
            startupManager.ToggleStartOnBoot();
        }

        private void OnAboutItemClick(object sender, EventArgs e)
        {
            (new AboutDialog()).Show();
        }

        private void OnExitItemClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnNotifyIconMouseUp(object sender, MouseEventArgs e)
        {
            // Hack to show menu on left click from https://stackoverflow.com/a/2208910/5504760
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
