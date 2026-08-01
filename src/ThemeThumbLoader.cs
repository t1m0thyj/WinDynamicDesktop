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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SkiaSharp;

namespace WinDynamicDesktop
{
    class ThemeThumbLoader
    {
        private static List<string> outdatedThemeIds = new List<string>();

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

        public static string GetWindowsWallpaper(bool isLockScreen = false)
        {
            string windowsWallpaperFolder = isLockScreen ? DefaultThemes.windowsLockScreenFolder :
                DefaultThemes.windowsWallpaperFolder;
            string wallpaperPath = null;

            if (Directory.Exists(windowsWallpaperFolder))
            {
                string[] wallpaperFiles = Directory.GetFiles(windowsWallpaperFolder);
                if (wallpaperFiles.Length > 0)
                {
                    wallpaperPath = wallpaperFiles[0];
                }
            }

            return wallpaperPath ?? CreateBlankWallpaper();
        }

        public static Image ScaleImage(string filename, Size size)
        {
            using (Stream stream = File.OpenRead(filename))
            {
                return ScaleImage(stream, size);
            }
        }

        // Images are decoded with SkiaSharp rather than System.Drawing so that formats GDI+ cannot read, such as
        // WebP, are supported here as well as in the theme preview
        private static Image ScaleImage(Stream stream, Size size)
        {
            using (SKCodec codec = SKCodec.Create(stream))
            {
                if (codec == null)
                {
                    throw new ArgumentException("Image could not be decoded because its format is not supported");
                }

                SKImageInfo info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888,
                    SKAlphaType.Premul);

                using (SKBitmap sourceBitmap = new SKBitmap(info))
                {
                    SKCodecResult result = codec.GetPixels(info, sourceBitmap.GetPixels());

                    // Truncated files still decode into a usable image, so only a hard failure is treated as an error
                    if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput)
                    {
                        throw new ArgumentException("Image could not be decoded, result was " + result);
                    }

                    if (sourceBitmap.Width == size.Width && sourceBitmap.Height == size.Height)
                    {
                        return ToBitmap(sourceBitmap);
                    }

                    using (SKBitmap scaledBitmap = new SKBitmap(info.WithSize(size.Width, size.Height)))
                    {
                        sourceBitmap.ScalePixels(scaledBitmap, new SKSamplingOptions(SKCubicResampler.Mitchell));
                        return ToBitmap(scaledBitmap);
                    }
                }
            }
        }

        private static Bitmap ToBitmap(SKBitmap skBitmap)
        {
            Bitmap bmp = new Bitmap(skBitmap.Width, skBitmap.Height, PixelFormat.Format32bppPArgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly,
                bmp.PixelFormat);

            try
            {
                byte[] pixels = skBitmap.Bytes;
                int rowBytes = Math.Min(skBitmap.RowBytes, bmpData.Stride);

                for (int y = 0; y < skBitmap.Height; y++)
                {
                    Marshal.Copy(pixels, y * skBitmap.RowBytes, IntPtr.Add(bmpData.Scan0, y * bmpData.Stride),
                        rowBytes);
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }

            return bmp;
        }

        public static Image GetThumbnailImage(ThemeConfig theme, Size size, bool useCache)
        {
            if (useCache)
            {
                string thumbnailPath = GetThumbnailPath(theme);

                try
                {
                    if (File.Exists(thumbnailPath))
                    {
                        // Scaling instead of discarding a thumbnail whose size does not match keeps a thumbnail
                        // supplied with the theme, and avoids regenerating cached ones whenever the display DPI
                        // makes the requested size something other than 192x108
                        return ScaleImage(thumbnailPath, size);
                    }
                    else if (ThemeManager.defaultThemes.Contains(theme.themeId))
                    {
                        string resourceName = "WinDynamicDesktop.resources.images." + theme.themeId +
                            "_thumbnail.jpg";

                        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                        {
                            return ScaleImage(stream, size);
                        }
                    }
                }
                catch (Exception exc)
                {
                    LoggingHandler.LogMessage("Failed to load cached thumbnail for '{0}' theme, generating a new " +
                        "one: {1}", theme.themeId, exc);
                }
            }

            string themePath = ThemeManager.GetThemeDirectory(theme);
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
                if (item.Tag == null)
                {
                    continue;
                }

                string themeId = (string)item.Tag;

                if (outdatedThemeIds.Contains(themeId))
                {
                    ThemeConfig theme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);
                    Image thumbnailImage = listView.LargeImageList.Images[item.ImageIndex];
                    string thumbnailPath = GetThumbnailPath(theme);

                    Task.Run(new Action(() => thumbnailImage.Save(thumbnailPath, ImageFormat.Png)));
                    outdatedThemeIds.Remove(themeId);
                }
            }
        }

        private static string CreateBlankWallpaper()
        {
            string wallpaperPath = Path.Combine(Path.GetTempPath(), "WinDynamicDesktop_blank_preview.jpg");
            if (!File.Exists(wallpaperPath))
            {
                (new Bitmap(1, 1)).Save(wallpaperPath, ImageFormat.Jpeg);
            }
            return wallpaperPath;
        }

        private static string GetThumbnailPath(ThemeConfig theme)
        {
            return !ThemeManager.IsThemePreinstalled(theme) ? Path.Combine("themes", theme.themeId, "thumbnail.png") :
                Path.Combine(Path.GetTempPath(), "WinDynamicDesktop_" + theme.themeId + "_thumbnail.png");
        }
    }
}
