// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

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
        public int? daySegment4;
        public string[] imagePaths;
    }

    class ScriptManager
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;

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
            MainMenu.enableScriptsItem.Checked = enableScripts;
            JsonConfig.settings.enableScripts = enableScripts;

            if (enableScripts)
            {
                AppContext.wpEngine.RunScheduler();
            }
        }

        public static void RunScripts(ScriptArgs args)
        {
            if (!JsonConfig.settings.enableScripts)
            {
                return;
            }

            foreach (string scriptPath in Directory.EnumerateFiles("scripts", "*.ps1"))
            {
                Task.Run(() => RunScript(scriptPath, args));
            }
        }

        private static async void RunScript(string path, ScriptArgs args)
        {
            Process proc = new Process();
            string command = BuildCommandString(Path.GetFileName(path), args);
            proc.StartInfo = new ProcessStartInfo("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + command + "\"")
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetDirectoryName(path)
            };

            var errors = new StringBuilder();
            proc.ErrorDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data)) {
                    errors.Append(e.Data + "\n");
                }
            };
            proc.Start();
            proc.BeginErrorReadLine();
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0)
            {
                MessageDialog.ShowWarning(string.Format(_("Error(s) running PowerShell script '{0}':\n\n{1}"), path,
                    errors), _("Script Error"));
            }
        }

        private static string BuildCommandString(string filename, ScriptArgs args)
        {
            return "& \\\".\\" + filename + "\\\"" +
                " -daySegment2 " + args.daySegment2.ToString() +
                " -daySegment4 " + (args.daySegment4 ?? -1).ToString() +
                " -imagePath \\\"" + args.imagePaths[0] + "\\\"" +
                " -nightMode " + Convert.ToInt32(JsonConfig.settings.darkMode);
        }
    }
}
