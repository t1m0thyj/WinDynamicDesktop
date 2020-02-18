// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WinDynamicDesktop
{
    class RegistryTweaker
    {
        private const string registryDesktopPath = @"Control Panel\Desktop";
        private const string registrySerializePath =
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize";

        private static bool isJpegCompressionTweaked;
        private static bool isStartupDelayTweaked;

        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static ToolStripMenuItem jpegCompressionItem;
        private static ToolStripMenuItem startupDelayItem;

        public static List<ToolStripItem> GetMenuItems()
        {
            if (UwpDesktop.IsRunningAsUwp())
            {
                return new List<ToolStripItem>();
            }

            int jpegImportQuality = (int)(GetValue(registryDesktopPath, "JPEGImportQuality") ?? 0);
            isJpegCompressionTweaked = jpegImportQuality == 100;

            jpegCompressionItem = new ToolStripMenuItem(
                _("Disable Windows 10 &JPEG wallpaper compression"), null,
                OnJpegCompressionItemClick);
            jpegCompressionItem.Checked = isJpegCompressionTweaked;

            int startupDelay = (int)(GetValue(registrySerializePath, "StartupDelayInMSec") ?? 1);
            isStartupDelayTweaked = startupDelay == 0;

            startupDelayItem = new ToolStripMenuItem(
                _("Disable Windows 10 &program startup delay"), null,
                OnStartupDelayItemClick);
            startupDelayItem.Checked = isStartupDelayTweaked;

            return new List<ToolStripItem>() {
                new ToolStripSeparator(),
                jpegCompressionItem,
                startupDelayItem
            };
        }

        private static void DeleteValue(string path, string name)
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(path);
            key.DeleteValue(name);
        }

        private static object GetValue(string path, string name)
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(path);
            return key?.GetValue(name);
        }

        private static void SetValue(string path, string name, int value)
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
            key.SetValue(name, value);
        }

        private static void ToggleJpegCompressionTweak()
        {
            if (isJpegCompressionTweaked)  // Then disable the tweak
            {
                DeleteValue(registryDesktopPath, "JPEGImportQuality");
            }
            else  // Enable the tweak
            {
                SetValue(registryDesktopPath, "JPEGImportQuality", 100);
            }

            isJpegCompressionTweaked ^= true;
        }

        private static void ToggleStartupDelayTweak()
        {
            if (isStartupDelayTweaked)  // Then disable the tweak
            {
                DeleteValue(registrySerializePath, "StartupDelayInMSec");
            }
            else  // Enable the tweak
            {
                SetValue(registrySerializePath, "StartupDelayInMSec", 0);
            }

            isStartupDelayTweaked ^= true;
        }

        private static void OnJpegCompressionItemClick(object sender, EventArgs e)
        {
            ToggleJpegCompressionTweak();
            jpegCompressionItem.Checked = isJpegCompressionTweaked;
            MessageDialog.ShowInfo(_("This tweak only affects wallpapers that are JPEG images. " +
                "In order for this change to take effect, you should restart your computer."));
        }

        private static void OnStartupDelayItemClick(object sender, EventArgs e)
        {
            ToggleStartupDelayTweak();
            startupDelayItem.Checked = isStartupDelayTweaked;
            MessageDialog.ShowInfo(_("Restart your computer for this change to take effect."));
        }
    }
}
