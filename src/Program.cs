using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace WinDynamicDesktop
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string localFolder = UwpDesktop.GetHelper().GetLocalFolder();
            Application.ThreadException +=
                (sender, e) => OnThreadException(sender, e, localFolder);
            AppDomain.CurrentDomain.UnhandledException +=
                (sender, e) => OnUnhandledException(sender, e, localFolder);

            string cwd = localFolder;
            if (File.Exists(Path.Combine(localFolder, "WinDynamicDesktop.pth")))
            {
                cwd = File.ReadAllText(Path.Combine(localFolder, "WinDynamicDesktop.pth")).Trim();
            }
            Directory.SetCurrentDirectory(cwd);

            SetDpiAwareness();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext());
        }

        // Docs for PROCESS_DPI_AWARENESS and SetProcessDpiAwareness here:
        // https://docs.microsoft.com/en-us/windows/desktop/api/shellscalingapi/
        private enum ProcessDpiAwareness
        {
            DpiUnaware = 0,
            SystemDpiAware = 1,
            PerMonitorDpiAware = 2
        }

        [System.Runtime.InteropServices.DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(ProcessDpiAwareness value);

        static void SetDpiAwareness()
        {
            try
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    SetProcessDpiAwareness(ProcessDpiAwareness.PerMonitorDpiAware);
                }
            }
            catch { }
        }

        static void OnThreadException(object sender, ThreadExceptionEventArgs e, string cwd)
        {
            LogError(cwd, e.Exception);
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e, string cwd)
        {
            LogError(cwd, e.ExceptionObject as Exception);
        }

        static void LogError(string cwd, Exception exc)
        {
            string errorMessage = exc.ToString() + "\n";
            string logFilename = Path.Combine(cwd,
                Path.GetFileName(Environment.GetCommandLineArgs()[0]) + ".log");

            try
            {
                File.AppendAllText(logFilename, errorMessage);

                MessageBox.Show("See the logfile '" + logFilename + "' for details",
                    "Errors occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show("The logfile '" + logFilename + "' could not be opened:\n " +
                    errorMessage, "Errors occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
