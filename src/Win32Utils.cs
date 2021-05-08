// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;

namespace WinDynamicDesktop
{
    class Win32Utils
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
            DpiAwarenessUnaware = -1,
            DpiAwarenessSystemAware = -2,
            DpiAwarenessPerMonitorAware = -3,
            DpiAwarenessPerMonitorAwareV2 = -4
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

        [DllImport("shcore.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwareness(ProcessDpiAwareness value);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        public static void SetDpiAwareness()
        {
            var user32 = LoadLibrary("user32.dll");
            var method = GetProcAddress(user32, nameof(SetProcessDpiAwarenessContext));

            if (method != IntPtr.Zero)
            {
                if (SetProcessDpiAwarenessContext((IntPtr)DpiAwarenessContext.DpiAwarenessPerMonitorAwareV2) ||
                    SetProcessDpiAwarenessContext((IntPtr)DpiAwarenessContext.DpiAwarenessPerMonitorAware))
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

        // Code to change ListView appearance from https://stackoverflow.com/a/4463114/5504760
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);
    }
}
