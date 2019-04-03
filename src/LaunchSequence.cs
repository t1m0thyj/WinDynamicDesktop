// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    class LaunchSequence
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;

        public static bool IsLocationReady()
        {
            return JsonConfig.settings.latitude != null && JsonConfig.settings.longitude != null;
        }

        public static bool IsThemeReady()
        {
            return (!(ThemeManager.currentTheme == null && (JsonConfig.firstRun ||
                JsonConfig.settings.themeName != null)) && ThemeManager.importPaths.Count == 0);
        }

        public static void NextStep(bool themeReadyOverride = false)
        {
            if (!IsLocationReady())
            {
                LocationManager.ChangeLocation();

                if (JsonConfig.firstRun)
                {
                    AppContext.ShowPopup(_("Welcome! Please enter your location so the app can " +
                        "determine sunrise and sunset times."));
                }
            }
            else if (!IsThemeReady() && !themeReadyOverride)  // Override if theme=None chosen
            {
                if (ThemeManager.filesVerified)
                {
                    ThemeManager.SelectTheme();
                }
            }
            else if (JsonConfig.firstRun)
            {
                AppContext.ShowPopup(_("The app is still running in the background. You can " +
                    "access it at any time by clicking on the icon in the system tray."));

                JsonConfig.firstRun = false;  // Don't show this message again
            }
        }

        public static void Launch()
        {
            if (IsLocationReady() && IsThemeReady())
            {
                AppContext.wpEngine.RunScheduler();
            }
            else
            {
                NextStep();
            }
        }
    }
}
