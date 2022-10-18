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
        private static Color backColor = Color.FromArgb(32, 32, 32);
        private static Color backColor2 = Color.FromArgb(64, 64, 64);
        private static Color foreColor = Color.FromArgb(232, 232, 232);

        public static bool IsDark
        {
            get { return IsSupported && DarkNet.Instance.UserDefaultAppThemeIsDark; }
        }

        public static void ThemeContextMenu(ContextMenuStrip menu)
        {
            menu.Renderer = IsDark ? new ToolStripProfessionalRenderer(new DarkColorTable()) :
                new ToolStripProfessionalRenderer();

            foreach (ToolStripItem menuItem in menu.Items)
            {
                menuItem.BackColor = IsDark ? backColor : Control.DefaultBackColor;
                menuItem.ForeColor = IsDark ? foreColor : Control.DefaultForeColor;

                if (menuItem is ToolStripMenuItem)
                {
                    foreach (ToolStripItem childMenuItem in ((ToolStripMenuItem)menuItem).DropDownItems)
                    {
                        childMenuItem.BackColor = IsDark ? backColor : Control.DefaultBackColor;
                        childMenuItem.ForeColor = IsDark ? foreColor : Control.DefaultForeColor;
                    }
                }
            }
        }

        public static void ThemeForm(Form form, bool onInit = true)
        {
            if (onInit && IsSupported)
            {
                DarkNet.Instance.SetWindowThemeForms(form, Theme.Auto);
            }

            form.BackColor = IsDark ? backColor : Control.DefaultBackColor;
            form.ForeColor = IsDark ? foreColor : Control.DefaultForeColor;

            foreach (Control childControl in GetControls(form))
            {
                childControl.BackColor = IsDark ? backColor : Control.DefaultBackColor;
                childControl.ForeColor = IsDark ? foreColor : Control.DefaultForeColor;

                if (childControl is Button)
                {
                    ((Button)childControl).FlatStyle = IsDark ? FlatStyle.Flat : FlatStyle.System;
                }

                if (childControl is ComboBox)
                {
                    ((ComboBox)childControl).FlatStyle = IsDark ? FlatStyle.Flat : FlatStyle.System;
                }

                if (childControl is LinkLabel)
                {
                    ((LinkLabel)childControl).LinkColor = IsDark ? Color.Orange : Color.Blue;
                }

                if (onInit)
                {
                    childControl.Paint += (object sender, PaintEventArgs e) =>
                    {
                        if (IsDark && !childControl.Enabled)
                        {
                            TextRenderer.DrawText(e.Graphics, childControl.Text, childControl.Font,
                                childControl.ClientRectangle, Color.Gray, backColor);
                        }
                    };
                }
            }
        }

        public static void UserDefaultAppThemeIsDarkChanged(object sender, bool isDark)
        {
            AppContext.notifyIcon.ContextMenuStrip.BeginInvoke(() =>
            {
                ThemeContextMenu(AppContext.notifyIcon.ContextMenuStrip);

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
            get { return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 17763; }
        }

        // https://github.com/AutoDarkMode/Windows-Auto-Night-Mode/blob/bef463c6ee937558cadfc3116bc23e83291b5ac0/AutoDarkModeSvc/Service.cs#L524
        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color MenuItemBorder
            {
                get { return DarkUI.backColor; }
            }

            public override Color MenuItemSelected
            {
                get { return DarkUI.backColor; }
            }

            public override Color MenuItemSelectedGradientBegin
            {
                get { return DarkUI.backColor2; }
            }

            public override Color MenuItemSelectedGradientEnd
            {
                get { return DarkUI.backColor2; }
            }

            public override Color ToolStripDropDownBackground
            {
                get { return DarkUI.backColor; }
            }

            public override Color ImageMarginGradientBegin
            {
                get { return DarkUI.backColor; }
            }

            public override Color ImageMarginGradientMiddle
            {
                get { return DarkUI.backColor; }
            }

            public override Color ImageMarginGradientEnd
            {
                get { return DarkUI.backColor; }
            }
        }
    }
}
