// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class LocationManager
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static ScheduleDialog locationDialog;

        public static void Initialize()
        {
            if (JsonConfig.settings.locationMode == 1 && !UwpLocation.HasAccess())
            {
                JsonConfig.settings.locationMode = 0;
                JsonConfig.settings.latitude = null;
                JsonConfig.settings.longitude = null;
            }
        }

        public static void ChangeLocation()
        {
            if (locationDialog == null)
            {
                locationDialog = new ScheduleDialog();
                locationDialog.FormClosed += OnLocationDialogClosed;
                locationDialog.Show();
            }

            locationDialog.BringToFront();
        }

        private static void OnLocationDialogClosed(object sender, FormClosedEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                locationDialog = null;
                LaunchSequence.NextStep();
            }
        }

        public static async Task FetchLocationData(string locationStr, ScheduleDialog dialog)
        {
            var client = new RestClient("https://us1.locationiq.com");

            var request = new RestRequest("v1/search.php");
            request.AddParameter("key", Environment.GetEnvironmentVariable("LOCATIONIQ_API_KEY"));
            request.AddParameter("q", locationStr);
            request.AddParameter("format", "json");
            request.AddParameter("limit", "1");

            var response = await client.ExecuteAsync<List<LocationIQData>>(request);

            if (response.IsSuccessful)
            {
                JsonConfig.settings.location = locationStr;
                HandleLocationSuccess(response.Data[0], dialog);
            }
            else
            {
                MessageDialog.ShowWarning(_("The location you entered was invalid, or you are not connected to " +
                    "the Internet. Check your Internet connection and try a different location. You can use a " +
                    "complete address or just the name of your city/region."), _("Error"));
            }
        }

        private static void HandleLocationSuccess(LocationIQData data, ScheduleDialog dialog)
        {
            JsonConfig.settings.latitude = double.Parse(data.lat, CultureInfo.InvariantCulture);
            JsonConfig.settings.longitude = double.Parse(data.lon, CultureInfo.InvariantCulture);
            SolarData solarData = SunriseSunsetService.GetSolarData(DateTime.Today);

            DialogResult result = MessageDialog.ShowQuestion(string.Format(_("Is this location correct?\n\n{0}\n{1}"),
                data.display_name, SunriseSunsetService.GetSunriseSunsetString(solarData)), _("Question"));

            if (result == DialogResult.Yes)
            {
                dialog.Invoke(new Action(() => dialog.HandleScheduleChange()));
            }
        }
    }
}
