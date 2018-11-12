using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Innovative.SolarCalculator;

namespace WinDynamicDesktop
{
    public class SolarData
    {
        public DateTime SunriseTime { get; set; }
        public DateTime SunsetTime { get; set; }
    }

    class SunriseSunsetService
    {
        public static SolarData GetSolarData(string lat, string lon, DateTime date)
        {
            double latitude = double.Parse(lat, CultureInfo.InvariantCulture);
            double longitude = double.Parse(lon, CultureInfo.InvariantCulture);

            SolarTimes solarTimes = new SolarTimes(date, latitude, longitude);

            SolarData data = new SolarData();
            data.SunriseTime = solarTimes.Sunrise;
            data.SunsetTime = solarTimes.Sunset;

            return data;
        }
    }
}
