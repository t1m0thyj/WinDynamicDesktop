// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinDynamicDesktop
{
    class ThemeManager
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        public static List<ThemeConfig> themeSettings = new List<ThemeConfig>();

        public static bool downloadMode = false;
        public static bool importMode = false;
        public static List<string> importPaths;
        public static List<ThemeConfig> importedThemes = new List<ThemeConfig>();

        public static ThemeConfig currentTheme;
        public static string[] defaultThemes;
        private static ThemeDialog themeDialog;

        public static void Initialize()
        {
            Directory.CreateDirectory("themes");
            UpdateHandler.CompatibilizeThemes();

            defaultThemes = DefaultThemes.GetDefaultThemes();
            List<string> themeIds = new List<string>();

            foreach (string filePath in Directory.EnumerateFiles("themes", "*.json", SearchOption.AllDirectories))
            {
                string themeId = Path.GetFileName(Path.GetDirectoryName(filePath));

                if (!themeId.StartsWith("."))
                {
                    themeIds.Add(themeId);
                }
            }

            LoadInstalledThemes(themeIds);
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

        public static string GetThemeAuthor(ThemeConfig theme)
        {
            return IsThemeDownloaded(theme) ? theme.imageCredits : "Apple";
        }

        public static bool IsTheme4Segment(ThemeConfig theme)
        {
            return (!ThemeJsonValidator.IsNullOrEmpty(theme.sunriseImageList) &&
                !ThemeJsonValidator.IsNullOrEmpty(theme.sunsetImageList));
        }

        public static bool IsThemeDownloaded(ThemeConfig theme)
        {
            string themePath = Path.Combine("themes", theme.themeId);
            return (Directory.Exists(themePath) && (Directory.GetFiles(themePath, theme.imageFilename).Length > 0));
        }

        public static void DisableTheme(string themeId, bool permanent)
        {
            themeSettings.RemoveAll(t => t.themeId == themeId);

            if (currentTheme != null && (currentTheme.themeId == themeId))
            {
                currentTheme = null;
            }

            if (permanent)
            {
                Directory.Move(Path.Combine("themes", themeId),
                    Path.Combine("themes", "." + themeId));
            }
        }

        public static ThemeResult ImportTheme(string importPath)
        {
            string themeId = Path.GetFileNameWithoutExtension(importPath);
            int themeIndex = themeSettings.FindIndex(t => t.themeId == themeId);
            ThemeResult result;

            if (Path.GetExtension(importPath) != ".json")
            {
                result = ThemeLoader.ExtractTheme(importPath, themeId);
            }
            else
            {
                result = ThemeLoader.CopyLocalTheme(importPath, themeId);
            }

            return result.Match(e => new ThemeResult(e), theme =>
            {
                if (themeIndex == -1)
                {
                    themeSettings.Add(theme);
                    themeSettings.Sort((t1, t2) => t1.themeId.CompareTo(t2.themeId));
                }
                else
                {
                    themeSettings[themeIndex] = theme;
                }

                return new ThemeResult(theme);
            });
        }

        public static void RemoveTheme(ThemeConfig theme)
        {
            if (currentTheme == theme)
            {
                currentTheme = null;
            }

            if (themeSettings.Contains(theme) && !defaultThemes.Contains(theme.themeId))
            {
                themeSettings.Remove(theme);
            }

            try
            {
                Directory.Delete(Path.Combine("themes", theme.themeId), true);
            }
            catch { /* Do nothing */ }
        }

        private static void LoadInstalledThemes(List<string> themeIds)
        {
            foreach (string themeId in themeIds)
            {
                ThemeLoader.TryLoad(themeId).Match(ThemeLoader.HandleError, theme =>
                {
                    themeSettings.Add(theme);

                    if (theme.themeId == JsonConfig.settings.themeName)
                    {
                        currentTheme = theme;
                    }
                });
            }

            foreach (string themeId in defaultThemes)
            {
                if (!themeIds.Contains(themeId))
                {
                    themeSettings.Add(new ThemeConfig { themeId = themeId });
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