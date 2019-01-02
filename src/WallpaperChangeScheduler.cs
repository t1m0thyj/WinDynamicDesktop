using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Win32;
using System.IO;
using System.Timers;

namespace WinDynamicDesktop
{
    class WallpaperChangeScheduler
    {
        private enum DaySegment { Sunrise, Day, Sunset, Night };

        private string lastImagePath;
        private DateTime? nextUpdateTime;

        public static bool isSunUp;

        private Timer backgroundTimer = new Timer();
        private Timer schedulerTimer = new Timer();
        private const long timerError = (long)(TimeSpan.TicksPerMillisecond * 15.6);

        public WallpaperChangeScheduler()
        {
            backgroundTimer.AutoReset = true;
            backgroundTimer.Interval = 60e3;
            backgroundTimer.Elapsed += OnBackgroundTimerElapsed;
            backgroundTimer.Start();

            schedulerTimer.Elapsed += OnSchedulerTimerElapsed;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.TimeChanged += OnTimeChanged;
        }

        public void RunScheduler()
        {
            schedulerTimer.Stop();

            SolarData data = SunriseSunsetService.GetSolarData(DateTime.Today);
            DaySegment currentSegment;

            if (data.solarTimes[0] <= DateTime.Now && DateTime.Now < data.solarTimes[1])
            {
                currentSegment = DaySegment.Sunrise;
            }
            else if (data.solarTimes[1] <= DateTime.Now && DateTime.Now < data.solarTimes[2])
            {
                currentSegment = DaySegment.Day;
            }
            else if (data.solarTimes[2] <= DateTime.Now && DateTime.Now < data.solarTimes[3])
            {
                currentSegment = DaySegment.Sunset;
            }
            else
            {
                currentSegment = DaySegment.Night;
            }

            isSunUp = (data.sunriseTime <= DateTime.Now && DateTime.Now < data.sunsetTime);
            DateTime? nextImageUpdateTime = null;

            if (ThemeManager.currentTheme != null)
            {
                nextImageUpdateTime = UpdateImage(data, currentSegment);
            }

            SystemThemeChanger.TryUpdateSystemTheme();

            if (isSunUp)
            {
                nextUpdateTime = data.sunsetTime;
            }
            else if (DateTime.Now < data.solarTimes[0])
            {
                nextUpdateTime = data.sunriseTime;
            }
            else
            {
                SolarData tomorrowsData = SunriseSunsetService.GetSolarData(
                    DateTime.Today.AddDays(1));
                nextUpdateTime = tomorrowsData.sunriseTime;
            }

            if (nextImageUpdateTime.HasValue && nextImageUpdateTime.Value < nextUpdateTime.Value)
            {
                nextUpdateTime = nextImageUpdateTime;
            }

            StartTimer(nextUpdateTime.Value);
            JsonConfig.SaveConfig(); 
        }

        private int GetImageNumber(DateTime startTime, TimeSpan timerLength)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;

            return (int)(elapsedTime.Ticks / timerLength.Ticks);
        }

        private void SetWallpaper(int imageId)
        {
            string imageFilename = ThemeManager.currentTheme.imageFilename.Replace("*",
                imageId.ToString());
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "themes",
                ThemeManager.currentTheme.themeId, imageFilename);

            if (imagePath == lastImagePath)
            {
                return;
            }

            WallpaperApi.SetWallpaper(imagePath);

            if (UwpDesktop.IsRunningAsUwp() && JsonConfig.settings.changeLockScreen)
            {
                UwpHelper.SetLockScreenImage(imageFilename);
            }

            lastImagePath = imagePath;
        }

        private DateTime UpdateImage(SolarData data, DaySegment segment)
        {
            int[] imageList;
            DateTime segmentStart;
            DateTime segmentEnd;

            if (!JsonConfig.settings.darkMode)
            {
                switch (segment)
                {
                    case DaySegment.Sunrise:
                        imageList = ThemeManager.currentTheme.sunriseImageList;
                        segmentStart = data.solarTimes[0];
                        segmentEnd = data.solarTimes[1];
                        break;
                    case DaySegment.Day:
                        imageList = ThemeManager.currentTheme.dayImageList;
                        segmentStart = data.solarTimes[1];
                        segmentEnd = data.solarTimes[2];
                        break;
                    case DaySegment.Sunset:
                        imageList = ThemeManager.currentTheme.sunsetImageList;
                        segmentStart = data.solarTimes[2];
                        segmentEnd = data.solarTimes[3];
                        break;
                    default:
                        imageList = ThemeManager.currentTheme.nightImageList;

                        if (DateTime.Now < data.solarTimes[0])
                        {
                            SolarData yesterdaysData = SunriseSunsetService.GetSolarData(
                                DateTime.Today.AddDays(-1));
                            segmentStart = yesterdaysData.solarTimes[3];
                            segmentEnd = data.solarTimes[0];
                        }
                        else
                        {
                            segmentStart = data.solarTimes[3];
                            SolarData tomorrowsData = SunriseSunsetService.GetSolarData(
                                DateTime.Today.AddDays(1));
                            segmentEnd = tomorrowsData.solarTimes[0];
                        }

                        break;
                }
            }
            else
            {
                imageList = ThemeManager.currentTheme.nightImageList;

                if (isSunUp)
                {
                    segmentStart = data.sunriseTime;
                    segmentEnd = data.sunsetTime;
                }
                else if (DateTime.Now < data.sunriseTime)
                {
                    SolarData yesterdaysData = SunriseSunsetService.GetSolarData(
                        DateTime.Today.AddDays(-1));
                    segmentStart = yesterdaysData.sunsetTime;
                    segmentEnd = data.sunriseTime;
                }
                else
                {
                    segmentStart = data.sunsetTime;
                    SolarData tomorrowsData = SunriseSunsetService.GetSolarData(
                        DateTime.Today.AddDays(1));
                    segmentEnd = tomorrowsData.sunriseTime;
                }
            }

            TimeSpan segmentLength = segmentEnd - segmentStart;
            TimeSpan timerLength = new TimeSpan(segmentLength.Ticks / imageList.Length);

            int imageNumber = GetImageNumber(segmentStart, timerLength);
            long nextUpdateTicks = segmentStart.Ticks + timerLength.Ticks * (imageNumber + 1);

            SetWallpaper(imageList[imageNumber]);

            return new DateTime(nextUpdateTicks);
        }

        private void StartTimer(DateTime futureTime)
        {
            long intervalTicks = futureTime.Ticks - DateTime.Now.Ticks;

            if (intervalTicks < timerError)
            {
                intervalTicks = 1;
            }

            TimeSpan interval = new TimeSpan(intervalTicks);

            schedulerTimer.Interval = interval.TotalMilliseconds;
            schedulerTimer.Start();
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

        private void OnBackgroundTimerElapsed(object sender, EventArgs e)
        {
            if (nextUpdateTime.HasValue && DateTime.Now >= nextUpdateTime.Value)
            {
                HandleTimerEvent(true);
            }
        }

        private void OnSchedulerTimerElapsed(object sender, EventArgs e)
        {
            HandleTimerEvent(true);
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
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
