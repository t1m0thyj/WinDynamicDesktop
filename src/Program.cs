// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string localFolder = UwpDesktop.GetHelper().GetLocalFolder();
            Application.ThreadException += (sender, e) => ErrorHandler.LogError(localFolder, e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                ErrorHandler.LogError(localFolder, e.ExceptionObject as Exception);
            Directory.SetCurrentDirectory(FindCwd(localFolder));

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new AppContext(args));
        }

        static string FindCwd(string localFolder)
        {
            string cwd = localFolder;
            string pathFile = Path.Combine(localFolder, "WinDynamicDesktop.pth");

            if (File.Exists(pathFile))
            {
                cwd = Environment.ExpandEnvironmentVariables(File.ReadAllText(pathFile).Trim());

                if (!Directory.Exists(cwd))
                {
                    Directory.CreateDirectory(cwd);
                }
            }

            return cwd;
        }
    }
}
