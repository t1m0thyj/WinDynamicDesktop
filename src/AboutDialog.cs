// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public partial class AboutDialog : Form
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private readonly string websiteLink = "https://github.com/t1m0thyj/WinDynamicDesktop";
        private readonly string donateLink = "https://paypal.me/t1m0thyj";
        private readonly string rateLink = "ms-windows-store://review/?ProductId=9NM8N7DQ3Z5F";

        public AboutDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);
        }

        private void AboutDialog_Load(object sender, EventArgs e)
        {
            iconBox.Image = (new Icon(Properties.Resources.AppIcon, 64, 64)).ToBitmap();
            richTextBox1.Rtf = GetRtfUnicodeEscapedString(GetRtfText());
        }

        private string GetRtfLink(string href, string linkText = null)
        {
            return @"{\field{\*\fldinst HYPERLINK " + '"' + href + '"' + @"}{\fldrslt " + (linkText ?? href) + "}}";
        }

        private string GetRtfText()
        {
            string versionStr = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            versionStr = versionStr.Remove(versionStr.Length - 2);
            versionStr += UwpDesktop.IsRunningAsUwp() ? " (UWP)" : string.Empty;

            string copyrightLine = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(
                Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute), false)).Copyright;

            string donateRateLine = string.Format(_("If you like the app, please consider {0} or {1} :)"),
                GetRtfLink(donateLink, _("donating")), GetRtfLink(rateLink, _("rating it")));

            return @"{\rtf1\pc\fs5\par" +
                @"\sb72\qc\fs30\b WinDynamicDesktop " + versionStr + @"\b0\par" +
                @"\fs20 " + _("Port of macOS Mojave Dynamic Desktop feature to Windows 10") + @"\par " +
                copyrightLine + @"\par " +
                GetRtfLink(websiteLink) + @"\par " +
                donateRateLine + @"\par" +
                @"\par" +
                @"\b " + _("Thanks to:") + @"\b0\par " +
                _("Apple for the Mojave wallpapers") + @"\par " +
                _("Contributors and translators on GitHub") + @"\par " +
                _("LocationIQ for their free geocoding API") + @"\par " +
                _("Roundicons from flaticon.com for the icon (licensed by CC 3.0 BY)") + @"\par" +
                @"\par }";
        }

        // Code from https://stackoverflow.com/questions/1368020/how-to-output-unicode-string-to-rtf-using-c
        private string GetRtfUnicodeEscapedString(string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (c <= 0x7f)
                    sb.Append(c);
                else
                    sb.Append("\\u" + Convert.ToUInt32(c) + "?");
            }
            return sb.ToString();
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.LinkText) { UseShellExecute = true });
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
