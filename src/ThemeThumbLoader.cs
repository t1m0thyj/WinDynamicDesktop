// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    class ThemeThumbLoader
    {
        private static List<string> outdatedThemeIds = new List<string>();
        private static string windowsWallpaperFolder = Environment.ExpandEnvironmentVariables(
            @"%SystemRoot%\Web\Wallpaper\Windows");

        public static Size GetThumbnailSize(System.Windows.Forms.Control control)
        {
            int scaledWidth;

            using (Graphics g = control.CreateGraphics())
            {
                scaledWidth = (int)(192 * g.DpiX / 96);
            }

            if (scaledWidth > 256)
            {
                scaledWidth = 256;
            }

            return new Size(scaledWidth, scaledWidth * 9 / 16);
        }

        public static string GetWindowsWallpaper()
        {
            string wallpaperPath = null;

            if (Directory.Exists(windowsWallpaperFolder))
            {
                string[] wallpaperFiles = Directory.GetFiles(windowsWallpaperFolder);
                if (wallpaperFiles.Length > 0)
                {
                    wallpaperPath = wallpaperFiles[0];
                }
            }

            if (wallpaperPath == null)
            {
                wallpaperPath = Path.Combine(Environment.CurrentDirectory, "wallpaper_blank.jpg");
                if (!File.Exists(wallpaperPath))
                {
                    (new Bitmap(1, 1)).Save(wallpaperPath, ImageFormat.Jpeg);
                }
            }

            return wallpaperPath;
        }

        public static Image ScaleImage(Image tempImage, Size size)
        {
            if (tempImage.Size == size)
            {
                return tempImage;
            }

            // Image scaling code from https://stackoverflow.com/a/7677163/5504760
            using (tempImage)
            {
                Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(tempImage, new Rectangle(0, 0, bmp.Width, bmp.Height));
                }

                return bmp;
            }
        }

        public static Image ScaleImage(string filename, Size size)
        {
            return ScaleImage(Image.FromFile(filename), size);
        }

        public static Image GetThumbnailImage(ThemeConfig theme, Size size, bool useCache)
        {
            string themePath = Path.Combine("themes", theme.themeId);
            string thumbnailPath = Path.Combine(themePath, "thumbnail.png");

            if (useCache)
            {
                if (File.Exists(thumbnailPath))
                {
                    Image cachedImage = Image.FromFile(thumbnailPath);

                    if (cachedImage.Size == size)
                    {
                        return cachedImage;
                    }
                    else
                    {
                        cachedImage.Dispose();
                    }
                }
                else if (ThemeManager.defaultThemes.Contains(theme.themeId))
                {
                    string resourceName = "WinDynamicDesktop.resources.images." + theme.themeId + "_thumbnail.jpg";

                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    {
                        return ScaleImage(Image.FromStream(stream), size);
                    }
                }
            }

            int imageId1 = theme.dayHighlight ?? theme.dayImageList[theme.dayImageList.Length / 2];
            int imageId2 = theme.nightHighlight ?? theme.nightImageList[theme.nightImageList.Length / 2];
            string imageFilename1 = theme.imageFilename.Replace("*", imageId1.ToString());
            string imageFilename2 = theme.imageFilename.Replace("*", imageId2.ToString());

            using (var bmp1 = ScaleImage(Path.Combine(themePath, imageFilename1), size))
            {
                Bitmap bmp2 = (Bitmap)ScaleImage(Path.Combine(themePath, imageFilename2), size);

                using (Graphics g = Graphics.FromImage(bmp2))
                {
                    g.DrawImage(bmp1, 0, 0, new Rectangle(0, 0, bmp1.Width / 2, bmp1.Height), GraphicsUnit.Pixel);
                }

                outdatedThemeIds.Add(theme.themeId);
                return bmp2;
            }
        }

        public static void CacheThumbnails(System.Windows.Forms.ListView listView)
        {
            foreach (System.Windows.Forms.ListViewItem item in listView.Items)
            {
                if (item.Index == 0)
                {
                    continue;
                }

                string themeId = (string)item.Tag;

                if (outdatedThemeIds.Contains(themeId))
                {
                    ThemeConfig theme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);
                    Image thumbnailImage = listView.LargeImageList.Images[item.ImageIndex];
                    string thumbnailPath = Path.Combine("themes", themeId, "thumbnail.png");

                    Task.Run(new Action(() => thumbnailImage.Save(thumbnailPath, ImageFormat.Png)));
                    outdatedThemeIds.Remove(themeId);
                }
            }
        }
    }
}
