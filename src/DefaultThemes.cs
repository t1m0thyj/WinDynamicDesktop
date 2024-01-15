// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WinDynamicDesktop
{
    class DefaultThemes
    {
        private static string[] yamlLines = Array.Empty<string>();
        public static string windowsWallpaperFolder = Environment.ExpandEnvironmentVariables(
            @"%SystemRoot%\Web\Wallpaper\Windows");

        public static string[] GetDefaultThemes()
        {
            string yamlText = Properties.Resources.DefaultThemesYaml;
            yamlLines = yamlText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select((line) => line.Trim())
                .Where((line) => !line.StartsWith("#")).ToArray();

            return yamlLines.Where((line) => !line.StartsWith("-"))
                .Select((line) => line.TrimEnd(':')).ToArray();
        }

        public static ThemeConfig GetWindowsTheme()
        {
            if (Environment.OSVersion.Version.Build >= 22000 && Directory.Exists(windowsWallpaperFolder))
            {
                return new ThemeConfig
                {
                    themeId = "Windows_11",
                    imageFilename = "img*.jpg",
                    imageCredits = "Microsoft",
                    dayImageList = new[] { 0 },
                    nightImageList = new[] { 19 }
                };
            }

            return null;
        }

        public static Uri[] GetThemeUriList(string themeId)
        {
            int startIndex = Array.FindIndex(yamlLines, (line) => line == themeId + ":") + 1;
            List<Uri> uriList = new List<Uri>();

            while ((startIndex < yamlLines.Length) && yamlLines[startIndex].StartsWith("- "))
            {
                uriList.Add(new Uri(yamlLines[startIndex].Substring(2)));
                startIndex++;
            }

            return uriList.ToArray();
        }

        public static void InstallWindowsTheme(ThemeConfig theme)
        {
            string themePath = Path.Combine("themes", theme.themeId);
            Directory.CreateDirectory(themePath);
            string jsonText = JsonConvert.SerializeObject(theme, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            });
            File.WriteAllText(Path.Combine(themePath, "theme.json"), jsonText);
            string[] imagePaths = Directory.GetFiles(windowsWallpaperFolder, theme.imageFilename);
            foreach (string imagePath in imagePaths)
            {
                File.Copy(imagePath, Path.Combine(themePath, Path.GetFileName(imagePath)), true);
            }
        }
    }
}
