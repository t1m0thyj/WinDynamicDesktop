// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    // https://github.com/specshell/specshell.software.winform.hiddenform/blob/main/src/Specshell.WinForm.HiddenForm/HiddenForm.cs
    internal class HiddenForm : Form
    {
        private const int ENDSESSION_CLOSEAPP = 0x1;
        private const int WM_QUERYENDSESSION = 0x11;
        private const int WM_ENDSESSION = 0x16;
        private const int WM_POWERBROADCAST = 0x0218;
        private const int PBT_APMRESUMESUSPEND = 0x0007;
        private const int PBT_APMRESUMEAUTOMATIC = 0x0012;

        public static Microsoft.Win32.PowerModeChangedEventHandler OnPowerModeChanged;
        
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hwndNewParent);

        public HiddenForm()
        {
            this.Load += this.HiddenForm_Load;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_POPUP = unchecked((int)0x80000000);
                const int WS_EX_TOOLWINDOW = 0x80;

                CreateParams cp = base.CreateParams;
                cp.ExStyle = WS_EX_TOOLWINDOW;
                cp.Style = WS_POPUP;
                cp.Height = 0;
                cp.Width = 0;
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            // https://github.com/rocksdanister/lively/blob/9142f6a4cfc222cd494f205a5daaa1a0238282e3/src/Lively/Lively/Views/WindowMsg/WndProcMsgWindow.xaml.cs#L41
            switch (m.Msg)
            {
                case WM_QUERYENDSESSION:
                    if ((m.LParam & ENDSESSION_CLOSEAPP) != 0)
                    {
                        UpdateChecker.RegisterApplicationRestart(null, (int)RestartFlags.RESTART_NO_CRASH |
                            (int)RestartFlags.RESTART_NO_HANG | (int)RestartFlags.RESTART_NO_REBOOT);
                    }
                    m.Result = new IntPtr(1);
                    break;
                case WM_ENDSESSION:
                    if ((m.LParam & ENDSESSION_CLOSEAPP) != 0)
                    {
                        Application.Exit();
                    }
                    m.Result = IntPtr.Zero;
                    break;
                case WM_POWERBROADCAST:
                    int pbt = m.WParam.ToInt32();
                    if (pbt == PBT_APMRESUMEAUTOMATIC || pbt == PBT_APMRESUMESUSPEND)
                    {
                        OnPowerModeChanged(null, null);
                    }
                    break;
            }

            base.WndProc(ref m);
        }

        private void HiddenForm_Load(object source, EventArgs e)
        {
            const int HWND_MESSAGE = -1;
            SetParent(this.Handle, new IntPtr(HWND_MESSAGE));
        }
    }
}
