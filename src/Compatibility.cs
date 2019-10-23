using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinDynamicDesktop
{
    class Compatibility
    {
        private readonly static string Catalina_JSON = @"{
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

        private readonly static string Mojave_Desert_JSON = @"{
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

        private readonly static string Solar_Gradients_JSON = @"{
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

        // TODO Added 2019-10-10, remove eventually
        public static void CompatibilizeThemes()
        {
            if (Directory.Exists(Path.Combine("themes", "Catalina")) && !File.Exists(Path.Combine("themes", "Catalina", "theme.json")))
            {
                File.WriteAllText(Path.Combine("themes", "Catalina", "theme.json"), Catalina_JSON);
            }

            if (Directory.Exists(Path.Combine("themes", "Mojave_Desert")) && !File.Exists(Path.Combine("themes", "Mojave_Desert", "theme.json")))
            {
                File.WriteAllText(Path.Combine("themes", "Mojave_Desert", "theme.json"), Mojave_Desert_JSON);
            }

            if (Directory.Exists(Path.Combine("themes", "Solar_Gradients")) && !File.Exists(Path.Combine("themes", "Solar_Gradients", "theme.json")))
            {
                File.WriteAllText(Path.Combine("themes", "Solar_Gradients", "theme.json"), Solar_Gradients_JSON);
            }
        }

        // TODO Added 2019-10-22, remove eventually
        public static void CompatibilizeLocale()
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
    }
}
