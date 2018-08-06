using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class UwpHelper
    {
        public static string GetCurrentDirectory()
        {
            return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        }

        public static async void SetWallpaper(string imageFilename)
        {
            var uri = new Uri("ms-appdata:///local/images/" + imageFilename);
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);

            var profileSettings =
                Windows.System.UserProfile.UserProfilePersonalizationSettings.Current;
            await profileSettings.TrySetWallpaperImageAsync(file);
        }
    }

    class UwpStartupManager : StartupManager
    {
        public UwpStartupManager(MenuItem startupMenuItem) : base(startupMenuItem)
        {
            UpdateStatus();
        }

        private async void UpdateStatus()
        {
            var startupTask = await Windows.ApplicationModel.StartupTask.GetAsync(
                "WinDynamicDesktopUwp");

            switch (startupTask.State)
            {
                case Windows.ApplicationModel.StartupTaskState.Disabled:
                    startOnBoot = false;
                    break;
                case Windows.ApplicationModel.StartupTaskState.DisabledByUser:
                    startOnBoot = false;
                    _menuItem.Enabled = false;
                    break;
                case Windows.ApplicationModel.StartupTaskState.Enabled:
                    startOnBoot = true;
                    break;
            }

            _menuItem.Checked = startOnBoot;
        }

        public override async void ToggleStartOnBoot()
        {
            var startupTask = await Windows.ApplicationModel.StartupTask.GetAsync(
                "WinDynamicDesktopUwp");

            if (!startOnBoot)
            {
                var state = await startupTask.RequestEnableAsync();

                switch (state)
                {
                    case Windows.ApplicationModel.StartupTaskState.DisabledByUser:
                        startOnBoot = false;
                        break;
                    case Windows.ApplicationModel.StartupTaskState.Enabled:
                        startOnBoot = true;
                        break;
                }
            }
            else
            {
                startupTask.Disable();
                startOnBoot = false;
            }

            _menuItem.Checked = startOnBoot;
        }
    }
}
