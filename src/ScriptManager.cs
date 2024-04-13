// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    class ScriptArgs
    {
        public int daySegment2;
        public int daySegment4;
        public int themeMode;
        public string[] imagePaths;
    }

    class ScriptManager
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static string lastArgs;

        public static void Initialize()
        {
            Directory.CreateDirectory("scripts");

            string urlShortcutFile = Path.Combine("scripts", _("Browse for scripts online") + ".url");
            if (!File.Exists(urlShortcutFile))
            {
                File.WriteAllText(urlShortcutFile, "[InternetShortcut]\nURL=https://windd.info/scripts/");
            }
        }

        public static void ToggleEnableScripts()
        {
            bool enableScripts = JsonConfig.settings.enableScripts ^ true;
            TrayMenu.enableScriptsItem.Checked = enableScripts;
            JsonConfig.settings.enableScripts = enableScripts;

            if (enableScripts)
            {
                int scriptCount = Directory.GetFiles("scripts", "*.ps1").Length;
                AppContext.ShowPopup(string.Format(_("Found {0} PowerShell script(s) to enable"), scriptCount));
                if (scriptCount > 0)
                {
                    lastArgs = null;
                    AppContext.scheduler.Run();
                }
            }
        }

        public static void RunScripts(ScriptArgs args, bool forceUpdate = false)
        {
            string jsonArgs = JsonConvert.SerializeObject(args, Formatting.None);
            if (!JsonConfig.settings.enableScripts || (jsonArgs.Equals(lastArgs) && !forceUpdate))
            {
                return;
            }

            LoggingHandler.LogMessage("Running scripts with arguments: {0}", args);
            foreach (string scriptPath in Directory.EnumerateFiles("scripts", "*.ps1"))
            {
                Task.Run(() => RunScript(scriptPath, jsonArgs));
            }
            lastArgs = jsonArgs;
        }

        private static async void RunScript(string path, string jsonArgs)
        {
            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo(ExistsOnPath("pwsh.exe") ? "pwsh.exe" : "powershell.exe",
                "-NoProfile -ExecutionPolicy Bypass -File \"" + Path.GetFileName(path) + "\"")
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetDirectoryName(path)
            };
            LoggingHandler.LogMessage("Running PowerShell script: {0}", proc.StartInfo.Arguments);

            var errors = new StringBuilder();
            proc.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errors.Append(e.Data + "\n");
                }
            };
            proc.Start();
            proc.BeginErrorReadLine();
            using (StreamWriter sw = proc.StandardInput)
            {
                sw.WriteLine(jsonArgs);
            }
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0 || errors.Length > 0)
            {
                LoggingHandler.LogMessage("Script failed with errors: {0}", errors);
                MessageDialog.ShowWarning(string.Format(_("Error(s) running PowerShell script '{0}':\n\n{1}"), path,
                    errors), _("Script Error"));
            }
        }

        private static bool ExistsOnPath(string filename)
        {
            if (File.Exists(filename))
            {
                return true;
            }
            foreach (string path in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator))
            {
                if (File.Exists(Path.Combine(path.Trim(), filename)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
