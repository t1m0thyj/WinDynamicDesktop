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
            //var uri = new Uri("ms-appx://Local/images/" + imageFilename);
            //var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "images", imageFilename));
            var profileSettings = Windows.System.UserProfile.UserProfilePersonalizationSettings.Current;
            await profileSettings.TrySetWallpaperImageAsync(file);
        }
    }

    class UwpStartupManager : StartupManager
    {
        private bool startOnBoot;

        public UwpStartupManager(MenuItem startupMenuItem) : base(startupMenuItem)
        {
            UpdateStatus();
        }

        private async void UpdateStatus()
        {
            var startupTask = await Windows.ApplicationModel.StartupTask.GetAsync("WinDynamicDesktopUwp");

            switch (startupTask.State)
            {
                case Windows.ApplicationModel.StartupTaskState.Disabled:
                    startOnBoot = false;
                    _menuItem.Checked = startOnBoot;
                    break;
                case Windows.ApplicationModel.StartupTaskState.DisabledByUser:
                    startOnBoot = false;
                    _menuItem.Checked = startOnBoot;
                    break;
                case Windows.ApplicationModel.StartupTaskState.Enabled:
                    startOnBoot = true;
                    _menuItem.Checked = startOnBoot;
                    break;
            }
        }

        public override async void ToggleStartOnBoot()
        {
            var startupTask = await Windows.ApplicationModel.StartupTask.GetAsync("WinDynamicDesktopUwp");

            if (!startOnBoot)
            {
                var state = await startupTask.RequestEnableAsync();

                switch (state)
                {
                    case Windows.ApplicationModel.StartupTaskState.DisabledByUser:
                        //MessageBox.Show("The task has been disabled by the user");
                        startOnBoot = false;
                        _menuItem.Checked = startOnBoot;
                        break;
                    case Windows.ApplicationModel.StartupTaskState.Enabled:
                        startOnBoot = true;
                        _menuItem.Checked = startOnBoot;
                        break;
                }
            }
            else
            {
                startupTask.Disable();

                startOnBoot = false;
                _menuItem.Checked = startOnBoot;
            }
        }
    }
}
