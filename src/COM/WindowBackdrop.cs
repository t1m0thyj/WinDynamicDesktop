// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;

namespace WinDynamicDesktop.COM
{
    // https://github.com/AutoDarkMode/Windows-Auto-Night-Mode/blob/5b63c69f5d8ebf72b217e19a3ee2f82945102384/AutoDarkModeApp/PInvoke.cs
    public class WindowBackdrop
    {
        public class ParameterTypes
        {
            /*
            [Flags]
            enum DWM_SYSTEMBACKDROP_TYPE
            {
                DWMSBT_MAINWINDOW = 2, // Mica
                DWMSBT_TRANSIENTWINDOW = 3, // Acrylic
                DWMSBT_TABBEDWINDOW = 4 // Tabbed
            }
            */

            [Flags]
            public enum DWMWINDOWATTRIBUTE
            {
                DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
                DWMWA_SYSTEMBACKDROP_TYPE = 38
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MARGINS
            {
                public int cxLeftWidth;      // width of left border that retains its size
                public int cxRightWidth;     // width of right border that retains its size
                public int cyTopHeight;      // height of top border that retains its size
                public int cyBottomHeight;   // height of bottom border that retains its size
            };
        }

        public static class Methods
        {
            [DllImport("DwmApi.dll")]
            private static extern int DwmExtendFrameIntoClientArea(
                IntPtr hwnd,
                ref ParameterTypes.MARGINS pMarInset);

            [DllImport("dwmapi.dll")]
            private static extern int DwmSetWindowAttribute(IntPtr hwnd, ParameterTypes.DWMWINDOWATTRIBUTE dwAttribute,
                ref int pvAttribute, int cbAttribute);

            public static int ExtendFrame(IntPtr hwnd, ParameterTypes.MARGINS margins)
                => DwmExtendFrameIntoClientArea(hwnd, ref margins);

            public static int SetWindowAttribute(IntPtr hwnd, ParameterTypes.DWMWINDOWATTRIBUTE attribute, int parameter)
                => DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());
        }
    }
}
