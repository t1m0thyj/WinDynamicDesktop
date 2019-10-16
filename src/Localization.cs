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
using System.Net;
using System.Windows.Forms;
using Karambolo.PO;
using NGettext;

namespace WinDynamicDesktop
{
    class Localization
    {
        public static string[] languageNames = new string[] {
            "Čeština",  // Czech
            "Deutsch",  // German
            "English",  // English (US)
            "Español",  // Spanish
            "Français",  // French
            "Eλληνικά",  // Greek
            "Italiano",  // Italian
            "Македонски",  // Macedonian
            "Polski",  // Polish
            "Română",  // Romanian
            "Pусский",  // Russian
            "Türkçe",  // Turkish
            "中文 (简体)"  // Simplified Chinese
        };

        public static string[] localeNames = new string[] { "cs_CZ", "de_DE", "en_US", "es_ES",
            "fr_FR", "el_GR", "it_IT", "mk_MK", "pl_PL", "ro_RO", "ru_RU", "tr_TR", "zh_CN" };

        public static string currentLocale;
        private static ICatalog moCatalog = null;
        private static POCatalog poCatalog = null;

        public static void Initialize()
        {
            currentLocale = JsonConfig.settings.language;

            if (currentLocale == null)
            {
                string systemLocale = CultureInfo.CurrentUICulture.Name.Replace('-', '_');
                currentLocale = localeNames.Contains(systemLocale) ? systemLocale : "en_US";
                JsonConfig.settings.language = currentLocale;
            }
            else if (!IsLocaleFromWeb())
            {
                LoadLocaleFromFile();
            }
            else
            {
                LoadLocaleFromWeb();
            }


            if (JsonConfig.firstRun)
            {
                SelectLanguage();
            }
        }

        private static bool IsLocaleFromWeb()
        {
            return currentLocale.StartsWith("http://") || currentLocale.StartsWith("https://");
        }

        public static void LoadLocaleFromFile()
        {
            string poFile = currentLocale + ".po";
            string moFile = currentLocale + ".mo";

            if (File.Exists(poFile))
            {
                using (Stream stream = File.OpenRead(poFile))
                {
                    POParser parser = new POParser();
                    poCatalog = parser.Parse(stream).Catalog;
                }
            }
            else if (File.Exists(moFile))
            {
                using (Stream stream = File.OpenRead(moFile))
                {
                    moCatalog = new Catalog(stream,
                        new CultureInfo(currentLocale.Replace('_', '-')));
                }
            }
            else
            {
                byte[] embeddedMo = (byte[])Properties.Resources.ResourceManager.GetObject(
                    "locale_" + currentLocale);

                if (embeddedMo == null)
                {
                    return;
                }

                using (Stream stream = new MemoryStream(embeddedMo))
                {
                    moCatalog = new Catalog(stream,
                        new CultureInfo(currentLocale.Replace('_', '-')));
                }
            }
        }

        private static void LoadLocaleFromWeb()
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                string poText = wc.DownloadString(currentLocale);
                POParser parser = new POParser();
                poCatalog = parser.Parse(poText).Catalog;
            }
        }

        public static void SelectLanguage()
        {
            LanguageDialog langDialog = new LanguageDialog();
            langDialog.ShowDialog();
        }

        public static string GetTranslation(string msg)
        {
            if (moCatalog != null)
            {
                return moCatalog.GetString(msg);
            }
            else if (poCatalog != null)
            {
                string translated = poCatalog.GetTranslation(new POKey(msg));
                return string.IsNullOrEmpty(translated) ? msg : translated;
            }

            return msg;
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
