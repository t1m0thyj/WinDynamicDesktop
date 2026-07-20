// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WinDynamicDesktop.COM;

namespace WinDynamicDesktop
{
    internal static class SpannedWallpaperRenderer
    {
        private static readonly SKSamplingOptions sampling =
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);

        internal static RECT GetVirtualBounds(IReadOnlyList<WallpaperMonitor> monitors)
        {
            if (monitors == null || monitors.Count == 0)
            {
                throw new ArgumentException("At least one monitor is required", nameof(monitors));
            }

            return new RECT
            {
                Left = monitors.Min(m => m.Bounds.Left),
                Top = monitors.Min(m => m.Bounds.Top),
                Right = monitors.Max(m => m.Bounds.Right),
                Bottom = monitors.Max(m => m.Bounds.Bottom)
            };
        }

        internal static SKBitmap Render(IReadOnlyList<WallpaperMonitor> monitors,
            DesktopWallpaperPosition position, SKColor backgroundColor)
        {
            RECT virtualBounds = GetVirtualBounds(monitors);
            int width = checked(virtualBounds.Right - virtualBounds.Left);
            int height = checked(virtualBounds.Bottom - virtualBounds.Top);
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("The monitor layout has invalid bounds", nameof(monitors));
            }
            if ((width * (long)height) > 100_000_000)
            {
                throw new ArgumentException("The monitor layout is too large to render safely", nameof(monitors));
            }

            SKBitmap output = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888,
                SKAlphaType.Opaque));
            using SKCanvas canvas = new SKCanvas(output);
            canvas.Clear(backgroundColor);

            foreach (WallpaperMonitor monitor in monitors)
            {
                using SKBitmap source = SKBitmap.Decode(monitor.ImagePath) ??
                    throw new InvalidDataException("Could not decode wallpaper image: " + monitor.ImagePath);
                SKRect destination = new SKRect(
                    monitor.Bounds.Left - virtualBounds.Left,
                    monitor.Bounds.Top - virtualBounds.Top,
                    monitor.Bounds.Right - virtualBounds.Left,
                    monitor.Bounds.Bottom - virtualBounds.Top);

                canvas.Save();
                canvas.ClipRect(destination);
                DrawImage(canvas, source, destination, position);
                canvas.Restore();
            }

            return output;
        }

        internal static string RenderToCache(IReadOnlyList<WallpaperMonitor> monitors,
            DesktopWallpaperPosition position, SKColor backgroundColor, string cacheDirectory)
        {
            Directory.CreateDirectory(cacheDirectory);
            string cacheKey = GetCacheKey(monitors, position, backgroundColor);
            string outputPath = Path.Combine(cacheDirectory, "spanned-" + cacheKey + ".jpg");
            if (File.Exists(outputPath))
            {
                return outputPath;
            }

            string tempPath = outputPath + ".tmp";
            using (SKBitmap bitmap = Render(monitors, position, backgroundColor))
            using (SKImage image = SKImage.FromBitmap(bitmap))
            using (SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 95))
            using (FileStream stream = File.Create(tempPath))
            {
                data.SaveTo(stream);
            }

            File.Move(tempPath, outputPath, true);
            return outputPath;
        }

        internal static void CleanCache(string cacheDirectory, string currentPath, int filesToKeep = 4)
        {
            try
            {
                foreach (FileInfo file in new DirectoryInfo(cacheDirectory).EnumerateFiles("spanned-*.jpg")
                    .OrderByDescending(f => f.LastWriteTimeUtc).Skip(filesToKeep))
                {
                    if (!string.Equals(file.FullName, currentPath, StringComparison.OrdinalIgnoreCase))
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception exc)
            {
                LoggingHandler.LogMessage("Could not clean spanned wallpaper cache: {0}", exc.Message);
            }
        }

        private static void DrawImage(SKCanvas canvas, SKBitmap source, SKRect monitor,
            DesktopWallpaperPosition position)
        {
            switch (position)
            {
                case DesktopWallpaperPosition.DWPOS_CENTER:
                    float left = monitor.MidX - (source.Width / 2f);
                    float top = monitor.MidY - (source.Height / 2f);
                    canvas.DrawBitmap(source, left, top, sampling, null);
                    break;

                case DesktopWallpaperPosition.DWPOS_TILE:
                    for (float y = monitor.Top; y < monitor.Bottom; y += source.Height)
                    {
                        for (float x = monitor.Left; x < monitor.Right; x += source.Width)
                        {
                            canvas.DrawBitmap(source, x, y, sampling, null);
                        }
                    }
                    break;

                case DesktopWallpaperPosition.DWPOS_STRETCH:
                    canvas.DrawBitmap(source, monitor, sampling, null);
                    break;

                case DesktopWallpaperPosition.DWPOS_FIT:
                    DrawScaled(canvas, source, monitor, Math.Min(
                        monitor.Width / source.Width, monitor.Height / source.Height));
                    break;

                case DesktopWallpaperPosition.DWPOS_FILL:
                case DesktopWallpaperPosition.DWPOS_SPAN:
                default:
                    DrawScaled(canvas, source, monitor, Math.Max(
                        monitor.Width / source.Width, monitor.Height / source.Height));
                    break;
            }
        }

        private static void DrawScaled(SKCanvas canvas, SKBitmap source, SKRect monitor, float scale)
        {
            float width = source.Width * scale;
            float height = source.Height * scale;
            SKRect destination = SKRect.Create(monitor.MidX - (width / 2f), monitor.MidY - (height / 2f),
                width, height);
            canvas.DrawBitmap(source, destination, sampling, null);
        }

        private static string GetCacheKey(IReadOnlyList<WallpaperMonitor> monitors,
            DesktopWallpaperPosition position, SKColor backgroundColor)
        {
            StringBuilder value = new StringBuilder();
            value.Append((int)position).Append('|').Append(backgroundColor).Append('|');
            foreach (WallpaperMonitor monitor in monitors)
            {
                FileInfo file = new FileInfo(monitor.ImagePath);
                value.Append(monitor.Bounds.Left).Append(',').Append(monitor.Bounds.Top).Append(',')
                    .Append(monitor.Bounds.Right).Append(',').Append(monitor.Bounds.Bottom).Append('|')
                    .Append(file.FullName).Append('|').Append(file.Length).Append('|')
                    .Append(file.LastWriteTimeUtc.Ticks).Append(';');
            }

            byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(value.ToString()));
            return Convert.ToHexString(digest)[..16].ToLowerInvariant();
        }
    }
}
