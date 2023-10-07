// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace WinDynamicDesktop
{
    public enum DaySegment { Sunrise, Day, Sunset, Night, AlwaysDay, AlwaysNight }

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
                if (theme.sunriseImageList != null && !theme.sunriseImageList.SequenceEqual(theme.dayImageList))
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
                        if (DateTime.now < (data.solarTimes[2] - TimeSpan.FromHours(12))
                        {
                            long x = data.solarTimes[2] + TimeSpan.FromTicks((TimeSpan.FromDays(1).Ticks * i / theme.dayImageList.Length) - TimeSpan.FromHours(12).Ticks);
                            DateTime x_min = data.solarTimes[2] - TimeSpan.FromHours(36);
                            DateTime x_max = data.solarTimes[2] - TimeSpan.FromHours(12);
                            long xx = (((x.Ticks - x_min.Ticks) % (x_max.Ticks - x_min.Ticks)) + (x_max.Ticks - x_min.Ticks)) % (x_max.Ticks - x_min.Ticks) + x_min.Ticks;
                            times.Add(new DateTime(xx));
                        }
                        else if (DateTime.now > (data.solarTimes[2] + TimeSpan.FromHours(12))
                        {
                            long x = data.solarTimes[2] + TimeSpan.FromTicks((TimeSpan.FromDays(1).Ticks * i / theme.dayImageList.Length) - TimeSpan.FromHours(12).Ticks);
                            DateTime x_min = data.solarTimes[2] + TimeSpan.FromHours(12);
                            DateTime x_max = data.solarTimes[2] + TimeSpan.FromHours(36);
                            long xx = (((x.Ticks - x_min.Ticks) % (x_max.Ticks - x_min.Ticks)) + (x_max.Ticks - x_min.Ticks)) % (x_max.Ticks - x_min.Ticks) + x_min.Ticks;
                            times.Add(new DateTime(xx));
                        }
                        else 
                        { 
                            long x = data.solarTimes[2] + TimeSpan.FromTicks((TimeSpan.FromDays(1).Ticks * i / theme.dayImageList.Length) - TimeSpan.FromHours(12).Ticks);
                            DateTime x_min = data.solarTimes[2] - TimeSpan.FromHours(12);
                            DateTime x_max = data.solarTimes[2] + TimeSpan.FromHours(12);
                            long xx = (((x.Ticks - x_min.Ticks) % (x_max.Ticks - x_min.Ticks)) + (x_max.Ticks - x_min.Ticks)) % (x_max.Ticks - x_min.Ticks) + x_min.Ticks;
                            times.Add(new DateTime(xx));
                        }
                            
                    }
                    else
                    {
                        times.Add(DateTime.MinValue);
                    }
                }
                if (theme.sunsetImageList != null && !theme.sunsetImageList.SequenceEqual(theme.dayImageList))
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
                        if (DateTime.now > data.solarTimes[2])
                        {
                            long x = data.solarTimes[2] + TimeSpan.FromTicks(TimeSpan.FromDays(1).Ticks * i / theme.dayImageList.Length);
                            DateTime x_min = data.solarTimes[2];
                            DateTime x_max = data.solarTimes[2] + TimeSpan.FromDays(1);
                            long xx = (((x - x_min) % (x_max - x_min)) + (x_max - x_min)) % (x_max - x_min) + x_min;
                            times.Add(new DateTime(xx));
                        }
                        else
                        {
                            long x = data.solarTimes[2] + TimeSpan.FromTicks(TimeSpan.FromDays(1).Ticks * i / theme.dayImageList.Length);
                            DateTime x_min = data.solarTimes[2] - TimeSpan.FromDays(1);
                            DateTime x_max = data.solarTimes[2];
                            long xx = (((x - x_min) % (x_max - x_min)) + (x_max - x_min)) % (x_max - x_min) + x_min;
                            times.Add(new DateTime(xx));
                        }
                    else
                    {
                        times.Add(DateTime.MinValue);
                    }
                }
            }
            else
            {
                bool hasSunriseImages = !JsonConfig.IsNullOrEmpty(theme.sunriseImageList);
                bool hasSunsetImages = !JsonConfig.IsNullOrEmpty(theme.sunsetImageList);
                if (hasSunriseImages)
                {
                    for (int i = 0; i < theme.sunriseImageList.Length; i++)
                    {
                        times.Add(data.solarTimes[0] + TimeSpan.FromTicks((data.solarTimes[1].Ticks - data.solarTimes[0].Ticks) * i / theme.sunriseImageList.Length));
                    }
                }
                if (!hasSunriseImages || !theme.dayImageList.SequenceEqual(theme.sunriseImageList))
                {
                    for (int i = 0; i < theme.dayImageList.Length; i++)
                    {
                        DateTime dayStartTime = hasSunriseImages ? data.solarTimes[1] : data.sunriseTime;
                        DateTime dayEndTime = hasSunsetImages ? data.solarTimes[3] : data.sunsetTime;
                        times.Add(dayStartTime + TimeSpan.FromTicks((dayEndTime.Ticks - dayStartTime.Ticks) * i / theme.dayImageList.Length));
                    }
                }
                if (hasSunsetImages && !theme.sunsetImageList.SequenceEqual(theme.dayImageList))
                {
                    for (int i = 0; i < theme.sunsetImageList.Length; i++)
                    {
                        times.Add(data.solarTimes[3] + TimeSpan.FromTicks((data.solarTimes[4].Ticks - data.solarTimes[3].Ticks) * i / theme.sunsetImageList.Length));
                    }
                }
                for (int i = 0; i < theme.nightImageList.Length; i++)
                {
                    DateTime nightStartTime = hasSunsetImages ? data.solarTimes[4] : data.sunsetTime;
                    DateTime nightEndTime = hasSunriseImages ? nextData.solarTimes[0] : nextData.sunriseTime;
                    times.Add(nightStartTime + TimeSpan.FromTicks((nightEndTime.Ticks - nightStartTime.Ticks) * i / theme.nightImageList.Length));
                }
            }

            return times;
        }

        public static DaySegmentData GetDaySegmentData(SolarData data, DateTime time)
        {
            int daySegment2 = (data.sunriseTime <= DateTime.Now && DateTime.Now < data.sunsetTime) ? 0 : 1;

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
            else if (data.solarTimes[1] <= time && time < data.solarTimes[3])
            {
                return new DaySegmentData(DaySegment.Day, daySegment2, 1);
            }
            else if (data.solarTimes[3] <= time && time < data.solarTimes[4])
            {
                return new DaySegmentData(DaySegment.Sunset, daySegment2, 2);
            }
            else
            {
                return new DaySegmentData(DaySegment.Night, daySegment2, 3);
            }
        }

        public static void CalcNextUpdateTime(SolarData data, DisplayEvent e)
        {
            int[] imageList;
            DateTime segmentStart;
            DateTime segmentEnd;
            DateTime dateNow = DateTime.Now;
            DaySegmentData segmentData = GetDaySegmentData(data, dateNow);
            e.daySegment2 = segmentData.segment2;

            bool preferSegment2 = JsonConfig.settings.darkMode;
            if (!preferSegment2 && e.currentTheme != null)
            {
                if ((segmentData.segmentType == DaySegment.Sunrise || segmentData.segmentType == DaySegment.Night) &&
                    JsonConfig.IsNullOrEmpty(e.currentTheme.sunriseImageList))
                {
                    preferSegment2 = true;
                }
                else if ((segmentData.segmentType == DaySegment.Sunset || segmentData.segmentType == DaySegment.Day) &&
                    JsonConfig.IsNullOrEmpty(e.currentTheme.sunsetImageList))
                {
                    preferSegment2 = true;
                }
            }

            if (data.polarPeriod != PolarPeriod.None)
            {
                imageList = e.currentTheme?.dayImageList;
                if (data.polarPeriod == PolarPeriod.PolarNight || JsonConfig.settings.darkMode)
                {
                    imageList = e.currentTheme?.nightImageList;
                }
                segmentStart = dateNow.Date;
                segmentEnd = dateNow.Date.AddDays(1);
            }
            else if (!preferSegment2)
            {
                e.daySegment4 = segmentData.segment4;

                switch (segmentData.segmentType)
                {
                    case DaySegment.Sunrise:
                        imageList = e.currentTheme?.sunriseImageList;
                        segmentStart = data.solarTimes[0];
                        segmentEnd = data.solarTimes[1];
                        break;
                    case DaySegment.Day:
                        imageList = e.currentTheme?.dayImageList;
                        segmentStart = data.solarTimes[1];
                        segmentEnd = data.solarTimes[3];
                        break;
                    case DaySegment.Sunset:
                        imageList = e.currentTheme?.sunsetImageList;
                        segmentStart = data.solarTimes[3];
                        segmentEnd = data.solarTimes[4];
                        break;
                    default:
                        imageList = e.currentTheme?.nightImageList;

                        if (dateNow < data.solarTimes[0])
                        {
                            SolarData yesterdaysData = SunriseSunsetService.GetSolarData(dateNow.Date.AddDays(-1));
                            segmentStart = yesterdaysData.solarTimes[4];
                            segmentEnd = data.solarTimes[0];
                        }
                        else
                        {
                            segmentStart = data.solarTimes[4];
                            SolarData tomorrowsData = SunriseSunsetService.GetSolarData(dateNow.Date.AddDays(1));
                            segmentEnd = tomorrowsData.solarTimes[0];
                        }

                        break;
                }
            }
            else
            {
                imageList = e.currentTheme?.dayImageList;
                if (segmentData.segment2 == 1 || JsonConfig.settings.darkMode)
                {
                    imageList = e.currentTheme?.nightImageList;
                }

                if (segmentData.segment2 == 0)
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

            if (imageList != null)
            {
                TimeSpan imageDuration = new TimeSpan((segmentEnd - segmentStart).Ticks / imageList.Length);
                int imageNumber = (int)((dateNow.Ticks - segmentStart.Ticks) / imageDuration.Ticks);
                e.imageId = imageList[imageNumber];
                e.nextUpdateTime = new DateTime(segmentStart.Ticks + imageDuration.Ticks * (imageNumber + 1));
            }
        }
    }
}
