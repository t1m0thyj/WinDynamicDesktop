using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace WinDynamicDesktop
{
    public class WeatherData
    {
        public DateTime SunriseTime { get; set; }
        public DateTime SunsetTime { get; set; }
    }

    class Results
    {
        public string sunrise { get; set; }
        public string sunset { get; set; }
        public string solar_noon { get; set; }
        public string day_length { get; set; }
        public string civil_twilight_begin { get; set; }
        public string civil_twilight_end { get; set; }
        public string nautical_twilight_begin { get; set; }
        public string nautical_twilight_end { get; set; }
        public string astronomical_twilight_begin { get; set; }
        public string astronomical_twilight_end { get; set; }
    }

    class SunriseSunsetData
    {
        public Results results { get; set; }
        public string status { get; set; }
    }

    class SunriseSunsetService
    {
        public WeatherData GetWeatherData(string lat, string lon, string date)
        {
            var client = new RestClient("https://api.sunrise-sunset.org");

            var request = new RestRequest("json", Method.GET);
            request.AddParameter("lat", lat);
            request.AddParameter("lng", lon);
            request.AddParameter("date", date);
            request.AddParameter("formatted", "0");

            var response = client.Execute<SunriseSunsetData>(request);
            if (!response.IsSuccessful)
            {
                return null;
            }

            WeatherData data = new WeatherData();
            data.SunsetTime = DateTime.Parse(response.Data.results.sunrise);
            data.SunriseTime = DateTime.Parse(response.Data.results.sunset);
            return data;
        }
    }
}
