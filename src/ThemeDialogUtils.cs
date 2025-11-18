// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Drawing;
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
        private static List<ListViewItem> allThemeItems = new List<ListViewItem>();
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
                if (JsonConfig.settings.showInstalledOnly && !ThemeManager.IsThemeDownloaded(theme))
                {
                    continue;
                }

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

        internal static void ApplySearchFilter(ListView listView, string searchText)
        {
            searchText = searchText.Trim().ToLower();

            // Save all items on first search
            if (allThemeItems.Count == 0 && listView.Items.Count > 0)
            {
                foreach (ListViewItem item in listView.Items)
                {
                    allThemeItems.Add((ListViewItem)item.Clone());
                }
            }

            listView.BeginUpdate();
            listView.Items.Clear();

            // Filter and add items back
            foreach (ListViewItem item in allThemeItems)
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // Show all items when search is empty
                    listView.Items.Add((ListViewItem)item.Clone());
                }
                else
                {
                    // Get theme name (remove favorite star)
                    string themeName = item.Text.Replace("★ ", "").ToLower();

                    if (themeName.Contains(searchText))
                    {
                        listView.Items.Add((ListViewItem)item.Clone());
                    }
                }
            }

            listView.EndUpdate();

            // Update selection
            if (listView.Items.Count > 0 && listView.SelectedItems.Count == 0)
            {
                listView.Items[0].Selected = true;
            }
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

        internal static bool UpdateConfigForLockScreen(string activeTheme)
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
            else if (JsonConfig.settings.lockScreenTheme == null && activeTheme != null)
            {
                MessageDialog.ShowInfo(_("WinDynamicDesktop cannot change the lock screen image when Windows " +
                    "Spotlight is enabled. If the lock screen image does not update, navigate to \"Personalization " +
                    "-> Lock screen\" in Windows settings and check that the lock screen is set to Picture mode."));
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

        internal static void ToggleShowInstalledOnly(ListView listView, bool newValue)
        {
            JsonConfig.settings.showInstalledOnly = newValue;
            if (newValue)
            {
                foreach (ListViewItem item in listView.Items)
                {
                    if (item.Tag != null)
                    {
                        string themeId = (string)item.Tag;
                        ThemeConfig theme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);
                        if (!ThemeManager.IsThemeDownloaded(theme))
                        {
                            listView.Items.Remove(item);
                            listView.LargeImageList.Images[item.ImageIndex].Dispose();
                        }
                    }
                }
            }
            else
            {
                LoadThemes(ThemeManager.themeSettings.Where(theme => !ThemeManager.IsThemeDownloaded(theme)).ToList(),
                    listView, new ThemeLoadOpts());
            }
        }
    }
}
