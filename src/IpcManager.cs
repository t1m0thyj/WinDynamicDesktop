// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class IpcManager : IDisposable
    {
        public bool isFirstInstance;

        private Mutex _mutex;
        private NamedPipeServerStream namedPipeServer;

        public IpcManager()
        {
            _mutex = new Mutex(true, "WinDynamicDesktop", out isFirstInstance);
            GC.KeepAlive(_mutex);
        }

        public void Dispose()
        {
            _mutex?.Dispose();
            namedPipeServer?.Close();
            namedPipeServer?.Dispose();
        }

        public void ListenForArgs(ApplicationContext app)
        {
            Task.Factory.StartNew(() =>
            {
                namedPipeServer = new NamedPipeServerStream("WinDynamicDesktop", PipeDirection.In);
                using StreamReader reader = new StreamReader(namedPipeServer);

                while (true)
                {
                    if (!namedPipeServer.IsConnected)
                    {
                        namedPipeServer.WaitForConnection();
                    }

                    ProcessArgs(app, reader.ReadToEnd().Split(Environment.NewLine,
                        StringSplitOptions.RemoveEmptyEntries));
                    namedPipeServer.Disconnect();
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void SendArgsToFirstInstance(string[] args)
        {
            using var namedPipeClient = new NamedPipeClientStream(".", "WinDynamicDesktop", PipeDirection.Out);
            using StreamWriter writer = new StreamWriter(namedPipeClient);
            namedPipeClient.Connect();

            writer.Write(string.Join(Environment.NewLine, args));
            writer.Flush();
        }

        public void ProcessArgs(string[] initialArgs)
        {
            foreach (string arg in initialArgs)
            {
                if (arg.StartsWith("/theme", StringComparison.OrdinalIgnoreCase) && arg.IndexOf('=') != -1)
                {
                    ProcessThemeArg(arg);
                }
                else if (arg.StartsWith('/'))
                {
                    switch (arg.ToLower())
                    {
                        case "/refresh":
                            break;
                        case "/theme:auto":
                        case "/theme:light":
                        case "/theme:dark":
                            AppearanceMode mode = (AppearanceMode)Enum.Parse(typeof(AppearanceMode),
                                arg.Substring(7), true);
                            JsonConfig.settings.appearanceMode = (int)mode;
                            break;
                        default:
                            Console.WriteLine("Unrecognized command line option: " + arg);
                            break;
                    }
                }
                else if (File.Exists(arg))
                {
                    ThemeManager.importPaths.Add(arg);
                }
            }
        }

        private void ProcessArgs(ApplicationContext app, string[] args)
        {
            if (JsonConfig.settings.hideTrayIcon)
            {
                app.MainForm.BeginInvoke(AppContext.ToggleTrayIcon);
            }

            foreach (string arg in args)
            {
                if (arg.StartsWith("/theme", StringComparison.OrdinalIgnoreCase) && arg.IndexOf('=') != -1)
                {
                    string themeId = ProcessThemeArg(arg);
                    if (themeId != null)
                    {
                        ThemeShuffler.AddThemeToHistory(themeId);
                        AppContext.scheduler.Run(true);
                    }
                }
                else if (arg.StartsWith('/'))
                {
                    switch (arg.ToLower())
                    {
                        case "/refresh":
                            AppContext.scheduler.RunAndUpdateLocation(true);
                            break;
                        case "/theme:auto":
                        case "/theme:light":
                        case "/theme:dark":
                            AppearanceMode mode = (AppearanceMode)Enum.Parse(typeof(AppearanceMode),
                                arg.Substring(7), true);
                            app.MainForm.BeginInvoke(SolarScheduler.SetAppearanceMode, mode);
                            break;
                        default:
                            Console.WriteLine("Unrecognized command line option: " + arg);
                            break;
                    }
                }
                else if (File.Exists(arg))
                {
                    ThemeManager.importPaths.Add(arg);
                }
            }

            if (ThemeManager.importPaths.Count > 0 && !ThemeManager.importMode)
            {
                app.MainForm.BeginInvoke(ThemeManager.SelectTheme);
            }
        }

        private string ProcessThemeArg(string arg)
        {
            string themeId = arg.Substring(arg.IndexOf('=') + 1);
            if (ThemeManager.themeSettings.Find((theme) => theme.themeId == themeId) == null)
            {
                Console.WriteLine("Failed to set theme - unknown theme ID: " + themeId);
                return null;
            }

            if (arg.StartsWith("/theme:L=", StringComparison.OrdinalIgnoreCase))
            {
                JsonConfig.settings.lockScreenTheme = themeId;
                JsonConfig.settings.lockScreenDisplayIndex = -1;
                return themeId;
            }

            List<string> activeThemes = JsonConfig.IsNullOrEmpty(JsonConfig.settings.activeThemes) ?
                new List<string> { null } : JsonConfig.settings.activeThemes.ToList();
            if (arg.StartsWith("/theme=", StringComparison.OrdinalIgnoreCase))
            {
                activeThemes[0] = themeId;
            }
            else
            {
                if (!arg.StartsWith("/theme:", StringComparison.OrdinalIgnoreCase) ||
                    !int.TryParse(arg.AsSpan(7, arg.IndexOf('=') - 7), out int displayNumber) ||
                    displayNumber < 1 || displayNumber > Screen.AllScreens.Length)
                {
                    Console.WriteLine("Failed to set theme - invalid display number: " + arg);
                    return null;
                }

                ThemeDialogUtils.UpdateConfigForDisplay(activeThemes);
                activeThemes[displayNumber] = themeId;
            }

            JsonConfig.settings.activeThemes = activeThemes.ToArray();

            return themeId;
        }
    }
}
