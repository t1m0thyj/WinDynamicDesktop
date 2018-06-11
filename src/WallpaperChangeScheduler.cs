using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class WallpaperChangeScheduler
    {
        private string imageFilename = "mojave_dynamic_{0}.jpeg";
        private int[] dayImages = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        private int[] nightImages = new[] { 13, 14, 15, 16, 1 };
        private int lastImageNumber = -1;

        private WeatherData yesterdaysData;
        private WeatherData todaysData;
        private WeatherData tomorrowsData;
        private Timer wallpaperTimer;

        private string GetDateString(int todayDelta=0)
        {
            DateTime date = DateTime.Today;
            if (todayDelta != 0)
            {
                date = date.AddDays(todayDelta);   
            }

            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public void StartScheduler()
        {
            SunriseSunsetService service = new SunriseSunsetService();

            todaysData = service.GetWeatherData(
                JsonConfig.settings.Latitude, JsonConfig.settings.Longitude, GetDateString());
            
            if (DateTime.Now < todaysData.SunriseTime)
            {
                // Before sunrise
                yesterdaysData = service.GetWeatherData(
                    JsonConfig.settings.Latitude, JsonConfig.settings.Longitude, GetDateString(-1));
                tomorrowsData = null;
            }
            else if (DateTime.Now > todaysData.SunsetTime)
            {
                // After sunset
                yesterdaysData = null;
                tomorrowsData = service.GetWeatherData(
                    JsonConfig.settings.Latitude, JsonConfig.settings.Longitude, GetDateString(1));
            }
            else
            {
                // Between sunrise and sunset
                yesterdaysData = null;
                tomorrowsData = null;
            }
        }

        private void SetWallpaper(int imageNumber)
        {
            Uri wallpaperUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "images",
                String.Format(imageFilename, imageNumber)));

            Wallpaper.Set(wallpaperUri, Wallpaper.Style.Stretched);
        }

        private void wallpaperTimer_Tick(object sender, EventArgs e)
        {
            if (lastImageNumber > 0 && lastImageNumber < 16)
            {
                SetWallpaper(lastImageNumber++);
            }
            else
            {
                SetWallpaper(1);
                lastImageNumber = 1;
            }
        }

        private void StartDaySchedule()
        {
            TimeSpan dayTime = todaysData.SunsetTime - todaysData.SunriseTime;
            TimeSpan timerLength = new TimeSpan(dayTime.Ticks / dayImages.Length);

            wallpaperTimer = new Timer();
            wallpaperTimer.Interval = timerLength.Milliseconds;
            wallpaperTimer.Tick += new EventHandler(wallpaperTimer_Tick);
            wallpaperTimer.Start();
        }

        private void StartNightSchedule()
        {

        }

        private void UpdateDayImage()
        {
            int imageNumber = 4;

            if (imageNumber != lastImageNumber)
            {
                SetWallpaper(imageNumber);
                lastImageNumber = imageNumber;
            }

            if (imageNumber != dayImages[dayImages.Length - 1])
            {
                wallpaperTimer.Start();
            }
            else
            {
                StartNightSchedule();
            }
        }
    }
}
