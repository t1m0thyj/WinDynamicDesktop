// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class LockScreenChanger
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static ToolStripMenuItem menuItem;

        public static List<ToolStripItem> GetMenuItems()
        {
            if ((JsonConfig.settings.lockScreenFollowIndex != -1 || JsonConfig.settings.lockScreenTheme != null) &&
                !UwpDesktop.IsUwpSupported())
            {
                JsonConfig.settings.lockScreenFollowIndex = -1;
                JsonConfig.settings.lockScreenTheme = null;
            }

            menuItem = new ToolStripMenuItem(_("Sync &lockscreen image with this display"), null, OnLockScreenItemClick);
            menuItem.Enabled = UwpDesktop.IsUwpSupported();

            return new List<ToolStripItem>() {
                menuItem
            };
        }

        public static async void UpdateImage(string imagePath)
        {
            var imageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(imagePath);
            await Windows.System.UserProfile.LockScreen.SetImageFileAsync(imageFile);
        }

        public static void UpdateMenuItems(int displayIndex, int? lockScreenFollowIndex = null)
        {
            if (lockScreenFollowIndex.HasValue)
            {
                JsonConfig.settings.lockScreenFollowIndex = lockScreenFollowIndex.Value;
            }

            menuItem.Checked = menuItem.Enabled && JsonConfig.settings.lockScreenFollowIndex == displayIndex;
        }

        private static void OnLockScreenItemClick(object sender, EventArgs e)
        {
            bool isEnabled = JsonConfig.settings.changeLockScreen ^ true;
            JsonConfig.settings.changeLockScreen = isEnabled;
            menuItem.Checked = isEnabled;

            AppContext.scheduler.Run(true);
        }
    }
}
