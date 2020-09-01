// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    using ThemeImageData = List<Tuple<int, int>>;

    class ThemePreviewer
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static string[] sunPhases = new string[] { "Sunrise", "Day", "Sunset", "Night" };
        private static string[] translatedSunPhases = new string[] { _("Sunrise"), _("Day"), _("Sunset"), _("Night") };

        public static string GeneratePreviewHtml(ThemeConfig theme)
        {
            string htmlText = Properties.Resources.preview_html;
            Dictionary<string, string> replacers = new Dictionary<string, string>();
            replacers.Add("basePath", new Uri(Environment.CurrentDirectory).AbsoluteUri);

            if (theme != null)
            {
                replacers.Add("themeName", ThemeManager.GetThemeName(theme));
                replacers.Add("themeAuthor", ThemeManager.GetThemeAuthor(theme));
                replacers.Add("previewMessage", string.Format(_("Previewing {0}"), "<span id=\"previewText\"></span>"));

                SolarData solarData = SunriseSunsetService.GetSolarData(DateTime.Today);
                SchedulerState wpState = AppContext.wpEngine.GetImageData(solarData, theme);

                if (ThemeManager.IsThemeDownloaded(theme))
                {
                    // TODO Why are images flickering?
                    ThemeImageData imageData = GetThemeImageData(theme);
                    int activeImage = imageData.FindIndex(entry => entry.Item2 == wpState.daySegment4) +
                        wpState.imageNumber;

                    replacers.Add("downloadMessage", "");
                    replacers.Add("carouselIndicators", GetCarouselIndicators(imageData.Count, activeImage));
                    replacers.Add("carouselItems", GetCarouselItems(imageData, activeImage, theme));
                }
                else
                {
                    replacers.Add("downloadMessage", string.Format("<div id=\"bottomCenterPanel\">{0}</div>",
                        _("Theme is not downloaded. Click Download button to enable full preview.")));
                    replacers.Add("carouselIndicators", "");
                    replacers.Add("carouselItems", GetCarouselItems(wpState, theme));
                }
            }
            else
            {
                replacers.Add("themeAuthor", "Microsoft");

                int startCarouselIndex = htmlText.IndexOf("<!--");
                int endCarouselIndex = htmlText.LastIndexOf("-->") + 3;
                string imageTag = string.Format("<img src=\"{0}\">",
                    (new Uri(ThemeThumbLoader.GetWindowsWallpaper())).AbsoluteUri);

                htmlText = htmlText.Substring(0, startCarouselIndex) + imageTag +
                    htmlText.Substring(endCarouselIndex + 1);
            }

            return RenderTemplate(htmlText, replacers);
        }

        private static string GetCarouselIndicators(int imageCount, int activeImage)
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < imageCount; i++)
            {
                if (i == activeImage)
                {
                    lines.Add(string.Format(
                        "<li data-target=\"#myCarousel\" data-slide-to=\"{0}\" class=\"active\"></li>", i));
                }
                else
                {
                    lines.Add(string.Format("<li data-target=\"#myCarousel\" data-slide-to=\"{0}\"></li>", i));
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static string GetCarouselItems(ThemeImageData imageData, int activeImage, ThemeConfig theme)
        {
            List<string> lines = new List<string>();
            int[] phaseNumbers = new int[4] { 0, 0, 0, 0 };
            int[] phaseTotals = new int[4] { 0, 0, 0, 0 };

            foreach (Tuple<int, int> entry in imageData)
            {
                phaseTotals[entry.Item2]++;
            }

            for (int i = 0; i < imageData.Count; i++)
            {
                if (i == activeImage)
                {
                    lines.Add("<div class=\"carousel-item active\">");
                }
                else
                {
                    lines.Add("<div class=\"carousel-item\">");
                }

                string imageFilename = theme.imageFilename.Replace("*", imageData[i].Item1.ToString());
                string imagePath = Path.Combine("themes", theme.themeId, imageFilename).Replace(@"\", "/");
                int sunPhase = imageData[i].Item2;
                phaseNumbers[sunPhase]++;
                string altText = string.Format("{0} ({1}/{2})", translatedSunPhases[sunPhase], phaseNumbers[sunPhase],
                    phaseTotals[sunPhase]);
                lines.Add(string.Format("  <img src=\"{0}\" alt=\"{1}\">", imagePath, altText));
                lines.Add("</div>");
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static string GetCarouselItems(SchedulerState wpState, ThemeConfig theme)
        {
            List<string> lines = new List<string>();
            int imageCount = Directory.EnumerateFiles(Path.Combine("assets", "images"),
                theme.themeId + "_*.jpg").Count();
            int activeImage = (imageCount == 2) ? (wpState.daySegment2 * 2 + 1) : wpState.daySegment4;

            for (int i = 0; i < sunPhases.Length; i++)
            {
                if (imageCount == 2 && i % 2 == 0)
                {
                    continue;
                }

                if (i == activeImage)
                {
                    lines.Add("<div class=\"carousel-item active\">");
                }
                else
                {
                    lines.Add("<div class=\"carousel-item\">");
                }

                string imageFilename = theme.themeId + "_" + sunPhases[i].ToLower() + ".jpg";
                string imagePath = Path.Combine("assets", "images", imageFilename).Replace(@"\", "/");
                lines.Add(string.Format("  <img src=\"{0}\" alt=\"{1}\">", imagePath, translatedSunPhases[i]));
                lines.Add("</div>");
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static ThemeImageData GetThemeImageData(ThemeConfig theme)
        {
            ThemeImageData imageData = new ThemeImageData();

            if (!theme.sunriseImageList.SequenceEqual(theme.dayImageList))
            {
                foreach (int imageId in theme.sunriseImageList)
                {
                    imageData.Add(Tuple.Create(imageId, 0));
                }
            }

            foreach (int imageId in theme.dayImageList)
            {
                imageData.Add(Tuple.Create(imageId, 1));
            }

            if (!theme.sunsetImageList.SequenceEqual(theme.dayImageList))
            {
                foreach (int imageId in theme.sunsetImageList)
                {
                    imageData.Add(Tuple.Create(imageId, 2));
                }
            }

            foreach (int imageId in theme.nightImageList)
            {
                imageData.Add(Tuple.Create(imageId, 3));
            }

            return imageData;
        }

        private static string RenderTemplate(string template, Dictionary<string, string> replacers)
        {
            foreach (KeyValuePair<string, string> replacer in replacers)
            {
                template = template.Replace("{{" + replacer.Key + "}}", replacer.Value);
            }

            return template;
        }
    }
}
