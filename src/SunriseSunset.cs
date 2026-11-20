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

        // Sun altitudes in degrees (sunrise/sunset accounts for refraction and solar radius), converted to match
        // GetSunPosition which applies atmospheric refraction unlike the geometric angles used by GetSunPhases
        private static readonly double HorizonAltitude = ToApparentAltitude(-0.833);
        private static readonly double CivilTwilightAltitude = ToApparentAltitude(-6.0);
        private static readonly double GoldenHourAltitude = ToApparentAltitude(6.0);

        private static double ToApparentAltitude(double degrees)
        {
            // Same refraction formula as SunCalc (Bennett 1982), clamped at the horizon
            double h = Math.Max(degrees, 0) * Math.PI / 180;
            double refraction = 0.0002967 / Math.Tan(h + 0.00312536 / (h + 0.08901179));
            return degrees + refraction * 180 / Math.PI;
        }

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
            // The next nadir is derived from tomorrow's solar noon so consecutive days join without gaps.
            data.solarNoon = GetSolarTime(GetSunPhases(date, latitude, longitude), SunPhaseName.SolarNoon);
            DateTime noonUtc = data.solarNoon.ToUniversalTime();
            DateTime prevNadirUtc = noonUtc.AddHours(-12);
            DateTime nextNadirUtc = GetSolarTime(GetSunPhases(date.AddDays(1), latitude, longitude),
                SunPhaseName.SolarNoon).ToUniversalTime().AddHours(-12);

            double GetAltitude(DateTime utc) => GetSunAltitude(utc, latitude, longitude);
            DateTime Rising(double threshold) =>
                FindSunCrossing(prevNadirUtc, noonUtc, threshold, GetAltitude).ToLocalTime();
            DateTime Setting(double threshold) =>
                FindSunCrossing(nextNadirUtc, noonUtc, threshold, GetAltitude).ToLocalTime();

            data.sunriseTime = Rising(HorizonAltitude);
            data.sunsetTime = Setting(HorizonAltitude);
            data.solarTimes = new DateTime[4]
            {
                Rising(CivilTwilightAltitude),
                Rising(GoldenHourAltitude),
                Setting(GoldenHourAltitude),
                Setting(CivilTwilightAltitude)
            };

            double noonAltitude = GetAltitude(noonUtc);
            double prevNadirAltitude = GetAltitude(prevNadirUtc);
            double nextNadirAltitude = GetAltitude(nextNadirUtc);
            data.hasSunrise = prevNadirAltitude < HorizonAltitude && noonAltitude >= HorizonAltitude;
            data.hasSunset = nextNadirAltitude < HorizonAltitude && noonAltitude >= HorizonAltitude;

            if (noonAltitude < HorizonAltitude)
            {
                data.polarPeriod = PolarPeriod.PolarNight;
            }
            else if (prevNadirAltitude >= HorizonAltitude && nextNadirAltitude >= HorizonAltitude)
            {
                data.polarPeriod = PolarPeriod.PolarDay;
            }
            else if (noonAltitude < GoldenHourAltitude)
            {
                data.polarPeriod = PolarPeriod.CivilPolarNight;
            }
            else if (prevNadirAltitude >= CivilTwilightAltitude && nextNadirAltitude >= CivilTwilightAltitude)
            {
                data.polarPeriod = PolarPeriod.CivilPolarDay;
            }

            return data;
        }

        private static double GetSunAltitude(DateTime utc, double latitude, double longitude)
        {
            return SunCalc.GetSunPosition(utc, latitude, longitude).Altitude * 180 / Math.PI;
        }

        // Finds when the sun crosses the threshold altitude between nadir and noon, where altitude is monotonic.
        // If it never drops below the threshold, the event is clamped to the nadir (segment extends all night).
        // If it never climbs above the threshold, the event is clamped to noon (segment collapses to nothing).
        private static DateTime FindSunCrossing(DateTime nadir, DateTime noon, double threshold,
            Func<DateTime, double> getAltitude)
        {
            if (getAltitude(nadir) >= threshold)
            {
                return nadir;
            }
            else if (getAltitude(noon) < threshold)
            {
                return noon;
            }

            DateTime below = nadir, above = noon;
            while (Math.Abs((above - below).Ticks) > TimeSpan.TicksPerSecond)
            {
                DateTime middle = below.AddTicks((above - below).Ticks / 2);
                if (getAltitude(middle) >= threshold)
                {
                    above = middle;
                }
                else
                {
                    below = middle;
                }
            }
            return below.AddTicks((above - below).Ticks / 2);
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
