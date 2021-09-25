// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace WinDynamicDesktop
{
    public enum DaySegment { Sunrise, Day, Sunset, Night, AlwaysDay, AlwaysNight };

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

        public static DaySegment GetDaySegment(SolarData data, DateTime time)
        {
            if (data.polarPeriod == PolarPeriod.PolarDay)
            {
                return DaySegment.AlwaysDay;
            }
            else if (data.polarPeriod == PolarPeriod.PolarNight)
            {
                return DaySegment.AlwaysNight;
            }
            else if (data.solarTimes[0] <= time && time < data.solarTimes[1])
            {
                return DaySegment.Sunrise;
            }
            else if (data.solarTimes[1] <= time && time < data.solarTimes[2])
            {
                return DaySegment.Day;
            }
            else if (data.solarTimes[2] <= time && time < data.solarTimes[3])
            {
                return DaySegment.Sunset;
            }
            else
            {
                return DaySegment.Night;
            }
        }
    }
}
