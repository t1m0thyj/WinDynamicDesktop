using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DesktopBridge;

namespace WinDynamicDesktop
{
    abstract class StartupManager
    {
        public abstract void ToggleStartOnBoot();
    }

    class UwpDesktop
    {
        private static bool IsRunningAsUwp()
        {
            Helpers helpers = new Helpers();

            return helpers.IsRunningAsUwp();
        }

        public static string GetCurrentDirectory()
        {
            if (!IsRunningAsUwp())
            {
                return DesktopHelper.GetCurrentDirectory();
            }
            else
            {
                return UwpHelper.GetCurrentDirectory();
            }
        }

        public static StartupManager GetStartupManager(MenuItem startupMenuItem)
        {
            if (!IsRunningAsUwp())
            {
                return new DesktopStartupManager(startupMenuItem);
            }
            else
            {
                return new UwpStartupManager(startupMenuItem);
            }
        }
    }
}
