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
            Directory.SetCurrentDirectory(UwpDesktop.GetHelper().GetCurrentDirectory());
            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext());
        }

        static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogError(e.Exception);
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogError(e.ExceptionObject as Exception);
        }

        static void LogError(Exception exc)
        {
            string errorMessage = exc.ToString() + "\n";
            string logFilename = Path.Combine(Directory.GetCurrentDirectory(),
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
