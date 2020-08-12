using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    class ThemePreviewer
    {
        public static void LaunchPreview(ThemeConfig theme, int activeImage)
        {
            System.Diagnostics.Process.Start(GeneratePreviewHtml(theme, activeImage));
        }

        private static string GeneratePreviewHtml(ThemeConfig theme, int activeImage)
        {
            string htmlText = Properties.Resources.preview_html;
            htmlText = htmlText.Replace("{{themeName}}", ThemeManager.GetThemeName(theme));
            htmlText = htmlText.Replace("{{themeAuthor}}", ThemeManager.GetThemeAuthor(theme));

            List<int> imageList = ThemeManager.GetThemeImageList(theme);
            htmlText = htmlText.Replace("{{carouselIndicators}}", GetCarouselIndicators(imageList, activeImage));
            htmlText = htmlText.Replace("{{carouselItems}}", GetCarouselItems(imageList, activeImage, theme));

            string filename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".html");
            File.WriteAllText(filename, htmlText);
            return filename;
        }

        private static string GetCarouselIndicators(List<int> imageList, int activeImage)
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < imageList.Count; i++)
            {
                if (i + 1 == activeImage)
                {
                    lines.Add(string.Format("<li data-target=\"#demo\" data-slide-to=\"{0}\" class=\"active\"></li>",
                        i));
                }
                else
                {
                    lines.Add(string.Format("<li data-target=\"#demo\" data-slide-to=\"{0}\"></li>", i));
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static string GetCarouselItems(List<int> imageList, int activeImage, ThemeConfig theme)
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < imageList.Count; i++)
            {
                if (i + 1 == activeImage)
                {
                    lines.Add("<div class=\"carousel-item active\">");
                }
                else
                {
                    lines.Add("<div class=\"carousel-item\">");
                }

                string imageFilename = theme.imageFilename.Replace("*", imageList[i].ToString());
                Uri uri = new Uri(Path.Combine(Environment.CurrentDirectory, "themes", theme.themeId, imageFilename));
                lines.Add(string.Format("  <img src=\"{0}\" alt=\"Image {1} of {2}\">", uri.AbsoluteUri, i + 1,
                    imageList.Count));
                lines.Add("</div>");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
