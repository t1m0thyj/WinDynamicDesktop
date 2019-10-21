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
    class LocationIQService
    {
        private static readonly string apiKey = Encoding.UTF8.GetString(Convert.FromBase64String(
            "cGsuYmRhNTk1NDRhN2VjZWMxYjAxMDZkNzg5MzdlMDQzOTk ="));
        private static readonly Func<string, string> _ = Localization.GetTranslation;

        private static void HandleLocationSuccess(LocationIQData data, LocationDialog dialog)
        {
            JsonConfig.settings.latitude = data.lat;
            JsonConfig.settings.longitude = data.lon;
            SolarData solarData = SunriseSunsetService.GetSolarData(DateTime.Today);

            DialogResult result = MessageDialog.ShowQuestion(string.Format(_("Is this location " +
                "correct?\n\n{0}\n{1}"), data.display_name,
                SunriseSunsetService.GetSunriseSunsetString(solarData)), _("Question"), true);

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
                    MessageDialog.ShowWarning(_("The location you entered was invalid, or you " +
                        "are not connected to the Internet. Check your Internet connection and " +
                        "try a different location. You can use a complete address or just the " +
                        "name of your city/region."), _("Error"));
                }
            });
        }
    }
}
