// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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

        public static void SyncVirtualDesktops(string imagePath)
        {
            if (UwpDesktop.IsVirtualDesktopSupported())
            {
                VirtualDesktopApi.SetWallpaper(imagePath);
            }
        }

        public static async void SetWallpaper(string imagePath, int displayIndex = -1)
        {
            EnableTransitions();

            if (displayIndex != -1)
            {
                // TODO Error handling for older Windows versions without this API
                IDesktopWallpaper desktopWallpaper = DesktopWallpaperFactory.Create();
                string monitorId = desktopWallpaper.GetMonitorDevicePathAt((uint)displayIndex);
                desktopWallpaper.SetWallpaper(monitorId, imagePath);
                await SyncWithPrimaryDisplay(imagePath, displayIndex);
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
                var task = SyncWithPrimaryDisplay(imagePath, displayIndex);
                thread.Join(2000);
                await task;
            }
        }

        private static async Task SyncWithPrimaryDisplay(string imagePath, int displayIndex)
        {
            if (displayIndex > 0)
            {
                return;
            }

            if (UwpDesktop.IsUwpSupported() && JsonConfig.settings.changeLockScreen)
            {
                await LockScreenChanger.UpdateImage(imagePath);
            }

            SyncVirtualDesktops(imagePath);
        }
    }
}
