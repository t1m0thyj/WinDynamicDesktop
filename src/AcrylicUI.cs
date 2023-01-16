// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Dark.Net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    internal class AcrylicUI
    {
        public static bool IsDark
        {
            get { return IsSupported && DarkNet.Instance.UserDefaultAppThemeIsDark; }
        }

        public static void ThemeForm(Form form, bool onInit = true)
        {
            if (!IsSupported)
            {
                return;
            }

            COM.WinBlur.BlurType blurType = IsDark ? COM.WinBlur.BlurType.Mica : COM.WinBlur.BlurType.None;
            COM.WinBlur.SetBlurStyle(form, blurType, COM.WinBlur.Mode.DarkMode);
            form.ForeColor = IsDark ? Color.White : Control.DefaultForeColor;

            foreach (Control childControl in GetControls(form))
            {
                COM.WinBlur.SetBlurStyle(childControl, blurType, COM.WinBlur.Mode.DarkMode);
                childControl.ForeColor = IsDark ? Color.White : Control.DefaultForeColor;

                if (childControl is LinkLabel)
                {
                    ((LinkLabel)childControl).LinkColor = IsDark ? Color.LightBlue : Color.Blue;
                }

                if (onInit)
                {
                    childControl.Paint += (object sender, PaintEventArgs e) =>
                    {
                        if (!childControl.Enabled)
                        {
                            TextRenderer.DrawText(e.Graphics, childControl.Text, childControl.Font,
                                childControl.ClientRectangle, Color.Gray);
                        }
                    };
                }
            }
        }

        public static void UserDefaultAppThemeIsDarkChanged(object sender, bool isDark)
        {
            AppContext.notifyIcon.ContextMenuStrip.BeginInvoke(() =>
            {
                foreach (Form form in Application.OpenForms)
                {
                    ThemeForm(form, false);
                }
            });
        }

        // Code from https://stackoverflow.com/a/664083/5504760
        internal static IEnumerable<Control> GetControls(Control form)
        {
            foreach (Control childControl in form.Controls)
            {
                foreach (Control grandChild in GetControls(childControl))
                {
                    yield return grandChild;
                }

                yield return childControl;
            }
        }

        private static bool IsSupported
        {
            get { return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000; }
        }
    }
}
