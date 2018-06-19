using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Timers;

namespace WinDynamicDesktop
{
    class WallpaperChangeScheduler
    {
        private int[] dayImages;
        private int[] nightImages;
        private bool isSunUp;

        private string lastDate = "yyyy-MM-dd";
        private int lastImageId = -1;
        private int lastImageNumber = -1;

        private WeatherData yesterdaysData;
        private WeatherData todaysData;
        private WeatherData tomorrowsData;

        public bool enableTransitions = true;
        public Timer wallpaperTimer;

        public WallpaperChangeScheduler()
        {
            dayImages = JsonConfig.imageSettings.dayImageList;
            nightImages = JsonConfig.imageSettings.nightImageList;
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

        private int GetImageNumber(DateTime startTime, TimeSpan timerLength, int imageCount)
        {
            int imageNumber = 0;

            while (imageNumber < imageCount)
            {
                if ((startTime.Ticks + timerLength.Ticks * (imageNumber + 1)) < DateTime.Now.Ticks)
                {
                    imageNumber++;
                }
                else
                {
                    break;
                }
            }

            return imageNumber;
        }

        private void SetWallpaper(int imageId)
        {
            string imageFilename = String.Format(JsonConfig.imageSettings.imageFilename, imageId);
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "images", imageFilename);

            if (enableTransitions)
            {
                WallpaperChanger.EnableTransitions();
            }

            WallpaperChanger.SetWallpaper(imagePath);
            
            lastImageId = imageId;
        }

        public void StartScheduler(bool forceRefresh = false)
        {
            if (wallpaperTimer != null)
            {
                wallpaperTimer.Stop();
            }

            string currentDate = GetDateString();
            if (currentDate != lastDate || forceRefresh)
            {
                todaysData = GetWeatherData(currentDate);
                lastDate = currentDate;
            }

            if (DateTime.Now < todaysData.SunriseTime)
            {
                // Before sunrise
                yesterdaysData = GetWeatherData(GetDateString(-1));
                tomorrowsData = null;
                isSunUp = false;
            }
            else if (DateTime.Now > todaysData.SunsetTime)
            {
                // After sunset
                yesterdaysData = null;
                tomorrowsData = GetWeatherData(GetDateString(1));
                isSunUp = false;
            }
            else
            {
                // Between sunrise and sunset
                yesterdaysData = null;
                tomorrowsData = null;
                isSunUp = true;
            }

            if (isSunUp)
            {
                StartDaySchedule();
            }
            else
            {
                StartNightSchedule();
            }

            lastImageId = 0;
        }

        private void StartDaySchedule()
        {
            TimeSpan dayTime = todaysData.SunsetTime - todaysData.SunriseTime;
            TimeSpan timerLength = new TimeSpan(dayTime.Ticks / dayImages.Length);

            int imageNumber = GetImageNumber(todaysData.SunriseTime, timerLength, dayImages.Length);
            TimeSpan interval = new TimeSpan(todaysData.SunriseTime.Ticks + timerLength.Ticks *
                (imageNumber + 1) - DateTime.Now.Ticks);

            wallpaperTimer = new Timer(interval.TotalMilliseconds);
            wallpaperTimer.Elapsed += new ElapsedEventHandler(wallpaperTimer_Elapsed);
            wallpaperTimer.Start();

            if (dayImages[imageNumber] != lastImageId)
            {
                SetWallpaper(dayImages[imageNumber]);
                lastImageNumber = imageNumber;
            }
        }

        private void NextDayImage()
        {
            if (lastImageNumber == dayImages.Length - 1)
            {
                SwitchToNight();
                return;
            }

            TimeSpan dayTime = todaysData.SunsetTime - todaysData.SunriseTime;
            TimeSpan timerLength = new TimeSpan(dayTime.Ticks / dayImages.Length);

            wallpaperTimer.Interval = timerLength.TotalMilliseconds;
            wallpaperTimer.Start();

            lastImageNumber++;
            SetWallpaper(dayImages[lastImageNumber]);
        }

        private void SwitchToNight()
        {
            tomorrowsData = GetWeatherData(GetDateString(1));
            lastImageNumber = -1;

            StartNightSchedule();
        }

        private void StartNightSchedule()
        {
            WeatherData day1Data = (yesterdaysData == null) ? todaysData : yesterdaysData;
            WeatherData day2Data = (yesterdaysData == null) ? tomorrowsData : todaysData;
            
            TimeSpan nightTime = day2Data.SunriseTime - day1Data.SunsetTime;
            TimeSpan timerLength = new TimeSpan(nightTime.Ticks / nightImages.Length);

            int imageNumber = GetImageNumber(day1Data.SunsetTime, timerLength, nightImages.Length);
            TimeSpan interval = new TimeSpan(day1Data.SunsetTime.Ticks + timerLength.Ticks *
                (imageNumber + 1) - DateTime.Now.Ticks);

            wallpaperTimer = new Timer(interval.TotalMilliseconds);
            wallpaperTimer.Elapsed += new ElapsedEventHandler(wallpaperTimer_Elapsed);
            wallpaperTimer.Start();

            if (nightImages[imageNumber] != lastImageId)
            {
                SetWallpaper(nightImages[imageNumber]);
                lastImageNumber = imageNumber;
            }
        }

        private void NextNightImage()
        {
            if (lastImageNumber == nightImages.Length - 1)
            {
                SwitchToDay();
                return;
            }

            WeatherData day1Data = (yesterdaysData == null) ? todaysData : yesterdaysData;
            WeatherData day2Data = (yesterdaysData == null) ? tomorrowsData : todaysData;

            TimeSpan nightTime = day2Data.SunriseTime - day1Data.SunsetTime;
            TimeSpan timerLength = new TimeSpan(nightTime.Ticks / nightImages.Length);

            wallpaperTimer.Interval = timerLength.TotalMilliseconds;
            wallpaperTimer.Start();

            lastImageNumber++;
            SetWallpaper(nightImages[lastImageNumber]);

            if (DateTime.Now.Hour == 0 && tomorrowsData != null)
            {
                yesterdaysData = todaysData;
                todaysData = tomorrowsData;
                tomorrowsData = null;
                lastDate = GetDateString();
            }
        }

        private void SwitchToDay()
        {
            yesterdaysData = null;
            lastImageNumber = -1;

            StartDaySchedule();
        }

        private void wallpaperTimer_Elapsed(object sender, EventArgs e)
        {
            if (isSunUp)
            {
                NextDayImage();
            }
            else
            {
                NextNightImage();
            }
        }
    }
}
