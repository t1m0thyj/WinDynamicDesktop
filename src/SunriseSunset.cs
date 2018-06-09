using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace WinDynamicDesktop
{
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
        public SunriseSunsetData GetWeatherData(string lat, string lon)
        {
            var client = new RestClient("https://api.sunrise-sunset.org");

            var request = new RestRequest("json", Method.GET);
            request.AddParameter("lat", lat);
            request.AddParameter("lng", lon);
            request.AddParameter("formatted", "0");

            var response = client.Execute<SunriseSunsetData>(request);

            return response.Data;
        }
    }
}
