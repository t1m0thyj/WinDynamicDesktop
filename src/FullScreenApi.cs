// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;

namespace WinDynamicDesktop
{
    // Code based on https://stackoverflow.com/a/10280800/5504760
    // and https://www.richard-banks.org/2007/09/how-to-detect-if-another-application-is.html
    class FullScreenApi
    {
        public bool runningFullScreen = false;
        public bool timerEventPending = false;

        private Action timerEventHandler;
        private IntPtr winEventHook;
        private WinEventDelegate winEventProc;

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowRect(IntPtr hwnd, out RECT rc);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject,
            int idChild, uint dwEventThread, uint dwmsEventTime);

        public FullScreenApi(WallpaperEngine wcs)
        {
            timerEventHandler = new Action(() => wcs.HandleTimerEvent(true));

            if (JsonConfig.settings.fullScreenPause)
            {
                SetFullScreenPause(true);
            }
        }

        public void ToggleFullScreenPause()
        {
            bool fullScreenPause = JsonConfig.settings.fullScreenPause ^ true;
            MainMenu.fullScreenItem.Checked = fullScreenPause;
            SetFullScreenPause(fullScreenPause);
            JsonConfig.settings.fullScreenPause = fullScreenPause;
        }

        private void SetFullScreenPause(bool fullScreenPause)
        {
            if (fullScreenPause)
            {
                winEventProc = new WinEventDelegate(WinEventProc);
                winEventHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero,
                    winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
            }
            else
            {
                UnhookWinEvent(winEventHook);
            }
        }

        private bool IsRunningFullScreen()
        {
            IntPtr desktopHandle = GetDesktopWindow();
            IntPtr shellHandle = GetShellWindow();
            IntPtr hWnd = GetForegroundWindow();

            if (hWnd != null && !hWnd.Equals(IntPtr.Zero))
            {
                if (!(hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle)))
                {
                    GetWindowRect(hWnd, out RECT appBounds);
                    System.Drawing.Rectangle screenBounds = System.Windows.Forms.Screen.FromHandle(hWnd).Bounds;

                    if ((appBounds.Bottom - appBounds.Top) == screenBounds.Height &&
                        (appBounds.Right - appBounds.Left) == screenBounds.Width)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime)
        {
            runningFullScreen = IsRunningFullScreen();

            if (!runningFullScreen && timerEventPending)
            {
                timerEventPending = false;
                timerEventHandler.Invoke();
            }
        }
    }
}
