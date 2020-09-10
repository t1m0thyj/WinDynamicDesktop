// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace WinDynamicDesktop
{
    public class LegacyConfig
    {
        public bool changeSystemTheme { get; set; }
        public bool changeAppTheme { get; set; }
        public bool changeLockScreen { get; set; }
        public bool useAutoBrightness { get; set; }
        public bool useCustomAutoBrightness { get; set; }
    }

    class UpdateHandler
    {
        private static readonly string Catalina_JSON = @"{
  'imageUrls': [
    'https://onedrive.live.com/download?cid=CC2E3BD0360C1775&resid=CC2E3BD0360C1775%216170&authkey=AIMgqOpAsERVe0c',
    'https://bitbucket.org/t1m0thyj/wdd-themes/downloads/Catalina_images.zip'
  ],
  'imageFilename': 'catalina_*.jpg',
  'imageCredits': 'Apple',
  'sunriseImageList': [
    1
  ],
  'dayImageList': [
    1
  ],
  'sunsetImageList': [
    1
  ],
  'nightImageList': [
    2
  ]
}";

        private static readonly string Mojave_Desert_JSON = @"{
  'imageUrls': [
    'https://onedrive.live.com/download?cid=CC2E3BD0360C1775&resid=CC2E3BD0360C1775%216110&authkey=AOBrcljXRqwNSZo',
    'https://bitbucket.org/t1m0thyj/wdd-themes/downloads/Mojave_Desert_images.zip'
  ],
  'imageFilename': 'mojave_dynamic_*.jpeg',
  'imageCredits': 'Apple',
  'sunriseImageList': [
    1,
    2,
    3
  ],
  'dayImageList': [
    4,
    5,
    6,
    7,
    8,
    9,
    10
  ],
  'sunsetImageList': [
    11,
    12,
    13
  ],
  'nightImageList': [
    14,
    15,
    16
  ]
}";

        private static readonly string Solar_Gradients_JSON = @"{
  'imageUrls': [
    'https://onedrive.live.com/download?cid=CC2E3BD0360C1775&resid=CC2E3BD0360C1775%21721&authkey=AK4kktXlvN1KJzQ',
    'https://bitbucket.org/t1m0thyj/wdd-themes/downloads/Solar_Gradients_images.zip'
  ],
  'imageFilename': 'solar_gradients_*.jpeg',
  'imageCredits': 'Apple',
  'sunriseImageList': [
    4,
    5,
    6
  ],
  'dayImageList': [
    7,
    8,
    9,
    10,
    11
  ],
  'sunsetImageList': [
    12,
    13,
    14
  ],
  'nightImageList': [
    15,
    16,
    1,
    2,
    3
  ]
}";

        public static void CompatibilizeThemes()  // Added 2019-10-10
        {
            if (Directory.Exists(Path.Combine("themes", "Catalina")) &&
                !File.Exists(Path.Combine("themes", "Catalina", "theme.json")))
            {
                File.WriteAllText(Path.Combine("themes", "Catalina", "theme.json"), Catalina_JSON);
            }

            if (Directory.Exists(Path.Combine("themes", "Mojave_Desert")) &&
                !File.Exists(Path.Combine("themes", "Mojave_Desert", "theme.json")))
            {
                File.WriteAllText(Path.Combine("themes", "Mojave_Desert", "theme.json"), Mojave_Desert_JSON);
            }

            if (Directory.Exists(Path.Combine("themes", "Solar_Gradients")) &&
                !File.Exists(Path.Combine("themes", "Solar_Gradients", "theme.json")))
            {
                File.WriteAllText(Path.Combine("themes", "Solar_Gradients", "theme.json"), Solar_Gradients_JSON);
            }
        }

        public static void CompatibilizeLocale()  // Added 2019-10-22
        {
            switch (JsonConfig.settings.language)
            {
                case "cs_CZ":
                    JsonConfig.settings.language = "cs";
                    return;
                case "de_DE":
                    JsonConfig.settings.language = "de";
                    return;
                case "en_US":
                    JsonConfig.settings.language = "en";
                    return;
                case "es_ES":
                    JsonConfig.settings.language = "es";
                    return;
                case "fr_FR":
                    JsonConfig.settings.language = "fr";
                    return;
                case "el_GR":
                    JsonConfig.settings.language = "el";
                    return;
                case "it_IT":
                    JsonConfig.settings.language = "it";
                    return;
                case "mk_MK":
                    JsonConfig.settings.language = "mk";
                    return;
                case "pl_PL":
                    JsonConfig.settings.language = "pl";
                    return;
                case "ro_RO":
                    JsonConfig.settings.language = "ro";
                    return;
                case "ru_RU":
                    JsonConfig.settings.language = "ru";
                    return;
                case "tr_TR":
                    JsonConfig.settings.language = "tr";
                    return;
                case "zh_CN":
                    JsonConfig.settings.language = "zh-Hans";
                    return;
                default:
                    return;
            }
        }

        public static void UpdateToVersion4()  // Added 2020-01-01
        {
            if (!File.Exists("settings.conf"))
            {
                return;
            }
            string jsonText = File.ReadAllText("settings.conf");
            LegacyConfig settings = JsonConvert.DeserializeObject<LegacyConfig>(jsonText);
            bool legacySettingsEnabled = (settings.changeSystemTheme || settings.changeAppTheme ||
                settings.changeLockScreen || settings.useAutoBrightness || settings.useCustomAutoBrightness);
            if (legacySettingsEnabled)
            {
                jsonText = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<AppConfig>(jsonText),
                    Formatting.Indented);
                File.WriteAllText("settings.conf", jsonText);
                MessageDialog.ShowInfo("Updated to WinDynamicDesktop 4.0 successfully. Some features you were using " +
                    "have been disabled because they were removed from the core app. You were using one or more of " +
                    "the following features:\n\n* Change Windows 10 app/system theme\n* Change screen brightness\n* " +
                    "Change lockscreen image\n\nTo re-enable these features, install scripts for them from here: " +
                    "https://windd.info/scripts/");
            }
        }

        public static DateTime SafeParse(string dateTime)  // Added 2020-05-21
        {
            try
            {
                return DateTime.Parse(dateTime, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return DateTime.Parse(dateTime);
            }
        }
    }
}
