// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinDynamicDesktop.COM;

namespace WinDynamicDesktop
{
    public class ThemeConfig
    {
        public string themeId { get; set; }
        public string displayName { get; set; }
        public string imageFilename { get; set; }
        public string imageCredits { get; set; }
        public int? dayHighlight { get; set; }
        public int? nightHighlight { get; set; }
        public int[] sunriseImageList { get; set; }
        public int[] dayImageList { get; set; }
        public int[] sunsetImageList { get; set; }
        public int[] nightImageList { get; set; }
    }

    class ThemeLoader
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        public static IntPtr taskbarHandle = IntPtr.Zero;

        public static ThemeResult TryLoad(string themeId)
        {
            string jsonPath = Path.Combine("themes", themeId, "theme.json");

            if (!File.Exists(jsonPath))
            {
                return new ThemeResult(new NoThemeJSON(themeId));
            }

            ThemeConfig theme;

            try
            {
                theme = JsonConvert.DeserializeObject<ThemeConfig>(File.ReadAllText(jsonPath));
            }
            catch (JsonException e)
            {
                return new ThemeResult(new InvalidThemeJSON(themeId, e.Message));
            }

            if (theme == null)
            {
                return new ThemeResult(new InvalidThemeJSON(themeId, _("Empty JSON file")));
            }

            theme.themeId = themeId;
            return ThemeJsonValidator.ValidateQuick(theme);
        }

        public static void HandleError(ThemeError e)
        {
            if (taskbarHandle != IntPtr.Zero)
            {
                TaskbarProgress.SetState(taskbarHandle, TaskbarProgress.TaskbarStates.Error);
            }

            if (ThemeManager.downloadMode || ThemeManager.importMode)
            {
                MessageDialog.ShowWarning(string.Format(_("Failed to import '{0}' theme:\n{1}"), e.themeId, e.errorMsg),
                    _("Error"));
            }
            else
            {
                DialogResult result = MessageDialog.ShowQuestion(string.Format(_("Failed to load '{0}' theme:\n{1}\n" +
                    "\nDo you want to disable this theme to prevent the error from happening again?"), e.themeId,
                    e.errorMsg), _("Error"), true);
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

            DialogResult result = MessageDialog.ShowQuestion(dialogText, _("Question"), true);
            bool isAffirmative = (result == DialogResult.Yes);

            if (taskbarHandle != IntPtr.Zero)
            {
                TaskbarProgress.SetState(taskbarHandle, TaskbarProgress.TaskbarStates.Normal);
            }

            return isAffirmative;
        }

        public static ThemeResult ExtractTheme(string zipPath, string themeId)
        {
            if (!File.Exists(zipPath))
            {
                return new ThemeResult(new FailedToFindLocation(themeId, zipPath));
            }

            string themePath = Path.Combine("themes", themeId);
            Directory.CreateDirectory(themePath);

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    try
                    {
                        ZipArchiveEntry themeJson = archive.Entries.Single(
                            entry => Path.GetExtension(entry.Name) == ".json");
                        themeJson.ExtractToFile(Path.Combine(themePath, "theme.json"), true);
                    }
                    catch (InvalidOperationException)
                    {
                        return RollbackInstall(new NoThemeJSONInZIP(themeId, zipPath));
                    }

                    return TryLoad(themeId).Match(RollbackInstall, theme =>
                    {
                        ZipArchiveEntry[] imageEntries = archive.Entries.Where(
                            entry => Path.GetDirectoryName(entry.FullName) == ""
                            && Path.GetExtension(entry.Name) != ".json").ToArray();

                        if (imageEntries.Length == 0)
                        {
                            return RollbackInstall(new NoImagesInZIP(themeId, zipPath));
                        }

                        foreach (ZipArchiveEntry imageEntry in imageEntries)
                        {
                            imageEntry.ExtractToFile(Path.Combine(themePath, imageEntry.Name), true);
                        }

                        return ThemeJsonValidator.ValidateFull(theme).Match(RollbackInstall,
                            theme2 => new ThemeResult(theme2));
                    });
                }
            }
            catch (InvalidDataException)
            {
                return RollbackInstall(new InvalidZIP(themeId, zipPath));
            }
        }

        public static ThemeResult CopyLocalTheme(string jsonPath, string themeId)
        {
            if (!File.Exists(jsonPath))
            {
                return new ThemeResult(new FailedToFindLocation(themeId, jsonPath));
            }

            string themePath = Path.Combine("themes", themeId);
            Directory.CreateDirectory(themePath);
            File.Copy(jsonPath, Path.Combine(themePath, "theme.json"), true);

            return TryLoad(themeId).Match(RollbackInstall, theme =>
            {
                string sourcePath = Path.GetDirectoryName(jsonPath);
                string[] imagePaths = Directory.GetFiles(sourcePath, theme.imageFilename);

                if (imagePaths.Length == 0)
                {
                    return RollbackInstall(new NoImagesInFolder(themeId, sourcePath));
                }

                foreach (string imagePath in imagePaths)
                {
                    try
                    {
                        File.Copy(imagePath, Path.Combine(themePath, Path.GetFileName(imagePath)), true);
                    }
                    catch
                    {
                        return RollbackInstall(new FailedToCopyImage(themeId, imagePath));
                    }
                }

                return ThemeJsonValidator.ValidateFull(theme).Match(RollbackInstall, theme2 => new ThemeResult(theme2));
            });
        }

        private static ThemeResult RollbackInstall(ThemeError error)
        {
            Task.Run(() =>
            {
                try
                {
                    System.Threading.Thread.Sleep(100);  // Wait for folder to free up
                    Directory.Delete(Path.Combine("themes", error.themeId), true);
                }
                catch { /* Do nothing */ }
            });

            return new ThemeResult(error);
        }
    }
}