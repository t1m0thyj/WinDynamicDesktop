// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class LockScreenChanger
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static ToolStripMenuItem menuItem;

        public static List<ToolStripItem> GetMenuItems()
        {
            if (JsonConfig.settings.changeLockScreen && !UwpDesktop.IsUwpSupported())
            {
                JsonConfig.settings.changeLockScreen = false;
            }

            menuItem = new ToolStripMenuItem(_("Change &Lockscreen Image"), null, OnLockScreenItemClick);
            menuItem.Checked = JsonConfig.settings.changeLockScreen;
            menuItem.Enabled = UwpDesktop.IsUwpSupported();

            return new List<ToolStripItem>() {
                menuItem
            };
        }

        public static async Task UpdateImage(string imagePath)
        {
            var imageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(imagePath);
            await Windows.System.UserProfile.LockScreen.SetImageFileAsync(imageFile);
        }

        private static void OnLockScreenItemClick(object sender, EventArgs e)
        {
            bool isEnabled = JsonConfig.settings.changeLockScreen ^ true;
            JsonConfig.settings.changeLockScreen = isEnabled;
            menuItem.Checked = isEnabled;

            AppContext.wpEngine.RunScheduler(true);
        }
    }
}
