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
            if (JsonConfig.settings.location != null)
            {
                isReady = true;
            }
            else if (!UwpDesktop.hasLocationAccess)
            {
                UpdateLocation();
            }
            else
            {
                try
                {
                    UwpLocation.UnsafeUpdateGeoposition();
                    isReady = true;
                }
                catch
                {
                    UpdateLocation();
                }
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
