// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WinDynamicDesktop.COM;
using static WinDynamicDesktop.COM.WindowBackdrop.ParameterTypes;

namespace WinDynamicDesktop
{
    internal class AcrylicUI
    {
        private const string registryThemeLocation = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private static bool? appsUseLightTheme;

        public static bool IsDark
        {
            get { return IsSupported && !(appsUseLightTheme ?? GetWindowsThemeSetting()); }
        }

        public static void ThemeForm(Form form, bool onInit = true)
        {
            if (!IsSupported)
            {
                return;
            }
            else if (onInit && !appsUseLightTheme.HasValue)
            {
                SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            }

            EnableMica(form);
            form.BackColor = IsDark ? Color.Black : default;
            form.BackColor = Color.FromArgb(form.BackColor.R, form.BackColor.G,
                (form.BackColor.B < 255) ? (form.BackColor.B + 1) : (form.BackColor.B - 1));
            form.ForeColor = IsDark ? Color.White : default;
            form.TransparencyKey = form.BackColor;

            foreach (Control childControl in GetControls(form))
            {
                childControl.BackColor = IsDark ? form.BackColor : default;
                childControl.ForeColor = IsDark ? form.ForeColor : default;

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
            get { return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22523; }
        }

        private static bool GetWindowsThemeSetting()
        {
            using (RegistryKey themeKey = Registry.CurrentUser.OpenSubKey(registryThemeLocation))
            {
                appsUseLightTheme = (int)themeKey.GetValue("AppsUseLightTheme", 1) != 0;
            };

            return appsUseLightTheme.Value;
        }

        private static void EnableMica(Form form)
        {
            var margins = new MARGINS()
            {
                cxLeftWidth = -1,
                cxRightWidth = -1,
                cyTopHeight = -1,
                cyBottomHeight = -1
            };
            WindowBackdrop.Methods.ExtendFrame(form.Handle, margins);
            WindowBackdrop.Methods.SetWindowAttribute(form.Handle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                IsDark ? 1 : 0);
            WindowBackdrop.Methods.SetWindowAttribute(form.Handle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, 2);
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category != UserPreferenceCategory.General || appsUseLightTheme.Value == GetWindowsThemeSetting())
            {
                return;
            }

            AppContext.notifyIcon.ContextMenuStrip.BeginInvoke(() =>
            {
                foreach (Form form in Application.OpenForms)
                {
                    ThemeForm(form, false);
                }
            });
        }
    }
}
