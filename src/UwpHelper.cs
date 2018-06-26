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
                    menuItem.Checked = startOnBoot;
                    break;
                case Windows.ApplicationModel.StartupTaskState.DisabledByUser:
                    startOnBoot = false;
                    menuItem.Checked = startOnBoot;
                    break;
                case Windows.ApplicationModel.StartupTaskState.Enabled:
                    startOnBoot = true;
                    menuItem.Checked = startOnBoot;
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
                        menuItem.Checked = startOnBoot;
                        break;
                    case Windows.ApplicationModel.StartupTaskState.Enabled:
                        startOnBoot = true;
                        menuItem.Checked = startOnBoot;
                        break;
                }
            }
            else
            {
                startupTask.Disable();

                startOnBoot = false;
                menuItem.Checked = startOnBoot;
            }
        }
    }
}
