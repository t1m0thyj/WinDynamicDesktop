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
        public static IntPtr taskbarHandle = IntPtr.Zero;

        public static ThemeResult TryLoad(string themeId)
        {
            if (!ThemeManager.defaultThemes.Contains(themeId) &&
                !File.Exists(Path.Combine("themes", themeId, "theme.json")))
            {
                return new ThemeResult(new NoThemeJSON(themeId));
            }
            else
            {
                return ValidateThemeJSON(JsonConfig.LoadTheme(themeId));
            }
        }

        private static bool IsNullOrEmpty(Array array)
        {
            return (array == null || array.Length == 0);
        }

        private static ThemeResult ValidateThemeJSON(ThemeConfig theme)
        {
            if (theme == null)
            {
                return new ThemeResult(new InvalidThemeJSON(theme.themeId));
            }
            else if (string.IsNullOrEmpty(theme.imageFilename) ||
                IsNullOrEmpty(theme.sunriseImageList) || IsNullOrEmpty(theme.dayImageList) ||
                IsNullOrEmpty(theme.sunsetImageList) || IsNullOrEmpty(theme.nightImageList))
            {
                return new ThemeResult(new MissingFieldsInThemeJSON(theme.themeId));
            }

            return new ThemeResult(theme);
        }

        public static void HandleError(ThemeError e)
        {
            if (taskbarHandle != IntPtr.Zero)
            {
                TaskbarProgress.SetState(taskbarHandle, TaskbarProgress.TaskbarStates.Error);
            }

            if (ThemeManager.downloadMode || ThemeManager.importMode)
            {
                MessageBox.Show(string.Format(_("Failed to import '{0}' theme:\n{1}"), e.themeId,
                    e.errorMsg), _("Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                DialogResult result = MessageBox.Show(string.Format(_("Failed to load '{0}' " +
                    "theme:\n{1}\n\nDo you want to disable this theme to prevent the error from " +
                    "happening again?"), e.themeId, e.errorMsg), _("Error"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                ThemeManager.DisableTheme(e.themeId, result == DialogResult.Yes);
            }

            if (taskbarHandle != IntPtr.Zero)
            {
                TaskbarProgress.SetState(taskbarHandle, TaskbarProgress.TaskbarStates.Normal);
            }
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

        public static ThemeResult ExtractTheme(string zipPath, string themeId,
            ThemeConfig preloadedTheme = null)
        {
            if (!File.Exists(zipPath))
            {
                return new ThemeResult(new FailedToFindLocation(themeId, zipPath));
            }

            string themePath = Path.Combine("themes", themeId);
            ThemeResult result;

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    if (preloadedTheme == null)
                    {
                        try
                        {
                            ZipArchiveEntry themeJson = archive.Entries.Single(
                                entry => Path.GetExtension(entry.Name) == ".json");
                            themeJson.ExtractToFile(Path.Combine(themePath, "theme.json"), true);
                        }
                        catch (InvalidOperationException)
                        {
                            return new ThemeResult(new NoThemeJSONInZIP(themeId, zipPath));
                        }

                        result = TryLoad(themeId);
                    }
                    else
                    {
                        Directory.CreateDirectory(themePath);
                        result = new ThemeResult(preloadedTheme);
                    }

                    ZipArchiveEntry[] imageEntries = archive.Entries.Where(
                        entry => Path.GetDirectoryName(entry.FullName) == ""
                        && Path.GetExtension(entry.Name) != ".json").ToArray();

                    if (imageEntries.Length == 0)
                    {
                        return new ThemeResult(new NoImagesInZIP(themeId, zipPath));
                    }

                    foreach (ZipArchiveEntry imageEntry in imageEntries)
                    {
                        imageEntry.ExtractToFile(Path.Combine(themePath, imageEntry.Name), true);
                    }
                }
            }
            catch (InvalidDataException)
            {
                return new ThemeResult(new InvalidZIP(themeId, zipPath));
            }

            return result;
        }

        public static ThemeResult CopyLocalTheme(string jsonPath, string themeId)
        {
            if (!File.Exists(jsonPath))
            {
                return new ThemeResult(new FailedToFindLocation(themeId, jsonPath));
            }

            File.Copy(jsonPath, Path.Combine("themes", themeId, "theme.json"), true);

            return TryLoad(themeId).Match(e => new ThemeResult(e), theme =>
            {
                string sourcePath = Path.GetDirectoryName(jsonPath);
                string[] imagePaths = Directory.GetFiles(sourcePath, theme.imageFilename);

                if (imagePaths.Length == 0)
                {
                    return new ThemeResult(new NoImagesInFolder(themeId, sourcePath));
                }

                foreach (string imagePath in imagePaths)
                {
                    try
                    {
                        File.Copy(imagePath, Path.Combine("themes", theme.themeId,
                            Path.GetFileName(imagePath)), true);
                    }
                    catch
                    {
                        return new ThemeResult(new FailedToCopyImage(themeId, imagePath));
                    }
                }

                return new ThemeResult(theme);
            });
        }
    }
}