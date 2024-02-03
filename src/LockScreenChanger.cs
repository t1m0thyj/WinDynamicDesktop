// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class LockScreenChanger
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static ToolStripMenuItem menuItem;
        private static int activeDisplayIndex;

        public static ToolStripItem[] GetMenuItems()
        {
            if ((JsonConfig.settings.lockScreenDisplayIndex != -1 || JsonConfig.settings.lockScreenTheme != null) &&
                !UwpDesktop.IsUwpSupported())
            {
                JsonConfig.settings.lockScreenDisplayIndex = -1;
                JsonConfig.settings.lockScreenTheme = null;
            }

            menuItem = new ToolStripMenuItem(_("Sync &lockscreen image with {0}"), null, OnLockScreenItemClick);
            menuItem.Enabled = UwpDesktop.IsUwpSupported();

            return new ToolStripItem[] {
                menuItem
            };
        }

        public static async void UpdateImage(string imagePath)
        {
            var imageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(imagePath);
            await Windows.System.UserProfile.LockScreen.SetImageFileAsync(imageFile);
        }

        public static void UpdateMenuItems(int? selectedDisplayIndex = null)
        {
            activeDisplayIndex = Math.Max(0, selectedDisplayIndex ?? JsonConfig.settings.lockScreenDisplayIndex);
            menuItem.Checked = menuItem.Enabled && activeDisplayIndex == JsonConfig.settings.lockScreenDisplayIndex;
            menuItem.Text = string.Format(_("Sync &lock screen image with {0}"), GetDisplayName());
        }

        public static string GetDisplayName()
        {
            return activeDisplayIndex == 0 ? _("All Displays") : string.Format(_("Display {0}"), activeDisplayIndex);
        }

        private static void OnLockScreenItemClick(object sender, EventArgs e)
        {
            JsonConfig.settings.lockScreenDisplayIndex = JsonConfig.settings.lockScreenDisplayIndex == -1 ?
                activeDisplayIndex : -1;
            UpdateMenuItems();
        }
    }
}
