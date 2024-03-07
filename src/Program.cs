// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Text;
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
            Application.ThreadException += (sender, e) => LoggingHandler.LogError(localFolder, e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                LoggingHandler.LogError(localFolder, e.ExceptionObject as Exception);
            Directory.SetCurrentDirectory(FindCwd(localFolder));
            LoadDotEnv();

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

        static void LoadDotEnv()
        {
            string envText = Encoding.UTF8.GetString(Properties.Resources.DotEnv);
            string[] envLines = envText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in envLines)
            {
                string[] parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                {
                    string rawValue = parts[1].StartsWith("base64:") ?
                        Encoding.UTF8.GetString(Convert.FromBase64String(parts[1].Substring(7))) : parts[1];
                    Environment.SetEnvironmentVariable(parts[0], rawValue);
                }
            }
        }
    }
}
