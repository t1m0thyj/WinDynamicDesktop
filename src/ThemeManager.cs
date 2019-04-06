// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

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
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        public static string[] defaultThemes = new string[] { "Mojave_Desert", "Solar_Gradients" };
        public static bool filesVerified = false;
        public static List<ThemeConfig> themeSettings = new List<ThemeConfig>();

        public static bool importMode = false;
        public static List<string> importPaths;
        public static List<ThemeConfig> importedThemes = new List<ThemeConfig>();

        private static ProgressDialog downloadDialog = new ProgressDialog();
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

            themeDialog.BringToFront();

            if (importPaths.Count > 0)
            {
                List<string> tempImportPaths = new List<string>(importPaths.ToArray());
                importPaths.Clear();
                themeDialog.ImportThemes(tempImportPaths);
            }
        }

        public static ThemeConfig ImportTheme(string themePath, IntPtr dialogHandle)
        {
            string themeId = Path.GetFileNameWithoutExtension(themePath);
            int themeIndex = themeSettings.FindIndex(t => t.themeId == themeId);

            if (themeIndex != -1)
            {
                TaskbarProgress.SetState(dialogHandle, TaskbarProgress.TaskbarStates.Paused);
                DialogResult result = MessageBox.Show(string.Format(_("The '{0}' theme is " +
                    "already installed. Do you want to overwrite it?"), themeId), _("Question"),
                    MessageBoxButtons.YesNo,  MessageBoxIcon.Warning);
                TaskbarProgress.SetState(dialogHandle, TaskbarProgress.TaskbarStates.Normal);

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

                ThemeConfig theme = JsonConfig.LoadTheme(themeId);

                if (themeIndex == -1)
                {
                    themeSettings.Add(theme);
                    themeSettings.Sort((t1, t2) => t1.themeId.CompareTo(t2.themeId));
                }
                else
                {
                    themeSettings[themeIndex] = theme;
                }

                return theme;
            }
            catch (Exception e)
            {
                TaskbarProgress.SetState(dialogHandle, TaskbarProgress.TaskbarStates.Error);
                MessageBox.Show(string.Format(_("Failed to import theme from {0}\n\n{1}"),
                    themePath, e.Message), _("Error"), MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                TaskbarProgress.SetState(dialogHandle, TaskbarProgress.TaskbarStates.Normal);

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

            try
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
            }catch(DirectoryNotFoundException d)
            {

                MessageBox.Show("This directory on '" + localPath.ToString() + "' does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            MessageBox.Show(string.Format(_("The '{0}' theme could not be loaded and has been " +
                "disabled. This is probably because it was created for an older version of the " +
                "app or its config file is formatted incorrectly."), themeId), _("Error"),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void DownloadMissingImages(List<ThemeConfig> missingThemes)
        {
            if (missingThemes.Count == 0)
            {
                filesVerified = true;
                LaunchSequence.NextStep();
                return;
            }

            downloadDialog.FormClosed += OnDownloadDialogClosed;
            downloadDialog.Show();

            MainMenu.themeItem.Enabled = false;
            downloadDialog.InitDownload(missingThemes.FindAll(
                theme => theme.imagesZipUri != null));
           
            if (downloadDialog.isUriEmpty == true)
            {
                // Attempt to close dialog to give dialog result options if uri is empty
                downloadDialog.Close();
            }

        }
      
        private static void OnDownloadDialogClosed(object sender, EventArgs e)
        {
            MainMenu.themeItem.Enabled = true;
            List<ThemeConfig> missingThemes = FindMissingThemes();

            if (missingThemes.Count == 0)
            {
                filesVerified = true;
                LaunchSequence.NextStep();
            }
            else
            {
                DialogResult result = MessageBox.Show(_("Failed to download images. Click Retry " +
                    "to try again, Ignore to continue with some themes disabled, or Abort to " +
                    "exit the program."), _("Error"), MessageBoxButtons.AbortRetryIgnore,
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

                    filesVerified = true;
                    LaunchSequence.NextStep();
                }
            }
        }

        private static void OnThemeDialogClosed(object sender, EventArgs e)
        {
            themeDialog = null;
            LaunchSequence.NextStep(true);
        }
    }
}
