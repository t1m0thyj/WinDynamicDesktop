using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace WinDynamicDesktop
{
    public class AppConfig
    {
        public string location { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public bool darkMode { get; set; }
        public bool hideTrayIcon { get; set; }
        public bool disableAutoUpdate { get; set; }
        public string lastUpdateCheck { get; set; }
        public bool changeSystemTheme { get; set; }
        public string themeName { get; set; }
    }

    public class ThemeConfig
    {
        public string themeName { get; set; }
        public string imagesZipUri { get; set; }
        public string imageFilename { get; set; }
        public int[] dayImageList { get; set; }
        public int[] nightImageList { get; set; }
    }

    class JsonConfig
    {
        public static AppConfig settings = new AppConfig();
        public static bool firstRun = !File.Exists("settings.conf");

        public static ThemeConfig themeSettings;

        public static void LoadConfig()
        {
            if (!firstRun)
            {
                settings = JsonConvert.DeserializeObject<AppConfig>(
                    File.ReadAllText("settings.conf"));
            }

            ThemeManager.LoadAllThemes();
        }

        public static void SaveConfig()
        {
            File.WriteAllText("settings.conf", JsonConvert.SerializeObject(settings));
        }
        
        public static ThemeConfig LoadTheme(string name)
        {
            string themeJson;

            if (name == "Mojave_Desert")
            {
                themeJson = Encoding.UTF8.GetString(Properties.Resources.jsonMojaveDesert);
            }
            else if (name == "Solar_Gradients")
            {
                themeJson = Encoding.UTF8.GetString(Properties.Resources.jsonSolarGradients);
            }
            else
            {
                themeJson = File.ReadAllText(name + ".json");
            }

            ThemeConfig theme = JsonConvert.DeserializeObject<ThemeConfig>(themeJson);
            theme.themeName = name;

            return theme;
        }
    }
}
