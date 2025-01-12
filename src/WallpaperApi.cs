// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using WinDynamicDesktop.COM;

namespace WinDynamicDesktop
{
    public class WallpaperApi
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags,
            uint uTimeout, out IntPtr result);

        public static void EnableTransitions()
        {
            IntPtr result = IntPtr.Zero;
            SendMessageTimeout(FindWindow("Progman", null), 0x52c, IntPtr.Zero, IntPtr.Zero, 0, 500, out result);
        }

        public static void RefreshDesktop()
        {
            IntPtr result = IntPtr.Zero;
            SendMessageTimeout(FindWindow("Progman", null), 0x001a, IntPtr.Zero, IntPtr.Zero, 0, 500, out result);
        }

        public static void SetWallpaper(string imagePath, int displayIndex = -1)
        {
            EnableTransitions();

            if (displayIndex != -1)
            {
                IDesktopWallpaper desktopWallpaper = DesktopWallpaperFactory.Create();
                string monitorId = desktopWallpaper.GetMonitorDevicePathAt((uint)displayIndex);
                desktopWallpaper.SetWallpaper(monitorId, imagePath);
            }
            else
            {
                ThreadStart threadStarter = () =>
                {
                    IActiveDesktop _activeDesktop = ActiveDesktopWrapper.GetActiveDesktop();
                    _activeDesktop.SetWallpaper(imagePath, 0);
                    _activeDesktop.ApplyChanges(AD_Apply.ALL | AD_Apply.FORCE);

                    Marshal.ReleaseComObject(_activeDesktop);
                };
                Thread thread = new Thread(threadStarter);
                thread.SetApartmentState(ApartmentState.STA);  // Set the thread to STA (required!)
                thread.Start();
                thread.Join(2000);
            }

            RefreshDesktop();  // Send WM_SETTINGCHANGE to refresh the desktop
        }
    }
}
