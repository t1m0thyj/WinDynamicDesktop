using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

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
            Environment.CurrentDirectory = UwpDesktop.GetCurrentDirectory();
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(
                OnUnhandledException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormWrapper());
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string errorMessage = ((Exception)e.ExceptionObject).ToString() + Environment.NewLine;
            string logFilename = Environment.GetCommandLineArgs()[0] + ".log";

            try
            {
                File.AppendAllText(logFilename, errorMessage);

                MessageBox.Show("See the logfile '" + Path.Combine(Directory.GetCurrentDirectory(),
                    logFilename) + "' for details", "Errors occurred", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show("The logfile '" + Path.Combine(Directory.GetCurrentDirectory(),
                    logFilename) + "' could not be opened:" + Environment.NewLine + " " + errorMessage,
                    "Errors occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
