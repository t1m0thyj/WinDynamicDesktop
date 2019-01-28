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
        public static string[] defaultThemes = new string[] { "Mojave_Desert", "Solar_Gradients" };
        public static List<ThemeConfig> themeSettings = new List<ThemeConfig>();
        public static ThemeConfig currentTheme;

        private static ThemeDialog themeDialog;

        public static void Initialize()
        {
            Directory.CreateDirectory("themes");
            Compatibility.CompatibilizeThemes();

            List<string> themeIds = defaultThemes.ToList();

            foreach (string filePath in Directory.EnumerateFiles("themes", "*.json",
                SearchOption.AllDirectories))
            {
                string themeId = Path.GetFileName(Path.GetDirectoryName(filePath));

                if (!themeId.StartsWith("."))
                {
                    themeIds.Add(themeId);
                }
            }

            themeIds.Sort();

            foreach (string themeId in themeIds)
            {
                try
                {
                    ThemeConfig theme = JsonConfig.LoadTheme(themeId);

                    themeSettings.Add(theme);

                    if (theme.themeId == JsonConfig.settings.themeName)
                    {
                        currentTheme = theme;
                    }
                }
                catch
                {
                    DisableTheme(themeId);
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
            string themeId = Path.GetFileNameWithoutExtension(themePath);
            bool isInstalled = themeSettings.FindIndex(
                theme => theme.themeId == themeId) != -1;

            if (isInstalled)
            {
                DialogResult result = MessageBox.Show("The '" + themeId + "' theme is already " +
                    "installed. Do you want to overwrite it?", "Question", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    return null;
                }
            }

            try
            {
                Directory.CreateDirectory(Path.Combine("themes", themeId));

                if (Path.GetExtension(themePath) != ".json")
                {
                    using (ZipArchive archive = ZipFile.OpenRead(themePath))
                    {
                        ZipArchiveEntry themeJson = archive.Entries.Single(
                            entry => Path.GetExtension(entry.Name) == ".json");
                        themeJson.ExtractToFile(Path.Combine("themes", themeId, "theme.json"),
                            true);
                    }

                    ExtractTheme(themePath, themeId);
                }
                else
                {
                    File.Copy(themePath, Path.Combine("themes", themeId, "theme.json"), true);
                }

                return JsonConfig.LoadTheme(themeId);
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to import theme from " + themePath + "\n\n" + e.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        public static void ExtractTheme(string imagesZip, string themeId, bool deleteZip = false)
        {
            try
            {
                string themePath = Path.Combine("themes", themeId);
                Directory.CreateDirectory(themePath);

                using (ZipArchive archive = ZipFile.OpenRead(imagesZip))
                {
                    foreach (ZipArchiveEntry imageEntry in archive.Entries.Where(
                        entry => Path.GetDirectoryName(entry.FullName) == ""
                        && Path.GetExtension(entry.Name) != ".json"))
                    {
                        imageEntry.ExtractToFile(Path.Combine(themePath, imageEntry.Name), true);
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
                    File.Copy(imagePath, Path.Combine("themes", theme.themeId,
                        Path.GetFileName(imagePath)), true);
                }
                catch { }

                updatePercentage.Invoke((int)((i + 1) / (double)imagePaths.Length * 100));
            }
        }

        public static List<ThemeConfig> FindMissingThemes()
        {
            List<ThemeConfig> missingThemes = new List<ThemeConfig>();

            foreach (ThemeConfig theme in themeSettings)
            {
                int imageFileCount = 0;
                string themePath = Path.Combine("themes", theme.themeId);

                if (Directory.Exists(themePath))
                {
                    imageFileCount = Directory.GetFiles(themePath, theme.imageFilename).Length;
                }

                List<int> imageList = new List<int>();
                imageList.AddRange(theme.sunriseImageList);
                imageList.AddRange(theme.dayImageList);
                imageList.AddRange(theme.sunsetImageList);
                imageList.AddRange(theme.nightImageList);

                if (imageFileCount < imageList.Distinct().Count())
                {
                    missingThemes.Add(theme);
                }
            }

            return missingThemes;
        }

        public static void RemoveTheme(ThemeConfig theme)
        {
            if (currentTheme == theme)
            {
                currentTheme = null;
            }

            if (themeSettings.Contains(theme))
            {
                themeSettings.Remove(theme);
            }

            try
            {
                Directory.Delete(Path.Combine("themes", theme.themeId), true);
            }
            catch { }
        }

        private static void DisableTheme(string themeId)
        {
            Directory.Move(Path.Combine("themes", themeId), Path.Combine("themes", "." + themeId));

            MessageBox.Show("The '" + themeId + "' theme could not be loaded and has been " +
                "disabled. This is probably because it was created for an older version of the " +
                "app or its config file is formatted incorrectly.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            else
            {
                DialogResult result = MessageBox.Show("Failed to download images. Click Retry " +
                    "to try again, Ignore to continue with some themes disabled, or Abort to " +
                    "exit the program.", "Error", MessageBoxButtons.AbortRetryIgnore,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Abort)
                {
                    Environment.Exit(0);
                }
                else if (result == DialogResult.Retry)
                {
                    DownloadMissingImages(missingThemes);
                }
                else
                {
                    foreach (ThemeConfig theme in missingThemes)
                    {
                        themeSettings.Remove(theme);
                    }

                    if (missingThemes.Contains(currentTheme))
                    {
                        currentTheme = null;
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
