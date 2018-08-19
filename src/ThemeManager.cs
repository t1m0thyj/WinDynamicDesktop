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
        public static List<ThemeConfig> themeSettings = new List<ThemeConfig>();
        public static ThemeConfig currentTheme;

        private static ThemeDialog themeDialog;

        public static void Initialize()
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
                themeSettings.Add(theme);

                if (theme.themeName == JsonConfig.settings.themeName)
                {
                    currentTheme = theme;
                }
            }
        }

        public static void DownloadMissingImages()
        {
            DownloadMissingImages(FindMissingThemes());
        }

        public static void SelectTheme()
        {
            if (themeDialog == null)
            {
                themeDialog = new ThemeDialog();
                themeDialog.FormClosed += OnThemeDialogClosed;
                themeDialog.Show();
            }
            else
            {
                themeDialog.Activate();
            }
        }

        public static void ExtractThemes(List<string> themeNames)
        {
            foreach (string name in themeNames)
            {
                string imagesZip = name + "_images.zip";

                ZipFile.ExtractToDirectory(imagesZip, "images");
                File.Delete(imagesZip);
            }
        }

        private static List<ThemeConfig> FindMissingThemes()
        {
            List<ThemeConfig> missingThemes = new List<ThemeConfig>();

            foreach (ThemeConfig theme in themeSettings)
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
            MainMenu.themeItem.Enabled = false;

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

        private static void SetReady()
        {
            isReady = true;

            if (LocationManager.isReady)
            {
                AppContext.wcsService.RunScheduler();
                AppContext.BackgroundNotify();
            }
        }

        private static void OnDownloadDialogClosed(object sender, EventArgs e)
        {
            MainMenu.themeItem.Enabled = true;
            List<ThemeConfig> missingThemes = FindMissingThemes();

            if (missingThemes.Count == 0)
            {
                if (currentTheme == null)
                {
                    SelectTheme();
                }
                else
                {
                    SetReady();
                }
            }
            else if (currentTheme != null && missingThemes.Contains(currentTheme))
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
            else
            {
                DialogResult result = MessageBox.Show("Failed to download images. Click Retry " +
                    "to try again or Cancel to continue with some themes disabled.", "Error",
                    MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Retry)
                {
                    DownloadMissingImages(missingThemes);
                }
                else
                {
                    foreach (ThemeConfig theme in missingThemes)
                    {
                        themeSettings.Remove(theme);
                    }
                }
            }
        }

        private static void OnThemeDialogClosed(object sender, EventArgs e)
        {
            SetReady();
        }
    }
}
