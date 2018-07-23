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
        internal MenuItem _menuItem;

        public StartupManager(MenuItem startupMenuItem)
        {
            _menuItem = startupMenuItem;
        }

        public abstract void ToggleStartOnBoot();
    }

    class UwpDesktop
    {
        private static bool? _isRunningAsUwp;

        public static bool IsRunningAsUwp()
        {
            if (!_isRunningAsUwp.HasValue)
            {
                Helpers helpers = new Helpers();
                _isRunningAsUwp = helpers.IsRunningAsUwp();
            }

            return _isRunningAsUwp.Value;
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
