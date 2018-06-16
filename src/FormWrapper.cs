using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WinDynamicDesktop
{
    class FormWrapper : ApplicationContext
    {
        private Uri imagesZipUri = new Uri("https://files.rb.gd/mojave_dynamic.zip");
        private ProgressDialog downloadDialog;
        private InputDialog locationDialog;
        private NotifyIcon notifyIcon;

        public WallpaperChangeScheduler wcsService = new WallpaperChangeScheduler();

        public FormWrapper()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(OnPowerModeChanged);

            InitializeComponent();

            JsonConfig.LoadConfig();

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
            notifyIcon = new NotifyIcon();
            notifyIcon.Visible = true;
            notifyIcon.Icon = Properties.Resources.AppIcon;
            notifyIcon.Text = "WinDynamicDesktop";
            notifyIcon.BalloonTipTitle = "WinDynamicDesktop";
            
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("WinDynamicDesktop"),
                new MenuItem("-"),
                new MenuItem("&Update Location", locationItem_Click),
                new MenuItem("E&xit", exitItem_Click)
            });
            notifyIcon.ContextMenu.MenuItems[0].Enabled = false;
        }

        private void locationItem_Click(object sender, EventArgs e)
        {
            UpdateLocation();
        }

        private void exitItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        public void DownloadImages()
        {
            downloadDialog = new ProgressDialog();
            downloadDialog.FormClosed += downloadDialog_Closed;
            downloadDialog.Show();

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += downloadDialog.onDownloadProgressChanged;
                client.DownloadFileCompleted += downloadDialog.onDownloadFileCompleted;
                client.DownloadFileAsync(imagesZipUri, "images.zip");
            }
        }

        private void downloadDialog_Closed(object sender, EventArgs e)
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
            else if (JsonConfig.settings.Location == null)
            {
                UpdateLocation();
            }
        }

            public void UpdateLocation()
        {
            if (locationDialog == null)
            {
                locationDialog = new InputDialog();
                locationDialog.wcsService = wcsService;
                locationDialog.FormClosed += locationDialog_Closed;
                locationDialog.Show();
            }
            else
            {
                locationDialog.ShowDialog();
            }
        }

        private void locationDialog_Closed(object sender, EventArgs e)
        {
            locationDialog = null;

            if (JsonConfig.settings.Location == null)
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
            }
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
                if (JsonConfig.settings.Location != null)
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