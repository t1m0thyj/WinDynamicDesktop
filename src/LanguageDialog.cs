// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public partial class LanguageDialog : Form
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;

        public LanguageDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);

            this.Font = SystemFonts.MessageBoxFont;
        }

        private void LanguageDialog_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(Localization.languageNames);

            int langIndex = Array.IndexOf(Localization.localeNames, Localization.currentLocale);

            if (langIndex != -1)
            {
                comboBox1.SelectedIndex = langIndex;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            string oldLocaleName = Localization.currentLocale;
            string localeName = Localization.localeNames[comboBox1.SelectedIndex];

            if (localeName != oldLocaleName)
            {
                Localization.currentLocale = localeName;

                if (AppContext.notifyIcon == null)  // Has UI been loaded yet?
                {
                    Localization.LoadLocaleFromFile();
                }

                JsonConfig.settings.language = localeName;

                if (AppContext.notifyIcon != null)
                {
                    DialogResult result = MessageDialog.ShowQuestion(_("WinDynamicDesktop needs " +
                        "to restart for the language to change. Do you want to restart the app " +
                        "now?"), _("Question"));

                    if (result == DialogResult.Yes)
                    {
                        JsonConfig.EnablePendingRestart();
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
