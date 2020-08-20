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
        private static readonly Func<string, string> _ = Localization.GetTranslation;

        public static string GeneratePreviewHtml(ThemeConfig theme)
        {
            string htmlText = Properties.Resources.preview_html;

            if (theme != null)
            {
                htmlText = htmlText.Replace("{{themeName}}", ThemeManager.GetThemeName(theme));
                htmlText = htmlText.Replace("{{themeAuthor}}", ThemeManager.GetThemeAuthor(theme));

                List<int> imageList = ThemeManager.GetThemeImageList(theme);
                SolarData solarData = SunriseSunsetService.GetSolarData(DateTime.Today);
                int activeImage = imageList.IndexOf(AppContext.wpEngine.GetImageData(solarData, theme).imageId) + 1;

                htmlText = htmlText.Replace("{{carouselIndicators}}", GetCarouselIndicators(imageList, activeImage));
                htmlText = htmlText.Replace("{{carouselItems}}", GetCarouselItems(imageList, activeImage, theme));
            }
            else
            {
                htmlText = htmlText.Replace("{{themeName}}", _("Default Wallpaper"));
                htmlText = htmlText.Replace("{{themeAuthor}}", _("Microsoft"));

                int startCarouselIndex = htmlText.IndexOf("<!--");
                int endCarouselIndex = htmlText.LastIndexOf("-->") + 3;
                string imageTag = string.Format("<img src=\"{0}\">",
                    (new Uri(ThemeThumbLoader.GetWindowsWallpaper())).AbsoluteUri);

                htmlText = htmlText.Substring(0, startCarouselIndex) + imageTag +
                    htmlText.Substring(endCarouselIndex + 1);
            }

            return htmlText;
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
                string imagePath = Path.Combine(Environment.CurrentDirectory, "themes", theme.themeId, imageFilename);
                string altText = string.Format(_("Image {0} of {1}"), i + 1, imageList.Count);
                lines.Add(string.Format("  <img src=\"{0}\" alt=\"{1}\">", (new Uri(imagePath)).AbsoluteUri, altText));
                lines.Add("</div>");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
