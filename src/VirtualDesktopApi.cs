// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;
using WindowsDesktop;

namespace WinDynamicDesktop
{
    public class VirtualDesktopApi
    {
        // TODO Catch current changed event for VirtualDesktop
        private static bool isInitialized = false;

        public static void SetWallpaper(string imagePath)
        {
            for (int attempts = 0; attempts < 2; attempts++)
            {
                if (!isInitialized || attempts > 0)
                {
                    // TODO Should VirtualDesktop init be done earlier?
                    VirtualDesktop.Configure();
                    isInitialized = true;
                }

                try
                {
                    UnsafeSetWallpaper(imagePath);
                    break;
                }
                catch (COMException)
                {
                    continue;
                }
            }
        }

        private static void UnsafeSetWallpaper(string imagePath)
        {
            Guid currentDesktopId = VirtualDesktop.Current.Id;
            foreach (VirtualDesktop virtualDesktop in VirtualDesktop.GetDesktops())
            {
                if (virtualDesktop.Id != currentDesktopId)
                {
                    virtualDesktop.WallpaperPath = imagePath;
                }
            }
        }
    }
}
