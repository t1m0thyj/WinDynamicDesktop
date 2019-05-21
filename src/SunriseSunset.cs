// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using SunCalcNet.Model;

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

        private static List<SunPhase> GetSunPhases(DateTime date, double latitude,
            double longitude)
        {
            return SunCalcNet.SunCalc.GetSunPhases(date.AddHours(12).ToUniversalTime(),
                latitude, longitude).ToList();
        }

        private static DateTime GetSolarTime(List<SunPhase> sunPhases, SunPhaseName desiredPhase)
        {
            return sunPhases.Single(sunPhase =>
                sunPhase.Name.Value == desiredPhase.Value).PhaseTime.ToLocalTime();
        }

        public static SolarData GetSolarData(DateTime date)
        {
            double latitude = double.Parse(JsonConfig.settings.latitude,
                CultureInfo.InvariantCulture);
            double longitude = double.Parse(JsonConfig.settings.longitude,
                CultureInfo.InvariantCulture);
            var sunPhases = GetSunPhases(date, latitude, longitude);
            SolarData data = new SolarData();

            try
            {
                data.sunriseTime = GetSolarTime(sunPhases, SunPhaseName.Sunrise);
                data.sunsetTime = GetSolarTime(sunPhases, SunPhaseName.Sunset);
                data.solarTimes = new DateTime[4]
                {
                    GetSolarTime(sunPhases, SunPhaseName.Dawn),
                    GetSolarTime(sunPhases, SunPhaseName.GoldenHourEnd),
                    GetSolarTime(sunPhases, SunPhaseName.GoldenHour),
                    GetSolarTime(sunPhases, SunPhaseName.Dusk)
                };
            }
            catch (InvalidOperationException)  // Handle polar day/night
            {
                DateTime solarNoon = GetSolarTime(sunPhases, SunPhaseName.SolarNoon);
                double sunAltitude = SunCalcNet.SunCalc.GetSunPosition(solarNoon.ToUniversalTime(),
                    latitude, longitude).Altitude;

                if (sunAltitude > 0)
                {
                    data.polarPeriod = PolarPeriod.PolarDay;
                }
                else
                {
                    data.polarPeriod = PolarPeriod.PolarNight;
                }
            }

            return data;
        }

        public static string GetSunriseSunsetString(SolarData solarData)
        {
            switch (solarData.polarPeriod)
            {
                case PolarPeriod.PolarDay:
                    return _("Sunrise/Sunset: Up all day");
                case PolarPeriod.PolarNight:
                    return _("Sunrise/Sunset: Down all day");
                default:
                    return string.Format(_("Sunrise: {0}, Sunset: {1}"), 
                        solarData.sunriseTime.ToShortTimeString(),
                        solarData.sunsetTime.ToShortTimeString());
            }
        }
    }
}
