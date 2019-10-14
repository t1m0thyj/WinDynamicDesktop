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
    class WallpaperCompressionChanger
    {
        private const string registryCompressionLocation = @"Control Panel\Desktop";

        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static ToolStripMenuItem compressionTweakItem;
        private static bool isWallpaperCompressionTweaked;

        public static List<ToolStripItem> GetMenuItems()
        {
            RegistryKey desktopKey = Registry.CurrentUser.OpenSubKey(registryCompressionLocation);

            if (UwpDesktop.IsRunningAsUwp() || desktopKey == null)
            {
                return new List<ToolStripItem>();
            }

            isWallpaperCompressionTweaked = (int)desktopKey.GetValue("JPEGImportQuality", 0) == 100;

            desktopKey.Close();

            compressionTweakItem = new ToolStripMenuItem(
                _("Disable Windows 10 &JPEG wallpaper compression"),
                null, OnWallpaperCompressionItemClick);
            compressionTweakItem.Checked = isWallpaperCompressionTweaked;

            return new List<ToolStripItem>() {
                compressionTweakItem,
                new ToolStripSeparator()
            };
        }

        public static void TryApplyWallpaperCompressionTweak()
        {
            RegistryKey desktopKey = Registry.CurrentUser.OpenSubKey(registryCompressionLocation, true);

            if (isWallpaperCompressionTweaked)
            {
                // Disable the wallpaper compression tweak.
                desktopKey.DeleteValue("JPEGImportQuality");
            }
            else
            {
                // Enable the wallpaper compression tweak.
                desktopKey.SetValue("JPEGImportQuality", 100);
            }

            isWallpaperCompressionTweaked = (int)desktopKey.GetValue("JPEGImportQuality", 0) == 100;

            desktopKey.Close();
        }

        private static void OnWallpaperCompressionItemClick(object sender, EventArgs e)
        {
            TryApplyWallpaperCompressionTweak();
            compressionTweakItem.Checked = isWallpaperCompressionTweaked;
            MessageDialog.ShowInfo(_("This tweak only affects wallpapers that are JPEG images. " +
                "In order for this change to take effect, you should restart your computer."));
        }
    }
}
