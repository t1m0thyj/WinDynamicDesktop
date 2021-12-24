// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace WinDynamicDesktop
{
    class LaunchSequence
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;

        public static bool IsLocationReady()
        {
            if (JsonConfig.settings.locationMode >= 0)
            {
                return (JsonConfig.settings.latitude.HasValue && JsonConfig.settings.longitude.HasValue);
            }
            else
            {
                return (JsonConfig.settings.sunriseTime != null && JsonConfig.settings.sunsetTime != null);
            }
        }

        public static bool IsThemeReady()
        {
            return (JsonConfig.settings.activeThemes != null && ThemeManager.importPaths.Count == 0);
        }

        public static void NextStep(bool themeReadyOverride = false)
        {
            if (!IsLocationReady())
            {
                LocationManager.ChangeLocation();

                if (JsonConfig.firstRun)
                {
                    AppContext.ShowPopup(_("Welcome! Please enter your location so the app can determine sunrise and " +
                        "sunset times."));
                }
            }
            else if (!IsThemeReady() && !themeReadyOverride)  // Override if theme=None chosen
            {
                ThemeManager.SelectTheme();
            }
            else if (JsonConfig.firstRun)
            {
                AppContext.ShowPopup(_("The app is still running in the background. You can access it at any time by " +
                    "clicking on the icon in the system tray."));

                JsonConfig.firstRun = false;  // Don't show this message again
            }
        }
    }
}
