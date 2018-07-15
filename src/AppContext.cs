using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class AppContext : ApplicationContext
    {
        private Mutex _mutex;
        private ProgressDialog downloadDialog;
        private InputDialog locationDialog;
        private NotifyIcon notifyIcon;

        public StartupManager _startupManager;
        public WallpaperChangeScheduler _wcsService;

        public AppContext()
        {
            EnforceSingleInstance();

            JsonConfig.LoadConfig();
            InitializeComponent();

            _startupManager = UwpDesktop.GetStartupManager(notifyIcon.ContextMenu.MenuItems[6]);
            _wcsService = new WallpaperChangeScheduler();

            if (!Directory.Exists("images"))
            {
                DownloadImages();
            }
            else if (JsonConfig.settings.location == null)
            {
                UpdateLocation();
            }
            else
            {
                _wcsService.RunScheduler();
            }

            UpdateChecker.Initialize(notifyIcon);
        }

        private void EnforceSingleInstance()
        {
            bool isNewInstance;
            _mutex = new Mutex(true, @"Global\WinDynamicDesktop", out isNewInstance);

            GC.KeepAlive(_mutex);

            if (!isNewInstance)
            {
                MessageBox.Show("Another instance of WinDynamicDesktop is already running.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                Environment.Exit(0);
            }
        }

        private void InitializeComponent()
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

            if (!UwpDesktop.IsRunningAsUwp())
            {
                notifyIcon.ContextMenu.MenuItems.Add(8, new MenuItem("&Check for Updates Now",
                    OnUpdateItemClick));
                notifyIcon.ContextMenu.MenuItems.Add(9, new MenuItem("C&heck Automatically Once a Week",
                    OnAutoUpdateItemClick));
                notifyIcon.ContextMenu.MenuItems[9].Checked = !JsonConfig.settings.disableAutoUpdate;
                notifyIcon.ContextMenu.MenuItems.Add(10, new MenuItem("-"));

                notifyIcon.BalloonTipClicked += OnBalloonTipClicked;
            }
        }

        private void OnLocationItemClick(object sender, EventArgs e)
        {
            UpdateLocation();
        }

        private void OnRefreshItemClick(object sender, EventArgs e)
        {
            _wcsService.RunScheduler(true);
        }

        private void OnDarkModeClick(object sender, EventArgs e)
        {
            ToggleDarkMode();
        }
        
        private void OnStartOnBootClick(object sender, EventArgs e)
        {
            _startupManager.ToggleStartOnBoot();
        }

        private void OnAutoUpdateItemClick(object sender, EventArgs e)
        {
            ToggleAutoUpdate();
        }

        private void OnUpdateItemClick(object sender, EventArgs e)
        {
            UpdateChecker.CheckManual();
        }

        private void OnAboutItemClick(object sender, EventArgs e)
        {
            (new AboutDialog()).Show();
        }

        private void OnExitItemClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnBalloonTipClicked(object sender, EventArgs e)
        {
            if (notifyIcon.BalloonTipTitle == "Update Available")
            {
                System.Diagnostics.Process.Start(UpdateChecker.updateLink);
            }
        }

        public void DownloadImages()
        {
            string imagesZipUri = JsonConfig.imageSettings.imagesZipUri;

            if (imagesZipUri == null)
            {
                MessageBox.Show("Images folder not found. The program will quit now.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                Environment.Exit(0);
            }

            downloadDialog = new ProgressDialog();
            downloadDialog.FormClosed += OnDownloadDialogClosed;
            downloadDialog.Show();

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += downloadDialog.OnDownloadProgressChanged;
                client.DownloadFileCompleted += downloadDialog.OnDownloadFileCompleted;
                client.DownloadFileAsync(new Uri(imagesZipUri), "images.zip");
            }

            if (JsonConfig.settings.location == null)
            {
                UpdateLocation();
            }
        }

        private void OnDownloadDialogClosed(object sender, EventArgs e)
        {
            downloadDialog = null;

            if (!Directory.Exists("images"))
            {
                DialogResult result = MessageBox.Show("Failed to download images. Click Retry to " +
                    "try again or Cancel to exit the program.", "Error", MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error);

                if (result == DialogResult.Retry)
                {
                    DownloadImages();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            else if (locationDialog == null)
            {
                _wcsService.RunScheduler();
                BackgroundNotify();
            }
        }

        public void UpdateLocation()
        {
            if (locationDialog == null)
            {
                locationDialog = new InputDialog { _wcsService = _wcsService };
                locationDialog.FormClosed += OnLocationDialogClosed;
                locationDialog.Show();
            }
            else
            {
                locationDialog.Activate();
            }
        }

        private void OnLocationDialogClosed(object sender, EventArgs e)
        {
            locationDialog = null;
            BackgroundNotify();
        }

        private void ToggleDarkMode()
        {
            JsonConfig.settings.darkMode ^= true;
            notifyIcon.ContextMenu.MenuItems[5].Checked = JsonConfig.settings.darkMode;

            _wcsService.RunScheduler();
            JsonConfig.SaveConfig();
        }

        private void ToggleAutoUpdate()
        {
            JsonConfig.settings.disableAutoUpdate ^= true;
            notifyIcon.ContextMenu.MenuItems[9].Checked = !JsonConfig.settings.disableAutoUpdate;

            UpdateChecker.TryCheckAuto();
            JsonConfig.SaveConfig();
        }

        private void BackgroundNotify()
        {
            if (!JsonConfig.firstRun || downloadDialog != null || locationDialog != null)
            {
                return;
            }

            notifyIcon.BalloonTipTitle = "WinDynamicDesktop";
            notifyIcon.BalloonTipText = "The app is still running in the background. " +
                "You can access it at any time by right-clicking on this icon.";
            notifyIcon.ShowBalloonTip(10000);

            JsonConfig.firstRun = false;    // Don't show this message again
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (downloadDialog != null)
            {
                downloadDialog.Close();
            }

            if (locationDialog != null)
            {
                locationDialog.Close();
            }

            notifyIcon.Visible = false;
        }
    }
}
