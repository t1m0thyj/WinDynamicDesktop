using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Innovative.SolarCalculator;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public class WeatherData
    {
        public DateTime SunriseTime { get; set; }
        public DateTime SunsetTime { get; set; }
    }

    class SunriseSunsetService
    {
        public static WeatherData GetWeatherData(string lat, string lon, string dateStr)
        {
            DateTime date = DateTime.Parse(dateStr);
            double latitude = Double.Parse(lat);
            double longitude = Double.Parse(lon);

            SolarTimes solarTimes = new SolarTimes(date, latitude, longitude);

            WeatherData data = new WeatherData();
            data.SunriseTime = solarTimes.Sunrise;
            data.SunsetTime = solarTimes.Sunset;

            return data;
        }
    }
}
