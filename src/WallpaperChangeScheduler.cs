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
        private int[] dayImages;
        private int[] nightImages;
        private string lastDate = "yyyy-MM-dd";
        private int lastImageId = -1;

        private WeatherData yesterdaysData;
        private WeatherData todaysData;
        private WeatherData tomorrowsData;

        public Timer wallpaperTimer = new Timer();

        public WallpaperChangeScheduler()
        {
            nightImages = JsonConfig.imageSettings.nightImageList;

            wallpaperTimer.Tick += new EventHandler(OnWallpaperTimerTick);
        }

        private string GetDateString(int todayDelta = 0)
        {
            DateTime date = DateTime.Today;

            if (todayDelta != 0)
            {
                date = date.AddDays(todayDelta);
            }

            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private WeatherData GetWeatherData(string dateStr)
        {
            WeatherData data = SunriseSunsetService.GetWeatherData(
                JsonConfig.settings.latitude, JsonConfig.settings.longitude, dateStr);

            return data;
        }

        private int GetImageNumber(DateTime startTime, TimeSpan timerLength)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;

            return (int)(elapsedTime.Ticks / timerLength.Ticks);
        }

        private void StartTimer(long tickInterval)
        {
            TimeSpan interval = new TimeSpan(tickInterval);

            wallpaperTimer.Interval = Math.Max(1, (int)interval.TotalMilliseconds);
            wallpaperTimer.Start();
        }

        private void SetWallpaper(int imageId)
        {
            string imageFilename = String.Format(JsonConfig.imageSettings.imageFilename, imageId);
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "images", imageFilename);

            WallpaperChanger.EnableTransitions();
            WallpaperChanger.SetWallpaper(imagePath);

            lastImageId = imageId;
        }

        public void RunScheduler(bool forceRefresh = false)
        {
            if (wallpaperTimer != null)
            {
                wallpaperTimer.Stop();
            }

            string currentDate = GetDateString();
            bool shouldRefresh = currentDate != lastDate || forceRefresh;

            if (shouldRefresh)
            {
                todaysData = GetWeatherData(currentDate);
                lastDate = currentDate;
            }

            if (DateTime.Now < todaysData.SunriseTime)
            {
                // Before sunrise
                if (shouldRefresh || yesterdaysData == null)
                {
                    yesterdaysData = GetWeatherData(GetDateString(-1));
                }

                tomorrowsData = null;
            }
            else if (DateTime.Now > todaysData.SunsetTime)
            {
                // After sunset
                yesterdaysData = null;

                if (shouldRefresh || tomorrowsData == null)
                {
                    tomorrowsData = GetWeatherData(GetDateString(1));
                }
            }
            else
            {
                // Between sunrise and sunset
                yesterdaysData = null;
                tomorrowsData = null;
            }

            dayImages = JsonConfig.settings.darkMode ? nightImages : JsonConfig.imageSettings.dayImageList;
            lastImageId = -1;

            if (yesterdaysData == null && tomorrowsData == null)
            {
                UpdateDayImage();
            }
            else
            {
                UpdateNightImage();
            }
        }

        private void UpdateDayImage()
        {
            TimeSpan dayTime = todaysData.SunsetTime - todaysData.SunriseTime;
            TimeSpan timerLength = new TimeSpan(dayTime.Ticks / dayImages.Length);
            int imageNumber = GetImageNumber(todaysData.SunriseTime, timerLength);
            
            StartTimer(todaysData.SunriseTime.Ticks + timerLength.Ticks * (imageNumber + 1)
                - DateTime.Now.Ticks);

            if (dayImages[imageNumber] != lastImageId)
            {
                SetWallpaper(dayImages[imageNumber]);
            }
        }

        private void UpdateNightImage()
        {
            WeatherData day1Data = (yesterdaysData == null) ? todaysData : yesterdaysData;
            WeatherData day2Data = (yesterdaysData == null) ? tomorrowsData : todaysData;

            TimeSpan nightTime = day2Data.SunriseTime - day1Data.SunsetTime;
            TimeSpan timerLength = new TimeSpan(nightTime.Ticks / nightImages.Length);
            int imageNumber = GetImageNumber(day1Data.SunsetTime, timerLength);

            StartTimer(day1Data.SunsetTime.Ticks + timerLength.Ticks * (imageNumber + 1)
                - DateTime.Now.Ticks);

            if (nightImages[imageNumber] != lastImageId)
            {
                SetWallpaper(nightImages[imageNumber]);
            }
        }

        private void OnWallpaperTimerTick(object sender, EventArgs e)
        {
            RunScheduler();
        }
    }
}
