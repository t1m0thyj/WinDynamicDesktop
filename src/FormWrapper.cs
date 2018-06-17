using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WinDynamicDesktop
{
    class FormWrapper : ApplicationContext
    {
        private string registryStartupLocation = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private bool startOnBoot;

        private ProgressDialog downloadDialog;
        private InputDialog locationDialog;
        private NotifyIcon notifyIcon;

        public WallpaperChangeScheduler wcsService = new WallpaperChangeScheduler();

        public FormWrapper()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(OnPowerModeChanged);

            JsonConfig.LoadConfig();

            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(registryStartupLocation);
            startOnBoot = startupKey.GetValue("WinDynamicDesktop") != null;
            startupKey.Close();

            InitializeComponent();

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
                wcsService.StartScheduler();
            }
        }

        private void InitializeComponent()
        {
            notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = Properties.Resources.AppIcon,
                Text = "WinDynamicDesktop",
                BalloonTipTitle = "WinDynamicDesktop"
            };
            
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("WinDynamicDesktop"),
                new MenuItem("-"),
                new MenuItem("&Update Location", OnLocationItemClick),
                new MenuItem("&Start on Boot", OnStartupItemClick),
                new MenuItem("E&xit", OnExitItemClick)
            });

            notifyIcon.ContextMenu.MenuItems[0].Enabled = false;
            notifyIcon.ContextMenu.MenuItems[3].Checked = startOnBoot;
        }

        private void OnLocationItemClick(object sender, EventArgs e)
        {
            UpdateLocation();
        }
        
        private void OnStartupItemClick(object sender, EventArgs e)
        {
            ToggleStartOnBoot();
        }

        private void OnExitItemClick(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        public void DownloadImages()
        {
            string imagesZipUri = JsonConfig.imageSettings.imagesZipUri;

            if (imagesZipUri == null)
            {
                MessageBox.Show("Images folder not found. The program will quit now.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
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
                    Application.Exit();
                }
            }
            else if (JsonConfig.settings.location == null)
            {
                UpdateLocation();
            }
        }

        public void UpdateLocation()
        {
            if (locationDialog == null)
            {
                locationDialog = new InputDialog { wcsService = wcsService };
                locationDialog.FormClosed += OnLocationDialogClosed;
                locationDialog.Show();
            }
            else
            {
                locationDialog.ShowDialog();
            }
        }

        private void OnLocationDialogClosed(object sender, EventArgs e)
        {
            locationDialog = null;

            if (JsonConfig.settings.location == null)
            {
                notifyIcon.BalloonTipText = "This app cannot display wallpapers until you have " +
                    "entered a valid location. Right-click on this icon and click Update Location " +
                    "to fix this.";
                notifyIcon.ShowBalloonTip(10000);
            }
            else if (JsonConfig.firstRun)
            {
                notifyIcon.BalloonTipText = "The app is still running in the background. " +
                    "You can access it at any time by right-clicking on this icon.";
                notifyIcon.ShowBalloonTip(10000);

                JsonConfig.firstRun = false;    // Don't show this message again
            }
        }

        private void ToggleStartOnBoot()
        {
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(registryStartupLocation, true);

            if (!startOnBoot)
            {
                string exePath = Path.Combine(Directory.GetCurrentDirectory(),
                    Environment.GetCommandLineArgs()[0]);
                startupKey.SetValue("WinDynamicDesktop", exePath);
            }
            else
            {
                startupKey.DeleteValue("WinDynamicDesktop");
            }

            startOnBoot = !startOnBoot;
            notifyIcon.ContextMenu.MenuItems[3].Checked = startOnBoot;
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                if (wcsService.wallpaperTimer != null)
                {
                    wcsService.wallpaperTimer.Stop();
                }
            }
            else if (e.Mode == PowerModes.Resume)
            {
                if (JsonConfig.settings.location != null)
                {
                    wcsService.StartScheduler();
                }
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