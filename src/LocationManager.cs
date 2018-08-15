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
            if (JsonConfig.settings.location == null)
            {
                UpdateLocation();
            }
            else
            {
                isReady = true;
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
            AppContext.BackgroundNotify();
        }
    }
}
