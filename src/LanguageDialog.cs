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
        public LanguageDialog()
        {
            InitializeComponent();

            this.Font = SystemFonts.MessageBoxFont;
        }

        private void LanguageDialog_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(Localization.languageDisplayNames);

            int langIndex = Array.IndexOf(Localization.languageNames,
                JsonConfig.settings.language);

            if (langIndex != -1)
            {
                comboBox1.SelectedIndex = langIndex;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            // TODO DO SOMETHING WITH STATE
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
