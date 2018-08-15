using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WinDynamicDesktop
{
    class WallpaperChangeScheduler
    {
        private int[] dayImages;
        private int[] nightImages;
        private string lastDate;
        private int lastImageId;

        public static bool isDayNow;

        private WeatherData yesterdaysData;
        private WeatherData todaysData;
        private WeatherData tomorrowsData;

        private Timer wallpaperTimer = new Timer();
        private TimeSpan timerError = new TimeSpan(TimeSpan.TicksPerMillisecond * 55);

        public WallpaperChangeScheduler()
        {
            LoadImageLists();

            wallpaperTimer.Tick += new EventHandler(OnWallpaperTimerTick);
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(OnPowerModeChanged);
        }

        public void LoadImageLists()
        {
            nightImages = JsonConfig.themeSettings.nightImageList;

            if (!JsonConfig.settings.darkMode)
            {
                dayImages = JsonConfig.themeSettings.dayImageList;
            }
            else
            {
                dayImages = nightImages;
            }
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

            return (int)((elapsedTime.Ticks + timerError.Ticks) / timerLength.Ticks);
        }

        private void StartTimer(long intervalTicks, TimeSpan maxInterval)
        {
            if (intervalTicks < timerError.Ticks)
            {
                intervalTicks += maxInterval.Ticks;
            }

            TimeSpan interval = new TimeSpan(intervalTicks);

            wallpaperTimer.Interval = (int)interval.TotalMilliseconds;
            wallpaperTimer.Start();
        }

        private void SetWallpaper(int imageId)
        {
            string imageFilename = JsonConfig.themeSettings.imageFilename.Replace("*", imageId.ToString());
            UwpDesktop.SetWallpaper(imageFilename);

            lastImageId = imageId;
        }

        public void RunScheduler(bool forceRefresh = false)
        {
            wallpaperTimer.Stop();

            string currentDate = GetDateString();
            bool shouldRefresh = currentDate != lastDate || forceRefresh;

            if (shouldRefresh)
            {
                todaysData = GetWeatherData(currentDate);
                lastDate = currentDate;
            }

            if (DateTime.Now < todaysData.SunriseTime + timerError)
            {
                // Before sunrise
                if (shouldRefresh || yesterdaysData == null)
                {
                    yesterdaysData = GetWeatherData(GetDateString(-1));
                }

                tomorrowsData = null;
            }
            else if (DateTime.Now >= todaysData.SunsetTime - timerError)
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

            isDayNow = (yesterdaysData == null && tomorrowsData == null);
            lastImageId = -1;

            if (isDayNow)
            {
                UpdateDayImage();
            }
            else
            {
                UpdateNightImage();
            }

            SystemThemeChanger.TryUpdateSystemTheme();
        }

        private void UpdateDayImage()
        {
            TimeSpan dayTime = todaysData.SunsetTime - todaysData.SunriseTime;
            TimeSpan timerLength = new TimeSpan(dayTime.Ticks / dayImages.Length);
            int imageNumber = GetImageNumber(todaysData.SunriseTime, timerLength);

            StartTimer(todaysData.SunriseTime.Ticks + timerLength.Ticks * (imageNumber + 1)
                - DateTime.Now.Ticks, timerLength);

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
                - DateTime.Now.Ticks, timerLength);

            if (nightImages[imageNumber] != lastImageId)
            {
                SetWallpaper(nightImages[imageNumber]);
            }
        }

        private void OnWallpaperTimerTick(object sender, EventArgs e)
        {
            RunScheduler();
            UpdateChecker.TryCheckAuto();
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume && !wallpaperTimer.Enabled)
            {
                RunScheduler();
            }
        }
    }
}
