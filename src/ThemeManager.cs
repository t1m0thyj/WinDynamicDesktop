using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using DarkUI.Forms;

namespace WinDynamicDesktop
{
    class ThemeManager
    {
        public static bool isReady = false;
        public static string[] defaultThemes = new string[] { "Mojave_Desert", "Solar_Gradients" };
        public static List<ThemeConfig> themeSettings = new List<ThemeConfig>();
        public static ThemeConfig currentTheme;

        private static ThemeDialog themeDialog;

        public static void Initialize()
        {
            Directory.CreateDirectory("images");
            Directory.CreateDirectory("themes");

            List<string> themeNames = defaultThemes.ToList();

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

        public static ThemeConfig ImportTheme(string themePath)
        {
            string themeName = Path.GetFileNameWithoutExtension(themePath);
            bool isInstalled = themeSettings.FindIndex(
                theme => theme.themeName == themeName) != -1;

            if (isInstalled)
            {
                DialogResult result = DarkMessageBox.ShowWarning("The '" +
                    themeName.Replace('_', ' ') + "' theme is already installed. Do you want to " +
                    "overwrite it?", "Question", DarkDialogButton.YesNo);

                if (result != DialogResult.Yes)
                {
                    return null;
                }
            }

            try
            {
                if (Path.GetExtension(themePath) == ".zip")
                {
                    using (ZipArchive archive = ZipFile.OpenRead(themePath))
                    {
                        ZipArchiveEntry themeJson = archive.Entries.Single(
                            entry => Path.GetExtension(entry.Name) == ".json");
                        themeJson.ExtractToFile(Path.Combine("themes", themeName + ".json"), true);
                    }

                    ExtractTheme(themePath);
                }
                else
                {
                    File.Copy(themePath, Path.Combine("themes", themeName + ".json"), true);
                }

                return JsonConfig.LoadTheme(themeName);
            }
            catch (Exception e)
            {
                DarkMessageBox.ShowWarning("Failed to import theme:\n" + e.Message, "Error");
                return null;
            }
        }

        public static void ExtractTheme(string imagesZip, bool deleteZip = false)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(imagesZip))
                {
                    foreach (ZipArchiveEntry imageEntry in archive.Entries.Where(
                        entry => Path.GetDirectoryName(entry.FullName) == ""
                        && Path.GetExtension(entry.Name) != ".json"))
                    {
                        imageEntry.ExtractToFile(Path.Combine("images", imageEntry.Name), true);
                    }
                }

                if (deleteZip)
                {
                    File.Delete(imagesZip);
                }
            }
            catch { }
        }

        public static void CopyLocalTheme(ThemeConfig theme, string localPath,
            Action<int> updatePercentage)
        {
            string[] imagePaths = Directory.GetFiles(localPath, theme.imageFilename);

            for (int i = 0; i < imagePaths.Length; i++)
            {
                string imagePath = imagePaths[i];

                try
                {
                    File.Copy(imagePath, Path.Combine("images", Path.GetFileName(imagePath)),
                        true);
                }
                catch { }

                updatePercentage.Invoke((int)((i + 1) / (float)imagePaths.Length * 100));
            }
        }

        public static List<ThemeConfig> FindMissingThemes()
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

        public static void RemoveTheme(ThemeConfig theme)
        {
            if (themeSettings.Contains(theme))
            {
                themeSettings.Remove(theme);
            }

            try
            {
                File.Delete(Path.Combine("themes", theme.themeName + ".json"));

                foreach (string imagePath in Directory.GetFiles("images", theme.imageFilename))
                {
                    File.Delete(imagePath);
                }
            }
            catch { }
        }

        private static void ReadyUp()
        {
            if (currentTheme == null && (JsonConfig.firstRun
                || JsonConfig.settings.themeName != null))
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
                DialogResult result = DarkMessageBox.ShowError("Failed to download images. " +
                    "Click Retry to try again or Cancel to exit the program.", "Error",
                    DarkDialogButton.RetryCancel);

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
                DialogResult result = DarkMessageBox.ShowWarning("Failed to download images. " +
                    "Click Retry to try again or Cancel to continue with some themes disabled.",
                    "Error", DarkDialogButton.RetryCancel);

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
