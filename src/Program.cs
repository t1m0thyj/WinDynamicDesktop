// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;

            string localFolder = UwpDesktop.GetHelper().GetLocalFolder();
            Application.ThreadException += (sender, e) => OnThreadException(sender, e, localFolder);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => OnUnhandledException(sender, e, localFolder);

            string cwd = localFolder;
            if (File.Exists(Path.Combine(localFolder, "WinDynamicDesktop.pth")))
            {
                cwd = File.ReadAllText(Path.Combine(localFolder, "WinDynamicDesktop.pth")).Trim();
            }
            Directory.SetCurrentDirectory(cwd);

            DpiHelper.SetDpiAwareness();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext(args));
        }

        // Will attempt to load missing assembly from either x86 or x64 subdir
        private static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath;
                if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "cef",
                                                       Environment.Is64BitProcess ? "x64" : "x86")))
                {
                    archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "cef",
                                                       Environment.Is64BitProcess ? "x64" : "x86",
                                                       assemblyName);
                }
                else {
                    archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "cef",
                                                           assemblyName);
                }

                return File.Exists(archSpecificPath)
                           ? Assembly.LoadFile(archSpecificPath)
                           : null;
            }

            return null;
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e, string cwd)
        {
            LogError(cwd, e.Exception);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e, string cwd)
        {
            LogError(cwd, e.ExceptionObject as Exception);
        }

        private static void LogError(string cwd, Exception exc)
        {
            string errorMessage = exc.ToString();
            string logFilename = Path.Combine(cwd, Path.GetFileName(Environment.GetCommandLineArgs()[0]) + ".log");

            try
            {
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                File.AppendAllText(logFilename, string.Format("[{0}] {1}\n\n", timestamp, errorMessage));

                MessageDialog.ShowError(string.Format("See the logfile '{0}' for details", logFilename),
                    "Errors occurred");
            }
            catch
            {
                MessageDialog.ShowError(string.Format("The logfile '{0}' could not be opened:\n {1}", logFilename,
                    errorMessage), "Errors occurred");
            }
        }
    }
}