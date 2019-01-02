using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using SunCalcNet.Model;

namespace WinDynamicDesktop
{
    public class SolarData
    {
        public DateTime sunriseTime { get; set; }
        public DateTime sunsetTime { get; set; }
        public DateTime[] solarTimes { get; set; }
    }

    class SunriseSunsetService
    {
        private static Dictionary<string,DateTime> GetSunPhases(DateTime date, string lat,
            string lng)
        {
            double latitude = double.Parse(lat, CultureInfo.InvariantCulture);
            double longitude = double.Parse(lng, CultureInfo.InvariantCulture);
            var sunPhases = new Dictionary<string, DateTime>();

            foreach (SunPhase phase in SunCalcNet.SunCalc.GetSunPhases(date, latitude, longitude))
            {
                sunPhases.Add(phase.Name.Value, phase.PhaseTime);
            }

            return sunPhases;
        }

        public static SolarData GetSolarData(DateTime date)
        {
            // TODO Why must I add 12 hrs for this to work?
            var sunPhases = GetSunPhases(date.AddHours(12).ToUniversalTime(),
                JsonConfig.settings.latitude, JsonConfig.settings.longitude);

            SolarData data = new SolarData
            {
                sunriseTime = sunPhases[SunPhaseName.Sunrise.Value].ToLocalTime(),
                sunsetTime = sunPhases[SunPhaseName.Sunset.Value].ToLocalTime(),
                solarTimes = new DateTime[4]
            };

            data.solarTimes[0] = sunPhases[SunPhaseName.NauticalDawn.Value].ToLocalTime();
            data.solarTimes[1] = sunPhases[SunPhaseName.GoldenHourEnd.Value].ToLocalTime();
            data.solarTimes[2] = sunPhases[SunPhaseName.GoldenHour.Value].ToLocalTime();
            data.solarTimes[3] = sunPhases[SunPhaseName.NauticalDusk.Value].ToLocalTime();
            //System.Windows.Forms.MessageBox.Show(date.ToUniversalTime().ToString() + ",,," + data.solarTimes[0].ToLocalTime().ToString() + "," + data.solarTimes[1].ToLocalTime().ToString() + "," + data.solarTimes[2].ToLocalTime().ToString() + "," + data.solarTimes[3].ToLocalTime().ToString() + ",");

            return data;
        }
    }
}
