using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class ThemeManager
    {
        public static bool isReady = false;
        public static List<ThemeConfig> themeData = new List<ThemeConfig>();

        private static ThemeDialog themeDialog;

        public static void Initialize()
        {
            DownloadMissingImages(FindMissingThemes());
        }

        public static void LoadAllThemes()
        {
            Directory.CreateDirectory("themes");

            List<string> themeNames = new List<string>() { "Mojave_Desert", "Solar_Gradients" };

            foreach (string filePath in Directory.EnumerateFiles("themes", "*.json"))
            {
                themeNames.Add(Path.GetFileNameWithoutExtension(filePath));
            }
            themeNames.Sort();

            foreach (string name in themeNames)
            {
                ThemeConfig theme = JsonConfig.LoadTheme(name);
                themeData.Add(theme);

                if (theme.themeName == JsonConfig.settings.themeName)
                {
                    JsonConfig.themeSettings = theme;
                }
            }
        }

        public static void SelectTheme()
        {
            if (themeDialog == null)
            {
                themeDialog = new ThemeDialog();
                //themeDialog.FormClosed += OnThemeDialogClosed;
                themeDialog.Show();
            }
            else
            {
                themeDialog.Activate();
            }
        }

        public static async void ExtractThemes(List<string> themeNames)
        {
            foreach (string name in themeNames)
            {
                string imagesZip = name + "_images.zip";

                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(imagesZip, "images");
                    File.Delete(imagesZip);
                });
            }
        }

        private static void RemoveTheme(ThemeConfig theme)
        {
            File.Delete(Path.Combine("themes", theme.themeName + ".json"));

            themeData.Remove(theme);
        }

        private static List<ThemeConfig> FindMissingThemes()
        {
            List<ThemeConfig> missingThemes = new List<ThemeConfig>();

            foreach (ThemeConfig theme in themeData)
            {
                int imageFileCount = Directory.GetFiles("images", theme.imageFilename).Length;
                List<int> imageIds = new List<int>();
                imageIds.AddRange(theme.dayImageList);
                imageIds.AddRange(theme.nightImageList);

                if (imageFileCount < imageIds.Distinct().Count())
                {
                    missingThemes.Add(theme);
                }
            }

            return missingThemes;
        }

        private static void DownloadMissingImages(List<ThemeConfig> missingThemes)
        {
            if (missingThemes.Count == 0)
            {
                isReady = true;
                return;
            }

            ProgressDialog downloadDialog = new ProgressDialog();
            downloadDialog.FormClosed += OnDownloadDialogClosed;
            downloadDialog.Show();
            AppContext.notifyIcon.ContextMenu.MenuItems[2].Enabled = false;

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += downloadDialog.OnDownloadProgressChanged;
                client.DownloadFileCompleted += downloadDialog.OnDownloadFileCompleted;

                foreach (ThemeConfig theme in missingThemes)
                {
                    if (theme.imagesZipUri != null)
                    {
                        downloadDialog.numDownloads++;
                        client.DownloadFileAsync(new Uri(theme.imagesZipUri),
                            theme.themeName + "_images.zip", theme.themeName);
                    }
                }
            }
        }

        private static void OnDownloadDialogClosed(object sender, EventArgs e)
        {
            AppContext.notifyIcon.ContextMenu.MenuItems[2].Enabled = true;
            List<ThemeConfig> missingThemes = FindMissingThemes();

            if (missingThemes.Count > 0)
            {
                DialogResult result = MessageBox.Show("Failed to download images. Click Retry " +
                    "to try again or Cancel to exit the program.", "Error",
                    MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                if (result == DialogResult.Retry)
                {
                    DownloadMissingImages(missingThemes);
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
