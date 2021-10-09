// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;

namespace WinDynamicDesktop
{
    class ErrorHandler
    {
        public static void LogError(string cwd, Exception exc)
        {
            string errorMessage = exc.ToString();
            string logFilename = Path.Combine(cwd, Path.GetFileName(Environment.GetCommandLineArgs()[0]) + ".log");

            try
            {
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                File.AppendAllText(logFilename, string.Format("[{0}] {1}\n\n", timestamp, errorMessage));
                WriteDebugLog();

                MessageDialog.ShowError(string.Format("See the logfile '{0}' for details", logFilename),
                    "Errors occurred");
            }
            catch
            {
                MessageDialog.ShowError(string.Format("The logfile '{0}' could not be opened:\n {1}", logFilename,
                    errorMessage), "Errors occurred");
            }
        }

        private static void WriteDebugLog()
        {
            AppConfig settings = null;

            try
            {
                string jsonText = File.ReadAllText("settings.json");
                settings = JsonConvert.DeserializeObject<AppConfig>(jsonText);
            }
            catch { /* Do nothing */ }

            using (StreamWriter debugLog = new StreamWriter("debug.log"))
            {
                debugLog.WriteLine("//" + DateTime.Now.ToString());

                if (settings != null)
                {
                    if (settings.location != null)
                    {
                        settings.location = "XXX";
                    }
                    if (settings.latitude.HasValue)
                    {
                        settings.latitude = Math.Round(settings.latitude.Value, MidpointRounding.AwayFromZero);
                    }
                    if (settings.longitude.HasValue)
                    {
                        settings.longitude = Math.Round(settings.longitude.Value, MidpointRounding.AwayFromZero);
                    }

                    debugLog.WriteLine("./settings.json");
                    debugLog.WriteLine(JsonConvert.SerializeObject(settings, Formatting.Indented));
                }
                else
                {
                    debugLog.WriteLine("WARNING: Settings file not found or invalid");
                }

                if (Directory.Exists("scripts"))
                {
                    foreach (string path in Directory.EnumerateFiles("scripts", "*.ps1"))
                    {
                        debugLog.WriteLine("./" + path.Replace('\\', '/'));
                    }
                }
                else
                {
                    debugLog.WriteLine("WARNING: Scripts directory not found");
                }

                if (Directory.Exists("themes"))
                {
                    foreach (string path in Directory.EnumerateFiles("themes", "*", SearchOption.AllDirectories))
                    {
                        debugLog.WriteLine("./" + path.Replace('\\', '/'));

                        if (Path.GetExtension(path) == ".json")
                        {
                            debugLog.WriteLine(File.ReadAllText(path));
                        }
                    }
                }
                else
                {
                    debugLog.WriteLine("WARNING: Themes directory not found");
                }
            }
        }
    }
}
