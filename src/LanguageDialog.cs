// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public partial class LanguageDialog : Form
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private const string translateLink = "https://poeditor.com/join/project/DEgfVpyuiK";
        private List<string> languageNames = new List<string>();

        public LanguageDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);
            this.linkLabel1.LinkColor = System.Drawing.SystemColors.HotTrack;
        }

        private void LanguageDialog_Load(object sender, EventArgs e)
        {
            foreach (string langCode in Localization.languageCodes)
            {
                try
                {
                    languageNames.Add(new CultureInfo(langCode).NativeName);
                }
                catch (CultureNotFoundException)
                {
                    languageNames.Add(null);
                }
            }
            comboBox1.Items.AddRange(languageNames.Where(x => x != null).ToArray());
            comboBox1.Sorted = true;

            int langIndex = Array.IndexOf(Localization.languageCodes, Localization.currentLocale);
            if (langIndex != -1)
            {
                comboBox1.SelectedItem = languageNames[langIndex];
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(translateLink) { UseShellExecute = true });
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            int langIndex = languageNames.IndexOf((string)comboBox1.SelectedItem);
            string languageCode = Localization.languageCodes[langIndex];

            if (languageCode != Localization.currentLocale)
            {
                Localization.currentLocale = languageCode;

                if (AppContext.notifyIcon == null)  // Has UI been loaded yet?
                {
                    Localization.LoadLocaleFromFile();
                }

                JsonConfig.settings.language = languageCode;

                if (AppContext.notifyIcon != null)
                {
                    DialogResult result = MessageDialog.ShowQuestion(_("WinDynamicDesktop needs to restart for the " +
                        "language to change. Do you want to restart the app now?"), _("Question"));

                    if (result == DialogResult.Yes)
                    {
                        JsonConfig.ReloadConfig();
                    }
                }
            }

            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
