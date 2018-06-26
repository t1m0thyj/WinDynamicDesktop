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
        public string location { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }

    public class ImagesConfig
    {
        public string themeName { get; set; }
        public string imagesZipUri { get; set; }
        public string imageFilename { get; set; }
        public int[] dayImageList { get; set; }
        public int[] nightImageList { get; set; }
    }

    class JsonConfig
    {
        public static LocationConfig settings = new LocationConfig();
        public static ImagesConfig imageSettings = new ImagesConfig();
        public static bool firstRun = !File.Exists("settings.conf");

        public static void LoadConfig()
        {
            if (!firstRun)
            {
                settings = JsonConvert.DeserializeObject<LocationConfig>(
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

            imageSettings = JsonConvert.DeserializeObject<ImagesConfig>(imagesConf);
        }

        public static void SaveConfig()
        {
            File.WriteAllText("settings.conf", JsonConvert.SerializeObject(settings));
        }
    }
}
