// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    class UwpLocation
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;

        private static async Task<bool> UnsafeRequestAccess()
        {
            var accessStatus = await Windows.Devices.Geolocation.Geolocator.RequestAccessAsync();

            return (accessStatus == Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed);
        }

        public static bool HasAccess()
        {
            bool hasAccess = false;

            try
            {
                hasAccess = Task.Run(() => UnsafeRequestAccess()).Result;
            }
            catch   // Error when attempting to show UWP location prompt in WinForms app
            {
                hasAccess = false;
            }

            return hasAccess;
        }

        public static async Task<bool> RequestAccess()
        {
            bool hasAccess = HasAccess();

            if (!hasAccess)
            {
                AppContext.ShowPopup(_("In Windows 10 location settings, grant location access to WinDynamicDesktop. " +
                    "Then return to the app and click \"Check for Permission\"."));

                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));
            }

            return hasAccess;
        }

        private static async Task<Windows.Devices.Geolocation.BasicGeoposition> UnsafeUpdateGeoposition()
        {
            var geolocator = new Windows.Devices.Geolocation.Geolocator();
            var geoposition = await geolocator.GetGeopositionAsync(maximumAge: TimeSpan.FromMinutes(1),
                timeout: TimeSpan.FromSeconds(10));

            return geoposition.Coordinate.Point.Position;
        }

        public static async Task<bool> UpdateGeoposition()
        {
            try
            {
                var pos = await UnsafeUpdateGeoposition();
                JsonConfig.settings.latitude = pos.Latitude;
                JsonConfig.settings.longitude = pos.Longitude;

                return true;
            }
            catch { /* Do nothing */ }

            return false;
        }
    }
}
