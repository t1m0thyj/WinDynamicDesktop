using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class WallpaperChangeScheduler
    {
        private int[] dayImages;
        private int[] nightImages;
        private int lastImageId;

        public static bool isDayNow;

        private SolarData yesterdaysData;
        private SolarData todaysData;
        private SolarData tomorrowsData;

        private Timer wallpaperTimer = new Timer();
        private TimeSpan timerError = new TimeSpan(TimeSpan.TicksPerMillisecond * 55);

        public WallpaperChangeScheduler()
        {
            wallpaperTimer.Tick += OnWallpaperTimerTick;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.TimeChanged += OnTimeChanged;

            if (ThemeManager.currentTheme != null)
            {
                LoadImageLists();
            }
        }

        public void LoadImageLists()
        {
            nightImages = ThemeManager.currentTheme.nightImageList;

            if (!JsonConfig.settings.darkMode)
            {
                dayImages = ThemeManager.currentTheme.dayImageList;
            }
            else
            {
                dayImages = nightImages;
            }
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
            string imageFilename = ThemeManager.currentTheme.imageFilename.Replace("*",
                imageId.ToString());
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "themes",
                ThemeManager.currentTheme.themeName, imageFilename);

            WallpaperApi.SetWallpaper(imagePath);

            if (UwpDesktop.IsRunningAsUwp() && JsonConfig.settings.changeLockScreen)
            {
                UwpHelper.SetLockScreenImage(imagePath);
            }

            lastImageId = imageId;
        }

        public void RunScheduler()
        {
            wallpaperTimer.Stop();

            DateTime today = DateTime.Today;
            todaysData = SunriseSunsetService.GetSolarData(today);

            if (DateTime.Now < todaysData.SunriseTime + timerError)
            {
                // Before sunrise
                yesterdaysData = SunriseSunsetService.GetSolarData(today.AddDays(-1));
                tomorrowsData = null;
            }
            else if (DateTime.Now >= todaysData.SunsetTime - timerError)
            {
                // After sunset
                yesterdaysData = null;
                tomorrowsData = SunriseSunsetService.GetSolarData(today.AddDays(1));
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
            JsonConfig.SaveConfig();
        }

        private void UpdateDayImage()
        {
            TimeSpan dayTime = todaysData.SunsetTime - todaysData.SunriseTime;

            if (ThemeManager.currentTheme != null)
            {
                TimeSpan timerLength = new TimeSpan(dayTime.Ticks / dayImages.Length);
                int imageNumber = GetImageNumber(todaysData.SunriseTime, timerLength);

                if (imageNumber >= dayImages.Length)
                {
                    UpdateNightImage();
                    return;
                }

                StartTimer(todaysData.SunriseTime.Ticks + timerLength.Ticks * (imageNumber + 1)
                    - DateTime.Now.Ticks, timerLength);

                if (dayImages[imageNumber] != lastImageId)
                {
                    SetWallpaper(dayImages[imageNumber]);
                }
            }
            else
            {
                StartTimer(todaysData.SunsetTime.Ticks - DateTime.Now.Ticks, dayTime);
            }
        }

        private void UpdateNightImage()
        {
            SolarData day1Data = (yesterdaysData == null) ? todaysData : yesterdaysData;
            SolarData day2Data = (yesterdaysData == null) ? tomorrowsData : todaysData;

            TimeSpan nightTime = day2Data.SunriseTime - day1Data.SunsetTime;

            if (ThemeManager.currentTheme != null)
            {
                TimeSpan timerLength = new TimeSpan(nightTime.Ticks / nightImages.Length);
                int imageNumber = GetImageNumber(day1Data.SunsetTime, timerLength);

                if (imageNumber >= nightImages.Length)
                {
                    UpdateDayImage();
                    return;
                }

                StartTimer(day1Data.SunsetTime.Ticks + timerLength.Ticks * (imageNumber + 1)
                    - DateTime.Now.Ticks, timerLength);

                if (nightImages[imageNumber] != lastImageId)
                {
                    SetWallpaper(nightImages[imageNumber]);
                }
            }
            else
            {
                StartTimer(day2Data.SunriseTime.Ticks - DateTime.Now.Ticks, nightTime);
            }
        }

        private void HandleTimerEvent(bool updateLocation)
        {
            if (updateLocation && JsonConfig.settings.useWindowsLocation)
            {
                Task.Run(() => UwpLocation.UpdateGeoposition());
            }

            RunScheduler();
            UpdateChecker.TryCheckAuto();
        }

        private void OnWallpaperTimerTick(object sender, EventArgs e)
        {
            HandleTimerEvent(true);
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume && !wallpaperTimer.Enabled)
            {
                HandleTimerEvent(false);
            }
        }

        private void OnTimeChanged(object sender, EventArgs e)
        {
            HandleTimerEvent(false);
        }
    }
}
