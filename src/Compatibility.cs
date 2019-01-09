using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace WinDynamicDesktop
{
    class Compatibility
    {
        public static void CompatibilizeThemes()
        {
            if (Directory.Exists("images"))
            {
                UpdateThemeFileStructure();
            }

            UpdateThemeImageLists();
        }

        // TODO Remove after 2.3.0
        private static void UpdateThemeFileStructure()
        {
            List<string> filePaths = Directory.GetFiles("themes", "*.json").ToList();
            filePaths.AddRange(ThemeManager.defaultThemes.Select(
                themeId => Path.Combine("themes", themeId + ".json")));

            foreach (string filePath in filePaths)
            {
                string themeId = Path.GetFileNameWithoutExtension(filePath);
                Directory.CreateDirectory(Path.Combine("themes", themeId));

                if (File.Exists(filePath))
                {
                    File.Move(filePath, Path.Combine("themes", themeId, "theme.json"));
                }

                ThemeConfig theme = JsonConfig.LoadTheme(themeId);
                foreach (string imagePath in Directory.GetFiles("images", theme.imageFilename))
                {
                    File.Move(imagePath,
                        Path.Combine("themes", themeId, Path.GetFileName(imagePath)));
                }
            }

            if (Directory.GetFiles("images").Length == 0 &&
                Directory.GetDirectories("images").Length == 0)
            {
                Directory.Delete("images", false);
            }
        }

        // TODO Remove after 3.0
        private static void UpdateThemeImageLists()
        {
            List<string> upgradeIds = new List<string>() { "BitDay", "Earth_View", "Firewatch",
                "New_York", "San_Francisco", "High_Sierra"};
            foreach (string filePath in Directory.EnumerateFiles("themes", "*.json",
                SearchOption.AllDirectories))
            {
                string themeId = Path.GetFileName(Path.GetDirectoryName(filePath));

                if (!upgradeIds.Contains(themeId))
                {
                    continue;
                }

                string jsonText = File.ReadAllText(filePath);

                if (!jsonText.Contains("sunriseImageList") ||
                    !jsonText.Contains("sunsetImageList"))
                {
                    jsonText = jsonText.Replace("}",
                        ",\"sunriseImageList\":[],\"sunsetImageList\":[]}");
                    ThemeConfig theme = JsonConvert.DeserializeObject<ThemeConfig>(jsonText);

                    if (themeId == "BitDay")
                    {
                        theme.sunriseImageList = new int[] { 12, 1 };
                        theme.dayImageList = new int[] { 2, 3, 4, 5, 6 };
                        theme.sunsetImageList = new int[] { 7, 8 };
                        theme.nightImageList = new int[] { 9, 10, 11 };
                    }
                    else if (themeId == "Earth_View")
                    {
                        theme.sunriseImageList = new int[] { 4, 5, 6 };
                        theme.dayImageList = new int[] { 7, 8, 9, 10, 11 };
                        theme.sunsetImageList = new int[] { 12, 13, 14 };
                        theme.nightImageList = new int[] { 15, 16, 1, 2, 3 };
                    }
                    else if (themeId == "Firewatch")
                    {
                        theme.sunriseImageList = new int[] { 1, 2 };
                        theme.dayImageList = new int[] { 3, 4, 5 };
                        theme.sunsetImageList = new int[] { 6, 7 };
                        theme.nightImageList = new int[] { 8 };
                    }
                    else if (themeId == "New_York")
                    {
                        theme.sunriseImageList = new int[] { 1, 2, 3 };
                        theme.dayImageList = new int[] { 4, 5, 6, 7, 8 };
                        theme.sunsetImageList = new int[] { 9, 10, 11, 12 };
                        theme.nightImageList = new int[] { 13, 14, 15, 16 };
                    }
                    else if (themeId == "San_Francisco")
                    {
                        theme.sunriseImageList = new int[] { 2, 3, 4 };
                        theme.dayImageList = new int[] { 5, 6, 7, 8, 9 };
                        theme.sunsetImageList = new int[] { 10, 16, 11, 12 };
                        theme.nightImageList = new int[] { 13, 14, 15, 1 };
                    }
                    else if (themeId == "High_Sierra")
                    {
                        theme.sunriseImageList = new int[] { 3, 4, 5 };
                        theme.dayImageList = new int[] { 6, 7, 8, 9, 10 };
                        theme.sunsetImageList = new int[] { 9, 1, 12, 13 };
                        theme.nightImageList = new int[] { 14, 15, 16, 2 };
                    }

                    string newJson = JsonConvert.SerializeObject(theme);
                    File.WriteAllText(filePath, newJson);
                }
            }
        }
    }
}
