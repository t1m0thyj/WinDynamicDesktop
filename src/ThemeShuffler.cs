// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class ThemeShuffler
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static ToolStripMenuItem noneMenuItem;
        private static ToolStripMenuItem favoritesMenuItem;
        private static ToolStripMenuItem allMenuItem;
        private static ToolStripMenuItem shufflePeriodItem;
        private static Random rng = new Random();

        public static List<ToolStripItem> GetMenuItems()
        {
            noneMenuItem = new ToolStripMenuItem(_("Don't &shuffle themes"), null, OnShuffleNoneItemClick);
            favoritesMenuItem = new ToolStripMenuItem(_("Shuffle &favorite themes"), null, OnShuffleFavoritesItemClick);
            allMenuItem = new ToolStripMenuItem(_("Shuffle &all themes"), null, OnShuffleAllItemClick);
            shufflePeriodItem = new ToolStripMenuItem(_("Choose shuffle frequency"), null);
            // TODO Add event handlers for submenu
            shufflePeriodItem.DropDownItems.Add(new ToolStripMenuItem(_("Every Hour"), null));
            shufflePeriodItem.DropDownItems.Add(new ToolStripMenuItem(_("Every 12 Hours"), null));
            shufflePeriodItem.DropDownItems.Add(new ToolStripMenuItem(_("Every Day"), null));
            shufflePeriodItem.DropDownItems.Add(new ToolStripMenuItem(_("Every 2 Days"), null));
            shufflePeriodItem.DropDownItems.Add(new ToolStripMenuItem(_("Every Week"), null));
            shufflePeriodItem.DropDownItems.Add(new ToolStripMenuItem(_("Every Month"), null));
            ((ToolStripMenuItem)shufflePeriodItem.DropDownItems[1]).Checked = true;
            UpdateMenuItems();

            return new List<ToolStripItem>()
            {
                noneMenuItem,
                favoritesMenuItem,
                allMenuItem,
                shufflePeriodItem
            };
        }

        public static void AddThemeToHistory(string themeId, bool clearHistory = false)
        {
            List<string> shuffleHistory;

            if (!clearHistory)
            {
                shuffleHistory = JsonConfig.settings.shuffleHistory?.ToList() ?? new List<string>();
                shuffleHistory.Remove(themeId);
            }
            else
            {
                shuffleHistory = new List<string>();
            }

            shuffleHistory.Add(themeId);
            JsonConfig.settings.shuffleHistory = shuffleHistory.ToArray();
            JsonConfig.settings.lastShuffleDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        public static void MaybeShuffleWallpaper()
        {
            if (JsonConfig.settings.themeShuffleMode == 0)
            {
                return;
            }

            if (JsonConfig.settings.lastShuffleDate != null)
            {
                DateTime lastShuffleDate = DateTime.Parse(JsonConfig.settings.lastShuffleDate,
                    CultureInfo.InvariantCulture);

                if (lastShuffleDate.Date == DateTime.Now.Date)
                {
                    return;
                }
            }

            ShuffleWallpaper();
        }

        private static ThemeConfig GetNextTheme()
        {
            List<string> shuffleHistory = JsonConfig.settings.shuffleHistory?.ToList() ?? new List<string>();
            List<ThemeConfig> themeChoices = new List<ThemeConfig>();
            ThemeConfig nextTheme;

            foreach (ThemeConfig theme in ThemeManager.themeSettings)
            {
                if ((int)(JsonConfig.settings.themeShuffleMode / 10) == 1 &&
                    (JsonConfig.settings.favoriteThemes == null ||
                    !JsonConfig.settings.favoriteThemes.Contains(theme.themeId)))
                {
                    continue;
                }

                if (!shuffleHistory.Contains(theme.themeId) && theme.imageFilename != null)
                {
                    themeChoices.Add(theme);
                }
            }

            if (themeChoices.Count > 0)
            {
                nextTheme = themeChoices[rng.Next(themeChoices.Count)];
            }
            else
            {
                themeChoices = ThemeManager.themeSettings.Where((theme) => theme.imageFilename != null).ToList();
                nextTheme = themeChoices[rng.Next(themeChoices.Count)];
                string lastThemeId = shuffleHistory.LastOrDefault();

                while ((themeChoices.Count > 1) && (nextTheme.themeId == lastThemeId))
                {
                    nextTheme = themeChoices[rng.Next(themeChoices.Count)];
                }
            }

            AddThemeToHistory(nextTheme.themeId, themeChoices.Count == 0);
            return nextTheme;
        }

        private static void ShuffleWallpaper()
        {
            if (JsonConfig.settings.activeThemes[0] != null)
            {
                JsonConfig.settings.activeThemes[0] = GetNextTheme().themeId;
            }
            else
            {
                for (int i = 1; i < JsonConfig.settings.activeThemes.Length; i++)
                {
                    JsonConfig.settings.activeThemes[i] = GetNextTheme().themeId;
                }
            }
        }

        private static void UpdateMenuItems(int? shuffleMode = null)
        {
            if (shuffleMode.HasValue)
            {
                JsonConfig.settings.themeShuffleMode = shuffleMode.Value;
                if (shuffleMode > 0)
                {
                    JsonConfig.settings.lastShuffleDate = null;
                }
            }

            noneMenuItem.Checked = JsonConfig.settings.themeShuffleMode == 0;
            favoritesMenuItem.Checked = JsonConfig.settings.themeShuffleMode == 1;
            allMenuItem.Checked = JsonConfig.settings.themeShuffleMode == 2;
        }

        private static void OnShuffleNoneItemClick(object sender, EventArgs e)
        {
            UpdateMenuItems(0);
        }

        private static void OnShuffleFavoritesItemClick(object sender, EventArgs e)
        {
            UpdateMenuItems(12);
        }

        private static void OnShuffleAllItemClick(object sender, EventArgs e)
        {
            UpdateMenuItems(22);
        }
    }
}
