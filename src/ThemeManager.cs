// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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

        private static ProgressDialog downloadDialog;
        public static ThemeConfig currentTheme;
        private static ThemeDialog themeDialog;

        public static void Initialize()
        {
            Directory.CreateDirectory("themes");
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
            LoadInstalledThemes(themeIds);
            DownloadMissingImages();
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
                // Convert to array and back to list for deep copy
                List<string> tempImportPaths = new List<string>(importPaths.ToArray());
                importPaths.Clear();
                themeDialog.ImportThemes(tempImportPaths);
            }
        }

        public static string GetThemeName(ThemeConfig theme)
        {
            return theme.displayName ?? theme.themeId.Replace('_', ' ');
        }

        public static void DisableTheme(string themeId)
        {
            themeSettings.RemoveAll(t => t.themeId == themeId);

            if (currentTheme.themeId == themeId)
            {
                currentTheme = null;
            }

            bool shouldDisable = ThemeLoader.PromptDialog(_("The '{0}' theme could not be " +
                "loaded. Do you want to disable it to prevent this error from happening again?"));

            if (shouldDisable)
            {
                Directory.Move(Path.Combine("themes", themeId),
                    Path.Combine("themes", "." + themeId));
            }
        }

        public static ThemeConfig ImportTheme(string importPath)
        {
            string themeId = Path.GetFileNameWithoutExtension(importPath);
            int themeIndex = themeSettings.FindIndex(t => t.themeId == themeId);

            if (themeIndex != -1)
            {
                bool shouldOverwrite = ThemeLoader.PromptDialog(string.Format(_("The '{0}' " +
                    "theme is already installed. Do you want to overwrite it?"), themeId));

                if (!shouldOverwrite)
                {
                    return null;
                }
            }

            Directory.CreateDirectory(Path.Combine("themes", themeId));

            if (Path.GetExtension(importPath) != ".json")
            {
                ThemeLoader.ExtractTheme(importPath, themeId);
            }
            else
            {
                File.Copy(importPath, Path.Combine("themes", themeId, "theme.json"), true);
            }

            ThemeConfig theme = ThemeLoader.TryLoad(themeId);

            if (theme == null)
            {
                return null;
            }

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

        private static void LoadInstalledThemes(List<string> themeIds)
        {
            foreach (string themeId in themeIds)
            {
                ThemeConfig theme = ThemeLoader.TryLoad(themeId);
                ThemeLoader.HandleError(themeId);

                if (theme != null)
                {
                    themeSettings.Add(theme);

                    if (theme.themeId == JsonConfig.settings.themeName)
                    {
                        currentTheme = theme;
                    }
                }
            }
        }

        private static void DownloadMissingImages()
        {
            List<ThemeConfig> missingThemes = new List<ThemeConfig>();

            foreach (ThemeConfig theme in themeSettings)
            {
                string themePath = Path.Combine("themes", theme.themeId);

                if (!Directory.Exists(themePath) ||
                    (Directory.GetFiles(themePath, theme.imageFilename).Length == 0))
                {
                    missingThemes.Add(theme);
                }
            }

            if (missingThemes.Count == 0)
            {
                filesVerified = true;
                LaunchSequence.NextStep();
                return;
            }

            downloadDialog = new ProgressDialog();
            downloadDialog.FormClosed += OnDownloadDialogClosed;
            downloadDialog.Show();

            MainMenu.themeItem.Enabled = false;
            downloadDialog.InitDownload(missingThemes.FindAll(
                theme => !string.IsNullOrEmpty(theme.imagesZipUri)));  // TODO Handle error if null or empty and missing images
        }

        private static void OnDownloadDialogClosed(object sender, EventArgs e)
        {
            MainMenu.themeItem.Enabled = true;
            filesVerified = true;
            LaunchSequence.NextStep();
        }

        private static void OnThemeDialogClosed(object sender, EventArgs e)
        {
            themeDialog = null;
            LaunchSequence.NextStep(true);
        }
    }
}
