// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class DesktopHelper : PlatformHelper
    {
        private const string registryStartupLocation = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string updateLink = "https://github.com/t1m0thyj/WinDynamicDesktop/releases";

        private bool startOnBoot;

        public override string GetLocalFolder()
        {
            return Application.StartupPath;
        }

        public override void CheckStartOnBoot()
        {
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(registryStartupLocation);
            startOnBoot = startupKey.GetValue("WinDynamicDesktop") != null;
            startupKey.Close();

            MainMenu.startOnBootItem.Checked = startOnBoot;
        }

        public override void ToggleStartOnBoot()
        {
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(registryStartupLocation, true);

            if (!startOnBoot)
            {
                startupKey.SetValue("WinDynamicDesktop", Application.ExecutablePath);
                startOnBoot = true;
            }
            else
            {
                startupKey.DeleteValue("WinDynamicDesktop");
                startOnBoot = false;
            }

            MainMenu.startOnBootItem.Checked = startOnBoot;
        }

        public override void OpenUpdateLink()
        {
            System.Diagnostics.Process.Start(updateLink);
        }

        public override void SetWallpaper(string imagePath)
        {
            WallpaperApi.SetWallpaper(imagePath);
        }
    }
}
