using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace WinDynamicDesktop
{
    public class LocationConfig
    {
        public string Location { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    class JsonConfig
    {
        public static LocationConfig settings = new LocationConfig();
        public static bool firstRun = !File.Exists("settings.conf");

        public static void LoadConfig()
        {
            if (!firstRun)
            {
                settings = JsonConvert.DeserializeObject<LocationConfig>(
                    File.ReadAllText("settings.conf"));
            }
        }

        public static void SaveConfig()
        {
            File.WriteAllText("settings.conf", JsonConvert.SerializeObject(settings));
        }
    }
}
