// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Timers;

namespace WinDynamicDesktop
{
    class WallpaperChangeScheduler
    {
        private enum DaySegment { AllDay, AllNight, Sunrise, Day, Sunset, Night };

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

        public void RunScheduler(bool forceImageUpdate = false)
        {
            if (!LaunchSequence.IsLocationReady() || !LaunchSequence.IsThemeReady())
            {
                return;
            }

            schedulerTimer.Stop();

            SolarData data = SunriseSunsetService.GetSolarData(DateTime.Today);
            isSunUp = (data.sunriseTime <= DateTime.Now && DateTime.Now < data.sunsetTime);
            DateTime? nextImageUpdateTime = null;

            if (ThemeManager.currentTheme != null)
            {
                if (forceImageUpdate)
                {
                    lastImagePath = null;
                }

                Tuple<int, long> imageData = GetImageData(data, ThemeManager.currentTheme);
                SetWallpaper(imageData.Item1);
                nextImageUpdateTime = new DateTime(imageData.Item2);
            }

            SystemThemeChanger.TryUpdateSystemTheme();

            if (data.polarPeriod != PolarPeriod.None)
            {
                nextUpdateTime = DateTime.Today.AddDays(1);
            }
            else if (isSunUp)
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
        }

        private DaySegment GetCurrentDaySegment(SolarData data)
        {
            if (data.polarPeriod == PolarPeriod.PolarDay)
            {
                return DaySegment.AllDay;
            }
            else if (data.polarPeriod == PolarPeriod.PolarNight)
            {
                return DaySegment.AllNight;
            }
            else if (data.solarTimes[0] <= DateTime.Now && DateTime.Now < data.solarTimes[1])
            {
                return DaySegment.Sunrise;
            }
            else if (data.solarTimes[1] <= DateTime.Now && DateTime.Now < data.solarTimes[2])
            {
                return DaySegment.Day;
            }
            else if (data.solarTimes[2] <= DateTime.Now && DateTime.Now < data.solarTimes[3])
            {
                return DaySegment.Sunset;
            }
            else
            {
                return DaySegment.Night;
            }
        }

        public Tuple<int, long> GetImageData(SolarData data, ThemeConfig theme)
        {
            int[] imageList;
            DateTime segmentStart;
            DateTime segmentEnd;

            if (!JsonConfig.settings.darkMode)
            {
                switch (GetCurrentDaySegment(data))
                {
                    case DaySegment.AllDay:
                        imageList = theme.dayImageList;
                        segmentStart = DateTime.Today;
                        segmentEnd = DateTime.Today.AddDays(1);
                        BrightnessManager.ChangeBrightness(0);
                        break;
                    case DaySegment.AllNight:
                        imageList = theme.nightImageList;
                        segmentStart = DateTime.Today;
                        segmentEnd = DateTime.Today.AddDays(1);
                        BrightnessManager.ChangeBrightness(1);
                        break;
                    case DaySegment.Sunrise:
                        imageList = theme.sunriseImageList;
                        segmentStart = data.solarTimes[0];
                        segmentEnd = data.solarTimes[1];
                        BrightnessManager.ChangeBrightness(2);
                        break;
                    case DaySegment.Day:
                        imageList = theme.dayImageList;
                        segmentStart = data.solarTimes[1];
                        segmentEnd = data.solarTimes[2];
                        BrightnessManager.ChangeBrightness(3);
                        break;
                    case DaySegment.Sunset:
                        imageList = theme.sunsetImageList;
                        segmentStart = data.solarTimes[2];
                        segmentEnd = data.solarTimes[3];
                        BrightnessManager.ChangeBrightness(4);
                        break;
                    default:
                        imageList = theme.nightImageList;

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

                        BrightnessManager.ChangeBrightness(5);
                        break;
                }
            }
            else
            {
                imageList = theme.nightImageList;

                BrightnessManager.ChangeBrightness(5);

                if (data.polarPeriod != PolarPeriod.None)
                {
                    segmentStart = DateTime.Today;
                    segmentEnd = DateTime.Today.AddDays(1);
                }
                else if (isSunUp)
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

            int imageNumber = (int)((DateTime.Now - segmentStart).Ticks / timerLength.Ticks);
            long nextUpdateTicks = segmentStart.Ticks + timerLength.Ticks * (imageNumber + 1);

            return new Tuple<int, long>(imageList[imageNumber], nextUpdateTicks);
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

            WallpaperApi.EnableTransitions();
            UwpDesktop.GetHelper().SetWallpaper(imageFilename);

            if (UwpDesktop.IsRunningAsUwp() && JsonConfig.settings.changeLockScreen)
            {
                UwpHelper.SetLockScreenImage(imageFilename);
            }

            lastImagePath = imagePath;
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
