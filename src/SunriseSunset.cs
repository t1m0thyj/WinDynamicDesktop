// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using GeoTimeZone;
using SunCalcNet;
using SunCalcNet.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TimeZoneConverter;

namespace WinDynamicDesktop
{
    public enum PolarPeriod { None, PolarDay, PolarNight, CivilPolarDay, CivilPolarNight }

    public class SolarData
    {
        public PolarPeriod polarPeriod = PolarPeriod.None;
        public DateTime sunriseTime { get; set; }
        public DateTime sunsetTime { get; set; }
        public DateTime[] solarTimes { get; set; }
        public DateTime solarNoon { get; set; }
        public bool hasSunrise = true;
        public bool hasSunset = true;

        public bool IsPolarPeriodTotal()
        {
            return polarPeriod == PolarPeriod.PolarDay || polarPeriod == PolarPeriod.PolarNight;
        }
    }

    class SunriseSunsetService
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        internal static Func<DateTime> GetDateTimeNow = () => DateTime.Now;

        private static SolarData GetUserProvidedSolarData(DateTime date)
        {
            SolarData data = new SolarData();
            data.sunriseTime = date.Date + DateTime.Parse(JsonConfig.settings.sunriseTime,
                CultureInfo.InvariantCulture).TimeOfDay;
            data.sunsetTime = date.Date + DateTime.Parse(JsonConfig.settings.sunsetTime,
                CultureInfo.InvariantCulture).TimeOfDay;

            int halfSunriseSunsetDuration = JsonConfig.settings.sunriseSunsetDuration * 30;
            data.solarTimes = new DateTime[4]
            {
                data.sunriseTime.AddSeconds(-halfSunriseSunsetDuration),
                data.sunriseTime.AddSeconds(halfSunriseSunsetDuration),
                data.sunsetTime.AddSeconds(-halfSunriseSunsetDuration),
                data.sunsetTime.AddSeconds(halfSunriseSunsetDuration)
            };

            return data;
        }

        private static List<SunPhase> GetSunPhases(DateTime date, double latitude, double longitude)
        {
            string tzName = TimeZoneLookup.GetTimeZone(latitude, longitude).Result;
            TimeZoneInfo tzInfo = TZConvert.GetTimeZoneInfo(tzName);
            DateTime localDate = date.Add(GetDateTimeNow().TimeOfDay);
            if (tzInfo.IsInvalidTime(localDate))
            {
                TimeSpan offsetBefore = tzInfo.GetUtcOffset(localDate.AddDays(-1));
                TimeSpan offsetAfter = tzInfo.GetUtcOffset(localDate.AddDays(1));
                localDate = localDate.Add(offsetAfter - offsetBefore);
            }
            DateTimeOffset tzDate = TimeZoneInfo.ConvertTime(new DateTimeOffset(localDate), tzInfo);
            return SunCalc.GetSunPhases(tzDate, latitude, longitude).ToList();
        }

        private static DateTime GetSolarTime(List<SunPhase> sunPhases, SunPhaseName desiredPhase)
        {
            DateTimeOffset? phaseTime = SunPhaseExtensions.FindTime(sunPhases, desiredPhase);
            return phaseTime != null ? phaseTime.Value.LocalDateTime : DateTime.MinValue;
        }

        public static SolarData GetSolarData(DateTime date)
        {
            if (JsonConfig.settings.locationMode < 0)
            {
                return GetUserProvidedSolarData(date);
            }

            double latitude = JsonConfig.settings.latitude.Value;
            double longitude = JsonConfig.settings.longitude.Value;
            SolarData data = new SolarData();

            // A solar day spans from the nadir before solar noon to the nadir after it, which does not line up
            // with the calendar day at high latitudes (e.g. sunset at 00:30 belongs to the previous evening).
            // SunCalc only returns events within a calendar day, so gather them from yesterday through tomorrow.
            List<SunPhase> sunPhases = GetSunPhases(date, latitude, longitude);
            List<SunPhase> nextSunPhases = GetSunPhases(date.AddDays(1), latitude, longitude);
            List<SunPhase> allSunPhases = [.. GetSunPhases(date.AddDays(-1), latitude, longitude), .. sunPhases,
                .. nextSunPhases];
            data.solarNoon = GetSolarTime(sunPhases, SunPhaseName.SolarNoon);
            DateTime prevNadir = data.solarNoon.AddHours(-12);
            DateTime nextNadir = GetSolarTime(nextSunPhases, SunPhaseName.SolarNoon).AddHours(-12);
            double noonAltitude = SunCalc.GetSunPosition(data.solarNoon.ToUniversalTime(), latitude,
                longitude).Altitude;

            // Events are ordered from lowest to highest sun altitude. If one is missing, the sun stayed on one side
            // of it all morning/evening: above it (clamp to nadir) if a higher event happened or noon is above
            // the horizon with no events at all, otherwise below it (clamp to noon).
            DateTime[] FindEvents(SunPhaseName[] names, DateTime start, DateTime end, DateTime nadir)
            {
                DateTime[] times = names.Select(name => allSunPhases
                    .Where(sp => sp.Name.Value == name.Value)
                    .Select(sp => sp.PhaseTime.LocalDateTime)
                    .FirstOrDefault(t => start <= t && t < end, DateTime.MinValue)).ToArray();
                int highestFound = Array.FindLastIndex(times, t => t != DateTime.MinValue);
                for (int i = 0; i < times.Length; i++)
                {
                    if (times[i] == DateTime.MinValue)
                    {
                        bool sunAbove = highestFound >= 0 ? i < highestFound : noonAltitude > 0;
                        times[i] = sunAbove ? nadir : data.solarNoon;
                    }
                }
                return times;
            }

            DateTime[] rising = FindEvents([SunPhaseName.Dawn, SunPhaseName.Sunrise, SunPhaseName.GoldenHourEnd],
                prevNadir, data.solarNoon, prevNadir);
            DateTime[] setting = FindEvents([SunPhaseName.Dusk, SunPhaseName.Sunset, SunPhaseName.GoldenHour],
                data.solarNoon, nextNadir, nextNadir);
            data.sunriseTime = rising[1];
            data.sunsetTime = setting[1];
            data.solarTimes = [rising[0], rising[2], setting[2], setting[0]];
            data.hasSunrise = prevNadir < data.sunriseTime && data.sunriseTime < data.solarNoon;
            data.hasSunset = data.solarNoon < data.sunsetTime && data.sunsetTime < nextNadir;

            if (!data.hasSunrise && !data.hasSunset)
            {
                data.polarPeriod = noonAltitude > 0 ? PolarPeriod.PolarDay : PolarPeriod.PolarNight;
            }
            else if (data.solarTimes[1] == data.solarNoon && data.solarTimes[2] == data.solarNoon)
            {
                data.polarPeriod = PolarPeriod.CivilPolarNight;
            }
            else if (data.solarTimes[0] == prevNadir && data.solarTimes[3] == nextNadir)
            {
                data.polarPeriod = PolarPeriod.CivilPolarDay;
            }

            return data;
        }

        public static string GetSunriseSunsetString(SolarData solarData)
        {
            switch (solarData.polarPeriod)
            {
                case PolarPeriod.PolarDay:
                    return _("Sunrise/Sunset: Always up");
                case PolarPeriod.PolarNight:
                    return _("Sunrise/Sunset: Always down");
                default:
                    return string.Format(_("Sunrise: {0}, Sunset: {1}"),
                        solarData.hasSunrise ? solarData.sunriseTime.ToShortTimeString() : "—",
                        solarData.hasSunset ? solarData.sunsetTime.ToShortTimeString() : "—");
            }
        }
    }
}
