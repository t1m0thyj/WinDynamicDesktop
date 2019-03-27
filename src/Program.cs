using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
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
        static void Main(string[] args)
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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext(args));
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
            string errorMessage = exc.ToString();
            string logFilename = Path.Combine(cwd,
                Path.GetFileName(Environment.GetCommandLineArgs()[0]) + ".log");

            try
            {
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                    CultureInfo.InvariantCulture);
                File.AppendAllText(logFilename,
                    string.Format("[{0}] {1}\n\n", timestamp, errorMessage));

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
