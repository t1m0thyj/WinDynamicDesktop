using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using RestSharp;

namespace WinDynamicDesktop
{
    class LocationIQData
    {
        public string place_id { get; set; }
        public string licence { get; set; }
        public string osm_type { get; set; }
        public string osm_id { get; set; }
        public List<string> boundingbox { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string display_name { get; set; }
        public string @class { get; set; }
        public string type { get; set; }
        public double importance { get; set; }
        public string icon { get; set; }
    }

    class LocationIQService
    {
        private string apiKey = Encoding.UTF8.GetString(Convert.FromBase64String(
            "cGsuYmRhNTk1NDRhN2VjZWMxYjAxMDZkNzg5MzdlMDQzOTk ="));
        
        public LocationIQData GetLocationData(string locationStr)
        {
            var client = new RestClient("https://us1.locationiq.org");

            var request = new RestRequest("v1/search.php", Method.GET);
            request.AddParameter("key", apiKey);
            request.AddParameter("q", locationStr);
            request.AddParameter("format", "json");

            var response = client.Execute<List<LocationIQData>>(request);

            return response.Data[0];
        }
    }
}
