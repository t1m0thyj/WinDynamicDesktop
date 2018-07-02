using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WinDynamicDesktop
{
    class FormWrapper : ApplicationContext
    {
        private Mutex _mutex;
        private ProgressDialog downloadDialog;
        private InputDialog locationDialog;
        private NotifyIcon notifyIcon;

        public StartupManager _startupManager;
        public WallpaperChangeScheduler _wcsService;

        public FormWrapper()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            EnforceSingleInstance();
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(OnPowerModeChanged);

            JsonConfig.LoadConfig();
            _wcsService = new WallpaperChangeScheduler();

            InitializeComponent();
            _startupManager = UwpDesktop.GetStartupManager(notifyIcon.ContextMenu.MenuItems[6]);

            if (!Directory.Exists("images"))
            {
                DownloadImages();
            }
            else if (JsonConfig.firstRun)
            {
                UpdateLocation();
            }
            else
            {
                _wcsService.StartScheduler();
            }
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
            notifyIcon = new NotifyIcon
            {
                Visible = !JsonConfig.settings.hideTrayIcon,
                Icon = Properties.Resources.AppIcon,
                Text = "WinDynamicDesktop",
                BalloonTipTitle = "WinDynamicDesktop"
            };

            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("WinDynamicDesktop"),
                new MenuItem("-"),
                new MenuItem("&Update Location...", OnLocationItemClick),
                new MenuItem("&Refresh Wallpaper", OnRefreshItemClick),
                new MenuItem("-"),
                new MenuItem("&Dark Mode", OnDarkItemClick),
                new MenuItem("&Start on Boot", OnStartupItemClick),
                new MenuItem("-"),
                new MenuItem("E&xit", OnExitItemClick)
            });

            notifyIcon.ContextMenu.MenuItems[0].Enabled = false;
            notifyIcon.ContextMenu.MenuItems[5].Checked = JsonConfig.settings.darkMode;
        }

        private void OnLocationItemClick(object sender, EventArgs e)
        {
            UpdateLocation();
        }

        private void OnRefreshItemClick(object sender, EventArgs e)
        {
            _wcsService.StartScheduler(true);
        }

        private void OnDarkItemClick(object sender, EventArgs e)
        {
            ToggleDarkMode();
        }
        
        private void OnStartupItemClick(object sender, EventArgs e)
        {
            _startupManager.ToggleStartOnBoot();
        }

        private void OnExitItemClick(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        public void DownloadImages()
        {
            string imagesZipUri = JsonConfig.imageSettings.imagesZipUri;
            bool imagesNotFound = false;

            if (imagesZipUri == null)
            {
                imagesNotFound = true;
            }
            else
            {
                DialogResult result = MessageBox.Show("WinDynamicDesktop needs to download images for " +
                    "the " + JsonConfig.imageSettings.themeName + " theme from " + imagesZipUri +
                    Environment.NewLine + Environment.NewLine + "Do you want to continue?", "Setup",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    imagesNotFound = true;
                }
            }

            if (imagesNotFound)
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
            else
            {
                if (JsonConfig.settings.location != null)
                {
                    _wcsService.StartScheduler(true);
                }

                if (JsonConfig.firstRun && locationDialog == null)
                {
                    BackgroundNotify();
                }
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

            if (JsonConfig.firstRun && downloadDialog == null)
            {
                BackgroundNotify();
            }
        }

        private void ToggleDarkMode()
        {
            _wcsService.ToggleDarkMode();
            notifyIcon.ContextMenu.MenuItems[5].Checked = JsonConfig.settings.darkMode;

            JsonConfig.SaveConfig();
        }

        private void BackgroundNotify()
        {
            notifyIcon.BalloonTipText = "The app is still running in the background. " +
                "You can access it at any time by right-clicking on this icon.";
            notifyIcon.ShowBalloonTip(10000);

            JsonConfig.firstRun = false;    // Don't show this message again
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                if (_wcsService.wallpaperTimer != null)
                {
                    _wcsService.wallpaperTimer.Stop();
                }

                _wcsService.enableTransitions = false;
            }
            else if (e.Mode == PowerModes.Resume)
            {
                if (JsonConfig.settings.location != null)
                {
                    _wcsService.StartScheduler();
                }

                _wcsService.enableTransitions = true;
            }
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