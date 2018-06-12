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

            SunriseSunsetService service = new SunriseSunsetService();

            string currentDate = GetDateString();
            if (currentDate != lastDate)
            {
                todaysData = service.GetWeatherData(
                    JsonConfig.settings.Latitude, JsonConfig.settings.Longitude, GetDateString());
                lastDate = currentDate;
            }

            if (DateTime.Now < todaysData.SunriseTime)
            {
                // Before sunrise
                yesterdaysData = service.GetWeatherData(
                    JsonConfig.settings.Latitude, JsonConfig.settings.Longitude, GetDateString(-1));
                tomorrowsData = null;

                StartNightSchedule();
            }
            else if (DateTime.Now > todaysData.SunsetTime)
            {
                // After sunset
                yesterdaysData = null;
                tomorrowsData = service.GetWeatherData(
                    JsonConfig.settings.Latitude, JsonConfig.settings.Longitude, GetDateString(1));

                StartNightSchedule();
            }
            else
            {
                // Between sunrise and sunset
                yesterdaysData = null;
                tomorrowsData = null;

                StartDaySchedule();
            }
        }

        private void StartDaySchedule()
        {
            isSunUp = true;
            yesterdaysData = null;

            TimeSpan dayTime = todaysData.SunsetTime - todaysData.SunriseTime;
            TimeSpan timerLength = new TimeSpan(dayTime.Ticks / dayImages.Length);

            int imageNumber = 0;
            while (imageNumber < dayImages.Length)
            {
                if ((todaysData.SunriseTime.Ticks + timerLength.Ticks * imageNumber) <
                        DateTime.Now.Ticks)
                {
                    imageNumber++;
                }
                else
                {
                    break;
                }
            }

            TimeSpan interval = new TimeSpan(todaysData.SunriseTime.Ticks + timerLength.Ticks *
                (imageNumber + 1) - DateTime.Now.Ticks);
            wallpaperTimer = new Timer(interval.Milliseconds);
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
            TimeSpan dayTime = todaysData.SunsetTime - todaysData.SunriseTime;
            TimeSpan timerLength = new TimeSpan(dayTime.Ticks / dayImages.Length);

            wallpaperTimer.Interval = timerLength.Milliseconds;
            wallpaperTimer.Start();

            TimeSpan dayElapsedTime = DateTime.Now - todaysData.SunriseTime;
            int imageNumber = (int)(dayElapsedTime.Ticks / timerLength.Ticks);

            if (imageNumber != lastImageNumber)
            {
                SetWallpaper(dayImages[imageNumber]);
                lastImageNumber = imageNumber;
            }
        }

        private void StartNightSchedule()
        {
            isSunUp = false;

            TimeSpan nightTime = todaysData.SunsetTime - todaysData.SunriseTime;
            TimeSpan timerLength = new TimeSpan(nightTime.Ticks / nightImages.Length);

            int imageNumber = 0;
            while (imageNumber < nightImages.Length)
            {
                if ((todaysData.SunriseTime.Ticks + timerLength.Ticks * imageNumber) <
                        DateTime.Now.Ticks)
                {
                    imageNumber++;
                }
                else
                {
                    break;
                }
            }

            TimeSpan interval = new TimeSpan(todaysData.SunriseTime.Ticks + timerLength.Ticks *
                (imageNumber + 1) - DateTime.Now.Ticks);
            wallpaperTimer = new Timer(interval.Milliseconds);
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
            TimeSpan nightTime = todaysData.SunsetTime - todaysData.SunriseTime;
            TimeSpan timerLength = new TimeSpan(nightTime.Ticks / nightImages.Length);

            wallpaperTimer.Interval = timerLength.Milliseconds;
            wallpaperTimer.Start();

            TimeSpan nightElapsedTime = DateTime.Now - todaysData.SunriseTime;
            int imageNumber = (int)(nightElapsedTime.Ticks / timerLength.Ticks);

            if (imageNumber != lastImageNumber)
            {
                SetWallpaper(nightImages[imageNumber]);
                lastImageNumber = imageNumber;
            }
        }

        private void wallpaperTimer_Elapsed(object sender, EventArgs e)
        {
            if (isSunUp)
            {
                if (lastImageNumber < dayImages.Length - 1)
                {
                    NextDayImage();
                }
                else
                {
                    lastImageNumber = -1;
                    StartNightSchedule();
                }
            }
            else
            {
                if (lastImageNumber < nightImages.Length - 1)
                {
                    NextNightImage();
                }
                else
                {
                    lastImageNumber = -1;
                    StartDaySchedule();
                }
            }
        }
    }
}
