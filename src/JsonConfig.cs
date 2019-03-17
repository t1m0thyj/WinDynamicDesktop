using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
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
        public bool useWindowsLocation { get; set; }
        public bool changeLockScreen { get; set; }
        public string language { get; set; }
    }

    public class ThemeConfig
    {
        public string themeId { get; set; }
        public string displayName { get; set; }
        public string imagesZipUri { get; set; }
        public string imageFilename { get; set; }
        public string imageCredits { get; set; }
        public int? dayHighlight { get; set; }
        public int? nightHighlight { get; set; }
        public int[] sunriseImageList { get; set; }
        public int[] dayImageList { get; set; }
        public int[] sunsetImageList { get; set; }
        public int[] nightImageList { get; set; }
    }

    class JsonConfig
    {
        private static string lastJson;
        private static readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);

        public static AppConfig settings = new AppConfig();
        public static bool firstRun = !File.Exists("settings.conf");

        public static void LoadConfig()
        {
            if (!firstRun)
            {
                lastJson = File.ReadAllText("settings.conf");
                settings = JsonConvert.DeserializeObject<AppConfig>(lastJson);
            }
        }

        public static async void SaveConfig()
        {
            string newJson = JsonConvert.SerializeObject(settings);

            if (newJson != lastJson)
            {
                await writeLock.WaitAsync();

                try
                {
                    await Task.Run(() => File.WriteAllText("settings.conf", newJson));
                    lastJson = newJson;
                }
                finally
                {
                    writeLock.Release();
                }
            }
        }
        
        public static ThemeConfig LoadTheme(string name)
        {
            string themeJson;

            if (ThemeManager.defaultThemes.Contains(name))
            {
                themeJson = Encoding.UTF8.GetString((byte[])Properties.Resources.ResourceManager.
                    GetObject(name + "_json"));
            }
            else
            {
                themeJson = File.ReadAllText(Path.Combine("themes", name, "theme.json"));
            }

            ThemeConfig theme = JsonConvert.DeserializeObject<ThemeConfig>(themeJson);
            theme.themeId = name;

            return theme;
        }
    }
}
