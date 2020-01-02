using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management.Automation;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class ScriptArgs
    {
        public int daySegment2;
        public int daySegment4;
        public string imagePath;
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

        private static void RunScript(string path, ScriptArgs args)
        {
            using var ps = PowerShell.Create();
            ps.AddScript("Set-ExecutionPolicy Bypass -Scope Process -Force");
            ps.AddScript(File.ReadAllText(path));
            ps.AddArgument(args.daySegment2);
            ps.AddArgument(args.daySegment4);
            ps.AddArgument(args.imagePath);
            ps.Invoke();

            if (ps.Streams.Error.Count > 0)
            {
                MessageDialog.ShowWarning(string.Format(
                    _("Error running the PowerShell script '{0}':\n\n{1}"), path,
                    string.Join("\n", ps.Streams.Error.ReadAll().Select(
                        (er) => er.Exception.ToString()))), _("Script Error"));
            }
        }
    }
}
