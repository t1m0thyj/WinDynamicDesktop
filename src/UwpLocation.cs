using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    class UwpLocation
    {
        public static async void RequestAccess()
        {
            var accessStatus = await Windows.Devices.Geolocation.Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed:
                    UwpDesktop.hasLocationAccess = true;
                    break;
                case Windows.Devices.Geolocation.GeolocationAccessStatus.Denied:
                    System.Windows.Forms.MessageBox.Show("denied");
                    UwpDesktop.hasLocationAccess = false;
                    break;
                case Windows.Devices.Geolocation.GeolocationAccessStatus.Unspecified:
                    System.Windows.Forms.MessageBox.Show("unspecified");
                    UwpDesktop.hasLocationAccess = false;
                    break;
            }
        }

        public static async void UnsafeUpdateGeoposition()
        {
            var geolocator = new Windows.Devices.Geolocation.Geolocator
            {
                DesiredAccuracyInMeters = 0
            };

            var pos = await geolocator.GetGeopositionAsync(maximumAge: TimeSpan.FromMinutes(5),
                timeout: TimeSpan.FromSeconds(10));

            JsonConfig.settings.latitude = pos.Coordinate.Point.Position.Latitude.ToString();
            JsonConfig.settings.longitude = pos.Coordinate.Point.Position.Longitude.ToString();
            JsonConfig.SaveConfig();
        }

        public static void UpdateGeoposition()
        {
            try
            {
                UnsafeUpdateGeoposition();
            }
            catch
            {
            }
        }
    }
}
