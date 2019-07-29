// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        private static string apiKey = Encoding.UTF8.GetString(Convert.FromBase64String(
            "cGsuYmRhNTk1NDRhN2VjZWMxYjAxMDZkNzg5MzdlMDQzOTk ="));
        private static readonly Func<string, string> _ = Localization.GetTranslation;

        private static void HandleLocationSuccess(LocationIQData data, LocationDialog dialog)
        {
            JsonConfig.settings.latitude = data.lat;
            JsonConfig.settings.longitude = data.lon;
            SolarData solarData = SunriseSunsetService.GetSolarData(DateTime.Today);

            DialogResult result = MessageBox.Show(string.Format(_("Is this location " +
                "correct?\n\n{0}\n{1}"), data.display_name,
                SunriseSunsetService.GetSunriseSunsetString(solarData)), _("Question"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                dialog.Invoke(new Action(() => dialog.HandleLocationChange()));
            }
        }

        public static void GetLocationData(string locationStr, LocationDialog dialog)
        {
            var client = new RestClient("https://us1.locationiq.org");

            var request = new RestRequest("v1/search.php");
            request.AddParameter("key", apiKey);
            request.AddParameter("q", locationStr);
            request.AddParameter("format", "json");
            request.AddParameter("limit", "1");

            client.ExecuteAsync<List<LocationIQData>>(request, response =>
            {
                if (response.IsSuccessful)
                {
                    JsonConfig.settings.location = locationStr;
                    HandleLocationSuccess(response.Data[0], dialog);
                }
                else
                {
                    MessageBox.Show(_("The location you entered was invalid, or you are not " +
                        "connected to the Internet. Check your Internet connection and try a " +
                        "different location. You can use a complete address or just the name of " +
                        "your city/region."), _("Error"), MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            });
        }
    }
}
