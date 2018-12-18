using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class UwpLocation
    {
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
            catch   // Error when attempting to show UWP location prompt in WPF app
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
                DialogResult result = MessageBox.Show("WinDynamicDesktop needs location access " +
                    "for this feature. Click OK to open the Windows 10 location settings and " +
                    "grant location access to the app, then select the checkbox again.",
                    "Location Access", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                if (result == DialogResult.OK)
                {
                    await Windows.System.Launcher.LaunchUriAsync(
                        new Uri("ms-settings:privacy-location"));
                }
            }

            return hasAccess;
        }

        private static async Task<Windows.Devices.Geolocation.BasicGeoposition>
            UnsafeUpdateGeoposition()
        {
            var geolocator = new Windows.Devices.Geolocation.Geolocator
            {
                DesiredAccuracyInMeters = 0
            };

            var pos = await geolocator.GetGeopositionAsync(maximumAge: TimeSpan.FromMinutes(5),
                timeout: TimeSpan.FromSeconds(10));

            return pos.Coordinate.Point.Position;
        }

        public static async Task<bool> UpdateGeoposition()
        {
            try
            {
                var pos = await UnsafeUpdateGeoposition();
                JsonConfig.settings.latitude = pos.Latitude.ToString(CultureInfo.InvariantCulture);
                JsonConfig.settings.longitude = pos.Longitude.ToString(
                    CultureInfo.InvariantCulture);

                return true;
            }
            catch { }

            return false;
        }
    }
}
