using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
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
            Directory.CreateDirectory("images");
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

        public static void ExtractTheme(string themeName)
        {
            string imagesZip = themeName + "_images.zip";

            ZipFile.ExtractToDirectory(imagesZip, "images");
            File.Delete(imagesZip);
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

        private static void ReadyUp()
        {
            if (JsonConfig.firstRun && currentTheme == null)
            {
                SelectTheme();
            }
            else
            {
                isReady = true;

                if (LocationManager.isReady)
                {
                    AppContext.wcsService.RunScheduler();
                }

                AppContext.RunInBackground();
            }
        }

        private static void DownloadMissingImages(List<ThemeConfig> missingThemes)
        {
            if (missingThemes.Count == 0)
            {
                ReadyUp();
                return;
            }

            ProgressDialog downloadDialog = new ProgressDialog();
            downloadDialog.FormClosed += OnDownloadDialogClosed;
            downloadDialog.Show();

            MainMenu.themeItem.Enabled = false;
            downloadDialog.LoadQueue(missingThemes.FindAll(
                theme => theme.imagesZipUri != null));
            downloadDialog.DownloadNext();
        }

        private static void OnDownloadDialogClosed(object sender, EventArgs e)
        {
            MainMenu.themeItem.Enabled = true;
            List<ThemeConfig> missingThemes = FindMissingThemes();

            if (missingThemes.Count == 0)
            {
                ReadyUp();
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

                    ReadyUp();
                }
            }
        }

        private static void OnThemeDialogClosed(object sender, EventArgs e)
        {
            themeDialog = null;
            isReady = true;

            AppContext.RunInBackground();
        }
    }
}
