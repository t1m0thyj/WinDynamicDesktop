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

        private DateTime? nextUpdateTime;

        public static bool isNightNow;  // TODO Determine how dark mode should work

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
            // TODO Change system theme at sunrise/sunset not at these intervals
        }

        public void RunScheduler()
        {
            schedulerTimer.Stop();

            SolarData todaysData = SunriseSunsetService.GetSolarData(DateTime.Today);
            DaySegment currentSegment;

            DateTime[] solarTimes = new DateTime[4];
            solarTimes[0] = GetSolarTime(todaysData, DaySegment.Sunrise);
            solarTimes[1] = GetSolarTime(todaysData, DaySegment.Day);
            solarTimes[2] = GetSolarTime(todaysData, DaySegment.Sunset);
            solarTimes[3] = GetSolarTime(todaysData, DaySegment.Night);

            if (solarTimes[0] <= DateTime.Now && DateTime.Now < solarTimes[1])
            {
                currentSegment = DaySegment.Sunrise;
            }
            else if (solarTimes[1] <= DateTime.Now && DateTime.Now < solarTimes[2])
            {
                currentSegment = DaySegment.Day;
            }
            else if (solarTimes[2] <= DateTime.Now && DateTime.Now < solarTimes[3])
            {
                currentSegment = DaySegment.Sunset;
            }
            else
            {
                currentSegment = DaySegment.Night;
            }

            isNightNow = (currentSegment == DaySegment.Night);

            if (ThemeManager.currentTheme != null)
            {
                nextUpdateTime = UpdateImage(solarTimes, currentSegment);
            }
            else if (!isNightNow)
            {
                nextUpdateTime = solarTimes[3];
            }
            else if (DateTime.Now < solarTimes[0])
            {
                nextUpdateTime = solarTimes[0];
            }
            else
            {
                SolarData tomorrowsData = SunriseSunsetService.GetSolarData(
                    DateTime.Today.AddDays(1));
                nextUpdateTime = GetSolarTime(tomorrowsData, DaySegment.Sunrise);
            }

            SystemThemeChanger.TryUpdateSystemTheme();
            JsonConfig.SaveConfig();

            StartTimer(nextUpdateTime.Value);
        }

        private DateTime GetSolarTime(SolarData data, DaySegment segment)
        {
            switch (segment)
            {
                case DaySegment.Sunrise:
                    return new DateTime(data.SunriseTime.Ticks - TimeSpan.TicksPerHour);
                case DaySegment.Day:
                    return new DateTime(data.SunriseTime.Ticks + TimeSpan.TicksPerHour);
                case DaySegment.Sunset:
                    return new DateTime(data.SunsetTime.Ticks - TimeSpan.TicksPerHour);
                default:
                    return new DateTime(data.SunsetTime.Ticks + TimeSpan.TicksPerHour);
            }
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
                ThemeManager.currentTheme.themeName, imageFilename);

            WallpaperApi.SetWallpaper(imagePath);

            if (UwpDesktop.IsRunningAsUwp() && JsonConfig.settings.changeLockScreen)
            {
                UwpHelper.SetLockScreenImage(imageFilename);
            }
        }

        private DateTime UpdateImage(DateTime[] solarTimes, DaySegment segment)
        {
            int[] imageList;
            DateTime segmentStart;
            DateTime segmentEnd;

            switch (segment)
            {
                case DaySegment.Sunrise:
                    imageList = ThemeManager.currentTheme.sunriseImageList;
                    segmentStart = solarTimes[0];
                    segmentEnd = solarTimes[1];
                    break;
                case DaySegment.Day:
                    imageList = ThemeManager.currentTheme.dayImageList;
                    segmentStart = solarTimes[1];
                    segmentEnd = solarTimes[2];
                    TimeSpan dayTime = solarTimes[2] - solarTimes[1];
                    break;
                case DaySegment.Sunset:
                    imageList = ThemeManager.currentTheme.sunsetImageList;
                    segmentStart = solarTimes[2];
                    segmentEnd = solarTimes[3];
                    break;
                default:
                    imageList = ThemeManager.currentTheme.nightImageList;

                    if (DateTime.Now < solarTimes[0])
                    {
                        SolarData yesterdaysData = SunriseSunsetService.GetSolarData(
                            DateTime.Today.AddDays(-1));
                        segmentStart = GetSolarTime(yesterdaysData, DaySegment.Night);
                        segmentEnd = solarTimes[0];
                    }
                    else
                    {
                        segmentStart = solarTimes[3];
                        SolarData tomorrowsData = SunriseSunsetService.GetSolarData(
                            DateTime.Today.AddDays(1));
                        segmentEnd = GetSolarTime(tomorrowsData, DaySegment.Sunrise);
                    }

                    break;
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
                intervalTicks = 0;
            }

            TimeSpan interval = new TimeSpan(intervalTicks);

            schedulerTimer.Interval = interval.TotalMilliseconds;
            schedulerTimer.Start();
        }

        private void HandleTimerEvent(bool updateLocation)
        {
            // TODO Figure out how to handle no theme correctly
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
