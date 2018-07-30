using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public partial class AboutDialog : Form
    {
        private static string websiteLink = "https://github.com/t1m0thyj/WinDynamicDesktop";
        private static string donateLink =
            "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=H8ZZXM9ABRJFU";

        public AboutDialog()
        {
            InitializeComponent();
        }

        private void AboutDialog_Load(object sender, EventArgs e)
        {
            iconBox.Image = (new Icon(Properties.Resources.AppIcon, 64, 64)).ToBitmap();

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            nameLabel.Text += " " + version;

            if (!UwpDesktop.IsRunningAsUwp())
            {
                donateButton.Visible = true;
            }

            creditsBox.Text = GetCreditsText();
        }

        private string GetCreditsText()
        {
            List<string> lines = new List<string>() {
                "Thanks to:",
                "",
                "Apple for the Mojave wallpapers",
                "Contributors on GitHub",
                "LocationIQ for their free geocoding API",
                "Roundicons from flaticon.com for the icon (licensed by CC 3.0 BY)"
            };

            return String.Join("\n    ", lines);
        }

        private void websiteLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(websiteLink);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void creditsButton_CheckedChanged(object sender, EventArgs e)
        {
            creditsBox.Visible ^= true;
        }

        private void donateButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(donateLink);
        }
    }
}
