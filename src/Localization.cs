// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using NGettext;

namespace WinDynamicDesktop
{
    class Localization
    {
        public static string[] languageNames = new string[] {
            "Deutsch",  // German
            "English",
            "Français",  // French
            "Eλληνικά",  // Greek
            "Pусский",  // Russian
            "中文 (简体)"  // Simplified Chinese
        };

        public static string[] localeNames = new string[]
        {
            "de_DE",
            "en_US",
            "fr_FR",
            "el_GR",
            "ru_RU",
            "zh_CN"
        };

        public static string currentLocale;
        private static ICatalog catalog = null;

        public static void Initialize()
        {
            currentLocale = JsonConfig.settings.language;

            if (currentLocale == null)
            {
                string systemLocale = CultureInfo.CurrentUICulture.Name.Replace('-', '_');
                currentLocale = localeNames.Contains(systemLocale) ? systemLocale : "en_US";
            }

            LoadLocale();
            //GetUserLanguage();
        }

        private static void LoadLocale()
        {
            string moFile = currentLocale + ".mo";

            if (File.Exists(moFile))
            {
                using (Stream stream = File.OpenRead(moFile))
                {
                    catalog = new Catalog(stream,
                        new CultureInfo(currentLocale.Replace('_', '-')));
                }
            }
        }

        private static void GetUserLanguage()
        {
            if (JsonConfig.firstRun)
            {
                LanguageDialog langDialog = new LanguageDialog();
                langDialog.Show();
            }
        }

        public static string GetTranslation(string msg)
        {
            return (catalog != null) ? catalog.GetString(msg) : msg;
        }

        // Code from https://stackoverflow.com/a/664083/5504760
        private static IEnumerable<Control> GetControls(Control form)
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

        public static void TranslateForm(Form form)
        {
            if (form.Text != null)
            {
                form.Text = GetTranslation(form.Text);
            }

            foreach (Control childControl in GetControls(form))
            {
                if (childControl.GetType().GetProperty("Text") != null
                    && childControl.Text != null)
                {
                    childControl.Text = GetTranslation(childControl.Text);
                }
            }
        }
    }
}
