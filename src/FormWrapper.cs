using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class FormWrapper : ApplicationContext
    {
        private Uri imagesZipUri = new Uri("https://files.rb.gd/mojave_dynamic.zip");

        private MainForm mainForm;
        private NotifyIcon notifyIcon;

        public FormWrapper()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            notifyIcon.Visible = true;
        }

        private void InitializeComponent()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.AppIcon;
            
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("WinDynamicDesktop"),
                new MenuItem("-"),
                new MenuItem("&Settings", settingsItem_Click),
                new MenuItem("E&xit", exitItem_Click)
            });
            notifyIcon.ContextMenu.MenuItems[0].Enabled = false;

            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
        }

        private void settingsItem_Click(object sender, EventArgs e)
        {
            ShowMainForm();
        }

        private void exitItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowMainForm();
        }

        public void AppendToLog(string lineText, bool addNewLine = true)
        {
            if (mainForm != null)
            {
                mainForm.AppendToLog(lineText, addNewLine);
            }
        }

        public void DownloadImagesZip()
        {
            AppendToLog("Downloading images.zip (39.0 MB)", false);

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += client_DownloadProgressChanged;
                client.DownloadFileCompleted += client_DownloadFileCompleted;
                client.DownloadFileAsync(imagesZipUri, "images.zip");
            }
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage % 10 == 9)
            {
                AppendToLog(".", false);
            }
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            AppendToLog("done");
            ZipFile.ExtractToDirectory("images.zip", "images");
        }

        public void ShowMainForm()
        {
            if (mainForm == null)
            {
                mainForm = new MainForm();
                mainForm.FormClosed += mainForm_Closed;
                mainForm.Show();
            }
            else
            {
                mainForm.ShowDialog();
            }
        }

        private void mainForm_Closed(object sender, EventArgs e)
        {
            mainForm = null;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (mainForm != null)
            {
                mainForm.Close();
            }
            notifyIcon.Visible = false;
        }
    }
}