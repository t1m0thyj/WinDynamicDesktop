// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;

namespace WinDynamicDesktop
{
    class LoggingHandler
    {
        private static readonly object debugLogLock = new object();

        public static void LogError(string cwd, Exception exc)
        {
            string errorMessage = exc.ToString();
            string logFilename = Path.Combine(cwd, Path.GetFileName(Environment.GetCommandLineArgs()[0]) + ".log");

            try
            {
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                File.AppendAllText(logFilename, string.Format("[{0}] {1}\n\n", timestamp, errorMessage));
                WriteReportLog(exc);

                MessageDialog.ShowError(string.Format("See the logfile '{0}' for details", logFilename),
                    "Errors occurred");
            }
            catch
            {
                MessageDialog.ShowError(string.Format("The logfile '{0}' could not be opened:\n {1}", logFilename,
                    errorMessage), "Errors occurred");
            }
        }

        public static void LogMessage(string message, params object[] values)
        {
#if !DEBUG
            if (!JsonConfig.settings.debugLogging)
            {
                return;
            }
#endif

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            if (values.Length > 0)
            {
                for (int i = 0; i < values.Length; i++)
                {
#pragma warning disable SYSLIB0050
                    if (!values[i].GetType().IsSerializable)
#pragma warning restore SYSLIB0050
                    {
                        values[i] = JsonConvert.SerializeObject(values[i]);
                    }
                }
                message = string.Format(message, values);
            }

            lock (debugLogLock)
            {
                File.AppendAllText("debug.log", string.Format("[{0}] {1}\n", timestamp, message));
            }
        }

        public static void RotateDebugLog()
        {
#if !DEBUG
            if (!JsonConfig.settings.debugLogging)
            {
                return;
            }
#endif

            if (File.Exists("debug.log") && new FileInfo("debug.log").Length > 1e6)
            {
                File.Move("debug.log", "debug.old.log", true);
            }
        }

        private static void WriteReportLog(Exception exc)
        {
            AppConfig settings = null;

            try
            {
                string jsonText = File.ReadAllText("settings.json");
                settings = JsonConvert.DeserializeObject<AppConfig>(jsonText);
            }
            catch { /* Do nothing */ }

            using (StreamWriter reportLog = new StreamWriter("report.log"))
            {
                reportLog.WriteLine("//" + DateTime.Now.ToString());
                reportLog.WriteLine(JsonConvert.SerializeObject(exc, Formatting.Indented));

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

                    reportLog.WriteLine("./settings.json");
                    reportLog.WriteLine(JsonConvert.SerializeObject(settings, Formatting.Indented));
                }
                else
                {
                    reportLog.WriteLine("WARNING: Settings file not found or invalid");
                }

                if (Directory.Exists("scripts"))
                {
                    foreach (string path in Directory.EnumerateFiles("scripts", "*.ps1"))
                    {
                        reportLog.WriteLine("./" + path.Replace('\\', '/'));
                    }
                }
                else
                {
                    reportLog.WriteLine("WARNING: Scripts directory not found");
                }

                if (Directory.Exists("themes"))
                {
                    foreach (string path in Directory.EnumerateFiles("themes", "*", SearchOption.AllDirectories))
                    {
                        reportLog.WriteLine("./" + path.Replace('\\', '/'));

                        if (Path.GetExtension(path) == ".json")
                        {
                            reportLog.WriteLine(File.ReadAllText(path));
                        }
                    }
                }
                else
                {
                    reportLog.WriteLine("WARNING: Themes directory not found");
                }
            }
        }
    }
}
