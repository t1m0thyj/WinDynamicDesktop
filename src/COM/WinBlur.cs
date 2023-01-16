// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinDynamicDesktop.COM
{
    // https://github.com/Pronner/WinBlur/blob/70384557eab3fb00a83360de24a18a620f086cfa/src/WinBlur/PInvoke.cs
    public class WinBlur
    {
        public class ParameterTypes
        {
            [Flags]
            public enum DWMWINDOWATTRIBUTE
            {
                DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
                DWMWA_SYSTEMBACKDROP_TYPE = 38
            }

            public struct MARGINS
            {
                public int cxLeftWidth;

                public int cxRightWidth;

                public int cyTopHeight;

                public int cyBottomHeight;
            }
        }

        public class Methods
        {
            [DllImport("DwmApi.dll")]
            public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref ParameterTypes.MARGINS pMarInset);

            [DllImport("dwmapi.dll")]
            public static extern int DwmSetWindowAttribute(IntPtr hwnd, ParameterTypes.DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);

            public static int ExtendFrame(IntPtr hwnd, ParameterTypes.MARGINS margins)
            {
                return DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }

            public static int SetWindowAttribute(IntPtr hwnd, ParameterTypes.DWMWINDOWATTRIBUTE attribute, int parameter)
            {
                return DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());
            }
        }

        public static void SetBlurStyle(Control cntrl, BlurType blurType, Mode designMode)
        {
            ParameterTypes.MARGINS bounds = default(ParameterTypes.MARGINS);
            IntPtr hwnd = cntrl.Handle;
            bounds.cxLeftWidth = 0;
            bounds.cxRightWidth = 0;
            checked
            {
                bounds.cyTopHeight = cntrl.Height + 10000000;
                bounds.cyBottomHeight = 0;
                int result = Methods.DwmExtendFrameIntoClientArea(hwnd, ref bounds);
                cntrl.BackColor = blurType != BlurType.None ? System.Drawing.Color.Black : Control.DefaultBackColor;
            }

            if (blurType == BlurType.None)
            {
                Methods.SetWindowAttribute(cntrl.Handle, ParameterTypes.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 1);
            }
            if (blurType == BlurType.Mica)
            {
                Methods.SetWindowAttribute(cntrl.Handle, ParameterTypes.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 2);
            }
            if (blurType == BlurType.Acrylic)
            {
                Methods.SetWindowAttribute(cntrl.Handle, ParameterTypes.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 3);
            }
            if (blurType == BlurType.Tabbed)
            {
                Methods.SetWindowAttribute(cntrl.Handle, ParameterTypes.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 4);
            }
            if (designMode == Mode.LightMode)
            {
                Methods.SetWindowAttribute(cntrl.Handle, ParameterTypes.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, 0);
            }
            if (designMode == Mode.DarkMode)
            {
                Methods.SetWindowAttribute(cntrl.Handle, ParameterTypes.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, 1);
            }
        }

        public enum BlurType
        {
            None,
            Acrylic,
            Mica,
            Tabbed
        }

        public enum Mode
        {
            LightMode,
            DarkMode
        }
    }
}
