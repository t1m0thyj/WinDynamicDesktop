// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using DesktopBridge;
using System;

namespace WinDynamicDesktop
{
    abstract class PlatformHelper
    {
        public abstract string GetLocalFolder();

        public abstract void CheckStartOnBoot();

        public abstract void ToggleStartOnBoot();

        public abstract void OpenUpdateLink();

        public abstract void SetWallpaper(string imagePath);
    }

    class UwpDesktop
    {
        private static bool? _isRunningAsUwp;
        private static PlatformHelper helper;

        public static bool IsRunningAsUwp()
        {
            if (!_isRunningAsUwp.HasValue)
            {
                Helpers helpers = new Helpers();
                _isRunningAsUwp = helpers.IsRunningAsUwp();
            }

            return _isRunningAsUwp.Value;
        }

        public static bool IsUwpSupported()
        {
            return Environment.OSVersion.Version.Major >= 10;
        }

        public static bool IsVirtualDesktopSupported()
        {
            return Environment.OSVersion.Version.Build >= 21337;
        }

        public static PlatformHelper GetHelper()
        {
            if (helper == null)
            {
                if (!IsRunningAsUwp())
                {
                    helper = new DesktopHelper();
                }
                else
                {
                    helper = new UwpHelper();
                }
            }

            return helper;
        }
    }
}
