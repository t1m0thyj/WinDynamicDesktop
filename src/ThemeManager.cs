using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class ThemeManager
    {
        public static bool isReady = false;

        public static void Initialize()
        {
            Directory.CreateDirectory("themes");

            if (!Directory.Exists("images"))
            {
                DownloadImages();
            }
            else
            {
                isReady = true;
            }
        }

        public static List<ThemeConfig> GetInstalledThemes()
        {
            List<string> themeNames = new List<string>() { "Mojave_Desert", "Solar_Gradients" };

            foreach (string filePath in Directory.EnumerateFiles("themes", "*.json"))
            {
                themeNames.Add(Path.GetFileNameWithoutExtension(filePath));
            }

            themeNames.Sort();
            List<ThemeConfig> themes = new List<ThemeConfig>();

            foreach (string name in themeNames)
            {
                themes.Add(JsonConfig.LoadTheme(name));
            }

            return themes;
        }

        private static void DownloadImages()
        {
            string imagesZipUri = JsonConfig.themeSettings.imagesZipUri;

            if (imagesZipUri == null)
            {
                MessageBox.Show("Images folder not found. The program will quit now.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                Environment.Exit(0);
            }

            ProgressDialog downloadDialog = new ProgressDialog();
            downloadDialog.FormClosed += OnDownloadDialogClosed;
            downloadDialog.Show();

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += downloadDialog.OnDownloadProgressChanged;
                client.DownloadFileCompleted += downloadDialog.OnDownloadFileCompleted;
                client.DownloadFileAsync(new Uri(imagesZipUri), "images.zip");
            }
        }

        private static void OnDownloadDialogClosed(object sender, EventArgs e)
        {
            if (!Directory.Exists("images"))
            {
                DialogResult result = MessageBox.Show("Failed to download images. Click Retry to " +
                    "try again or Cancel to exit the program.", "Error",
                    MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                if (result == DialogResult.Retry)
                {
                    DownloadImages();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            else if (LocationManager.isReady)
            {
                isReady = true;
                AppContext.wcsService.RunScheduler();
                AppContext.BackgroundNotify();
            }
        }
    }
}
