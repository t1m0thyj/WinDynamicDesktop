using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopBridge;

namespace WinDynamicDesktop
{
    abstract class StartupManager
    {
        internal bool startOnBoot;

        public StartupManager()
        {
            UpdateStatus();
        }

        internal abstract void UpdateStatus();

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

        public static StartupManager GetStartupManager()
        {
            if (!IsRunningAsUwp())
            {
                return new DesktopStartupManager();
            }
            else
            {
                return new UwpStartupManager();
            }
        }

        public static void SetWallpaper(string imageFilename)
        {
            if (!IsRunningAsUwp())
            {
                DesktopHelper.SetWallpaper(imageFilename);
            }
            else
            {
                UwpHelper.SetWallpaper(imageFilename);
            }
        }
    }
}
