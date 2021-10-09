// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace WinDynamicDesktop
{
    public enum DaySegment { Sunrise, Day, Sunset, Night, AlwaysDay, AlwaysNight };

    public class DaySegmentData
    {
        public DaySegment segmentType;
        public int segment2;
        public int segment4;

        public DaySegmentData(DaySegment segmentType, int segment2, int segment4)
        {
            this.segmentType = segmentType;
            this.segment2 = segment2;
            this.segment4 = segment4;
        }
    }

    class SolarScheduler
    {
        public static List<DateTime> GetAllImageTimes(ThemeConfig theme)
        {
            List<DateTime> times = new List<DateTime>();
            SolarData data = SunriseSunsetService.GetSolarData(DateTime.Today);
            SolarData nextData = SunriseSunsetService.GetSolarData(DateTime.Today.AddDays(1));

            if (data.polarPeriod != PolarPeriod.None)
            {
                if (theme.sunriseImageList != null)
                {
                    for (int i = 0; i < theme.sunriseImageList.Length; i++)
                    {
                        times.Add(DateTime.MinValue);
                    }
                }
                for (int i = 0; i < theme.dayImageList.Length; i++)
                {
                    if (data.polarPeriod == PolarPeriod.PolarDay)
                    {
                        times.Add(DateTime.Today + TimeSpan.FromTicks(TimeSpan.FromDays(1).Ticks * i / theme.dayImageList.Length));
                    }
                    else
                    {
                        times.Add(DateTime.MinValue);
                    }
                }
                if (theme.sunsetImageList != null)
                {
                    for (int i = 0; i < theme.sunsetImageList.Length; i++)
                    {
                        times.Add(DateTime.MinValue);
                    }
                }
                for (int i = 0; i < theme.nightImageList.Length; i++)
                {
                    if (data.polarPeriod == PolarPeriod.PolarNight)
                    {
                        times.Add(DateTime.Today + TimeSpan.FromTicks(TimeSpan.FromDays(1).Ticks * i / theme.nightImageList.Length));
                    }
                    else
                    {
                        times.Add(DateTime.MinValue);
                    }
                }
            }
            else if (ThemeManager.IsTheme4Segment(theme))
            {
                for (int i = 0; i < theme.sunriseImageList.Length; i++)
                {
                    times.Add(data.solarTimes[0] + TimeSpan.FromTicks((data.solarTimes[1].Ticks - data.solarTimes[0].Ticks) * i / theme.sunriseImageList.Length));
                }
                for (int i = 0; i < theme.dayImageList.Length; i++)
                {
                    times.Add(data.solarTimes[1] + TimeSpan.FromTicks((data.solarTimes[2].Ticks - data.solarTimes[1].Ticks) * i / theme.dayImageList.Length));
                }
                for (int i = 0; i < theme.sunsetImageList.Length; i++)
                {
                    times.Add(data.solarTimes[2] + TimeSpan.FromTicks((data.solarTimes[3].Ticks - data.solarTimes[2].Ticks) * i / theme.sunsetImageList.Length));
                }
                for (int i = 0; i < theme.nightImageList.Length; i++)
                {
                    times.Add(data.solarTimes[3] + TimeSpan.FromTicks((nextData.solarTimes[0].Ticks - data.solarTimes[3].Ticks) * i / theme.nightImageList.Length));
                }
            }
            else
            {
                for (int i = 0; i < theme.dayImageList.Length; i++)
                {
                    times.Add(data.sunriseTime + TimeSpan.FromTicks((data.sunsetTime.Ticks - data.sunriseTime.Ticks) * i / theme.dayImageList.Length));
                }
                for (int i = 0; i < theme.nightImageList.Length; i++)
                {
                    times.Add(data.sunsetTime + TimeSpan.FromTicks((nextData.sunriseTime.Ticks - data.sunsetTime.Ticks) * i / theme.nightImageList.Length));
                }
            }

            return times;
        }

        public static DaySegmentData GetDaySegmentData(SolarData data, DateTime time)
        {
            int daySegment2 = (data.sunriseTime <= DateTime.Now && DateTime.Now < data.sunsetTime) ? 1 : 0;

            if (data.polarPeriod == PolarPeriod.PolarDay)
            {
                return new DaySegmentData(DaySegment.AlwaysDay, 0, 1);
            }
            else if (data.polarPeriod == PolarPeriod.PolarNight)
            {
                return new DaySegmentData(DaySegment.AlwaysNight, 1, 3);
            }
            else if (data.solarTimes[0] <= time && time < data.solarTimes[1])
            {
                return new DaySegmentData(DaySegment.Sunrise, daySegment2, 0);
            }
            else if (data.solarTimes[1] <= time && time < data.solarTimes[2])
            {
                return new DaySegmentData(DaySegment.Day, daySegment2, 1);
            }
            else if (data.solarTimes[2] <= time && time < data.solarTimes[3])
            {
                return new DaySegmentData(DaySegment.Sunset, daySegment2, 2);
            }
            else
            {
                return new DaySegmentData(DaySegment.Night, daySegment2, 3);
            }
        }

        public static DisplayEvent GetNextUpdateData(SolarData data, ThemeConfig theme, DateTime dateNow)
        {
            int[] imageList;
            DateTime segmentStart;
            DateTime segmentEnd;
            DaySegmentData segmentData = GetDaySegmentData(data, dateNow);
            DisplayEvent imageData = new DisplayEvent() { currentTheme = theme, daySegment2 = segmentData.segment2 };

            if (ThemeManager.IsTheme4Segment(theme) && !JsonConfig.settings.darkMode)
            {
                imageData.daySegment4 = segmentData.segment4;

                switch (segmentData.segmentType)
                {
                    case DaySegment.AlwaysDay:
                        imageList = theme.dayImageList;
                        segmentStart = dateNow.Date;
                        segmentEnd = dateNow.Date.AddDays(1);
                        break;
                    case DaySegment.AlwaysNight:
                        imageList = theme.nightImageList;
                        segmentStart = dateNow.Date;
                        segmentEnd = dateNow.Date.AddDays(1);
                        break;
                    case DaySegment.Sunrise:
                        imageList = theme.sunriseImageList;
                        segmentStart = data.solarTimes[0];
                        segmentEnd = data.solarTimes[1];
                        break;
                    case DaySegment.Day:
                        imageList = theme.dayImageList;
                        segmentStart = data.solarTimes[1];
                        segmentEnd = data.solarTimes[2];
                        break;
                    case DaySegment.Sunset:
                        imageList = theme.sunsetImageList;
                        segmentStart = data.solarTimes[2];
                        segmentEnd = data.solarTimes[3];
                        break;
                    default:
                        imageList = theme.nightImageList;

                        if (dateNow < data.solarTimes[0])
                        {
                            SolarData yesterdaysData = SunriseSunsetService.GetSolarData(dateNow.Date.AddDays(-1));
                            segmentStart = yesterdaysData.solarTimes[3];
                            segmentEnd = data.solarTimes[0];
                        }
                        else
                        {
                            segmentStart = data.solarTimes[3];
                            SolarData tomorrowsData = SunriseSunsetService.GetSolarData(dateNow.Date.AddDays(1));
                            segmentEnd = tomorrowsData.solarTimes[0];
                        }

                        break;
                }
            }
            else
            {
                imageList = theme.dayImageList;

                if (segmentData.segment2 == 1 || JsonConfig.settings.darkMode)
                {
                    imageList = theme.nightImageList;
                }

                if (data.polarPeriod != PolarPeriod.None)
                {
                    segmentStart = dateNow.Date;
                    segmentEnd = dateNow.Date.AddDays(1);
                }
                else if (segmentData.segment2 == 0)
                {
                    segmentStart = data.sunriseTime;
                    segmentEnd = data.sunsetTime;
                }
                else if (dateNow < data.sunriseTime)
                {
                    SolarData yesterdaysData = SunriseSunsetService.GetSolarData(dateNow.Date.AddDays(-1));
                    segmentStart = yesterdaysData.sunsetTime;
                    segmentEnd = data.sunriseTime;
                }
                else
                {
                    segmentStart = data.sunsetTime;
                    SolarData tomorrowsData = SunriseSunsetService.GetSolarData(dateNow.Date.AddDays(1));
                    segmentEnd = tomorrowsData.sunriseTime;
                }
            }

            TimeSpan segmentLength = segmentEnd - segmentStart;
            TimeSpan timerLength = new TimeSpan(segmentLength.Ticks / imageList.Length);

            int imageNumber = (int)((dateNow.Ticks - segmentStart.Ticks) / timerLength.Ticks);
            imageData.imageId = imageList[imageNumber];
            imageData.nextUpdateTicks = segmentStart.Ticks + timerLength.Ticks * (imageNumber + 1);

            return imageData;
        }
    }
}
