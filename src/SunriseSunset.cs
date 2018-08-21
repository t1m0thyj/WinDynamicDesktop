using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Innovative.SolarCalculator;

namespace WinDynamicDesktop
{
    public class WeatherData
    {
        public DateTime SunriseTime { get; set; }
        public DateTime SunsetTime { get; set; }
    }

    class SunriseSunsetService
    {
        private static CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US");

        public static WeatherData GetWeatherData(string lat, string lon, string dateStr)
        {
            DateTime date = DateTime.Parse(dateStr, cultureInfo);
            double latitude = double.Parse(lat, cultureInfo);
            double longitude = double.Parse(lon, cultureInfo);

            SolarTimes solarTimes = new SolarTimes(date, latitude, longitude);

            WeatherData data = new WeatherData();
            data.SunriseTime = solarTimes.Sunrise;
            data.SunsetTime = solarTimes.Sunset;

            return data;
        }
    }
}
