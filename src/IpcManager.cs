// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using NamedPipeWrapper;
using System;
using System.Threading;

namespace WinDynamicDesktop
{
    class IpcManager : IDisposable
    {
        public bool isFirstInstance;

        private Mutex _mutex;
        private NamedPipeServer<string[]> namedPipeServer;

        public IpcManager()
        {
            _mutex = new Mutex(true, "WinDynamicDesktop", out isFirstInstance);
            GC.KeepAlive(_mutex);
        }

        public void Dispose()
        {
            _mutex?.Dispose();
            namedPipeServer?.Stop();
        }

        public void ListenForArgs(Action<string[]> listener)
        {
            namedPipeServer = new NamedPipeServer<string[]>("WinDynamicDesktop");
            namedPipeServer.ClientMessage += (conn, args) => listener(args);
            namedPipeServer.Start();
        }

        public void SendArgsToFirstInstance(string[] args)
        {
            var namedPipeClient = new NamedPipeClient<string[]>("WinDynamicDesktop");
            namedPipeClient.Start();
            namedPipeClient.WaitForConnection();
            namedPipeClient.PushMessage(args);
            namedPipeClient.WaitForDisconnection();
            namedPipeClient.Stop();
        }
    }
}
