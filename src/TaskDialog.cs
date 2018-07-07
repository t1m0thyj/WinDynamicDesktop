using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using TaskDialogInterop;

namespace WinDynamicDesktop
{
    class AboutDialog
    {
        private static string githubLink = "https://github.com/t1m0thyj/WinDynamicDesktop";
        private static string gitterLink = "https://gitter.im/t1m0thyj/WinDynamicDesktop";
        private static string paypalLink =
            "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=H8ZZXM9ABRJFU";

        public static void Show()
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(
                Assembly.GetExecutingAssembly().Location);

            TaskDialogOptions config = new TaskDialogOptions();
            config.Title = "About WinDynamicDesktop";
            config.MainInstruction = "WinDynamicDesktop " + versionInfo.FileVersion;
            config.Content = "Port of macOS Mojave Dynamic Desktop feature to Windows 10" +
                Environment.NewLine + versionInfo.LegalCopyright;
            config.CommonButtons = TaskDialogCommonButtons.Close;
            config.CustomMainIcon = Properties.Resources.AppIcon;

            if (!UwpDesktop.IsRunningAsUwp())
            {
                config.CommandButtons = new string[]
                {
                    "View on GitHub",
                    "Chat on Gitter",
                    "Support with a donation"
                };
            }
            else
            {
                config.CommandButtons = new string[]
                {
                    "View on GitHub",
                    "Chat on Gitter"
                };
            }

            TaskDialogResult result = TaskDialog.Show(config);
            ProcessResult(result);
        }

        private static void ProcessResult(TaskDialogResult result)
        {
            switch (result.CommandButtonResult)
            {
                case 0: // View on GitHub
                    Process.Start(githubLink);
                    break;
                case 1: // Chat on Gitter
                    Process.Start(gitterLink);
                    break;
                case 2: // Support with a donation (desktop only)
                    Process.Start(paypalLink);
                    break;
            }
        }
    }
}
