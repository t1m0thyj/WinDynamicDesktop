﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public class DisplayEvent
    {
        public const int LockScreenIndex = int.MaxValue;

        public ThemeConfig currentTheme;
        public int daySegment2;
        public int? daySegment4;
        public int displayIndex;
        public int imageId;
        public string lastImagePath;
        public DateTime nextUpdateTime;
    }

    class EventScheduler
    {
        private System.Timers.Timer backgroundTimer = new System.Timers.Timer();
        private System.Timers.Timer schedulerTimer = new System.Timers.Timer();
        private const long timerError = (long)(TimeSpan.TicksPerMillisecond * 15.6);

        public FullScreenApi fullScreenChecker;
        private List<DisplayEvent> displayEvents;
        private DateTime? nextUpdateTime;

        public EventScheduler()
        {
            fullScreenChecker = new FullScreenApi(() => HandleTimerEvent(true));

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

        public bool Run(bool forceImageUpdate = false, DisplayEvent overrideEvent = null)
        {
            if (!LaunchSequence.IsLocationReady() || !LaunchSequence.IsThemeReady())
            {
                return false;
            }
            else if (displayEvents == null || forceImageUpdate)
            {
                displayEvents = new List<DisplayEvent> { null, null };
            }

            schedulerTimer.Stop();
            forceImageUpdate = UpdateDisplayList() || forceImageUpdate;
            SolarData data = SunriseSunsetService.GetSolarData(DateTime.Today);
            LoggingHandler.LogMessage("Calculated solar data: {0}", data);
            long nextDisplayUpdateTicks = ThemeShuffler.MaybeShuffleWallpaper(data)?.Ticks ?? long.MaxValue;

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

                if (i < displayEvents.Count - 1)
                {
                    string themeId = JsonConfig.settings.activeThemes[0];
                    if (themeId == null && JsonConfig.settings.activeThemes.Length > 1 &&
                        i + 1 < JsonConfig.settings.activeThemes.Length)
                    {
                        themeId = JsonConfig.settings.activeThemes[i + 1];
                    }
                    displayEvents[i].currentTheme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);
                    displayEvents[i].displayIndex = JsonConfig.settings.activeThemes[0] == null ? i : -1;
                }
                else
                {
                    string themeId = JsonConfig.settings.lockScreenDisplayIndex != -1 ?
                        JsonConfig.settings.activeThemes[JsonConfig.settings.lockScreenDisplayIndex] :
                        JsonConfig.settings.lockScreenTheme;
                    displayEvents[i].currentTheme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);
                    displayEvents[i].displayIndex = DisplayEvent.LockScreenIndex;
                }

                SolarScheduler.CalcNextUpdateTime(data, displayEvents[i]);
                LoggingHandler.LogMessage("Updated display event: {0}", displayEvents[i]);

                bool isEventOverridden = overrideEvent != null &&
                    displayEvents[i].displayIndex == Math.Max(0, overrideEvent.displayIndex);
                if (displayEvents[i].currentTheme != null || isEventOverridden)
                {
                    HandleDisplayEvent(isEventOverridden ? overrideEvent : displayEvents[i]);

                    if (displayEvents[i].nextUpdateTime.Ticks < nextDisplayUpdateTicks)
                    {
                        nextDisplayUpdateTicks = displayEvents[i].nextUpdateTime.Ticks;
                    }
                }
            }

            ScriptManager.RunScripts(new ScriptArgs
            {
                daySegment2 = displayEvents[0].daySegment2,
                daySegment4 = displayEvents[0].daySegment4 ?? -1,
                themeMode = JsonConfig.settings.appearanceMode,
                imagePaths = displayEvents.Select(e =>
                    (e.displayIndex == overrideEvent?.displayIndex ? overrideEvent : e).lastImagePath).ToArray()
            }, forceImageUpdate);

            nextUpdateTime = SolarScheduler.CalcNextUpdateTime(data);
            if (nextDisplayUpdateTicks > 0 && nextDisplayUpdateTicks < nextUpdateTime.Value.Ticks)
            {
                nextUpdateTime = new DateTime(nextDisplayUpdateTicks);
            }

            StartTimer(nextUpdateTime.Value);
            return true;
        }

        public async void RunAndUpdateLocation(bool forceImageUpdate = false, Action locationReady = null)
        {
            bool result = Run(forceImageUpdate);
            locationReady?.Invoke();

            if (result && JsonConfig.settings.locationMode == 1 && await UwpLocation.UpdateGeoposition())
            {
                Run();  // Update wallpaper again if location has changed
            }
        }

        private bool UpdateDisplayList()
        {
            if (JsonConfig.IsNullOrEmpty(JsonConfig.settings.activeThemes) ||
                JsonConfig.settings.activeThemes[0] != null)
            {
                return false;
            }

            int numDisplaysBefore = displayEvents.Count - 1;
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
                    displayEvents.Insert(displayEvents.Count - 1, null);
                }
            }
            else if (numDisplaysAfter < numDisplaysBefore)
            {
                displayEvents.RemoveRange(numDisplaysAfter, numDisplaysBefore - numDisplaysAfter);
            }

            return numDisplaysAfter > numDisplaysBefore;
        }

        private void HandleDisplayEvent(DisplayEvent e)
        {
            string imagePath = e.lastImagePath;
            if (e.currentTheme != null)
            {
                string imageFilename = e.currentTheme.imageFilename.Replace("*", e.imageId.ToString());
                imagePath = Path.Combine(Path.GetFullPath("themes"), e.currentTheme.themeId, imageFilename);
                if (imagePath == e.lastImagePath)
                {
                    return;
                }
            }

            LoggingHandler.LogMessage("Setting wallpaper {0} to {1}", e.displayIndex, imagePath);
            try
            {
                if (e.displayIndex != DisplayEvent.LockScreenIndex)
                {
                    UwpDesktop.GetHelper().SetWallpaper(imagePath, e.displayIndex);
                }
                else
                {
                    UwpDesktop.GetHelper().SetLockScreen(imagePath);
                }
                e.lastImagePath = imagePath;
            }
            catch (Exception exc)
            {
                LoggingHandler.LogMessage("Error setting wallpaper: {0}", exc.ToString());
                LoggingHandler.LogError(UwpDesktop.GetHelper().GetLocalFolder(), exc);
            }
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

        private void HandleTimerEvent(bool updateLocation)
        {
            if (JsonConfig.settings.fullScreenPause && fullScreenChecker.runningFullScreen)
            {
                fullScreenChecker.timerEventPending = true;
                return;
            }

            if (updateLocation)
            {
                RunAndUpdateLocation();
            }
            else
            {
                Run();
            }

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
                LoggingHandler.LogMessage("Scheduler event triggered by display change");
                HandleTimerEvent(false);
            }
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume && (UpdateDisplayList() ||
                (nextUpdateTime.HasValue && DateTime.Now >= nextUpdateTime.Value)))
            {
                LoggingHandler.LogMessage("Scheduler event triggered by resume from sleep");
                HandleTimerEvent(false);
            }
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionUnlock && (UpdateDisplayList() ||
                (nextUpdateTime.HasValue && DateTime.Now >= nextUpdateTime.Value)))
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
