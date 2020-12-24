// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Windows.Forms;

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
            Application.ThreadException += (sender, e) => ErrorHandler.LogError(localFolder, e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                ErrorHandler.LogError(localFolder, e.ExceptionObject as Exception);

            string cwd = localFolder;
            if (File.Exists(Path.Combine(localFolder, "WinDynamicDesktop.pth")))
            {
                cwd = Environment.ExpandEnvironmentVariables(
                    File.ReadAllText(Path.Combine(localFolder, "WinDynamicDesktop.pth")).Trim());
                if (!Directory.Exists(cwd))
                {
                    Directory.CreateDirectory(cwd);
                }
            }
            Directory.SetCurrentDirectory(cwd);

            Win32Utils.SetDpiAwareness();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext(args));
        }
    }
}
