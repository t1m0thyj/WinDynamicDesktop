// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using WinDynamicDesktop.COM;

namespace WinDynamicDesktop
{
    internal static class SpannedWallpaperManager
    {
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);
        private static readonly string cacheDirectory =
            Path.Combine(Path.GetFullPath("cache"), "virtual-desktop-wallpaper");
        private static readonly object wallpaperLock = new object();

        [DllImport("user32.dll")]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr dpiContext);

        internal static void Update(IReadOnlyList<string> imagePaths, bool usePerDisplayThemes)
        {
            lock (wallpaperLock)
            {
                UpdateCore(imagePaths, usePerDisplayThemes);
            }
        }

        private static void UpdateCore(IReadOnlyList<string> imagePaths, bool usePerDisplayThemes)
        {
            if (!CanApply(imagePaths, usePerDisplayThemes))
            {
                RestoreWallpaperPosition();
                return;
            }

            if (!imagePaths.All(path => !string.IsNullOrEmpty(path) && File.Exists(path)))
            {
                LoggingHandler.LogMessage("Skipping virtual desktop wallpaper sync because an image is missing");
                RestoreWallpaperPosition();
                return;
            }

            Apply(imagePaths);
        }

        private static bool CanApply(IReadOnlyList<string> imagePaths, bool usePerDisplayThemes)
        {
            return JsonConfig.settings.syncVirtualDesktopWallpapers && usePerDisplayThemes &&
                imagePaths != null && imagePaths.Count > 1 && VirtualDesktopWallpaperApi.IsSupported;
        }

        private static void Apply(IReadOnlyList<string> imagePaths)
        {
            IDesktopWallpaper desktopWallpaper = null;
            List<string> monitorIds = new List<string>();
            try
            {
                desktopWallpaper = DesktopWallpaperFactory.Create();
                List<WallpaperMonitor> monitors = GetMonitors(desktopWallpaper, imagePaths, monitorIds);
                DesktopWallpaperPosition sourcePosition = SaveWallpaperPosition(desktopWallpaper);
                COLORREF color = desktopWallpaper.GetBackgroundColor();
                string outputPath = SpannedWallpaperRenderer.RenderToCache(monitors, sourcePosition,
                    new SKColor(color.R, color.G, color.B), cacheDirectory);

                desktopWallpaper.SetPosition(DesktopWallpaperPosition.DWPOS_SPAN);
                desktopWallpaper.SetWallpaper(null, outputPath);
                if (!VirtualDesktopWallpaperApi.TrySetWallpaperForAllDesktops(outputPath))
                {
                    RestoreCurrentDesktop(desktopWallpaper, monitorIds, imagePaths, sourcePosition);
                    return;
                }

                LoggingHandler.LogMessage("Set spanned wallpaper for all virtual desktops: {0}", outputPath);
                SpannedWallpaperRenderer.CleanCache(cacheDirectory, outputPath);
            }
            catch (Exception exc)
            {
                LoggingHandler.LogMessage("Error syncing virtual desktop wallpapers: {0}", exc.ToString());
                LoggingHandler.LogError(exc);
                RestoreAfterFailure(desktopWallpaper, monitorIds, imagePaths);
            }
            finally
            {
                ReleaseComObject(desktopWallpaper);
            }
        }

        private static List<WallpaperMonitor> GetMonitors(IDesktopWallpaper desktopWallpaper,
            IReadOnlyList<string> imagePaths, List<string> monitorIds)
        {
            uint monitorCount = desktopWallpaper.GetMonitorDevicePathCount();
            if (monitorCount < (uint)imagePaths.Count)
            {
                RestoreWallpaperPosition(desktopWallpaper);
                throw new InvalidOperationException(string.Format("Windows returned {0} monitors but {1} images " +
                    "are configured", monitorCount, imagePaths.Count));
            }

            List<WallpaperMonitor> monitors = new List<WallpaperMonitor>();
            IntPtr previousDpiContext = SetThreadDpiAwarenessContext(
                DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            try
            {
                for (int i = 0; i < imagePaths.Count; i++)
                {
                    string monitorId = desktopWallpaper.GetMonitorDevicePathAt((uint)i);
                    monitorIds.Add(monitorId);
                    monitors.Add(new WallpaperMonitor
                    {
                        Bounds = desktopWallpaper.GetMonitorRECT(monitorId),
                        ImagePath = imagePaths[i]
                    });
                }
            }
            finally
            {
                if (previousDpiContext != IntPtr.Zero)
                {
                    SetThreadDpiAwarenessContext(previousDpiContext);
                }
            }

            return monitors;
        }

        private static DesktopWallpaperPosition SaveWallpaperPosition(IDesktopWallpaper desktopWallpaper)
        {
            DesktopWallpaperPosition currentPosition = desktopWallpaper.GetPosition();
            if (JsonConfig.settings.wallpaperPositionBeforeVirtualDesktopSync < 0)
            {
                JsonConfig.settings.wallpaperPositionBeforeVirtualDesktopSync = (int)currentPosition;
            }

            return GetSavedPosition();
        }

        private static void RestoreAfterFailure(IDesktopWallpaper desktopWallpaper,
            IReadOnlyList<string> monitorIds, IReadOnlyList<string> imagePaths)
        {
            if (desktopWallpaper != null && monitorIds.Count == imagePaths.Count)
            {
                RestoreCurrentDesktop(desktopWallpaper, monitorIds, imagePaths, GetSavedPosition());
            }
            else
            {
                RestoreWallpaperPosition(desktopWallpaper);
            }
        }

        internal static void RestoreWallpaperPosition()
        {
            lock (wallpaperLock)
            {
                RestoreWallpaperPosition(null);
            }
        }

        private static void RestoreWallpaperPosition(IDesktopWallpaper existingDesktopWallpaper)
        {
            if (JsonConfig.settings.wallpaperPositionBeforeVirtualDesktopSync < 0)
            {
                return;
            }

            IDesktopWallpaper desktopWallpaper = existingDesktopWallpaper;
            try
            {
                desktopWallpaper ??= DesktopWallpaperFactory.Create();
                if (desktopWallpaper.GetPosition() == DesktopWallpaperPosition.DWPOS_SPAN)
                {
                    desktopWallpaper.SetPosition(GetSavedPosition());
                }
                JsonConfig.settings.wallpaperPositionBeforeVirtualDesktopSync = -1;
            }
            catch (Exception exc)
            {
                LoggingHandler.LogMessage("Could not restore wallpaper position: {0}", exc.ToString());
                LoggingHandler.LogError(exc);
            }
            finally
            {
                if (existingDesktopWallpaper == null && desktopWallpaper != null &&
                    Marshal.IsComObject(desktopWallpaper))
                {
                    Marshal.ReleaseComObject(desktopWallpaper);
                }
            }
        }

        private static void RestoreCurrentDesktop(IDesktopWallpaper desktopWallpaper,
            IReadOnlyList<string> monitorIds, IReadOnlyList<string> imagePaths,
            DesktopWallpaperPosition position)
        {
            try
            {
                desktopWallpaper.SetPosition(position);
                for (int i = 0; i < monitorIds.Count; i++)
                {
                    desktopWallpaper.SetWallpaper(monitorIds[i], imagePaths[i]);
                }
            }
            catch (Exception exc)
            {
                LoggingHandler.LogMessage("Could not restore per-monitor wallpapers: {0}", exc.ToString());
                LoggingHandler.LogError(exc);
            }
        }

        private static void ReleaseComObject(object value)
        {
            if (value != null && Marshal.IsComObject(value))
            {
                Marshal.ReleaseComObject(value);
            }
        }

        private static DesktopWallpaperPosition GetSavedPosition()
        {
            int position = JsonConfig.settings.wallpaperPositionBeforeVirtualDesktopSync;
            return Enum.IsDefined(typeof(DesktopWallpaperPosition), position) ?
                (DesktopWallpaperPosition)position : DesktopWallpaperPosition.DWPOS_FILL;
        }
    }
}
