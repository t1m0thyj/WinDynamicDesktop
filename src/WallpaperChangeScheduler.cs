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
        private string imageFilename = "mojave_dynamic_{0}.jpeg";
        private int[] dayImages = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        private int[] nightImages = new[] { 13, 14, 15, 16, 1 };

        private string lastDate = "yyyy-MM-dd";
        private int lastImageNumber = -1;
        private bool isSunUp;

        private WeatherData yesterdaysData;
        private WeatherData todaysData;
        private WeatherData tomorrowsData;

        public Timer wallpaperTimer;

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
            SunriseSunsetService service = new SunriseSunsetService();

            WeatherData data = service.GetWeatherData(
                JsonConfig.settings.Latitude, JsonConfig.settings.Longitude, dateStr);

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
            Uri wallpaperUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "images",
                String.Format(imageFilename, imageId)));

            Wallpaper.Set(wallpaperUri, Wallpaper.Style.Stretched);
        }

        public void StartScheduler()
        {
            if (wallpaperTimer != null)
            {
                wallpaperTimer.Stop();
            }

            string currentDate = GetDateString();
            if (currentDate != lastDate)
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

            if (imageNumber != lastImageNumber)
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

            if (imageNumber != lastImageNumber)
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
