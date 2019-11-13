// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace WinDynamicDesktop
{
    class DpiHelper
    {
        // Code based on https://github.com/modern-forms/Modern.Forms/tree/master/src/Modern.Forms/Avalonia/Avalonia.Win32
        private enum ProcessDpiAwareness
        {
            ProcessDpiUnaware = 0,
            ProcessSystemDpiAware = 1,
            ProcessPerMonitorDpiAware = 2
        }

        private enum DpiAwarenessContext
        {
            DpiAwarenessContextUnaware = -1,
            DpiAwarenessContextSystemAware = -2,
            DpiAwarenessContextPerMonitorAware = -3,
            DpiAwarenessContextPerMonitorAwareV2 = -4
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

        [DllImport("shcore.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwareness(ProcessDpiAwareness value);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        public static void SetDpiAwareness()
        {
            var user32 = LoadLibrary("user32.dll");
            var method = GetProcAddress(user32, nameof(SetProcessDpiAwarenessContext));

            if (method != IntPtr.Zero)
            {
                if (SetProcessDpiAwarenessContext((IntPtr)DpiAwarenessContext.DpiAwarenessContextPerMonitorAwareV2) ||
                    SetProcessDpiAwarenessContext((IntPtr)DpiAwarenessContext.DpiAwarenessContextPerMonitorAware))
                {
                    return;
                }
            }

            var shcore = LoadLibrary("shcore.dll");
            method = GetProcAddress(shcore, nameof(SetProcessDpiAwareness));

            if (method != IntPtr.Zero)
            {
                SetProcessDpiAwareness(ProcessDpiAwareness.ProcessPerMonitorDpiAware);
                return;
            }

            SetProcessDPIAware();
        }
    }
}
