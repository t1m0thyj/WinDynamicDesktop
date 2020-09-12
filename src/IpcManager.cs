// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NamedPipeWrapper;

namespace WinDynamicDesktop
{
    class IpcManager
    {
        public static NamedPipeServer<string[]> StartServer()
        {
            var namedPipeServer = new NamedPipeServer<string[]>("WinDynamicDesktop");
            namedPipeServer.ClientMessage += OnNamedPipeClientMessage;
            namedPipeServer.Start();
            return namedPipeServer;
        }

        public static void SendArgsToServer(string[] args)
        {
            var namedPipeClient = new NamedPipeClient<string[]>("WinDynamicDesktop");
            namedPipeClient.Start();
            namedPipeClient.WaitForConnection();
            namedPipeClient.PushMessage(args);
            namedPipeClient.WaitForDisconnection();
            namedPipeClient.Stop();
        }

        private static void OnNamedPipeClientMessage(NamedPipeConnection<string[], string[]> conn, string[] message)
        {
            ThemeManager.importPaths.AddRange(message);

            if (!ThemeManager.importMode)
            {
                AppContext.notifyIcon.ContextMenuStrip.BeginInvoke(new Action(() => ThemeManager.SelectTheme()));
            }
        }
    }
}
