using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    class LocationManager
    {
        public static bool isReady = false;

        private static InputDialog locationDialog;

        public static void Initialize()
        {
            if (!UwpDesktop.IsRunningAsUwp())
            {
                JsonConfig.settings.useWindowsLocation = false;
            }
            else if (JsonConfig.settings.useWindowsLocation && !UwpLocation.HasAccess())
            {
                JsonConfig.settings.useWindowsLocation = false;
                JsonConfig.settings.latitude = null;
                JsonConfig.settings.longitude = null;
            }

            if (JsonConfig.settings.latitude != null && JsonConfig.settings.longitude != null)
            {
                isReady = true;
            }
            else
            {
                UpdateLocation();
            }
        }

        public static void UpdateLocation()
        {
            if (locationDialog == null)
            {
                locationDialog = new InputDialog();
                locationDialog.FormClosed += OnLocationDialogClosed;
                locationDialog.Show();
            }
            else
            {
                locationDialog.Activate();
            }
        }

        private static void OnLocationDialogClosed(object sender, EventArgs e)
        {
            locationDialog = null;
            isReady = true;

            AppContext.RunInBackground();
        }
    }
}
