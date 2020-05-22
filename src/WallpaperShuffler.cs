// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace WinDynamicDesktop
{
    class WallpaperShuffler
    {
        private static Random rng = new Random();

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
            if (!JsonConfig.settings.enableShuffle)
            {
                return;
            }

            if (JsonConfig.settings.lastShuffleDate != null)
            {
                DateTime lastShuffleDate = UpdateHandler.SafeParse(JsonConfig.settings.lastShuffleDate);

                if (lastShuffleDate.Date == DateTime.Now.Date)
                {
                    return;
                }
            }

            ShuffleWallpaper();
        }

        public static void ToggleShuffle()
        {
            bool isEnabled = JsonConfig.settings.enableShuffle ^ true;
            JsonConfig.settings.enableShuffle = isEnabled;
            MainMenu.shuffleItem.Checked = isEnabled;

            if (JsonConfig.settings.enableShuffle)
            {
                JsonConfig.settings.lastShuffleDate = null;
                AppContext.wpEngine.RunScheduler();
            }
        }

        private static void ShuffleWallpaper()
        {
            List<string> shuffleHistory = JsonConfig.settings.shuffleHistory?.ToList() ?? new List<string>();
            List<ThemeConfig> themeChoices = new List<ThemeConfig>();
            ThemeConfig nextTheme;

            foreach (ThemeConfig theme in ThemeManager.themeSettings)
            {
                if (!shuffleHistory.Contains(theme.themeId) && (theme.imageFilename != null))
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

            ThemeManager.currentTheme = nextTheme;
            JsonConfig.settings.themeName = nextTheme.themeId;
            AddThemeToHistory(nextTheme.themeId, themeChoices.Count == 0);
        }
    }
}
