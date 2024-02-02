// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    class ScriptArgs
    {
        public int daySegment2;
        public int? daySegment4;
        public string[] imagePaths;

        public bool Equals(ScriptArgs other)
        {
            return other != null && this.daySegment2 == other.daySegment2 &&
                this.daySegment4 == other.daySegment4 && this.imagePaths.SequenceEqual(other.imagePaths);
        }
    }

    class ScriptManager
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private static ScriptArgs lastArgs;

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
            if (!JsonConfig.settings.enableScripts || (args.Equals(lastArgs) && !forceUpdate))
            {
                return;
            }

            LoggingHandler.LogMessage("Running scripts with arguments: {0}", args);
            foreach (string scriptPath in Directory.EnumerateFiles("scripts", "*.ps1"))
            {
                Task.Run(() => RunScript(scriptPath, args));
            }
            lastArgs = args;
        }

        private static async void RunScript(string path, ScriptArgs args)
        {
            Process proc = new Process();
            string command = BuildCommandString(Path.GetFileName(path), args);
            proc.StartInfo = new ProcessStartInfo(ExistsOnPath("pwsh.exe") ? "pwsh.exe" : "powershell.exe",
                "-NoProfile -ExecutionPolicy Bypass -Command \"" + command + "\"")
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
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
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0 || errors.Length > 0)
            {
                LoggingHandler.LogMessage("Script failed with errors: {0}", errors);
                MessageDialog.ShowWarning(string.Format(_("Error(s) running PowerShell script '{0}':\n\n{1}"), path,
                    errors), _("Script Error"));
            }
        }

        private static string BuildCommandString(string filename, ScriptArgs args)
        {
            string cmdStr = "& \\\".\\" + filename + "\\\"" +
                " -daySegment2 " + args.daySegment2.ToString() +
                " -daySegment4 " + (args.daySegment4 ?? -1).ToString() +
                " -nightMode " + Convert.ToInt32(JsonConfig.settings.darkMode);
            if (args.imagePaths.Length > 0)
            {
                cmdStr += " -imagePath \\\"" + args.imagePaths[0] + "\\\"";
            }
            return cmdStr;
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
