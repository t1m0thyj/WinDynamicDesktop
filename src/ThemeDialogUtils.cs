// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsDisplayAPI.DisplayConfig;

namespace WinDynamicDesktop
{
    public class ThemeLoadOpts
    {
        public string activeTheme;
        public string focusTheme;

        public ThemeLoadOpts(string activeTheme = null, string focusTheme = null)
        {
            this.activeTheme = activeTheme;
            this.focusTheme = focusTheme;
        }
    }

    internal class ThemeDialogUtils
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static SemaphoreSlim loadSemaphore = new SemaphoreSlim(1);

        internal static string[] GetDisplayNames()
        {
            // https://github.com/winleafs/Winleafs/blob/98ba3ba/Winleafs.Wpf/Helpers/ScreenBoundsHelper.cs#L36=
            var activeDisplays = WindowsDisplayAPI.Display.GetDisplays();
            var activeDisplayDevicePaths = activeDisplays.OrderBy(d => d.DisplayName)
                .Select(d => d.DevicePath).ToArray();
            return PathDisplayTarget.GetDisplayTargets()
                .Where(dt => activeDisplayDevicePaths.Contains(dt.DevicePath))
                .OrderBy(dt => Array.IndexOf(activeDisplayDevicePaths, dt.DevicePath)).Select(dt =>
                {
                    return string.IsNullOrEmpty(dt.FriendlyName) ? _("Internal Display") : dt.FriendlyName;
                }).ToArray();
        }

        internal static Bitmap GetMeatballIcon(Color iconColor)
        {
            Bitmap bitmap = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                for (int i = 0; i < 3; i++)
                {
                    graphics.FillEllipse(new SolidBrush(iconColor), i * 6, 6, 4, 4);
                }
            }
            return bitmap;
        }

        // Code to change ListView appearance from https://stackoverflow.com/a/4463114/5504760
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        internal static void LoadThemes(List<ThemeConfig> themes, ListView listView, ThemeLoadOpts opts)
        {
            loadSemaphore.Wait(60000);
            Size thumbnailSize = ThemeThumbLoader.GetThumbnailSize(listView);
            ListViewItem focusedItem = null;

            foreach (ThemeConfig theme in themes.ToList())
            {
                try
                {
                    using (Image thumbnailImage = ThemeThumbLoader.GetThumbnailImage(theme, thumbnailSize, true))
                    {
                        listView.Invoke(new Action(() =>
                        {
                            listView.LargeImageList.Images.Add(thumbnailImage);
                            string itemText = ThemeManager.GetThemeName(theme);
                            if (JsonConfig.settings.favoriteThemes != null &&
                                JsonConfig.settings.favoriteThemes.Contains(theme.themeId))
                            {
                                itemText = "★ " + itemText;
                            }
                            ListViewItem newItem = listView.Items.Add(itemText,
                                listView.LargeImageList.Images.Count - 1);
                            newItem.Tag = theme.themeId;

                            if (opts.activeTheme != null && opts.activeTheme == theme.themeId)
                            {
                                newItem.Font = new Font(newItem.Font, FontStyle.Bold);
                            }
                            if (opts.focusTheme == null || opts.focusTheme == theme.themeId)
                            {
                                focusedItem = newItem;
                            }
                        }));
                    }
                }
                catch (OutOfMemoryException)
                {
                    ThemeLoader.HandleError(new FailedToCreateThumbnail(theme.themeId));
                }
            }

            listView.Invoke(new Action(() =>
            {
                listView.Sort();

                if (focusedItem == null)
                {
                    focusedItem = listView.Items[0];
                }

                focusedItem.Selected = true;
                listView.EnsureVisible(focusedItem.Index);

                ThemeThumbLoader.CacheThumbnails(listView);
            }));
            loadSemaphore.Release();
        }

        internal static void LoadImportedThemes(List<ThemeConfig> themes, ListView listView, ImportDialog importDialog)
        {
            themes.Sort((t1, t2) => t1.themeId.CompareTo(t2.themeId));

            foreach (ThemeConfig theme in themes)
            {
                foreach (ListViewItem item in listView.Items)
                {
                    if ((string)item.Tag == theme.themeId)
                    {
                        listView.Items.RemoveAt(item.Index);
                        listView.LargeImageList.Images[item.ImageIndex].Dispose();
                        break;
                    }
                }
            }

            Task.Run(() =>
            {
                LoadThemes(themes, listView, new ThemeLoadOpts());

                importDialog.Invoke(new Action(() =>
                {
                    importDialog.thumbnailsLoaded = true;
                    importDialog.Close();
                }));
            });
        }

        internal static void UpdateConfigForDisplay(List<string> activeThemes)
        {
            int numNewDisplays = Screen.AllScreens.Length - activeThemes.Count + 1;

            if (numNewDisplays > 0)
            {
                for (int i = 0; i < numNewDisplays; i++)
                {
                    activeThemes.Add(null);
                }
            }

            if (activeThemes[0] != null)
            {
                for (int i = 0; i < activeThemes.Count; i++)
                {
                    if (activeThemes[i] == null)
                    {
                        activeThemes[i] = activeThemes[0];
                    }
                }

                activeThemes[0] = null;
            }
        }

        internal static bool UpdateConfigForLockScreen()
        {
            if (JsonConfig.settings.lockScreenDisplayIndex != -1)
            {
                DialogResult result = MessageDialog.ShowQuestion(string.Format(
                    _("Applying this theme will stop the lock screen image from syncing with {0}. " +
                    "Are you sure you want to continue?"), LockScreenChanger.GetDisplayName()));
                if (result != DialogResult.Yes)
                {
                    return false;
                }
            }

            JsonConfig.settings.lockScreenDisplayIndex = -1;
            LockScreenChanger.UpdateMenuItems();
            return true;
        }

        internal static bool DeleteTheme(ThemeConfig theme, ListView listView, int itemIndex)
        {
            if ((!JsonConfig.IsNullOrEmpty(JsonConfig.settings.activeThemes) &&
                (JsonConfig.settings.activeThemes[0] == theme.themeId || (JsonConfig.settings.activeThemes[0] == null &&
                JsonConfig.settings.activeThemes.Contains(theme.themeId)))) ||
                JsonConfig.settings.lockScreenTheme == theme.themeId)
            {
                MessageDialog.ShowWarning(string.Format(_("The '{0}' theme cannot be deleted because it is " +
                    "currently active for one or more displays."), ThemeManager.GetThemeName(theme)), _("Error"));
                return false;
            }

            DialogResult result = MessageDialog.ShowQuestion(string.Format(_("Are you sure you want to remove " +
                "the '{0}' theme?"), ThemeManager.GetThemeName(theme)), _("Question"), MessageBoxIcon.Warning);

            if (result == DialogResult.Yes && !ThemeManager.defaultThemes.Contains(theme.themeId))
            {
                int imageIndex = listView.Items[itemIndex].ImageIndex;
                listView.Items.RemoveAt(itemIndex);
                listView.Items[itemIndex - 1].Selected = true;
                listView.LargeImageList.Images[imageIndex].Dispose();
            }

            return result == DialogResult.Yes;
        }
    }
}
