// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using GeoTimeZone;
using SunCalcNet.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TimeZoneConverter;

namespace WinDynamicDesktop
{
    public enum PolarPeriod { None, PolarDay, PolarNight };

    public class SolarData
    {
        public PolarPeriod polarPeriod = PolarPeriod.None;
        public DateTime sunriseTime { get; set; }
        public DateTime sunsetTime { get; set; }
        public DateTime[] solarTimes { get; set; }
    }

    class SunriseSunsetService
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;

        private static SolarData GetUserProvidedSolarData(DateTime date)
        {
            SolarData data = new SolarData();
            data.sunriseTime = date.Date + ConfigMigrator.SafeParse(JsonConfig.settings.sunriseTime).TimeOfDay;
            data.sunsetTime = date.Date + ConfigMigrator.SafeParse(JsonConfig.settings.sunsetTime).TimeOfDay;

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
            DateTime localDate = TimeZoneInfo.ConvertTime(date.Add(DateTime.Now.TimeOfDay), tzInfo);
            // Set time to noon because of https://github.com/mourner/suncalc/issues/107
            DateTime utcDate = new DateTimeOffset(
                localDate.Year, localDate.Month, localDate.Day, 12, 0, 0, tzInfo.GetUtcOffset(localDate)).UtcDateTime;
            return SunCalcNet.SunCalc.GetSunPhases(utcDate, latitude, longitude).ToList();
        }

        private static DateTime GetSolarTime(List<SunPhase> sunPhases, SunPhaseName desiredPhase)
        {
            SunPhase sunPhase = sunPhases.FirstOrDefault(sp => sp.Name.Value == desiredPhase.Value);
            return (sunPhase != null) ? sunPhase.PhaseTime.ToLocalTime() : DateTime.MinValue;
        }

        public static SolarData GetSolarData(DateTime date)
        {
            if (JsonConfig.settings.dontUseLocation)
            {
                return GetUserProvidedSolarData(date);
            }

            double latitude = double.Parse(JsonConfig.settings.latitude, CultureInfo.InvariantCulture);
            double longitude = double.Parse(JsonConfig.settings.longitude, CultureInfo.InvariantCulture);
            var sunPhases = GetSunPhases(date, latitude, longitude);
            SolarData data = new SolarData();

            data.sunriseTime = GetSolarTime(sunPhases, SunPhaseName.Sunrise);
            data.sunsetTime = GetSolarTime(sunPhases, SunPhaseName.Sunset);
            data.solarTimes = new DateTime[4]
            {
                GetSolarTime(sunPhases, SunPhaseName.Dawn),
                GetSolarTime(sunPhases, SunPhaseName.GoldenHourEnd),
                GetSolarTime(sunPhases, SunPhaseName.GoldenHour),
                GetSolarTime(sunPhases, SunPhaseName.Dusk)
            };

            // Assume polar day/night if sunrise/sunset time are undefined
            if (data.sunriseTime == DateTime.MinValue || data.sunsetTime == DateTime.MinValue)
            {
                DateTime solarNoon = GetSolarTime(sunPhases, SunPhaseName.SolarNoon);
                double sunAltitude = SunCalcNet.SunCalc.GetSunPosition(solarNoon.ToUniversalTime(), latitude,
                    longitude).Altitude;

                if (sunAltitude > 0)
                {
                    data.polarPeriod = PolarPeriod.PolarDay;
                }
                else
                {
                    data.polarPeriod = PolarPeriod.PolarNight;
                }
            }
            // Skip night segment if dawn/dusk are undefined
            else if (data.solarTimes[0] == DateTime.MinValue && data.solarTimes[3] == DateTime.MinValue)
            {
                data.solarTimes[0] = data.sunriseTime.Date;
                data.solarTimes[3] = data.sunsetTime.Date.AddDays(1).AddTicks(-1);
            }
            // Skip day segment if golden hour (end) are undefined
            else if (data.solarTimes[1] == DateTime.MinValue && data.solarTimes[2] == DateTime.MinValue)
            {
                DateTime midDay = new DateTime((data.solarTimes[0].Ticks + data.solarTimes[3].Ticks) / 2);
                data.solarTimes[1] = midDay;
                data.solarTimes[2] = midDay;
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
                    return string.Format(_("Sunrise: {0}, Sunset: {1}"), solarData.sunriseTime.ToShortTimeString(),
                        solarData.sunsetTime.ToShortTimeString());
            }
        }
    }
}
