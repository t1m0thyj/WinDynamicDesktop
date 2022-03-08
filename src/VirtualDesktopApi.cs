// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using WindowsDesktop;

namespace WinDynamicDesktop
{
    class VirtualDesktopApi
    {
        private static Action timerEventHandler;

        public static void Initialize(WallpaperEngine wcs)
        {
            timerEventHandler = new Action(() => wcs.HandleTimerEvent(false));

            if (UwpDesktop.IsVirtualDesktopSupported())
            {
                VirtualDesktop.Configure();
                VirtualDesktop.CurrentChanged += OnVirtualDesktopCurrentChanged;
            }
        }

        public static void SetWallpaper(string imagePath)
        {
            if (JsonConfig.settings.activeThemes[0] == null)
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
            if (JsonConfig.settings.activeThemes[0] == null)
            {
                timerEventHandler.Invoke();
            }
        }
    }
}
