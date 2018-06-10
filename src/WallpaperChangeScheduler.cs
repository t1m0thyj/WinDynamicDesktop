using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;

namespace WinDynamicDesktop
{
    class WallpaperChangeScheduler
    {
        public LocationConfig config;

        private WeatherData yesterdaysData;
        private WeatherData todaysData;
        private WeatherData tomorrowsData;

        private string imageFilename = "mojave_dynamic_{0}.jpeg";
        private int[] dayImages = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        private int[] nightImages = new[] { 13, 14, 15, 16, 1 };

        public WallpaperChangeScheduler(LocationConfig configObj)
        {
            config = configObj;
        }

        private string GetDateString(int todayDelta=0)
        {
            DateTime date = DateTime.Today;
            if (todayDelta != 0)
            {
                date = date.AddDays(todayDelta);   
            }

            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public void startScheduler()
        {
            SunriseSunsetService service = new SunriseSunsetService();

            todaysData = service.GetWeatherData(
                config.Latitude, config.Longitude, GetDateString());
            
            if (DateTime.Now < todaysData.SunriseTime)
            {
                // Before sunrise
                yesterdaysData = service.GetWeatherData(
                    config.Latitude, config.Longitude, GetDateString(-1));
                tomorrowsData = null;
            }
            else if (DateTime.Now > todaysData.SunsetTime)
            {
                // After sunset
                yesterdaysData = null;
                tomorrowsData = service.GetWeatherData(
                    config.Latitude, config.Longitude, GetDateString(1));
            }
            else
            {
                // Between sunrise and sunset
                yesterdaysData = null;
                tomorrowsData = null;
            }

            TimeSpan diff = todaysData.SunsetTime - todaysData.SunriseTime;

            /*Wallpaper.Set(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "images",
                "mojave_dynamic_1.jpeg")), Wallpaper.Style.Stretched);*/
        }

        private void SetWallpaper(int imageNumber)
        {
            Uri wallpaperUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "images",
                String.Format(imageFilename, imageNumber)));

            Wallpaper.Set(wallpaperUri, Wallpaper.Style.Stretched);
        }

        private void RunDaySchedule()
        {

        }

        private void RunNightSchedule()
        {

        }
    }
}
