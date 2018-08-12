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
        public static ThemeConfig themeSettings = new ThemeConfig();
        public static bool firstRun = !File.Exists("settings.conf");

        public static void LoadConfig()
        {
            if (!firstRun)
            {
                settings = JsonConvert.DeserializeObject<AppConfig>(
                    File.ReadAllText("settings.conf"));
            }

            string imagesConf;

            if (File.Exists("images.conf"))
            {
                imagesConf = File.ReadAllText("images.conf");
            }
            else
            {
                imagesConf = Encoding.UTF8.GetString(Properties.Resources.imagesConf);
            }

            themeSettings = JsonConvert.DeserializeObject<ThemeConfig>(imagesConf);
        }

        public static void SaveConfig()
        {
            File.WriteAllText("settings.conf", JsonConvert.SerializeObject(settings));
        }
    }
}
