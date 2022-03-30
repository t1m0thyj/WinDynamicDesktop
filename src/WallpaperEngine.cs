// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public class DisplayEvent
    {
        public ThemeConfig currentTheme;
        public int daySegment2;
        public int? daySegment4;
        public int displayIndex;
        public int imageId;
        public string lastImagePath;
        public long nextUpdateTicks;
    }

    class WallpaperEngine
    {
        public List<DisplayEvent> displayEvents;
        public FullScreenApi fullScreenChecker;

        private DateTime? nextUpdateTime;
        private System.Timers.Timer backgroundTimer = new System.Timers.Timer();
        private System.Timers.Timer schedulerTimer = new System.Timers.Timer();
        private const long timerError = (long)(TimeSpan.TicksPerMillisecond * 15.6);

        public WallpaperEngine()
        {
            fullScreenChecker = new FullScreenApi(this);
            VirtualDesktopApi.Initialize();

            backgroundTimer.AutoReset = true;
            backgroundTimer.Interval = 60e3;
            backgroundTimer.Elapsed += OnBackgroundTimerElapsed;
            backgroundTimer.Start();

            schedulerTimer.Elapsed += OnSchedulerTimerElapsed;
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.TimeChanged += OnTimeChanged;
        }

        public void RunScheduler(bool forceImageUpdate = false)
        {
            if (!LaunchSequence.IsLocationReady() || !LaunchSequence.IsThemeReady())
            {
                return;
            }
            else if (displayEvents == null || forceImageUpdate)
            {
                displayEvents = new List<DisplayEvent> { null };
                RefreshDisplayList(false);
            }

            schedulerTimer.Stop();
            SolarData data = SunriseSunsetService.GetSolarData(DateTime.Today);
            long nextDisplayUpdateTicks = long.MaxValue;

            ThemeShuffler.MaybeShuffleWallpaper();

            for (int i = 0; i < displayEvents.Count; i++)
            {
                // TODO After Nvidia update, Display 1 became theme 2, Display 2 became None
                if (displayEvents[i] == null || displayEvents[i].nextUpdateTicks <= DateTime.Now.Ticks)
                {
                    if (displayEvents[i] == null)
                    {
                        displayEvents[i] = new DisplayEvent();
                    }
                    else if (forceImageUpdate)
                    {
                        displayEvents[i].lastImagePath = null;
                    }

                    string themeId = JsonConfig.settings.activeThemes[0] ?? JsonConfig.settings.activeThemes[i + 1];
                    displayEvents[i].currentTheme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);
                    displayEvents[i].displayIndex = (JsonConfig.settings.activeThemes[0] == null) ? i : -1;

                    if (displayEvents[i].currentTheme != null)
                    {
                        SolarScheduler.CalcNextUpdateTime(data, displayEvents[i]);
                        SetWallpaper(displayEvents[i]);

                        if (displayEvents[i].nextUpdateTicks < nextDisplayUpdateTicks)
                        {
                            nextDisplayUpdateTicks = displayEvents[i].nextUpdateTicks;
                        }
                    }
                }
            }

            ScriptManager.RunScripts(new ScriptArgs
            {
                daySegment2 = displayEvents[0].daySegment2,
                daySegment4 = displayEvents[0].daySegment4,
                imagePaths = displayEvents.Select(e => e.lastImagePath).ToArray()
            });

            if (data.polarPeriod != PolarPeriod.None)
            {
                nextUpdateTime = DateTime.Today.AddDays(1);
            }
            else if (data.sunriseTime <= DateTime.Now && DateTime.Now < data.sunsetTime)
            {
                nextUpdateTime = data.sunsetTime;
            }
            else if (DateTime.Now < data.solarTimes[0])
            {
                nextUpdateTime = data.sunriseTime;
            }
            else
            {
                SolarData tomorrowsData = SunriseSunsetService.GetSolarData(DateTime.Today.AddDays(1));
                nextUpdateTime = tomorrowsData.sunriseTime;
            }

            if (nextDisplayUpdateTicks > 0 && nextDisplayUpdateTicks < nextUpdateTime.Value.Ticks)
            {
                nextUpdateTime = new DateTime(nextDisplayUpdateTicks);
            }

            StartTimer(nextUpdateTime.Value);
        }

        public void ToggleDarkMode()
        {
            bool isEnabled = JsonConfig.settings.darkMode ^ true;
            JsonConfig.settings.darkMode = isEnabled;
            MainMenu.darkModeItem.Checked = isEnabled;

            RunScheduler();
        }

        private void RefreshDisplayList(bool sendEvent)
        {
            if (JsonConfig.settings.activeThemes == null || JsonConfig.settings.activeThemes[0] != null)
            {
                return;
            }

            int numDisplaysBefore = displayEvents.Count;
            int numDisplaysAfter = Screen.AllScreens.Length;

            if (numDisplaysAfter > numDisplaysBefore)
            {
                for (int i = 0; i < (numDisplaysAfter - numDisplaysBefore); i++)
                {
                    displayEvents.Add(null);
                }
                if (sendEvent)
                {
                    HandleTimerEvent(false);
                }
            }
            else if (numDisplaysAfter < numDisplaysBefore)
            {
                displayEvents.RemoveRange(numDisplaysAfter, numDisplaysBefore - numDisplaysAfter);
            }
        }

        private void SetWallpaper(DisplayEvent e)
        {
            string imageFilename = e.currentTheme.imageFilename.Replace("*", e.imageId.ToString());
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "themes", e.currentTheme.themeId,
                imageFilename);

            if (imagePath == e.lastImagePath)
            {
                return;
            }
            else
            {
                UwpDesktop.GetHelper().SetWallpaper(imagePath, e.displayIndex);
            }

            e.lastImagePath = imagePath;
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

        public void HandleTimerEvent(bool updateLocation)
        {
            if (JsonConfig.settings.fullScreenPause && fullScreenChecker.runningFullScreen)
            {
                fullScreenChecker.timerEventPending = true;
                return;
            }

            if (updateLocation && JsonConfig.settings.locationMode == 1)
            {
                Task.Run(UwpLocation.UpdateGeoposition);
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

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            RefreshDisplayList(true);
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
