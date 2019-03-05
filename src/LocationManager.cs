using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    class LocationManager
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
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
                AppContext.RunInBackground();
            }
            else
            {
                ChangeLocation();

                if (JsonConfig.firstRun)
                {
                    AppContext.ShowPopup(_("Welcome! Please enter your location so the app can " +
                        "determine sunrise and sunset times."));
                }
            }
        }

        public static void ChangeLocation()
        {
            if (locationDialog == null)
            {
                locationDialog = new InputDialog();
                locationDialog.FormClosed += OnLocationDialogClosed;
                locationDialog.Show();
            }

            locationDialog.BringToFront();
        }

        private static void OnLocationDialogClosed(object sender, EventArgs e)
        {
            locationDialog = null;
            isReady = true;

            AppContext.RunInBackground();
        }
    }
}
