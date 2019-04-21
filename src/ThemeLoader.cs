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
    class ThemeLoader
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static string errorMsg;

        public static IntPtr taskbarHandle = IntPtr.Zero;

        public static ThemeConfig TryLoad(string themeId)
        {
            errorMsg = null;
            ThemeConfig theme = null;

            if (!ThemeManager.defaultThemes.Contains(themeId) &&
                !File.Exists(Path.Combine("themes", themeId, "theme.json")))
            {
                errorMsg = _("Theme JSON file could not be found.");
            }
            else
            {
                theme = JsonConfig.LoadTheme(themeId);
                ValidateThemeJSON(theme);
            }

            return (errorMsg == null) ? theme : null;
        }

        private static bool IsNullOrEmpty(Array array)
        {
            return (array == null || array.Length == 0);
        }

        private static void ValidateThemeJSON(ThemeConfig theme)
        {
            if (theme == null)
            {
                errorMsg = _("Theme JSON file could not be read because its format is invalid.");
            }
            else if (string.IsNullOrEmpty(theme.imageFilename) ||
                IsNullOrEmpty(theme.sunriseImageList) || IsNullOrEmpty(theme.dayImageList) ||
                IsNullOrEmpty(theme.sunsetImageList) || IsNullOrEmpty(theme.nightImageList))
            {
                errorMsg = _("Theme JSON file is missing required fields. These include " +
                    "'dayImageList', 'imageFilename', 'nightImageList', 'sunriseImageList', and " +
                    "'sunsetImageList'.");
            }
        }

        public static void HandleError(string themeId)
        {
            if (errorMsg == null)
            {
                return;
            }

            if (taskbarHandle != IntPtr.Zero)
            {
                TaskbarProgress.SetState(taskbarHandle, TaskbarProgress.TaskbarStates.Error);
            }

            if (!ThemeManager.importMode)
            {
                DialogResult result = MessageBox.Show(string.Format(_("Failed to load '{0}' " +
                    "theme:\n{1}\n\nDo you want to disable this theme to prevent the error from " +
                    "happening again?"), themeId, errorMsg), _("Error"), MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                ThemeManager.DisableTheme(themeId, result == DialogResult.Yes);
            }
            else
            {
                MessageBox.Show(string.Format(_("Failed to import '{0}' theme:\n{1}"), themeId,
                    errorMsg), _("Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (taskbarHandle != IntPtr.Zero)
            {
                TaskbarProgress.SetState(taskbarHandle, TaskbarProgress.TaskbarStates.Normal);
            }

            errorMsg = null;
        }

        public static void HandleError(string themeId, string errorText)
        {
            errorMsg = errorText;
            HandleError(themeId);
        }

        public static bool PromptDialog(string dialogText)
        {
            if (taskbarHandle != IntPtr.Zero)
            {
                TaskbarProgress.SetState(taskbarHandle, TaskbarProgress.TaskbarStates.Paused);
            }

            DialogResult result = MessageBox.Show(dialogText, _("Question"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            bool isAffirmative = (result == DialogResult.Yes);

            if (taskbarHandle != IntPtr.Zero)
            {
                TaskbarProgress.SetState(taskbarHandle, TaskbarProgress.TaskbarStates.Normal);
            }

            return isAffirmative;
        }

        public static bool ExtractTheme(string zipPath, string themeId, bool imagesOnly = false)
        {
            if (!File.Exists(zipPath))
            {
                errorMsg = string.Format(_("Failed to find the location {0}"), zipPath);
                return false;
            }

            string themePath = Path.Combine("themes", themeId);
            Directory.CreateDirectory(themePath);

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    if (!imagesOnly)
                    {
                        try
                        {
                            ZipArchiveEntry themeJson = archive.Entries.Single(
                                entry => Path.GetExtension(entry.Name) == ".json");
                            themeJson.ExtractToFile(Path.Combine(themePath, "theme.json"), true);
                        }
                        catch (InvalidOperationException)
                        {
                            errorMsg = string.Format(_("No theme JSON found in the ZIP file {0}"),
                                zipPath);
                            return false;
                        }
                    }

                    ZipArchiveEntry[] imageEntries = archive.Entries.Where(
                        entry => Path.GetDirectoryName(entry.FullName) == ""
                        && Path.GetExtension(entry.Name) != ".json").ToArray();

                    if (imageEntries.Length == 0)
                    {
                        errorMsg = string.Format(_("No images found in the ZIP file {0}"),
                            zipPath);
                        return false;
                    }

                    foreach (ZipArchiveEntry imageEntry in imageEntries)
                    {
                        imageEntry.ExtractToFile(Path.Combine(themePath, imageEntry.Name), true);
                    }
                }
            }
            catch (InvalidDataException)
            {
                errorMsg = string.Format(_("Failed to read the ZIP file at {0} because its " +
                    "format is invalid."), zipPath);
                return false;
            }

            if (imagesOnly)
            {
                File.Delete(zipPath);
            }

            return true;
        }

        public static bool CopyLocalTheme(ThemeConfig theme, string localPath,
            Action<int> updatePercentage)
        {
            if (!Directory.Exists(localPath))
            {
                errorMsg = string.Format(_("Failed to find the location {0}"), localPath);
                return false;
            }

            string[] imagePaths = Directory.GetFiles(localPath, theme.imageFilename);

            for (int i = 0; i < imagePaths.Length; i++)
            {
                string imagePath = imagePaths[i];

                try
                {
                    File.Copy(imagePath, Path.Combine("themes", theme.themeId,
                        Path.GetFileName(imagePath)), true);
                }
                catch
                {
                    errorMsg = string.Format(_("Failed to copy the image file {0}"), imagePath);
                    return false;
                }

                updatePercentage.Invoke((int)((i + 1) / (double)imagePaths.Length * 100));
            }

            return true;
        }
    }
}
