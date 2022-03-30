// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

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

        public void ListenForArgs(Action<string[]> listener)
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

                    listener(reader.ReadToEnd().Split(Environment.NewLine));
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
    }
}
