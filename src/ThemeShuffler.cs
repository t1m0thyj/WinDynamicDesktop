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
    enum ShufflePeriod
    {
        EveryHour = 0,
        Every12Hours = 1,
        EveryDay = 2,
        Every2Days = 3,
        EveryWeek = 4,
        EveryMonth = 5
    }

    class ThemeShuffler
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static ToolStripMenuItem noneMenuItem;
        private static ToolStripMenuItem favoritesMenuItem;
        private static ToolStripMenuItem allMenuItem;
        private static ToolStripMenuItem shufflePeriodItem;
        private static Random rng = new Random();

        public static ToolStripItem[] GetMenuItems()
        {
            noneMenuItem = new ToolStripMenuItem(_("Don't &shuffle themes"), null, OnShuffleNoneItemClick);
            favoritesMenuItem = new ToolStripMenuItem(_("Shuffle &favorite themes"), null, OnShuffleFavoritesItemClick);
            allMenuItem = new ToolStripMenuItem(_("Shuffle &all themes"), null, OnShuffleAllItemClick);
            shufflePeriodItem = new ToolStripMenuItem(_("Choose shuffle &duration"), null);
            shufflePeriodItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem(_("Every Hour"), null, OnShufflePeriodItemClick),
                new ToolStripMenuItem(_("Every 12 Hours"), null, OnShufflePeriodItemClick),
                new ToolStripMenuItem(_("Every Day"), null, OnShufflePeriodItemClick),
                new ToolStripMenuItem(_("Every 2 Days"), null, OnShufflePeriodItemClick),
                new ToolStripMenuItem(_("Every Week"), null, OnShufflePeriodItemClick),
                new ToolStripMenuItem(_("Every Month"), null, OnShufflePeriodItemClick)
            });
            UpdateMenuItems();

            return new ToolStripItem[]
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
            JsonConfig.settings.lastShuffleTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        public static DateTime? MaybeShuffleWallpaper(SolarData solarData)
        {
            if (JsonConfig.settings.themeShuffleMode / 10 == 0)
            {
                return null;
            }

            bool shouldShuffle = true;
            DateTime? lastShuffleTime = null;
            DateTime? nextUpdateTime = null;

            if (JsonConfig.settings.lastShuffleTime != null)
            {
                lastShuffleTime = DateTime.Parse(JsonConfig.settings.lastShuffleTime, CultureInfo.InvariantCulture);

                switch (JsonConfig.settings.themeShuffleMode % 10)
                {
                    case (int)ShufflePeriod.EveryHour:
                        shouldShuffle = lastShuffleTime.Value.Date != DateTime.Now.Date ||
                            lastShuffleTime.Value.Hour != DateTime.Now.Hour;
                        break;
                    case (int)ShufflePeriod.Every12Hours:
                        shouldShuffle = lastShuffleTime.Value < DateTime.Now.AddHours(-12);
                        break;
                    case (int)ShufflePeriod.EveryDay:
                        shouldShuffle = lastShuffleTime.Value.Date != DateTime.Now.Date;
                        break;
                    case (int)ShufflePeriod.Every2Days:
                        shouldShuffle = lastShuffleTime.Value.Date < DateTime.Now.Date.AddDays(-1);
                        break;
                    case (int)ShufflePeriod.EveryWeek:
                        shouldShuffle = lastShuffleTime.Value.Date < DateTime.Now.Date.AddDays(-6);
                        break;
                    case (int)ShufflePeriod.EveryMonth:
                        shouldShuffle = lastShuffleTime.Value.Date != DateTime.Now.Date &&
                            lastShuffleTime.Value.Month != DateTime.Now.Month;
                        break;
                }
            }

            if (shouldShuffle)
            {
                LoggingHandler.LogMessage(string.Format("Last shuffle time was {0}",
                    JsonConfig.settings.lastShuffleTime));
                ShuffleWallpaper();
            }

            switch (JsonConfig.settings.themeShuffleMode % 10)
            {
                case (int)ShufflePeriod.EveryHour:
                    nextUpdateTime = DateTime.Today.AddHours(DateTime.Now.Hour + 1);
                    break;
                case (int)ShufflePeriod.Every12Hours:
                    nextUpdateTime = DateTime.Now < solarData.solarNoon ? solarData.solarNoon :
                        solarData.solarNoon.AddHours(12);
                    break;
                case (int)ShufflePeriod.EveryDay:
                    nextUpdateTime = DateTime.Today.AddDays(1);
                    break;
                case (int)ShufflePeriod.Every2Days:
                    nextUpdateTime = (lastShuffleTime?.Date ?? DateTime.Today).AddDays(2);
                    break;
                case (int)ShufflePeriod.EveryWeek:
                    nextUpdateTime = DateTime.Today.AddDays(7 - (int)DateTime.Today.DayOfWeek);
                    break;
                case (int)ShufflePeriod.EveryMonth:
                    nextUpdateTime = DateTime.Today.AddDays(-DateTime.Today.Day).AddMonths(1);
                    break;
            }

            return nextUpdateTime?.AddTicks(1);
        }

        private static ThemeConfig GetNextTheme()
        {
            List<string> shuffleHistory = JsonConfig.settings.shuffleHistory?.ToList() ?? new List<string>();
            List<ThemeConfig> themeChoices = new List<ThemeConfig>();
            ThemeConfig nextTheme;

            foreach (ThemeConfig theme in ThemeManager.themeSettings)
            {
                if (JsonConfig.settings.themeShuffleMode / 10 == 1 &&
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

            if (JsonConfig.settings.lockScreenTheme != null)
            {
                JsonConfig.settings.lockScreenTheme = GetNextTheme().themeId;
            }
        }

        private static void UpdateMenuItems(int? shuffleMode = null, int? shufflePeriod = null)
        {
            bool settingsChanged = shuffleMode.HasValue || shufflePeriod.HasValue;
            shuffleMode = shuffleMode ?? (JsonConfig.settings.themeShuffleMode / 10);
            shufflePeriod = shufflePeriod ?? (JsonConfig.settings.themeShuffleMode % 10);

            if (settingsChanged)
            {
                JsonConfig.settings.themeShuffleMode = shuffleMode.Value * 10 + shufflePeriod.Value;

                if (shuffleMode > 0)
                {
                    JsonConfig.settings.lastShuffleTime = null;
                }
            }

            noneMenuItem.Checked = shuffleMode == 0;
            favoritesMenuItem.Checked = shuffleMode == 1;
            allMenuItem.Checked = shuffleMode == 2;
            shufflePeriodItem.Enabled = shuffleMode > 0;
            for (int i = 0; i < shufflePeriodItem.DropDownItems.Count; i++)
            {
                ((ToolStripMenuItem)shufflePeriodItem.DropDownItems[i]).Checked = i == shufflePeriod;
            }
        }

        private static void OnShuffleNoneItemClick(object sender, EventArgs e)
        {
            UpdateMenuItems(0);
        }

        private static void OnShuffleFavoritesItemClick(object sender, EventArgs e)
        {
            UpdateMenuItems(1);
        }

        private static void OnShuffleAllItemClick(object sender, EventArgs e)
        {
            UpdateMenuItems(2);
        }

        private static void OnShufflePeriodItemClick(object sender, EventArgs e)
        {
            UpdateMenuItems(null, shufflePeriodItem.DropDownItems.IndexOf((ToolStripMenuItem)sender));
        }
    }
}
