using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WinDynamicDesktop
{
    class DesktopHelper
    {
        public static string GetCurrentDirectory()
        {
            return Path.GetDirectoryName(Application.ExecutablePath);
        }

        public static void SetWallpaper(string imageFilename)
        {
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "images",
                imageFilename);

            WallpaperChanger.EnableTransitions();
            WallpaperChanger.SetWallpaper(imagePath);
        }
    }

    class DesktopStartupManager : StartupManager
    {
        private string registryStartupLocation = @"Software\Microsoft\Windows\CurrentVersion\Run";

        internal override void UpdateStatus()
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
                string exePath = Path.Combine(Directory.GetCurrentDirectory(),
                    Environment.GetCommandLineArgs()[0]);
                startupKey.SetValue("WinDynamicDesktop", exePath);
                startOnBoot = true;
            }
            else
            {
                startupKey.DeleteValue("WinDynamicDesktop");
                startOnBoot = false;
            }

            MainMenu.startOnBootItem.Checked = startOnBoot;
        }
    }
}
