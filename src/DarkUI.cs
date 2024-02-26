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
    internal class DarkUI
    {
        private static readonly Color bgColorDark = Color.FromArgb(32, 32, 32);
        private static readonly Color fgColorDark = Color.FromArgb(224, 224, 224);

        public static bool IsDark
        {
            get { return IsSupported && DarkNet.Instance.UserDefaultAppThemeIsDark; }
        }

        public static void ThemeForm(Form form, bool onInit = true)
        {
            if (onInit && IsSupported)
            {
                DarkNet.Instance.SetWindowThemeForms(form, Theme.Auto);
            }

            form.BackColor = IsDark ? bgColorDark : default;
            form.ForeColor = IsDark ? fgColorDark : default;

            foreach (Control childControl in GetControls(form))
            {
                childControl.BackColor = IsDark ? bgColorDark : default;
                childControl.ForeColor = IsDark ? fgColorDark : default;

                if (childControl is LinkLabel)
                {
                    ((LinkLabel)childControl).LinkColor = IsDark ? Color.LightBlue : default;
                }

                if (onInit)
                {
                    childControl.Paint += (object sender, PaintEventArgs e) =>
                    {
                        if (IsDark && !childControl.Enabled)
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
            foreach (Form form in Application.OpenForms)
            {
                form.BeginInvoke(() => ThemeForm(form, false));
            }
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
            get { return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 17763; }
        }
    }
}
