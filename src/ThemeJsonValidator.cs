using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinDynamicDesktop
{
    class ThemeJsonValidator
    {
        private static bool IsNullOrEmpty(Array array)
        {
            return (array == null || array.Length == 0);
        }

        public static ThemeResult ValidateQuick(ThemeConfig theme)
        {
            if (string.IsNullOrEmpty(theme.imageFilename) || IsNullOrEmpty(theme.sunriseImageList) ||
                IsNullOrEmpty(theme.dayImageList) || IsNullOrEmpty(theme.sunsetImageList) ||
                IsNullOrEmpty(theme.nightImageList))
            {
                return new ThemeResult(new MissingFieldsInThemeJSON(theme.themeId));
            }

            return new ThemeResult(theme);
        }

        public static ThemeResult ValidateFull(ThemeConfig theme)
        {
            string themePath = Path.Combine("themes", theme.themeId);

            if (Directory.GetFiles(themePath, theme.imageFilename).Length == 0)
            {
                return new ThemeResult(new NoImagesMatchingPattern(theme.themeId, theme.imageFilename));
            }

            foreach (int imageId in ThemeManager.GetThemeImageList(theme))
            {
                string imageFilename = theme.imageFilename.Replace("*", imageId.ToString());
                if (!File.Exists(Path.Combine(themePath, imageFilename)))
                {
                    return new ThemeResult(new InvalidImageInThemeJSON(theme.themeId, imageId, imageFilename));
                }
            }

            return new ThemeResult(theme);
        }
    }
}
