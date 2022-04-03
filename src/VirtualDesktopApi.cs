// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using WindowsDesktop;

namespace WinDynamicDesktop
{
    class VirtualDesktopApi
    {
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (UwpDesktop.IsVirtualDesktopSupported())
            {
                try
                {
                    // TODO How to clean up Explorer listener if configure fails
                    VirtualDesktop.Configure();
                    VirtualDesktop.CurrentChanged += OnVirtualDesktopCurrentChanged;
                    isInitialized = true;
                }
                catch { /* Do nothing */ }
            }
        }

        public static void SetWallpaper(string imagePath)
        {
            if (!isInitialized || JsonConfig.settings.activeThemes[0] == null)
            {
                return;
            }

            Guid currentDesktopId = VirtualDesktop.Current.Id;
            foreach (VirtualDesktop virtualDesktop in VirtualDesktop.GetDesktops())
            {
                if (virtualDesktop.Id != currentDesktopId)
                {
                    virtualDesktop.WallpaperPath = imagePath;
                }
            }
        }

        private static void OnVirtualDesktopCurrentChanged(object sender, VirtualDesktopChangedEventArgs e)
        {
            if (!isInitialized || JsonConfig.settings.activeThemes[0] == null)
            {
                foreach (DisplayEvent de in AppContext.wpEngine.displayEvents)
                {
                    if (de.lastImagePath != null)
                    {
                        UwpDesktop.GetHelper().SetWallpaper(de.lastImagePath, de.displayIndex);
                    }
                }
            }
        }
    }
}
