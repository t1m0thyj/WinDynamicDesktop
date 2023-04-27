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
        public DateTime nextUpdateTime;
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

            backgroundTimer.AutoReset = true;
            backgroundTimer.Interval = 60e3;
            backgroundTimer.Elapsed += OnBackgroundTimerElapsed;
            backgroundTimer.Start();

            schedulerTimer.Elapsed += OnSchedulerTimerElapsed;
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.SessionSwitch += OnSessionSwitch;
            SystemEvents.TimeChanged += OnTimeChanged;
        }

        public void RunScheduler(bool forceImageUpdate = false, string staticImagePath = null)
        {
            if (!LaunchSequence.IsLocationReady() || !LaunchSequence.IsThemeReady())
            {
                return;
            }
            else if (displayEvents == null || forceImageUpdate)
            {
                displayEvents = new List<DisplayEvent> { null };
            }

            schedulerTimer.Stop();
            forceImageUpdate = UpdateDisplayList() || forceImageUpdate;
            SolarData data = SunriseSunsetService.GetSolarData(DateTime.Today);
            LoggingHandler.LogMessage("Calculated solar data: {0}", data);

            ThemeShuffler.MaybeShuffleWallpaper();
            long nextDisplayUpdateTicks = long.MaxValue;

            for (int i = 0; i < displayEvents.Count; i++)
            {
                if (displayEvents[i] == null)
                {
                    displayEvents[i] = new DisplayEvent();
                }
                else if (forceImageUpdate)
                {
                    displayEvents[i].lastImagePath = null;
                }

                string themeId = JsonConfig.settings.activeThemes[0];
                if (themeId == null && JsonConfig.settings.activeThemes.Length > 1)
                {
                    themeId = JsonConfig.settings.activeThemes[i + 1];
                }
                displayEvents[i].currentTheme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);
                displayEvents[i].displayIndex = (JsonConfig.settings.activeThemes[0] == null) ? i : -1;
                SolarScheduler.CalcNextUpdateTime(data, displayEvents[i]);
                LoggingHandler.LogMessage("Updated display event: {0}", displayEvents[i]);

                if (displayEvents[i].currentTheme != null)
                {
                    SetWallpaper(displayEvents[i]);

                    if (displayEvents[i].nextUpdateTime.Ticks < nextDisplayUpdateTicks)
                    {
                        nextDisplayUpdateTicks = displayEvents[i].nextUpdateTime.Ticks;
                    }
                }
            }

            ScriptManager.RunScripts(new ScriptArgs
            {
                daySegment2 = displayEvents[0].daySegment2,
                daySegment4 = displayEvents[0].daySegment4,
                imagePaths = staticImagePath != null ? new string[] { staticImagePath } :
                    displayEvents.Select(e => e.lastImagePath).ToArray()
            }, forceImageUpdate);

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

        private bool UpdateDisplayList()
        {
            if (JsonConfig.settings.activeThemes == null || JsonConfig.settings.activeThemes[0] != null)
            {
                return false;
            }

            int numDisplaysBefore = displayEvents.Count;
            int numDisplaysAfter = Screen.AllScreens.Length;
            if (numDisplaysAfter != numDisplaysBefore)
            {
                LoggingHandler.LogMessage("Number of displays updated from {0} to {1}", numDisplaysBefore,
                    numDisplaysAfter);
            }

            if (numDisplaysAfter > numDisplaysBefore)
            {
                for (int i = 0; i < (numDisplaysAfter - numDisplaysBefore); i++)
                {
                    displayEvents.Add(null);
                }
            }
            else if (numDisplaysAfter < numDisplaysBefore)
            {
                displayEvents.RemoveRange(numDisplaysAfter, numDisplaysBefore - numDisplaysAfter);
            }

            return numDisplaysAfter > numDisplaysBefore;
        }

        private void SetWallpaper(DisplayEvent e)
        {
            string imageFilename = e.currentTheme.imageFilename.Replace("*", e.imageId.ToString());
            string imagePath = Path.Combine(Path.GetFullPath("themes"), e.currentTheme.themeId, imageFilename);
            if (imagePath == e.lastImagePath)
            {
                return;
            }

            LoggingHandler.LogMessage("Setting wallpaper to {0}", imagePath);
            UwpDesktop.GetHelper().SetWallpaper(imagePath, e.displayIndex);
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
            LoggingHandler.LogMessage("Started timer for {0:0.000} sec", interval.TotalSeconds);
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
                LoggingHandler.LogMessage("Scheduler event triggered by timer 2");
                HandleTimerEvent(true);
            }
        }

        private void OnSchedulerTimerElapsed(object sender, EventArgs e)
        {
            LoggingHandler.LogMessage("Scheduler event triggered by timer 1");
            HandleTimerEvent(true);
        }

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            if (UpdateDisplayList())
            {
                HandleTimerEvent(false);
            }
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                LoggingHandler.LogMessage("Scheduler event triggered by resume from sleep");
                HandleTimerEvent(false);
            }
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                LoggingHandler.LogMessage("Scheduler event triggered by user session unlock");
                HandleTimerEvent(false);
            }
        }

        private void OnTimeChanged(object sender, EventArgs e)
        {
            LoggingHandler.LogMessage("Scheduler event triggered by system time change");
            HandleTimerEvent(false);
        }
    }
}
