// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;
using WinDynamicDesktop.COM;

namespace WinDynamicDesktop
{
    public class VirtualDesktopApi
    {
        private static IVirtualDesktopManagerInternal manager;

        public static void SetWallpaper(string imagePath)
        {
            for (int attempts = 0; attempts < 2; attempts++)
            {
                if (manager == null || attempts > 0)
                {
                    manager = ImmersiveShellWrapper.GetVirtualDesktopManager();
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
            Guid currentDesktopId = manager.GetCurrentDesktop(IntPtr.Zero).GetId();
            IObjectArray objectArray = manager.GetDesktops(IntPtr.Zero);

            for (uint i = 0u; i < objectArray.GetCount(); i++)
            {
                objectArray.GetAt(i, typeof(IVirtualDesktop).GUID, out object virtualDesktop);
                if ((virtualDesktop as IVirtualDesktop).GetId() != currentDesktopId)
                {
                    manager.SetWallpaperPath((IVirtualDesktop)virtualDesktop, imagePath);
                }
            }
        }
    }
}
