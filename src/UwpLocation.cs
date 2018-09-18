using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static async Task<bool> RequestAccess(Form dialog)
        {
            bool accessGranted = false;

            try
            {
                accessGranted = await UnsafeRequestAccess();
            }
            catch   // Error when attempting to show UWP location prompt in WPF app
            {
                accessGranted = false;
            }

            if (!accessGranted)
            {
                bool result = await Windows.System.Launcher.LaunchUriAsync(
                    new Uri("ms-settings:privacy-location"));

                if (result)
                {
                    // MessageBox hack from https://stackoverflow.com/a/20729034/5504760
                    MessageBox.Show("In the Windows 10 location settings, make sure that " +
                        "location is enabled. Once it is, scroll down to \"Choose apps that can " +
                        "use your precise location\", and turn on access for WinDynamicDesktop," +
                        "then click OK.", "WinDynamicDesktop", MessageBoxButtons.OK,
                        MessageBoxIcon.None, MessageBoxDefaultButton.Button1,
                        (MessageBoxOptions)0x40000);    // MB_TOPMOST
                    dialog.Activate();

                    try
                    {
                        accessGranted = await UnsafeRequestAccess();
                    }
                    catch { }
                }
            }

            return accessGranted;
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
                JsonConfig.settings.latitude = pos.Latitude.ToString();
                JsonConfig.settings.longitude = pos.Longitude.ToString();
                JsonConfig.SaveConfig();

                return true;
            }
            catch { }

            return false;
        }
    }
}
