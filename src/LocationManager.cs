// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

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
            LaunchSequence.NextStep();
        }
    }
}
