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
    class UwpDesktop
    {
        public static string GetCurrentDirectory()
        {
            return Path.GetDirectoryName(Application.ExecutablePath);
        }
    }

    class StartupManager
    {
        private bool startOnBoot;
        private string registryStartupLocation = @"Software\Microsoft\Windows\CurrentVersion\Run";

        private MenuItem menuItem;

        public StartupManager(MenuItem startupMenuItem)
        {
            menuItem = startupMenuItem;

            CheckStatus();
        }

        private void CheckStatus()
        {
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(registryStartupLocation);
            startOnBoot = startupKey.GetValue("WinDynamicDesktop") != null;
            startupKey.Close();

            menuItem.Checked = startOnBoot;
        }

        public void ToggleStartOnBoot()
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

            menuItem.Checked = startOnBoot;
        }
    }
}
